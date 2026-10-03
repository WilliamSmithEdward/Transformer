namespace Transformer.Tests;

/// <summary>
/// Round and ToTitleCase.
/// </summary>
public class TextTests
{
    [Fact]
    public void Round_on_a_double_sends_a_midpoint_to_the_even_number()
    {
        Assert.Equal(3.14, 3.14159.Round(2));
        Assert.Equal(2d, 2.5.Round(0));
        Assert.Equal(4d, 3.5.Round(0));
    }

    [Fact]
    public void Round_on_a_float_rounds_the_value_as_a_double()
    {
        Assert.Equal(1.2f, 1.25f.Round(1));
        Assert.Equal(2f, 2.5f.Round(0));
        Assert.Equal(1f, 1.005f.Round(2));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(16)]
    public void Round_refuses_digits_outside_0_to_15(int digits)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 1.5.Round(digits));
        Assert.Throws<ArgumentOutOfRangeException>(() => 1.5f.Round(digits));
    }

    [Fact]
    public void ToTitleCase_lower_cases_then_capitalizes_each_word()
    {
        Assert.Equal("Hello Nasa", "hello NASA".ToTitleCase());
        Assert.Equal("O'neil Mcdonald-Smith", "o'neil mcdonald-smith".ToTitleCase());
        Assert.Equal("", "".ToTitleCase());
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("az-Latn-AZ")]
    [InlineData("de-DE")]
    public void ToTitleCase_gives_the_same_result_under_any_culture(string culture)
    {
        using var scope = new CultureScope(culture);

        Assert.Equal("Title Istanbul", "TITLE ISTANBUL".ToTitleCase());
        Assert.Equal("Hello Nasa", "hello NASA".ToTitleCase());
    }

    [Fact]
    public void ToTitleCase_of_null_throws_ArgumentNullException()
    {
        var e = Assert.Throws<ArgumentNullException>(() => ((string)null!).ToTitleCase());
        Assert.Equal("s", e.ParamName);
    }
}
