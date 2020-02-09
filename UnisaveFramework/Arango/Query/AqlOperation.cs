namespace Unisave.Arango.Query
{
    /// <summary>
    /// Base class for an AQL operation (FOR, RETURN, FILTER, ...)
    /// </summary>
    public abstract class AqlOperation
    {
        /// <summary>
        /// Type of the operation at hand
        /// </summary>
        public abstract AqlOperationType OperationType { get; }

        /// <summary>
        /// Renders the query operation to an AQL string
        /// </summary>
        public abstract string ToAql();
    }
}