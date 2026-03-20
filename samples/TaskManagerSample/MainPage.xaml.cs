using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace TaskManagerSample;

public partial class MainPage : Page
{
    private readonly DispatcherTimer _timer;
    private readonly Random _rng = new();

    public ObservableCollection<ProcessItem> Processes { get; } = [];

    public MainPage()
    {
        this.InitializeComponent();

        PopulateProcesses();
        UpdateStatusBar();
        UpdateSummary();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnThemeToggled(object sender, RoutedEventArgs e)
    {
        RequestedTheme = ThemeToggle.IsOn ? ElementTheme.Dark : ElementTheme.Light;
    }

    private void OnTimerTick(object? sender, object e)
    {
        foreach (var process in Processes)
        {
            RandomizeValues(process);
            if (process.Children is { Count: > 0 })
            {
                foreach (var child in process.Children)
                {
                    RandomizeValues(child);
                }
                // Parent aggregates from children
                AggregateFromChildren(process);
            }
        }

        UpdateSummary();
    }

    private void RandomizeValues(ProcessItem p)
    {
        // CPU: mostly low, occasional spikes
        var cpuBase = p.IsHighCpu ? _rng.NextDouble() * 15 + 5 : _rng.NextDouble() * 3;
        p.CpuPercent = Math.Round(cpuBase, 1);

        // Memory: drift slightly around baseline
        var memDrift = (_rng.NextDouble() - 0.5) * p.MemoryBaseline * 0.08;
        p.MemoryMB = Math.Max(0.5, Math.Round(p.MemoryBaseline + memDrift, 1));

        // Disk
        var diskBase = p.IsHighDisk ? _rng.NextDouble() * 2 : _rng.NextDouble() * 0.3;
        p.DiskMBps = Math.Round(diskBase, 1);

        // Network
        var netBase = p.IsHighNetwork ? _rng.NextDouble() * 1.5 : _rng.NextDouble() * 0.2;
        p.NetworkMbps = Math.Round(netBase, 1);
    }

    private void AggregateFromChildren(ProcessItem parent)
    {
        if (parent.Children is not { Count: > 0 }) return;

        double cpu = 0, mem = 0, disk = 0, net = 0;
        foreach (var c in parent.Children)
        {
            cpu += c.CpuPercent;
            mem += c.MemoryMB;
            disk += c.DiskMBps;
            net += c.NetworkMbps;
        }

        parent.CpuPercent = Math.Round(cpu, 1);
        parent.MemoryMB = Math.Round(mem, 1);
        parent.DiskMBps = Math.Round(disk, 1);
        parent.NetworkMbps = Math.Round(net, 1);
    }

    private void UpdateSummary()
    {
        double totalCpu = 0, totalMem = 0, totalDisk = 0, totalNet = 0;
        int count = 0;

        foreach (var p in Processes)
        {
            totalCpu += p.CpuPercent;
            totalMem += p.MemoryMB;
            totalDisk += p.DiskMBps;
            totalNet += p.NetworkMbps;
            count++;
        }

        CpuSummary.Text = $"{Math.Min(totalCpu, 100):F0}%";
        MemorySummary.Text = $"{totalMem / 1024 / 16 * 100:F0}%"; // Simulate % of 16GB
        DiskSummary.Text = $"{Math.Min(totalDisk, 100):F0}%";
        NetworkSummary.Text = $"{Math.Min(totalNet, 100):F0}%";
    }

    private void UpdateStatusBar()
    {
        int total = 0;
        foreach (var p in Processes)
        {
            total++;
            if (p.Children is { Count: > 0 })
                total += p.Children.Count;
        }
        StatusBar.Text = $"Processes: {Processes.Count}   Threads: {total * 12}   Handles: {total * 347}";
    }

    private void OnEndTaskClick(object sender, RoutedEventArgs e)
    {
        if (ProcessTable.SelectedItem is ProcessItem selected)
        {
            // Remove from parent collection
            foreach (var p in Processes)
            {
                if (p.Children?.Remove(selected) == true)
                {
                    UpdateStatusBar();
                    return;
                }
            }
            Processes.Remove(selected);
            UpdateStatusBar();
        }
    }

    private void PopulateProcesses()
    {
        // ── Apps ──
        Processes.Add(new ProcessItem
        {
            Name = "Calendar",
            Category = "Apps",
            IconGlyph = "\uE787",
            MemoryBaseline = 28.4,
            IsExpanded = true,
            Children =
            [
                new() { Name = "Calendar", Category = "Apps", IconGlyph = "\uE787", MemoryBaseline = 10.7 },
                new() { Name = "Crashpad", Category = "Apps", IconGlyph = "\uE7BA", MemoryBaseline = 1.3 },
                new() { Name = "WebView2 GPU Process", Category = "Apps", IconGlyph = "\uE943", MemoryBaseline = 1.7 },
                new() { Name = "WebView2 Manager", Category = "Apps", IconGlyph = "\uE912", MemoryBaseline = 10.9 },
                new() { Name = "WebView2 Utility: Network Service", Category = "Apps", IconGlyph = "\uE968", MemoryBaseline = 2.4 },
                new() { Name = "WebView2 Utility: Storage Service", Category = "Apps", IconGlyph = "\uEDA2", MemoryBaseline = 1.4 },
                new() { Name = "WebView2: Calendar in Taskbar", Category = "Apps", IconGlyph = "\uE774", MemoryBaseline = 0.8, StatusText = "Efficiency …", IsEfficiency = true },
            ]
        });

        Processes.Add(new ProcessItem
        {
            Name = "Files",
            Category = "Apps",
            IconGlyph = "\uE8B7",
            MemoryBaseline = 34.6,
            IsExpanded = false,
            Children =
            [
                new() { Name = "Crashpad", Category = "Apps", IconGlyph = "\uE7BA", MemoryBaseline = 1.5 },
                new() { Name = "Files", Category = "Apps", IconGlyph = "\uE8B7", MemoryBaseline = 13.0 },
                new() { Name = "WebView2 GPU Process", Category = "Apps", IconGlyph = "\uE943", MemoryBaseline = 1.7 },
                new() { Name = "WebView2 Manager", Category = "Apps", IconGlyph = "\uE912", MemoryBaseline = 12.7 },
                new() { Name = "WebView2 Utility: Network Service", Category = "Apps", IconGlyph = "\uE968", MemoryBaseline = 3.9 },
                new() { Name = "WebView2 Utility: Storage Service", Category = "Apps", IconGlyph = "\uEDA2", MemoryBaseline = 1.8 },
                new() { Name = "WebView2: Files Search App", Category = "Apps", IconGlyph = "\uE774", MemoryBaseline = 0.5, StatusText = "Efficiency …", IsEfficiency = true },
            ]
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Edge",
            Category = "Apps",
            IconGlyph = "\uE774",
            MemoryBaseline = 2371.1,
            IsHighCpu = true,
            IsHighNetwork = true,
            IsExpanded = false,
            Children =
            [
                new() { Name = "Browser", Category = "Apps", IconGlyph = "\uE774", MemoryBaseline = 320, IsHighCpu = true },
                new() { Name = "GPU Process", Category = "Apps", IconGlyph = "\uE943", MemoryBaseline = 198 },
                new() { Name = "Utility: Audio Service", Category = "Apps", IconGlyph = "\uE8D6", MemoryBaseline = 42 },
                new() { Name = "Utility: Network Service", Category = "Apps", IconGlyph = "\uE968", MemoryBaseline = 56, IsHighNetwork = true },
                new() { Name = "Utility: Storage Service", Category = "Apps", IconGlyph = "\uEDA2", MemoryBaseline = 28 },
                new() { Name = "Extension: uBlock Origin", Category = "Apps", IconGlyph = "\uE71B", MemoryBaseline = 45 },
                new() { Name = "Tab: GitHub", Category = "Apps", IconGlyph = "\uE8A7", MemoryBaseline = 180 },
                new() { Name = "Tab: Stack Overflow", Category = "Apps", IconGlyph = "\uE8A7", MemoryBaseline = 155 },
                new() { Name = "Tab: YouTube", Category = "Apps", IconGlyph = "\uE8A7", MemoryBaseline = 340, IsHighCpu = true },
                new() { Name = "Tab: Microsoft Learn", Category = "Apps", IconGlyph = "\uE8A7", MemoryBaseline = 120 },
                new() { Name = "Tab: Outlook", Category = "Apps", IconGlyph = "\uE8A7", MemoryBaseline = 205 },
                new() { Name = "Tab: Twitter", Category = "Apps", IconGlyph = "\uE8A7", MemoryBaseline = 175 },
                new() { Name = "Tab: Azure Portal", Category = "Apps", IconGlyph = "\uE8A7", MemoryBaseline = 260, IsHighCpu = true },
                new() { Name = "Service Worker", Category = "Apps", IconGlyph = "\uE713", MemoryBaseline = 35 },
                new() { Name = "Crashpad Handler", Category = "Apps", IconGlyph = "\uE7BA", MemoryBaseline = 2.4 },
            ]
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Excel",
            Category = "Apps",
            IconGlyph = "\uE9F9",
            MemoryBaseline = 246.8,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft OneNote",
            Category = "Apps",
            IconGlyph = "\uE70B",
            MemoryBaseline = 166.9,
            IsHighNetwork = true,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Teams",
            Category = "Apps",
            IconGlyph = "\uE902",
            MemoryBaseline = 264.8,
            IsHighCpu = true,
            IsHighNetwork = true,
            IsExpanded = false,
            StatusText = "Efficiency …",
            IsEfficiency = true,
            Children =
            [
                new() { Name = "Teams Main", Category = "Apps", IconGlyph = "\uE902", MemoryBaseline = 180, IsHighCpu = true },
                new() { Name = "Teams GPU", Category = "Apps", IconGlyph = "\uE943", MemoryBaseline = 32 },
                new() { Name = "Teams Utility", Category = "Apps", IconGlyph = "\uE713", MemoryBaseline = 18 },
                new() { Name = "Teams Media", Category = "Apps", IconGlyph = "\uE8D6", MemoryBaseline = 34.8, IsHighNetwork = true },
            ]
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Visual Studio 2022",
            Category = "Apps",
            IconGlyph = "\uE7C3",
            MemoryBaseline = 642.8,
            IsHighCpu = true,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Visual Studio 2022",
            Category = "Apps",
            IconGlyph = "\uE7C3",
            MemoryBaseline = 1159.1,
            IsHighCpu = true,
            IsHighDisk = true,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Visual Studio 2022",
            Category = "Apps",
            IconGlyph = "\uE7C3",
            MemoryBaseline = 962.4,
            IsHighCpu = true,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Visual Studio 2022",
            Category = "Apps",
            IconGlyph = "\uE7C3",
            MemoryBaseline = 1301.1,
            IsHighCpu = true,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Word",
            Category = "Apps",
            IconGlyph = "\uE8D2",
            MemoryBaseline = 360.3,
            IsHighCpu = true,
        });

        Processes.Add(new ProcessItem
        {
            Name = "MUXControlsTestApp",
            Category = "Apps",
            IconGlyph = "\uE737",
            MemoryBaseline = 170.1,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Notepad.exe",
            Category = "Apps",
            IconGlyph = "\uE70F",
            MemoryBaseline = 57.3,
            IsExpanded = false,
            Children =
            [
                new() { Name = "Notepad", Category = "Apps", IconGlyph = "\uE70F", MemoryBaseline = 32 },
                new() { Name = "Notepad", Category = "Apps", IconGlyph = "\uE70F", MemoryBaseline = 25.3 },
            ]
        });

        Processes.Add(new ProcessItem
        {
            Name = "Outlook",
            Category = "Apps",
            IconGlyph = "\uE715",
            MemoryBaseline = 456.4,
            IsHighCpu = true,
            IsHighNetwork = true,
            IsExpanded = false,
            Children =
            [
                new() { Name = "Outlook", Category = "Apps", IconGlyph = "\uE715", MemoryBaseline = 220 },
                new() { Name = "Crashpad", Category = "Apps", IconGlyph = "\uE7BA", MemoryBaseline = 1.8 },
                new() { Name = "GPU Process", Category = "Apps", IconGlyph = "\uE943", MemoryBaseline = 45 },
                new() { Name = "Utility: Network Service", Category = "Apps", IconGlyph = "\uE968", MemoryBaseline = 32, IsHighNetwork = true },
                new() { Name = "Utility: Storage Service", Category = "Apps", IconGlyph = "\uEDA2", MemoryBaseline = 18 },
                new() { Name = "Service Worker", Category = "Apps", IconGlyph = "\uE713", MemoryBaseline = 14 },
                new() { Name = "Renderer: Mail", Category = "Apps", IconGlyph = "\uE8A7", MemoryBaseline = 68 },
                new() { Name = "Renderer: Calendar", Category = "Apps", IconGlyph = "\uE8A7", MemoryBaseline = 42 },
                new() { Name = "Manager", Category = "Apps", IconGlyph = "\uE912", MemoryBaseline = 15.6 },
            ]
        });

        Processes.Add(new ProcessItem
        {
            Name = "Task Manager",
            Category = "Apps",
            IconGlyph = "\uE9D9",
            MemoryBaseline = 38.2,
            IsHighCpu = true,
        });

        // ── Background processes ──
        Processes.Add(new ProcessItem
        {
            Name = "Antimalware Service Executable",
            Category = "Background processes",
            IconGlyph = "\uE74D",
            MemoryBaseline = 210.8,
            IsHighCpu = true,
            IsHighDisk = true,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Application Frame Host",
            Category = "Background processes",
            IconGlyph = "\uE737",
            MemoryBaseline = 14.2,
        });

        Processes.Add(new ProcessItem
        {
            Name = "COM Surrogate",
            Category = "Background processes",
            IconGlyph = "\uE7BA",
            MemoryBaseline = 3.1,
        });

        Processes.Add(new ProcessItem
        {
            Name = "COM Surrogate",
            Category = "Background processes",
            IconGlyph = "\uE7BA",
            MemoryBaseline = 2.8,
        });

        Processes.Add(new ProcessItem
        {
            Name = "CTF Loader",
            Category = "Background processes",
            IconGlyph = "\uE7BA",
            MemoryBaseline = 5.4,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Desktop Window Manager",
            Category = "Background processes",
            IconGlyph = "\uE7BA",
            MemoryBaseline = 124.6,
            IsHighCpu = true,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Device Census",
            Category = "Background processes",
            IconGlyph = "\uE7BA",
            MemoryBaseline = 6.2,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Host Process for Windows Tasks",
            Category = "Background processes",
            IconGlyph = "\uE7BA",
            MemoryBaseline = 8.8,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Distributed Transaction Coordinator",
            Category = "Background processes",
            IconGlyph = "\uE7BA",
            MemoryBaseline = 4.1,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Text Input Application",
            Category = "Background processes",
            IconGlyph = "\uE765",
            MemoryBaseline = 42.3,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Registry",
            Category = "Background processes",
            IconGlyph = "\uE74C",
            MemoryBaseline = 58.0,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Runtime Broker",
            Category = "Background processes",
            IconGlyph = "\uE7BA",
            MemoryBaseline = 22.5,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Runtime Broker",
            Category = "Background processes",
            IconGlyph = "\uE7BA",
            MemoryBaseline = 15.8,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Search Host",
            Category = "Background processes",
            IconGlyph = "\uE721",
            MemoryBaseline = 92.3,
            IsHighCpu = true,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Security Health Service",
            Category = "Background processes",
            IconGlyph = "\uE74D",
            MemoryBaseline = 7.6,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Shell Infrastructure Host",
            Category = "Background processes",
            IconGlyph = "\uE7BA",
            MemoryBaseline = 18.4,
        });

        Processes.Add(new ProcessItem
        {
            Name = "StartMenuExperienceHost.exe",
            Category = "Background processes",
            IconGlyph = "\uE700",
            MemoryBaseline = 56.7,
        });

        Processes.Add(new ProcessItem
        {
            Name = "System",
            Category = "Background processes",
            IconGlyph = "\uE770",
            MemoryBaseline = 0.1,
        });

        Processes.Add(new ProcessItem
        {
            Name = "System Idle Process",
            Category = "Background processes",
            IconGlyph = "\uE770",
            MemoryBaseline = 0.0,
        });

        Processes.Add(new ProcessItem
        {
            Name = "TextInputHost.exe",
            Category = "Background processes",
            IconGlyph = "\uE765",
            MemoryBaseline = 68.9,
        });

        Processes.Add(new ProcessItem
        {
            Name = "User Manager",
            Category = "Background processes",
            IconGlyph = "\uE7BA",
            MemoryBaseline = 5.2,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Widget Platform",
            Category = "Background processes",
            IconGlyph = "\uE71D",
            MemoryBaseline = 45.1,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Windows Security",
            Category = "Background processes",
            IconGlyph = "\uE74D",
            MemoryBaseline = 0.6,
        });

        // ── Windows processes ──
        Processes.Add(new ProcessItem
        {
            Name = "Client Server Runtime Process",
            Category = "Windows processes",
            IconGlyph = "\uE770",
            MemoryBaseline = 2.1,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Client Server Runtime Process",
            Category = "Windows processes",
            IconGlyph = "\uE770",
            MemoryBaseline = 1.8,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Local Security Authority Process",
            Category = "Windows processes",
            IconGlyph = "\uE72E",
            MemoryBaseline = 18.4,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Service Host: Local System",
            Category = "Windows processes",
            IconGlyph = "\uE770",
            MemoryBaseline = 12.3,
            IsExpanded = false,
            Children =
            [
                new() { Name = "Background Intelligent Transfer Service", Category = "Windows processes", IconGlyph = "\uE770", MemoryBaseline = 2.1 },
                new() { Name = "Cryptographic Services", Category = "Windows processes", IconGlyph = "\uE72E", MemoryBaseline = 4.6 },
                new() { Name = "Windows Update", Category = "Windows processes", IconGlyph = "\uE895", MemoryBaseline = 5.6 },
            ]
        });

        Processes.Add(new ProcessItem
        {
            Name = "Service Host: Network Service",
            Category = "Windows processes",
            IconGlyph = "\uE770",
            MemoryBaseline = 8.7,
            IsExpanded = false,
            Children =
            [
                new() { Name = "DNS Client", Category = "Windows processes", IconGlyph = "\uE968", MemoryBaseline = 3.2 },
                new() { Name = "Network List Service", Category = "Windows processes", IconGlyph = "\uE968", MemoryBaseline = 5.5 },
            ]
        });

        Processes.Add(new ProcessItem
        {
            Name = "Services",
            Category = "Windows processes",
            IconGlyph = "\uE770",
            MemoryBaseline = 9.4,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Windows Logon Application",
            Category = "Windows processes",
            IconGlyph = "\uE770",
            MemoryBaseline = 4.2,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Windows Start",
            Category = "Windows processes",
            IconGlyph = "\uE700",
            MemoryBaseline = 32.4,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Windows Explorer",
            Category = "Windows processes",
            IconGlyph = "\uE8B7",
            MemoryBaseline = 112.5,
            IsHighCpu = true,
        });
    }
}

/// <summary>
/// Represents a process or sub-process row displayed in the Task Manager table.
/// All numeric stats fire PropertyChanged so the TableView cells update in real time.
/// </summary>
public sealed class ProcessItem : INotifyPropertyChanged
{
    // ── Static brushes (shared across all items) ──
    private static readonly SolidColorBrush TransparentBrush = new(Colors.Transparent);
    private static readonly SolidColorBrush LowBrush = new(Color.FromArgb(30, 255, 185, 60));     // faint amber
    private static readonly SolidColorBrush MedBrush = new(Color.FromArgb(60, 255, 150, 30));      // amber
    private static readonly SolidColorBrush HighBrush = new(Color.FromArgb(90, 255, 100, 20));     // orange
    private static readonly SolidColorBrush VeryHighBrush = new(Color.FromArgb(120, 230, 60, 20)); // red-orange

