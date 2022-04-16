using System;
using System.Text.Json;

namespace SourceGenerator.Library
{
    public class JsonUtils
    {
        public static (string type, string value) GetTypeAndValue(JsonProperty property)
        {
            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Number:
                    var value = property.Value.ToString();
                    var type = value.Contains(".") ? "decimal" : "int";
                    return (type, value);
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return ("bool", property.Value.GetBoolean().ToString().ToLower());
                case JsonValueKind.String:
                    return ("string", property.Value.GetString());
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    return (null, null);
                default:
                    throw new ArgumentException($"Unknown value kind: {property.Value.ValueKind}");
            }
        }
    }
}