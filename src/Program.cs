using System;
using System.Collections.Generic;

using Teto;
using Teto.Proc;
using Teto.MMU;
using Teto.Debugging;

var program = new List<byte>();
program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EAX, InstrMode.IMM, 0x1234)); // MOV R0, 0x1234
program.AddRange(Utils.EncodeInstruction(Opcode.CALL, 0x0, InstrMode.IMM, 12)); // CALL 12
program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0)); // HLT
program.AddRange(Utils.EncodeInstruction(Opcode.ENTER, 0x0, InstrMode.IMM, 8)); // ENTER 8
program.AddRange(Utils.EncodeInstruction(Opcode.ST, CPU.EAX, InstrMode.REL_HEAP, 0x4)); // ST R0, [EBP-4]
program.AddRange(Utils.EncodeInstruction(Opcode.LD, CPU.EBX, InstrMode.REL_HEAP, 0x4)); // LD R0, [EBP-4]
program.AddRange(Utils.EncodeInstruction(Opcode.LEAVE, 0x0, 0x0, 0)); // LEAVE
program.AddRange(Utils.EncodeInstruction(Opcode.RET, 0x0, 0x0, 0)); // RET

RAM ram = new();
ram.LoadProgram(program.ToArray());

CPU cpu = new(ram);
Debugger debugger = new(cpu, ram);
debugger.Start();

