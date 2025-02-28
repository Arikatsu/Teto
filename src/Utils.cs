using System;

using Teto.Proc;

namespace Teto;

public static class Utils
{
    public static byte[] EncodeInstruction(Opcode opcode, byte reg, byte mode, int operand)
    {
        var instruction = ((byte)opcode & 0xFF)           // 8-bit opcode
                          | ((reg & 0xF) << 8)               // 4-bit register
                          | ((mode & 0xF) << 12)             // 4-bit mode
                          | ((operand & 0xFFFF) << 16);      // 16-bit operand
    
        return
        [
            (byte)(instruction & 0xFF),
            (byte)((instruction >> 8) & 0xFF),
            (byte)((instruction >> 16) & 0xFF),
            (byte)((instruction >> 24) & 0xFF)
        ];
    }
}