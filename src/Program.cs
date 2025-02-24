using System;
using Teto;

var program = new byte[8];

Utils.WriteInstructionToByteArray(program, 0, Utils.EncodeInstruction(Opcode.MOV, 0, 0, 42));
Utils.WriteInstructionToByteArray(program, 4, Utils.EncodeInstruction(Opcode.MOV, 1, 0, 24));
Utils.WriteInstructionToByteArray(program, 8, Utils.EncodeInstruction(Opcode.ADD, 0, 0, 1));
Utils.WriteInstructionToByteArray(program, 12, Utils.EncodeInstruction(Opcode.SUB, 1, 0, 1));
Utils.WriteInstructionToByteArray(program, 16, Utils.EncodeInstruction(Opcode.HLT, 0, 0, 0));

Memory.LoadProgram(program);

CPU cpu = new();
cpu.Run();

Console.WriteLine($"R0: {cpu.GetRegister(0)}");
Console.WriteLine($"R1: {cpu.GetRegister(1)}");

