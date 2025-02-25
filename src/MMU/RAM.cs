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
    
    public uint ReadWord(uint address)
    {
        if (address + 4 > _memory.Length)
        {
            throw new IndexOutOfRangeException($"Address out of bounds: {address}");
        }
        
        return (uint)(_memory[address] |
                      (_memory[address + 1] << 8) |
                      (_memory[address + 2] << 16) |
                      (_memory[address + 3] << 24));
    }
    
    public void Write(uint address, byte value)
    {
        if (address >= _memory.Length)
        {
            throw new IndexOutOfRangeException($"Address out of bounds: {address}");
        }
        
        _memory[address] = value;
    }
    
    public void WriteWord(uint address, uint value)
    {
        if (address + 4 > _memory.Length)
        {
            throw new IndexOutOfRangeException($"Address out of bounds: {address}");
        }
        
        _memory[address] = (byte)(value & 0xFF);
        _memory[address + 1] = (byte)((value >> 8) & 0xFF);
        _memory[address + 2] = (byte)((value >> 16) & 0xFF);
        _memory[address + 3] = (byte)((value >> 24) & 0xFF);
    }
    
    public void LoadProgram(byte[] program)
    {
        if (program.Length + (uint)Segments.TextStart > _memory.Length)
        {
            throw new IndexOutOfRangeException("Program too large");
        }
        
        Array.Copy(program, 0, _memory, (uint)Segments.TextStart, program.Length);
    }
    
    public void Clear()
    {
        Array.Clear(_memory, 0, _memory.Length);
    }
}