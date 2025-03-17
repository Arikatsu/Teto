using System;
using System.Collections.Generic;
using Terminal.Gui;

using Teto.Proc;

using Attribute = Terminal.Gui.Attribute;

namespace Teto.Debugging;

public class DebuggerUi(CPU cpu)
{
    private readonly Label[] _cpuLabels = new Label[3];
    private readonly HashSet<int> _changedRegisters = [];
    private readonly List<string> _dataSegmentLines = [];
    private readonly List<string> _disassemblyLines = [];
    private readonly List<string> _heapSegmentLines = [];
    private readonly Label[] _registerFloatLabels = new Label[8];
    private readonly Label[] _registerLabels = new Label[8];
    private readonly List<string> _stackSegmentLines = [];
    private readonly List<string> _historyLines = [];
    private TextField? _cliInput;
    private ListView? _historyListView;
    private FrameView? _historyFrame;
    private FrameView? _cliFrame;
    private FrameView? _cpuFrame;
    private FrameView? _dataSegmentFrame;
    private ListView _dataSegmentListView = new();
    private FrameView? _disassemblyFrame;
    private ListView _disassemblyListView = new();
    private FrameView? _heapSegmentFrame;
    private ListView _heapSegmentListView = new();
    private Window? _mainWindow;
    private FrameView? _registersFrame;
    private FrameView? _stackSegmentFrame;
    private ListView _stackSegmentListView = new();

    public event Action? StepRequested;
    public event Action? ResetRequested;
    public event Action<string>? CommandEntered;
    
    public static void Start()
    {
        Application.Run();
    }
    
    public void Initialize()
    {
        Application.Init();

        Colors.Base.Normal = new Attribute(Color.White, Color.Black);
        Colors.Base.Focus = new Attribute(Color.Black, Color.Gray);
        Colors.Base.HotNormal = new Attribute(Color.Blue, Color.Black);
        Colors.Base.HotFocus = new Attribute(Color.Black, Color.Blue);

        _mainWindow = new Window("Teto Debugger")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Border = new Border
            {
                BorderStyle = BorderStyle.Double, 
                BorderBrush = Color.Blue,
                Title = "Teto Debugger"
            }
        };

        CreateLeftPanel();
        CreateRightPanel();
        CreateCliPanel();

        Application.Top.Add(_mainWindow);

