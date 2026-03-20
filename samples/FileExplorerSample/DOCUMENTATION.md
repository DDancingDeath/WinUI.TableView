# File Explorer Sample - Project Documentation

## Project Overview

A fully functional File Explorer application built with WinUI 3 and the WinUI.TableView control. This sample demonstrates real-world usage of TableView in a familiar application context.

## Technology Stack

- **Framework**: .NET 9.0
- **UI Framework**: WinUI 3 (Windows App SDK)
- **Target Platform**: Windows 10.0.19041.0 or later
- **Control**: WinUI.TableView (project reference)
- **Language**: C# 12 with nullable reference types

## Project Files

### Core Application Files

| File | Purpose |
|------|---------|
| `App.xaml` | Application resources and styling |
| `App.xaml.cs` | Application startup and initialization |
| `Imports.cs` | Global using directives for cleaner code |
| `MainPage.xaml` | Main UI layout with TableView and navigation |
| `MainPage.xaml.cs` | Code-behind with all application logic |

### Data Layer

| File | Purpose |
|------|---------|
| `Models/FileSystemItem.cs` | Data model representing files/folders with INotifyPropertyChanged |
| `Services/FileSystemService.cs` | Service for loading directory contents and common locations |

### Configuration Files

| File | Purpose |
|------|---------|
| `FileExplorerSample.csproj` | MSBuild project file with dependencies and settings |
| `app.manifest` | Windows application manifest for DPI awareness |
| `Properties/launchSettings.json` | Debug launch settings |

### Assets

Located in `Assets/` folder:
- Application icons (various sizes)
- Splash screen
- Store logo

## Key Classes

### FileSystemItem (Model)

```csharp
public class FileSystemItem : INotifyPropertyChanged
{
    public string Name { get; set; }
    public string Type { get; set; }
    public long Size { get; set; }
    public string SizeDisplay { get; }
    public DateTime DateModified { get; set; }
    public string Path { get; set; }
    public bool IsFolder { get; set; }
    public string IconGlyph { get; }
}
```

**Features:**
- Full property change notification
- Automatic icon selection based on file type
- Formatted size display (bytes, KB, MB, GB)
- Formatted date display

### FileSystemService (Service)

```csharp
public class FileSystemService
{
    public List<FileSystemItem> GetItems(string directoryPath)
    public List<LocationItem> GetCommonLocations()
}
```

**Features:**
- Safe directory enumeration with exception handling
- Skip inaccessible folders/files
- Friendly file type names
- Quick access to common Windows folders

### MainPage (View)

```csharp
public sealed partial class MainPage : Page, INotifyPropertyChanged
{
    public ObservableCollection<FileSystemItem> FileItems { get; }
    public ObservableCollection<LocationItem> Locations { get; }
    public string CurrentPath { get; set; }
}
```

**Features:**
- Full navigation stack (back/forward)
- Address bar for direct path entry
- Quick access panel
- Status bar with item counts and selection info
- Double-click to open files/folders

## UI Layout Structure

```
Grid
??? Row 0: Title Bar
??? Row 1: Navigation/Address Bar
?   ??? Back Button
?   ??? Forward Button
?   ??? Up Button
?   ??? Address TextBox
?   ??? Refresh Button
??? Row 2: Main Content
?   ??? Column 0: Side Panel (Quick Access)
?   ??? Column 1: Splitter
?   ??? Column 2: TableView Area
?       ??? Toolbar (New, View, Sort buttons)
?       ??? TableView with 4 columns
??? Row 3: Status Bar
    ??? Status Message
    ??? Selection Info
```

## TableView Configuration

### Columns

1. **Name Column** (Template Column)
   - Custom template with icon and text
   - Width: 300px
   - Sortable

2. **Date Modified Column** (Text Column)
   - Displays formatted date/time
   - Width: 180px
   - Sortable

3. **Type Column** (Text Column)
   - Shows file type description
   - Width: 200px
   - Sortable

4. **Size Column** (Text Column)
   - Shows formatted file size
   - Width: 120px
   - Sortable

### Features Enabled

- `AutoGenerateColumns="False"` - Manual column definition
- `SelectionMode="Extended"` - Multi-select with Ctrl/Shift
- `CanUserSortColumns="True"` - Click headers to sort
- `ShowExportOptions="True"` - Built-in CSV export
- `CornerButtonMode="Options"` - Corner button for options menu
- `AlternateRowBackground` - Zebra striping for readability
- `GridLinesVisibility="Horizontal"` - Horizontal grid lines

## Event Handlers

