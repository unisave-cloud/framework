using System;

namespace Unisave
{
    /// <summary>
    /// Represents a single player of your game
    /// </summary>
    public class UnisavePlayer
    {
        /// <summary>
        /// Player unique identifier
        /// </summary>
        public string Id { get; private set; }

        public UnisavePlayer(string id)
        {
            this.Id = id;
        }
    }
}
