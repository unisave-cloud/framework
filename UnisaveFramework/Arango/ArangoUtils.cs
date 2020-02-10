using System.Linq;

namespace Unisave.Arango
{
    public static class ArangoUtils
    {
        public static void ValidateCollectionName(string name)
        {
            const string charset = "abcdefghijklmnopqrstuvwxyz"
                + "0123456789"
                + "_-";

            bool charactersOk = name.All(
                c => charset.Contains(char.ToLower(c))
            );

            bool lengthOk = name.Length <= 256;

            if (!charactersOk || !lengthOk)
                throw new ArangoException(
                    400, 1208, "illegal collection name"
                );
        }

        public static void ValidateDocumentKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArangoException(
                    400, 1221, "illegal document key"
                );
            
            const string charset = "abcdefghijklmnopqrstuvwxyz"
                                   + "0123456789"
                                   + "_-:.@()+,=;$!*'%";

            bool charactersOk = key.All(
                c => charset.Contains(char.ToLower(c))
            );

            bool lengthOk = key.Length <= 254;
            
            if (!charactersOk || !lengthOk)
                throw new ArangoException(
                    400, 1221, "illegal document key"
                );
        }

        /// <summary>
        /// Parses document id into its two components
        /// </summary>
        public static DocumentId ParseDocumentId(string id)
        {
            if (id == null)
                throw new ArangoException(
                    400, 1221, "null document id is invalid"
                );
            
            int i = id.IndexOf('/');
            
            if (i == -1)
                throw new ArangoException(
                    400, 1221,
                    $"Given document id '{id}' is invalid"
                );
            
            var documentId = new DocumentId();
            documentId.collection = id.Substring(0, i);
            documentId.key = id.Substring(i + 1);
            
            ValidateCollectionName(documentId.collection);
            ValidateDocumentKey(documentId.key);
            
            return documentId;
        }

        public class DocumentId
        {
            public string collection;
            public string key;
        }
    }
}