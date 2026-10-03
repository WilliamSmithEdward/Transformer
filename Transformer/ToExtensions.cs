using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Transformer
{
    /// <summary>
    /// Provides extension methods for type conversion.
    /// </summary>
    public static class ToExtensions
    {
        /// <summary>
        /// Converts the specified object to the specified non-nullable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The target non-nullable value type.</typeparam>
        /// <param name="o">The object to be converted.</param>
        /// <param name="returnDefaultOnConversionError">
        /// If <c>true</c>, returns the default value of type <typeparamref name="T"/> on conversion error.
        /// If <c>false</c>, throws an <see cref="InvalidCastException"/> on conversion error.
        /// </param>
        /// <returns>
        /// The converted value, with text read in the current culture: as <see cref="Convert.ChangeType(object, Type)"/> converts
        /// it for the types it knows, an enum from its name or a whole number, and another value type from text through its own
        /// <c>TryParse(string, IFormatProvider, out T)</c>.
        /// If <paramref name="returnDefaultOnConversionError"/> is <c>true</c> and conversion fails, the default value of type <typeparamref name="T"/> is returned.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Thrown when <paramref name="returnDefaultOnConversionError"/> is <c>false</c> and conversion fails. Its
        /// <see cref="Exception.InnerException"/> is the cause, such as a <see cref="FormatException"/> for text that does
        /// not parse as <typeparamref name="T"/>, an <see cref="OverflowException"/> for a value out of range, or an
        /// <see cref="ArgumentException"/> for a name an enum does not have.
        /// </exception>
        public static T ToNonNullableType<T>(this object o, bool returnDefaultOnConversionError = true) where T : struct
            => o.ToNonNullableType<T>(null, returnDefaultOnConversionError);

        /// <summary>
        /// Converts the specified object to the specified non-nullable value type <typeparamref name="T"/>, reading text with
        /// the given format provider instead of the current culture.
        /// </summary>
        /// <typeparam name="T">The target non-nullable value type.</typeparam>
        /// <param name="o">The object to be converted.</param>
        /// <param name="provider">
        /// The culture or format provider that reads text, such as <see cref="CultureInfo.InvariantCulture"/> for text written
        /// in a fixed format; <c>null</c> for the current culture.
        /// </param>
        /// <param name="returnDefaultOnConversionError">
        /// If <c>true</c>, returns the default value of type <typeparamref name="T"/> on conversion error.
        /// If <c>false</c>, throws an <see cref="InvalidCastException"/> on conversion error.
        /// </param>
        /// <returns>
        /// The value converted as <see cref="ToNonNullableType{T}(object, bool)"/> converts it, with text read by
        /// <paramref name="provider"/>.
        /// If <paramref name="returnDefaultOnConversionError"/> is <c>true</c> and conversion fails, the default value of type <typeparamref name="T"/> is returned.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Thrown when <paramref name="returnDefaultOnConversionError"/> is <c>false</c> and conversion fails. Its
        /// <see cref="Exception.InnerException"/> is the cause.
        /// </exception>
        public static T ToNonNullableType<T>(this object? o, IFormatProvider? provider, bool returnDefaultOnConversionError = true) where T : struct
        {
            if (Conversion.TryConvert(o, provider, out T value)) return value;
            if (returnDefaultOnConversionError) return default;
            throw new InvalidCastException($"Cannot convert value to type {typeof(T)}.", Conversion.Failure<T>(o, provider));
        }

        /// <summary>
        /// Converts the specified object to the specified nullable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The target nullable value type.</typeparam>
        /// <param name="o">The object to be converted.</param>
        /// <param name="returnNullOnConversionError">
        /// If <c>true</c>, returns <c>null</c> on conversion error.
        /// If <c>false</c>, returns the default value of type <typeparamref name="T"/> on conversion error.
        /// </param>
        /// <returns>
        /// The converted value, with text read in the current culture: as <see cref="Convert.ChangeType(object, Type)"/> converts
        /// it for the types it knows, an enum from its name or a whole number, and another value type from text through its own
        /// <c>TryParse(string, IFormatProvider, out T)</c>.
        /// <c>null</c> for a blank value (<c>null</c>, <see cref="DBNull.Value"/>, or a value whose string form is empty or
        /// only white space), whatever <paramref name="returnNullOnConversionError"/> is.
        /// If <paramref name="returnNullOnConversionError"/> is <c>true</c> and conversion fails, <c>null</c> is returned.
        /// If <paramref name="returnNullOnConversionError"/> is <c>false</c> and conversion fails, the default value of type <typeparamref name="T"/> is returned.
        /// </returns>
        public static T? ToNullableType<T>(this object? o, bool returnNullOnConversionError = true) where T : struct
            => o.ToNullableType<T>(null, returnNullOnConversionError);

        /// <summary>
        /// Converts the specified object to the specified nullable value type <typeparamref name="T"/>, reading text with the
        /// given format provider instead of the current culture.
        /// </summary>
        /// <typeparam name="T">The target nullable value type.</typeparam>
        /// <param name="o">The object to be converted.</param>
        /// <param name="provider">
        /// The culture or format provider that reads text, such as <see cref="CultureInfo.InvariantCulture"/> for text written
        /// in a fixed format; <c>null</c> for the current culture.
        /// </param>
        /// <param name="returnNullOnConversionError">
        /// If <c>true</c>, returns <c>null</c> on conversion error.
        /// If <c>false</c>, returns the default value of type <typeparamref name="T"/> on conversion error.
        /// </param>
        /// <returns>
        /// The value converted as <see cref="ToNullableType{T}(object, bool)"/> converts it, with text read by
        /// <paramref name="provider"/>: <c>null</c> for a blank value whatever <paramref name="returnNullOnConversionError"/>
        /// is, and <c>null</c> or the default value of type <typeparamref name="T"/>, as it says, when conversion fails.
        /// </returns>
        public static T? ToNullableType<T>(this object? o, IFormatProvider? provider, bool returnNullOnConversionError = true) where T : struct
        {
            if (Conversion.IsBlank(o)) return null;
            if (Conversion.TryConvert(o, provider, out T value)) return value;
            if (returnNullOnConversionError) return null;
            return default(T);
        }

        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to a <see cref="DataTable"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="list">The <see cref="IEnumerable{T}"/> to convert to a <see cref="DataTable"/>.</param>
        /// <returns>
        /// A <see cref="DataTable"/> named after the full name of <typeparamref name="T"/>, with one column for each public
        /// instance property of <typeparamref name="T"/> that has a public getter and is not an indexer (of the property's
        /// type, or its underlying type for a <see cref="Nullable{T}"/> property), and one row for each element. A property
        /// hidden with <c>new</c> gives one column, from the most derived type. A <c>null</c> element gives a row of
        /// <see cref="DBNull.Value"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="list"/> is null.</exception>
        public static DataTable IEnumerableToDataTable<T>(this IEnumerable<T> list)
        {
            ArgumentNullException.ThrowIfNull(list);
            PropertyInfo[] properties = ColumnProperties(typeof(T));

            var dataTable = new DataTable
            {
                TableName = typeof(T).FullName
            };

            foreach (PropertyInfo info in properties)
            {
                dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            foreach (T entity in list)
            {
                // A null element leaves every value null, which the row stores as DBNull.
                object?[] values = new object?[properties.Length];
                if (entity is not null)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        values[i] = properties[i].GetValue(entity);
                    }
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        /// <summary>
        /// The properties of <paramref name="type"/> that become columns: public, not static, with a public getter and no
        /// index parameters, one per name. Where <c>new</c> hides a base type's property, the most derived one is kept, in
        /// the place the name first appears.
        /// </summary>
        private static PropertyInfo[] ColumnProperties(Type type) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0 && p.GetGetMethod() is not null)
                .GroupBy(p => p.Name)
                .Select(g => g.OrderByDescending(p => Depth(p.DeclaringType)).First())
                .ToArray();

        /// <summary>How many base types <paramref name="type"/> has.</summary>
        private static int Depth(Type? type)
        {
            int depth = 0;
            for (Type? t = type?.BaseType; t is not null; t = t.BaseType)
            {
                depth++;
            }

            return depth;
        }

        /// <summary>
        /// Converts the specified <see cref="DataTable"/> to a formatted string suitable for console output.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to convert to a console string.</param>
        /// <returns>A formatted string representing the contents of the <see cref="DataTable"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="table"/> is null.</exception>
        public static string ToConsoleString(this DataTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var builder = new StringBuilder();

            var columnWidths = new int[table.Columns.Count];
            foreach (DataColumn column in table.Columns)
            {
                columnWidths[column.Ordinal] = table.AsEnumerable()
                                                     .Select(row => (row[column].ToString() ?? string.Empty).Length)
                                                     .Union(new[] { column.ColumnName.Length })
                                                     .Max();
            }

            var separator = new string('-', columnWidths.Sum(width => width + 3) + 1);

            builder.AppendLine(separator);
            for (int i = 0; i < table.Columns.Count; i++)
            {
                builder.Append("| ").Append(table.Columns[i].ColumnName.PadRight(columnWidths[i])).Append(' ');
            }
            builder.AppendLine("|");
            builder.AppendLine(separator);

            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    builder.Append("| ").Append(row[i]?.ToString()?.PadRight(columnWidths[i])).Append(' ');
                }
                builder.AppendLine("|");
            }
            builder.AppendLine(separator);

            return builder.ToString();
        }

        /// <summary>
        /// Rounds a single-precision floating-point value to a specified number of fractional digits.
        /// </summary>
        /// <param name="value">The single-precision floating-point number to be rounded.</param>
        /// <param name="digits">The number of fractional digits in the return value.</param>
        /// <returns>
        /// The number nearest to <paramref name="value"/> that contains a number of fractional digits equal to <paramref name="digits"/>;
        /// a value halfway between two such numbers goes to the even one. The value is rounded as a <see cref="double"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="digits"/> is less than 0 or greater than 15.</exception>
        public static float Round(this float value, int digits) => Math.Round(value, digits).ToNonNullableType<float>();

        /// <summary>
        /// Rounds a double-precision floating-point value to a specified number of fractional digits.
        /// </summary>
        /// <param name="value">The double-precision floating-point number to be rounded.</param>
        /// <param name="digits">The number of fractional digits in the return value.</param>
        /// <returns>
        /// The number nearest to <paramref name="value"/> that contains a number of fractional digits equal to <paramref name="digits"/>;
        /// a value halfway between two such numbers goes to the even one.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="digits"/> is less than 0 or greater than 15.</exception>
        public static double Round(this double value, int digits) => Math.Round(value, digits);

        /// <summary>
        /// Converts the specified string to title case: the whole string lower-cased, then the first letter of each word capitalized.
        /// </summary>
        /// <param name="s">The string to convert to title case.</param>
        /// <returns>
        /// The specified string converted to title case, the same under every culture, since both steps use the invariant
        /// culture. Words in capitals are lower-cased too, so "NASA" becomes "Nasa".
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="s"/> is null.</exception>
        public static string ToTitleCase(this string s)
        {
            ArgumentNullException.ThrowIfNull(s);

            // Both steps use the invariant culture, so the result does not depend on the machine's
            // settings: lower-casing in Turkish would turn "I" into a dotless i.
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());
        }
    }
}
