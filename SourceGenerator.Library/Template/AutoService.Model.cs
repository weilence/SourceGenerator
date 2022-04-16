using System.Collections.Generic;

namespace SourceGenerator.Library.Template
{
    public partial class AutoService
    {
        public AutoServiceModel Model { get; set; }

        public AutoService(AutoServiceModel model)
        {
            Model = model;
        }
    }

    public class AutoServiceModel
    {
        public List<AutoServiceModelItem> ClassList { get; set; }
    }

    public class AutoServiceModelItem
    {
        public string Namespace { get; set; }

        public string Class { get; set; }
    }
}