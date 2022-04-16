using System;
using System.Text.Json;

namespace SourceGenerator.Library
{
    public class JsonUtils
    {
        public static string GetType(JsonValueKind kind, string value)
        {
            switch (kind)
            {
                case JsonValueKind.Number:
                    return value.Contains(".") ? "double" : "int";
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return "bool";
                case JsonValueKind.String:
                    return "string";
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    return null;
                default:
                    throw new ArgumentException($"Unknown value kind: {kind}");
            }
        }
    }
}