using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave.Networking;

namespace Unisave.Database
{
    /// <summary>
    /// Service that allows you to talk to the underlying database system
    /// 
    /// This service is recyclable - it can connect multiple times
    /// </summary>
    public class UnisaveDatabase : IDatabase
    {
        /// <summary>
        /// Instance of this service, that facades use
        /// It's set up by the bootstrapping logic
        /// </summary>
        public static UnisaveDatabase Instance { get; internal set; }

        /// <summary>
        /// TCP Client that is connected to a database proxy server
        /// </summary>
        private Client client;

        public UnisaveDatabase()
        {
        }

        /// <summary>
        /// Connect to a database proxy
        /// </summary>
        public void Connect(string executionId, string databaseProxyIp, int databaseProxyPort)
        {
            client = Client.Connect(databaseProxyIp, databaseProxyPort);

            client.SendJsonMessage(
                (int)ProxyMessage.ClientAuthenticates,
                new JsonObject().Add("executionId", executionId)
            );
            client.ReceiveJsonMessageAndExpectType((int)ProxyMessage.OK);
        }

        /// <summary>
        /// End the database connection
        /// </summary>
        public void Disconnect()
        {
            client.Disconnect();
        }

        /// <inheritdoc />
        public void SaveEntity(RawEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            client.SendJsonMessage(
                (int)ProxyMessage.SaveEntity,
                entity.ToJson()
            );

            JsonObject response = client.ReceiveJsonMessageAndExpectType((int)ProxyMessage.SaveEntityResponse);

            if (entity.id == null)
                entity.id = response["entityId"].AsString;
        }

        /// <inheritdoc />
        public RawEntity LoadEntity(string id)
        {
            client.SendJsonMessage(
                (int)ProxyMessage.LoadEntity,
                new JsonObject().Add("entityId", id)
            );
            
            JsonValue response = client.ReceiveJsonMessageAndExpectType((int)ProxyMessage.LoadEntityResponse);

            if (response.IsNull)
                return null;

            return RawEntity.FromJson(response.AsJsonObject);
        }

        /// <inheritdoc />
        public bool DeleteEntity(string id)
        {
            client.SendJsonMessage(
                (int)ProxyMessage.DeleteEntity,
                new JsonObject().Add("entityId", id)
            );

            return client.ReceiveJsonMessageAndExpectType((int)ProxyMessage.DeleteEntityResponse).AsBoolean;
        }

        /// <inheritdoc />
        public IEnumerable<RawEntity> QueryEntities(string entityType, EntityQuery query)
        {
            client.SendJsonMessage(
                (int)ProxyMessage.QueryEntities,
                new JsonObject()
                    .Add("entityType", entityType)
                    .Add("query", query.ToJson())
            );

            JsonArray response = client.ReceiveJsonMessageAndExpectType((int)ProxyMessage.QueryEntitiesResponse);

            return response.Select(x => RawEntity.FromJson(x));
        }
    }
}
