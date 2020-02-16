using System;
using System.Collections.Generic;
using Unisave.Contracts;
using Unisave.Database;
using Unisave.Database.Query;
using Unisave.Foundation;
using Unisave.Runtime;
using Unisave.Services;

namespace Unisave
{
    /// <summary>
    /// Facade for accessing database entities
    /// </summary>
    [Obsolete]
    public static class GetEntity<E> where E : Entity, new()
    {
        public static IEnumerable<E> Get() // for entities belonging to the game
            => QueryBuilder<E>.Create().Get();

        public static E First() // for entities belonging to the game
            => QueryBuilder<E>.Create().First();

        /// <summary>
        /// Obtain an entity by it's ID
        /// </summary>
        public static E Find(string entityId)
        {
            var database = Application.Default.Resolve<IDatabase>();
            
            return (E)Entity.FromRawEntity(
                database.LoadEntity(entityId),
                typeof(E)
            );
        }

        public static QueryBuilder<E> OfPlayer(UnisavePlayer player)
            => QueryBuilder<E>.Create().OfPlayer(player);

        public static QueryBuilder<E> OfPlayers(IEnumerable<UnisavePlayer> players)
            => QueryBuilder<E>.Create().OfPlayers(players);

        public static QueryBuilder<E> OfAnyPlayers()
            => QueryBuilder<E>.Create().AndOthers();

        public static QueryBuilder<E> Where(string jsonPath, object value)
            => QueryBuilder<E>.Create().Where(jsonPath, value);

        public static QueryBuilder<E> Where(string jsonPath, string op, object value)
            => QueryBuilder<E>.Create().Where(jsonPath, op, value);
    }
}
