namespace Teto;

public class Utils
{
    public static int EncodeInstruction(Opcode opcode, int reg, int mode, int operand)
    {
        return (operand << 10) | (mode << 9) | (reg << 6) | (int)opcode;
    }

    public static void WriteInstructionToByteArray(byte[] byteArray, int offset, int instruction)
    {
        byteArray[offset] = (byte)(instruction & 0xFF);             // Byte 0 (LSB)
        byteArray[offset + 1] = (byte)((instruction >> 8) & 0xFF);  // Byte 1
        byteArray[offset + 2] = (byte)((instruction >> 16) & 0xFF); // Byte 2
        byteArray[offset + 3] = (byte)((instruction >> 24) & 0xFF); // Byte 3 (MSB)
    }
}