using LightJson;

namespace Unisave.Arango.Expressions
{
    // ReSharper disable once InconsistentNaming
    public static class AF
    {
        [ArangoFunction("DOCUMENT")]
        public static JsonObject Document(string collection, string key)
        {
            return new JsonObject().Add("dummy", "object");
        }
    }
}