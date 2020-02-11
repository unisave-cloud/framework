using System;
using System.Linq;
using NUnit.Framework;
using Unisave.Arango.Query;

namespace FrameworkTests.Arango
{
    [TestFixture]
    [Ignore("Query verification is disabled")]
    public class QueryVerificationTest
    {
        [Test]
        public void ValidateReturn()
        {
            // one return is OK
            Assert.DoesNotThrow(() => {
                new AqlQuery()
                    .Return(() => 5);
            });
            
            // multiple returns are bad
            Assert.Throws<InvalidQueryException>(() => {
                new AqlQuery()
                    .Return(() => 5)
                    .Return(() => 10);
            });
            
            // no return (or other workhorse) is bad
            Assert.Throws<InvalidQueryException>(() => {
                new AqlQuery().ValidateQuery();
            });
            
            // return operation, that is not the last is bad
//            Assert.Throws<InvalidQueryException>(() => {
//                new AqlQuery()
//                    .Return(() => 5)
//                    .Filter(() => 10);
//            });
        }

        [Test]
        public void ValidateScopes()
        {
            // empty scope, empty parameters -> ok
            Assert.DoesNotThrow(() => {
                new AqlQuery()
                    .Return(() => 5);
            });
            
            // empty scope, expecting parameters -> bad
            Assert.Throws<InvalidQueryException>(() => {
                new AqlQuery()
                    .Return((x) => x);
            });
            
//            // having a scope and ignoring parameters -> ok
//            Assert.DoesNotThrow(() => {
//                new AqlQuery()
//                    .For("u").In("users").Do()
//                    .Return(() => 5);
//            });
//            
//            // having a scope and requesting proper parameters -> ok
//            Assert.DoesNotThrow(() => {
//                new AqlQuery()
//                    .For("u").In("users").Do()
//                    .Return((u) => u);
//            });
//            
//            // having a scope and requesting non-existing parameters -> bad
//            Assert.Throws<InvalidQueryException>(() => {
//                new AqlQuery()
//                    .For("u").In("users").Do()
//                    .Return((u, x) => u + 5 + x);
//            });
        }
    }
}