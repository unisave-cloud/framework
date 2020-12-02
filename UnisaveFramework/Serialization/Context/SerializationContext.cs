namespace Unisave.Serialization.Context
{
    /// <summary>
    /// Context for serialization
    /// </summary>
    public class SerializationContext
    {
        /// <summary>
        /// Why the serialization takes place
        /// </summary>
        public SerializationReason reason
            = SerializationReason.Storage;

        /// <summary>
        /// When should the $type attribute be added to serialized objects
        /// </summary>
        public TypeSerialization typeSerialization
            = TypeSerialization.DuringPolymorphism;

        /// <summary>
        /// Build the default serialization context
        /// (used in most unit tests and potentially used by user code)
        /// </summary>
        public static SerializationContext DefaultContext()
        {
            return new SerializationContext();
        }

        /// <summary>
        /// Build the serialization context used for facet calling
        /// </summary>
        public static SerializationContext FacetCallingContext()
        {
            return new SerializationContext {
                reason = SerializationReason.Transmission
            };
        }
        
        /// <summary>
        /// Build the serialization context used for entity storage
        /// </summary>
        public static SerializationContext EntitySavingContext()
        {
            return new SerializationContext {
                reason = SerializationReason.Storage
            };
        }
        
        /// <summary>
        /// Build the serialization context used for message broadcasting
        /// </summary>
        public static SerializationContext BroadcastingContext()
        {
            return new SerializationContext {
                reason = SerializationReason.Transmission
            };
        }
    }
}