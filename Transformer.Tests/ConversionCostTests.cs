using System.Globalization;
using System.Runtime.ExceptionServices;

namespace Transformer.Tests;

/// <summary>
/// What a conversion costs: text that does not convert raises no exception
/// inside the library, and the collection method converts each element once.
/// The results are pinned against Convert.ChangeType, which the library's
/// conversions are documented to match.
/// </summary>
public class ConversionCostTests
{
    /// <summary>Counts the exceptions thrown on this thread while it is alive.</summary>
    private sealed class ExceptionCounter : IDisposable
    {
        private readonly int thread = Environment.CurrentManagedThreadId;

        public ExceptionCounter() => AppDomain.CurrentDomain.FirstChanceException += Count;

        public int Thrown { get; private set; }

        public void Dispose() => AppDomain.CurrentDomain.FirstChanceException -= Count;

        private void Count(object? sender, FirstChanceExceptionEventArgs e)
        {
            if (Environment.CurrentManagedThreadId == thread)
            {
                Thrown++;
            }
        }
    }

    [Fact]
    public void Text_that_does_not_convert_raises_no_exception_inside_the_library()
    {
        var bad = Enumerable.Range(0, 1000).Select(i => "x" + i).ToList();

        using var counter = new ExceptionCounter();
        int value = "abc".ToNonNullableType<int>();
        int? nullable = "abc".ToNullableType<int>();
        bool parseable = "300".IsParseable<byte>();
        DateTime date = "not a date".ToNonNullableType<DateTime>();
        var result = bad.ToNonNullableCollectionType<List<string>, string, List<int>, int>();
        bool name = "Purple".IsParseable<Color>();
        Guid guid = "not a guid".ToNonNullableType<Guid>();

        Assert.Equal(0, counter.Thrown);
        Assert.Equal(0, value);
        Assert.Null(nullable);
        Assert.False(parseable);
        Assert.Equal(default, date);
        Assert.Equal(1000, result.TransformationFailures.Count);
        Assert.False(name);
        Assert.Equal(Guid.Empty, guid);
    }

    [Fact]
    public void The_collection_method_converts_each_element_once()
    {
        var items = new List<Counted> { new(), new(), new() };

        var result = items.ToNonNullableCollectionType<List<Counted>, Counted, List<int>, int>();

        Assert.Equal(new[] { 7, 7, 7 }, result.TransformationSuccesses);
        Assert.All(items, item => Assert.Equal(1, item.Conversions));
    }

    public static TheoryData<string> Cultures => new() { "", "en-US", "de-DE", "fr-FR", "tr-TR", "ja-JP" };

    // A no-break space before a digit, and the Arabic-Indic digits one and two,
    // built from their code points so this file stays ASCII.
    private static readonly string NoBreakSpaceOne = (char)0x00A0 + "1";
    private static readonly string ArabicIndicTwelve = new(new[] { (char)0x0661, (char)0x0662 });

    private static readonly string[] Texts =
    {
        "0", "1", "-1", "123", " 12 ", "+7", "1.5", "1,5", "1,000", "1.000,5", "1e5", "-1e-3", "300", "-300",
        "255", "256", "65536", "2147483648", "9223372036854775808", "18446744073709551616", "NaN", "Infinity",
        "-Infinity", "true", "False", "TRUE", " true ", "yes", "x", "ab", "", " ", "\t", "0x10", "(5)", "5-",
        "$5", "1 000", NoBreakSpaceOne, "1/2/2023", "13/1/2023", "2023-01-02", "2023-01-02T03:04:05",
        "2023-01-02T03:04:05Z", "12:30", "Jan 2 2023", "1.5.2023", "abc123", ArabicIndicTwelve,
    };

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Every_conversion_of_text_matches_Convert_ChangeType(string cultureName)
    {
        using var culture = new CultureScope(cultureName);

        foreach (var text in Texts)
        {
            Same<bool>(text);
            Same<char>(text);
            Same<sbyte>(text);
            Same<byte>(text);
            Same<short>(text);
            Same<ushort>(text);
            Same<int>(text);
            Same<uint>(text);
            Same<long>(text);
            Same<ulong>(text);
            Same<float>(text);
            Same<double>(text);
            Same<decimal>(text);
            Same<DateTime>(text);
        }
    }

    private static void Same<T>(string text) where T : struct
    {
        bool expectedOk;
        T expected;
        try
        {
            expected = (T)Convert.ChangeType(text, typeof(T), CultureInfo.CurrentCulture);
            expectedOk = true;
        }
        catch (Exception)
        {
            expected = default;
            expectedOk = false;
        }

        bool blank = string.IsNullOrWhiteSpace(text);
        string where = $"{typeof(T).Name} from \"{text}\" under '{CultureInfo.CurrentCulture.Name}'";
        Assert.True(expected.Equals(text.ToNonNullableType<T>()), where);
        Assert.True((expectedOk && !blank) == text.IsParseable<T>(), where);
        Assert.True(Nullable.Equals(expectedOk && !blank ? (T?)expected : null, text.ToNullableType<T>()), where);
        Assert.True(Nullable.Equals(blank ? null : (T?)expected, text.ToNullableType<T>(false)), where);
    }

    /// <summary>A value that converts to 7 and counts how often it is converted.</summary>
    public sealed class Counted : IConvertible
    {
        public int Conversions { get; private set; }

        public TypeCode GetTypeCode() => TypeCode.Object;

        public int ToInt32(IFormatProvider? provider)
        {
            Conversions++;
            return 7;
        }

        public override string ToString() => "7";

        public string ToString(IFormatProvider? provider) => ToString();

        public bool ToBoolean(IFormatProvider? provider) => throw new InvalidCastException();

        public byte ToByte(IFormatProvider? provider) => throw new InvalidCastException();

        public char ToChar(IFormatProvider? provider) => throw new InvalidCastException();

        public DateTime ToDateTime(IFormatProvider? provider) => throw new InvalidCastException();

        public decimal ToDecimal(IFormatProvider? provider) => throw new InvalidCastException();

        public double ToDouble(IFormatProvider? provider) => throw new InvalidCastException();

        public short ToInt16(IFormatProvider? provider) => throw new InvalidCastException();

        public long ToInt64(IFormatProvider? provider) => throw new InvalidCastException();

        public sbyte ToSByte(IFormatProvider? provider) => throw new InvalidCastException();

        public float ToSingle(IFormatProvider? provider) => throw new InvalidCastException();

        public object ToType(Type conversionType, IFormatProvider? provider) => throw new InvalidCastException();

        public ushort ToUInt16(IFormatProvider? provider) => throw new InvalidCastException();

        public uint ToUInt32(IFormatProvider? provider) => throw new InvalidCastException();

        public ulong ToUInt64(IFormatProvider? provider) => throw new InvalidCastException();
    }
}
