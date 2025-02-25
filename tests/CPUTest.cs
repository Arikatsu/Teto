using System;
using System.Collections.Generic;
using Xunit;

using Teto.Proc;
using Teto.MMU;

namespace Teto.Tests;

public class CPUTest
{
    [Fact]
    public void TestCPU()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(0x01, 0x0, 0x0, 0x5)); // MOV R0, 5
        program.AddRange(Utils.EncodeInstruction(0x01, 0x1, 0x0, 0x3)); // MOV R1, 3
        program.AddRange(Utils.EncodeInstruction(0x07, 0x0, 0x1, 0x1)); // ADD R0, R1
        program.AddRange(Utils.EncodeInstruction(0x03, 0x0, 0x1, 0x1)); // ST R0, R1
        program.AddRange(Utils.EncodeInstruction(0x02, 0x3, 0x0, 0x3)); // LD R3, 3
        program.AddRange(Utils.EncodeInstruction(0x2C, 0x0, 0x0, 0x0)); // HLT
        
        RAM ram = new();
        ram.LoadProgram(program.ToArray());
        
        CPU cpu = new(ram);
        cpu.Run();
        
        Assert.Equal(8u, cpu.GetRegister(0));
        Assert.Equal(3u, cpu.GetRegister(1));
        Assert.Equal(8u, cpu.GetRegister(3));
    }
}