using System.Collections.Generic;

namespace SourceGenerator.Library.Template
{
    public partial class AppSettings
    {
        private readonly AppSettingsModel Model;

        public AppSettings(AppSettingsModel model)
        {
            this.Model = model;
        }
    }

    public class AppSettingsModel
    {
        public string Namespace { get; set; }

        public string Class { get; set; } = "AppSettings";

        public List<Column> Columns { get; set; }
    }

    public class Column
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }
    }
}