| Event | Handler | Purpose |
|-------|---------|---------|
| `DoubleTapped` | `FileTableView_DoubleTapped` | Open folders or launch files |
| `SelectionChanged` | `FileTableView_SelectionChanged` | Update selection info in status bar |
| `BackButton.Click` | `BackButton_Click` | Navigate to previous folder |
| `ForwardButton.Click` | `ForwardButton_Click` | Navigate to next folder in history |
| `UpButton.Click` | `UpButton_Click` | Navigate to parent folder |
| `RefreshButton.Click` | `RefreshButton_Click` | Reload current directory |
| `AddressBar.KeyDown` | `AddressBar_KeyDown` | Navigate on Enter key |
| `LocationsListView.SelectionChanged` | `LocationsListView_SelectionChanged` | Navigate to quick access location |

## Navigation Architecture

### History Management

```
???????????????????????
? Navigation History  ?
???????????????????????
?  _navigationHistory ?  Stack<string> - Previous paths
?  _forwardHistory    ?  Stack<string> - Forward paths
?  CurrentPath        ?  string - Active path
???????????????????????
```

### Navigation Flow

```
User Action ? NavigateTo() ? Push to history ? LoadDirectory() ? Update UI
```

## Data Flow

```
FileSystemService
    ? GetItems(path)
ObservableCollection<FileSystemItem>
    ? Binding
TableView.ItemsSource
    ? Display
UI (with sorting, filtering, selection)
```

## Build Configuration

- **Platform Targets**: x86, x64, ARM64
- **Self-Contained**: Yes (WindowsAppSDKSelfContained)
- **Package Type**: None (unpackaged)
- **Nullable**: Enabled
- **Implicit Usings**: Enabled
- **Language Version**: Latest (C# 12)

## Dependencies

### NuGet Packages
- `Microsoft.WindowsAppSDK` - Version 1.*
- `Microsoft.Windows.SDK.BuildTools` - Version 10.*

### Project References
- `../../src/WinUI.TableView.csproj` - Main TableView control library

## How to Build

```powershell
# From repository root
msbuild /restore samples/FileExplorerSample/FileExplorerSample.csproj /p:Configuration=Release /p:Platform=x64

# Or from FileExplorerSample directory
dotnet build -c Release
```

## How to Run

```powershell
# From Visual Studio
# 1. Set FileExplorerSample as startup project
# 2. Select platform (x64 recommended)
# 3. Press F5

# From command line (after build)
samples/FileExplorerSample/bin/x64/Release/net9.0-windows10.0.19041.0/FileExplorerSample.exe
```

## Testing Checklist

### Navigation
- [ ] Back button works and is disabled when no history
- [ ] Forward button works and is disabled when no forward history
- [ ] Up button works and is disabled at root level
- [ ] Address bar accepts valid paths
- [ ] Quick access locations navigate correctly
- [ ] Double-clicking folders opens them

### TableView
- [ ] All columns display correctly
- [ ] Icons show for different file types
- [ ] Sorting works on all columns
- [ ] Multi-selection works (Ctrl+Click, Shift+Click)
- [ ] Export to CSV works
- [ ] Grid lines are visible
- [ ] Alternate row colors show

### Status Bar
- [ ] Shows correct item and folder count
- [ ] Updates selection info when items selected
- [ ] Shows total size of selected files
- [ ] Displays error messages when appropriate

## Known Limitations

1. **Read-Only**: No file operations (copy, paste, delete, rename)
2. **Permissions**: Some system folders may not be accessible
3. **Performance**: Large directories (10,000+ items) may be slow to load
4. **Thumbnails**: No image preview or thumbnails
5. **File Icons**: Uses generic icons, not actual file type icons

## Future Enhancements

### High Priority
- [ ] Add context menu (right-click operations)
- [ ] Implement search/filter functionality
- [ ] Add breadcrumb navigation bar
- [ ] Show folder size calculation

### Medium Priority
- [ ] Add thumbnail view mode
- [ ] Implement file properties dialog
- [ ] Add file/folder operations (copy, move, delete, rename)
- [ ] Support for multiple windows/tabs

### Low Priority
- [ ] Drag and drop support
- [ ] Custom sort orders
- [ ] File preview panel
- [ ] Integration with Windows Shell

## Learning Resources

- [WinUI.TableView Documentation](https://w-ahmad.github.io/WinUI.TableView/)
- [WinUI 3 Documentation](https://learn.microsoft.com/windows/apps/winui/winui3/)
- [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/)

## Support

For questions or issues:
1. Check the README.md and QUICKSTART.md
2. Review the code comments and documentation
3. Browse the main TableView repository
4. Open an issue on GitHub

---

**Version**: 1.0  
**Created**: 2024  
**Author**: WinUI.TableView Contributors  
**License**: MIT
