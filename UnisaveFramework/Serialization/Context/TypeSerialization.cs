namespace Unisave.Serialization.Context
{
    /// <summary>
    /// When should type "$type" attribute be added to serialized objects
    /// </summary>
    public enum TypeSerialization
    {
        /// <summary>
        /// Never add the $type attribute
        /// </summary>
        Never = 1,
        
        /// <summary>
        /// Only when we're serializing a subtype of the context type
        /// (e.g. we're serializing a MyClass as a System.Object)
        /// </summary>
        DuringPolymorphism = 2,
        
        /// <summary>
        /// Always add the $type attribute
        /// </summary>
        Always = 3
    }
}