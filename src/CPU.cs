using System;

namespace Teto;

public enum Opcode
{
    // --- Data Movement ---
    NOP  = 0x00,  // No operation
    MOV  = 0x01,  // Move data
    LD   = 0x02,  // Load data from memory
    ST   = 0x03,  // Store data to memory
    PUSH = 0x04,  // Push to stack
    POP  = 0x05,  // Pop from stack
    XCHG = 0x06,  // Exchange values

    // --- Arithmetic (Integer) ---
    ADD  = 0x07,  // Integer addition
    SUB  = 0x08,  // Integer subtraction
    MUL  = 0x09,  // Integer multiplication
    DIV  = 0x0A,  // Integer division
    MOD  = 0x0B,  // Integer modulo
    INC  = 0x0C,  // Increment register
    DEC  = 0x0D,  // Decrement register
    NEG  = 0x0E,  // Negate (two's complement)

    // --- Arithmetic (Floating-Point) ---
    ADDF = 0x0F,  // Floating-point addition
    SUBF = 0x10,  // Floating-point subtraction
    MULF = 0x11,  // Floating-point multiplication
    DIVF = 0x12,  // Floating-point division

    // --- Bitwise Operations ---
    AND  = 0x13,  // Bitwise AND
    OR   = 0x14,  // Bitwise OR
    XOR  = 0x15,  // Bitwise XOR
    NOT  = 0x16,  // Bitwise NOT
    SHL  = 0x17,  // Shift left
    SHR  = 0x18,  // Shift right
    ROL  = 0x19,  // Rotate left
    ROR  = 0x1A,  // Rotate right
    TEST = 0x1B,  // Bitwise test

    // --- Control Flow ---
    CMP    = 0x1C,  // Compare two values
    JEQ    = 0x1D,  // Jump if equal
    JNE    = 0x1E,  // Jump if not equal
    JGT    = 0x1F,  // Jump if greater
    JLT    = 0x20,  // Jump if less
    JGE    = 0x21,  // Jump if greater or equal
    JLE    = 0x22,  // Jump if less or equal
    JMP    = 0x23,  // Unconditional jump
    JMPREL = 0x24,  // Relative jump
    JMPX   = 0x25,  // Jump with absolute address
    CALL   = 0x26,  // Call subroutine
    RET    = 0x27,  // Return from subroutine

    // --- Stack Operations ---
    ENTER = 0x28,  // Set up stack frame
    LEAVE = 0x29,  // Tear down stack frame

    // --- System & Interrupts ---
    INT     = 0x2A,  // Software interrupt
    IRET    = 0x2B,  // Return from interrupt
    HLT     = 0x2C,  // Halt execution
    SYSCALL = 0x2D,  // System call
    CLI     = 0x2E,  // Disable interrupts
    STI     = 0x2F   // Enable interrupts
}

public class CPU
{
    private readonly uint[] _registers = new uint[8];
    private uint _pc = 0x200;
    private uint _flags = 0;
    
    public uint GetRegister(uint index) => _registers[index];
    
    private void Fetch()
    {
        var instr = Memory.Read(_pc) |
                    (uint)(Memory.Read(_pc + 1) << 8) |
                    (uint)(Memory.Read(_pc + 2) << 16) |
                    (uint)(Memory.Read(_pc + 3) << 24);
        _pc += 4;
        
        Console.WriteLine($"0x{_pc - 4:X4}: 0b{Convert.ToString(instr, 2).PadLeft(32, '0')}");
        
        var opcode = instr & 0x3F;                  // 6 bits opcode
        var reg = (instr >> 6) & 0x7;               // 3 bits register
        var mode = (instr >> 9) & 0x1;              // 1 bit mode (0 = immediate, 1 = register)
        var operand = (instr >> 10) & 0x3FFFFF;     // 22 bits operand
        
        Console.WriteLine($"Opcode: 0x{opcode:X2}, Reg: {reg}, Mode: {mode}, Operand: {operand}");
        
        Execute((Opcode)opcode, reg, mode, operand);
    }
    
    private void Execute(Opcode opcode, uint reg, uint mode, uint value)
    {
        switch (opcode)
        {
            case Opcode.NOP:
                break;
            
            case Opcode.MOV:
                _registers[reg] = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.ADD:
                _registers[reg] += mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.SUB:
                _registers[reg] -= mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.MUL:
                _registers[reg] *= mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.DIV:
                _registers[reg] /= mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.AND:
                _registers[reg] &= mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.OR:
                _registers[reg] |= mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.XOR:
                _registers[reg] ^= mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.NOT:
                _registers[reg] = ~_registers[reg];
                break;
            
            case Opcode.SHL:
                _registers[reg] <<= (int)(mode == 0 ? value : _registers[value]);
                break;
            
            case Opcode.SHR:
                _registers[reg] >>= (int)(mode == 0 ? value : _registers[value]);
                break;
            
            case Opcode.CMP:
                _flags = _registers[reg] == (mode == 0 ? value : _registers[value]) ? 1u : 0u;
                break;
            
            case Opcode.JEQ:
                if (_flags == 1)
                    _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JNE:
                if (_flags == 0)
                    _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JGT:
                if (_flags == 1 && _registers[reg] > (mode == 0 ? value : _registers[value]))
                    _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JLT:
                if (_flags == 1 && _registers[reg] < (mode == 0 ? value : _registers[value]))
                    _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JGE:
                if (_flags == 1 && _registers[reg] >= (mode == 0 ? value : _registers[value]))
                    _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JLE:
                if (_flags == 1 && _registers[reg] <= (mode == 0 ? value : _registers[value]))
                    _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JMP:
                _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.LD:
                _registers[reg] = Memory.Read(mode == 0 ? value : _registers[value]);
                break;
            
            case Opcode.ST:
                Memory.Write(mode == 0 ? value : _registers[value], (byte)_registers[reg]);
                break;
            
            case Opcode.HLT:
                _pc = Memory.Size;
                break;
            
            default:
                throw new InvalidOperationException($"Unknown opcode: {opcode}");
        }
    }
    
    public void Run(uint maxCycles = 1000)
    {
        uint cycles = 0;
        while (_pc < Memory.Size && cycles++ < maxCycles)
        {
            Fetch();
        }
    }
}