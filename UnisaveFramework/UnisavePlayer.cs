using System;

namespace Unisave
{
    /// <summary>
    /// Represents a single player of your game
    /// </summary>
    public class UnisavePlayer : IEquatable<UnisavePlayer>
    {
        /// <summary>
        /// Player unique identifier
        /// </summary>
        public string Id { get; private set; }

        public UnisavePlayer(string id)
        {
            this.Id = id;
        }

        public override bool Equals(object that)
        {
            if (that == null || this.GetType() != that.GetType())
                return false;
            
            return this.Id == ((UnisavePlayer)that).Id;
        }
        
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(UnisavePlayer that)
        {
            return this.Id == that.Id;
        }

        public static bool operator ==(UnisavePlayer a, UnisavePlayer b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Id == b.Id;
        }

        public static bool operator !=(UnisavePlayer a, UnisavePlayer b)
        {
            return !(a == b);
        }
    }
}
