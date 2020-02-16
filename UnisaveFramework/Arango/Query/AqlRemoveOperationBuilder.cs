using System;
using LightJson;

namespace Unisave.Arango.Query
{
    public class AqlRemoveOperationBuilder
    {
        private readonly Action<AqlRemoveOperationBuilder> doneCallback;
        private readonly AqlQuery query;
        
        public string CollectionName { get; private set; }
        public JsonObject Options { get; private set; }
        
        public AqlRemoveOperationBuilder(
            AqlQuery query,
            Action<AqlRemoveOperationBuilder> doneCallback
        )
        {
            this.query = query;
            this.doneCallback = doneCallback;
        }
        
        #region "Fluent API"

        /// <summary>
        /// Collection into which to insert
        /// </summary>
        public AqlQuery In(string collectionName)
        {
            ArangoUtils.ValidateCollectionName(collectionName);
            CollectionName = collectionName;
            
            doneCallback.Invoke(this);
            
            return query;
        }

        /// <summary>
        /// Set an options value
        /// </summary>
        public AqlRemoveOperationBuilder WithOption(
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
        /// Suppress errors due to non-existing keys
        /// </summary>
        public AqlRemoveOperationBuilder IgnoreErrors(bool value = true)
            => WithOption("ignoreErrors", value);
        
        /// <summary>
        /// Make sure the data will be written to disk when the query finishes
        /// </summary>
        public AqlRemoveOperationBuilder WaitForSync(bool value = true)
            => WithOption("waitForSync", value);
        
        /// <summary>
        /// Check document revisions when doing the update to detect conflicts
        /// </summary>
        public AqlRemoveOperationBuilder CheckRevs(bool check = true)
            => WithOption("ignoreRevs", !check);
        
        /// <summary>
        /// Ignore document revisions (conflict detection)
        /// </summary>
        public AqlRemoveOperationBuilder IgnoreRevs(bool ignore = true)
            => WithOption("ignoreRevs", ignore);
        
        /// <summary>
        /// Request collection-level exclusive lock on RocksDB engine
        /// </summary>
        public AqlRemoveOperationBuilder Exclusive(bool value = true)
            => WithOption("exclusive", value);

        #endregion
    }
}