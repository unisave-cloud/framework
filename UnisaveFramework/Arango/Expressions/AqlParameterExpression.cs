namespace Unisave.Arango.Expressions
{
    public class AqlParameterExpression : AqlExpression
    {
        public override AqlExpressionType ExpressionType
            => AqlExpressionType.Parameter;
        
        public override bool HasParameters => true;
        
        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get; }

        public AqlParameterExpression(string name)
        {
            // TODO: check parameter name for invalid characters
            
            Name = name;
        }

        public override string ToAql()
        {
            return Name;
        }
    }
}