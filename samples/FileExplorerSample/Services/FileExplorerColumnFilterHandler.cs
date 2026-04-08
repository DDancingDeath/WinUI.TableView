using FileExplorerSample.Models;
using WinUI.TableView;

namespace FileExplorerSample.Services;

/// <summary>
/// Custom filter handler for File Explorer with alphabetic range filtering for Name column.
/// </summary>
public class FileExplorerColumnFilterHandler : ColumnFilterHandler
{
    public FileExplorerColumnFilterHandler(TableView tableView) : base(tableView)
    {
    }

    /// <inheritdoc/>
    public override IList<TableViewFilterItem> GetFilterItems(TableViewColumn column, string? searchText = null)
    {
        var columnTag = column.Tag?.ToString();

        // Use alphabetic ranges for Name column
        if (columnTag == "Name")
        {
            return GetAlphabeticRangeFilterItems(column);
        }

        // Use date ranges for Date Modified column
        if (columnTag == "DateModified")
        {
            return GetDateRangeFilterItems(column);
        }

        // Use size ranges for Size column
        if (columnTag == "Size")
        {
            return GetSizeRangeFilterItems(column);
        }

        // Use default behavior for other columns
        return base.GetFilterItems(column, searchText);
    }

    /// <summary>
    /// Gets alphabetic range filter items for the Name column.
    /// </summary>
    private IList<TableViewFilterItem> GetAlphabeticRangeFilterItems(TableViewColumn column)
    {
        var items = new List<TableViewFilterItem>();

        // Define alphabetic ranges like File Explorer
        var ranges = new[]
        {
            "A - H",
            "I - P",
            "Q - Z",
            "0 - 9 and symbols"
        };

        // Check if column is filtered and what ranges are selected
        var hasFilter = column.IsFiltered && SelectedValues.ContainsKey(column);

        foreach (var range in ranges)
        {
            var isSelected = !hasFilter || SelectedValues[column].Contains(range);
            items.Add(new TableViewFilterItem(isSelected, range, 0));
        }

        return items;
    }

    /// <summary>
    /// Gets date range filter items for the Date Modified column.
    /// </summary>
    private IList<TableViewFilterItem> GetDateRangeFilterItems(TableViewColumn column)
    {
        var items = new List<TableViewFilterItem>();

        // Define date ranges like File Explorer
        var ranges = new[]
        {
            "A long time ago",
            "Earlier this year",
            "Earlier this month",
            "Last week",
            "Yesterday",
            "Today"
        };

        // Check if column is filtered and what ranges are selected
        var hasFilter = column.IsFiltered && SelectedValues.ContainsKey(column);

        foreach (var range in ranges)
        {
            var isSelected = !hasFilter || SelectedValues[column].Contains(range);
            items.Add(new TableViewFilterItem(isSelected, range, 0));
        }

        return items;
    }

    /// <summary>
    /// Gets size range filter items for the Size column.
    /// </summary>
    private IList<TableViewFilterItem> GetSizeRangeFilterItems(TableViewColumn column)
    {
        var items = new List<TableViewFilterItem>();

        // Define size ranges like File Explorer
        var ranges = new[]
        {
            "Empty (0 KB)",
            "Tiny (0 - 16 KB)",
            "Small (16 KB - 1 MB)",
            "Medium (1 MB - 128 MB)",
            "Unspecified"
        };

        // Check if column is filtered and what ranges are selected
        var hasFilter = column.IsFiltered && SelectedValues.ContainsKey(column);

        foreach (var range in ranges)
        {
            var isSelected = !hasFilter || SelectedValues[column].Contains(range);
            items.Add(new TableViewFilterItem(isSelected, range, 0));
        }

        return items;
    }

    /// <inheritdoc/>
    public override bool Filter(TableViewColumn column, object? item)
    {
        var columnTag = column.Tag?.ToString();

        if (item is not FileSystemItem fileItem)
        {
            return base.Filter(column, item);
        }

        // Use alphabetic range filtering for Name column
        if (columnTag == "Name")
        {
            return FilterByAlphabeticRange(fileItem, column);
        }

        // Use date range filtering for Date Modified column
        if (columnTag == "DateModified")
        {
            return FilterByDateRange(fileItem, column);
        }

        // Use size range filtering for Size column
        if (columnTag == "Size")
        {
            return FilterBySizeRange(fileItem, column);
        }

        // Use default behavior for other columns
        return base.Filter(column, item);
    }

    /// <summary>
    /// Determines if an item passes the alphabetic range filter.
    /// </summary>
    private bool FilterByAlphabeticRange(FileSystemItem item, TableViewColumn column)
    {
        if (!SelectedValues.TryGetValue(column, out var selectedValues) || selectedValues.Count == 0)
        {
            return true; // No filter applied
        }

        if (string.IsNullOrEmpty(item.Name))
        {
            return selectedValues.Contains("0 - 9 and symbols");
        }

        var firstChar = char.ToUpper(item.Name[0]);

        foreach (var value in selectedValues)
        {
            var range = value?.ToString();
            var matches = range switch
            {
                "A - H" => firstChar >= 'A' && firstChar <= 'H',
                "I - P" => firstChar >= 'I' && firstChar <= 'P',
                "Q - Z" => firstChar >= 'Q' && firstChar <= 'Z',
                "0 - 9 and symbols" => !char.IsLetter(firstChar),
                _ => false
            };

            if (matches)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if an item passes the date range filter.
    /// </summary>
    private bool FilterByDateRange(FileSystemItem item, TableViewColumn column)
    {
        if (!SelectedValues.TryGetValue(column, out var selectedValues) || selectedValues.Count == 0)
        {
            return true; // No filter applied
        }

        var now = DateTime.Now;
        var dateModified = item.DateModified;

        foreach (var value in selectedValues)
        {
            var range = value?.ToString();
            var matches = range switch
            {
                "Today" => dateModified.Date == now.Date,
                "Yesterday" => dateModified.Date == now.Date.AddDays(-1),
                "Last week" => dateModified.Date >= now.Date.AddDays(-(int)now.DayOfWeek - 7) &&
                               dateModified.Date < now.Date.AddDays(-(int)now.DayOfWeek),
                "Earlier this month" => dateModified.Year == now.Year &&
                                        dateModified.Month == now.Month &&
                                        dateModified.Date < now.Date.AddDays(-7),
                "Earlier this year" => dateModified.Year == now.Year &&
                                       dateModified.Month < now.Month,
                "A long time ago" => dateModified.Year < now.Year,
                _ => false
            };

            if (matches)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if an item passes the size range filter.
    /// </summary>
    private bool FilterBySizeRange(FileSystemItem item, TableViewColumn column)
    {
        if (!SelectedValues.TryGetValue(column, out var selectedValues) || selectedValues.Count == 0)
        {
            return true; // No filter applied
        }

        // Folders fall into "Unspecified" category
        if (item.IsFolder)
        {
            return selectedValues.Contains("Unspecified");
        }

        var sizeKB = item.Size / 1024.0;
        var sizeMB = sizeKB / 1024.0;

        foreach (var value in selectedValues)
        {
            var range = value?.ToString();
            var matches = range switch
            {
                "Empty (0 KB)" => item.Size == 0,
                "Tiny (0 - 16 KB)" => sizeKB > 0 && sizeKB <= 16,
                "Small (16 KB - 1 MB)" => sizeKB > 16 && sizeMB <= 1,
                "Medium (1 MB - 128 MB)" => sizeMB > 1 && sizeMB <= 128,
                "Unspecified" => item.IsFolder,
                _ => false
            };

            if (matches)
                return true;
        }

        return false;
    }
}



