using System;
using System.Reflection;
using System.Linq;
using Unisave;
using Unisave.Exceptions;
using Unisave.Serialization;
using LightJson;

namespace Unisave.Database
{
    /// <summary>
    /// Crawls entity properties and loads / saves them
    /// </summary>
    public class EntityCrawler
    {
        /*
            TODO: create a static cache for different entity types where will
            be extracted only the final properties and fields.
         */

        private readonly Type entityType;

        public EntityCrawler(Type entityType)
        {
            if (!typeof(Entity).IsAssignableFrom(entityType))
                throw new ArgumentException("Provided type does not inherit from Entity type.");

            this.entityType = entityType;
        }

        public JsonObject ExtractData(object entityInstance)
        {
            if (entityInstance.GetType() != entityType)
                throw new ArgumentException(
                    "Given instance is not of the entity type "
                    + "this crawler was initialized to."
                );

            JsonObject jsonData = new JsonObject();

            foreach (PropertyInfo pi in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (pi.GetCustomAttributes().Any(a => a.GetType() == typeof(XAttribute)))
                {
                    if (typeof(Entity).IsAssignableFrom(pi.PropertyType))
                        throw new UnisaveException(
                            "Entities cannot contain other entities inside, "
                            + "the logic is not yet implemented inside Unisave. "
                            + "Use a string containing the target entity id instead. "
                            + "I'm planning to add support for this in the future however."
                        );

                    jsonData.Add(pi.Name, Serializer.ToJson(pi.GetValue(entityInstance)));
                }
            }

            return jsonData;
        }

        public void InsertData(object entityInstance, JsonObject jsonData)
        {
            if (entityInstance.GetType() != entityType)
                throw new ArgumentException(
                    "Given instance is not of the entity type "
                    + "this crawler was initialized to."
                );

            foreach (PropertyInfo pi in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (pi.GetCustomAttributes().Any(a => a.GetType() == typeof(XAttribute)))
                {
                    if (typeof(Entity).IsAssignableFrom(pi.PropertyType))
                        throw new UnisaveException(
                            "Entities cannot contain other entities inside, "
                            + "the logic is not yet implemented inside Unisave. "
                            + "Use a string containing the target entity id instead. "
                            + "I'm planning to add support for this in the future however."
                        );

                    pi.SetValue(entityInstance, Serializer.FromJson(jsonData[pi.Name], pi.PropertyType));
                }
            }
        }
    }
}
