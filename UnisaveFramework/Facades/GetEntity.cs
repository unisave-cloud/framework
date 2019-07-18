using System;
using System.Collections.Generic;
using Unisave.Database;

namespace Unisave
{
    /// <summary>
    /// Facade for accessing database entities
    /// </summary>
    public static class GetEntity<E> where E : Entity, new()
    {
        public static IEnumerable<E> Get() // for entities belonging to the game
            => QueryBuilder<E>.Create().Get();

        public static E First() // for entities belonging to the game
            => QueryBuilder<E>.Create().First();

        public static QueryBuilder<E> OfPlayer(UnisavePlayer player)
            => QueryBuilder<E>.Create().OfPlayer(player);

        public static QueryBuilder<E> OfPlayers(IEnumerable<UnisavePlayer> players)
            => QueryBuilder<E>.Create().OfPlayers(players);
    }
}
