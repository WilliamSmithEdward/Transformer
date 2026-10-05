# Changelog

Each release's notes. The Publish workflow takes the section for the
version it releases as the GitHub release's body, so a section is written
here before the version is tagged.

The package is `WilliamSmithE.Transformer` on nuget.org. The sections up to
1.0.0.5 were gathered from its nuget.org version history, with the UTC date
the nuget.org catalog records for each upload. Neither nuget.org nor the
README carried release notes for them. Versions 1.0.0 to 1.0.0.4 are
unlisted on nuget.org; 1.0.0.5 is the listed version. All of them target
net7.0.

## [2.0.1] - 2026-10-04

* The NuGet package now embeds the root GitHub `README.md`, including its badges, as its only README. The OpenSSF Scorecard badge is served through `img.shields.io`, which NuGet supports.
* CI and Publish verify that the packaged README exactly matches the root file.
* No library API or runtime behavior changes.

## [2.0.0] - 2026-10-02

Conversions now reach enums and value types such as `Guid`, `TimeSpan` and `DateOnly`, can read text in a culture the caller chooses, and cost no exception for text that does not convert. `IEnumerableToDataTable` converts any list without throwing, and `ToTitleCase` gives the same result on every machine. Several of the fixes change what callers see, hence the major version.

### Breaking changes

* The package targets net8.0, net9.0 and net10.0. net7.0, which is out of support, is dropped.
* An enum converts from its name, in any case, or from a whole number, as text or as a value of an integer type, and like any enum it takes a number it has no name for. Text converts to `Guid`, `TimeSpan`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `Int128`, `BigInteger` and any other value type with a public static `TryParse(string, IFormatProvider, out T)`. All of these used to give the default, `null` or `false` for every value, in `ToNonNullableType`, `ToNullableType`, `IsParseable` and `ToNonNullableCollectionType` alike.
* `IEnumerableToDataTable` makes columns only of public instance properties with a public getter. Static properties, and properties whose getter is private, used to be columns too. A `null` element gives a row of `DBNull.Value`, and a `null` list throws `ArgumentNullException`; they used to throw `TargetException` and `NullReferenceException`.
* `ToTitleCase` lower-cases with the invariant culture, so under Turkish or Azerbaijani settings it keeps the dotted i, where it used to give a dotless one. A `null` string throws `ArgumentNullException` instead of `NullReferenceException`.

### Fixes

* `IEnumerableToDataTable` converts a type with an indexer, a write-only property or a property hidden with `new`. These used to throw `TargetParameterCountException`, `ArgumentException` and `DuplicateNameException`, so a list of strings, whose type has an indexer, could not be converted at all.
* The `InvalidCastException` from `ToNonNullableType<T>(false)` carries the cause as its `InnerException`: a `FormatException`, an `OverflowException`, or an `ArgumentException` for a name an enum does not have.
* Text is read with each type's own `TryParse` and the styles `Convert` uses, so text that does not convert costs no exception, and `ToNonNullableCollectionType` converts each element once instead of twice. 100,000 strings, half of them not numbers, took about 6 ms instead of about 380 ms. The results are the same as before.
* The build is deterministic, so the same commit gives the same dll.

### Additions

* Overloads of `ToNonNullableType`, `ToNullableType`, `IsParseable` and `ToNonNullableCollectionType` take an `IFormatProvider` to read text with, such as `CultureInfo.InvariantCulture` for text written in a fixed format. The existing methods still read the current culture.
* The READMEs are rewritten against the code, with samples that compile and print what they say, and the nuget.org readme is the full README. SECURITY.md says what the library does with what it is given and how to convert text you did not create.
* The package is built in CI from the tagged commit, tested on all three frameworks, scanned for vulnerabilities and malware, and published through nuget.org trusted publishing. The GitHub release carries the package's signed build provenance.

## [1.0.0.5] - 2024-06-21

No notes were recorded.

## [1.0.0.4] - 2023-12-03

No notes were recorded.

## [1.0.0.3] - 2023-11-29

No notes were recorded.

## [1.0.0.2] - 2023-11-29

No notes were recorded.

## [1.0.0.1] - 2023-11-29

No notes were recorded.

## [1.0.0] - 2023-11-29

No notes were recorded.
