namespace Teto.Proc;

public enum Opcode : byte
{
    // --- Data Movement ---
    NOP  = 0x00,  // No operation
    MOV  = 0x01,  // Move data
    MOVHI = 0x02, // Move high 16 bits to register
    MOVLO = 0x03, // Move low 16 bits to register
    LD   = 0x04,  // Load data from memory
    LDHI = 0x05,  // Load high 16 bits from memory
    LDLO = 0x06,  // Load low 16 bits from memory
    ST   = 0x07,  // Store data to memory
    STHI = 0x08,  // Store high 16 bits to memory
    STLO = 0x09,  // Store low 16 bits to memory
    PUSH = 0x0A,  // Push to stack
    POP  = 0x0B,  // Pop from stack
    XCHG = 0x0C,  // Exchange values

    // --- Arithmetic (Integer) ---
    ADD  = 0x0D,  // Integer addition
    SUB  = 0x0E,  // Integer subtraction
    MUL  = 0x0F,  // Integer multiplication
    DIV  = 0x10,  // Integer division
    MOD  = 0x11,  // Integer modulo
    INC  = 0x12,  // Increment register
    DEC  = 0x13,  // Decrement register
    NEG  = 0x14,  // Negate (two's complement)

    // --- Arithmetic (Floating-Point) ---
    FADD = 0x15,  // Floating-point addition
    FSUB = 0x16,  // Floating-point subtraction
    FMUL = 0x17,  // Floating-point multiplication
    FDIV = 0x18,  // Floating-point division
    FMOVHI = 0x19, // Move high 16 bits of float to register
    FMOVLO = 0x1A, // Move low 16 bits of float to register
    ITOF = 0x1B,   // Convert integer to float
    FTOI = 0x1C,   // Convert float to integer

    // --- Bitwise Operations ---
    AND  = 0x1D,  // Bitwise AND
    OR   = 0x1E,  // Bitwise OR
    XOR  = 0x1F,  // Bitwise XOR
    NOT  = 0x20,  // Bitwise NOT
    SHL  = 0x21,  // Shift left
    SHR  = 0x22,  // Shift right
    ROL  = 0x23,  // Rotate left
    ROR  = 0x24,  // Rotate right
    TEST = 0x25,  // Bitwise test

    // --- Control Flow ---
    CMP    = 0x26,  // Compare two values
    JEQ    = 0x27,  // Jump if equal
    JNE    = 0x28,  // Jump if not equal
    JGT    = 0x29,  // Jump if greater
    JLT    = 0x2A,  // Jump if less
    JGE    = 0x2B,  // Jump if greater or equal
    JLE    = 0x2C,  // Jump if less or equal
    JMP    = 0x2D,  // Unconditional jump
    JMPREL = 0x2E,  // Relative jump
    JMPX   = 0x2F,  // Jump with absolute address
    CALL   = 0x30,  // Call subroutine
    RET    = 0x31,  // Return from subroutine

    // --- Stack Operations ---
    ENTER = 0x32,  // Set up stack frame
    LEAVE = 0x33,  // Tear down stack frame

    // --- System & Interrupts ---
    INT     = 0x34,  // Software interrupt
    IRET    = 0x35,  // Return from interrupt
    HLT     = 0x36,  // Halt execution
    SYSCALL = 0x37,  // System call
    CLI     = 0x38,  // Disable interrupts
    STI     = 0x39   // Enable interrupts
}