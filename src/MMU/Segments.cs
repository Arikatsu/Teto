namespace Teto.MMU;

public enum Segments : uint
{
    TEXT_START = 0x0000,
    TEXT_END = 0x3FFF,
    
    DATA_START = 0x4000,
    DATA_END = 0x7FFF,
    
    HEAP_START = 0x8000,
    HEAP_END = 0xBFFF,
    
    STACK_START = 0xC000,
    STACK_END = 0xFFFF
}