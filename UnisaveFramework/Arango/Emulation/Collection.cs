using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using LightJson.Serialization;
using Unisave.Utils;

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
        private readonly Dictionary<string, JsonObject> documents
            = new Dictionary<string, JsonObject>();
        
        /// <summary>
        /// Name of the collection
        /// (used to generate document IDs)
        /// </summary>
        public string Name { get; internal set; }
        
        /// <summary>
        /// Type of the collection
        /// </summary>
        public CollectionType CollectionType { get; }

        /// <summary>
        /// Next auto increment key
        /// </summary>
        private long nextKey = 1;

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
            
            if (!documents.ContainsKey(key))
                return null;

            return PrepareDocumentForReturn(key, documents[key]);
        }

        /// <summary>
        /// Inserts a document into the collection with options and
        /// returns the exact value that will be stored after the insert
        /// </summary>
        public JsonObject InsertDocument(JsonObject document, JsonObject options)
        {
            if (document == null)
                document = new JsonObject();
            
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            
            if (CollectionType == CollectionType.Edge)
                ValidateEdgeAttributes(document);
            
            string key = document["_key"].AsString ?? GenerateNewKey();
            
            ArangoUtils.ValidateDocumentKey(key);

            if (documents.ContainsKey(key) && !options["overwrite"])
            {
                // do not insert, but do not throw either
                if (options["ignoreErrors"])
                    return null; // ignore write
                
                throw new ArangoException(
                    409, 1210, "unique constraint violated"
                );
            }

            JsonObject insertedDocument = PrepareDocumentForWrite(
                GenerateNewRevision(),
                document
            );

            documents[key] = insertedDocument;
            return insertedDocument;
        }

        /// <summary>
        /// Replace a document in the collection
        /// </summary>
        public JsonObject ReplaceDocument(
            string key,
            JsonObject document,
            JsonObject options
        )
        {
            if (document == null)
                document = new JsonObject();
            
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (CollectionType == CollectionType.Edge)
                ValidateEdgeAttributes(document);

            JsonObject oldDocument = GetDocument(key);
            
            // validate the key
            if (oldDocument == null)
            {
                if (options["ignoreErrors"])
                    return null; // ignore write
                
                throw new ArangoException(404, 1202, "document not found");
            }
            
            // validate revision
            bool checkRevs = options["checkRevs"].IsBoolean
                             && options["checkRevs"].AsBoolean;
            if (checkRevs
                && oldDocument["_rev"].AsString != document["_rev"].AsString)
            {
                // revisions differ and we DO check them
                
                if (options["ignoreErrors"])
                    return null; // ignore write
                
                throw new ArangoException(409, 1200, "conflict");
            }
            
            JsonObject insertedDocument = PrepareDocumentForWrite(
                GenerateNewRevision(),
                document
            );

            documents[key] = insertedDocument;
            return insertedDocument;
        }

        /// <summary>
        /// Removes a document from the collection
        /// </summary>
        /// <param name="key"></param>
        /// <param name="rev"></param>
        /// <param name="options"></param>
        public void RemoveDocument(string key, string rev, JsonObject options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            JsonObject oldDocument = GetDocument(key);
            
            // validate the key
            if (oldDocument == null)
            {
                if (options["ignoreErrors"])
                    return; // ignore write
                
                throw new ArangoException(404, 1202, "document not found");
            }
            
            // validate revision
            bool checkRevs = options["checkRevs"].IsBoolean
                             && options["checkRevs"].AsBoolean;
            if (checkRevs
                && oldDocument["_rev"].AsString != rev)
            {
                // revisions differ and we DO check them
                
                if (options["ignoreErrors"])
                    return; // ignore write
                
                throw new ArangoException(409, 1200, "conflict");
            }

            documents.Remove(key);
        }

        private void ValidateEdgeAttributes(JsonObject document)
        {
            string from = document["_from"].AsString ?? "";
            string to = document["_to"].AsString ?? "";
            
            if (!from.Contains('/') || !to.Contains('/'))
                throw new ArangoException(
                    400, 1233, "edge attribute missing or invalid"
                );
        }

        protected string GenerateNewKey()
        {
            return (nextKey++).ToString();
        }

        protected string GenerateNewRevision()
        {
            return Str.Random(8);
        }

        /// <summary>
        /// Clones the document and adds pseudo fields
        /// </summary>
        private JsonObject PrepareDocumentForReturn(
            string key,
            JsonObject document
        )
        {
            if (document == null)
                return null;

            // clone
            document = JsonReader.Parse(document.ToString());
            
            // pseudo fields
            document.Add("_key", key);
            document.Add("_id", Name + "/" + key);
            
            return document;
        }

        /// <summary>
        /// Clones and removes unnecessary pseudo-fields
        /// </summary>
        private JsonObject PrepareDocumentForWrite(
            string revision,
            JsonObject document
        )
        {
            // handle null & clone
            if (document == null)
                document = new JsonObject();
            else
                document = JsonReader.Parse(document.ToString());
            
            // remove pseudo fields
            document.Remove("_key");
            document.Remove("_id");

            // set revision
            document["_rev"] = revision;
            
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
            return documents
                .Select(pair => PrepareDocumentForReturn(pair.Key, pair.Value))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}