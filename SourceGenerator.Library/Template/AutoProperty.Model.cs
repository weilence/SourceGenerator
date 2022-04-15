namespace SourceGenerator.Library.Template
{
    public partial class AutoProperty
    {
        private readonly ClassModel Model;

        public AutoProperty(ClassModel model)
        {
            this.Model = model;
        }
        
        private string GetPropertyName(Field column)
        {
            return char.ToUpper(column.Name[1]) + column.Name.Substring(2);
        }
    }
}