using System;

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
    private int _pc = Segments.TextStart;
    private uint _flags;
    
    public CPU(RAM ram)
    {
        _ram = ram;
        _registers[ESP] = Segments.StackEnd;
    }
    
    public int GetRegister(int index) => _registers[index];
    public float GetRegisterF(int index) => BitConverter.Int32BitsToSingle(_registers[index]);
    
    public void SetRegister(int index, int value) => _registers[index] = value;
    public void SetRegisterF(int index, float value) => _registers[index] = BitConverter.SingleToInt32Bits(value);
    
    public void Run(uint maxCycles = 1000, int delay = 0, bool dumpRegisters = false, bool dumpMemory = false, uint offset = 0, uint length = 0)
    {
        uint cycles = 0;
        while (_pc < _ram.Size && cycles++ < maxCycles)
        {
            Fetch();
            if (delay > 0) System.Threading.Thread.Sleep(delay);
            if (dumpRegisters) DumpState();
            if (dumpMemory) _ram.Dump(offset, length);
            
            Console.WriteLine();
        }
    }
    
    public void Reset()
    {
        _pc = Segments.TextStart;
        _flags = 0;
        for (var i = 0; i < _registers.Length; i++)
        {
            _registers[i] = 0;
        }
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
        var instr = _ram.Read((uint)_pc) |
                    (_ram.Read((uint)_pc + 1) << 8) |
                    (_ram.Read((uint)_pc + 2) << 16) |
                    (_ram.Read((uint)_pc + 3) << 24);
        _pc += 4;
        
        var opcode = instr & 0xFF;                     // 8 bits opcode
        var reg = (instr >> 8) & 0xF;                  // 4 bits register
        var mode = (instr >> 12) & 0xF;                // 4 bits mode
        var operand = (instr >> 16) & 0xFFFF;          // 16 bits operand

        if ((operand & 0x8000) != 0) 
            unchecked { operand |= (int)0xFFFF0000; }
        
        Execute((Opcode)opcode, reg, mode, operand);
    }
    
    private void Execute(Opcode opcode, int reg, int mode, int value)
    {
        switch (opcode)
        {
            case Opcode.NOP:
                break;
            
            case Opcode.MOV:
                _registers[reg] = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.MOVHI:
                _registers[reg] = (_registers[reg] & 0xFFFF) | ((value & 0xFFFF) << 16);
                break;
    
            case Opcode.MOVLO:
                _registers[reg] = (int)(_registers[reg] & 0xFFFF0000) | (value & 0xFFFF);
                break;
            
            case Opcode.LD:
                _registers[reg] = _ram.Read(mode == 0 ? (uint)value : (uint)_registers[value]);
                break;
            
            case Opcode.ST:
                _ram.Write(mode == 0 ? (uint)value : (uint)_registers[value], (byte)_registers[reg]);
                break;
            
            case Opcode.LDHI:
                var addrHi = mode == 0 ? (uint)value : (uint)_registers[value];
                _registers[reg] = (_registers[reg] & 0xFFFF) | ((_ram.Read(addrHi) | (_ram.Read(addrHi + 1) << 8)) << 16);
                break;

            case Opcode.LDLO:
                var addrLo = mode == 0 ? (uint)value : (uint)_registers[value];
                _registers[reg] = (int)(_registers[reg] & 0xFFFF0000) | _ram.Read(addrLo) | (_ram.Read(addrLo + 1) << 8);
                break;

            case Opcode.STHI:
                var addrHiSt = mode == 0 ? (uint)value : (uint)_registers[value];
                _ram.Write(addrHiSt, (byte)((_registers[reg] >> 16) & 0xFF));
                _ram.Write(addrHiSt + 1, (byte)((_registers[reg] >> 24) & 0xFF));
                break;

            case Opcode.STLO:
                var addrLoSt = mode == 0 ? (uint)value : (uint)_registers[value];
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
            
            case Opcode.MOD:
                _registers[reg] %= mode == 0 ? value : _registers[value];
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
                var f2 = BitConverter.Int32BitsToSingle(mode == 0 ? value : _registers[value]);
                _registers[reg] = BitConverter.SingleToInt32Bits(f1 + f2);
                break;
            
            case Opcode.FSUB:
                f1 = BitConverter.Int32BitsToSingle(_registers[reg]);
                f2 = BitConverter.Int32BitsToSingle(mode == 0 ? value : _registers[value]);
                _registers[reg] = BitConverter.SingleToInt32Bits(f1 - f2);
                break;
            
            case Opcode.FMUL:
                f1 = BitConverter.Int32BitsToSingle(_registers[reg]);
                f2 = BitConverter.Int32BitsToSingle(mode == 0 ? value : _registers[value]);
                _registers[reg] = BitConverter.SingleToInt32Bits(f1 * f2);
                break;
            
            case Opcode.FDIV:
                f1 = BitConverter.Int32BitsToSingle(_registers[reg]);
                f2 = BitConverter.Int32BitsToSingle(mode == 0 ? value : _registers[value]);
                _registers[reg] = BitConverter.SingleToInt32Bits(f1 / f2);
                break;
            
            case Opcode.FMOVHI:
                var currentVal = _registers[reg];
                var newHigh = (value & 0xFFFF) << 16;
                _registers[reg] = (currentVal & 0xFFFF) | newHigh;
                break;
    
            case Opcode.FMOVLO:
                currentVal = _registers[reg];
                var newLow = value & 0xFFFF;
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
                _registers[reg] <<= mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.SHR:
                _registers[reg] >>= mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.CMP:
                _flags = _registers[reg] == (mode == 0 ? value : _registers[value]) ? 1u : 
                         _registers[reg] > (mode == 0 ? value : _registers[value]) ? 2u : 0u;
                break;
            
            case Opcode.JEQ:
                if (_flags == 1)
                    _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JNE:
                if (_flags != 1)
                    _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JGT:
                if (_flags == 2)
                    _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JLT:
                if (_flags == 0)
                    _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JGE:
                if (_flags > 0)
                    _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JLE:
                if (_flags < 2)
                    _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JMP:
                _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JMPREL:
                _pc += mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.JMPX:
                _pc = value;
                break;
            
            case Opcode.CALL:
                StackPush(_pc);
                _pc = mode == 0 ? value : _registers[value];
                break;
            
            case Opcode.RET:
                _pc = StackPop();
                break;
            
            case Opcode.HLT:
                _pc = _ram.Size;
                break;
            
            default:
                throw new InvalidOperationException($"Unknown opcode: {opcode}");
        }
    }
    
    private void DumpState()
    {
        Console.WriteLine($"PC: 0x{_pc:X4}");
        for (var i = 0; i < _registers.Length; i++)
        {
            Console.WriteLine($"R{i}: {_registers[i]}");
        }
    }
}