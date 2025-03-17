namespace Teto.Proc;

public enum InstrMode
{
    IMM = 0,        // Immediate
    MEM = 1,        // Memory
    REG = 2,        // Register
    REL_HEAP = 3,   // Relative to heap base address
    REL_EAX = 4,    // Relative to EAX
    REL_EBX = 5,    // Relative to EBX
    REL_ECX = 6,    // Relative to ECX
    REL_EDX = 7,    // Relative to EDX
    REL_ESI = 8,    // Relative to ESI
    REL_EDI = 9,    // Relative to EDI
    REL_EBP = 10,   // Relative to EBP
    REL_ESP = 11,   // Relative to ESP
}