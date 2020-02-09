using System;

namespace Unisave.Arango.Expressions
{
    public class AqlBinaryOperator : AqlExpression
    {
        public override AqlExpressionType ExpressionType { get; }
        
        public override bool HasParameters { get; }
        
        /// <summary>
        /// Left operand
        /// </summary>
        public AqlExpression Left { get; }
        
        /// <summary>
        /// Right operand
        /// </summary>
        public AqlExpression Right { get; }
        
        public AqlBinaryOperator(
            AqlExpression left,
            AqlExpression right,
            AqlExpressionType type
        )
        {
            if (
                type != AqlExpressionType.Add
                && type != AqlExpressionType.Subtract
                && type != AqlExpressionType.Multiply
                && type != AqlExpressionType.Divide
                && type != AqlExpressionType.Modulo
                && type != AqlExpressionType.Equal
                && type != AqlExpressionType.NotEqual
                && type != AqlExpressionType.GreaterThan
                && type != AqlExpressionType.GreaterThanOrEqual
                && type != AqlExpressionType.LessThan
                && type != AqlExpressionType.LessThanOrEqual
                && type != AqlExpressionType.And
                && type != AqlExpressionType.Or
            )
                throw new ArgumentException(
                    $"Provided expression type {type} is not a binary operation"
                );
            
            ExpressionType = type;
            Left = left;
            Right = right;
            HasParameters = left.HasParameters || right.HasParameters;
        }
        
        public override string ToAql()
        {
            return $"({Left.ToAql()} {GetOperatorString()} {Right.ToAql()})";
        }

        private string GetOperatorString()
        {
            switch (ExpressionType)
            {
                case AqlExpressionType.Add: return "+";
                case AqlExpressionType.Subtract: return "-";
                case AqlExpressionType.Multiply: return "*";
                case AqlExpressionType.Divide: return "/";
                case AqlExpressionType.Modulo: return "%";
                case AqlExpressionType.Equal: return "==";
                case AqlExpressionType.NotEqual: return "!=";
                case AqlExpressionType.GreaterThan: return ">";
                case AqlExpressionType.GreaterThanOrEqual: return ">=";
                case AqlExpressionType.LessThan: return "<";
                case AqlExpressionType.LessThanOrEqual: return "<=";
                case AqlExpressionType.And: return "AND";
                case AqlExpressionType.Or: return "OR";
                
                default:
                    return ExpressionType.ToString();
            }
        }
    }
}