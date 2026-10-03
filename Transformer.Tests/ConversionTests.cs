namespace Transformer.Tests;

/// <summary>
/// ToNonNullableType and ToNullableType, as the READMEs document them.
/// </summary>
public class ConversionTests
{
    [Fact]
    public void ToNonNullableType_converts_text_and_numbers()
    {
        using var culture = new CultureScope("en-US");

        Assert.Equal(123, "123".ToNonNullableType<int>());
        Assert.Equal(1f, 1.ToNonNullableType<float>());
        Assert.Equal(new DateTime(2023, 1, 2), "2023-01-02".ToNonNullableType<DateTime>());
        Assert.Equal(12, " 12 ".ToNonNullableType<int>());
        Assert.Equal(1e5, "1e5".ToNonNullableType<double>());
        Assert.Equal('x', "x".ToNonNullableType<char>());
        Assert.Equal(' ', " ".ToNonNullableType<char>());
    }

    [Fact]
    public void ToNonNullableType_returns_the_default_when_the_value_does_not_convert()
    {
        Assert.Equal(0, "123A".ToNonNullableType<int>());
        Assert.Equal(default(DateTime), "not a date".ToNonNullableType<DateTime>());
        Assert.Equal(0, ((object)null!).ToNonNullableType<int>());
        Assert.Equal(0, DBNull.Value.ToNonNullableType<int>());
        Assert.Equal(0, "".ToNonNullableType<int>());
        Assert.Equal('\0', "ab".ToNonNullableType<char>());
    }

    [Fact]
    public void ToNonNullableType_with_false_throws_InvalidCastException_naming_the_type()
    {
        var e = Assert.Throws<InvalidCastException>(() => "123A".ToNonNullableType<int>(false));
        Assert.Equal("Cannot convert value to type System.Int32.", e.Message);
        Assert.Throws<InvalidCastException>(() => ((object)null!).ToNonNullableType<int>(false));
    }

    [Fact]
    public void A_value_outside_the_range_of_the_type_does_not_convert()
    {
        Assert.Equal((byte)0, "300".ToNonNullableType<byte>());
        Assert.Equal(0, 1e20.ToNonNullableType<int>());
        Assert.Null("300".ToNullableType<byte>());
        Assert.False("300".IsParseable<byte>());
    }

    [Fact]
    public void Whole_number_types_take_whole_numbers_only_and_a_double_rounds_to_even()
    {
        using var culture = new CultureScope("en-US");

        Assert.Equal(0, "1.5".ToNonNullableType<int>());
        Assert.Equal(0, "1,000".ToNonNullableType<int>());
        Assert.Equal(2, 1.5.ToNonNullableType<int>());
        Assert.Equal(2, 2.5.ToNonNullableType<int>());
        Assert.Equal(4, 3.5.ToNonNullableType<int>());
    }

    [Fact]
    public void Text_converts_to_bool_only_as_true_or_false()
    {
        Assert.True("true".ToNonNullableType<bool>());
        Assert.True("TRUE".ToNonNullableType<bool>());
        Assert.False("False".ToNonNullableType<bool>(false));
        Assert.False("1".IsParseable<bool>());
        Assert.False("yes".IsParseable<bool>());
        Assert.True(1.ToNonNullableType<bool>());
        Assert.False(0.ToNonNullableType<bool>());
    }

    [Fact]
    public void Text_is_read_in_the_current_culture()
    {
        using (new CultureScope("en-US"))
        {
            Assert.Equal(15d, "1,5".ToNonNullableType<double>());
            Assert.Equal(1.5d, "1.5".ToNonNullableType<double>());
            Assert.Equal(new DateTime(2023, 1, 2), "1/2/2023".ToNonNullableType<DateTime>());
            Assert.Equal(default(DateTime), "13/1/2023".ToNonNullableType<DateTime>());
        }

        using (new CultureScope("de-DE"))
        {
            Assert.Equal(1.5d, "1,5".ToNonNullableType<double>());
            Assert.Equal(15d, "1.5".ToNonNullableType<double>());
            Assert.Equal(1.5m, "1,5".ToNonNullableType<decimal>());
            Assert.Equal(new DateTime(2023, 2, 1), "1/2/2023".ToNonNullableType<DateTime>());
            Assert.Equal(new DateTime(2023, 1, 13), "13/1/2023".ToNonNullableType<DateTime>());
        }

        using (new CultureScope("fr-FR"))
        {
            Assert.Equal(1.5d, "1,5".ToNonNullableType<double>());
            Assert.Equal(0d, "1.5".ToNonNullableType<double>());
        }
    }

    [Fact]
    public void ToNullableType_returns_null_for_a_blank_value_whatever_the_flag()
    {
        foreach (var blank in new object?[] { null, DBNull.Value, "", "   " })
        {
            Assert.Null(blank.ToNullableType<int>());
            Assert.Null(blank.ToNullableType<int>(false));
        }

        Assert.Null(" ".ToNullableType<char>());
    }

    [Fact]
    public void ToNullableType_returns_null_or_the_default_for_a_value_that_does_not_convert()
    {
        Assert.Null("123A".ToNullableType<int>());

        int? zero = "123A".ToNullableType<int>(false);
        Assert.True(zero.HasValue);
        Assert.Equal(0, zero);
        Assert.Equal(default(DateTime), "x".ToNullableType<DateTime>(false));
    }

    [Fact]
    public void ToNullableType_converts_a_value_that_converts_whatever_the_flag()
    {
        Assert.Equal(123, "123".ToNullableType<int>());
        Assert.Equal(123, "123".ToNullableType<int>(false));
        Assert.Equal(4.5m, 4.5.ToNullableType<decimal>());
    }
}
