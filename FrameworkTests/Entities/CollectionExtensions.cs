using LightJson;
using LightJson.Serialization;
using Unisave.Arango.Emulation;

namespace FrameworkTests.Entities
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds a JSON document into a collection
        /// Strings can be single-quoted
        /// </summary>
        public static Collection Add(
            this Collection collection,
            string jsonDocument
        )
        {
            jsonDocument = jsonDocument.Replace("'", "\"");
            
            collection.InsertDocument(
                JsonReader.Parse(jsonDocument),
                new JsonObject()
            );
            
            return collection;
        }
    }
}