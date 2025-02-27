using System.Collections.Generic;
using Xunit;

using Teto.MMU;
using Teto.Proc;

namespace Teto.Tests;

public class ArithmeticTests
{
    private readonly RAM _ram;
    private readonly CPU _cpu;

    public ArithmeticTests()
    {
        _ram = new RAM();
        _cpu = new CPU(_ram);
    }

    [Fact]
    public void TwosComplementArithmetic_ShouldWorkCorrectly()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x0, 0x0, -5)); // MOV R0, -5
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x1, 0x0, -3)); // MOV R1, -3
        program.AddRange(Utils.EncodeInstruction(Opcode.ADD, 0x0, 0x1, 1));  // ADD R0, R1
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));  // HLT

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(-8, _cpu.GetRegister(CPU.EAX));
    }
}