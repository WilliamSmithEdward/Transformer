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
        /// <c>true</c> if <see cref="Convert.ChangeType(object, Type)"/> converts the value to type <typeparamref name="T"/>,
        /// which reads text in the current culture; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsParseable<T>(this object? o, bool allowNullable = false) where T : struct
        {
            if (string.IsNullOrEmpty(o?.ToString()?.Trim()))
            {
                if (allowNullable) return true;
                else return false;
            }

            try
            {
                Convert.ChangeType(o, typeof(T));
                return true;
            }

            catch
            {
                return false;
            }
        }
    }
}
