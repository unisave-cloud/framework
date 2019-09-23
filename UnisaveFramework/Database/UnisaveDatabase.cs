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
    [Obsolete("Database will be accessed through the script runner service interface")]
    public class UnisaveDatabase : IDatabase
    {
        /// <summary>
        /// Connect to a database proxy
        /// </summary>
        public void Connect(string executionId, string databaseProxyIp, int databaseProxyPort)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// End the database connection
        /// </summary>
        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SaveEntity(RawEntity entity)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public RawEntity LoadEntity(string id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<string> GetEntityOwners(string entityId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsEntityOwner(string entityId, string playerId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool DeleteEntity(string id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<RawEntity> QueryEntities(EntityQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
