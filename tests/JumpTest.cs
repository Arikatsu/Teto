using System.Collections.Generic;
using Xunit;

using Teto.MMU;
using Teto.Proc;

namespace Teto.Tests;

public class JumpTest
{
    private readonly RAM _ram;
    private readonly CPU _cpu;

    public JumpTest()
    {
        _ram = new RAM();
        _cpu = new CPU(_ram);
    }
    
    [Fact]
    public void ConditionalJumps_ShouldWorkCorrectly()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x0, 0x0, 5));     // MOV R0, 5
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x1, 0x0, 3));     // MOV R1, 3
        program.AddRange(Utils.EncodeInstruction(Opcode.CMP, 0x0, 0x1, 1));     // CMP R0, R1
        program.AddRange(Utils.EncodeInstruction(Opcode.JGT, 0x0, 0x0, 24));    // JGT 24 (skip 2 instructions)
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x2, 0x0, 0));     // MOV R2, 0 (skipped)
        program.AddRange(Utils.EncodeInstruction(Opcode.JMP, 0x0, 0x0, 28));    // JMP 28 (skipped)
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x2, 0x0, 1));     // MOV R2, 1
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));     // HLT

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(1, _cpu.GetRegister(2));
    }

    [Fact]
    public void RelativeJump_ShouldWorkCorrectly()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x0, 0x0, 1));      // MOV R0, 1
        program.AddRange(Utils.EncodeInstruction(Opcode.JMPREL, 0x0, 0x0, 8));   // JMPREL 8 (skip next instruction)
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x0, 0x0, 2));      // MOV R0, 2 (skipped)
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));      // HLT

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(1, _cpu.GetRegister(0));
    }
}