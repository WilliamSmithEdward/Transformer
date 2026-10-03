using System.Numerics;

namespace Transformer.Tests;

/// <summary>
/// Value types Convert.ChangeType does not convert to: enums, and types such as
/// Guid, TimeSpan and DateOnly that read text through their own TryParse.
/// </summary>
public class ValueTypeTests
{
    [Fact]
    public void An_enum_converts_from_its_name_in_any_case_and_from_its_number()
    {
        Assert.Equal(Color.Green, "Green".ToNonNullableType<Color>());
        Assert.Equal(Color.Green, " green ".ToNonNullableType<Color>());
        Assert.Equal(Color.Blue, "2".ToNonNullableType<Color>());
        Assert.Equal(Color.Blue, 2.ToNonNullableType<Color>());
        Assert.Equal(Color.Blue, 2L.ToNonNullableType<Color>());
        Assert.Equal(Color.Blue, ((byte)2).ToNonNullableType<Color>());
        Assert.Equal(Color.Blue, Color.Blue.ToNonNullableType<Color>());
        Assert.Equal(Color.Green, "GREEN".ToNullableType<Color>());
        Assert.True("green".IsParseable<Color>());
    }

    [Fact]
    public void An_enum_takes_a_number_it_does_not_name_as_enums_do()
    {
        Assert.Equal((Color)42, "42".ToNonNullableType<Color>());
        Assert.Equal((Color)42, 42.ToNonNullableType<Color>());
    }

    [Fact]
    public void An_enum_refuses_a_name_it_does_not_have_a_fraction_and_another_enum()
    {
        Assert.Equal(Color.Red, "Purple".ToNonNullableType<Color>());
        Assert.Null("Purple".ToNullableType<Color>());
        Assert.False("Purple".IsParseable<Color>());
        Assert.False(1.5.IsParseable<Color>());
        Assert.False(DayOfWeek.Monday.IsParseable<Color>());
        Assert.Equal(1, Color.Green.ToNonNullableType<int>());

        var e = Assert.Throws<InvalidCastException>(() => "Purple".ToNonNullableType<Color>(false));
        Assert.Equal($"Cannot convert value to type {typeof(Color)}.", e.Message);
        Assert.IsType<ArgumentException>(e.InnerException);
    }

    [Fact]
    public void A_collection_of_enum_names_converts()
    {
        var names = new List<string> { "Red", "blue", "Purple" };

        var result = names.ToNonNullableCollectionType<List<string>, string, List<Color>, Color>();

        Assert.Equal(new[] { Color.Red, Color.Blue }, result.TransformationSuccesses);
        Assert.Equal(new object[] { "Purple" }, result.TransformationFailures);
    }

    [Fact]
    public void Value_types_with_their_own_TryParse_convert_from_text()
    {
        using var culture = new CultureScope("en-US");
        var guid = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e");

        Assert.Equal(guid, "0f8fad5b-d9cb-469f-a165-70867728950e".ToNonNullableType<Guid>());
        Assert.Equal(new TimeSpan(1, 30, 0), "01:30:00".ToNonNullableType<TimeSpan>());
        Assert.Equal(new DateOnly(2023, 1, 2), "2023-01-02".ToNonNullableType<DateOnly>());
        Assert.Equal(new TimeOnly(13, 45), "13:45".ToNonNullableType<TimeOnly>());
        Assert.Equal(new DateTimeOffset(2023, 1, 2, 0, 0, 0, TimeSpan.FromHours(2)),
            "2023-01-02T00:00:00+02:00".ToNonNullableType<DateTimeOffset>());
        Assert.Equal(Int128.Parse("170141183460469231731687303715884105727"),
            "170141183460469231731687303715884105727".ToNonNullableType<Int128>());
        Assert.Equal(new BigInteger(12345), "12345".ToNullableType<BigInteger>());
        Assert.True("2023-01-02".IsParseable<DateOnly>());
        Assert.Equal(guid, guid.ToNonNullableType<Guid>());
    }

    [Fact]
    public void Value_types_with_their_own_TryParse_read_text_in_the_current_culture()
    {
        using var culture = new CultureScope("de-DE");

        Assert.Equal(new DateOnly(2023, 1, 13), "13.01.2023".ToNonNullableType<DateOnly>());
        Assert.Equal(new TimeSpan(0, 1, 30, 0, 500), "1:30:00,5".ToNonNullableType<TimeSpan>());
    }

    [Fact]
    public void Value_types_with_their_own_TryParse_refuse_text_that_does_not_parse()
    {
        Assert.Equal(Guid.Empty, "not a guid".ToNonNullableType<Guid>());
        Assert.Null("x".ToNullableType<TimeSpan>());
        Assert.Equal(default(TimeSpan), "x".ToNullableType<TimeSpan>(false));
        Assert.False("x".IsParseable<DateOnly>());
        Assert.False(5.IsParseable<Int128>());

        var e = Assert.Throws<InvalidCastException>(() => "x".ToNonNullableType<Guid>(false));
        Assert.IsType<FormatException>(e.InnerException);
    }
}
