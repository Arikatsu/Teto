using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

using Teto.MMU;
using Teto.Proc;

namespace Teto.Tests;

public class StackTests
{
    private readonly RAM _ram;
    private readonly CPU _cpu;
    private readonly ITestOutputHelper _output;

    public StackTests(ITestOutputHelper output)
    {
        _ram = new RAM();
        _cpu = new CPU(_ram);
        
        _output = output;
    }

    [Fact]
    public void PushAndPop_ShouldMaintainFifoOrder()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(0x01, 0x0, 0x0, 0x1234)); // MOV R0, 0x1234
        program.AddRange(Utils.EncodeInstruction(0x01, 0x1, 0x0, 0x5678)); // MOV R1, 0x5678
        program.AddRange(Utils.EncodeInstruction(0x04, 0x0, 0x0, 0x0000)); // PUSH R0
        program.AddRange(Utils.EncodeInstruction(0x04, 0x1, 0x0, 0x0000)); // PUSH R1
        program.AddRange(Utils.EncodeInstruction(0x05, 0x2, 0x0, 0x0000)); // POP R2
        program.AddRange(Utils.EncodeInstruction(0x05, 0x3, 0x0, 0x0000)); // POP R3
        program.AddRange(Utils.EncodeInstruction(0x2C, 0x0, 0x0, 0x0000)); // HLT

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();
        
        Assert.Equal(5678u, _cpu.GetRegister(2));
        Assert.Equal(1234u, _cpu.GetRegister(3));
        
        _cpu.Reset();
        _ram.Clear();
    }

    [Fact]
    public void CallAndReturn_ShouldWorkCorrectly()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(0x01, 0x0, 0x0, 0x01)); // MOV R0, 1
        program.AddRange(Utils.EncodeInstruction(0x26, 0x0, 0x0, 0x20)); // CALL subroutine
        program.AddRange(Utils.EncodeInstruction(0x2C, 0x0, 0x0, 0x00)); // HLT
        program.AddRange(new byte[0x20 - program.Count]); // Padding
        program.AddRange(Utils.EncodeInstruction(0x0C, 0x0, 0x0, 0x00)); // INC R0
        program.AddRange(Utils.EncodeInstruction(0x27, 0x0, 0x0, 0x00)); // RET

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(2u, _cpu.GetRegister(0));
        
        _cpu.Reset();
        _ram.Clear();
    }
}