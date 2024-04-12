namespace SourceGenerator.Library.Models
{
    public record ClassModel
    {
        public string Namespace { get; set; }

        public string Class { get; set; }

        public ValueArray<Field> Fields { get; set; }
        public ValueArray<Using> Usings { get; set; }
    }

    public record Using
    {
        public string Alias { get; set; }
        public string Name { get; set; }

        public static implicit operator Using(string name) => new Using { Name = name };
    }

    public class Field
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public bool InBase { get; set; }
        public bool Ignore { get; set; }
    }
}