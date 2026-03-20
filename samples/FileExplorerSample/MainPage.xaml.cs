using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FileExplorerSample.Models;
using FileExplorerSample.Services;
using Microsoft.UI.Xaml.Input;

namespace FileExplorerSample;

/// <summary>
/// Main page displaying the file explorer interface.
/// </summary>
public sealed partial class MainPage : Page, INotifyPropertyChanged
{
    private readonly FileSystemService _fileSystemService;
    private readonly Stack<string> _navigationHistory;
    private readonly Stack<string> _forwardHistory;
    private string _currentPath;
    private string _statusMessage;
    private string _selectionText;

    /// <summary>
    /// Initializes the new instance of the MainPage class.
    /// </summary>
    public MainPage()
    {
        InitializeComponent();
        _fileSystemService = new FileSystemService();
        _navigationHistory = new Stack<string>();
        _forwardHistory = new Stack<string>();
        
        // Initialize with user's Documents folder
        _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _statusMessage = string.Empty;
        _selectionText = string.Empty;

        FileItems = new ObservableCollection<FileSystemItem>();
        Locations = new ObservableCollection<LocationItem>(_fileSystemService.GetCommonLocations());

        Loaded += MainPage_Loaded;
    }

    /// <summary>
    /// Gets the collection of file system items to display.
    /// </summary>
    public ObservableCollection<FileSystemItem> FileItems { get; }

    /// <summary>
    /// Gets the collection of quick access locations.
    /// </summary>
    public ObservableCollection<LocationItem> Locations { get; }

    /// <summary>
    /// Gets or sets the current directory path.
    /// </summary>
    public string CurrentPath
    {
        get => _currentPath;
        set
        {
            if (_currentPath != value)
            {
                _currentPath = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the status bar message.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the selection info text.
    /// </summary>
    public string SelectionText
    {
        get => _selectionText;
        set
        {
            if (_selectionText != value)
            {
                _selectionText = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        LoadDirectory(CurrentPath);
    }

    /// <summary>
    /// Loads the contents of the specified directory.
    /// </summary>
    /// <param name="path">The path to load.</param>
    /// <param name="enableGrouping">Whether to enable grouping by type.</param>
    private void LoadDirectory(string path, bool enableGrouping = false)
    {
        try
        {
            var items = _fileSystemService.GetItems(path, enableGrouping);
            
            FileItems.Clear();
            foreach (var item in items)
            {
                FileItems.Add(item);
            }

            CurrentPath = path;
            UpdateStatusBar();
            
            BackButton.IsEnabled = _navigationHistory.Count > 0;
            ForwardButton.IsEnabled = _forwardHistory.Count > 0;
            UpButton.IsEnabled = Directory.GetParent(path) != null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading directory: {ex.Message}";
        }
    }

    /// <summary>
    /// Navigates to the specified path.
    /// </summary>
    /// <param name="path">The path to navigate to.</param>
    private void NavigateTo(string path)
    {
        if (!Directory.Exists(path))
        {
            StatusMessage = $"Directory not found: {path}";
            return;
        }

        _navigationHistory.Push(CurrentPath);
        _forwardHistory.Clear();
        LoadDirectory(path);
    }

    private void UpdateStatusBar()
    {
        var folderCount = FileItems.Count(x => x.IsFolder);
        var fileCount = FileItems.Count(x => !x.IsFolder);
        StatusMessage = $"{fileCount} item(s) · {folderCount} folder(s)";
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationHistory.Count > 0)
        {
            _forwardHistory.Push(CurrentPath);
            var previousPath = _navigationHistory.Pop();
            LoadDirectory(previousPath);
        }
    }

    private void ForwardButton_Click(object sender, RoutedEventArgs e)
    {
        if (_forwardHistory.Count > 0)
        {
            _navigationHistory.Push(CurrentPath);
            var forwardPath = _forwardHistory.Pop();
            LoadDirectory(forwardPath);
        }
    }

    private void UpButton_Click(object sender, RoutedEventArgs e)
    {
        var parent = Directory.GetParent(CurrentPath);
        if (parent != null)
        {
            NavigateTo(parent.FullName);
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadDirectory(CurrentPath);
        StatusMessage = "Refreshed";
    }

    private void AddressBar_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            var path = AddressBar.Text;
            if (Directory.Exists(path))
            {
                NavigateTo(path);
            }
            else
            {
                StatusMessage = $"Invalid path: {path}";
                AddressBar.Text = CurrentPath;
            }
        }
    }

    private void LocationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LocationsListView.SelectedItem is LocationItem location)
        {
            NavigateTo(location.Path);
        }
    }

    private void DriveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string drivePath)
        {
            NavigateTo(drivePath);
        }
    }

    private void FileTableView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (FileTableView.SelectedItem is FileSystemItem item)
        {
            if (item.IsFolder)
            {
                NavigateTo(item.Path);
            }
            else
            {
                // For files, you could open them with the default application
                StatusMessage = $"Opening {item.Name}...";
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = item.Path,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error opening file: {ex.Message}";
                }
            }
        }
    }

    private void FileTableView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedCount = FileTableView.SelectedItems.Count;
        if (selectedCount == 0)
        {
            SelectionText = string.Empty;
        }
        else if (selectedCount == 1 && FileTableView.SelectedItem is FileSystemItem item)
        {
            SelectionText = item.IsFolder ? "1 folder selected" : $"1 item selected ({item.SizeDisplay})";
        }
        else
        {
            var totalSize = FileTableView.SelectedItems
                .Cast<FileSystemItem>()
                .Where(x => !x.IsFolder)
                .Sum(x => x.Size);
            
            var sizeDisplay = totalSize < 1024 ? $"{totalSize} bytes" :
                             totalSize < 1024 * 1024 ? $"{totalSize / 1024.0:F2} KB" :
                             totalSize < 1024 * 1024 * 1024 ? $"{totalSize / (1024.0 * 1024.0):F2} MB" :
                             $"{totalSize / (1024.0 * 1024.0 * 1024.0):F2} GB";
            
            SelectionText = $"{selectedCount} items selected ({sizeDisplay})";
        }
    }

    private void DarkModeToggle_Click(object sender, RoutedEventArgs e)
    {
        if (DarkModeToggle.IsChecked == true)
        {
            RequestedTheme = ElementTheme.Dark;
        }
        else
        {
            RequestedTheme = ElementTheme.Light;
        }
    }
}



