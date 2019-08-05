using System;
using LightJson;
using Unisave.Exceptions;

namespace Unisave.Runtime
{
    public static class MigrationCall
    {
        /// <summary>
        /// How did the bootstraping fail
        /// These codes are important outside of this assembly
        /// </summary>
        internal enum ErrorType
        {
            // 1xx ... nailing down the exact migration to call
            MigrationAmbiguous = 101,
            MigrationNotFound = 102,

            // 2xx ... other
            MigrationCreationException = 201
        }

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
        ///     // how did the execution go? If there was an exception, was it caused by
        ///     // the game code, or worse by the boostrapping framework code?
        ///     "result": "ok" / "game-exception" / "error",
        /// 
        ///     // WHEN "ok"
        /// 
        ///     nothing more
        /// 
        ///     // WHEN "game-exception"
        /// 
        ///     "exceptionAsString": "...",
        /// 
        ///     // WHEN "error"
        /// 
        ///     "errorType": see the ErrorType enumeration (not all errors need to be saved to logs)
        ///     "messageForUser": "This will be displayed to the user in unity editor",
        ///     "messageForLog": "This massive explanation will be put into unisave logs."
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
                ErrorType type;

                switch (e.Problem)
                {
                    case MigrationInstantiationException.ProblemType.MigrationAmbiguous:
                        type = ErrorType.MigrationAmbiguous;
                        break;

                    case MigrationInstantiationException.ProblemType.MigrationNotFound:
                        type = ErrorType.MigrationNotFound;
                        break;

                    case MigrationInstantiationException.ProblemType.Other:
                        type = ErrorType.MigrationCreationException;
                        break;
                    
                    default:
                        // take down everything so that I notice and fix this
                        throw new NotImplementedException(
                            $"Method {nameof(Migration.CreateMigrationTypeByVersionJump)} threw an unknown error {e.Problem}.",
                            e
                        );
                }

                return Error(type, e.Message);
            }

            // run the migration

            try
            {
                migration.Run();
            }
            catch (Exception e)
            {
                return GameException(e);
            }

            // build the response

            var response = new JsonObject();
            response.Add("result", "ok");
            return response;
        }

        /// <summary>
        /// Formats an error response
        /// </summary>
        private static JsonObject Error(ErrorType type, string messageForUser, string messageForLog = null)
        {
            var result = new JsonObject();
            result.Add("result", "error");
            result.Add("errorType", (int)type);
            result.Add("messageForUser", messageForUser);
            result.Add("messageForLog", messageForLog ?? messageForUser);
            return result;
        }

        /// <summary>
        /// Formats an exception response
        /// </summary>
        private static JsonObject GameException(Exception e)
        {
            var result = new JsonObject();
            result.Add("result", "game-exception");
            result.Add("exceptionAsString", e.ToString());
            return result;
        }
    }
}
