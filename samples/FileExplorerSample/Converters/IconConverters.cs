using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace FileExplorerSample.Converters;

/// <summary>
/// Converts a file extension or icon type string to the corresponding SVG PathGeometry resource key.
/// </summary>
public class IconPathConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string iconType)
            return null;

        return iconType switch
        {
            "Folder" => "FolderIconPath",
            ".txt" or ".doc" or ".docx" => "DocumentIconPath",
            ".xls" or ".xlsx" => "ExcelIconPath",
            ".pdf" => "PDFIconPath",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "ImageIconPath",
            ".mp3" or ".wav" or ".wma" => "AudioIconPath",
            ".mp4" or ".avi" or ".mkv" => "VideoIconPath",
            ".zip" or ".rar" or ".7z" => "ArchiveIconPath",
            ".exe" => "ApplicationIconPath",
            ".cs" => "CodeIconPath",
            ".xaml" or ".xml" => "XMLIconPath",
            _ => "DefaultFileIconPath"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a boolean to a Brush for icon coloring.
/// </summary>
public class IconColorConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isFolder && isFolder)
        {
            // Windows Explorer folder color: #FFCA5010 (orange-brown)
            return new SolidColorBrush(Color.FromArgb(255, 202, 80, 16));
        }

        // Default file icon color: subtle gray
        return new SolidColorBrush(Color.FromArgb(255, 96, 94, 92));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts an icon path resource key string to the actual PathGeometry from resources.
/// </summary>
public class PathGeometryResourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string resourceKey || string.IsNullOrEmpty(resourceKey))
            return null;

        try
        {
            if (Application.Current.Resources.TryGetValue(resourceKey, out var resource))
            {
                // If it's already a Geometry, return it directly
                if (resource is Geometry geometry)
                {
                    return geometry;
                }

                // If it's a string, try to parse it
                if (resource is string pathData)
                {
                    var xaml = $"<Path xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Data='{pathData}'/>";
                    if (XamlReader.Load(xaml) is Microsoft.UI.Xaml.Shapes.Path path)
                    {
                        return path.Data;
                    }
                }
            }
        }
        catch
        {
            // Fallback: return null if resource not found
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a color hex string to a SolidColorBrush.
/// </summary>
public class ColorStringToBrushConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not string colorString || string.IsNullOrEmpty(colorString))
            return new SolidColorBrush(Color.FromArgb(255, 96, 94, 92)); // Default gray

        try
        {
            // Remove # if present
            colorString = colorString.TrimStart('#');

            // Parse hex color
            if (colorString.Length == 6)
            {
                var r = System.Convert.ToByte(colorString.Substring(0, 2), 16);
                var g = System.Convert.ToByte(colorString.Substring(2, 2), 16);
                var b = System.Convert.ToByte(colorString.Substring(4, 2), 16);
                return new SolidColorBrush(Color.FromArgb(255, r, g, b));
            }
            else if (colorString.Length == 8)
            {
                var a = System.Convert.ToByte(colorString.Substring(0, 2), 16);
                var r = System.Convert.ToByte(colorString.Substring(2, 2), 16);
                var g = System.Convert.ToByte(colorString.Substring(4, 2), 16);
                var b = System.Convert.ToByte(colorString.Substring(6, 2), 16);
                return new SolidColorBrush(Color.FromArgb(a, r, g, b));
            }
        }
        catch
        {
            // Fallback to default gray
        }

        return new SolidColorBrush(Color.FromArgb(255, 96, 94, 92));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

