namespace Unisave.Arango.Expressions
{
    public enum AqlExpressionType
    {
        Constant,
        Parameter,
        Function,
        
        UnaryMinus,
        UnaryPlus,
        Not,
        
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        And,
        Or,
        
        JsonObject,
        // JsonArray ?
        
        MemberAccess
    }
}