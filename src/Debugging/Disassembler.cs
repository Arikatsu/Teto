using System.Collections.Generic;

using Teto.MMU;

namespace Teto.Debugging;

public class Disassembler(RAM ram)
{
    private readonly Dictionary<int, string> _cache = new();

    public string DisassembleInstruction(int address)
    {
        if (_cache.TryGetValue(address, out var cached)) return cached;

        try
        {
            var instruction = ram.ReadWord((uint)address);

            var opcode = (byte)(instruction & 0xFF);
            var reg = (byte)((instruction >> 8) & 0xF);
            var mode = (byte)((instruction >> 12) & 0xF);
            var operand = (ushort)((instruction >> 16) & 0xFFFF);

            var opcodeName = GetOpcodeName(opcode);
            var result = opcode switch
            {
                0x00 or 0x31 or 0x36 => opcodeName,
            
                >= 0x27 and <= 0x2F => 
                    $"{opcodeName} {FormatOperandWithAddrMode(mode, operand)}",
            
                _ => FormatStandardInstruction(opcodeName, reg, mode, operand)
            };

            _cache[address] = result;
            return result;
        }
        catch
        {
            return "???";
        }
    }

    public void ClearCache()
    {
        _cache.Clear();
    }
    
    public static string GetRegisterName(int reg)
    {
        return reg switch
        {
            0 => "eax",
            1 => "ebx",
            2 => "ecx",
            3 => "edx",
            4 => "esi",
            5 => "edi",
            6 => "ebp",
            7 => "esp",
            _ => $"r{reg}"
        };
    }
    
    public static string DisassembleIntInstruction(int instruction)
    {
        var opcode = (byte)(instruction & 0xFF);
        var reg = (byte)((instruction >> 8) & 0xF);
        var mode = (byte)((instruction >> 12) & 0xF);
        var operand = (ushort)((instruction >> 16) & 0xFFFF);

        var opcodeName = GetOpcodeName(opcode);
        return opcode switch
        {
            0x00 or 0x31 or 0x36 => opcodeName,
        
            >= 0x27 and <= 0x2F => 
                $"{opcodeName} {FormatOperandWithAddrMode(mode, operand)}",
        
            _ => FormatStandardInstruction(opcodeName, reg, mode, operand)
        };
    }
    
    private static string FormatStandardInstruction(string opcodeName, byte reg, byte mode, ushort operand)
    {
        var regName = GetRegisterName(reg);
        var operandStr = FormatOperandWithAddrMode(mode, operand);
        return $"{opcodeName} {regName}, {operandStr}";
    }
    
    private static string FormatOperandWithAddrMode(byte mode, ushort operand)
    {
        return mode switch
        {
            0 => $"0x{operand:X4}",
            1 => $"[0x{operand:X4}]",
            2 => $"{GetRegisterName(operand & 0xF)}",
            3 => $"[heap + 0x{operand:X4}]",
            4 => $"[eax + 0x{operand:X4}]",
            5 => $"[ebx + 0x{operand:X4}]",
            6 => $"[ecx + 0x{operand:X4}]",
            7 => $"[edx + 0x{operand:X4}]",
            8 => $"[esi + 0x{operand:X4}]",
            9 => $"[edi + 0x{operand:X4}]",
            10 => $"[ebp - 0x{operand:X4}]",
            11 => $"[esp - 0x{operand:X4}]",
            _ => $"[??? {operand:X4}]"
        };
    }

    private static string GetOpcodeName(byte opcode)
    {
        return opcode switch
        {
            0x00 => "nop",
            0x01 => "mov",
            0x02 => "movhi",
            0x03 => "movlo",
            0x04 => "ld",
            0x05 => "ldhi",
            0x06 => "ldlo",
            0x07 => "st",
            0x08 => "sthi",
            0x09 => "stlo",
            0x0A => "push",
            0x0B => "pop",
            0x0C => "xchg",
            0x0D => "add",
            0x0E => "sub",
            0x0F => "mul",
            0x10 => "div",
            0x11 => "mod",
            0x12 => "inc",
            0x13 => "dec",
            0x14 => "neg",
            0x15 => "fadd",
            0x16 => "fsub",
            0x17 => "fmul",
            0x18 => "fdiv",
            0x19 => "fmovhi",
            0x1A => "fmovlo",
            0x1B => "itof",
            0x1C => "ftoi",
            0x1D => "and",
            0x1E => "or",
            0x1F => "xor",
            0x20 => "not",
            0x21 => "shl",
            0x22 => "shr",
            0x23 => "rol",
            0x24 => "ror",
            0x25 => "test",
            0x26 => "cmp",
            0x27 => "jeq",
            0x28 => "jne",
            0x29 => "jgt",
            0x2A => "jlt",
            0x2B => "jge",
            0x2C => "jle",
            0x2D => "jmp",
            0x2E => "jmprel",
            0x2F => "jmpx",
            0x30 => "call",
            0x31 => "ret",
            0x32 => "enter",
            0x33 => "leave",
            0x34 => "int",
            0x35 => "iret",
            0x36 => "hlt",
            0x37 => "cli",
            0x38 => "sti",
            _ => $"op{opcode:X2}"
        };
    }
}