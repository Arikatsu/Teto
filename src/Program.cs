using System;
using Teto;

var program = new byte[]
{
    0x01, 0x00, 0x05, 0x00, // MOV R0, 5
    0x01, 0x01, 0x03, 0x00, // MOV R1, 3
    0x03, 0x00, 0x01, 0x00, // ADD R0, R1
    0xFF, 0x00, 0x00, 0x00  // HLT
};

Memory.LoadProgram(program);

CPU cpu = new();
cpu.Run();

Console.WriteLine($"R0: {cpu.GetRegister(0)}");
Console.WriteLine($"R1: {cpu.GetRegister(1)}");

