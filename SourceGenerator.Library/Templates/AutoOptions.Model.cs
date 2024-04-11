using System.Collections.Generic;
using SourceGenerator.Library.Models;

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

    public record AutoOptionsModel
    {
        public string Namespace { get; set; }

        public ClassInfo Class { get; set; }

        public string Path { get; set; }
    }

    public record ClassInfo
    {
        public string Name { get; set; }

        public ValueArray<PropertyInfo> Properties { get; set; }
    }

    public record PropertyInfo
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public ClassInfo Class { get; set; }
    }
}