namespace Transformer
{
    /// <summary>
    /// Provides extension methods to check if a nullable value can be parsed to a specified type.
    /// </summary>
    public static class ParseExtensions
    {
        /// <summary>
        /// Checks if the specified value can be parsed to the specified non-nullable or nullable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The target value type.</typeparam>
        /// <param name="o">The object to be checked for parseability.</param>
        /// <param name="allowNullable">
        /// If <c>true</c>, a blank value (<c>null</c>, <see cref="DBNull.Value"/>, or a value whose string form is empty or
        /// only white space) counts as parseable. If <c>false</c>, a blank value is not parseable.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value converts to type <typeparamref name="T"/>, with text read in the current culture: as
        /// <see cref="Convert.ChangeType(object, Type)"/> converts it for the types it knows, an enum from its name or a whole
        /// number, and another value type from text through its own <c>TryParse(string, IFormatProvider, out T)</c>; otherwise,
        /// <c>false</c>.
        /// </returns>
        public static bool IsParseable<T>(this object? o, bool allowNullable = false) where T : struct
        {
            if (Conversion.IsBlank(o))
            {
                return allowNullable;
            }

            return Conversion.TryConvert<T>(o, null, out _);
        }
    }
}
