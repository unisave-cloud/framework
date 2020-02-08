using System;

namespace Unisave.Arango.Expressions
{
    [AttributeUsage(
        AttributeTargets.Method,
        Inherited = false,
        AllowMultiple = false
    )]
    public class ArangoFunctionAttribute : Attribute
    {
        public string FunctionName { get; }
        
        public ArangoFunctionAttribute(string functionName)
        {
            FunctionName = functionName;
        }
    }
}