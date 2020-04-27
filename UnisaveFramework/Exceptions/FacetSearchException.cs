using System;

namespace Unisave.Exceptions
{
    [Serializable]
    public class FacetSearchException : System.Exception
    {
        public FacetSearchException() { }
        public FacetSearchException(string message) : base(message) { }
        public FacetSearchException(string message, System.Exception inner) : base(message, inner) { }
        protected FacetSearchException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
