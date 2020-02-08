namespace Unisave.Arango.Query
{
    public class ReturnOperation : AqlOperation
    {
        public override AqlOperationType OperationType
            => AqlOperationType.Return;
    }
}