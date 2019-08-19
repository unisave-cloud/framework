using System;
using LightJson;
using Unisave.Exceptions;

namespace Unisave.Runtime
{
    public static class MigrationCall
    {
        /// <summary>
        /// Bootstrap a facet call
        /// </summary>
        /// <param name="executionParameters">
        /// Execution parameters as json
        /// {
        ///     "migrateFrom": 1,
        ///     "migrateTo": 2
        /// }    
        /// </param>
        /// <param name="gameAssemblyTypes">
        /// Game assembly types to look through to find the requested migration class
        /// </param>
        /// <returns>
        /// Json string containing result of the execution.
        /// {
        ///     // empty json object
        /// }    
        /// </returns>
        public static JsonObject Start(JsonObject executionParameters, Type[] gameAssemblyTypes)
        {
            // extract arguments

            int migrateFrom = executionParameters["migrateFrom"];
            int migrateTo = executionParameters["migrateTo"];

            // find the requested migration

            Migration migration;
            try
            {
                migration = Migration.CreateMigrationTypeByVersionJump(migrateFrom, migrateTo, gameAssemblyTypes);
            }
            catch (MigrationInstantiationException e)
            {
                throw new InvalidMethodParametersException("Migration wasn't found.", e);
            }

            // run the migration

            try
            {
                migration.Run();
            }
            catch (Exception e)
            {
                throw new GameScriptException(e);
            }

            // build the response

            return new JsonObject();
        }
    }
}
