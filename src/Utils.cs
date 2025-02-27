using Teto.Proc;

namespace Teto;

public class Utils
{
    public static byte[] EncodeInstruction(Opcode opcode, byte reg, byte mode, int operand)
    {
        var instruction = ((byte)opcode & 0x3F)             // 6-bit opcode
                          | ((reg & 0x7) << 6)              // 3-bit register
                          | ((mode & 0x1) << 9)             // 1-bit mode
                          | ((operand & 0x3FFFFF) << 10);   // 22-bit operand
    
        return
        [
            (byte)(instruction & 0xFF),
            (byte)((instruction >> 8) & 0xFF),
            (byte)((instruction >> 16) & 0xFF),
            (byte)((instruction >> 24) & 0xFF)
        ];
    }
}