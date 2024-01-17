using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.Library.Receivers;

namespace SourceGenerator.Library.Generators
{
    public abstract class BaseGenerator : ISourceGenerator
    {
        private readonly AttributeSyntaxReceiver _receiver;

        public BaseGenerator(IEnumerable<string> attributeNames)
        {
            _receiver = new AttributeSyntaxReceiver(attributeNames.ToList());
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => _receiver);
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxList = _receiver.AttributeSyntaxList;

            if (syntaxList.Count == 0)
            {
                return;
            }

            foreach (var attributeSyntax in syntaxList)
            {
                Execute(context, attributeSyntax);
            }

            AfterExecute(context);
        }

        protected abstract void Execute(GeneratorExecutionContext context, AttributeSyntax attributeSyntax);

        protected virtual void AfterExecute(GeneratorExecutionContext context)
        {
        }
    }
}