# Transformer

Transformer is a set of extension methods for .NET that convert values from one type to another: text to numbers, dates and booleans, one value or a whole collection at a time, and a list of objects to a `DataTable` you can print.

```
dotnet add package WilliamSmithE.Transformer
```

Everything is in the `Transformer` namespace. The package targets net8.0, net9.0 and net10.0 and has no dependencies.

---

## Convert a value

```csharp
using Transformer;

int number = "123".ToNonNullableType<int>();
int failed = "123A".ToNonNullableType<int>();
float single = 1.ToNonNullableType<float>();
DateTime date = "2023-01-02".ToNonNullableType<DateTime>();

Console.WriteLine(number);                       // 123
Console.WriteLine(failed);                       // 0
Console.WriteLine(single);                       // 1
Console.WriteLine(date.ToString("yyyy-MM-dd"));  // 2023-01-02

int? missing = "123A".ToNullableType<int>();
int? zero = "123A".ToNullableType<int>(false);
int? blank = "  ".ToNullableType<int>(false);

Console.WriteLine(missing is null);  // True
Console.WriteLine(zero);             // 0
Console.WriteLine(blank is null);    // True

try
{
    "123A".ToNonNullableType<int>(false);
}
catch (InvalidCastException e)
{
    Console.WriteLine(e.Message);                         // Cannot convert value to type System.Int32.
    Console.WriteLine(e.InnerException?.GetType().Name);  // FormatException
}
```

## Check before converting

```csharp
using Transformer;

string? text = "123A";
string? nothing = null;

Console.WriteLine("123".IsParseable<int>());                       // True
Console.WriteLine(text.IsParseable<int>());                        // False
Console.WriteLine(nothing.IsParseable<int>());                     // False
Console.WriteLine(nothing.IsParseable<int>(allowNullable: true));  // True
```

## Enums, Guid, TimeSpan and other value types

```csharp
using Transformer;

Console.WriteLine("friday".ToNonNullableType<DayOfWeek>());     // Friday
Console.WriteLine(5.ToNonNullableType<DayOfWeek>());            // Friday
Console.WriteLine("Funday".IsParseable<DayOfWeek>());           // False
Console.WriteLine("01:30:00".ToNonNullableType<TimeSpan>());    // 01:30:00
Console.WriteLine("2023-01-02".ToNonNullableType<DateOnly>().ToString("yyyy-MM-dd"));  // 2023-01-02
Console.WriteLine("0f8fad5b-d9cb-469f-a165-70867728950e".IsParseable<Guid>());          // True
```

## Convert a collection

```csharp
using Transformer;

var list = new List<string> { "1", "bob", "apple", "2", "3" };

var result = list.ToNonNullableCollectionType<List<string>, string, List<int>, int>();

Console.WriteLine(string.Join(", ", result.TransformationSuccesses));  // 1, 2, 3
Console.WriteLine(string.Join(", ", result.TransformationFailures));   // bob, apple
```

The four type arguments are the source collection, its element type, the collection to fill and its element type. The result is a `CollectionTransformResult<TCollectionNew, TNewType>`: `TransformationSuccesses` holds the converted values in a new `TCollectionNew`, and `TransformationFailures` the source elements that did not convert, as a `List<object>`.

## Make a DataTable and print it

```csharp
using System.Data;
using Transformer;

var people = new List<Person>
{
    new() { Name = "Ann", Age = 31 },
    new() { Name = "Bob", Age = 4 },
};

DataTable table = people.IEnumerableToDataTable();
Console.WriteLine(table.Columns[1].DataType);  // System.Int32
Console.Write(table.ToConsoleString());
// --------------
// | Name | Age |
// --------------
// | Ann  | 31  |
// | Bob  | 4   |
// --------------

class Person
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
}
```

## Round a number and title-case a string

```csharp
using Transformer;

Console.WriteLine(3.14159.Round(2));             // 3.14
Console.WriteLine(2.5.Round(0));                 // 2
Console.WriteLine(1.25f.Round(1));               // 1.2
Console.WriteLine("hello NASA".ToTitleCase());   // Hello Nasa
```

---

## How values convert

`ToNonNullableType<T>`, `ToNullableType<T>`, `IsParseable<T>` and `ToNonNullableCollectionType` convert in the same way. `T` must be a value type. For the types `Convert.ChangeType` knows, the numeric types, `bool`, `char` and `DateTime`, they give the result `Convert.ChangeType(value, typeof(T))` gives. They read text with the target type's own `TryParse`, so text that does not convert costs no exception.

