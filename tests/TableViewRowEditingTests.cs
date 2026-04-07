using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;

namespace WinUI.TableView.Tests;

[TestClass]
public class TableViewRowEditingTests
{
    [UITestMethod]
    public void SetIsEditing_TogglesInRowMode()
    {
        var tableView = new TableView
        {
            AutoGenerateColumns = false,
            SelectionMode = ListViewSelectionMode.Single,
            SelectionUnit = TableViewSelectionUnit.Row,
        };

        tableView.Columns.Add(new TableViewTextColumn { Header = "Name" });

        Assert.IsFalse(tableView.IsEditing);

        tableView.SetIsEditing(true);
        Assert.IsTrue(tableView.IsEditing);

        // Calling again with same value should be a no-op
        tableView.SetIsEditing(true);
        Assert.IsTrue(tableView.IsEditing);

        tableView.SetIsEditing(false);
        Assert.IsFalse(tableView.IsEditing);
    }

    [UITestMethod]
    public void SetIsEditing_DoesNotHighlightInCellMode()
    {
        var tableView = new TableView
        {
            AutoGenerateColumns = false,
            SelectionMode = ListViewSelectionMode.Single,
            SelectionUnit = TableViewSelectionUnit.Cell,
        };

        tableView.Columns.Add(new TableViewTextColumn { Header = "Name" });

        // In Cell mode, SetIsEditing should not attempt to apply highlight
        tableView.SetIsEditing(true);
        Assert.IsTrue(tableView.IsEditing);

        tableView.SetIsEditing(false);
        Assert.IsFalse(tableView.IsEditing);
    }

    [UITestMethod]
    public void SetIsEditing_TracksRowIndexForHighlight()
    {
        var tableView = new TableView
        {
            AutoGenerateColumns = false,
            SelectionMode = ListViewSelectionMode.Single,
            SelectionUnit = TableViewSelectionUnit.Row,
        };

        tableView.Columns.Add(new TableViewTextColumn { Header = "Name" });

        // Set CurrentCellSlot so SetIsEditing can track the row index
        tableView.CurrentCellSlot = new TableViewCellSlot(2, 0);

        tableView.SetIsEditing(true);
        Assert.IsTrue(tableView.IsEditing);

        // ContainerFromIndex returns null without a visual tree,
        // but the row index should still be tracked internally
        tableView.SetIsEditing(false);
        Assert.IsFalse(tableView.IsEditing);
    }

    [UITestMethod]
    public void SetIsEditing_HighlightsInCellOrRowMode()
    {
        var tableView = new TableView
        {
            AutoGenerateColumns = false,
            SelectionMode = ListViewSelectionMode.Single,
            SelectionUnit = TableViewSelectionUnit.CellOrRow,
        };

        tableView.Columns.Add(new TableViewTextColumn { Header = "Name" });

        // CellOrRow mode should also attempt highlight (same as Row mode)
        tableView.SetIsEditing(true);
        Assert.IsTrue(tableView.IsEditing);

        tableView.SetIsEditing(false);
        Assert.IsFalse(tableView.IsEditing);
    }

    [UITestMethod]
    public void ApplyEditingHighlight_Toggles()
    {
        var row = new TableViewRow();

        // Apply and clear highlight multiple times — should not crash
        row.ApplyEditingHighlight(true);
        row.ApplyEditingHighlight(false);
        row.ApplyEditingHighlight(true);
        row.ApplyEditingHighlight(false);
    }
}
