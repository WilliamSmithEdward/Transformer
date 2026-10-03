using System.Globalization;

namespace Transformer.Tests;

/// <summary>
/// The overloads that read text with a format provider the caller chooses,
/// whatever the current culture is.
/// </summary>
public class CultureTests
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    [Fact]
    public void Text_written_in_a_fixed_format_reads_the_same_under_any_culture()
    {
        using var culture = new CultureScope("de-DE");

        Assert.Equal(1.5d, "1.5".ToNonNullableType<double>(Invariant));
        Assert.Equal(1.5m, "1.5".ToNullableType<decimal>(Invariant));
        Assert.True("1.5".IsParseable<double>(Invariant));
        Assert.Equal(new DateTime(2023, 1, 2), "01/02/2023".ToNonNullableType<DateTime>(Invariant));
        Assert.Equal(new TimeSpan(0, 1, 30, 0, 500), "1:30:00.5".ToNonNullableType<TimeSpan>(Invariant));

        // The current culture still reads it its own way.
        Assert.Equal(15d, "1.5".ToNonNullableType<double>());
    }

    [Fact]
    public void A_named_culture_reads_text_its_way_under_another_current_culture()
    {
        using var culture = new CultureScope("en-US");
        var german = new CultureInfo("de-DE");

        Assert.Equal(1.5d, "1,5".ToNonNullableType<double>(german));
        Assert.Equal(new DateTime(2023, 1, 13), "13/1/2023".ToNonNullableType<DateTime>(german));
        Assert.True("13/1/2023".IsParseable<DateTime>(german));
        Assert.False("13/1/2023".IsParseable<DateTime>());
        Assert.Equal(new DateOnly(2023, 1, 13), "13.01.2023".ToNullableType<DateOnly>(german));
    }

    [Fact]
    public void A_null_provider_means_the_current_culture()
    {
        using var culture = new CultureScope("de-DE");

        Assert.Equal(1.5d, "1,5".ToNonNullableType<double>((IFormatProvider?)null));
        Assert.True("1,5".IsParseable<decimal>((IFormatProvider?)null));
    }

    [Fact]
    public void The_overloads_keep_the_flags_of_the_methods_they_extend()
    {
        Assert.Equal(0, "x".ToNonNullableType<int>(Invariant));
        var e = Assert.Throws<InvalidCastException>(() => "1,5".ToNonNullableType<int>(Invariant, false));
        Assert.IsType<FormatException>(e.InnerException);

        Assert.Null("x".ToNullableType<int>(Invariant));
        Assert.Equal(0, "x".ToNullableType<int>(Invariant, false));
        Assert.Null(" ".ToNullableType<int>(Invariant, false));

        Assert.False(" ".IsParseable<int>(Invariant));
        Assert.True(" ".IsParseable<int>(Invariant, allowNullable: true));
    }

    [Fact]
    public void A_collection_converts_with_the_provider_given()
    {
        using var culture = new CultureScope("de-DE");
        var list = new List<string> { "1.5", "2,5", "x" };

        var result = list.ToNonNullableCollectionType<List<string>, string, List<double>, double>(Invariant);

        Assert.Equal(new[] { 1.5d, 25d }, result.TransformationSuccesses);
        Assert.Equal(new object[] { "x" }, result.TransformationFailures);
    }
}
