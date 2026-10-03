using System.Data;

namespace Transformer.Tests;

/// <summary>
/// IEnumerableToDataTable and ToConsoleString.
/// </summary>
public class DataTableTests
{
    private static readonly string NL = Environment.NewLine;

    [Fact]
    public void Each_public_property_becomes_a_column_of_its_type_and_each_element_a_row()
    {
        var people = new List<Person>
        {
            new() { Name = "Ann", Age = 31, Score = 1.5 },
            new() { Name = "Bob", Age = 4 },
        };

        DataTable table = people.IEnumerableToDataTable();

        Assert.Equal(typeof(Person).FullName, table.TableName);
        Assert.Equal(new[] { "Name", "Age", "Score" }, table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
        Assert.Equal(new[] { typeof(string), typeof(int), typeof(double) },
            table.Columns.Cast<DataColumn>().Select(c => c.DataType));
        Assert.Equal(2, table.Rows.Count);
        Assert.Equal("Ann", table.Rows[0]["Name"]);
        Assert.Equal(31, table.Rows[0]["Age"]);
        Assert.Equal(1.5, table.Rows[0]["Score"]);
        Assert.Equal(DBNull.Value, table.Rows[1]["Score"]);
    }

    [Fact]
    public void The_columns_come_from_the_declared_element_type()
    {
        IEnumerable<object> items = new object[] { new Person() };

        Assert.Empty(items.IEnumerableToDataTable().Columns);
    }

    [Fact]
    public void ToConsoleString_prints_the_README_table()
    {
        var people = new List<Person>
        {
            new() { Name = "Ann", Age = 31 },
            new() { Name = "Bob", Age = 4 },
        };

        string text = people.Select(p => new { p.Name, p.Age }).IEnumerableToDataTable().ToConsoleString();

        Assert.Equal(
            "--------------" + NL +
            "| Name | Age |" + NL +
            "--------------" + NL +
            "| Ann  | 31  |" + NL +
            "| Bob  | 4   |" + NL +
            "--------------" + NL,
            text);
    }

    [Fact]
    public void ToConsoleString_writes_values_in_the_current_culture()
    {
        using var culture = new CultureScope("de-DE");

        string text = new[] { new { D = 1.5 } }.IEnumerableToDataTable().ToConsoleString();

        Assert.Contains("| 1,5 |", text);
    }

    [Fact]
    public void ToConsoleString_of_null_throws_ArgumentNullException()
    {
        var e = Assert.Throws<ArgumentNullException>(() => ((DataTable)null!).ToConsoleString());
        Assert.Equal("table", e.ParamName);
    }
}
