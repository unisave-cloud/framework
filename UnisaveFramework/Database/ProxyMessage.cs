using System;

namespace Unisave.Database
{
    /// <summary>
    /// Types of messages sent to and from database proxy server
    /// </summary>
    public enum ProxyMessage
    {
        /// <summary>
        /// Generic OK response, that the previous message was understood and accepted
        /// May contain some response payload
        /// </summary>
        OK = 1,
        
        /// <summary>
        /// Client sends his execution id so the proxy gets to know the game id
        /// {"executionId": "..."}
        /// </summary>
        ClientAuthenticates = 2,

        /// <summary>
        /// Client wants to load an entity
        /// {"entityId": "..."}
        /// </summary>
        LoadEntity = 3,

        /// <summary>
        /// Server responds to entity load request
        /// RawEntity.ToJson / JsonValue.Null
        /// </summary>
        LoadEntityResponse = 4,

        /// <summary>
        /// Client wants to save or create an entity
        /// RawEntity.ToJson
        /// </summary>
        SaveEntity = 5,

        /// <summary>
        /// Response to entity saving / creation
        /// {"entityId": "...", "createdAt": "...", "updatedAt": "..."}
        /// </summary>
        SaveEntityResponse = 6,

        /// <summary>
        /// Client wants to delete an entity
        /// {"entityId": "..."}
        /// </summary>
        DeleteEntity = 7,

        /// <summary>
        /// Server deleted an entity, return json ture or false if it actually existed before
        /// true / false
        /// </summary>
        DeleteEntityResponse = 8,

        /// <summary>
        /// Client wants to query entities
        /// {"entityType": "...", "query": EntityQuery.ToJson}
        /// </summary>
        QueryEntities = 9,

        /// <summary>
        /// Server did an entity query and now returns the results
        /// [json array of RawEntity.ToJson items]
        /// </summary>
        QueryEntitiesResponse = 10,
    }
}
