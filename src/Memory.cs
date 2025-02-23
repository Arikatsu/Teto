using System;

namespace Teto;

public static class Memory
{
    private static readonly byte[] MemoryArray = new byte[64 * 1024]; // 64KB
    
    public static uint Size => (uint)MemoryArray.Length;
    
    public static void Dump(uint offset, uint length)
    {
        if (offset + length > MemoryArray.Length)
        {
            throw new IndexOutOfRangeException("Dump out of bounds");
        }
        
        for (var i = offset; i < offset + length; i++)
        {
            Console.WriteLine($"0x{i:X4}: 0x{MemoryArray[i]:X2}");
        }
    }
    
    public static byte Read(uint address)
    {
        if (address >= MemoryArray.Length)
        {
            throw new IndexOutOfRangeException($"Address out of bounds: {address}");
        }
        
        return MemoryArray[address];
    }
    
    public static void Write(uint address, byte value)
    {
        if (address >= MemoryArray.Length)
        {
            throw new IndexOutOfRangeException($"Address out of bounds: {address}");
        }
        
        MemoryArray[address] = value;
    }
    
    public static void LoadProgram(byte[] program)
    {
        if (program.Length + 0x200 > MemoryArray.Length)
        {
            throw new IndexOutOfRangeException("Program too large");
        }
        
        Array.Copy(program, 0, MemoryArray, 0x200, program.Length);
    }
    
    public static void Clear()
    {
        Array.Clear(MemoryArray, 0, MemoryArray.Length);
    }
}