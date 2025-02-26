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
        program.AddRange(Utils.EncodeInstruction(0x01, 0x0, 0x0, 1234)); // MOV R0, 0x1234
        program.AddRange(Utils.EncodeInstruction(0x01, 0x1, 0x0, 5678)); // MOV R1, 0x5678
        program.AddRange(Utils.EncodeInstruction(0x04, 0x0, 0x0, 0));    // PUSH R0
        program.AddRange(Utils.EncodeInstruction(0x04, 0x1, 0x0, 0));    // PUSH R1
        program.AddRange(Utils.EncodeInstruction(0x05, 0x2, 0x0, 0));    // POP R2
        program.AddRange(Utils.EncodeInstruction(0x05, 0x3, 0x0, 0));    // POP R3
        program.AddRange(Utils.EncodeInstruction(0x2C, 0x0, 0x0, 0));    // HLT

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
        program.AddRange(Utils.EncodeInstruction(0x01, 0x0, 0x0, 1));  // MOV R0, 1
        program.AddRange(Utils.EncodeInstruction(0x26, 0x0, 0x0, 20)); // CALL subroutine
        program.AddRange(Utils.EncodeInstruction(0x2C, 0x0, 0x0, 0));  // HLT
        program.AddRange(new byte[0x20 - program.Count]); // Padding
        program.AddRange(Utils.EncodeInstruction(0x0C, 0x0, 0x0, 0));  // INC R0
        program.AddRange(Utils.EncodeInstruction(0x27, 0x0, 0x0, 0));  // RET

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(2, _cpu.GetRegister(0));
        
        _cpu.Reset();
        _ram.Clear();
    }
}