# Teto

A minimal 32-bit CPU emulator that implements a simplified fixed-width x86-like instruction set.

## Architecture (currently)

- **Memory**: 64KB addressable memory space
- **Registers**: 8 general-purpose 32-bit registers (similar to x86):
    - eax, ebx, ecx, edx, esi, edi, esp, ebp
- **Memory Segments**:
    - Text Segment: 16KB
    - Data Segment: 16KB
    - Heap Segment: 16KB
    - Stack Segment: 16KB
- **Program Counter**: Starts at 0x0000
- **Stack**: Grows downwards from 0xFFFF
- **Heap**: Grows upwards from 0x8000

## Instruction Format

Each instruction is 32 bits (4 bytes) with the following layout:

```
0            8       12     16                32 (BIT)
+------------+-------+------+--------------------+
|   Opcode   |  Reg  | Mode |      Operand       |
+------------+-------+------+--------------------+

Opcode  = 8 bits  (Operation to execute)  
Reg     = 4 bits  (Target register)  
Mode    = 4 bits  (Addressing mode)
Operand = 16 bits (Immediate value or register address)
```

#### Addressing Modes

| Binary    | Decimal | Mode Name         | Description                                  |
|-----------|---------|-------------------|----------------------------------------------|
| 0000      | 0       | Immediate         | Operand is an immediate 16-bit WORD          |
| 0001      | 1       | Memory Direct     | Operand is a 16-bit address in memory        |
| 0010      | 2       | Register Direct   | Operand is a register (EAX, EBX, etc.)       |
| 0011      | 3       | Heap Relative     | Operand is an offset from the heap base      |
| 0100-1011 | 4-11    | Register Relative | Operand is an offset from the register value |

