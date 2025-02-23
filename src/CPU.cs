using System;

namespace Teto;

public class CPU
{
    private readonly uint[] _registers = new uint[8];
    private uint _pc = 0x200;
    // private uint _flags = 0;
    
    public uint GetRegister(uint index) => _registers[index];
    
    private void Fetch()
    {
        var instr = Memory.Read(_pc) |
                    (uint)(Memory.Read(_pc + 1) << 8) |
                    (uint)(Memory.Read(_pc + 2) << 16) |
                    (uint)(Memory.Read(_pc + 3) << 24);
        _pc += 4;

        var opcode = (instr) & 0xFF;        // 8 bits opcode
        var reg = (instr >> 8) & 0xFF;      // 8 bits register
        var value = (instr >> 16) & 0xFFFF; // 16 bits immediate
        
        Execute(opcode, reg, value);
    }
    
    private void Execute(uint opcode, uint reg, uint value)
    {
        switch (opcode)
        {
            case 0x01: // MOV reg, imm
                _registers[reg] = value;
                break;
            
            case 0x02: // ADD reg, imm
                _registers[reg] += value;
                break;
            
            case 0x03: // ADD reg, reg
                _registers[reg] += _registers[value];
                break;
            
            case 0x04: // SUB reg, imm
                _registers[reg] -= value;
                break;
            
            case 0x05: // SUB reg, reg
                _registers[reg] -= _registers[value];
                break;
            
            case 0x06: // MUL reg, imm
                _registers[reg] *= value;
                break;
            
            case 0x07: // MUL reg, reg
                _registers[reg] *= _registers[value];
                break;
            
            case 0x08: // DIV reg, imm
                _registers[reg] /= value;
                break;
            
            case 0x09: // DIV reg, reg
                _registers[reg] /= _registers[value];
                break;
            
            case 0x0A: // AND reg, imm
                _registers[reg] &= value;
                break;
            
            case 0x0B: // AND reg, reg
                _registers[reg] &= _registers[value];
                break;
            
            case 0x0C: // OR reg, imm
                _registers[reg] |= value;
                break;
            
            case 0x0D: // OR reg, reg
                _registers[reg] |= _registers[value];
                break;
            
            case 0x0E: // XOR reg, imm
                _registers[reg] ^= value;
                break;
            
            case 0x0F: // XOR reg, reg
                _registers[reg] ^= _registers[value];
                break;
            
            case 0x10: // NOT reg
                _registers[reg] = ~_registers[reg];
                break;
            
            case 0x11: // SHL reg, imm
                _registers[reg] <<= (int)value;
                break;
            
            case 0x12: // SHL reg, reg
                _registers[reg] <<= (int)_registers[value];
                break;
            
            case 0x13: // SHR reg, imm
                _registers[reg] >>= (int)value;
                break;
            
            case 0x14: // SHR reg, reg
                _registers[reg] >>= (int)_registers[value];
                break;
            
            case 0x15: // CMP reg, imm
                _registers[reg] = _registers[reg] == value ? 1u : 0u;
                break;
            
            case 0x16: // CMP reg, reg
                _registers[reg] = _registers[reg] == _registers[value] ? 1u : 0u;
                break;
            
            case 0xFF: // HLT
                _pc = Memory.Size;
                break;
            
            default:
                throw new InvalidOperationException($"Unknown opcode: 0x{opcode:X2}");
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