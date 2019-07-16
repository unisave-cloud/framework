using System;
using System.Collections.Generic;
using LightJson;

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

        public UnisaveDatabase()
        {
        }

        /// <summary>
        /// Connect to a database proxy
        /// </summary>
        public void Connect(string executionId, string databaseProxyIp, int databaseProxyPort)
        {

        }

        /// <summary>
        /// End the database connection
        /// </summary>
        public void Disconnect()
        {

        }

        /// <inheritdoc />
        public void SaveEntity(RawEntity entity)
        {
            
        }

        /// <inheritdoc />
        public RawEntity LoadEntity(string id)
        {
            return null;
        }

        /// <inheritdoc />
        public bool DeleteEntity(string id)
        {
            return false;
        }

        /// <inheritdoc />
        public IEnumerable<RawEntity> QueryEntities(string entityType, EntityQuery query)
        {
            return null;
        }
    }
}
