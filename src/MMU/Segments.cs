namespace Teto.MMU;

public static class Segments
{
    public const int TextStart = 0x0000;
    public const int TextEnd = 0x3FFF;
    
    public const int DataStart = 0x4000;
    public const int DataEnd = 0x7FFF;
    
    public const int HeapStart = 0x8000;
    public const int HeapEnd = 0xBFFF;
    
    public const int StackStart = 0xC000;
    public const int StackEnd = 0xFFFF;
}