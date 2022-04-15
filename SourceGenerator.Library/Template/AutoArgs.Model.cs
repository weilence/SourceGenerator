namespace SourceGenerator.Library.Template
{
    public partial class AutoArgs
    {
        public ClassModel Model { get; set; }

        public AutoArgs(ClassModel model)
        {
            Model = model;
        }
    }
}