using System;

namespace Unisave.Exceptions
{
    public class FacetSearchException : System.Exception
    {
        /// <summary>
        /// Possible ways the facet type finding may fail
        /// </summary>
        public enum ProblemType
        {
            FacetNameAmbiguous,
            FacetNotFound
        }

        public ProblemType Problem { get; private set; }

        public FacetSearchException(string message, ProblemType problem) : base(message)
        {
            this.Problem = problem;
        }
    }
}
