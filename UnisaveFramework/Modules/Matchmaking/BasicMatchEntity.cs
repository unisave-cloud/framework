using System.Collections.Generic;
using Unisave.Entities;

namespace Unisave.Modules.Matchmaking
{
    /// <summary>
    /// Base class for match entities produced by the basic matchmaker
    /// </summary>
    /// <typeparam name="TPlayerEntity">
    /// PlayerEntity participating in the matchmaking
    /// </typeparam>
    public class BasicMatchEntity<TPlayerEntity> : Entity
        where TPlayerEntity : Entity
    {
        /// <summary>
        /// Players who participate in the match
        /// </summary>
        [X]
        public List<EntityReference<TPlayerEntity>> Participants
        {
            get
            {
                // never return null
                if (participants == null)
                    participants = new List<EntityReference<TPlayerEntity>>();
                
                return participants;
            }

            set
            {
                participants = value;
            }
        }

        /// <summary>
        /// Backing field for the participants reference list
        /// </summary>
        private List<EntityReference<TPlayerEntity>> participants;
    }
}