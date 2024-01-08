using System.Collections.Generic;

namespace SourceGenerator.Library.Templates
{
    partial class AutoOptions
    {
        private readonly AutoOptionsModel Model;

        public AutoOptions(AutoOptionsModel Model)
        {
            this.Model = Model;
        }
    }

    public class AutoOptionsModel
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