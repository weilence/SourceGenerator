using SourceGenerator.Library.Models;

namespace SourceGenerator.Library.Templates
{
    public record ClassModel
    {
        public string Namespace { get; set; }

        public string Class { get; set; }

        public ValueArray<Field> Fields { get; set; }
        public ValueArray<string> Usings { get; set; }
    }

    public class Field
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public bool InBase { get; set; }
        public bool Ignore { get; set; }
    }
}