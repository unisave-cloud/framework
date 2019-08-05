using System;

namespace Unisave.Exceptions
{
    [System.Serializable]
    public class MigrationInstantiationException : System.Exception
    {
        /// <summary>
        /// Possible ways the migration instantiation may fail
        /// </summary>
        public enum ProblemType
        {
            MigrationAmbiguous,
            MigrationNotFound,
            Other
        }

        public ProblemType Problem { get; private set; }

        public MigrationInstantiationException(string message, ProblemType problem) : base(message)
        {
            this.Problem = problem;
        }
    }
}
