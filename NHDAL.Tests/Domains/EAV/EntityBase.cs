using NHDAL.Tests.Domains.EAV.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NHDAL.Tests.Domains.EAV
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityAttribute : System.Attribute
    {
        public string Name { get; set; } = string.Empty;
        public string ValueType { get; set; } = string.Empty;
    }

    public abstract class EntityBase
    {
        private readonly Dictionary<Guid, AttributeRecord> _attributes;
        private readonly Dictionary<string, Guid> _attributeNameToId;

        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }

        protected EntityBase(EntityRecord record, Dictionary<Guid, Entities.Attribute> attributeCatalog)
        {
            Id = record.Id;
            ProjectId = record.ProjectId;

            _attributes = record.AttributeMap
                .ToDictionary(pv => pv.Id);

            _attributeNameToId = attributeCatalog
                .ToDictionary(p => p.Value.Name, p => p.Key);

            MapParametersFromCatalog();
        }

        private void MapParametersFromCatalog()
        {
            foreach (var prop in GetType().GetProperties()
                .Where(p => p.GetCustomAttribute<EntityAttribute>() != null))
            {
                var attr = prop.GetCustomAttribute<EntityAttribute>()!;
                if (_attributeNameToId.TryGetValue(attr.Name, out var paramId))
                {
                    if (_attributes.TryGetValue(paramId, out var pv))
                    {
                        var value = Convert.ChangeType(pv.Value, prop.PropertyType);
                        prop.SetValue(this, value);
                    }
                }
            }
        }
    }
}
