using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave.Serialization;

namespace Unisave.Components.Matchmaking
{
    public class BasicMatchmakerEntity : Entity
    {
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