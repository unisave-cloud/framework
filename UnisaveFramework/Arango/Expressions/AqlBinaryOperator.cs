using System;
using System.Collections.ObjectModel;
using System.Linq;
using LightJson;
using Unisave.Arango.Execution;

namespace Unisave.Arango.Expressions
{
    public class AqlBinaryOperator : AqlExpression
    {
        public override AqlExpressionType ExpressionType { get; }
        
        public override ReadOnlyCollection<string> Parameters { get; }
        
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
            Parameters = new ReadOnlyCollection<string>(
                left.Parameters.Union(right.Parameters).ToList()
            );
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

        public override JsonValue Evaluate(
            QueryExecutor executor,
            ExecutionFrame frame
        )
        {
            JsonValue l = Left.Evaluate(executor, frame);
            JsonValue r = Right.Evaluate(executor, frame);
            
            switch (ExpressionType)
            {
                case AqlExpressionType.Add: return AqlArithmetic.Add(l, r);
                case AqlExpressionType.Subtract: return AqlArithmetic.Subtract(l, r);
                case AqlExpressionType.Multiply: return AqlArithmetic.Multiply(l, r);
                case AqlExpressionType.Divide: return AqlArithmetic.Divide(l, r);
                case AqlExpressionType.Modulo: return AqlArithmetic.Modulo(l, r);

                case AqlExpressionType.Equal: return AqlArithmetic.Equal(l, r);
                case AqlExpressionType.NotEqual: return AqlArithmetic.NotEqual(l, r);
                case AqlExpressionType.GreaterThan: return AqlArithmetic.GreaterThan(l, r);
                case AqlExpressionType.GreaterThanOrEqual: return AqlArithmetic.GreaterThenOrEqual(l, r);
                case AqlExpressionType.LessThan: return AqlArithmetic.LessThan(l, r);
                case AqlExpressionType.LessThanOrEqual: return AqlArithmetic.LessThanOrEqual(l, r);
                
                case AqlExpressionType.And: return AqlArithmetic.And(l, r);
                case AqlExpressionType.Or: return AqlArithmetic.Or(l, r);
            }
            
            throw new QueryExecutionException(
                $"Cannot evaluate expression {ExpressionType} - not implemented"
            );
        }
    }
}