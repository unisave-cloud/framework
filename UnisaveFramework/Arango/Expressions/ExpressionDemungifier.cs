using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Unisave.Arango.Expressions
{
    public class ExpressionDemungifier : ExpressionVisitor
    {
        /// <summary>
        /// Main method
        /// </summary>
        public Expression Demungify(Expression expression)
        {
            return Visit(expression);
        }
        
        protected override Expression VisitUnary(UnaryExpression node)
        {
            var operand = Visit(node.Operand);
            var unary = Expression.MakeUnary(
                node.NodeType, operand,
                node.Type, node.Method
            );

            if (operand is ConstantExpression)
                return EvaluateExpression(unary);
            
            return unary;
        }
        
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);
            var binary = Expression.MakeBinary(
                node.NodeType, left, right,
                node.IsLiftedToNull, node.Method, node.Conversion
            );

            if (left is ConstantExpression && right is ConstantExpression)
                return EvaluateExpression(binary);

            return binary;
        }
        
        protected override Expression VisitMember(MemberExpression node)
        {
            var instance = Visit(node.Expression);
            var access = Expression.MakeMemberAccess(
                instance, node.Member
            );

            if (instance == null || instance is ConstantExpression)
                return EvaluateExpression(access);
            
            return access;
        }
        
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var instance = Visit(node.Object);
            var args = node.Arguments.Select(Visit).ToList();
            var call = Expression.Call(instance, node.Method, args);

            if (IsAqlFunction(node.Method)) // don't evaluate
                return call;
            
            if ((instance == null || instance is ConstantExpression)
                && args.All(a => a is ConstantExpression))
                return EvaluateExpression(call);
            
            return call;
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            var instance = Visit(node.Expression);
            var args = node.Arguments.Select(Visit).ToList();
            var call = Expression.Invoke(instance, args);
            
            if (instance is ConstantExpression
                && args.All(a => a is ConstantExpression))
                return EvaluateExpression(call);
            
            return call;
        }
        
        protected override Expression VisitNew(NewExpression node)
        {
            var args = node.Arguments.Select(Visit).ToList();
            var call = Expression.New(node.Constructor, args);
            
            if (args.All(a => a is ConstantExpression))
                return EvaluateExpression(call);
            
            return call;
        }
        
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var args = node.Expressions.Select(Visit).ToList();

            NewArrayExpression call = null;

            Type elementType = node.Type.GetElementType();
            
            if (elementType == null)
                throw new AqlParsingException(
                    $"Array initialization has no element type {node}"
                );
            
            if (node.NodeType == ExpressionType.NewArrayInit)
                call = Expression.NewArrayInit(elementType, args);
            
            if (node.NodeType == ExpressionType.NewArrayBounds)
                call = Expression.NewArrayBounds(elementType, args);
            
            if (call == null)
                throw new AqlParsingException(
                    $"Unknown array initialization type {node.Type}"
                );
            
            if (args.All(a => a is ConstantExpression))
                return EvaluateExpression(call);

            return call;
        }
        
        /// <summary>
        /// Returns true if the given method is an AQL function
        /// and thus shouldn't be evaluated
        /// </summary>
        private static bool IsAqlFunction(MethodInfo method)
        {
            ArangoFunctionAttribute attribute = method
                .GetCustomAttribute<ArangoFunctionAttribute>();

            return attribute != null;
        }
        
        /// <summary>
        /// Evaluate expression to a constant expression
        /// </summary>
        private ConstantExpression EvaluateExpression(Expression expression)
        {
            LambdaExpression lambdaExpression = Expression.Lambda(expression);
            Delegate compiledLambda = lambdaExpression.Compile();
            object result = compiledLambda.DynamicInvoke();
            return Expression.Constant(result, expression.Type);
        }
    }
}