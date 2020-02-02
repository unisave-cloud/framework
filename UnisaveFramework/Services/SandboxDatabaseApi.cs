using System;
using System.Collections.Generic;
using LightJson;
using Unisave.Contracts;
using Unisave.Database;
using Unisave.Database.Query;
using Unisave.Exceptions;
using Unisave.Runtime;

namespace Unisave.Services
{
    /// <summary>
    /// Communicates with the database proxy via the sandbox API
    /// </summary>
    public class SandboxDatabaseApi : IDatabase
    {
        private readonly ApiChannel channel;
        
        public SandboxDatabaseApi()
        {
            channel = new ApiChannel("db");
        }

        public SandboxDatabaseApi(ApiChannel channel)
        {
            this.channel = channel;
        }
        
        public void SaveEntity(RawEntity entity) // 100
        {
            JsonObject response = channel.SendJsonMessage(
                100,
                new JsonObject()
                    .Add("entity", entity.ToJson())
            );
            
            if (entity.id == null)
            {
                entity.id = response["entity_id"].AsString;
                entity.createdAt = DateTime.Parse(response["created_at"].AsString);
            }

            entity.updatedAt = DateTime.Parse(response["updated_at"].AsString);
            entity.ownerIds = EntityOwnerIds.FromJson(response["owner_ids"]);
        }

        public RawEntity LoadEntity(string id, string lockType = null) // 101
        {
            JsonObject response = channel.SendJsonMessage(
                101,
                new JsonObject()
                    .Add("entity_id", id)
                    .Add("lock_type", lockType)
            );
            
            if (response.ContainsKey("exception"))
            {
                if (response["exception"].AsString == "deadlock")
                    throw new DatabaseDeadlockException();
                
                throw new UnisaveException("Entity loading failed.");
            }
            
            return RawEntity.FromJson(
                response["entity"].AsJsonObject
            );
        }

        public IEnumerable<string> GetEntityOwners(string entityId) // 102, 103, 104
        {
            int enumeratorId = -1;
            
            try
            {
                enumeratorId = channel.SendJsonMessage(
                    102,
                    new JsonObject()
                        .Add("entity_id", entityId)
                )["enumerator_id"].AsInteger;

                while (true)
                {
                    JsonObject next = channel.SendJsonMessage(
                        103,
                        new JsonObject().Add("enumerator_id", enumeratorId)
                    );
                    
                    if (next["done"].AsBoolean)
                        break;

                    yield return next["item"].AsString;
                }
            }
            finally
            {
                channel.SendJsonMessage(
                    104,
                    new JsonObject().Add("enumerator_id", enumeratorId)
                );
            }
        }

        public bool IsEntityOwner(string entityId, string playerId) // 105
        {
            JsonObject response = channel.SendJsonMessage(
                105,
                new JsonObject()
                    .Add("entity_id", entityId)
                    .Add("player_id", playerId)
            );

            return response["is_owner"].AsBoolean;
        }

        public bool DeleteEntity(string id) // 106
        {
            JsonObject response = channel.SendJsonMessage(
                106,
                new JsonObject()
                    .Add("entity_id", id)
            );

            return response["was_deleted"].AsBoolean;
        }

        public IEnumerable<RawEntity> QueryEntities(EntityQuery query) // 107, 108, 109
        {
            int enumeratorId = -1;
            
            try
            {
                enumeratorId = channel.SendJsonMessage(
                    107,
                    new JsonObject()
                        .Add("query", query.ToJson())
                )["enumerator_id"].AsInteger;

                while (true)
                {
                    JsonObject next = channel.SendJsonMessage(
                        108,
                        new JsonObject().Add("enumerator_id", enumeratorId)
                    );
                    
                    if (next["done"].AsBoolean)
                        break;

                    yield return RawEntity.FromJson(next["item"]);
                }
            }
            finally
            {
                channel.SendJsonMessage(
                    109,
                    new JsonObject().Add("enumerator_id", enumeratorId)
                );
            }
        }

        public void StartTransaction() // 110
        {
            channel.SendJsonMessage(110, new JsonObject());
        }

        public void RollbackTransaction() // 111
        {
            channel.SendJsonMessage(111, new JsonObject());
        }

        public void CommitTransaction() // 112
        {
            channel.SendJsonMessage(112, new JsonObject());
        }

        public int TransactionLevel() // 113
        {
            JsonObject response = channel.SendJsonMessage(113, new JsonObject());
            return response["transaction_level"].AsInteger;
        }
    }
}