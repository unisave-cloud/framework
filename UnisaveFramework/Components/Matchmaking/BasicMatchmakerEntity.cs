using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave.Serialization;

namespace Unisave.Components.Matchmaking
{
    public class BasicMatchmakerEntity : Entity
    {
        public const int CurrentVersion = 1;

        /// <summary>
        /// Entity version
        /// 
        /// When incremented, the entity gets recreated.
        /// This is to allow me to change entity schema
        /// without making migrations since the data inside
        /// the entity is not really that persistent.
        /// </summary>
        [X]
        public int Version { get; set; } = CurrentVersion;
        
        /// <summary>
        /// What matchmaker this entity belongs to
        /// </summary>
        [X] public string MatchmakerName { get; set; }
        
        /// <summary>
        /// Waiting tickets
        /// </summary>
        [X] public List<JsonObject> Tickets { get; set; }
            = new List<JsonObject>();

        public List<T> DeserializeTickets<T>() where T : BasicMatchmakerTicket
        {
            return Tickets.Select(t => Serializer.FromJson<T>(t)).ToList();
        }

        public void SerializeTickets<T>(List<T> tickets)
            where T : BasicMatchmakerTicket
        {
            Tickets = tickets
                .Select(t => Serializer.ToJson(t).AsJsonObject)
                .ToList();
        }
    }
}