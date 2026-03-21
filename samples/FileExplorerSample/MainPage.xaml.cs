using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FileExplorerSample.Models;
using FileExplorerSample.Services;
using Microsoft.UI.Input;
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
    private bool _isResizing;
    private double _startX;
    private double _startWidth;
    private ViewMode _currentViewMode;
    private SortProperty _currentSortProperty;
    private SortOrder _currentSortOrder;

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
        _currentViewMode = ViewMode.Details;
        _currentSortProperty = SortProperty.Name;
        _currentSortOrder = SortOrder.Ascending;

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
                OnPropertyChanged(nameof(TabTitle));
            }
        }
    }

    /// <summary>
    /// Gets the tab title (folder name).
    /// </summary>
    public string TabTitle => GetFolderName(CurrentPath);

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
                OnPropertyChanged(nameof(HasSelection));
            }
        }
    }

    /// <summary>
    /// Gets whether there is a selection.
    /// </summary>
    public Visibility HasSelection => string.IsNullOrEmpty(_selectionText) ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// Gets the folder name from a path.
    /// </summary>
    /// <param name="path">The full path.</param>
    /// <returns>The folder name.</returns>
    public string GetFolderName(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "File Explorer";

        try
        {
            var dirInfo = new DirectoryInfo(path);
            return dirInfo.Name;
        }
        catch
        {
            return "File Explorer";
        }
    }

    /// <summary>
    /// Gets or sets the current view mode.
    /// </summary>
    public ViewMode CurrentViewMode
    {
        get => _currentViewMode;
        set
        {
            if (_currentViewMode != value)
            {
                _currentViewMode = value;
                OnPropertyChanged();
                ApplyViewMode();
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
            ApplySorting();
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
        if (DetailsTableView.SelectedItem is FileSystemItem item)
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
        var selectedCount = DetailsTableView.SelectedItems.Count;
        if (selectedCount == 0)
        {
            SelectionText = string.Empty;
        }
        else if (selectedCount == 1 && DetailsTableView.SelectedItem is FileSystemItem item)
        {
            SelectionText = item.IsFolder ? "1 folder selected" : $"1 item selected ({item.SizeDisplay})";
        }
        else
        {
            var totalSize = DetailsTableView.SelectedItems
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

    private void Splitter_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeWestEast);
    }

    private void Splitter_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!_isResizing)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }
    }

    private void Splitter_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _isResizing = true;
        _startX = e.GetCurrentPoint(this).Position.X;
        _startWidth = Content is Grid grid ? grid.ColumnDefinitions[0].ActualWidth : 250;
        Splitter.CapturePointer(e.Pointer);
    }

    private void Splitter_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_isResizing && Content is Grid grid)
        {
            var currentX = e.GetCurrentPoint(this).Position.X;
            var delta = currentX - _startX;
            var newWidth = Math.Clamp(_startWidth + delta, 150, 400);
            grid.ColumnDefinitions[0].Width = new GridLength(newWidth);
        }
    }

    private void Splitter_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isResizing = false;
        Splitter.ReleasePointerCapture(e.Pointer);
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    }

    private void ViewButton_Click(object sender, RoutedEventArgs e)
    {
        ViewMenuFlyout.ShowAt(ViewButton);
    }

    private void ViewModeMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem && menuItem.Tag is string viewModeString)
        {
            if (Enum.TryParse<ViewMode>(viewModeString, out var viewMode))
            {
                CurrentViewMode = viewMode;
            }
        }
    }

    private void ApplyViewMode()
    {
        switch (CurrentViewMode)
        {
            case ViewMode.Details:
                // Show TableView (current default)
                DetailsTableView.Visibility = Visibility.Visible;
                IconsGridView.Visibility = Visibility.Collapsed;
                ListGridView.Visibility = Visibility.Collapsed;
                TilesGridView.Visibility = Visibility.Collapsed;
                ContentGridView.Visibility = Visibility.Collapsed;
                break;

            case ViewMode.ExtraLargeIcons:
                // Show GridView with extra large icons
                DetailsTableView.Visibility = Visibility.Collapsed;
                IconsGridView.Visibility = Visibility.Visible;
                ListGridView.Visibility = Visibility.Collapsed;
                TilesGridView.Visibility = Visibility.Collapsed;
                ContentGridView.Visibility = Visibility.Collapsed;
                IconsGridView.ItemTemplate = Resources["ExtraLargeIconViewTemplate"] as DataTemplate;
                UpdateIconSize();
                break;

            case ViewMode.LargeIcons:
                // Show GridView with large icons
                DetailsTableView.Visibility = Visibility.Collapsed;
                IconsGridView.Visibility = Visibility.Visible;
                ListGridView.Visibility = Visibility.Collapsed;
                TilesGridView.Visibility = Visibility.Collapsed;
                ContentGridView.Visibility = Visibility.Collapsed;
                IconsGridView.ItemTemplate = Resources["LargeIconViewTemplate"] as DataTemplate;
                UpdateIconSize();
                break;

            case ViewMode.MediumIcons:
                // Show GridView with medium icons
                DetailsTableView.Visibility = Visibility.Collapsed;
                IconsGridView.Visibility = Visibility.Visible;
                ListGridView.Visibility = Visibility.Collapsed;
                TilesGridView.Visibility = Visibility.Collapsed;
                ContentGridView.Visibility = Visibility.Collapsed;
                IconsGridView.ItemTemplate = Resources["MediumIconViewTemplate"] as DataTemplate;
                UpdateIconSize();
                break;

            case ViewMode.SmallIcons:
                // Show GridView with small icons
                DetailsTableView.Visibility = Visibility.Collapsed;
                IconsGridView.Visibility = Visibility.Visible;
                ListGridView.Visibility = Visibility.Collapsed;
                TilesGridView.Visibility = Visibility.Collapsed;
                ContentGridView.Visibility = Visibility.Collapsed;
                IconsGridView.ItemTemplate = Resources["SmallIconViewTemplate"] as DataTemplate;
                UpdateIconSize();
                break;

            case ViewMode.List:
                // Show compact list
                DetailsTableView.Visibility = Visibility.Collapsed;
                IconsGridView.Visibility = Visibility.Collapsed;
                ListGridView.Visibility = Visibility.Visible;
                TilesGridView.Visibility = Visibility.Collapsed;
                ContentGridView.Visibility = Visibility.Collapsed;
                break;

            case ViewMode.Tiles:
                // Show tiles view
                DetailsTableView.Visibility = Visibility.Collapsed;
                IconsGridView.Visibility = Visibility.Collapsed;
                ListGridView.Visibility = Visibility.Collapsed;
                TilesGridView.Visibility = Visibility.Visible;
                ContentGridView.Visibility = Visibility.Collapsed;
                break;

            case ViewMode.Content:
                // Show content view
                DetailsTableView.Visibility = Visibility.Collapsed;
                IconsGridView.Visibility = Visibility.Collapsed;
                ListGridView.Visibility = Visibility.Collapsed;
                TilesGridView.Visibility = Visibility.Collapsed;
                ContentGridView.Visibility = Visibility.Visible;
                break;
        }
    }

    private void UpdateIconSize()
    {
        var (itemWidth, itemHeight) = CurrentViewMode switch
        {
            ViewMode.ExtraLargeIcons => (200.0, 220.0),
            ViewMode.LargeIcons => (120.0, 130.0),
            ViewMode.MediumIcons => (90.0, 90.0),
            ViewMode.SmallIcons => (70.0, 70.0),
            _ => (120.0, 130.0)
        };

        if (IconsGridView.ItemsPanelRoot is ItemsWrapGrid wrapGrid)
        {
            wrapGrid.ItemWidth = itemWidth;
            wrapGrid.ItemHeight = itemHeight;
        }
    }

    private void GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is FileSystemItem item && item.IsFolder)
        {
            NavigateTo(item.Path);
        }
    }

    private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var gridView = sender as GridView;
        var selectedCount = gridView?.SelectedItems.Count ?? 0;
        
        if (selectedCount == 0)
        {
            SelectionText = string.Empty;
        }
        else if (selectedCount == 1 && gridView?.SelectedItem is FileSystemItem item)
        {
            SelectionText = item.IsFolder ? "1 folder selected" : $"1 item selected ({item.SizeDisplay})";
        }
        else
        {
            var totalSize = gridView?.SelectedItems
                .Cast<FileSystemItem>()
                .Where(x => !x.IsFolder)
                .Sum(x => x.Size) ?? 0;
            
            var sizeDisplay = totalSize < 1024 ? $"{totalSize} bytes" :
                             totalSize < 1024 * 1024 ? $"{totalSize / 1024.0:F2} KB" :
                             totalSize < 1024 * 1024 * 1024 ? $"{totalSize / (1024.0 * 1024.0):F2} MB" :
                             $"{totalSize / (1024.0 * 1024.0 * 1024.0):F2} GB";
            
            SelectionText = $"{selectedCount} items selected ({sizeDisplay})";
        }
    }

    private void ListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is FileSystemItem item && item.IsFolder)
        {
            NavigateTo(item.Path);
        }
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listView = sender as ListView;
        var selectedCount = listView?.SelectedItems.Count ?? 0;
        
        if (selectedCount == 0)
        {
            SelectionText = string.Empty;
        }
        else if (selectedCount == 1 && listView?.SelectedItem is FileSystemItem item)
        {
            SelectionText = item.IsFolder ? "1 folder selected" : $"1 item selected ({item.SizeDisplay})";
        }
        else
        {
            var totalSize = listView?.SelectedItems
                .Cast<FileSystemItem>()
                .Where(x => !x.IsFolder)
                .Sum(x => x.Size) ?? 0;
            
            var sizeDisplay = totalSize < 1024 ? $"{totalSize} bytes" :
                             totalSize < 1024 * 1024 ? $"{totalSize / 1024.0:F2} KB" :
                             totalSize < 1024 * 1024 * 1024 ? $"{totalSize / (1024.0 * 1024.0):F2} MB" :
                             $"{totalSize / (1024.0 * 1024.0 * 1024.0):F2} GB";
            
            SelectionText = $"{selectedCount} items selected ({sizeDisplay})";
        }
    }

    private void SortButton_Click(object sender, RoutedEventArgs e)
    {
        SortMenuFlyout.ShowAt(SortButton);
    }

    private void SortPropertyMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem && menuItem.Tag is string sortPropertyString)
        {
            if (Enum.TryParse<SortProperty>(sortPropertyString, out var sortProperty))
            {
                _currentSortProperty = sortProperty;
                ApplySorting();
            }
        }
    }

    private void SortOrderMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem && menuItem.Tag is string sortOrderString)
        {
            if (Enum.TryParse<SortOrder>(sortOrderString, out var sortOrder))
            {
                _currentSortOrder = sortOrder;
                
                // Update bullet visibility (radio button behavior)
                AscendingBullet.Visibility = sortOrder == SortOrder.Ascending ? Visibility.Visible : Visibility.Collapsed;
                DescendingBullet.Visibility = sortOrder == SortOrder.Descending ? Visibility.Visible : Visibility.Collapsed;
                
                ApplySorting();
            }
        }
    }

    private void ApplySorting()
    {
        if (FileItems.Count == 0)
            return;

        var sortedItems = _currentSortProperty switch
        {
            SortProperty.Name => _currentSortOrder == SortOrder.Ascending
                ? FileItems.OrderBy(x => x.IsFolder ? 0 : 1).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                : FileItems.OrderBy(x => x.IsFolder ? 0 : 1).ThenByDescending(x => x.Name, StringComparer.OrdinalIgnoreCase),
            
            SortProperty.DateModified => _currentSortOrder == SortOrder.Ascending
                ? FileItems.OrderBy(x => x.IsFolder ? 0 : 1).ThenBy(x => x.DateModified)
                : FileItems.OrderBy(x => x.IsFolder ? 0 : 1).ThenByDescending(x => x.DateModified),
            
            SortProperty.Type => _currentSortOrder == SortOrder.Ascending
                ? FileItems.OrderBy(x => x.IsFolder ? 0 : 1).ThenBy(x => x.Type, StringComparer.OrdinalIgnoreCase)
                : FileItems.OrderBy(x => x.IsFolder ? 0 : 1).ThenByDescending(x => x.Type, StringComparer.OrdinalIgnoreCase),
            
            SortProperty.Size => _currentSortOrder == SortOrder.Ascending
                ? FileItems.OrderBy(x => x.IsFolder ? 0 : 1).ThenBy(x => x.Size)
                : FileItems.OrderBy(x => x.IsFolder ? 0 : 1).ThenByDescending(x => x.Size),
            
            _ => FileItems.OrderBy(x => x.IsFolder ? 0 : 1).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
        };

        var tempList = sortedItems.ToList();
        FileItems.Clear();
        foreach (var item in tempList)
        {
            FileItems.Add(item);
        }
    }
}







