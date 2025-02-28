using System;
using System.Collections.Generic;
using Xunit;

using Teto.MMU;
using Teto.Proc;

namespace Teto.Tests;

public class MovHighLowTests
{
    private readonly RAM _ram;
    private readonly CPU _cpu;

    public MovHighLowTests()
    {
        _ram = new RAM();
        _cpu = new CPU(_ram);
    }

    [Fact]
    public void MovHighLow_ShouldLoadFull32BitInteger()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVLO, CPU.EAX, 0x0, 0x1234));  // MOVLO R0, 0x1234
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVHI, CPU.EAX, 0x0, 0x5678));  // MOVHI R0, 0x5678
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));             // HLT

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(0x56781234, _cpu.GetRegister(CPU.EAX));
    }

    [Fact]
    public void MovHighLow_ShouldPreserveUnmodifiedBits()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EAX, 0x0, 0xFFFF));    // MOV R0, 0xFFFF
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVHI, CPU.EAX, 0x0, 0xAAAA));  // MOVHI R0, 0xAAAA
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));             // HLT

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(0xAAAAFFFF, (uint)_cpu.GetRegister(CPU.EAX));

        // Reset and test MOVLO
        _ram.Clear();
        _cpu.Reset();
        
        program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EAX, 0x0, 0));         // MOV R0, 0
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVHI, CPU.EAX, 0x0, 0xBBBB));  // MOVHI R0, 0xBBBB
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVLO, CPU.EAX, 0x0, 0xCCCC));  // MOVLO R0, 0xCCCC
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));             // HLT

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(0xBBBBCCCC, (uint)_cpu.GetRegister(CPU.EAX));
    }

    [Fact]
    public void MovHighLow_ShouldHandleNegativeValues()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVLO, CPU.EAX, 0x0, 0xFFFF));  // MOVLO R0, 0xFFFF
        program.AddRange(Utils.EncodeInstruction(Opcode.MOVHI, CPU.EAX, 0x0, 0xFFFF));  // MOVHI R0, 0xFFFF
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));             // HLT

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(-1, _cpu.GetRegister(CPU.EAX));
    }
}