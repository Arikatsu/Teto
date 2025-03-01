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
                    $"{opcodeName} {(mode == 0 ? "$" : string.Empty)}{(mode == 0 ? $"0x{operand:X4}" : GetRegisterName(operand & 0xF))}",
            
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
            0 => "EAX",
            1 => "EBX",
            2 => "ECX",
            3 => "EDX",
            4 => "ESI",
            5 => "EDI",
            6 => "EBP",
            7 => "ESP",
            _ => $"R{reg}"
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
                $"{opcodeName} {(mode == 0 ? "$" : string.Empty)}{(mode == 0 ? $"0x{operand:X4}" : GetRegisterName(operand & 0xF))}",
        
            _ => FormatStandardInstruction(opcodeName, reg, mode, operand)
        };
    }
    
    private static string FormatStandardInstruction(string opcodeName, byte reg, byte mode, ushort operand)
    {
        var regName = GetRegisterName(reg);
        var modeStr = mode == 0 ? "$" : string.Empty;
        var operandStr = mode == 0 ? $"0x{operand:X4}" : GetRegisterName(operand & 0xF);
        return $"{opcodeName} {regName}, {modeStr}{operandStr}";
    }

    private static string GetOpcodeName(byte opcode)
    {
        return opcode switch
        {
            0x00 => "NOP",
            0x01 => "MOV",
            0x02 => "MOVHI",
            0x03 => "MOVLO",
            0x04 => "LD",
            0x05 => "LDHI",
            0x06 => "LDLO",
            0x07 => "ST",
            0x08 => "STHI",
            0x09 => "STLO",
            0x0A => "PUSH",
            0x0B => "POP",
            0x0C => "XCHG",
            0x0D => "ADD",
            0x0E => "SUB",
            0x0F => "MUL",
            0x10 => "DIV",
            0x11 => "MOD",
            0x12 => "INC",
            0x13 => "DEC",
            0x14 => "NEG",
            0x15 => "FADD",
            0x16 => "FSUB",
            0x17 => "FMUL",
            0x18 => "FDIV",
            0x19 => "FMOVHI",
            0x1A => "FMOVLO",
            0x1B => "ITOF",
            0x1C => "FTOI",
            0x1D => "AND",
            0x1E => "OR",
            0x1F => "XOR",
            0x20 => "NOT",
            0x21 => "SHL",
            0x22 => "SHR",
            0x23 => "ROL",
            0x24 => "ROR",
            0x25 => "TEST",
            0x26 => "CMP",
            0x27 => "JEQ",
            0x28 => "JNE",
            0x29 => "JGT",
            0x2A => "JLT",
            0x2B => "JGE",
            0x2C => "JLE",
            0x2D => "JMP",
            0x2E => "JMPREL",
            0x2F => "JMPX",
            0x30 => "CALL",
            0x31 => "RET",
            0x32 => "ENTER",
            0x33 => "LEAVE",
            0x34 => "INT",
            0x35 => "IRET",
            0x36 => "HLT",
            0x37 => "SYSCALL",
            0x38 => "CLI",
            0x39 => "STI",
            _ => $"OP{opcode:X2}"
        };
    }
}