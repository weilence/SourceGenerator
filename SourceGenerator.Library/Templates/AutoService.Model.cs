using System.Collections.Generic;
using System.Linq;
using SourceGenerator.Library.Models;

namespace SourceGenerator.Library.Templates
{
    partial class AutoService
    {
        private readonly IEnumerable<AutoServiceItem> ClassList;

        public AutoService(IEnumerable<AutoServiceItem> classList)
        {
            ClassList = classList;
        }
    }

    public record AutoServiceItem
    {
        public string Class { get; set; }

        public ValueArray<string> Types { get; set; }

        public string Lifetime { get; set; }
    }
}