namespace Teto.MMU;

public static class Segments
{
    public const uint TextStart = 0x0000;
    public const uint TextEnd = 0x3FFF;
    
    public const uint DataStart = 0x4000;
    public const uint DataEnd = 0x7FFF;
    
    public const uint HeapStart = 0x8000;
    public const uint HeapEnd = 0xBFFF;
    
    public const uint StackStart = 0xC000;
    public const uint StackEnd = 0xFFFF;
}