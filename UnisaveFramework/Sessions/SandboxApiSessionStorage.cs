using LightJson;
using Unisave.Runtime;

namespace Unisave.Sessions
{
    // TODO: replace this with session over IArango
    
    public class SandboxApiSessionStorage : ISessionStorage
    {
        private readonly ApiChannel channel;
        
        public SandboxApiSessionStorage() : this(new ApiChannel("session")) { }

        public SandboxApiSessionStorage(ApiChannel channel)
        {
            this.channel = channel;
        }

        // [100]
        public JsonObject Load(string sessionId)
        {
            JsonObject response = channel.SendJsonMessage(
                100,
                new JsonObject()
                    .Add("sessionId", sessionId)
            );

            return response["sessionData"];
        }

        // [200]
        public void Store(
            string sessionId,
            JsonObject sessionData,
            int lifetime
        )
        {
            channel.SendJsonMessage(
                200,
                new JsonObject()
                    .Add("sessionId", sessionId)
                    .Add("lifetime", lifetime)
                    .Add("sessionData", sessionData)
            );
        }
    }
}