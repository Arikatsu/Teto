# Teto

A minimal 32-bit CPU emulator that implements a simplified fixed-width x86-like instruction set.

## Architecture (currently)

- **Memory**: 64KB addressable memory space
- **Registers**: 8 general-purpose 32-bit registers (similar to x86):
    - EAX
    - EBX
    - ECX
    - EDX
    - ESI
    - EDI
    - EBP
    - ESP
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
0        6       9      10                32 (BIT)
+--------+-------+------+--------------------+
| Opcode |  Reg  | Mode |      Operand       |
+--------+-------+------+--------------------+

Opcode  = 6 bits  (Operation to execute)  
Reg     = 3 bits  (Target register)  
Mode    = 1 bit   (0 = Immediate, 1 = Register)  
Operand = 22 bits (Immediate value or register address)
```

