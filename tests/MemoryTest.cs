using System.Collections.Generic;
using Xunit;

using Teto.Proc;
using Teto.MMU;

namespace Teto.Tests;

public class MemoryTest
{
    [Fact]
    public void StoreAndLoad_ShouldWorkCorrectly()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x0, 0x0, 0x5)); // MOV R0, 5
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x1, 0x0, 0x3)); // MOV R1, 3
        program.AddRange(Utils.EncodeInstruction(Opcode.ADD, 0x0, 0x1, 0x1)); // ADD R0, R1
        program.AddRange(Utils.EncodeInstruction(Opcode.ST, 0x0, 0x1, 0x1));  // ST R0, R1
        program.AddRange(Utils.EncodeInstruction(Opcode.LD, 0x3, 0x0, 0x3));  // LD R3, 3
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0x0)); // HLT
        
        RAM ram = new();
        ram.LoadProgram(program.ToArray());
        
        CPU cpu = new(ram);
        cpu.Run();
        
        Assert.Equal(8, cpu.GetRegister(0));
        Assert.Equal(3, cpu.GetRegister(1));
        Assert.Equal(8, cpu.GetRegister(3));
    }
}