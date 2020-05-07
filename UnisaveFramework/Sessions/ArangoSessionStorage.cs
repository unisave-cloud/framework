using System;
using System.Linq;
using LightJson;
using Unisave.Arango;
using Unisave.Arango.Expressions;
using Unisave.Arango.Query;
using Unisave.Contracts;

namespace Unisave.Sessions
{
    /// <summary>
    /// Provides storage for session data inside an ArangoDB collection
    /// </summary>
    public class ArangoSessionStorage : ISessionStorage
    {
        /// <summary>
        /// Name of the collection to be used
        /// </summary>
        private const string CollectionName = "u_sessions";
        
        /*
         * Structure of a database document:
         * {
         *   "_key": "session-id-as-given-to-us"
         *   "expiresAt": "2020-05-07T11:21:25.249Z",
         *   "sessionData": { ... }
         * }
         */
        
        private readonly IArango arango;
        private readonly ILog log;
        
        public ArangoSessionStorage(IArango arango, ILog log)
        {
            this.arango = arango
                ?? throw new ArgumentNullException(nameof(arango));
            
            this.log = log
                ?? throw new ArgumentNullException(nameof(log));
        }
        
        public JsonObject Load(string sessionId)
        {
            try
            {
                JsonObject document = arango.ExecuteAqlQuery(
                    new AqlQuery()
                        .Return(() => AF.Document(CollectionName, sessionId))
                ).First().AsJsonObject;

                return document?["sessionData"].AsJsonObject
                       ?? new JsonObject();
            }
            catch (ArangoException e) when (e.ErrorNumber == 1203)
            {
                // collection or view not found
                return new JsonObject();
            }
        }

        public void Store(
            string sessionId,
            JsonObject sessionData,
            int lifetime
        )
        {
            try
            {
                AttemptStore(sessionId, sessionData, lifetime);
            }
            catch (ArangoException e) when (e.ErrorNumber == 1203)
            {
                // collection or view not found
                CreateCollection();
                AttemptStore(sessionId, sessionData, lifetime);
            }
            catch (ArangoException e) when (e.ErrorNumber == 1200)
            {
                // write-write conflict
                log.Error(
                    "Session wasn't saved due to a write-write conflict " +
                    "(other process was saving the same session at the " +
                    "same time).",
                    e
                );
            }
        }
        
        /// <summary>
        /// Attempts to just store the data without handling any
        /// arango exception that might occur.
        /// </summary>
        /// <param name="sessionId">Identifier for this session</param>
        /// <param name="sessionData">Data to be stored</param>
        /// <param name="lifetime">Lifetime for the session in seconds</param>
        /// <exception cref="ArangoException"></exception>
        private void AttemptStore(
            string sessionId,
            JsonObject sessionData,
            int lifetime
        )
        {
            DateTime expiresAt = DateTime.UtcNow.AddSeconds(lifetime);

            // datetime as produced by the arango DATE_ISO8601(...) function
            string expiresAtString = expiresAt.ToString(
                "yyyy-MM-dd\\THH:mm:ss.fff\\Z"
            );
            
            JsonObject document = new JsonObject()
                .Add("_key", sessionId)
                .Add("expiresAt", expiresAtString)
                .Add("sessionData", sessionData);

            arango.ExecuteAqlQuery(
                new AqlQuery()
                    .Insert(document).Overwrite().Into(CollectionName)
            );
        }
        
        /// <summary>
        /// Creates the sessions collection
        /// </summary>
        private void CreateCollection()
        {
            arango.CreateCollection(CollectionName, CollectionType.Document);
            
            log.Info(
                $"[Unisave] {CollectionName} collection has been created. " +
                "This collection is used for storing session data."
            );
            
            arango.CreateIndex(
                CollectionName,
                IndexType.TTL,
                new string[] { "expiresAt" },
                new JsonObject()
                    .Add("expireAfter", 0)
            );
            
            log.Info(
                $"[Unisave] TTL index on collection {CollectionName} has " +
                "been created. It makes sure that old sessions get deleted."
            );
        }
    }
}