    // ── Identity ──
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = "\uE7BA";
    public string IconPath { get; set; } = string.Empty;

    // ── Visibility helpers for icon/glyph ──
    public Visibility HasIcon => string.IsNullOrEmpty(IconPath) ? Visibility.Collapsed : Visibility.Visible;
    public Visibility HasGlyph => string.IsNullOrEmpty(IconPath) ? Visibility.Visible : Visibility.Collapsed;

    // ── Status ──
    public string StatusText { get; set; } = string.Empty;
    public bool IsEfficiency { get; set; }
    public Visibility HasStatusText => string.IsNullOrEmpty(StatusText) ? Visibility.Collapsed : Visibility.Visible;
    public Visibility IsEfficiencyMode => IsEfficiency ? Visibility.Visible : Visibility.Collapsed;

    // ── Hierarchy ──
    public bool IsExpanded { get; set; }
    public List<ProcessItem> Children { get; set; } = [];

    // ── Behavioral hints (for simulation) ──
    public bool IsHighCpu { get; set; }
    public bool IsHighDisk { get; set; }
    public bool IsHighNetwork { get; set; }
    public double MemoryBaseline { get; set; } = 10;

    // ── Live stats ──
    private double _cpuPercent;
    public double CpuPercent
    {
        get => _cpuPercent;
        set { if (SetField(ref _cpuPercent, value)) { OnPropertyChanged(nameof(CpuDisplay)); OnPropertyChanged(nameof(CpuBackground)); } }
    }

