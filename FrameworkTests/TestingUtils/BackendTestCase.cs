using NUnit.Framework;
using Unisave.Foundation;
using Unisave.Testing;

namespace FrameworkTests.TestingUtils
{
    public class BackendTestCase : BasicBackendTestCase
    {
        [SetUp]
        public virtual void SetUp()
        {
            var env = new Env();
            SetUpEnv(env);
            
            base.SetUp(
                typeof(BackendTestCase).Assembly.GetTypes(),
                env
            );
        }

        /// <summary>
        /// Sets up values for the env configuration
        /// </summary>
        protected void SetUpEnv(Env env)
        {
            env["SESSION_DRIVER"] = "memory";
        }
        
        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        //////////////////////////
        // Implement assertions //
        //////////////////////////
        
        protected override void AssertAreEqual(
            object expected, object actual, string message = null
        )
        {
            if (message == null)
                Assert.AreEqual(expected, actual);
            else
                Assert.AreEqual(expected, actual, message);
        }

        protected override void AssertIsNull(
            object subject, string message = null
        )
        {
            if (message == null)
                Assert.IsNull(subject);
            else
                Assert.IsNull(subject, message);
        }

        protected override void AssertIsNotNull(
            object subject, string message = null
        )
        {
            if (message == null)
                Assert.IsNotNull(subject);
            else
                Assert.IsNotNull(subject, message);
        }

        public override void AssertIsTrue(
            bool condition, string message = null
        )
        {
            if (message == null)
                Assert.IsTrue(condition);
            else
                Assert.IsTrue(condition, message);
        }

        public override void AssertIsFalse(
            bool condition, string message = null
        )
        {
            if (message == null)
                Assert.IsFalse(condition);
            else
                Assert.IsFalse(condition, message);
        }
    }
}