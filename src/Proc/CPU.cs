using System;
using System.Diagnostics;
using Teto.Debugging;
using Teto.MMU;

namespace Teto.Proc;

public class CPU
{
    public const int EAX = 0; 
    public const int EBX = 1;
    public const int ECX = 2;
    public const int EDX = 3;
    public const int ESI = 4;
    public const int EDI = 5;
    public const int EBP = 6;
    public const int ESP = 7;
    
    private readonly int[] _registers = new int[8];
    private readonly RAM _ram;
    
    public int PC { get; private set; } = Segments.TextStart;
    public uint Flags { get; private set; }
    public bool Halted { get; private set; }
    public string LastInstruction { get; private set; } = string.Empty;
    
    public CPU(RAM ram)
    {
        _ram = ram;
        _registers[ESP] = Segments.StackEnd;
        _registers[EBP] = Segments.StackEnd;
    }
    
    public int GetRegister(int index) => _registers[index];
    public float GetRegisterF(int index) => BitConverter.Int32BitsToSingle(_registers[index]);
    
    public void SetRegister(int index, int value) => _registers[index] = value;
    public void SetRegisterF(int index, float value) => _registers[index] = BitConverter.SingleToInt32Bits(value);
    
    public void Step() 
    {
        if (!Halted)
            Fetch();
    }
    
    public void Run(uint maxCycles = 1000, int delay = 0)
    {
        uint cycles = 0;
        while (!Halted && PC < _ram.Size && cycles++ < maxCycles)
        {
            Fetch();
            if (delay > 0) System.Threading.Thread.Sleep(delay);
        }
    }
    
    public void Reset()
    {
        PC = Segments.TextStart;
        Flags = 0;
        Halted = false;
        Array.Clear(_registers, 0, _registers.Length);
        _registers[ESP] = Segments.StackEnd;
        _registers[EBP] = Segments.StackEnd;
        LastInstruction = string.Empty;
    }
    
    // --- STACK OPERATIONS ---
    
    private void StackPush(int value)
    {
        if (_registers[ESP] - 4 < Segments.StackStart)
        {
            throw new StackOverflowException("Stack overflow");
        }
        
        _registers[ESP] -= 4;
        _ram.WriteWord((uint)_registers[ESP], value);
    }
    
    private int StackPop()
    {
        if (_registers[ESP] + 4 > Segments.StackEnd)
        {
            throw new StackOverflowException("Stack underflow");
        }
        
        var value = _ram.ReadWord((uint)_registers[ESP]);
        
        if (_registers[ESP] + 4 <= Segments.StackEnd) {
            _registers[ESP] += 4;
        } else {
            throw new InvalidOperationException("Stack pointer would exceed memory bounds");
        }
        
        return value;
    }
    
    
    // --- FETCH-DECODE-EXECUTE CYCLE ---
    
    
    private void Fetch()
    {
        var instr = _ram.Read((uint)PC) |
                    (_ram.Read((uint)PC + 1) << 8) |
                    (_ram.Read((uint)PC + 2) << 16) |
                    (_ram.Read((uint)PC + 3) << 24);
        PC += 4;
        
        var opcode = instr & 0xFF;                     // 8 bits opcode
        var reg = (instr >> 8) & 0xF;                  // 4 bits register
        var mode = (instr >> 12) & 0xF;                // 4 bits mode
        var operand = (instr >> 16) & 0xFFFF;          // 16 bits operand

        if ((operand & 0x8000) != 0) 
            unchecked { operand |= (int)0xFFFF0000; }
        
        Execute((Opcode)opcode, reg, mode, operand);
        LastInstruction = Disassembler.DisassembleIntInstruction(instr);
    }
    
