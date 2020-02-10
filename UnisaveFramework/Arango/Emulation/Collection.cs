using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Arango.Emulation
{
    /// <summary>
    /// Holds data of a single ArangoDB collection
    /// </summary>
    public class Collection : IEnumerable<JsonObject>
    {
        /// <summary>
        /// Documents of the collection
        /// </summary>
        private readonly List<JsonObject> documents = new List<JsonObject>();
        
        /// <summary>
        /// Name of the collection
        /// (used to generate document IDs)
        /// </summary>
        public string Name { get; internal set; }
        
        /// <summary>
        /// Type of the collection
        /// </summary>
        public CollectionType CollectionType { get; }

        public Collection(string name, CollectionType type)
        {
            Name = name;
            CollectionType = type;
        }

        /// <summary>
        /// Returns a document by its _key
        /// </summary>
        public JsonObject GetDocument(string key)
        {
            if (key == null)
                return null;
            
            JsonObject doc = documents.Find(i => i["_key"].AsString == key);

            return PrepareDocumentForReturn(doc);
        }

        /// <summary>
        /// Clones the document and adds pseudo fields
        /// </summary>
        private JsonObject PrepareDocumentForReturn(JsonObject document)
        {
            if (document == null)
                return null;

            // clone
            document = JsonReader.Parse(document.ToString());
            
            // pseudo fields
            document.Add("_id", Name + "/" + document["_key"]);
            
            return document;
        }
        
        /// <summary>
        /// Deletes collection content
        /// </summary>
        public void Truncate()
        {
            documents.Clear();
        }

        public IEnumerator<JsonObject> GetEnumerator()
        {
            return documents.Select(PrepareDocumentForReturn).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}