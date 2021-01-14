using System.Runtime.Serialization;

namespace Unisave.Serialization.Context
{
    /// <summary>
    /// Context for de-serialization
    /// </summary>
    public class DeserializationContext
    {
        /// <summary>
        /// Why the serialization takes place
        /// </summary>
        public SerializationReason reason
            = SerializationReason.Storage;
        
        /// <summary>
        /// Returns a .NET streaming context corresponding
        /// to this deserialization context
        /// </summary>
        public StreamingContext GetStreamingContext()
        {
            return new StreamingContext(StreamingContextStates.All, this);
        }
        
        /// <summary>
        /// Build the default de-serialization context
        /// (used in most unit tests and potentially used by user code)
        /// </summary>
        public static DeserializationContext DefaultContext()
        {
            return new DeserializationContext();
        }
        
        /// <summary>
        /// Build the de-serialization context used for facet calling
        /// </summary>
        public static DeserializationContext FacetCallingContext()
        {
            return new DeserializationContext {
                reason = SerializationReason.Transmission
            };
        }
        
        /// <summary>
        /// Build the de-serialization context used for entity storage
        /// </summary>
        public static DeserializationContext EntitySavingContext()
        {
            return new DeserializationContext {
                reason = SerializationReason.Storage
            };
        }
        
        /// <summary>
        /// Build the de-serialization context used for message broadcasting
        /// </summary>
        public static DeserializationContext BroadcastingContext()
        {
            return new DeserializationContext {
                reason = SerializationReason.Transmission
            };
        }
    }
}