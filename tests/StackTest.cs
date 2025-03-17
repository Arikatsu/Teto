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
        program.AddRange(Utils.EncodeInstruction(Opcode.PUSH, 0x0, InstrMode.REG, 0));   // PUSH R0
        program.AddRange(Utils.EncodeInstruction(Opcode.PUSH, 0x1, InstrMode.REG, 0));   // PUSH R1
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
    
    [Fact]
    public void StackFrame_ShouldWorkCorrectly()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EAX, InstrMode.IMM, 0x1234));  // MOV R0, 0x1234 (subroutine argument)
        program.AddRange(Utils.EncodeInstruction(Opcode.CALL, 0x0, InstrMode.IMM, 0x000C));     // CALL 0x000C (subroutine address)
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));                // HLT
        program.AddRange(Utils.EncodeInstruction(Opcode.ENTER, 0x0, InstrMode.IMM, 8));         // ENTER 8 (create stack frame)
        program.AddRange(Utils.EncodeInstruction(Opcode.ST, CPU.EAX, InstrMode.REL_EBP, 0x4));  // ST R0, [EBP-4] (store argument)
        program.AddRange(Utils.EncodeInstruction(Opcode.LD, CPU.EBX, InstrMode.REL_EBP, 0x4));  // LD R0, [EBP-4] (load argument)
        program.AddRange(Utils.EncodeInstruction(Opcode.LEAVE, 0x0, 0x0, 0));              // LEAVE (restore stack frame)
        program.AddRange(Utils.EncodeInstruction(Opcode.RET, 0x0, 0x0, 0));                // RET
        
        _ram.LoadProgram(program.ToArray());
        _cpu.Run();
        
        Assert.Equal(0x1234, _cpu.GetRegister(CPU.EBX)); // Check if the value is preserved
    }
}