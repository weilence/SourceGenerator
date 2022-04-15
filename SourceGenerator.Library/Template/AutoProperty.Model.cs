using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SourceGenerator.Library.Template
{
    public partial class AutoProperty
    {
        private readonly AutoPropertyModel Model;

        public AutoProperty(AutoPropertyModel model)
        {
            this.Model = model;
        }
        
        private string GetPropertyName(Field column)
        {
            return char.ToUpper(column.Name[1]) + column.Name.Substring(2);
        }
    }

    public class AutoPropertyModel
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