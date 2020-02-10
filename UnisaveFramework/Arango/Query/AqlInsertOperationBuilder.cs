using System;
using LightJson;

namespace Unisave.Arango.Query
{
    public class AqlInsertOperationBuilder
    {
        private readonly Action<AqlInsertOperationBuilder> doneCallback;
        private readonly AqlQuery query;
        
        public string CollectionName { get; private set; }
        public JsonObject Options { get; private set; }
        
        public AqlInsertOperationBuilder(
            AqlQuery query,
            Action<AqlInsertOperationBuilder> doneCallback
        )
        {
            this.query = query;
            this.doneCallback = doneCallback;
        }
        
        #region "Fluent API"

        /// <summary>
        /// Collection into which to insert
        /// </summary>
        public AqlQuery Into(string collectionName)
        {
            ArangoUtils.ValidateCollectionName(collectionName);
            CollectionName = collectionName;
            
            doneCallback.Invoke(this);
            
            return query;
        }

        /// <summary>
        /// Set an options value
        /// </summary>
        public AqlInsertOperationBuilder WithOption(
            string option,
            JsonValue value
        )
        {
            if (Options == null)
                Options = new JsonObject();

            Options.Add(option, value);
            
            return this;
        }

        /// <summary>
        /// Suppress errors due to violating unique key constraints
        /// </summary>
        public AqlInsertOperationBuilder IgnoreErrors(bool value = true)
            => WithOption("ignoreErrors", value);
        
        /// <summary>
        /// Make sure the data will be written to disk when the query finishes
        /// </summary>
        public AqlInsertOperationBuilder WaitForSync(bool value = true)
            => WithOption("waitForSync", value);
        
        /// <summary>
        /// Replace documents with the same key as the inserted one
        /// Without this option, an "unique constraint violated error" is raised
        /// </summary>
        public AqlInsertOperationBuilder Overwrite(bool value = true)
            => WithOption("overwrite", value);
        
        /// <summary>
        /// Request collection-level exclusive lock on RocksDB engine
        /// </summary>
        public AqlInsertOperationBuilder Exclusive(bool value = true)
            => WithOption("exclusive", value);

        #endregion
    }
}