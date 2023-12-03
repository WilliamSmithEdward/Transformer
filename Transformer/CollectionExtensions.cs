namespace Transformer
{
    /// <summary>
    /// Provides extension methods for collections to transform elements to a non-nullable type.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Transforms elements of the current collection to a new non-nullable collection type.
        /// </summary>
        /// <typeparam name="TCollectionCurrent">The type of the current collection.</typeparam>
        /// <typeparam name="TCurrentType">The type of elements in the current collection.</typeparam>
        /// <typeparam name="TCollectionNew">The type of the new non-nullable collection.</typeparam>
        /// <typeparam name="TNewType">The type of elements in the new non-nullable collection.</typeparam>
        /// <param name="collection">The current collection to transform.</param>
        /// <returns>
        /// A <see cref="CollectionTransformResult{TCollectionNew, TNewType}"/> containing the successfully
        /// transformed elements and a list of objects representing transformation failures.
        /// </returns>
        /// <remarks>
        /// This method filters the elements of the current collection, keeping only those that can be successfully
        /// parsed to the specified non-nullable type <typeparamref name="TNewType"/>. It returns a result object
        /// containing the successfully transformed elements and a list of objects representing transformation failures.
        /// </remarks>
        /// <seealso cref="CollectionTransformResult{TCollectionNew, TNewType}"/>
        public static CollectionTransformResult<TCollectionNew, TNewType> ToNonNullableCollectionType<TCollectionCurrent, TCurrentType, TCollectionNew, TNewType>(this TCollectionCurrent collection) 
            where TCollectionCurrent : ICollection<TCurrentType>
            where TCollectionNew : ICollection<TNewType>, new()
            where TNewType : struct
        {
            var successes = new TCollectionNew();
            var failures = new List<object>();

            foreach (var item in collection)
            {
                if (item.IsParseable<TNewType>() && item is not null)
                {
                    successes.Add(item.ToNonNullableType<TNewType>());
                }
                else
                {
                    if (item is not null)
                    {
                        failures.Add(item);
                    }
                }
            }

            return new CollectionTransformResult<TCollectionNew, TNewType>(successes, failures);
        }
    }
}
