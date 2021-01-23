using NUnit.Framework;
using Unisave.Arango;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Unisave
{
    [TestFixture]
    public class DocumentIdSerializationTest
    {
        [Test]
        public void ItSerializesDocumentIds()
        {
            Assert.AreEqual(
                "\"foo\\/bar\"",
                Serializer.ToJsonString(DocumentId.Parse("foo/bar"))
            );
            
            Assert.AreEqual(
                "null",
                Serializer.ToJsonString(DocumentId.Null)
            );
        }

        [Test]
        public void ItDeserializesDocumentIds()
        {
            var d = Serializer.FromJsonString<DocumentId>("\"foo/bar\"");
            Assert.AreEqual("foo/bar", d.Id);
            
            d = Serializer.FromJsonString<DocumentId>("null");
            Assert.AreEqual(null, d.Id);

            Assert.Throws<ArangoException>(() => {
                Serializer.FromJsonString<DocumentId>("\"lorem/ipsum/dolor\"");
            });
        }
    }
}