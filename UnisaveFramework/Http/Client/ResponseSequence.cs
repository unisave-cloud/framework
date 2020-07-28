using System.Collections.Generic;
using System.Net.Http;
using LightJson;

namespace Unisave.Http.Client
{
    public class ResponseSequence
    {
        protected bool failWhenEmpty = true;

        public ResponseSequence Push(
            JsonObject json,
            int status = 200,
            Dictionary<string, string> headers = null
        )
        {
            return this;
        }
        
        public ResponseSequence Push(
            string body,
            int status = 200,
            Dictionary<string, string> headers = null
        )
        {
            return this;
        }
        
        public ResponseSequence Push(
            HttpContent body = null,
            int status = 200,
            Dictionary<string, string> headers = null
        )
        {
            return this;
        }
        
        public ResponseSequence PushStatus(
            int status,
            Dictionary<string, string> headers = null
        )
        {
            return this;
        }
        
        public ResponseSequence PushResponse(Response response)
        {
            return this;
        }

        public ResponseSequence WhenEmpty(Response response)
        {
            return this;
        }

        public ResponseSequence DontFailWhenEmpty()
        {
            return WhenEmpty(Response.Create());
        }
    }
}