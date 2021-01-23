using System;

namespace Unisave
{
    /*
        Here are defined all the custom attributes of Unisave
     */

    /// <summary>
    /// Marks an auto-property or a field to not be serialized
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,
        Inherited = false,
        AllowMultiple = false
    )]
    public class DontSerializeAttribute : Attribute
    {
        public DontSerializeAttribute()
        { }
    }
    
    /// <summary>
    /// Marks an auto-property or a field to be serialized under different name
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,
        Inherited = false,
        AllowMultiple = false
    )]
    public class SerializeAsAttribute : Attribute
    {
        public string SerializedName { get; }

        public SerializeAsAttribute(string serializedName)
        {
            SerializedName = serializedName;
        }
    }
    
    /// <summary>
    /// Marks an auto-property or a field to not leave the server security domain
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,
        Inherited = false,
        AllowMultiple = false
    )]
    public class DontLeaveServerAttribute : Attribute
    {
        public DontLeaveServerAttribute()
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
