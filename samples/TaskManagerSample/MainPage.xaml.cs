using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using WinUI.TableView;

namespace TaskManagerSample;

public partial class MainPage : Page
{
    private readonly DispatcherTimer _timer;
    private readonly Random _rng = new();
    private readonly List<ProcessItem> _allProcesses = [];
    private TextBlock? _cpuHeaderPct, _memHeaderPct, _diskHeaderPct, _netHeaderPct;

    public ObservableCollection<ProcessItem> Processes { get; } = [];

    public MainPage()
    {
        this.InitializeComponent();

        // Extend content into title bar so our custom title bar fills the chrome area
        if (Application.Current is App { MainWindow: { } win })
        {
            win.ExtendsContentIntoTitleBar = true;
            win.SetTitleBar(AppTitleBar);
        }

        PopulateProcesses();
        SetupChildRowStyle();
        SetupColumnHeaders();

        ProcessItem.IsDarkTheme = ActualTheme == ElementTheme.Dark;
        ActualThemeChanged += (s, _) => ProcessItem.IsDarkTheme = ((FrameworkElement)s).ActualTheme == ElementTheme.Dark;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private static readonly SolidColorBrush _childRowBackground = new(Color.FromArgb(20, 255, 255, 255));
    private static readonly SolidColorBrush _defaultRowBackground = new(Colors.Transparent);
    
    // High resource usage row backgrounds (theme-aware)
    private static SolidColorBrush _highCpuRowBackground = new(Color.FromArgb(25, 255, 140, 0));      // Orange tint
    private static SolidColorBrush _highMemoryRowBackground = new(Color.FromArgb(25, 255, 50, 100));  // Pink tint
    private static SolidColorBrush _criticalRowBackground = new(Color.FromArgb(35, 230, 40, 0));      // Red tint

    private void SetupChildRowStyle()
    {
        // Mark all children
        foreach (var p in Processes)
        {
            if (p.Children is { Count: > 0 })
            {
                foreach (var child in p.Children)
                    child.IsChild = true;
            }
        }

        // Apply a subtle background to child rows at the row level
        ProcessTable.ContainerContentChanging += OnContainerContentChanging;
        
        // Update row background colors when theme changes
        ActualThemeChanged += (s, _) => UpdateRowBackgroundColors(((FrameworkElement)s).ActualTheme);
        UpdateRowBackgroundColors(ActualTheme);
    }
    
    private void UpdateRowBackgroundColors(ElementTheme theme)
    {
        if (theme == ElementTheme.Dark)
        {
            _highCpuRowBackground = new(Color.FromArgb(25, 255, 140, 0));      // Orange
            _highMemoryRowBackground = new(Color.FromArgb(25, 255, 50, 100));  // Pink
            _criticalRowBackground = new(Color.FromArgb(35, 230, 40, 0));      // Red
        }
        else
        {
            _highCpuRowBackground = new(Color.FromArgb(20, 255, 200, 100));    // Light orange
            _highMemoryRowBackground = new(Color.FromArgb(20, 200, 150, 255)); // Light purple
            _criticalRowBackground = new(Color.FromArgb(30, 255, 100, 100));   // Light red
        }
    }

    private StackPanel CreateHeaderContent(string label, out TextBlock pctBlock)
    {
        pctBlock = new TextBlock
        {
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        var labelBlock = new TextBlock
        {
            Text = label,
            FontSize = 12,
            Foreground = (SolidColorBrush)Application.Current.Resources["TextFillColorSecondaryBrush"],
            HorizontalAlignment = HorizontalAlignment.Center
        };
        return new StackPanel
        {
            Spacing = 0,
            HorizontalAlignment = HorizontalAlignment.Center,
            Children = { pctBlock, labelBlock }
        };
    }

    private void SetupColumnHeaders()
    {
        CpuColumn.Header = CreateHeaderContent("CPU", out _cpuHeaderPct);
        MemoryColumn.Header = CreateHeaderContent("Memory", out _memHeaderPct);
        DiskColumn.Header = CreateHeaderContent("Disk", out _diskHeaderPct);
        NetworkColumn.Header = CreateHeaderContent("Network", out _netHeaderPct);
        UpdateHeaderSummary();
    }

    private void UpdateHeaderSummary()
    {
        double totalCpu = 0, totalMem = 0, totalDisk = 0, totalNet = 0;
        foreach (var p in Processes)
        {
            totalCpu += p.CpuPercent;
            totalMem += p.MemoryMB;
            totalDisk += p.DiskMBps;
            totalNet += p.NetworkMbps;
        }

        if (_cpuHeaderPct is not null) _cpuHeaderPct.Text = $"{Math.Min(totalCpu, 100):F0}%";
        if (_memHeaderPct is not null) _memHeaderPct.Text = $"{totalMem / 1024 / 16 * 100:F0}%";
        if (_diskHeaderPct is not null) _diskHeaderPct.Text = $"{Math.Min(totalDisk, 100):F0}%";
        if (_netHeaderPct is not null) _netHeaderPct.Text = $"{Math.Min(totalNet, 100):F0}%";

        // Bottom status bar
        StatusCpuText.Text = $"{Math.Min(totalCpu, 100):F0}%";
        var ramGB = totalMem / 1024.0;
        var ramPct = totalMem / 16384.0 * 100.0;
        StatusRamText.Text = $"{ramGB:F1}/{16.0:F0} GB ({Math.Min(ramPct, 100):F0}%)";
        StatusDiskText.Text = $"{Math.Min(totalDisk, 100):F0}%";
        StatusNetText.Text = totalNet >= 1.0 ? $"{totalNet:F1} Mbps"
                           : totalNet > 0.001 ? $"{totalNet * 1000:F0} Kbps"
                           : "0 Kbps";
    }

    private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.ItemContainer is TableViewRow row && args.Item is ProcessItem process)
        {
            // Priority 1: Critical processes (high CPU AND high memory)
            if (process.IsCriticalResource)
            {
                row.Background = _criticalRowBackground;
                row.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 230, 40, 0));
                row.BorderThickness = new Thickness(2, 0, 0, 0);
            }
            // Priority 2: High CPU processes
            else if (process.IsHighCpuResource)
            {
                row.Background = _highCpuRowBackground;
                row.BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 140, 0));
                row.BorderThickness = new Thickness(2, 0, 0, 0);
            }
            // Priority 3: High memory processes
            else if (process.IsHighMemoryResource)
            {
                row.Background = _highMemoryRowBackground;
                row.BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 50, 100));
                row.BorderThickness = new Thickness(2, 0, 0, 0);
            }
            // Priority 4: Child rows (subtle background)
            else if (process.IsChild)
            {
                row.Background = _childRowBackground;
                row.BorderBrush = new SolidColorBrush(Colors.Transparent);
                row.BorderThickness = new Thickness(0);
            }
            // Default: transparent
            else
            {
                row.Background = _defaultRowBackground;
                row.BorderBrush = new SolidColorBrush(Colors.Transparent);
                row.BorderThickness = new Thickness(0);
            }
        }
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

        UpdateHeaderSummary();
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

    private FilterDescription? _searchFilter;

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        var query = sender.Text?.Trim() ?? string.Empty;

        // Remove previous search filter
        if (_searchFilter is not null)
        {
            ProcessTable.FilterDescriptions.Remove(_searchFilter);
            _searchFilter = null;
        }

        if (!string.IsNullOrEmpty(query))
        {
            _searchFilter = new FilterDescription(null, item =>
                item is ProcessItem p &&
                (p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                 (p.Children is { Count: > 0 } &&
                  p.Children.Any(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))));

            ProcessTable.FilterDescriptions.Add(_searchFilter);
        }
    }

    private void OnEndTaskClick(object sender, RoutedEventArgs e)
    {
        if (ProcessTable.SelectedItem is ProcessItem selected)
        {
            // Remove from parent collection
            foreach (var p in Processes)
            {
                if (p.Children?.Remove(selected) == true)
                    return;
            }
            Processes.Remove(selected);
            _allProcesses.Remove(selected);
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
            IsHighGpu = true,
            GpuEngine = "GPU 0 - 3D",
            IsExpanded = false,
            Children =
            [
                new() { Name = "Browser", Category = "Apps", IconGlyph = "\uE774", MemoryBaseline = 320, IsHighCpu = true, IsHighGpu = true, GpuEngine = "GPU 0 - 3D" },
                new() { Name = "GPU Process", Category = "Apps", IconGlyph = "\uE943", MemoryBaseline = 198, IsHighGpu = true, GpuEngine = "GPU 0 - Video Decode" },
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
            IsHighGpu = true,
            GpuEngine = "GPU 0 - 3D",
            IsExpanded = false,
            StatusText = "Efficiency …",
            IsEfficiency = true,
            Children =
            [
                new() { Name = "Teams Main", Category = "Apps", IconGlyph = "\uE902", MemoryBaseline = 180, IsHighCpu = true, IsHighGpu = true, GpuEngine = "GPU 0 - 3D" },
                new() { Name = "Teams GPU", Category = "Apps", IconGlyph = "\uE943", MemoryBaseline = 32, IsHighGpu = true, GpuEngine = "GPU 0 - Copy" },
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
            IsHighGpu = true,
            GpuEngine = "GPU 0 - 3D",
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Visual Studio 2022",
            Category = "Apps",
            IconGlyph = "\uE7C3",
            MemoryBaseline = 1159.1,
            IsHighCpu = true,
            IsHighDisk = true,
            IsHighGpu = true,
            GpuEngine = "GPU 0 - 3D",
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Visual Studio 2022",
            Category = "Apps",
            IconGlyph = "\uE7C3",
            MemoryBaseline = 962.4,
            IsHighCpu = true,
            IsHighGpu = true,
            GpuEngine = "GPU 0 - 3D",
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Visual Studio 2022",
            Category = "Apps",
            IconGlyph = "\uE7C3",
            MemoryBaseline = 1301.1,
            IsHighCpu = true,
            IsHighGpu = true,
            GpuEngine = "GPU 0 - 3D",
        });

        Processes.Add(new ProcessItem
        {
            Name = "Microsoft Word",
            Category = "Apps",
            IconGlyph = "\uE8D2",
            MemoryBaseline = 360.3,
            IsHighCpu = true,
            GpuEngine = "GPU 0 - 3D",
        });

        Processes.Add(new ProcessItem
        {
            Name = "MUXControlsTestApp",
            Category = "Apps",
            IconGlyph = "\uE737",
            MemoryBaseline = 170.1,
            IsHighGpu = true,
            GpuEngine = "GPU 0 - 3D",
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
            GpuEngine = "GPU 0 - 3D",
        });

        Processes.Add(new ProcessItem
        {
            Name = "Photos",
            Category = "Apps",
            IconGlyph = "\uEB9F",
            MemoryBaseline = 12.4,
            IsSuspended = true,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Snipping Tool",
            Category = "Apps",
            IconGlyph = "\uECD5",
            MemoryBaseline = 14.2,
            IsSuspended = true,
        });

        Processes.Add(new ProcessItem
        {
            Name = "Xbox",
            Category = "Apps",
            IconGlyph = "\uE7FC",
            MemoryBaseline = 18.6,
            IsSuspended = true,
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
            IsHighGpu = true,
            GpuEngine = "GPU 0 - 3D",
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
            GpuEngine = "GPU 0 - Video Decode",
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

        // Keep a copy for search filtering
        foreach (var p in Processes)
            _allProcesses.Add(p);
    }
}

/// <summary>
/// Represents a process or sub-process row displayed in the Task Manager table.
/// All numeric stats fire PropertyChanged so the TableView cells update in real time.
/// </summary>
public sealed class ProcessItem : INotifyPropertyChanged
{
    // ── Heat-map brushes: dark = amber/orange, light = blue ──
    private static readonly SolidColorBrush TransparentBrush = new(Colors.Transparent);

    // ── App icon color tiles (matches real app brand colors) ──
    private static readonly Dictionary<string, SolidColorBrush> _iconColors = new()
    {
        ["Calendar"]                       = new(Color.FromArgb(255,   0, 120, 212)),
        ["Files"]                          = new(Color.FromArgb(255, 255, 140,   0)),
        ["Microsoft Edge"]                 = new(Color.FromArgb(255,   0,  90, 158)),
        ["Microsoft Excel"]                = new(Color.FromArgb(255,  33, 115,  70)),
        ["Microsoft OneNote"]              = new(Color.FromArgb(255, 128,  57, 123)),
        ["Microsoft Teams"]                = new(Color.FromArgb(255,  98, 100, 167)),
        ["Microsoft Visual Studio 2022"]   = new(Color.FromArgb(255,  92,  45, 145)),
        ["Microsoft Word"]                 = new(Color.FromArgb(255,  43,  87, 154)),
        ["MUXControlsTestApp"]             = new(Color.FromArgb(255,   0, 120, 212)),
        ["Notepad.exe"]                    = new(Color.FromArgb(255, 255, 185,   0)),
        ["Outlook"]                        = new(Color.FromArgb(255,   0, 114, 198)),
        ["Photos"]                         = new(Color.FromArgb(255,   0, 153, 204)),
        ["Snipping Tool"]                  = new(Color.FromArgb(255,   0, 120, 212)),
        ["Task Manager"]                   = new(Color.FromArgb(255,   0, 153, 188)),
        ["Xbox"]                           = new(Color.FromArgb(255,  16, 124,  16)),
        ["Desktop Window Manager"]         = new(Color.FromArgb(255,   0, 120, 212)),
        ["Search Host"]                    = new(Color.FromArgb(255,   0, 120, 212)),
        ["Windows Explorer"]               = new(Color.FromArgb(255, 255, 185,   0)),
        ["Antimalware Service Executable"] = new(Color.FromArgb(255,   0, 153,  76)),
    };
    private static readonly SolidColorBrush _defaultAppBrush = new(Color.FromArgb(255, 100, 100, 190));
    private static readonly SolidColorBrush _defaultSysBrush = new(Color.FromArgb(255, 120, 120, 130));

    // Dark theme: yellow → amber → orange → deep red (matches Win11 TM heat-map)
    private static readonly SolidColorBrush DarkLow =      new(Color.FromArgb(45,  255, 210,  60));
    private static readonly SolidColorBrush DarkMed =      new(Color.FromArgb(85,  255, 165,  20));
    private static readonly SolidColorBrush DarkHigh =     new(Color.FromArgb(130, 255, 110,   0));
    private static readonly SolidColorBrush DarkVeryHigh = new(Color.FromArgb(180, 230,  40,   0));

    // Light theme: blue tints (pale → saturated blue)
    private static readonly SolidColorBrush LightLow =      new(Color.FromArgb(35,  60, 140, 230));
    private static readonly SolidColorBrush LightMed =      new(Color.FromArgb(70,  40, 120, 215));
    private static readonly SolidColorBrush LightHigh =     new(Color.FromArgb(110, 30, 100, 200));
    private static readonly SolidColorBrush LightVeryHigh = new(Color.FromArgb(155, 20,  80, 190));

    internal static bool IsDarkTheme { get; set; } = true;

    private static SolidColorBrush Low => IsDarkTheme ? DarkLow : LightLow;
    private static SolidColorBrush Med => IsDarkTheme ? DarkMed : LightMed;
    private static SolidColorBrush High => IsDarkTheme ? DarkHigh : LightHigh;
    private static SolidColorBrush VeryHigh => IsDarkTheme ? DarkVeryHigh : LightVeryHigh;

    // ── Identity ──
    public string Name { get; set; } = string.Empty;
    public string DisplayName => Children is { Count: > 0 } ? $"{Name} ({Children.Count})" : Name;
    public string Category { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = "\uE7BA";
    public string IconPath { get; set; } = string.Empty;
    public Brush IconBackground =>
        _iconColors.TryGetValue(Name, out var b) ? b :
        Category == "Apps" ? _defaultAppBrush : _defaultSysBrush;

    // ── Visibility helpers for icon/glyph ──
    public Visibility HasIcon => string.IsNullOrEmpty(IconPath) ? Visibility.Collapsed : Visibility.Visible;
    public Visibility HasGlyph => string.IsNullOrEmpty(IconPath) ? Visibility.Visible : Visibility.Collapsed;

    // ── Status ──
    public string StatusText { get; set; } = string.Empty;
    public bool IsEfficiency { get; set; }
    public Visibility HasStatusText => string.IsNullOrEmpty(StatusText) ? Visibility.Collapsed : Visibility.Visible;
    public Visibility IsEfficiencyMode => IsEfficiency ? Visibility.Visible : Visibility.Collapsed;

    public bool IsSuspended { get; set; }
    public Visibility IsSuspendedVisibility => IsSuspended ? Visibility.Visible : Visibility.Collapsed;

    // ── Hierarchy ──
    public bool IsExpanded { get; set; }
    public bool IsChild { get; set; }
    public List<ProcessItem> Children { get; set; } = [];

    // Name cell: children are indented with a plain icon; parents get the colored brand tile
    public Thickness NameIndentMargin => IsChild ? new Thickness(20, 0, 0, 0) : new Thickness(0);
    public Visibility ParentIconVisibility => IsChild ? Visibility.Collapsed : Visibility.Visible;
    public Visibility ChildIconVisibility  => IsChild ? Visibility.Visible   : Visibility.Collapsed;

    // ── Behavioral hints (for simulation) ──
    public bool IsHighCpu { get; set; }
    public bool IsHighDisk { get; set; }
    public bool IsHighNetwork { get; set; }
    public bool IsHighGpu { get; set; }
    public string GpuEngine { get; set; } = string.Empty;
    public double MemoryBaseline { get; set; } = 10;

    // ── Live stats ──
    private double _cpuPercent;
    public double CpuPercent
    {
        get => _cpuPercent;
        set 
        { 
            if (SetField(ref _cpuPercent, value)) 
            { 
                OnPropertyChanged(nameof(CpuDisplay)); 
                OnPropertyChanged(nameof(CpuBackground)); 
                OnPropertyChanged(nameof(HasCpuHeatMap)); 
                OnPropertyChanged(nameof(IsHighCpuResource)); 
                OnPropertyChanged(nameof(IsCriticalResource));
                OnPropertyChanged(nameof(IsCriticalResourceVisibility));
                OnPropertyChanged(nameof(IsHighCpuResourceVisibility));
                OnPropertyChanged(nameof(IsHighMemoryResourceVisibility));
            } 
        }
    }

    private double _memoryMB;
    public double MemoryMB
    {
        get => _memoryMB;
        set 
        { 
            if (SetField(ref _memoryMB, value)) 
            { 
                OnPropertyChanged(nameof(MemoryDisplay)); 
                OnPropertyChanged(nameof(MemoryBackground)); 
                OnPropertyChanged(nameof(HasMemoryHeatMap)); 
                OnPropertyChanged(nameof(IsHighMemoryResource)); 
                OnPropertyChanged(nameof(IsCriticalResource));
                OnPropertyChanged(nameof(IsCriticalResourceVisibility));
                OnPropertyChanged(nameof(IsHighCpuResourceVisibility));
                OnPropertyChanged(nameof(IsHighMemoryResourceVisibility));
            } 
        }
    }

    private double _diskMBps;
    public double DiskMBps
    {
        get => _diskMBps;
        set { if (SetField(ref _diskMBps, value)) { OnPropertyChanged(nameof(DiskDisplay)); OnPropertyChanged(nameof(DiskBackground)); OnPropertyChanged(nameof(HasDiskHeatMap)); OnPropertyChanged(nameof(IsHighDiskResource)); } }
    }

    private double _networkMbps;
    public double NetworkMbps
    {
        get => _networkMbps;
        set { if (SetField(ref _networkMbps, value)) { OnPropertyChanged(nameof(NetworkDisplay)); OnPropertyChanged(nameof(NetworkBackground)); OnPropertyChanged(nameof(HasNetworkHeatMap)); OnPropertyChanged(nameof(IsHighNetworkResource)); } }
    }

    private double _gpuPercent;
    public double GpuPercent
    {
        get => _gpuPercent;
        set { if (SetField(ref _gpuPercent, value)) { OnPropertyChanged(nameof(GpuDisplay)); OnPropertyChanged(nameof(GpuBackground)); } }
    }

    private string _powerUsage = "Very low";
    public string PowerUsage
    {
        get => _powerUsage;
        set { if (SetField(ref _powerUsage, value)) OnPropertyChanged(nameof(PowerUsageBackground)); }
    }

    private string _powerUsageTrend = "Very low";
    public string PowerUsageTrend
    {
        get => _powerUsageTrend;
        set { if (SetField(ref _powerUsageTrend, value)) OnPropertyChanged(nameof(PowerUsageTrendBackground)); }
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
    public string GpuDisplay => GpuPercent < 0.05 ? "0%" : $"{GpuPercent:F1}%";

    // ── Heat-map backgrounds ──
    public Brush CpuBackground => CpuPercent switch
    {
        > 20 => VeryHigh,
        > 10 => High,
        > 5 => Med,
        > 1 => Low,
        _ => TransparentBrush
    };

    public Brush MemoryBackground => MemoryMB switch
    {
        > 2000 => VeryHigh,
        > 1000 => High,
        > 500 => Med,
        > 100 => Low,
        _ => TransparentBrush
    };

    public Brush DiskBackground => DiskMBps switch
    {
        > 10 => VeryHigh,
        > 5 => High,
        > 2 => Med,
        > 0.5 => Low,
        _ => TransparentBrush
    };

    public Brush NetworkBackground => NetworkMbps switch
    {
        > 10 => VeryHigh,
        > 5 => High,
        > 1 => Med,
        > 0.3 => Low,
        _ => TransparentBrush
    };

    // ── Heat-map visibility (only show overlay when there's actual heat) ──
    public Visibility HasCpuHeatMap => CpuPercent > 1 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility HasMemoryHeatMap => MemoryMB > 100 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility HasDiskHeatMap => DiskMBps > 0.5 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility HasNetworkHeatMap => NetworkMbps > 0.3 ? Visibility.Visible : Visibility.Collapsed;
    
    // ── Row-level highlighting (for custom row templates) ──
    /// <summary>
    /// Indicates if this process is using high CPU resources (>15% CPU)
    /// </summary>
    public bool IsHighCpuResource => CpuPercent > 15;
    
    /// <summary>
    /// Indicates if this process is using high memory resources (>500 MB)
    /// </summary>
    public bool IsHighMemoryResource => MemoryMB > 500;
    
    /// <summary>
    /// Indicates if this process is using high disk I/O (>5 MB/s)
    /// </summary>
    public bool IsHighDiskResource => DiskMBps > 5;
    
    /// <summary>
    /// Indicates if this process is using high network bandwidth (>2 Mbps)
    /// </summary>
    public bool IsHighNetworkResource => NetworkMbps > 2;
    
    /// <summary>
    /// Indicates if this process is critical (high CPU AND high memory)
    /// </summary>
    public bool IsCriticalResource => CpuPercent > 20 && MemoryMB > 800;
    
    // Visibility properties for warning indicators in Name column
    public Visibility IsCriticalResourceVisibility => IsCriticalResource ? Visibility.Visible : Visibility.Collapsed;
    public Visibility IsHighCpuResourceVisibility => !IsCriticalResource && IsHighCpuResource ? Visibility.Visible : Visibility.Collapsed;
    public Visibility IsHighMemoryResourceVisibility => !IsCriticalResource && !IsHighCpuResource && IsHighMemoryResource ? Visibility.Visible : Visibility.Collapsed;

    public Brush GpuBackground => GpuPercent switch
    {
        > 20 => VeryHigh,
        > 10 => High,
        > 5  => Med,
        > 1  => Low,
        _    => TransparentBrush
    };

    public Brush PowerUsageBackground => PowerUsage switch
    {
        "Very high" => VeryHigh,
        "High"      => High,
        "Moderate"  => Med,
        "Low"       => Low,
        _           => TransparentBrush
    };

    public Brush PowerUsageTrendBackground => PowerUsageTrend switch
    {
        "Very high" => VeryHigh,
        "High"      => High,
        "Moderate"  => Med,
        "Low"       => Low,
        _           => TransparentBrush
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
