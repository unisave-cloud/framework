using FrameworkTests.TestingUtils;
using NUnit.Framework;
using Unisave.Exceptions;

namespace FrameworkTests
{
    [TestFixture]
    public class EntrypointTest
    {
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
        
        [Test]
        public void ItThrowsInvalidMethodParametersException()
        {
            ExecuteFramework.Begin()
                .SerializeExceptions()
                .Execute(@"
                    {
                        ""method"": ""entrypoint-test"",
                        ""methodParameters"": {
                            ""perform"": ""throw-invalid-method-parameters-ex""
                        }
                    }
                ")
                .AssertExceptionThrown()
                .AssertExceptionIs<InvalidMethodParametersException>();
        }
    }
}