namespace SourceGenerator.Library.Templates
{
    partial class AutoArgs
    {
        private readonly AutoArgsModel Model;

        public AutoArgs(AutoArgsModel model)
        {
            Model = model;
        }
    }

    public class AutoArgsModel : ClassModel
    {
        public bool HasBase { get; set; }
    }
}