        _mainWindow.KeyPress += OnKeyPress;
    }

    public void UpdateCpuInfo()
    {
        _cpuLabels[0].Text = $"PC      : 0x{cpu.PC:X4}";
        _cpuLabels[1].Text = $"Flags   : 0x{cpu.Flags:X8}";
        _cpuLabels[2].Text = $"Halted  : {cpu.Halted}";
    }

    public void UpdateRegisters()
    {
        for (var i = 0; i < 8; i++)
        {
            var currentValue = cpu.GetRegister(i);
            var valueText = $"0x{currentValue:X8}";
            
            var regColor = _changedRegisters.Contains(i) ? Color.Green : Color.White;
            
            switch (i)
            {
                case 0:
                    _registerLabels[0].Text = $"EAX: {valueText}";
                    _registerLabels[0].ColorScheme = new ColorScheme { Normal = new Attribute(regColor, Color.Black) };
                    break;
                case 1:
                    _registerLabels[1].Text = $"EBX: {valueText}";
                    _registerLabels[1].ColorScheme = new ColorScheme { Normal = new Attribute(regColor, Color.Black) };
                    break;
                case 2:
                    _registerLabels[2].Text = $"ECX: {valueText}";
                    _registerLabels[2].ColorScheme = new ColorScheme { Normal = new Attribute(regColor, Color.Black) };
                    break;
                case 3:
                    _registerLabels[3].Text = $"EDX: {valueText}";
                    _registerLabels[3].ColorScheme = new ColorScheme { Normal = new Attribute(regColor, Color.Black) };
                    break;
                case 4:
                    _registerLabels[4].Text = $"ESI: {valueText}";
                    _registerLabels[4].ColorScheme = new ColorScheme { Normal = new Attribute(regColor, Color.Black) };
                    break;
                case 5:
                    _registerLabels[5].Text = $"EDI: {valueText}";
                    _registerLabels[5].ColorScheme = new ColorScheme { Normal = new Attribute(regColor, Color.Black) };
                    break;
                case 6:
                    _registerLabels[6].Text = $"EBP: {valueText}";
                    _registerLabels[6].ColorScheme = new ColorScheme { Normal = new Attribute(regColor, Color.Black) };
                    break;
                case 7:
                    _registerLabels[7].Text = $"ESP: {valueText}";
                    _registerLabels[7].ColorScheme = new ColorScheme { Normal = new Attribute(regColor, Color.Black) };
                    break;
            }
        }

        for (var i = 0; i < 8; i++)
        {
            _registerFloatLabels[i].Text = $"F.{Disassembler.GetRegisterName(i).ToUpper()}: {cpu.GetRegisterF(i)}";
        }
    }

    public void MarkRegisterChanged(int regId)
    {
        _changedRegisters.Add(regId);
    }
    
    public void ClearChangedRegisters()
    {
        _changedRegisters.Clear();
    }

    public void UpdateDisassembly(List<string> lines)
    {
        _disassemblyLines.Clear();
        _disassemblyLines.AddRange(lines);
        _disassemblyListView.SetSource(_disassemblyLines);

        for (var i = 0; i < _disassemblyLines.Count; i++)
        {
            if (!_disassemblyLines[i].StartsWith('→')) continue;
            
            _disassemblyListView.SelectedItem = i;
            break;
        }
    }
    
    public void AddToHistory(string instruction)
    {
        if (_historyLines.Count >= 20)
            _historyLines.RemoveAt(0);
            
        _historyLines.Add(instruction);
        _historyListView?.SetSource(_historyLines);
    }
    
    public void ClearHistory()
    {
        _historyLines.Clear();
        _historyListView?.SetSource(_historyLines);
    }

    public void UpdateMemorySegment(MemorySegmentType segmentType, List<string> lines)
    {
        ListView listView;
        List<string> targetList;

        switch (segmentType)
        {
            case MemorySegmentType.DATA:
                listView = _dataSegmentListView;
                targetList = _dataSegmentLines;
                break;
            case MemorySegmentType.HEAP:
                listView = _heapSegmentListView;
                targetList = _heapSegmentLines;
                break;
            case MemorySegmentType.STACK:
                listView = _stackSegmentListView;
                targetList = _stackSegmentLines;
                break;
            default:
                throw new ArgumentException("Invalid segment type");
        }

        targetList.Clear();
        targetList.AddRange(lines);
        listView.SetSource(targetList);
    }

    private void OnKeyPress(View.KeyEventEventArgs args)
    {
        switch (args.KeyEvent.Key)
        {
            case Key.Enter:
                if (_cliInput != null && !_cliInput.HasFocus)
                {
                    StepRequested?.Invoke();
                    args.Handled = true;
                }
                break;
            case Key.R:
            case Key.r:
                if (_cliInput != null && !_cliInput.HasFocus)
                {
                    ResetRequested?.Invoke();
                    args.Handled = true;
                }
                break;
            case Key.Esc:
                Application.RequestStop();
                args.Handled = true;
                break;
        }
    }

    private void CreateLeftPanel()
    {
        _cpuFrame = new FrameView("CPU State")
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(50),
            Height = 5
        };

        _cpuLabels[0] = new Label(1, 0, "PC      : 0x0000");
        _cpuLabels[1] = new Label(1, 1, "Flags   : 0x0000");
        _cpuLabels[2] = new Label(1, 2, "Halted  : False");

        foreach (var label in _cpuLabels) _cpuFrame.Add(label);

        _registersFrame = new FrameView("Registers")
        {
            X = 0,
            Y = 5,
            Width = Dim.Percent(50),
            Height = 11
        };

        _registerLabels[0] = new Label(1, 0, "EAX: 0x00000000");
        _registerLabels[1] = new Label(1, 1, "EBX: 0x00000000");
        _registerLabels[2] = new Label(1, 2, "ECX: 0x00000000");
        _registerLabels[3] = new Label(1, 3, "EDX: 0x00000000");
        _registerLabels[4] = new Label(20, 0, "ESI: 0x00000000");
        _registerLabels[5] = new Label(20, 1, "EDI: 0x00000000");
        _registerLabels[6] = new Label(20, 2, "EBP: 0x00000000");
        _registerLabels[7] = new Label(20, 3, "ESP: 0x00000000");

        _registerFloatLabels[0] = new Label(1, 5, "F.EAX: 0.0");
        _registerFloatLabels[1] = new Label(1, 6, "F.EBX: 0.0");
        _registerFloatLabels[2] = new Label(1, 7, "F.ECX: 0.0");
        _registerFloatLabels[3] = new Label(1, 8, "F.EDX: 0.0");
        _registerFloatLabels[4] = new Label(20, 5, "F.ESI: 0.0");
        _registerFloatLabels[5] = new Label(20, 6, "F.EDI: 0.0");
        _registerFloatLabels[6] = new Label(20, 7, "F.EBP: 0.0");
        _registerFloatLabels[7] = new Label(20, 8, "F.ESP: 0.0");

        foreach (var label in _registerLabels) _registersFrame.Add(label);
        foreach (var label in _registerFloatLabels) _registersFrame.Add(label);

        _historyFrame = new FrameView("Instruction History")
        {
            X = Pos.Percent(25),
            Y = 16,
            Width = Dim.Percent(25),
            Height = Dim.Fill() - 3
        };
        
        _historyListView = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            AllowsMarking = false
        };
        _historyFrame.Add(_historyListView);

        _disassemblyFrame = new FrameView("Disassembly")
        {
            X = 0,
            Y = 16,
            Width = Dim.Percent(25),
            Height = Dim.Fill() - 3
        };

        _disassemblyListView = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            AllowsMarking = false
        };
        _disassemblyFrame.Add(_disassemblyListView);

        _mainWindow?.Add(_cpuFrame, _registersFrame, _disassemblyFrame, _historyFrame);
    }

    private void CreateRightPanel()
    {
        _dataSegmentFrame = new FrameView("Data Segment")
        {
            X = Pos.Percent(50),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Percent(25)
        };

        _dataSegmentListView = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        _dataSegmentFrame.Add(_dataSegmentListView);

        _heapSegmentFrame = new FrameView("Heap Segment")
        {
            X = Pos.Percent(50),
            Y = Pos.Percent(25),
            Width = Dim.Fill(),
            Height = Dim.Percent(25) 
        };

        _heapSegmentListView = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        _heapSegmentFrame.Add(_heapSegmentListView);

        _stackSegmentFrame = new FrameView("Stack Segment")
        {
            X = Pos.Percent(50),
            Y = Pos.Percent(50),
            Width = Dim.Fill(),
            Height = Dim.Fill() - 3
        };

        _stackSegmentListView = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        _stackSegmentFrame.Add(_stackSegmentListView);

        _mainWindow?.Add(_dataSegmentFrame, _heapSegmentFrame, _stackSegmentFrame);
    }
    
    private void CreateCliPanel()
    {
        _cliFrame = new FrameView("Command Line Interface")
        {
            X = 0,
            Y = Pos.AnchorEnd(3),
            Width = Dim.Fill(),
            Height = 3,
            ColorScheme = new ColorScheme { Normal = new Attribute(Color.White, Color.Black) },
            Border = new Border
            {
                BorderStyle = BorderStyle.Single,
                BorderBrush = Color.Green,
                Title = "Command Line Interface"
            }
        };
        
        _cliInput = new TextField("")
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill() - 1,
            Height = 1
        };
        
        _cliInput.KeyPress += (args) =>
        {
            if (args.KeyEvent.Key != Key.Enter) return;
            
            var command = _cliInput.Text.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(command)) return;
            
            CommandEntered?.Invoke(command);
            _cliInput.Text = "";
            args.Handled = true;
        };
        
        _cliFrame.Add(_cliInput);
        _mainWindow?.Add(_cliFrame);
    }
}

public enum MemorySegmentType
{
    TEXT,
    DATA,
    HEAP,
    STACK
}