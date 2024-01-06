using System.Collections.Generic;

namespace SourceGenerator.Library.Templates
{
    public class AutoServiceModel
    {
        public List<AutoServiceModelItem> ClassList { get; set; }
    }

    public class AutoServiceModelItem
    {
        public string Class { get; set; }

        public List<string> Types { get; set; }

        public string Lifetime { get; set; }
    }
}