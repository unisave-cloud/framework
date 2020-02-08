namespace Unisave.Arango.Query
{
    /// <summary>
    /// Base class for an AQL operation (FOR, RETURN, FILTER, ...)
    /// </summary>
    public abstract class AqlOperation
    {
        public abstract AqlOperationType OperationType { get; }
    }
}