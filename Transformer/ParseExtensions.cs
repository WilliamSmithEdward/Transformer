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
        /// If <c>true</c>, allows nullable types and considers them parseable even if they are null or empty.
        /// If <c>false</c>, only non-nullable values are considered parseable.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value can be parsed to type <typeparamref name="T"/>; otherwise, <c>false</c>.
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
