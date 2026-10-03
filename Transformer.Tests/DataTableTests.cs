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

    private sealed class WithIndexer
    {
        public int A { get; set; } = 1;

        public int this[int i] => i;
    }

    private sealed class WithStatic
    {
        public static int Shared { get; set; } = 7;

        public int A { get; set; } = 1;
    }

    private sealed class WithoutPublicGetters
    {
        public int A { get; set; } = 1;

        public int WriteOnly { set { } }

        public int PrivateGetter { private get; set; } = 5;
    }

    private class Base
    {
        public int A { get; set; } = 1;
    }

    private sealed class Derived : Base
    {
        public new string A { get; set; } = "x";

        public int B { get; set; } = 2;
    }

    private static string[] Columns(DataTable table) => table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();

    [Fact]
    public void A_list_of_strings_converts()
    {
        DataTable table = new List<string> { "a", "bb" }.IEnumerableToDataTable();

        Assert.Equal(new[] { "Length" }, Columns(table));
        Assert.Equal(new object[] { 1, 2 }, table.Rows.Cast<DataRow>().Select(r => r["Length"]));
    }

    [Fact]
    public void An_indexer_is_not_a_column()
    {
        DataTable table = new[] { new WithIndexer() }.IEnumerableToDataTable();

        Assert.Equal(new[] { "A" }, Columns(table));
    }

    [Fact]
    public void A_static_property_is_not_a_column()
    {
        DataTable table = new[] { new WithStatic() }.IEnumerableToDataTable();

        Assert.Equal(new[] { "A" }, Columns(table));
    }

    [Fact]
    public void A_property_without_a_public_getter_is_not_a_column()
    {
        DataTable table = new[] { new WithoutPublicGetters() }.IEnumerableToDataTable();

        Assert.Equal(new[] { "A" }, Columns(table));
    }

    [Fact]
    public void A_property_hidden_with_new_gives_one_column_from_the_derived_type()
    {
        DataTable table = new[] { new Derived() }.IEnumerableToDataTable();

        Assert.Equal(new[] { "A", "B" }, Columns(table));
        Assert.Equal(typeof(string), table.Columns["A"]!.DataType);
        Assert.Equal("x", table.Rows[0]["A"]);
    }

    [Fact]
    public void A_null_element_becomes_a_row_of_DBNull()
    {
        var people = new List<Person?> { new() { Name = "Ann", Age = 31 }, null };

        DataTable table = people.IEnumerableToDataTable();

        Assert.Equal(2, table.Rows.Count);
        Assert.Equal("Ann", table.Rows[0]["Name"]);
        Assert.All(table.Rows[1].ItemArray, value => Assert.Equal(DBNull.Value, value));
    }

    [Fact]
    public void A_null_list_throws_ArgumentNullException()
    {
        var e = Assert.Throws<ArgumentNullException>(() => ((IEnumerable<Person>)null!).IEnumerableToDataTable());
        Assert.Equal("list", e.ParamName);
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
