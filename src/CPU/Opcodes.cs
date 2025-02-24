namespace Teto.CPU;

public enum Opcode
{
    // --- Data Movement ---
    NOP  = 0x00,  // No operation
    MOV  = 0x01,  // Move data
    LD   = 0x02,  // Load data from memory
    ST   = 0x03,  // Store data to memory
    PUSH = 0x04,  // Push to stack
    POP  = 0x05,  // Pop from stack
    XCHG = 0x06,  // Exchange values

    // --- Arithmetic (Integer) ---
    ADD  = 0x07,  // Integer addition
    SUB  = 0x08,  // Integer subtraction
    MUL  = 0x09,  // Integer multiplication
    DIV  = 0x0A,  // Integer division
    MOD  = 0x0B,  // Integer modulo
    INC  = 0x0C,  // Increment register
    DEC  = 0x0D,  // Decrement register
    NEG  = 0x0E,  // Negate (two's complement)

    // --- Arithmetic (Floating-Point) ---
    ADDF = 0x0F,  // Floating-point addition
    SUBF = 0x10,  // Floating-point subtraction
    MULF = 0x11,  // Floating-point multiplication
    DIVF = 0x12,  // Floating-point division

    // --- Bitwise Operations ---
    AND  = 0x13,  // Bitwise AND
    OR   = 0x14,  // Bitwise OR
    XOR  = 0x15,  // Bitwise XOR
    NOT  = 0x16,  // Bitwise NOT
    SHL  = 0x17,  // Shift left
    SHR  = 0x18,  // Shift right
    ROL  = 0x19,  // Rotate left
    ROR  = 0x1A,  // Rotate right
    TEST = 0x1B,  // Bitwise test

    // --- Control Flow ---
    CMP    = 0x1C,  // Compare two values
    JEQ    = 0x1D,  // Jump if equal
    JNE    = 0x1E,  // Jump if not equal
    JGT    = 0x1F,  // Jump if greater
    JLT    = 0x20,  // Jump if less
    JGE    = 0x21,  // Jump if greater or equal
    JLE    = 0x22,  // Jump if less or equal
    JMP    = 0x23,  // Unconditional jump
    JMPREL = 0x24,  // Relative jump
    JMPX   = 0x25,  // Jump with absolute address
    CALL   = 0x26,  // Call subroutine
    RET    = 0x27,  // Return from subroutine

    // --- Stack Operations ---
    ENTER = 0x28,  // Set up stack frame
    LEAVE = 0x29,  // Tear down stack frame

    // --- System & Interrupts ---
    INT     = 0x2A,  // Software interrupt
    IRET    = 0x2B,  // Return from interrupt
    HLT     = 0x2C,  // Halt execution
    SYSCALL = 0x2D,  // System call
    CLI     = 0x2E,  // Disable interrupts
    STI     = 0x2F   // Enable interrupts
}