    private double _memoryMB;
    public double MemoryMB
    {
        get => _memoryMB;
        set { if (SetField(ref _memoryMB, value)) { OnPropertyChanged(nameof(MemoryDisplay)); OnPropertyChanged(nameof(MemoryBackground)); } }
    }

    private double _diskMBps;
    public double DiskMBps
    {
        get => _diskMBps;
        set { if (SetField(ref _diskMBps, value)) { OnPropertyChanged(nameof(DiskDisplay)); OnPropertyChanged(nameof(DiskBackground)); } }
    }

    private double _networkMbps;
    public double NetworkMbps
    {
        get => _networkMbps;
        set { if (SetField(ref _networkMbps, value)) { OnPropertyChanged(nameof(NetworkDisplay)); OnPropertyChanged(nameof(NetworkBackground)); } }
    }

    // ── Display strings (Task Manager formatting) ──
    public string CpuDisplay => CpuPercent < 0.05 ? "0%" : $"{CpuPercent:F1}%";

    public string MemoryDisplay
    {
        get
        {
            if (MemoryMB >= 1000) return $"{MemoryMB:N1} MB";
            if (MemoryMB >= 1) return $"{MemoryMB:F1} MB";
            return $"{MemoryMB * 1024:F0} KB";
        }
    }

    public string DiskDisplay => DiskMBps < 0.05 ? "0 MB/s" : $"{DiskMBps:F1} MB/s";
    public string NetworkDisplay => NetworkMbps < 0.05 ? "0 Mbps" : $"{NetworkMbps:F1} Mbps";

    // ── Heat-map backgrounds (amber → orange → red based on intensity) ──
    public Brush CpuBackground => CpuPercent switch
    {
        > 10 => VeryHighBrush,
        > 5 => HighBrush,
        > 2 => MedBrush,
        > 0.5 => LowBrush,
        _ => TransparentBrush
    };

    public Brush MemoryBackground => MemoryMB switch
    {
        > 1000 => VeryHighBrush,
        > 500 => HighBrush,
        > 200 => MedBrush,
        > 50 => LowBrush,
        _ => TransparentBrush
    };

    public Brush DiskBackground => DiskMBps switch
    {
        > 5 => VeryHighBrush,
        > 2 => HighBrush,
        > 0.5 => MedBrush,
        > 0.1 => LowBrush,
        _ => TransparentBrush
    };

    public Brush NetworkBackground => NetworkMbps switch
    {
        > 5 => VeryHighBrush,
        > 1 => HighBrush,
        > 0.3 => MedBrush,
        > 0.05 => LowBrush,
        _ => TransparentBrush
    };

    // ── INotifyPropertyChanged ──
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name!);
        return true;
    }
}
