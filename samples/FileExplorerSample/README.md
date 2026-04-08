# File Explorer Sample - TableView Demo

This is a proof-of-concept (POC) application that demonstrates the capabilities of the WinUI.TableView control by creating a Windows File Explorer-like interface.

## Features

This sample showcases the following TableView features:

### Core TableView Features
- **Custom column templates** - Name column displays file/folder icons with text
- **Multiple column types** - Text columns for Type, Date Modified, and Size
- **Sorting** - Click column headers to sort by Name, Type, Date, or Size
- **Extended selection** - Select multiple files/folders with Ctrl+Click or Shift+Click
- **Export options** - Built-in export functionality to CSV
- **Alternate row colors** - Better visual distinction between rows
- **Grid lines** - Horizontal grid lines for clarity

### File Explorer Features
- **Navigation**
  - Browse local file system directories
  - Back/Forward navigation buttons
  - Up one level button
  - Address bar for direct path entry
  
- **Quick Access Panel**
  - Desktop
  - Documents
  - Downloads
  - Pictures
  - Music
  - Videos
  - Local disk drives

- **File Operations**
  - Double-click folders to open them
  - Double-click files to open with default application
  - View file properties (name, type, size, date modified)

- **Status Bar**
  - Shows total number of items and folders
  - Displays selection information
  - Shows total size of selected files

## Project Structure

```
FileExplorerSample/
??? Models/
?   ??? FileSystemItem.cs          # Data model for files and folders
??? Services/
?   ??? FileSystemService.cs       # Service for loading file system data
??? MainPage.xaml                  # Main UI layout
??? MainPage.xaml.cs               # Code-behind with navigation logic
??? App.xaml                       # Application resources
??? App.xaml.cs                    # Application initialization
??? FileExplorerSample.csproj      # Project file
```

## How to Run

1. Open the WinUI.TableView solution in Visual Studio 2022
2. Set `FileExplorerSample` as the startup project
3. Select platform (x64, x86, or ARM64)
4. Press F5 to build and run

## Key Implementations

### TableView Usage

The main TableView is configured with:

```xml
<tableView:TableView ItemsSource="{x:Bind FileItems}"
                    AutoGenerateColumns="False"
                    SelectionMode="Extended"
                    CanUserSortColumns="True"
                    ShowExportOptions="True">
```

### Custom Column Templates

The Name column uses a custom template to show icons:

```xml
<tableView:TableViewTemplateColumn Header="Name" 
                                  CellTemplate="{StaticResource NameColumnTemplate}" />
```

### Data Binding

File system items are bound using an `ObservableCollection<FileSystemItem>`:

```csharp
public ObservableCollection<FileSystemItem> FileItems { get; }
```

## Learning Points

This sample demonstrates:

1. **How to use TableView with custom data models**
2. **Creating custom cell templates with icons**
3. **Implementing navigation and state management**
4. **Handling selection events**
5. **Using different column types effectively**
6. **Integrating TableView into a real-world application layout**

## Notes

- This is a read-only file browser (no file operations like copy/paste/delete)
- Some system folders may not be accessible due to permissions
- The application demonstrates TableView capabilities, not a production file manager
- File icons are simplified and use Segoe MDL2 Assets font glyphs

## Related Documentation

- [WinUI.TableView Documentation](https://w-ahmad.github.io/WinUI.TableView/)
- [WinUI.TableView GitHub Repository](https://github.com/w-ahmad/WinUI.TableView)
