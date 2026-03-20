using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileExplorerSample.Models;

/// <summary>
/// Represents a file system item (file or folder) in the File Explorer.
/// </summary>
public class FileSystemItem : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _type = string.Empty;
    private long _size;
    private DateTime _dateModified;
    private string _path = string.Empty;

    /// <summary>
    /// Gets or sets the name of the file or folder.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the type of the item (e.g., "File folder", "Text Document").
    /// </summary>
    public string Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the size of the item in bytes.
    /// </summary>
    public long Size
    {
        get => _size;
        set
        {
            if (_size != value)
            {
                _size = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SizeDisplay));
            }
        }
    }

    /// <summary>
    /// Gets the formatted size string for display.
    /// </summary>
    public string SizeDisplay
    {
        get
        {
            if (IsFolder)
                return string.Empty;

            if (Size < 1024)
                return $"{Size} bytes";
            if (Size < 1024 * 1024)
                return $"{Size / 1024.0:F2} KB";
            if (Size < 1024 * 1024 * 1024)
                return $"{Size / (1024.0 * 1024.0):F2} MB";
            return $"{Size / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }
    }

    /// <summary>
    /// Gets or sets the date the item was last modified.
    /// </summary>
    public DateTime DateModified
    {
        get => _dateModified;
        set
        {
            if (_dateModified != value)
            {
                _dateModified = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DateModifiedDisplay));
            }
        }
    }

    /// <summary>
    /// Gets the formatted date modified string for display.
    /// </summary>
    public string DateModifiedDisplay => DateModified.ToString("g");

    /// <summary>
    /// Gets or sets the full path of the item.
    /// </summary>
    public string Path
    {
        get => _path;
        set
        {
            if (_path != value)
            {
                _path = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this item is a folder.
    /// </summary>
    public bool IsFolder { get; set; }

    /// <summary>
    /// Gets or sets the group label for grouping items (e.g., "Folders", "Documents", "Images").
    /// </summary>
    public string? GroupLabel { get; set; }

    /// <summary>
    /// Gets the icon glyph for the item based on its type.
    /// </summary>
    public string IconGlyph
    {
        get
        {
            if (IsFolder)
                return "\uE8B7"; // Folder icon

            // File icons based on extension
            var extension = System.IO.Path.GetExtension(Name).ToLowerInvariant();
            return extension switch
            {
                ".txt" => "\uE8A5", // Document
                ".doc" or ".docx" => "\uE8A5", // Document
                ".xls" or ".xlsx" => "\uE9E9", // Excel
                ".pdf" => "\uE8A5", // Document
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "\uEB9F", // Picture
                ".mp3" or ".wav" or ".wma" => "\uE8D6", // Audio
                ".mp4" or ".avi" or ".mkv" => "\uE8B2", // Video
                ".zip" or ".rar" or ".7z" => "\uE8B5", // Archive
                ".exe" => "\uE756", // Application
                ".cs" => "\uE943", // Code
                ".xaml" or ".xml" => "\uE8A5", // Document
                _ => "\uE7C3", // Default file icon
            };
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
