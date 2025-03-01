using System;
using System.Collections.Generic;
using Terminal.Gui;

using Teto.MMU;
using Teto.Proc;

namespace Teto.Debugging;

public class Debugger
{
    private readonly CPU _cpu;
    private readonly Disassembler _disassembler;
    private readonly List<string> _disassemblyLines = [];
    private readonly List<string> _memoryLines = [];
    private readonly MemoryViewer _memoryViewer;
    private readonly RAM _ram;
    private readonly DebuggerUi _ui;
    private readonly Dictionary<int, int> _prevRegisterValues = new();

    public Debugger(CPU cpu, RAM ram)
    {
        _cpu = cpu;
        _ram = ram;
        _ui = new DebuggerUi(cpu);
        _disassembler = new Disassembler(ram);
        _memoryViewer = new MemoryViewer(ram);

        _ui.StepRequested += OnStepRequested;
        _ui.ResetRequested += OnResetRequested;
        _ui.CommandEntered += OnCommandEntered;
    }

    public void Start()
    {
        _ui.Initialize();
        RefreshAll();
        DebuggerUi.Start();
    }

    private void OnStepRequested()
    {
        if (!_cpu.Halted)
        {
            SaveRegisterValues();
            _cpu.Step();
            DetectChangedRegisters();
            _ui.AddToHistory(_cpu.LastInstruction);
            RefreshAll();
        }
        else
        {
            MessageBox.Query("CPU Halted", "The CPU is halted. Reset to continue.", "OK");
        }
    }
    
    private void SaveRegisterValues()
    {
        for (var i = 0; i < 8; i++)
        {
            _prevRegisterValues[i] = _cpu.GetRegister(i);
        }
    }
    
    private void DetectChangedRegisters()
    {
        _ui.ClearChangedRegisters();
        
        for (var i = 0; i < 8; i++)
        {
            var currentValue = _cpu.GetRegister(i);
            if (currentValue != _prevRegisterValues[i])
            {
                _ui.MarkRegisterChanged(i);
            }
        }
    }

    private void OnResetRequested()
    {
        if (MessageBox.Query("Reset CPU", "Are you sure you want to reset the CPU?", "Yes", "No") != 0) return;
        _cpu.Reset();
        _ram.Clear();
        _disassembler.ClearCache();
        RefreshAll();
    }
    
    private void OnCommandEntered(string command)
    {
        var parts = command.Trim().Split(' ');
        if (parts.Length == 0)
            return;
    
        var cmd = parts[0].ToLower();
    
        switch (cmd)
        {
            case "step":
            case "s":
                OnStepRequested();
                break;
    
            case "reset":
            case "r":
                OnResetRequested();
                break;
    
            case "dump":
                if (parts.Length < 2)
                {
                    MessageBox.ErrorQuery("Invalid Command", "Usage: dump <address> [length]", "OK");
                    break;
                }
                
                if (!TryParseAddress(parts[1], out var dumpAddr))
                {
                    MessageBox.ErrorQuery("Invalid Address", $"Invalid address format: {parts[1]}", "OK");
                    break;
                }
                
                uint dumpLen = 16;
                if (parts.Length > 2)
                {
                    if (!TryParseValue(parts[2], out var len))
                    {
                        MessageBox.ErrorQuery("Invalid Length", $"Invalid length format: {parts[2]}", "OK");
                        break;
                    }
                    
                    dumpLen = (uint)len;
                }
                
                try
                {
                    var memoryDump = _ram.Dump(dumpAddr, dumpLen);
                    MessageBox.Query("Memory Dump", memoryDump, "OK");
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery("Dump Error", ex.Message, "OK");
                }
                break;
    
            case "setreg":
                if (parts.Length < 3)
                {
                    MessageBox.ErrorQuery("Invalid Command", "Usage: setreg <register> <value>", "OK");
                    break;
                }
                
                var regIndex = ParseRegisterIndex(parts[1]);
                if (regIndex == -1)
                {
                    MessageBox.ErrorQuery("Invalid Register", $"Unknown register: {parts[1]}", "OK");
                    break;
                }
                
                if (!TryParseValue(parts[2], out var regValue))
                {
                    MessageBox.ErrorQuery("Invalid Value", $"Invalid value format: {parts[2]}", "OK");
                    break;
                }
                
                try
                {
                    _cpu.SetRegister(regIndex, regValue);
                    RefreshAll();
                    MessageBox.Query("Register Updated", $"Register {parts[1].ToUpper()} set to 0x{regValue:X8}", "OK");
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery("SetReg Error", ex.Message, "OK");
                }
                break;
    
            case "help":
                ShowHelp();
                break;
    
            case "exit":
            case "quit":
                Application.RequestStop();
                break;
    
            default:
                MessageBox.ErrorQuery("Unknown Command", $"Unknown command: {cmd}\nType 'help' for available commands.", "OK");
                break;
        }
    }

