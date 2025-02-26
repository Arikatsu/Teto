namespace Teto;

public class Utils
{
    public static byte[] EncodeInstruction(byte opcode, byte reg, byte mode, int operand)
    {
        opcode &= 0x3F;        // 6 bits
        reg &= 0x7;            // 3 bits
        mode &= 0x1;           // 1 bit
        operand &= 0x3FFFFF;   // 22 bits
    
        var instruction = opcode | (reg << 6) | (mode << 9) | (operand << 10);
    
        return
        [
            (byte)(instruction & 0xFF),
            (byte)((instruction >> 8) & 0xFF),
            (byte)((instruction >> 16) & 0xFF),
            (byte)((instruction >> 24) & 0xFF)
        ];
    }
}