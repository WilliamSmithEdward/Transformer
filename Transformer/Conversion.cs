using System.Globalization;
using System.Reflection;

namespace Transformer
{
    /// <summary>
    /// The conversion every public method shares. For the types <see cref="Convert"/> knows it gives the same result
    /// as <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/>, but reads text with the target type's
    /// <c>TryParse</c>, using the styles <see cref="Convert"/> uses, so text that does not convert costs no exception.
    /// It also converts to an enum, from a name or a whole number, and to any other value type with a public static
    /// <c>TryParse(string, IFormatProvider, out T)</c>, such as <see cref="Guid"/> or <see cref="TimeSpan"/>, from text.
    /// </summary>
    internal static class Conversion
    {
        /// <summary>
        /// Whether the value counts as no value: <c>null</c>, <see cref="DBNull.Value"/>, or a value whose string
        /// form is empty or only white space.
        /// </summary>
        internal static bool IsBlank(object? value) => string.IsNullOrEmpty(value?.ToString()?.Trim());

        /// <summary>
        /// Converts <paramref name="value"/> to <typeparamref name="T"/>, reading text with
        /// <paramref name="provider"/>, or the current culture when it is <c>null</c>.
        /// </summary>
        /// <returns><c>true</c> if the value converted; otherwise <c>false</c>, with <paramref name="result"/> the default.</returns>
        internal static bool TryConvert<T>(object? value, IFormatProvider? provider, out T result) where T : struct
        {
            result = default;

            // Convert.ChangeType refuses null for every value type.
            if (value is null)
            {
                return false;
            }

            if (value is T same)
            {
                result = same;
                return true;
            }

            if (typeof(T).IsEnum)
            {
                return TryEnum(value, out result);
            }

            if (value is string text)
            {
                if (TryParse(text, provider, out bool converted, out result))
                {
                    return converted;
                }

                if (Parser<T>.TryParse is { } parse)
                {
                    return parse(text, provider, out result);
                }
            }

            try
            {
                result = (T)Convert.ChangeType(value, typeof(T), provider);
                return true;
            }
            catch (Exception)
            {
                // Any failure is "does not convert", as the public methods document it.
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Why <paramref name="value"/> does not convert to <typeparamref name="T"/>: the exception the throwing form of
        /// the same conversion raises for it (<see cref="Convert.ChangeType(object, Type, IFormatProvider)"/>,
        /// <see cref="Enum.Parse(Type, string, bool)"/>, or the type's own <c>Parse</c>). Called only once
        /// <see cref="TryConvert{T}"/> has failed, to give a caller's exception its cause.
        /// </summary>
        /// <returns>The exception, or <c>null</c> if the value converts after all.</returns>
        internal static Exception? Failure<T>(object? value, IFormatProvider? provider) where T : struct
        {
            try
            {
                if (typeof(T).IsEnum && value is string name)
                {
                    Enum.Parse(typeof(T), name, ignoreCase: true);
                }
                else if (typeof(T).IsEnum && value is not null && IsWholeNumber(value))
                {
                    Convert.ChangeType(value, Enum.GetUnderlyingType(typeof(T)), CultureInfo.InvariantCulture);
                }
                else if (!typeof(T).IsEnum && value is string text && Parser<T>.Parse is { } parse)
                {
                    parse(text, provider);
                }
                else
                {
                    Convert.ChangeType(value, typeof(T), provider);
                }

                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        /// <summary>
        /// Converts to an enum: text as a name, in any case, or a whole number, as <see cref="Enum.TryParse(Type, string, bool, out object)"/>
        /// reads it; a whole number of an integer type that fits the enum's underlying type. Like any enum, it takes a number
        /// it has no name for. A value of another enum type does not convert.
        /// </summary>
        private static bool TryEnum<T>(object value, out T result) where T : struct
        {
            result = default;
            if (value is string text)
            {
                if (!Enum.TryParse(typeof(T), text, ignoreCase: true, out object? parsed) || parsed is null)
                {
                    return false;
                }

                result = (T)parsed;
                return true;
            }

            if (!IsWholeNumber(value))
            {
                return false;
            }

            try
            {
                // Checked against the underlying type, so a number too large for the enum does not wrap around.
                object number = Convert.ChangeType(value, Enum.GetUnderlyingType(typeof(T)), CultureInfo.InvariantCulture);
                result = (T)Enum.ToObject(typeof(T), number);
                return true;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        /// <summary>Whether the value is of an integer type, and not an enum.</summary>
        private static bool IsWholeNumber(object value) =>
            !value.GetType().IsEnum && Type.GetTypeCode(value.GetType()) is TypeCode.SByte or TypeCode.Byte
                or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64;

        /// <summary>
        /// The public static <c>TryParse(string, IFormatProvider, out T)</c> and <c>Parse(string, IFormatProvider)</c> of a
        /// value type <see cref="Convert"/> does not know, such as <see cref="Guid"/>, <see cref="TimeSpan"/>,
        /// <see cref="DateTimeOffset"/>, <see cref="DateOnly"/>, <see cref="TimeOnly"/> or <see cref="Int128"/>; <c>null</c>
        /// when the type has none. Looked up once per type.
        /// </summary>
        private static class Parser<T> where T : struct
        {
            internal delegate bool TryParseText(string text, IFormatProvider? provider, out T result);

            internal delegate T ParseText(string text, IFormatProvider? provider);

            internal static readonly TryParseText? TryParse =
                Find<TryParseText>("TryParse", typeof(string), typeof(IFormatProvider), typeof(T).MakeByRefType());

            internal static readonly ParseText? Parse = Find<ParseText>("Parse", typeof(string), typeof(IFormatProvider));

            private static TDelegate? Find<TDelegate>(string name, params Type[] parameters) where TDelegate : Delegate
            {
                // The types Convert reads from a string keep Convert's own styles.
                if (Type.GetTypeCode(typeof(T)) != TypeCode.Object)
                {
                    return null;
                }

                MethodInfo? method = typeof(T).GetMethod(name, BindingFlags.Public | BindingFlags.Static, null, parameters, null);
                return method is null ? null : (TDelegate?)Delegate.CreateDelegate(typeof(TDelegate), method, throwOnBindFailure: false);
            }
        }

        /// <summary>
        /// Parses text for the types <see cref="Convert"/> reads from a string, with the styles it uses for each.
        /// </summary>
        /// <returns><c>true</c> if the type is one of them, with <paramref name="converted"/> saying whether the text parsed.</returns>
        private static bool TryParse<T>(string text, IFormatProvider? provider, out bool converted, out T result) where T : struct
        {
            result = default;
            converted = false;
            if (typeof(T).IsEnum)
            {
                return false;
            }

            object? parsed;
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    converted = bool.TryParse(text, out bool boolean);
                    parsed = boolean;
                    break;
                case TypeCode.Char:
                    converted = text.Length == 1;
                    parsed = converted ? text[0] : '\0';
                    break;
                case TypeCode.SByte:
                    converted = sbyte.TryParse(text, NumberStyles.Integer, provider, out sbyte int8);
                    parsed = int8;
                    break;
                case TypeCode.Byte:
                    converted = byte.TryParse(text, NumberStyles.Integer, provider, out byte uint8);
                    parsed = uint8;
                    break;
                case TypeCode.Int16:
                    converted = short.TryParse(text, NumberStyles.Integer, provider, out short int16);
                    parsed = int16;
                    break;
                case TypeCode.UInt16:
                    converted = ushort.TryParse(text, NumberStyles.Integer, provider, out ushort uint16);
                    parsed = uint16;
                    break;
                case TypeCode.Int32:
                    converted = int.TryParse(text, NumberStyles.Integer, provider, out int int32);
                    parsed = int32;
                    break;
                case TypeCode.UInt32:
                    converted = uint.TryParse(text, NumberStyles.Integer, provider, out uint uint32);
                    parsed = uint32;
                    break;
                case TypeCode.Int64:
                    converted = long.TryParse(text, NumberStyles.Integer, provider, out long int64);
                    parsed = int64;
                    break;
                case TypeCode.UInt64:
                    converted = ulong.TryParse(text, NumberStyles.Integer, provider, out ulong uint64);
                    parsed = uint64;
                    break;
                case TypeCode.Single:
                    converted = float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, provider, out float single);
                    parsed = single;
                    break;
                case TypeCode.Double:
                    converted = double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, provider, out double number);
                    parsed = number;
                    break;
                case TypeCode.Decimal:
                    converted = decimal.TryParse(text, NumberStyles.Number, provider, out decimal money);
                    parsed = money;
                    break;
                case TypeCode.DateTime:
                    converted = DateTime.TryParse(text, provider, DateTimeStyles.None, out DateTime date);
                    parsed = date;
                    break;
                default:
                    return false;
            }

            if (converted)
            {
                result = (T)parsed;
            }

            return true;
        }
    }
}
