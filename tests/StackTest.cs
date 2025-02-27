using System;
using System.Collections.Generic;
using Xunit;

using Teto.MMU;
using Teto.Proc;

namespace Teto.Tests;

public class StackTests
{
    private readonly RAM _ram;
    private readonly CPU _cpu;

    public StackTests()
    {
        _ram = new RAM();
        _cpu = new CPU(_ram);
    }

    [Fact]
    public void PushAndPop_ShouldMaintainFifoOrder()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x0, 0x0, 1234)); // MOV R0, 0x1234
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x1, 0x0, 5678)); // MOV R1, 0x5678
        program.AddRange(Utils.EncodeInstruction(Opcode.PUSH, 0x0, 0x0, 0));   // PUSH R0
        program.AddRange(Utils.EncodeInstruction(Opcode.PUSH, 0x1, 0x0, 0));   // PUSH R1
        program.AddRange(Utils.EncodeInstruction(Opcode.POP, 0x2, 0x0, 0));    // POP R2
        program.AddRange(Utils.EncodeInstruction(Opcode.POP, 0x3, 0x0, 0));    // POP R3
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));    // HLT

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();
        
        Assert.Equal(5678, _cpu.GetRegister(2));
        Assert.Equal(1234, _cpu.GetRegister(3));
        
        _cpu.Reset();
        _ram.Clear();
    }

    [Fact]
    public void CallAndReturn_ShouldWorkCorrectly()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x0, 0x0, 1));   // MOV R0, 1
        program.AddRange(Utils.EncodeInstruction(Opcode.CALL, 0x0, 0x0, 20)); // CALL subroutine
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));   // HLT
        program.AddRange(new byte[0x20 - program.Count]); // Padding
        program.AddRange(Utils.EncodeInstruction(Opcode.INC, 0x0, 0x0, 0));   // INC R0
        program.AddRange(Utils.EncodeInstruction(Opcode.RET, 0x0, 0x0, 0));   // RET

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(2, _cpu.GetRegister(0));
        
        _cpu.Reset();
        _ram.Clear();
    }
}