    private void Execute(Opcode opcode, int reg, int mode, int value)
    {
        switch (opcode)
        {
            case Opcode.NOP:
                break;
    
            case Opcode.MOV:
                _registers[reg] = ResolveMode(mode, value);
                break;
    
            case Opcode.MOVHI:
                _registers[reg] = (_registers[reg] & 0xFFFF) | ((ResolveMode(mode, value) & 0xFFFF) << 16);
                break;
    
            case Opcode.MOVLO:
                _registers[reg] = (int)(_registers[reg] & 0xFFFF0000) | (ResolveMode(mode, value) & 0xFFFF);
                break;
    
            case Opcode.LD:
                Debug.WriteLine($"LD: reg {reg}, mode {(InstrMode)mode} value {value} resolved to 0x{ResolveMode(mode, value):X8}");
                _registers[reg] = _ram.ReadWord((uint)ResolveMode(mode, value));
                break;
    
            case Opcode.ST:
                _ram.WriteWord((uint)ResolveMode(mode, value), _registers[reg]);
                break;
    
            case Opcode.LDHI:
                var addrHi = (uint)ResolveMode(mode, value);
                _registers[reg] = (_registers[reg] & 0xFFFF) | ((_ram.Read(addrHi) | (_ram.Read(addrHi + 1) << 8)) << 16);
                break;
    
            case Opcode.LDLO:
                var addrLo = (uint)ResolveMode(mode, value);
                _registers[reg] = (int)(_registers[reg] & 0xFFFF0000) | _ram.Read(addrLo) | (_ram.Read(addrLo + 1) << 8);
                break;
    
            case Opcode.STHI:
                var addrHiSt = (uint)ResolveMode(mode, value);
                _ram.Write(addrHiSt, (byte)((_registers[reg] >> 16) & 0xFF));
                _ram.Write(addrHiSt + 1, (byte)((_registers[reg] >> 24) & 0xFF));
                break;
    
            case Opcode.STLO:
                var addrLoSt = (uint)ResolveMode(mode, value);
                _ram.Write(addrLoSt, (byte)(_registers[reg] & 0xFF));
                _ram.Write(addrLoSt + 1, (byte)((_registers[reg] >> 8) & 0xFF));
                break;
    
            case Opcode.PUSH:
                StackPush(_registers[reg]);
                break;
            
            case Opcode.POP:
                _registers[reg] = StackPop();
                break;
    
            case Opcode.XCHG:
                var tmp = _registers[reg];
                _registers[reg] = StackPop();
                StackPush(tmp);
                break;
    
            case Opcode.ADD:
                _registers[reg] += ResolveMode(mode, value);
                break;
    
            case Opcode.SUB:
                _registers[reg] -= ResolveMode(mode, value);
                break;
    
            case Opcode.MUL:
                _registers[reg] *= ResolveMode(mode, value);
                break;
    
            case Opcode.DIV:
                _registers[reg] /= ResolveMode(mode, value);
                break;
    
            case Opcode.MOD:
                _registers[reg] %= ResolveMode(mode, value);
                break;
    
            case Opcode.INC:
                _registers[reg]++;
                break;
    
            case Opcode.DEC:
                _registers[reg]--;
                break;
    
            case Opcode.NEG:
                _registers[reg] = ~_registers[reg] + 1;
                break;
    
            case Opcode.FADD:
                var f1 = BitConverter.Int32BitsToSingle(_registers[reg]);
                var f2 = BitConverter.Int32BitsToSingle(ResolveMode(mode, value));
                _registers[reg] = BitConverter.SingleToInt32Bits(f1 + f2);
                break;
    
            case Opcode.FSUB:
                f1 = BitConverter.Int32BitsToSingle(_registers[reg]);
                f2 = BitConverter.Int32BitsToSingle(ResolveMode(mode, value));
                _registers[reg] = BitConverter.SingleToInt32Bits(f1 - f2);
                break;
    
            case Opcode.FMUL:
                f1 = BitConverter.Int32BitsToSingle(_registers[reg]);
                f2 = BitConverter.Int32BitsToSingle(ResolveMode(mode, value));
                _registers[reg] = BitConverter.SingleToInt32Bits(f1 * f2);
                break;
    
            case Opcode.FDIV:
                f1 = BitConverter.Int32BitsToSingle(_registers[reg]);
                f2 = BitConverter.Int32BitsToSingle(ResolveMode(mode, value));
                _registers[reg] = BitConverter.SingleToInt32Bits(f1 / f2);
                break;
    
            case Opcode.FMOVHI:
                var currentVal = _registers[reg];
                var newHigh = (ResolveMode(mode, value) & 0xFFFF) << 16;
                _registers[reg] = (currentVal & 0xFFFF) | newHigh;
                break;
    
            case Opcode.FMOVLO:
                currentVal = _registers[reg];
                var newLow = ResolveMode(mode, value) & 0xFFFF;
                _registers[reg] = (int)(currentVal & 0xFFFF0000) | newLow;
                break;
    
            case Opcode.ITOF:
                var intValue = _registers[reg];
                var floatValue = (float)intValue;
                _registers[value] = BitConverter.SingleToInt32Bits(floatValue);
                break;
    
            case Opcode.FTOI:
                var fValue = BitConverter.Int32BitsToSingle(_registers[reg]);
                _registers[value] = (int)fValue;
                break;
    
            case Opcode.AND:
                _registers[reg] &= ResolveMode(mode, value);
                break;
    
            case Opcode.OR:
                _registers[reg] |= ResolveMode(mode, value);
                break;
    
            case Opcode.XOR:
                _registers[reg] ^= ResolveMode(mode, value);
                break;
    
            case Opcode.NOT:
                _registers[reg] = ~_registers[reg];
                break;
    
            case Opcode.SHL:
                _registers[reg] <<= ResolveMode(mode, value);
                break;
    
            case Opcode.SHR:
                _registers[reg] >>= ResolveMode(mode, value);
                break;
    
            case Opcode.ROL:
                var rolValue = ResolveMode(mode, value);
                rolValue %= 32;
                _registers[reg] = (_registers[reg] << rolValue) | (_registers[reg] >> (32 - rolValue));
                break;
    
            case Opcode.ROR:
                var rorValue = ResolveMode(mode, value);
                rorValue %= 32;
                _registers[reg] = (_registers[reg] >> rorValue) | (_registers[reg] << (32 - rorValue));
                break;
    
            case Opcode.TEST:
                Flags = (_registers[reg] & ResolveMode(mode, value)) == 0 ? 1u : 0u;
                break;
    
            case Opcode.CMP:
                var resolvedValue = ResolveMode(mode, value);
                Flags = _registers[reg] == resolvedValue ? 1u :
                        _registers[reg] > resolvedValue ? 2u : 0u;
                break;
    
            case Opcode.JEQ:
                if (Flags == 1)
                    PC = ResolveMode(mode, value);
                break;
    
            case Opcode.JNE:
                if (Flags != 1)
                    PC = ResolveMode(mode, value);
                break;
    
            case Opcode.JGT:
                if (Flags == 2)
                    PC = ResolveMode(mode, value);
                break;
    
            case Opcode.JLT:
                if (Flags == 0)
                    PC = ResolveMode(mode, value);
                break;
    
            case Opcode.JGE:
                if (Flags > 0)
                    PC = ResolveMode(mode, value);
                break;
    
            case Opcode.JLE:
                if (Flags < 2)
                    PC = ResolveMode(mode, value);
                break;
    
            case Opcode.JMP:
                PC = ResolveMode(mode, value);
                break;
    
            case Opcode.JMPREL:
                PC += ResolveMode(mode, value);
                break;
    
            case Opcode.JMPX:
                PC = value;
                break;
    
            case Opcode.CALL:
                StackPush(PC);
                PC = ResolveMode(mode, value);
                break;
    
            case Opcode.RET:
                PC = StackPop();
                break;
    
            case Opcode.ENTER:
                StackPush(_registers[EBP]);
                _registers[EBP] = _registers[ESP];
                _registers[ESP] -= ResolveMode(mode, value);
                break;
    
            case Opcode.LEAVE:
                _registers[ESP] = _registers[EBP];
                _registers[EBP] = StackPop();
                break;
    
            case Opcode.HLT:
                Halted = true;
                break;
    
            default:
                throw new InvalidOperationException($"Unknown opcode: {opcode}");
        }
    }
    
    private int ResolveMode(int mode, int value) {
        return mode switch
        {
            0 or 1 => value,                                // immediate
            2 => _registers[value],                         // register value as address
            3 => Segments.HeapStart + value,                // heap offset
            >= 4 and <= 9 => _registers[mode - 4] + value,  // register offset
            10 or 11 => _registers[mode - 4] - value,       // stack offset
            _ => throw new InvalidOperationException($"Unknown addressing mode: {mode}")
        };
    }
}