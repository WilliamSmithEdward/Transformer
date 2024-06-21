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
        /// The converted value of type <typeparamref name="T"/>.
        /// If <paramref name="returnDefaultOnConversionError"/> is <c>true</c> and conversion fails, the default value of type <typeparamref name="T"/> is returned.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Thrown when <paramref name="returnDefaultOnConversionError"/> is <c>false</c> and conversion fails.
        /// </exception>
        public static T ToNonNullableType<T>(this object o, bool returnDefaultOnConversionError = true) where T : struct
        {
            try
            {
                return (T)Convert.ChangeType(o, typeof(T));
            }
            catch
            {
                if (returnDefaultOnConversionError) return default;
                else throw new InvalidCastException($"Cannot convert value to type {typeof(T)}.");
            }
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
        /// The converted nullable value of type <typeparamref name="T"/>.
        /// If <paramref name="returnNullOnConversionError"/> is <c>true</c> and conversion fails, <c>null</c> is returned.
        /// If <paramref name="returnNullOnConversionError"/> is <c>false</c> and conversion fails, the default value of type <typeparamref name="T"/> is returned.
        /// </returns>
        public static T? ToNullableType<T>(this object? o, bool returnNullOnConversionError = true) where T : struct
        {
            if (string.IsNullOrEmpty(o?.ToString()?.Trim()))
            {
                if (returnNullOnConversionError) return null;
                else return default;
            }

            try
            {
                return (T)Convert.ChangeType(o, typeof(T));
            }

            catch
            {
                if (returnNullOnConversionError) return default;
                else return default(T);
            }
        }

        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to a <see cref="DataTable"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="list">The <see cref="IEnumerable{T}"/> to convert to a <see cref="DataTable"/>.</param>
        /// <returns>A <see cref="DataTable"/> representation of the <see cref="IEnumerable{T}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="list"/> is null.</exception>
        public static DataTable IEnumerableToDataTable<T>(this IEnumerable<T> list)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

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
                object?[] values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(entity);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
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
        /// <returns>The number nearest to <paramref name="value"/> that contains a number of fractional digits equal to <paramref name="digits"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="digits"/> is less than 0 or greater than 15.</exception>
        public static float Round(this float value, int digits) => Math.Round(value, digits).ToNonNullableType<float>();

        /// <summary>
        /// Rounds a double-precision floating-point value to a specified number of fractional digits.
        /// </summary>
        /// <param name="value">The double-precision floating-point number to be rounded.</param>
        /// <param name="digits">The number of fractional digits in the return value.</param>
        /// <returns>The number nearest to <paramref name="value"/> that contains a number of fractional digits equal to <paramref name="digits"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="digits"/> is less than 0 or greater than 15.</exception>
        public static double Round(this double value, int digits) => Math.Round(value, digits);

        /// <summary>
        /// Converts the specified string to title case (each word capitalized).
        /// </summary>
        /// <param name="s">The string to convert to title case.</param>
        /// <returns>The specified string converted to title case.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="s"/> is null.</exception>
        public static string ToTitleCase(this string s) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLower());
    }
}
