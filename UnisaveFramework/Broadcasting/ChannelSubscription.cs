using Unisave.Utils;

namespace Unisave.Broadcasting
{
    public class ChannelSubscription
    {
        /// <summary>
        /// Full name of the channel to which the subscription applies
        /// </summary>
        public string ChannelName { get; }
        
        /// <summary>
        /// Session ID of the client, receiving the subscription
        /// </summary>
        public string SessionId { get; }
        
        /// <summary>
        /// ID to identify the subscription
        /// </summary>
        public string SubscriptionId { get; }

        public ChannelSubscription(string channelName, string sessionId)
        {
            ChannelName = channelName;
            SessionId = sessionId;
            SubscriptionId = Str.Random(16);
        }

        protected bool Equals(ChannelSubscription other)
        {
            return SubscriptionId == other.SubscriptionId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ChannelSubscription) obj);
        }

        public override int GetHashCode()
        {
            return (SubscriptionId != null ? SubscriptionId.GetHashCode() : 0);
        }
    }
}