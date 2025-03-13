using System.Collections.Generic;

using Teto;
using Teto.Proc;
using Teto.MMU;
using Teto.Debugging;

var program = new List<byte>();
program.AddRange(Utils.EncodeInstruction(Opcode.MOV, 0x0, 0x0, 1));   // MOV R0, 1
program.AddRange(Utils.EncodeInstruction(Opcode.CALL, 0x0, 0x0, 0x0020)); // CALL subroutine
program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));   // HLT
program.AddRange(new byte[0x20 - program.Count]); // Padding
program.AddRange(Utils.EncodeInstruction(Opcode.INC, 0x0, 0x0, 0));   // INC R0
program.AddRange(Utils.EncodeInstruction(Opcode.PUSH, 0x0, 0x0, 0));  // PUSH R0
program.AddRange(Utils.EncodeInstruction(Opcode.PUSH, 0x1, 0x0, 0));  // PUSH R1
program.AddRange(Utils.EncodeInstruction(Opcode.POP, 0x2, 0x0, 0));   // POP R2
program.AddRange(Utils.EncodeInstruction(Opcode.POP, 0x3, 0x0, 0));   // POP R3
program.AddRange(Utils.EncodeInstruction(Opcode.RET, 0x0, 0x0, 0));   // RET

RAM ram = new();
ram.LoadProgram(program.ToArray());

CPU cpu = new(ram);
Debugger debugger = new(cpu, ram);
debugger.Start();

