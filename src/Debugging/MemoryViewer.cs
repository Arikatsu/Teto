using System.Collections.Generic;
using System.Text;

using Teto.MMU;

namespace Teto.Debugging;

public class MemoryViewer(RAM ram)
{
    public void UpdateSegmentView(uint startAddress, int rows, List<string> lines, int pcAddress = -1, bool isStack = false)
    {
        lines.Clear();

        // Align to 16-byte boundary
        uint baseAddress = startAddress & 0xFFF0;

        for (var row = 0; row < rows; row++)
        {
            uint rowAddress;
            
            // For stack segment, go from high to low addresses
            if (isStack)
            {
                rowAddress = baseAddress - (uint)(row * 16);
            }
            else
            {
                rowAddress = baseAddress + (uint)(row * 16);
            }
            
            var hexValues = new StringBuilder();
            var asciiValues = new StringBuilder();

            for (var col = 0; col < 16; col++)
            {
                uint currentAddress = rowAddress + (uint)col;
                byte value;

                try
                {
                    value = ram.Read(currentAddress);
                }
                catch
                {
                    value = 0;
                }

                hexValues.Append($"{value:X2} ");

                var asciiChar = value is >= 32 and <= 126 ? (char)value : '.';
                asciiValues.Append(asciiChar);
            }

            // Determine if this row contains the highlighted address
            bool isHighlighted = false;
            if (pcAddress >= 0)
            {
                uint pcAddressUint = (uint)pcAddress;
                isHighlighted = rowAddress <= pcAddressUint && pcAddressUint < rowAddress + 16;
            }
            
            var highlight = isHighlighted ? "* " : "  ";
            lines.Add($"{highlight}0x{rowAddress:X4}: {hexValues}| {asciiValues}");
        }
    }
}