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
                default:
                    throw new ArgumentException($"Unknown value kind: {kind}");
            }
        }
    }
}