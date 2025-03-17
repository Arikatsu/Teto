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
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EAX, InstrMode.IMM, 0x5));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EBX, InstrMode.IMM, 0x3));
        program.AddRange(Utils.EncodeInstruction(Opcode.ADD, CPU.EAX, InstrMode.REG, CPU.EBX));
        program.AddRange(Utils.EncodeInstruction(Opcode.ST, CPU.EAX, InstrMode.MEM, 0x1000));
        program.AddRange(Utils.EncodeInstruction(Opcode.LD, CPU.ECX, InstrMode.MEM, 0x1000));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0x0));
        
        _ram.LoadProgram(program.ToArray());
        _cpu.Run();
        
        Assert.Equal(0x8, _cpu.GetRegister(CPU.ECX));
        Assert.Equal(0x8, _ram.ReadWord(0x1000));
    }
    
    [Fact]
    public void LoadStoreHighLow_ShouldHandleMemoryOperations()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVHI, CPU.EAX, InstrMode.IMM, 0x1234)); 
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVLO, CPU.EAX, InstrMode.IMM, 0x5678));
        program.AddRange(Utils.EncodeInstruction(Opcode.STLO, CPU.EAX, InstrMode.MEM, 0x1000));
        program.AddRange(Utils.EncodeInstruction(Opcode.STHI, CPU.EAX, InstrMode.MEM, 0x1002));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EBX, InstrMode.IMM, 0));
        program.AddRange(Utils.EncodeInstruction(Opcode.LDLO, CPU.EBX, InstrMode.MEM, 0x1000));
        program.AddRange(Utils.EncodeInstruction(Opcode.LDHI, CPU.EBX, InstrMode.MEM, 0x1002));
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
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVHI, CPU.EAX, InstrMode.IMM, 0xAABB)); 
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVLO, CPU.EAX, InstrMode.IMM, 0xCCDD));
        program.AddRange(Utils.EncodeInstruction(Opcode.STHI, CPU.EAX, InstrMode.MEM, 0x1000));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVHI, CPU.EBX, InstrMode.IMM, 0x0000));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVLO, CPU.EBX, InstrMode.IMM, 0xFFFF));
        program.AddRange(Utils.EncodeInstruction(Opcode.LDHI, CPU.EBX, InstrMode.MEM, 0x1000));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(0xAABBFFFF, (uint)_cpu.GetRegister(CPU.EBX));
    }
}