using Unisave.Utils;
using UnityEngine.Scripting;

namespace Unisave.Broadcasting
{
    [Preserve]
    public class ChannelSubscription
    {
        /// <summary>
        /// Full name of the channel to which the subscription applies
        /// </summary>
        [Preserve]
        public string ChannelName { get; }
        
        /// <summary>
        /// Session ID of the client, receiving the subscription
        /// </summary>
        [Preserve]
        public string SessionId { get; }
        
        /// <summary>
        /// ID to identify the subscription
        /// </summary>
        [Preserve]
        public string SubscriptionId { get; }

        [Preserve]
        public ChannelSubscription(string channelName, string sessionId)
        {
            ChannelName = channelName;
            SessionId = sessionId;
            SubscriptionId = Str.Random(16);
        }

        [Preserve]
        protected bool Equals(ChannelSubscription other)
        {
            return SubscriptionId == other.SubscriptionId;
        }

        [Preserve]
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ChannelSubscription) obj);
        }

        [Preserve]
        public override int GetHashCode()
        {
            return (SubscriptionId != null ? SubscriptionId.GetHashCode() : 0);
        }
    }
}