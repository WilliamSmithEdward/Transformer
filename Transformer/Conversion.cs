using System.Globalization;

namespace Transformer
{
    /// <summary>
    /// The conversion every public method shares. It gives the same result as
    /// <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/>, but reads text with the target type's
    /// <c>TryParse</c>, using the styles <see cref="Convert"/> uses, so text that does not convert costs no exception.
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

            if (value is string text && TryParse(text, provider, out bool converted, out result))
            {
                return converted;
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
