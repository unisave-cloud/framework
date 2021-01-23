using LightJson;
using Unisave.Serialization;
using Unisave.Serialization.Context;

namespace Unisave.Arango
{
    /// <summary>
    /// Holds document id
    /// Both parts may be null, but if not, they are kept valid
    /// </summary>
    public struct DocumentId : IUnisaveSerializable
    {
        /// <summary>
        /// Name of the collection
        /// </summary>
        public string Collection
        {
            get => collection;
            
            set
            {
                if (value != null)
                    ArangoUtils.ValidateCollectionName(value);
                
                collection = value;
            }
        }
        
        private string collection;

        /// <summary>
        /// Key of the document
        /// </summary>
        public string Key
        {
            get => key;

            set
            {
                if (value != null)
                    ArangoUtils.ValidateDocumentKey(value);

                key = value;
            }
        }

        private string key;
        
        /// <summary>
        /// Creates a null document id
        /// </summary>
        public static DocumentId Null => new DocumentId();
        
        /// <summary>
        /// Returns the document ID as a string
        /// Is null if any of the two parts are null
        /// </summary>
        public string Id
            => collection == null || key == null
            ? null
            : Collection + "/" + Key;

        /// <summary>
        /// Throws arango exception if there is a null part
        /// </summary>
        /// <exception cref="ArangoException"></exception>
        public void ThrowIfHasNull()
        {
            if (string.IsNullOrEmpty(Collection) || string.IsNullOrEmpty(Key))
                throw new ArangoException(
                    400, 1221, "invalid document id: " + ToString()
                );
        }
        
        /// <summary>
        /// Parse and validate document id
        /// Return null ID of the input is null
        /// </summary>
        public static DocumentId Parse(string id)
        {
            if (id == null)
                return DocumentId.Null;
            
            int i = id.IndexOf('/');
            
            if (i == -1)
                throw new ArangoException(
                    400, 1221,
                    $"Given document id '{id}' is invalid"
                );

            return new DocumentId {
                Collection = id.Substring(0, i),
                Key = id.Substring(i + 1)
            };
        }
        
        #region "IUnisaveSerializable"

        private DocumentId(JsonValue json, DeserializationContext context)
        {
            this = Parse(json.AsString);
        }
        
        JsonValue IUnisaveSerializable.ToJson(SerializationContext context)
        {
            return Id;
        }
        
        #endregion
    }
}