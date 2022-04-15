using System.Collections.Generic;

namespace SourceGenerator.Library.Template
{
    public class ClassModel
    {
        public string Namespace { get; set; }

        public string Class { get; set; }

        public List<Field> Fields { get; set; }
    }

    public class Field
    {
        public string Type { get; set; }

        public string Name { get; set; }
    }
}