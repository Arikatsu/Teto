using System;
using System.Collections.Generic;

using Teto;
using Teto.CPU;

var program = new List<byte>();
program.AddRange(Utils.EncodeInstruction(0x01, 0x0, 0x0, 0x5));
program.AddRange(Utils.EncodeInstruction(0x01, 0x1, 0x0, 0x3));
program.AddRange(Utils.EncodeInstruction(0x0A, 0x0, 0x1, 0x1));
program.AddRange(Utils.EncodeInstruction(0x2C, 0x0, 0x0, 0x0));

Memory.LoadProgram(program.ToArray());

CPU cpu = new();
cpu.Run();

Console.WriteLine($"R0: {cpu.GetRegister(0)}");
Console.WriteLine($"R1: {cpu.GetRegister(1)}");