- Text is read in the current culture, so the same string can give different numbers and dates on different machines. Each of the four methods has an overload whose first argument is an `IFormatProvider` that reads text instead: pass `CultureInfo.InvariantCulture` for text written in a fixed format, such as a file or another program's output, or a named culture for text typed in that culture. `null` there means the current culture.

  ```csharp
  using System.Globalization;
  using Transformer;

  CultureInfo.CurrentCulture = new CultureInfo("en-US");
  Console.WriteLine("1,5".ToNonNullableType<double>());  // 15
  Console.WriteLine("1/2/2023".ToNonNullableType<DateTime>().ToString("yyyy-MM-dd"));  // 2023-01-02

  CultureInfo.CurrentCulture = new CultureInfo("de-DE");
  Console.WriteLine("1.5".ToNonNullableType<double>());  // 15
  Console.WriteLine("1/2/2023".ToNonNullableType<DateTime>().ToString("yyyy-MM-dd"));  // 2023-02-01

  var invariant = CultureInfo.InvariantCulture;
  Console.WriteLine("1.5".ToNonNullableType<double>(invariant).ToString(invariant));  // 1.5
  Console.WriteLine("1.5".IsParseable<decimal>(invariant));                           // True
  Console.WriteLine("x".ToNullableType<int>(invariant, false));                       // 0

  var list = new List<string> { "1.5", "2.5" };
  var result = list.ToNonNullableCollectionType<List<string>, string, List<double>, double>(invariant);
  Console.WriteLine(result.TransformationSuccesses.Sum().ToString(invariant));        // 4
  ```

- Text converts to a whole-number type only when it is a whole number: "1.5" and "1,000" do not convert to `int`. A `double` does convert, rounded to the nearest even number, so 1.5 and 2.5 both give 2.
- A value outside the range of `T`, such as "300" for `byte`, does not convert.
- Text converts to `bool` only as "true" or "false", in any case. "1" and "yes" do not convert; the number 1 converts to `true`.
- `null` and `DBNull.Value` do not convert.
- An enum converts from its name, in any case, or from a whole number, as text or as a value of an integer type. Like any enum, it takes a number it has no name for: "42" gives `(DayOfWeek)42`. A name it does not have, a fraction, a number too large for it and a value of another enum type do not convert.
- Other value types with a public static `TryParse(string, IFormatProvider, out T)`, such as `Guid`, `TimeSpan`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `Int128` and `BigInteger`, convert from text, read by that `TryParse` in the current culture, and from a value that already has the type. A number does not convert to them.
- `ToNonNullableType<T>()` returns `default(T)` (0, `false`, `DateTime.MinValue`) when the value does not convert. `ToNonNullableType<T>(false)` throws `InvalidCastException` instead, whose `InnerException` is the cause: a `FormatException` for text that is not a number of that type, an `OverflowException` for a value out of its range, an `ArgumentException` for a name an enum does not have, or an `InvalidCastException` for `null` or a type that does not convert.
- `ToNullableType<T>()` returns `null` for `null`, `DBNull.Value`, an empty string and a string of spaces, whatever its argument. For any other value that does not convert it returns `null`, or `default(T)` with `ToNullableType<T>(false)`.
- `IsParseable<T>()` returns whether the conversion would succeed. It returns `false` for `null`, `DBNull.Value`, an empty string and a string of spaces, and `true` for them with `allowNullable: true`. Because a space counts as blank, `" ".IsParseable<char>()` is `false` and `" ".ToNullableType<char>()` is `null`, while `" ".ToNonNullableType<char>()` returns the space.

## Collections

- `ToNonNullableCollectionType` keeps the source order. `TCollectionNew` needs a public parameterless constructor, and its `Add` decides what is kept: a `HashSet<int>` keeps one of each value.
- An empty string or a string of spaces does not convert, so it goes to `TransformationFailures`. A `null` element is left out of both lists.

## DataTables

- `IEnumerableToDataTable<T>()` makes one column for each public instance property of `T` that has a public getter, in the order reflection returns them, which is in practice the order they are declared. Static properties, indexers and properties without a public getter are left out, and a property hidden with `new` gives one column, from the most derived type. The column is named after the property and has its type, or the underlying type for a `Nullable<>` property, whose `null` becomes `DBNull.Value`.
- Each element becomes one row; a `null` element becomes a row of `DBNull.Value`. A `null` list throws `ArgumentNullException`.
- The columns come from `T`, the element type the list is declared with, not from the type of each element. The table is named after `T`'s full name. A list of strings gives one column, `Length`, and a list of numbers gives none, because those are the types' public properties.
- `ToConsoleString()` writes the table as text: a header row of column names and one row per data row, each column as wide as its longest value, values written with `ToString()` in the current culture. A value with a line break in it breaks the layout. A `null` table throws `ArgumentNullException`.

## Rounding and text

- `Round(digits)` on a `double` is `Math.Round(value, digits)`: a value halfway between two results goes to the even one, so `2.5.Round(0)` is 2 and `3.5.Round(0)` is 4.
- On a `float` it rounds the value as a `double` and converts back. A `float` holds most decimals inexactly, so `1.005f.Round(2)` is 1, because 1.005f is stored as 1.00499999523.
- `digits` must be 0 to 15, or `Round` throws `ArgumentOutOfRangeException`.
- `ToTitleCase()` lower-cases the whole string, then capitalizes the first letter of each word, so acronyms are lower-cased too: "hello NASA" gives "Hello Nasa", and "o'neil mcdonald-smith" gives "O'neil Mcdonald-Smith".

## Known problems in 1.0.0.5

- `ToTitleCase` lower-cases in the current culture, so under Turkish (tr-TR) "TITLE" gives "Title" spelled with a dotless i (U+0131). A `null` string throws `NullReferenceException`, where the XML docs promise `ArgumentNullException`.

## Attributions

Icon "Transformer.png" designed by Icon home on Freepik.com.

## License

[MIT](https://github.com/WilliamSmithEdward/Transformer/blob/main/LICENSE.txt)
