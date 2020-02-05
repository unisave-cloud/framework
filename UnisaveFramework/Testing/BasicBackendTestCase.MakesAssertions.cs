namespace Unisave.Testing
{
    public partial class BasicBackendTestCase
    {
        protected abstract void AssertAreEqual(
            object expected,
            object actual,
            string message = null
        );
        
        protected abstract void AssertIsNull(
            object subject,
            string message = null
        );
        
        protected abstract void AssertIsNotNull(
            object subject,
            string message = null
        );

        public abstract void AssertIsTrue(
            bool condition,
            string message = null
        );
        
        public abstract void AssertIsFalse(
            bool condition,
            string message = null
        );
    }
}