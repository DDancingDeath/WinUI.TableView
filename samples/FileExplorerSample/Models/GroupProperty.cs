namespace FileExplorerSample.Models;


/// <summary>
/// Represents the property to group by.
/// </summary>
public enum GroupProperty
{
    /// <summary>
    /// No grouping.
    /// </summary>
    None,

    /// <summary>
    /// Group by name (alphabetically).
    /// </summary>
    Name,

    /// <summary>
    /// Group by type.
    /// </summary>
    Type,

    /// <summary>
    /// Group by date modified.
    /// </summary>
    DateModified,

    /// <summary>
    /// Group by size.
    /// </summary>
    Size
}

