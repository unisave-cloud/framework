using System.Runtime.Serialization;

namespace Unisave.Serialization.Context
{
    /// <summary>
    /// Context for de-serialization
    /// </summary>
    public struct DeserializationContext
    {
        /// <summary>
        /// Why the serialization takes place
        /// </summary>
        public SerializationReason reason;
        
        /// <summary>
        /// Does the serialized data cross a security domain boundary?
        /// </summary>
        public SecurityDomainCrossing securityDomainCrossing;

        /// <summary>
        /// When set to true, no exception will be thrown when
        /// deserializing in the object or dynamic type scope
        /// </summary>
        public bool suppressInsecureDeserializationException;
        
        /// <summary>
        /// Returns a .NET streaming context corresponding
        /// to this deserialization context
        /// </summary>
        public StreamingContext GetStreamingContext()
        {
            return new StreamingContext(StreamingContextStates.All, this);
        }
        
        #region "Constants ___ToServer"
        
        public static readonly DeserializationContext ServerToServer
            = new DeserializationContext {
                reason = SerializationReason.Transfer,
                securityDomainCrossing = SecurityDomainCrossing.NoCrossing
            };
        
        public static readonly DeserializationContext ServerStorageToServer
            = new DeserializationContext {
                reason = SerializationReason.Storage,
                securityDomainCrossing = SecurityDomainCrossing.NoCrossing
            };
        
        public static readonly DeserializationContext ClientToServer
            = new DeserializationContext {
                reason = SerializationReason.Transfer,
                securityDomainCrossing = SecurityDomainCrossing.EnteringServer
            };
        
        public static readonly DeserializationContext ThirdPartyToServer
            = new DeserializationContext {
                reason = SerializationReason.Transfer,
                securityDomainCrossing = SecurityDomainCrossing.EnteringServer
            };
        
        public static readonly DeserializationContext ThirdPartyStorageToServer
            = new DeserializationContext {
                reason = SerializationReason.Storage,
                securityDomainCrossing = SecurityDomainCrossing.EnteringServer
            };
        
        #endregion
        
        #region "Constants ___ToClient"
        
        public static readonly DeserializationContext ClientToClient
            = new DeserializationContext {
                reason = SerializationReason.Transfer,
                securityDomainCrossing = SecurityDomainCrossing.NoCrossing
            };
        
        public static readonly DeserializationContext ClientStorageToClient
            = new DeserializationContext {
                reason = SerializationReason.Storage,
                securityDomainCrossing = SecurityDomainCrossing.NoCrossing
            };
        
        public static readonly DeserializationContext ServerToClient
            = new DeserializationContext {
                reason = SerializationReason.Transfer,
                securityDomainCrossing = SecurityDomainCrossing.LeavingServer
            };
        
        public static readonly DeserializationContext ThirdPartyToClient
            = new DeserializationContext {
                reason = SerializationReason.Transfer,
                securityDomainCrossing = SecurityDomainCrossing.NoCrossing
            };
        
        public static readonly DeserializationContext ThirdPartyStorageToClient
            = new DeserializationContext {
                reason = SerializationReason.Storage,
                securityDomainCrossing = SecurityDomainCrossing.NoCrossing
            };
        
        #endregion
    }
}