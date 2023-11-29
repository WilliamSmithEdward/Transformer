## Basic Usage Examples

### Convert from a string to an int
```csharp
using Transformer;

string? myString = "123";

var myInt = myString.ToNonNullableType<int>();

Console.WriteLine(myInt);
```

### Convert from a string to a date time
```csharp
using Transformer;

string? myString = "1/1/2023";

var myDateTime = myString.ToNonNullableType<DateTime>();

Console.WriteLine(myDateTime);
```

### Convert from a string to a nullable int
```csharp
using Transformer;

string? myString = "123A";

var myInt = myString.ToNullableType<int>();

Console.WriteLine(myInt); // returns null

var myInt2 = myString.ToNullableType<int>(false);

Console.WriteLine(myInt2); // returns 0
```

### Convert from an int to a float
```csharp
using Transformer;

var myInt = "1";

var myFloat = myInt.ToNonNullableType<float>();

Console.WriteLine(myFloat);
```

### Test if string is parseable to int
```csharp
using Transformer;

string? myString = "123A";

Console.WriteLine(myString.IsParseable<int>()); // returns false

string? myNullString = null;

Console.WriteLine(myNullString.IsParseable<int>()); // returns false
Console.WriteLine(myNullString.IsParseable<int>(allowNullable: true)); // returns true
```
