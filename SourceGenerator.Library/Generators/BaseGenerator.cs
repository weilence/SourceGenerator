using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.Library.Receivers;

namespace SourceGenerator.Library.Generators
{
    public abstract class BaseGenerator : ISourceGenerator
    {
        private readonly IEnumerable<string> _attributeNames;

        public BaseGenerator(IEnumerable<string> attributeNames)
        {
            _attributeNames = attributeNames;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new AttributeSyntaxReceiver(_attributeNames.ToList()));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (AttributeSyntaxReceiver)context.SyntaxReceiver;
            if (receiver == null)
            {
                return;
            }

            var syntaxList = receiver.AttributeSyntaxList;

            if (syntaxList.Count == 0)
            {
                return;
            }

            BeforeExecute(context);

            foreach (var attributeSyntax in syntaxList)
            {
                Execute(context, attributeSyntax);
            }

            AfterExecute(context);
        }

        protected virtual void BeforeExecute(GeneratorExecutionContext context)
        {
        }

        protected abstract void Execute(GeneratorExecutionContext context, AttributeSyntax attributeSyntax);

        protected virtual void AfterExecute(GeneratorExecutionContext context)
        {
        }
    }
}