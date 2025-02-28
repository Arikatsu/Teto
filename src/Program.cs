using System;
using System.Collections.Generic;

using Teto;
using Teto.Proc;
using Teto.MMU;

var program = new List<byte>();
program.AddRange(Utils.EncodeInstruction(Opcode.MOVHI, CPU.EAX, 0x0, 0x1234)); // MOVHI R0, 0x1234
program.AddRange(Utils.EncodeInstruction(Opcode.MOVLO, CPU.EAX, 0x0, 0x5678)); // MOVLO R0, 0x5678
program.AddRange(Utils.EncodeInstruction(Opcode.STLO, CPU.EAX, 0x0, 0x100)); // STLO R0, 0x100
program.AddRange(Utils.EncodeInstruction(Opcode.STHI, CPU.EAX, 0x0, 0x102)); // STHI R0, 0x102
        
// Test load operations to different register
program.AddRange(Utils.EncodeInstruction(Opcode.MOV, CPU.EBX, 0x0, 0)); // MOV R1, 0
program.AddRange(Utils.EncodeInstruction(Opcode.LDLO, CPU.EBX, 0x0, 0x100)); // LDLO R1, 0x100
program.AddRange(Utils.EncodeInstruction(Opcode.LDHI, CPU.EBX, 0x0, 0x102)); // LDHI R1, 0x101
program.AddRange(Utils.EncodeInstruction(Opcode.HLT, 0x0, 0x0, 0));  

RAM ram = new();
ram.LoadProgram(program.ToArray());

CPU cpu = new(ram);
cpu.Run(dumpMemory: true, offset: 0x100, length: 5);

Console.WriteLine($"R0: {cpu.GetRegister(0)}");
Console.WriteLine($"R1: {cpu.GetRegister(1)}");

