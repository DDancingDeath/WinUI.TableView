using FileExplorerSample.Models;

namespace FileExplorerSample.Services;

/// <summary>
/// Service for loading file system items.
/// </summary>
public class FileSystemService
{
    /// <summary>
    /// Gets the items in the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory.</param>
    /// <param name="enableGrouping">Whether to enable grouping by type.</param>
    /// <returns>A list of file system items.</returns>
    public List<FileSystemItem> GetItems(string directoryPath, bool enableGrouping = false)
    {
        var items = new List<FileSystemItem>();

        try
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            // Add directories
            foreach (var dir in directoryInfo.GetDirectories())
            {
                try
                {
                    items.Add(new FileSystemItem
                    {
                        Name = dir.Name,
                        Type = "File folder",
                        Size = 0,
                        DateModified = dir.LastWriteTime,
                        Path = dir.FullName,
                        IsFolder = true,
                        GroupLabel = enableGrouping ? "Folders" : null
                    });
                }
                catch
                {
                    // Skip directories we don't have access to
                }
            }

            // Add files
            foreach (var file in directoryInfo.GetFiles())
            {
                try
                {
                    items.Add(new FileSystemItem
                    {
                        Name = file.Name,
                        Type = GetFileType(file.Extension),
                        Size = file.Length,
                        DateModified = file.LastWriteTime,
                        Path = file.FullName,
                        IsFolder = false,
                        GroupLabel = enableGrouping ? GetFileGroupLabel(file.Extension) : null
                    });
                }
                catch
                {
                    // Skip files we don't have access to
                }
            }
        }
        catch
        {
            // Return empty list if we can't access the directory
        }

        return items;
    }

    /// <summary>
    /// Gets a friendly file type description based on the file extension.
    /// </summary>
    /// <param name="extension">The file extension.</param>
    /// <returns>A friendly file type description.</returns>
    private string GetFileType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".txt" => "Text Document",
            ".doc" => "Microsoft Word Document",
            ".docx" => "Microsoft Word Document",
            ".xls" => "Microsoft Excel Worksheet",
            ".xlsx" => "Microsoft Excel Worksheet",
            ".pdf" => "PDF Document",
            ".jpg" or ".jpeg" => "JPEG Image",
            ".png" => "PNG Image",
            ".gif" => "GIF Image",
            ".bmp" => "Bitmap Image",
            ".mp3" => "MP3 Audio",
            ".wav" => "WAV Audio",
            ".wma" => "Windows Media Audio",
            ".mp4" => "MP4 Video",
            ".avi" => "AVI Video",
            ".mkv" => "MKV Video",
            ".zip" => "ZIP Archive",
            ".rar" => "RAR Archive",
            ".7z" => "7-Zip Archive",
            ".exe" => "Application",
            ".cs" => "C# Source File",
            ".xaml" => "XAML Document",
            ".xml" => "XML Document",
            ".json" => "JSON File",
            ".csproj" => "C# Project File",
            ".sln" => "Visual Studio Solution",
            "" => "File",
            _ => $"{extension.TrimStart('.').ToUpperInvariant()} File"
        };
    }

    /// <summary>
    /// Gets the group label for a file based on its extension.
    /// </summary>
    /// <param name="extension">The file extension.</param>
    /// <returns>A group label.</returns>
    private string GetFileGroupLabel(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".txt" or ".doc" or ".docx" or ".pdf" or ".xaml" or ".xml" or ".json" => "Documents",
            ".xls" or ".xlsx" => "Spreadsheets",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "Images",
            ".mp3" or ".wav" or ".wma" => "Audio",
            ".mp4" or ".avi" or ".mkv" => "Videos",
            ".zip" or ".rar" or ".7z" => "Archives",
            ".exe" => "Applications",
            ".cs" or ".csproj" or ".sln" => "Code Files",
            "" => "Files",
            _ => "Other Files"
        };
    }

    /// <summary>
    /// Gets a list of common locations (This PC shortcuts).
    /// </summary>
    /// <returns>A list of common locations.</returns>
    public List<LocationItem> GetCommonLocations()
    {
        return new List<LocationItem>
        {
            new LocationItem { Name = "Desktop", Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), IconGlyph = "\uE8FC" },
            new LocationItem { Name = "Documents", Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), IconGlyph = "\uE8A5" },
            new LocationItem { Name = "Downloads", Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"), IconGlyph = "\uE896" },
            new LocationItem { Name = "Pictures", Path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), IconGlyph = "\uEB9F" },
            new LocationItem { Name = "Music", Path = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), IconGlyph = "\uE8D6" },
            new LocationItem { Name = "Videos", Path = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), IconGlyph = "\uE8B2" },
        };
    }
}

/// <summary>
/// Represents a common location (like Desktop, Documents, etc.).
/// </summary>
public class LocationItem
{
    /// <summary>
    /// Gets or sets the name of the location.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path of the location.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon glyph for the location.
    /// </summary>
    public string IconGlyph { get; set; } = string.Empty;
}
