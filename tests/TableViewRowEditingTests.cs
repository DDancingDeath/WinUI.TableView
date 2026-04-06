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
