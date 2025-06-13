using System.Collections.Generic;

namespace SourceGenerator.Library.Models
{
    public record ClassInfo
    {
        public string Namespace { get; set; }

        public string ClassName { get; set; }

        public List<FieldMetadata> Fields { get; set; } = [];
        public List<UsingInfo> Usings { get; set; } = [];
    }

    public record UsingInfo
    {
        public string Alias { get; set; }
        public string Name { get; set; }

        public static implicit operator UsingInfo(string name) => new UsingInfo { Name = name };
    }

    public class FieldMetadata
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public bool Ignore { get; set; }
        public string ServiceKey { get; set; }
    }
}