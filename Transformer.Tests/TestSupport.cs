using System.Globalization;

namespace Transformer.Tests;

/// <summary>
/// Sets the current culture, and the UI culture with it, until disposed.
/// Conversions read text in the current culture, so a test that depends on
/// one sets it rather than inheriting the machine's.
/// </summary>
public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo culture;
    private readonly CultureInfo uiCulture;

    public CultureScope(string name)
    {
        culture = CultureInfo.CurrentCulture;
        uiCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentCulture = new CultureInfo(name);
        CultureInfo.CurrentUICulture = new CultureInfo(name);
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = uiCulture;
    }
}

public enum Color
{
    Red,
    Green,
    Blue,
}

public class Person
{
    public string Name { get; set; } = "";

    public int Age { get; set; }

    public double? Score { get; set; }
}
