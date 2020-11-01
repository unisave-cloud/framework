using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightJson;
using Unisave.Serialization;
using Unisave.Serialization.Context;

namespace Unisave.Entities
{
    /// <summary>
    /// Mapping between attribute names and type members
    /// for a given entity type
    /// </summary>
    public class AttributeMapping
    {
        private class Mapping
        {
            public string attribute;
            public MemberInfo member;
        }
        
        private List<Mapping> mappings = new List<Mapping>();

        private Type entityType;

        public AttributeMapping(Type entityType)
        {
            this.entityType = entityType;
        }

        public void Add(MemberInfo member)
        {
            mappings.Add(new Mapping {
                attribute = EntityUtils.MemberInfoToDocumentAttributeName(
                    entityType,
                    member
                ),
                member = member
            });
        }

        public IEnumerable<string> GetAttributeNames()
        {
            return mappings.Select(m => m.attribute);
        }

        public MemberInfo GetMemberInfo(string attribute)
        {
            return mappings
                .FirstOrDefault(m => m.attribute == attribute)
                ?.member;
        }

        public void SetAttributeValue(
            Entity entity,
            string attribute,
            JsonValue value
        )
        {
            MemberInfo member = GetMemberInfo(attribute);
            
            switch (member)
            {
                case PropertyInfo pi:
                    pi.SetValue(
                        entity,
                        Serializer.FromJson(
                            value,
                            pi.PropertyType,
                            DeserializationContext.EntitySavingContext()
                        )
                    );
                    break;
                
                case FieldInfo fi:
                    fi.SetValue(
                        entity,
                        Serializer.FromJson(
                            value,
                            fi.FieldType,
                            DeserializationContext.EntitySavingContext()
                        )
                    );
                    break;
            }
        }
        
        public JsonValue GetAttributeValue(
            Entity entity,
            string attribute
        )
        {
            MemberInfo member = GetMemberInfo(attribute);

            object value = null;
            
            switch (member)
            {
                case PropertyInfo pi:
                    value = pi.GetValue(entity);
                    break;
                
                case FieldInfo fi:
                    value = fi.GetValue(entity);
                    break;
            }
            
            return Serializer.ToJson(
                value,
                null,
                SerializationContext.EntitySavingContext()
            );
        }
    }
}