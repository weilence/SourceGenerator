using System.Linq;

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

    public record AutoArgsModel : ClassModel
    {
        public bool HasBase => Fields.Any(m => m.InBase);
        public bool HasLogger => Fields.Any(m => m.Name == "_logger");
    }
}