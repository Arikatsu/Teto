using System.Collections.Generic;
using System.Text;

using Teto.MMU;

namespace Teto.Debugging;

public class MemoryViewer(RAM ram)
{
    public void UpdateSegmentView(uint startAddress, int rows, List<string> lines, bool isStack = false)
    {
        lines.Clear();

        var baseAddress = startAddress & 0xFFFF;

        for (var row = 0; row < rows; row++)
        {
            uint rowAddress;
            
            if (isStack) rowAddress = baseAddress - (uint)(row * 16);
            else rowAddress = baseAddress + (uint)(row * 16);
            
            var hexValues = new StringBuilder();
            var asciiValues = new StringBuilder();

            for (var col = 0; col < 16; col++)
            {
                var currentAddress = isStack ? rowAddress - (uint)col : rowAddress + (uint)col;
                
                byte value;

                try { value = ram.Read(currentAddress); }
                catch { value = 0; }

                hexValues.Append($"{value:X2} ");

                var asciiChar = value is >= 32 and <= 126 ? (char)value : '.';
                asciiValues.Append(asciiChar);
            }
            
            lines.Add($"0x{rowAddress:X4}: {hexValues}| {asciiValues}");
        }
    }
}