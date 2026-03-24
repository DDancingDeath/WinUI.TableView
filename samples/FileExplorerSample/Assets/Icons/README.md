# Icon Assets

This directory contains SVG icon files for the File Explorer sample application.

## Available Icons

The following SVG icons are available:

1. **folder.svg** - Folder icon (Orange-brown color: #FFCA5010)
2. **document.svg** - Document files (.txt, .doc, .docx)
3. **excel.svg** - Excel spreadsheets (.xls, .xlsx) - Green color: #217346
4. **pdf.svg** - PDF documents (.pdf) - Red color: #D83B01
5. **image.svg** - Image files (.jpg, .png, .gif, .bmp) - Blue color: #0078D4
6. **audio.svg** - Audio files (.mp3, .wav, .wma) - Purple color: #8764B8
7. **video.svg** - Video files (.mp4, .avi, .mkv) - Cyan color: #00B7C3
8. **archive.svg** - Archive files (.zip, .rar, .7z) - Yellow color: #FFB900
9. **application.svg** - Application files (.exe) - Red color: #E81123
10. **code.svg** - Code files (.cs) - Purple color: #68217A
11. **xml.svg** - XML/XAML files (.xaml, .xml) - Teal color: #008272
12. **file.svg** - Default file icon - Gray color: #605E5C

## Usage

These SVG files are provided as reference. The actual icon rendering uses the path data embedded in `Resources\IconResources.xaml` as string resources. The `PathGeometryResourceConverter` in `Converters\IconConverters.cs` converts these path data strings to geometry for rendering.

## Icon Sources

The icons are based on Material Design icons, adapted for the File Explorer sample application.
