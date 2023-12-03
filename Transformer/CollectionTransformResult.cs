namespace Transformer
{
    /// <summary>
    /// Represents the result of a collection transformation operation.
    /// </summary>
    /// <typeparam name="TCollection">The type of the collection to transform.</typeparam>
    /// <typeparam name="TType">The type of elements in the collection.</typeparam>
    /// <remarks>
    /// This class contains information about the successes and failures of the transformation.
    /// </remarks>
    public class CollectionTransformResult<TCollection, TType> where TCollection : ICollection<TType> where TType : struct
    {
        /// <summary>
        /// Gets the successfully transformed collection.
        /// </summary>
        public TCollection TransformationSuccesses { get; private set; }

        /// <summary>
        /// Gets the list of objects representing transformation failures.
        /// </summary>
        public List<object> TransformationFailures { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionTransformResult{TCollection, TType}"/> class.
        /// </summary>
        /// <param name="successes">The successfully transformed collection.</param>
        /// <param name="failures">The list of objects representing transformation failures.</param>
        public CollectionTransformResult(TCollection successes, List<object> failures)
        {
            TransformationSuccesses = successes;
            TransformationFailures = failures;
        }
    }
}
