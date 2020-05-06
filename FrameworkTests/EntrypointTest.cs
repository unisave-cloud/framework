using FrameworkTests.TestingUtils;
using NUnit.Framework;
using Unisave.Exceptions;

namespace FrameworkTests
{
    [TestFixture]
    public class EntrypointTest
    {
        // TODO: this can test entrypoint by mocking a kernel (e.g. facet kernel)
        // TODO: and not by having a special method case
        
        // hmm: Not by mocking a kernel, but instead:
        // - make methodName -> kernel mapping dynamic, initialized
        // in the static constructor of Entrypoint class
        // - add an additional method that will be our test method
        
        // TODO: also exception serialization should be extracted into an
        // TODO: exception handler that should be tested separately
        
        [Test]
        public void ItReturnsNull()
        {
            ExecuteFramework.Begin()
                .SerializeExceptions()
                .Execute(@"
                    {
                        ""method"": ""entrypoint-test"",
                        ""methodParameters"": {
                            ""perform"": ""return-null""
                        }
                    }
                ")
                .AssertResultIs(@"
                    {
                        ""result"": ""ok"",
                        ""returned"": null,
                        ""special"": {}
                    }
                ");
        }
        
        [Test]
        public void ItReturns42()
        {
            ExecuteFramework.Begin()
                .SerializeExceptions()
                .Execute(@"
                    {
                        ""method"": ""entrypoint-test"",
                        ""methodParameters"": {
                            ""perform"": ""return-42""
                        }
                    }
                ")
                .AssertResultIs(@"
                    {
                        ""result"": ""ok"",
                        ""returned"": 42,
                        ""special"": {}
                    }
                ");
        }
        
        [Test]
        public void ItReturnsFooBar()
        {
            ExecuteFramework.Begin()
                .SerializeExceptions()
                .Execute(@"
                    {
                        ""method"": ""entrypoint-test"",
                        ""methodParameters"": {
                            ""perform"": ""return-foo-bar""
                        }
                    }
                ")
                .AssertResultIs(@"
                    {
                        ""result"": ""ok"",
                        ""returned"": {
                            ""foo"": ""bar""
                        },
                        ""special"": {}
                    }
                ")
                .AssertReturned(@"{""foo"": ""bar""}");
        }

        [Test]
        public void ItReturnsNullWithSpecialFooBar()
        {
            ExecuteFramework.Begin()
                .SerializeExceptions()
                .Execute(@"
                    {
                        ""method"": ""entrypoint-test"",
                        ""methodParameters"": {
                            ""perform"": ""return-null-with-special-foo-bar""
                        }
                    }
                ")
                .AssertResultIs(@"
                    {
                        ""result"": ""ok"",
                        ""returned"": null,
                        ""special"": {
                            ""foo"": ""bar""
                        }
                    }
                ")
                .AssertHasSpecial("foo", @"""bar""");
        }

        [Test]
        public void ItThrowsUnisaveException()
        {
            ExecuteFramework.Begin()
                .SerializeExceptions()
                .Execute(@"
                    {
                        ""method"": ""entrypoint-test"",
                        ""methodParameters"": {
                            ""perform"": ""throw-ue""
                        }
                    }
                ")
                .AssertExceptionThrown()
                .AssertExceptionIs<UnisaveException>();
        }
        
        [Test]
        public void ItThrowsUnisaveExceptionWithSpecialFooBar()
        {
            ExecuteFramework.Begin()
                .SerializeExceptions()
                .Execute(@"
                    {
                        ""method"": ""entrypoint-test"",
                        ""methodParameters"": {
                            ""perform"": ""throw-ue-with-special-foo-bar""
                        }
                    }
                ")
                .AssertExceptionThrown()
                .AssertExceptionIs<UnisaveException>()
                .AssertHasSpecial("foo", @"""bar""");
        }
    }
}