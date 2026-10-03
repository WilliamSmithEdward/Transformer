namespace Transformer.Tests;

/// <summary>
/// ToNonNullableCollectionType and CollectionTransformResult.
/// </summary>
public class CollectionTests
{
    [Fact]
    public void The_README_sample_splits_successes_from_failures_in_order()
    {
        var list = new List<string> { "1", "bob", "apple", "2", "3" };

        var result = list.ToNonNullableCollectionType<List<string>, string, List<int>, int>();

        Assert.Equal(new[] { 1, 2, 3 }, result.TransformationSuccesses);
        Assert.Equal(new object[] { "bob", "apple" }, result.TransformationFailures);
    }

    [Fact]
    public void Blank_strings_are_failures_and_null_elements_are_in_neither_list()
    {
        var list = new List<string?> { "1", null, "", " ", "x", "2" };

        var result = list.ToNonNullableCollectionType<List<string?>, string?, List<int>, int>();

        Assert.Equal(new[] { 1, 2 }, result.TransformationSuccesses);
        Assert.Equal(new object[] { "", " ", "x" }, result.TransformationFailures);
    }

    [Fact]
    public void The_target_collection_decides_what_is_kept()
    {
        var list = new List<string> { "1", "1", "2" };

        var result = list.ToNonNullableCollectionType<List<string>, string, HashSet<int>, int>();

        Assert.Equal(new HashSet<int> { 1, 2 }, result.TransformationSuccesses);
        Assert.Empty(result.TransformationFailures);
    }

    [Fact]
    public void An_empty_collection_gives_two_empty_lists()
    {
        var result = new List<string>().ToNonNullableCollectionType<List<string>, string, List<int>, int>();

        Assert.Empty(result.TransformationSuccesses);
        Assert.Empty(result.TransformationFailures);
    }

    [Fact]
    public void CollectionTransformResult_holds_what_it_is_given()
    {
        var successes = new List<int> { 1 };
        var failures = new List<object> { "x" };

        var result = new CollectionTransformResult<List<int>, int>(successes, failures);

        Assert.Same(successes, result.TransformationSuccesses);
        Assert.Same(failures, result.TransformationFailures);
    }

    [Fact]
    public void Collection_conversion_reads_text_in_the_current_culture()
    {
        using var culture = new CultureScope("de-DE");
        var list = new List<string> { "1,5", "2.5" };

        var result = list.ToNonNullableCollectionType<List<string>, string, List<double>, double>();

        Assert.Equal(new[] { 1.5d, 25d }, result.TransformationSuccesses);
    }
}
