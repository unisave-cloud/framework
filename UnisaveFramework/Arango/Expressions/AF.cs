using LightJson;

namespace Unisave.Arango.Expressions
{
    // ReSharper disable once InconsistentNaming
    public static class AF
    {
        [ArangoFunction("DOCUMENT")]
        public static JsonObject Document(string collection, string key)
            => new JsonObject();

        [ArangoFunction("CONCAT")]
        public static string Concat(JsonValue a, JsonValue b)
            => "";
        
        [ArangoFunction("CONCAT")]
        public static string Concat(JsonValue a, JsonValue b, JsonValue c)
            => "";
        
        [ArangoFunction("CONCAT")]
        public static string Concat(JsonValue a, JsonValue b, JsonValue c, JsonValue d)
            => "";
    }
}