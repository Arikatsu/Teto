using System;

namespace Teto.CPU;

public class CPU
{
    private readonly uint[] _registers = new uint[8];
    private uint _pc = 0x200;
    private uint _flags = 0;
    
    public uint GetRegister(uint index) => _registers[index];
    
    public void DumpState()
    {
        Console.WriteLine($"PC: 0x{_pc:X4}");
        for (var i = 0; i < _registers.Length; i++)
        {
            Console.WriteLine($"R{i}: {_registers[i]}");
        }
    }
    
    private void Fetch()
    {
        var instr = Memory.Read(_pc) |
                    (uint)(Memory.Read(_pc + 1) << 8) |
                    (uint)(Memory.Read(_pc + 2) << 16) |
                    (uint)(Memory.Read(_pc + 3) << 24);
        _pc += 4;
        
        var opcode = instr & 0x3F;                  // 6 bits opcode
        var reg = (instr >> 6) & 0x7;               // 3 bits register
        var mode = (instr >> 9) & 0x1;              // 1 bit mode (0 = immediate, 1 = register)
        var operand = (instr >> 10) & 0x3FFFFF;     // 22 bits operand
        
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
            
            case Opcode.LD:
                _registers[reg] = Memory.Read(mode == 0 ? value : _registers[value]);
                break;
            
            case Opcode.ST:
                Memory.Write(mode == 0 ? value : _registers[value], (byte)_registers[reg]);
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