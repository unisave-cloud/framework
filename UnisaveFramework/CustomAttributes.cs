using System;

namespace Unisave
{
    /*
        Here are defined all of the custom attributes of Unisave
     */

    /// <summary>
    /// "cross" attribute, used to quickly mark certain properties
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        Inherited = false,
        AllowMultiple = false
    )]
    public sealed class XAttribute : Attribute
    {
        public XAttribute()
        { }
    }

    /// <summary>
    /// Specifies what middleware to apply before a method is called
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Class,
        Inherited = true,
        AllowMultiple = true
    )]
    public sealed class MiddlewareAttribute : Attribute
    {
        /// <summary>
        /// Order of middleware execution (smaller numbers run first)
        /// </summary>
        public int Order { get; }
        
        /// <summary>
        /// The actual middleware class to use
        /// </summary>
        public Type MiddlewareType { get; }
        
        /// <summary>
        /// Array of parameters that will be given to the middleware
        /// </summary>
        public string[] Parameters { get; }
        
        public MiddlewareAttribute(Type type, params string[] parameters)
            : this(1, type, parameters) { }
        
        public MiddlewareAttribute(
            int order,
            Type type,
            params string[] parameters
        )
        {
            Order = order;
            MiddlewareType = type;
            Parameters = parameters;
        }
    }
}
