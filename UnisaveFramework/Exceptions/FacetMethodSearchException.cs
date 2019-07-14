using System;

namespace Unisave.Exceptions
{
    public class FacetMethodSearchException : System.Exception
    {
        /// <summary>
        /// Possible ways the facet type finding may fail
        /// </summary>
        public enum ProblemType
        {
            MethodNameAmbiguous,
            MethodDoesNotExist,
            MethodNotPublic
        }

        public ProblemType Problem { get; private set; }

        public FacetMethodSearchException(string message, ProblemType problem) : base(message)
        {
            this.Problem = problem;
        }
    }
}
