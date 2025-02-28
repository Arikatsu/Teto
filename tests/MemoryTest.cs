using System.Collections.Generic;
using Xunit;

using Teto.Proc;
using Teto.MMU;

namespace Teto.Tests;

public class MemoryTest
{
    private readonly RAM _ram;
    private readonly CPU _cpu;
    
    public MemoryTest()
    {
        _ram = new RAM();
        _cpu = new CPU(_ram);
    }
    
    [Fact]
    public void StoreAndLoad_ShouldWorkCorrectly()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x0, 0x0, 0x5));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x1, 0x0, 0x3));
        program.AddRange(Utils.EncodeInstruction(Opcode.ADD, 0x0, 0x1, 0x1));
        program.AddRange(Utils.EncodeInstruction(Opcode.ST, 0x0, 0x1, 0x1));
        program.AddRange(Utils.EncodeInstruction(Opcode.LD, 0x3, 0x0, 0x3));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0x0));
        
        _ram.LoadProgram(program.ToArray());
        _cpu.Run();
        
        Assert.Equal(8, _cpu.GetRegister(0));
        Assert.Equal(3, _cpu.GetRegister(1));
        Assert.Equal(8, _cpu.GetRegister(3));
    }
    
    [Fact]
    public void LoadStoreHighLow_ShouldHandleMemoryOperations()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVHI, CPU.EAX, 0x0, 0x1234)); 
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVLO, CPU.EAX, 0x0, 0x5678));
        program.AddRange(Utils.EncodeInstruction(Opcode.STLO, CPU.EAX, 0x0, 0x100));
        program.AddRange(Utils.EncodeInstruction(Opcode.STHI, CPU.EAX, 0x0, 0x102));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EBX, 0x0, 0));
        program.AddRange(Utils.EncodeInstruction(Opcode.LDLO, CPU.EBX, 0x0, 0x100));
        program.AddRange(Utils.EncodeInstruction(Opcode.LDHI, CPU.EBX, 0x0, 0x102));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));          

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(0x12345678, _cpu.GetRegister(CPU.EAX));
        Assert.Equal(0x12345678, _cpu.GetRegister(CPU.EBX));
    }

    [Fact]
    public void LoadStoreHighLow_ShouldPreserveOtherBits()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVHI, CPU.EAX, 0x0, 0xAABB)); 
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVLO, CPU.EAX, 0x0, 0xCCDD));
        program.AddRange(Utils.EncodeInstruction(Opcode.STHI, CPU.EAX, 0x0, 0x100));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVHI, CPU.EBX, 0x0, 0x0000));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVLO, CPU.EBX, 0x0, 0xFFFF));
        program.AddRange(Utils.EncodeInstruction(Opcode.LDHI, CPU.EBX, 0x0, 0x100));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(0xAABBFFFF, (uint)_cpu.GetRegister(CPU.EBX));
    }
}