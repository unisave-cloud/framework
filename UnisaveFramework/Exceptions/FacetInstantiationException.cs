using System;

namespace Unisave.Exceptions
{
    [System.Serializable]
    public class FacetInstantiationException : System.Exception
    {
        public FacetInstantiationException() { }
        public FacetInstantiationException(string message) : base(message) { }
        public FacetInstantiationException(string message, System.Exception inner) : base(message, inner) { }
        protected FacetInstantiationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
