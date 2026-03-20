# Quick Start Guide - File Explorer Sample

## Overview
This WinUI 3 application demonstrates how to use the **WinUI.TableView** control to build a File Explorer-like interface.

## Quick Start

### Option 1: Run from Main Solution
1. Open `WinUI.TableView.slnx` in Visual Studio 2022
2. Right-click `FileExplorerSample` project ? Set as Startup Project
3. Select platform: **x64** (recommended), x86, or ARM64
4. Press **F5** to run

### Option 2: Run Standalone
1. Navigate to `samples/FileExplorerSample/`
2. Open `FileExplorerSample.slnx`
3. Select platform and press **F5**

## What You'll See

When you run the app, you'll see:

- **Left Panel**: Quick access locations (Desktop, Documents, Downloads, etc.)
- **Top Bar**: Navigation buttons (Back, Forward, Up) and address bar
- **Main Area**: TableView displaying files and folders with columns:
  - Name (with icons)
  - Date Modified
  - Type
  - Size
- **Bottom Bar**: Status information and selection details

## Try These Features

### Navigation
- Click on folders in the quick access panel
- Double-click folders in the table to open them
- Use Back/Forward buttons to navigate history
- Use Up button to go to parent folder
- Type a path in the address bar and press Enter

### TableView Features
- **Sort**: Click column headers to sort
- **Multi-select**: Ctrl+Click or Shift+Click to select multiple items
- **Export**: Click the corner button (?) to export to CSV
- **Filter**: Use the filter options in column headers
- **Resize**: Drag column borders to resize

### Selection
- Select files/folders to see total size in status bar
- Selection count updates in real-time

## Code Highlights

### 1. Setting up TableView

```xml
<tableView:TableView ItemsSource="{x:Bind FileItems}"
                    AutoGenerateColumns="False"
                    SelectionMode="Extended"
                    CanUserSortColumns="True"
                    ShowExportOptions="True">
```

### 2. Custom Column Template with Icons

```xml
<DataTemplate x:Key="NameColumnTemplate">
    <StackPanel Orientation="Horizontal" Spacing="8">
        <FontIcon Glyph="{Binding IconGlyph}" />
        <TextBlock Text="{Binding Name}" />
    </StackPanel>
</DataTemplate>
```

### 3. Loading Data

```csharp
public ObservableCollection<FileSystemItem> FileItems { get; }

private void LoadDirectory(string path)
{
    var items = _fileSystemService.GetItems(path);
    FileItems.Clear();
    foreach (var item in items)
    {
        FileItems.Add(item);
    }
}
```

### 4. Handling Double-Click

```csharp
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
            // Open file with default application
        }
    }
}
```

## Customization Ideas

Want to extend this sample? Try:

1. **Add context menu**: Right-click operations (copy, paste, delete, rename)
2. **Add search**: Filter files by name or extension
3. **Add file preview**: Show image thumbnails or text previews
4. **Add breadcrumb navigation**: Replace address bar with breadcrumb trail
5. **Add file properties**: Show detailed file information in a panel
6. **Add drag and drop**: Move files between folders
7. **Add multiple tabs**: Open multiple folders in tabs

## Architecture

### Models
- `FileSystemItem`: Represents a file or folder with properties like Name, Type, Size, etc.

### Services
- `FileSystemService`: Handles loading directory contents and provides common locations

### Views
- `MainPage`: Main UI with navigation, quick access panel, and TableView

## Troubleshooting

**Can't access certain folders?**
- Some system folders require administrator privileges
- The app skips folders/files it can't access

**Sorting not working?**
- Make sure `CanUserSortColumns="True"` is set on TableView
- Column must have `CanUserSort="True"`

**Icons not showing?**
- Icons use Segoe MDL2 Assets font (built into Windows)
- Check `IconGlyph` property in `FileSystemItem`

## Next Steps

1. Study the code in `MainPage.xaml.cs` to understand navigation logic
2. Look at `FileSystemItem.cs` to see data model with `INotifyPropertyChanged`
3. Explore `FileSystemService.cs` for directory enumeration
4. Modify column definitions in `MainPage.xaml` to add custom columns
5. Check [TableView documentation](https://w-ahmad.github.io/WinUI.TableView/) for more features

## Questions?

- Browse the [main repository](https://github.com/w-ahmad/WinUI.TableView)
- Check other samples in `samples/TableViewNuGetSample/`
- Open an issue or discussion on GitHub

---

**Happy coding!** ??
