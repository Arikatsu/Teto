# Teto

A minimal 32-bit CPU emulator that implements a simplified fixed-width x86-like instruction set.

## Architecture (currently)

- **Memory**: 64KB addressable memory space
- **Program Counter**: Starts at offset 0x200
- **Registers**: 8 general-purpose 32-bit registers (similar to x86):
    - R0 (EAX)
    - R1 (EBX)
    - R2 (ECX)
    - R3 (EDX)
    - R4 (ESI)
    - R5 (EDI)
    - R6 (EBP)
    - R7 (ESP)

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

