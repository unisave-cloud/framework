using System.Runtime.Serialization;

namespace Unisave.Serialization.Context
{
    /// <summary>
    /// Context for serialization
    /// </summary>
    public struct SerializationContext
    {
        /// <summary>
        /// Why the serialization takes place
        /// </summary>
        public SerializationReason reason;

        /// <summary>
        /// When should the $type attribute be added to serialized objects
        /// </summary>
        public TypeSerialization typeSerialization;

        /// <summary>
        /// Does the serialized data cross a security domain boundary?
        /// </summary>
        public SecurityDomainCrossing securityDomainCrossing;

        /// <summary>
        /// When true, a byte[] (binary data) will be serialized
        /// like any other array - to a JSON array. Otherwise
        /// a Base64 encoded string is used.
        /// </summary>
        public bool serializeBinaryAsByteArray;
        
        /// <summary>
        /// Returns a .NET streaming context corresponding
        /// to this serialization context
        /// </summary>
        public StreamingContext GetStreamingContext()
        {
            return new StreamingContext(StreamingContextStates.All, this);
        }
        
        #region "Constants ServerTo___"
        
        public static readonly SerializationContext ServerToServer
            = new SerializationContext {
                reason = SerializationReason.Transfer,
                typeSerialization = TypeSerialization.DuringPolymorphism,
                securityDomainCrossing = SecurityDomainCrossing.NoCrossing
            };
        
        public static readonly SerializationContext ServerToServerStorage
            = new SerializationContext {
                reason = SerializationReason.Storage,
                typeSerialization = TypeSerialization.DuringPolymorphism,
                securityDomainCrossing = SecurityDomainCrossing.NoCrossing
            };
        
        public static readonly SerializationContext ServerToClient
            = new SerializationContext {
                reason = SerializationReason.Transfer,
                typeSerialization = TypeSerialization.DuringPolymorphism,
                securityDomainCrossing = SecurityDomainCrossing.LeavingServer
            };
        
        public static readonly SerializationContext ServerToThirdParty
            = new SerializationContext {
                reason = SerializationReason.Transfer,
                typeSerialization = TypeSerialization.DuringPolymorphism,
                securityDomainCrossing = SecurityDomainCrossing.LeavingServer
            };
        
        public static readonly SerializationContext ServerToThirdPartyStorage
            = new SerializationContext {
                reason = SerializationReason.Storage,
                typeSerialization = TypeSerialization.DuringPolymorphism,
                securityDomainCrossing = SecurityDomainCrossing.LeavingServer
            };
        
        #endregion
        
        #region "Constants ClientTo___"
        
        public static readonly SerializationContext ClientToClient
            = new SerializationContext {
                reason = SerializationReason.Transfer,
                typeSerialization = TypeSerialization.DuringPolymorphism,
                securityDomainCrossing = SecurityDomainCrossing.NoCrossing
            };
        
        public static readonly SerializationContext ClientToClientStorage
            = new SerializationContext {
                reason = SerializationReason.Storage,
                typeSerialization = TypeSerialization.DuringPolymorphism,
                securityDomainCrossing = SecurityDomainCrossing.NoCrossing
            };
        
        public static readonly SerializationContext ClientToServer
            = new SerializationContext {
                reason = SerializationReason.Transfer,
                typeSerialization = TypeSerialization.DuringPolymorphism,
                securityDomainCrossing = SecurityDomainCrossing.EnteringServer
            };
        
        public static readonly SerializationContext ClientToThirdParty
            = new SerializationContext {
                reason = SerializationReason.Transfer,
                typeSerialization = TypeSerialization.DuringPolymorphism,
                securityDomainCrossing = SecurityDomainCrossing.NoCrossing
            };
        
        public static readonly SerializationContext ClientToThirdPartyStorage
            = new SerializationContext {
                reason = SerializationReason.Storage,
                typeSerialization = TypeSerialization.DuringPolymorphism,
                securityDomainCrossing = SecurityDomainCrossing.NoCrossing
            };
        
        #endregion
    }
}