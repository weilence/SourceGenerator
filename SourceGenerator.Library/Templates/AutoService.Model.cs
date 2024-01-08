using System.Collections.Generic;

namespace SourceGenerator.Library.Templates
{
    partial class AutoService
    {
        private readonly List<AutoServiceItem> ClassList;

        public AutoService(List<AutoServiceItem> classList)
        {
            ClassList = classList;
        }
    }

    public class AutoServiceItem
    {
        public string Class { get; set; }

        public List<string> Types { get; set; }

        public string Lifetime { get; set; }
    }
}