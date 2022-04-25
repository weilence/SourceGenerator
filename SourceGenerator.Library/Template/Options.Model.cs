using System.Collections.Generic;

namespace SourceGenerator.Library.Template
{
    public class OptionsModel
    {
        public string Namespace { get; set; }

        public ClassInfo Class { get; set; }
    }

    public class ClassInfo
    {
        public string Name { get; set; }
        
        public List<PropertyInfo> Properties { get; set; }
    }

    public class PropertyInfo
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }
        
        public ClassInfo Class { get; set; }
    }
}