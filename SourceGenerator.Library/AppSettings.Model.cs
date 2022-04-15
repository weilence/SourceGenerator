using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SourceGenerator.Library
{
    public partial class AppSettings
    {
        private readonly AppSettingsModel Model;

        public AppSettings(AppSettingsModel model)
        {
            this.Model = model;
        }

        private string GetType(Column column)
        {
            switch (column.Type)
            {
                case JTokenType.String:
                    return "string";
                case JTokenType.Integer:
                    return "int";
                case JTokenType.Boolean:
                    return "bool";
                default:
                    return null;
            }
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

        public JTokenType? Type { get; set; }

        public string Value { get; set; }
    }
}