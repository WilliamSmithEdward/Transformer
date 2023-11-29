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
    }
}
