using LightJson;
using LightJson.Serialization;

namespace Unisave.Runtime
{
    /// <summary>
    /// Holds special values returned by a method execution
    /// </summary>
    public class SpecialValues
    {
        private readonly JsonObject values = new JsonObject();

        /// <summary>
        /// Adds a value to special values
        /// </summary>
        public void Add(string key, JsonValue value)
        {
            values[key] = value;
        }

        /// <summary>
        /// Reads a special value
        /// </summary>
        public JsonValue Read(string key)
        {
            return values[key];
        }

        /// <summary>
        /// Returns the special values as a JSON object
        /// </summary>
        public JsonObject ToJsonObject()
        {
            return JsonReader.Parse(values.ToString());
        }
    }
}