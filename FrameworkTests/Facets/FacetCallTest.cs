using NUnit.Framework;
using System;
using LightJson;
using FrameworkTests.Facets.Stubs;
using FrameworkTests.TestingUtils;
using Unisave.Exceptions;

namespace FrameworkTests.Facets
{
    [TestFixture]
    public class FacetCallTest
    {
        [Test]
        public void ItRunsMyProcedure()
        {
            SomeFacet.flag = false;

            ExecuteFramework.Begin()
                .Execute(@"
                    {
                        ""method"": ""facet-call"",
                        ""methodParameters"": {
                            ""facetName"": ""SomeFacet"",
                            ""methodName"": ""MyProcedure"",
                            ""arguments"": []
                        }
                    }
                ")
                .AssertReturned(@"null")
                .AssertHasSpecial("sessionId");

            Assert.IsTrue(SomeFacet.flag);
        }

        [Test]
        public void ItRunsMyParametrizedProcedure()
        {
            SomeFacet.flag = false;

            ExecuteFramework.Begin()
                .Execute(@"
                    {
                        ""method"": ""facet-call"",
                        ""methodParameters"": {
                            ""facetName"": ""SomeFacet"",
                            ""methodName"": ""MyParametrizedProcedure"",
                            ""arguments"": [true]
                        }
                    }
                ")
                .AssertSuccess();
            Assert.IsTrue(SomeFacet.flag);
            
            ExecuteFramework.Begin()
                .Execute(@"
                    {
                        ""method"": ""facet-call"",
                        ""methodParameters"": {
                            ""facetName"": ""SomeFacet"",
                            ""methodName"": ""MyParametrizedProcedure"",
                            ""arguments"": [false]
                        }
                    }
                ")
                .AssertSuccess();
            Assert.IsFalse(SomeFacet.flag);
        }

        [Test]
        public void ItChecksParentFacet()
        {
            var e = Assert.Catch<InvalidMethodParametersException>(() => {
                ExecuteFramework.Begin()
                    .Execute(@"
                        {
                            ""method"": ""facet-call"",
                            ""methodParameters"": {
                                ""facetName"": ""WrongFacet"",
                                ""methodName"": ""MyProcedure"",
                                ""arguments"": []
                            }
                        }
                    ")
                    .AssertSuccess();
            });
            
            StringAssert.Contains("Facet class wasn't found.", e.Message);
        }

        [Test]
        public void ItChecksMethodExistence()
        {
            var e = Assert.Catch<InvalidMethodParametersException>(() => {
                ExecuteFramework.Begin()
                    .Execute(@"
                        {
                            ""method"": ""facet-call"",
                            ""methodParameters"": {
                                ""facetName"": ""SomeFacet"",
                                ""methodName"": ""NonExistingMethod"",
                                ""arguments"": []
                            }
                        }
                    ")
                    .AssertSuccess();
            });
            
            StringAssert.Contains("doesn't have public method called", e.ToString());
        }

        [Test]
        public void ItChecksAmbiguousMethods()
        {
            var e = Assert.Catch<InvalidMethodParametersException>(() => {
                ExecuteFramework.Begin()
                    .Execute(@"
                        {
                            ""method"": ""facet-call"",
                            ""methodParameters"": {
                                ""facetName"": ""SomeFacet"",
                                ""methodName"": ""AmbiguousMethod"",
                                ""arguments"": []
                            }
                        }
                    ")
                    .AssertSuccess();
            });
            
            StringAssert.Contains("has multiple methods called", e.ToString());
        }

        [Test]
        public void ItChecksPublicMethods()
        {
            var e = Assert.Catch<InvalidMethodParametersException>(() => {
                ExecuteFramework.Begin()
                    .Execute(@"
                        {
                            ""method"": ""facet-call"",
                            ""methodParameters"": {
                                ""facetName"": ""SomeFacet"",
                                ""methodName"": ""PrivateProcedure"",
                                ""arguments"": []
                            }
                        }
                    ")
                    .AssertSuccess();
            });
            
            StringAssert.Contains("has to be public in order to be called", e.ToString());
        }

        [Test]
        public void ItRunsFunctions()
        {
            ExecuteFramework.Begin()
                .Execute(@"
                    {
                        ""method"": ""facet-call"",
                        ""methodParameters"": {
                            ""facetName"": ""SomeFacet"",
                            ""methodName"": ""SquaringFunction"",
                            ""arguments"": [5]
                        }
                    }
                ")
                .AssertReturned(@"25");
        }
    }
}
