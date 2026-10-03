namespace Transformer.Tests;

/// <summary>
/// IsParseable, as the READMEs document it.
/// </summary>
public class ParseTests
{
    [Fact]
    public void IsParseable_says_whether_the_conversion_would_succeed()
    {
        Assert.True("123".IsParseable<int>());
        Assert.True(" 12 ".IsParseable<int>());
        Assert.False("123A".IsParseable<int>());
        Assert.True("2023-01-02".IsParseable<DateTime>());
        Assert.True(1.IsParseable<float>());
    }

    [Fact]
    public void A_blank_value_is_not_parseable_unless_allowNullable_is_true()
    {
        foreach (var blank in new object?[] { null, DBNull.Value, "", "   " })
        {
            Assert.False(blank.IsParseable<int>());
            Assert.True(blank.IsParseable<int>(allowNullable: true));
        }

        Assert.False(" ".IsParseable<char>());
    }

    [Fact]
    public void IsParseable_reads_text_in_the_current_culture()
    {
        using (new CultureScope("de-DE"))
        {
            Assert.True("1,5".IsParseable<decimal>());
            Assert.True("13/1/2023".IsParseable<DateTime>());
        }

        using (new CultureScope("en-US"))
        {
            Assert.False("13/1/2023".IsParseable<DateTime>());
        }
    }
}
