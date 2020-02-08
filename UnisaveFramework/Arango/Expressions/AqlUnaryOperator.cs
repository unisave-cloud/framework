using System;

namespace Unisave.Arango.Expressions
{
    public class AqlUnaryOperator : AqlExpression
    {
        public override AqlExpressionType ExpressionType { get; }
        
        public override bool HasParameters { get; }
        
        /// <summary>
        /// The only operand of the unary operation
        /// </summary>
        public AqlExpression Operand { get; }

        public AqlUnaryOperator(AqlExpression operand, AqlExpressionType type)
        {
            if (
                type != AqlExpressionType.UnaryPlus
                && type != AqlExpressionType.UnaryMinus
                && type != AqlExpressionType.Not
            )
                throw new ArgumentException(
                    $"Provided expression type {type} is not an unary operation"
                );
            
            ExpressionType = type;
            Operand = operand;
            HasParameters = operand.HasParameters;
        }
        
        public override string ToAql()
        {
            switch (ExpressionType)
            {
                case AqlExpressionType.UnaryPlus:
                    return "(+" + Operand.ToAql() + ")";
                
                case AqlExpressionType.UnaryMinus:
                    return "(-" + Operand.ToAql() + ")";
                
                case AqlExpressionType.Not:
                    return "(NOT " + Operand.ToAql() + ")";
            }

            return ExpressionType + " " + Operand.ToAql();
        }
    }
}