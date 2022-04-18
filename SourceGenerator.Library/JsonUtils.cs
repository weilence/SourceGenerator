using System;
using System.Collections.Generic;
using System.Text.Json;
using SourceGenerator.Library.Template;

namespace SourceGenerator.Library
{
    public class JsonUtils
    {
        public static ClassInfo ParseJson(JsonElement jsonElement)
        {
            var classInfo = new ClassInfo()
            {
                Properties = new List<PropertyInfo>()
            };

            foreach (var jsonProperty in jsonElement.EnumerateObject())
            {
                var property = ParseProperty(jsonProperty);
                if (property == null)
                {
                    continue;
                }

                classInfo.Properties.Add(property);
            }

            return classInfo;
        }

        public static PropertyInfo ParseProperty(JsonProperty property)
        {
            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Number:
                    var value = property.Value.ToString();
                    return new PropertyInfo()
                    {
                        Type = value.Contains(".") ? "decimal" : "int",
                        Name = property.Name,
                        Value = value,
                    };
                case JsonValueKind.True:
                    return new PropertyInfo()
                    {
                        Type = "bool",
                        Name = property.Name,
                        Value = "true",
                    };
                case JsonValueKind.False:
                    return new PropertyInfo()
                    {
                        Type = "bool",
                        Name = property.Name,
                        Value = "false",
                    };
                case JsonValueKind.String:
                    return new PropertyInfo()
                    {
                        Type = "string",
                        Name = property.Name,
                        Value = property.Value.GetString(),
                    };
                case JsonValueKind.Object:
                    var classInfo = ParseJson(property.Value);
                    classInfo.Name = property.Name;
                    return new PropertyInfo()
                    {
                        Type = property.Name,
                        Name = property.Name,
                        Value = property.Name,
                        Class = classInfo,
                    };
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                case JsonValueKind.Array:
                    return null;
                default:
                    throw new ArgumentException($"Unknown value kind: {property.Value.ValueKind}");
            }
        }
    }
}