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
        
        Plus,
        Minus,
        // ...
        
        JsonObject,
        // JsonArray ?
        
        MemberAccess
    }
}