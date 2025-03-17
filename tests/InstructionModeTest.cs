using System.Collections.Generic;
using Xunit;

using Teto.MMU;
using Teto.Proc;

namespace Teto.Tests
{
    public class ModesTests
    {
        private readonly RAM _ram;
        private readonly CPU _cpu;

        public ModesTests()
        {
            _ram = new RAM();
            _cpu = new CPU(_ram);
        }

        [Fact]
        public void ImmediateMode_ShouldUseValueDirectly()
        {
            var program = new List<byte>();
            program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0, InstrMode.IMM, 1234)); // MOV R0, $1234 (immediate)
            program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0, 0, 0));
            
            _ram.LoadProgram(program.ToArray());
            _cpu.Run();
            
            Assert.Equal(1234, _cpu.GetRegister(0));
            _cpu.Reset();
            _ram.Clear();
        }

        [Fact]
        public void RegisterMode_ShouldUseRegisterValue()
        {
            var program = new List<byte>();
            program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0, InstrMode.IMM, 1234)); // MOV R0, $1234
            program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 1, InstrMode.REG, 0));    // MOV R1, R0 (register mode)
            program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0, 0, 0));
            
            _ram.LoadProgram(program.ToArray());
            _cpu.Run();
            
            Assert.Equal(1234, _cpu.GetRegister(1));
            _cpu.Reset();
            _ram.Clear();
        }

        [Fact]
        public void MemoryDirectMode_ShouldReadFromMemory()
        {
            const uint memAddress = 0x1000;
            _ram.Write(0x1000, 42);
            
            var program = new List<byte>();
            program.AddRange(Utils.EncodeInstruction(Opcode.LD, 0, InstrMode.MEM, (int)memAddress)); // LD R0, [0x1000] (memory direct)
            program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0, 0, 0));
            
            _ram.LoadProgram(program.ToArray());
            _cpu.Run();
            
            Assert.Equal(42, _cpu.GetRegister(0));
            _cpu.Reset();
            _ram.Clear();
        }

        [Fact]
        public void HeapOffsetMode_ShouldReadFromHeapWithOffset()
        {
            const uint heapOffset = 100;
            _ram.Write(Segments.HeapStart + heapOffset, 55);
            
            var program = new List<byte>();
            program.AddRange(Utils.EncodeInstruction(Opcode.LD, 0, InstrMode.REL_HEAP, (int)heapOffset)); // LD R0, [HEAP+100]
            program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0, 0, 0));
            
            _ram.LoadProgram(program.ToArray());
            _cpu.Run();
            
            Assert.Equal(55, _cpu.GetRegister(0));
            _cpu.Reset();
            _ram.Clear();
        }

        [Fact]
        public void RegisterOffsetMode_ShouldReadFromRegisterPlusOffset()
        {
            const uint baseAddr = 0x2000;
            const int offset = 24;
            _ram.Write(baseAddr + (uint)offset, 99);
            
            var program = new List<byte>();
            program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0, InstrMode.IMM, (int)baseAddr)); // MOV R0, $0x2000
            program.AddRange(Utils.EncodeInstruction(Opcode.LD, 1, InstrMode.REL_EAX, offset));         // LD R1, [R0+24]
            program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0, 0, 0));
            
            _ram.LoadProgram(program.ToArray());
            _cpu.Run();
            
            Assert.Equal(99, _cpu.GetRegister(1));
            _cpu.Reset();
            _ram.Clear();
        }
    }
}