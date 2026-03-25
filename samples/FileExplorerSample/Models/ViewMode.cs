namespace FileExplorerSample.Models;

/// <summary>
/// Represents the different view modes available in the file explorer.
/// </summary>
public enum ViewMode
{
    /// <summary>
    /// Extra large icon view (256x256).
    /// </summary>
    ExtraLargeIcons,

    /// <summary>
    /// Large icon view (96x96).
    /// </summary>
    LargeIcons,

    /// <summary>
    /// Medium icon view (48x48).
    /// </summary>
    MediumIcons,

    /// <summary>
    /// Small icon view (16x16).
    /// </summary>
    SmallIcons,

    /// <summary>
    /// List view (compact).
    /// </summary>
    List,

    /// <summary>
    /// Details view (table with columns).
    /// </summary>
    Details,

    /// <summary>
    /// Tiles view (medium icons with details).
    /// </summary>
    Tiles,

    /// <summary>
    /// Content view (large icons with details).
    /// </summary>
    Content
}
