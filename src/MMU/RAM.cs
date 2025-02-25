using System;

namespace Teto.MMU;

public class RAM
{
    private readonly byte[] _memory = new byte[64 * 1024]; // 64KB

    public uint Size => (uint)_memory.Length;
    
    public void Dump(uint offset, uint length)
    {
        if (offset + length > _memory.Length)
        {
            throw new IndexOutOfRangeException("Dump out of bounds");
        }
        
        for (var i = offset; i < offset + length; i++)
        {
            Console.WriteLine($"0x{i:X4}: 0x{_memory[i]:X2}");
        }
    }
    
    public byte Read(uint address)
    {
        if (address >= _memory.Length)
        {
            throw new IndexOutOfRangeException($"Address out of bounds: {address}");
        }
        
        return _memory[address];
    }
    
    public void Write(uint address, byte value)
    {
        if (address >= _memory.Length)
        {
            throw new IndexOutOfRangeException($"Address out of bounds: {address}");
        }
        
        _memory[address] = value;
    }
    
    public void LoadProgram(byte[] program)
    {
        if (program.Length + (uint)Segments.TEXT_START > _memory.Length)
        {
            throw new IndexOutOfRangeException("Program too large");
        }
        
        Array.Copy(program, 0, _memory, (uint)Segments.TEXT_START, program.Length);
    }
    
    public void Clear()
    {
        Array.Clear(_memory, 0, _memory.Length);
    }
}