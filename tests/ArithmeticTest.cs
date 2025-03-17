using System.Collections.Generic;
using Teto.MMU;
using Teto.Proc;
using Xunit;

namespace Teto.Tests;

public class ArithmeticTests
{
    private readonly CPU _cpu;
    private readonly RAM _ram;

    public ArithmeticTests()
    {
        _ram = new RAM();
        _cpu = new CPU(_ram);
    }

    [Fact]
    public void TwosComplementArithmetic_ShouldWorkCorrectly()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EAX, InstrMode.IMM, -5));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EBX, InstrMode.IMM, -3));
        program.AddRange(Utils.EncodeInstruction(Opcode.ADD, CPU.EAX, InstrMode.REG, CPU.EBX));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(-8, _cpu.GetRegister(CPU.EAX));

        _ram.Clear();
        _cpu.Reset();

        program = [];
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EAX, InstrMode.IMM, -5));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EBX, InstrMode.IMM, -3));
        program.AddRange(Utils.EncodeInstruction(Opcode.SUB, CPU.EAX, InstrMode.REG, CPU.EBX));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(-2, _cpu.GetRegister(CPU.EAX));

        _ram.Clear();
        _cpu.Reset();

        program = [];
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EAX, InstrMode.IMM, -5));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EBX, InstrMode.IMM, -3));
        program.AddRange(Utils.EncodeInstruction(Opcode.MUL, CPU.EAX, InstrMode.REG, CPU.EBX));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(15, _cpu.GetRegister(CPU.EAX));

        _ram.Clear();
        _cpu.Reset();

        program = [];
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EAX, InstrMode.IMM, -5));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EBX, InstrMode.IMM, -3));
        program.AddRange(Utils.EncodeInstruction(Opcode.DIV, CPU.EAX, InstrMode.REG, CPU.EBX));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(1, _cpu.GetRegister(CPU.EAX));

        _ram.Clear();
        _cpu.Reset();

        program = [];
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EAX, InstrMode.IMM, -5));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EBX, InstrMode.IMM, -3));
        program.AddRange(Utils.EncodeInstruction(Opcode.MOD, CPU.EAX, InstrMode.REG, CPU.EBX));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(-2, _cpu.GetRegister(CPU.EAX));
    }

    [Fact]
    public void FloatingPointArithmetic_ShouldWorkCorrectly()
    {
        //
        // Operands: 3.14159 and 1.0
        //

        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVLO, CPU.EAX, InstrMode.IMM, 0x0FDB));
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVHI, CPU.EAX, InstrMode.IMM, 0x4049));
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVLO, CPU.EBX, InstrMode.IMM, 0x0000));
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVHI, CPU.EBX, InstrMode.IMM, 0x3F80));
        program.AddRange(Utils.EncodeInstruction(Opcode.FADD, CPU.EAX, InstrMode.REG, CPU.EBX));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(4.14159f, _cpu.GetRegisterF(CPU.EAX), 4);

        _ram.Clear();
        _cpu.Reset();

        program = [];
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVLO, CPU.EAX, InstrMode.IMM, 0x0FDB));
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVHI, CPU.EAX, InstrMode.IMM, 0x4049));
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVLO, CPU.EBX, InstrMode.IMM, 0x0000));
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVHI, CPU.EBX, InstrMode.IMM, 0x3F80));

        program.AddRange(Utils.EncodeInstruction(Opcode.FSUB, CPU.EAX, InstrMode.REG, CPU.EBX));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(2.14159f, _cpu.GetRegisterF(CPU.EAX), 4);

        _ram.Clear();
        _cpu.Reset();

        program = [];
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVLO, CPU.EAX, InstrMode.IMM, 0x0FDB));
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVHI, CPU.EAX, InstrMode.IMM, 0x4049));
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVLO, CPU.EBX, InstrMode.IMM, 0x0000));
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVHI, CPU.EBX, InstrMode.IMM, 0x4000));

        program.AddRange(Utils.EncodeInstruction(Opcode.FMUL, CPU.EAX, InstrMode.REG, CPU.EBX));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(6.28318f, _cpu.GetRegisterF(CPU.EAX), 4);

        _ram.Clear();
        _cpu.Reset();

        program = [];
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVLO, CPU.EAX, InstrMode.IMM, 0x0FDB));
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVHI, CPU.EAX, InstrMode.IMM, 0x4049));
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVLO, CPU.EBX, InstrMode.IMM, 0x0000));
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVHI, CPU.EBX, InstrMode.IMM, 0x4000));
        program.AddRange(Utils.EncodeInstruction(Opcode.FDIV, CPU.EAX, InstrMode.REG, CPU.EBX));
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(1.57079f, _cpu.GetRegisterF(CPU.EAX), 4);
    }
    
    [Fact]
    public void FloatingPointArithmetic_ShouldHandleNegativeValues()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVLO, CPU.EAX, InstrMode.IMM, 0x0FDB)); // 3.14159
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVHI, CPU.EAX, InstrMode.IMM, 0x4049));
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVLO, CPU.EBX, InstrMode.IMM, 0x0001)); // -1.0
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVHI, CPU.EBX, InstrMode.IMM, 0xBF80));
        program.AddRange(Utils.EncodeInstruction(Opcode.FADD, CPU.EAX, InstrMode.REG, CPU.EBX)); // 3.14159 + -1.0
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(2.14159f, _cpu.GetRegisterF(CPU.EAX), 4);
    }
    
    [Fact]
    public void FtoI_ItoF_ShouldWorkCorrectly()
    {
        var program = new List<byte>();
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVLO, CPU.EAX, InstrMode.IMM, 0x0001)); // 1.0
        program.AddRange(Utils.EncodeInstruction(Opcode.FMOVHI, CPU.EAX, InstrMode.IMM, 0x3F80));
        program.AddRange(Utils.EncodeInstruction(Opcode.FTOI, CPU.EAX, InstrMode.REG, CPU.EBX)); // Convert to int
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(1, _cpu.GetRegister(CPU.EBX));

        _ram.Clear();
        _cpu.Reset();

        program = [];
        program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EAX, InstrMode.IMM, 1)); // 1
        program.AddRange(Utils.EncodeInstruction(Opcode.ITOF, CPU.EAX, InstrMode.REG, CPU.EBX)); // Convert to float
        program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));

        _ram.LoadProgram(program.ToArray());
        _cpu.Run();

        Assert.Equal(1.0f, _cpu.GetRegisterF(CPU.EBX), 4);
    }
}