    private void RefreshAll()
    {
        UpdateCpuState();
        UpdateDisassembly();
        UpdateSegments();
        Application.Refresh();
    }

    private void UpdateCpuState()
    {
        _ui.UpdateCpuInfo();
        _ui.UpdateRegisters();
    }

    private void UpdateDisassembly()
    {
        _disassemblyLines.Clear();

        var startAddr = Math.Max(_cpu.PC - 16, Segments.TextStart);
        var endAddr = Math.Min(startAddr + 80, Segments.TextStart + 0x1000);

        for (var addr = startAddr; addr < endAddr; addr += 4)
        {
            var prefix = addr == _cpu.PC ? "→ " : "  ";
            var disasm = _disassembler.DisassembleInstruction(addr);
            _disassemblyLines.Add($"{prefix}0x{addr:X4}: {disasm}");
        }

        _ui.UpdateDisassembly(_disassemblyLines);
    }

    private void UpdateSegments()
    {
        _memoryLines.Clear();
        _memoryViewer.UpdateSegmentView(Segments.DataStart, 8, _memoryLines);
        _ui.UpdateMemorySegment(MemorySegmentType.DATA, _memoryLines);

        _memoryLines.Clear();
        _memoryViewer.UpdateSegmentView(Segments.HeapStart, 8, _memoryLines);
        _ui.UpdateMemorySegment(MemorySegmentType.HEAP, _memoryLines);

        _memoryLines.Clear();
        var stackPointer = _cpu.GetRegister(CPU.ESP);
        _memoryViewer.UpdateSegmentView(Segments.StackEnd, 8, _memoryLines, stackPointer, true);
        _ui.UpdateMemorySegment(MemorySegmentType.STACK, _memoryLines);
    }
    
    private static bool TryParseValue(string input, out int value)
    {
        value = 0;
        return input.StartsWith("0x", StringComparison.OrdinalIgnoreCase) 
            ? int.TryParse(input[2..], System.Globalization.NumberStyles.HexNumber, null, out value)
            : int.TryParse(input, out value);
    }
    
    private static bool TryParseAddress(string input, out uint address)
    {
        address = 0;
        return input.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? uint.TryParse(input[2..], System.Globalization.NumberStyles.HexNumber, null, out address)
            : uint.TryParse(input, out address);
    }
    
    private static int ParseRegisterIndex(string regName)
    {
        return regName.ToLower() switch
        {
            "eax" => CPU.EAX,
            "ebx" => CPU.EBX,
            "ecx" => CPU.ECX,
            "edx" => CPU.EDX,
            "esi" => CPU.ESI,
            "edi" => CPU.EDI,
            "ebp" => CPU.EBP,
            "esp" => CPU.ESP,
            _ => -1
        };
    }
    
    private static void ShowHelp()
    {
        const string helpText = "Available Commands:\n" +
                                "- step/s: Execute next instruction\n" +
                                "- reset/r: Reset CPU and memory\n" +
                                "- dump <addr> [length]: Dump memory contents\n" +
                                "- setreg <reg> <value>: Set register value\n" +
                                "- help: Show this help\n" +
                                "- exit/quit: Exit debugger\n\n" +
                                "Addresses and values can be decimal or hex (0x prefix).\n" +
                                "Registers: EAX, EBX, ECX, EDX, ESI, EDI, EBP, ESP";

        MessageBox.Query("Command Help", helpText, "OK");
    }
}