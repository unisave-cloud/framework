using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave.Contracts;
using Unisave.Serialization;

namespace Unisave.Arango
{
    public class RawAqlQuery : IAqlQuery
    {
        private IArango arango;

        private readonly JsonObject bindParams = new JsonObject();

        private string aql;
        
        public RawAqlQuery(IArango arango, string aql)
        {
            this.arango = arango;
            this.aql = aql;
        }
        
        public IAqlQuery Bind(string parameter, JsonValue value)
        {
            bindParams[parameter] = value;
            
            return this;
        }

        public void Run() => Get();

        public List<TResult> GetAs<TResult>()
        {
            return Get()
                .Select(d => Serializer.FromJson<TResult>(d))
                .ToList();
        }

        public List<JsonValue> Get()
        {
            return arango.ExecuteRawAqlQuery(aql, bindParams);
        }

        public TResult FirstAs<TResult>() => GetAs<TResult>().FirstOrDefault();

        public JsonValue First() => Get().FirstOrDefault();
    }
}