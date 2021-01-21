namespace Unisave.Serialization.Context
{
    /// <summary>
    /// When should type "$type" attribute be added to serialized objects
    /// </summary>
    public enum TypeSerialization
    {
        /// <summary>
        /// Only when we're serializing a subtype of the context type
        /// (e.g. we're serializing a MyClass as a System.Object)
        /// </summary>
        DuringPolymorphism = 0,
        
        /// <summary>
        /// Never add the $type attribute
        /// </summary>
        Never = 1,
        
        /// <summary>
        /// Always add the $type attribute
        /// </summary>
        Always = 2
    }
}