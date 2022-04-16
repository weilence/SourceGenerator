using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.Common;
using SourceGenerator.Library.Template;

namespace SourceGenerator.Library
{
    [Generator]
    public class AutoServiceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new ClassAttributeReceiver(new List<string> { nameof(ServiceAttribute), ServiceAttribute.Name }));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (ClassAttributeReceiver)context.SyntaxReceiver;
            var syntaxList = receiver.AttributeSyntaxList;

            if (syntaxList.Count == 0)
            {
                return;
            }

            var classList = new List<AutoServiceModelItem>();
            foreach (var classDeclarationSyntax in syntaxList)
            {
                var baseNamespaceDeclarationSyntax =
                    classDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();
                var namespaceName = SyntaxUtils.GetName(baseNamespaceDeclarationSyntax);
                var className = SyntaxUtils.GetName(classDeclarationSyntax);

                classList.Add(new AutoServiceModelItem()
                {
                    Class = className,
                    Namespace = namespaceName,
                });
            }

            if (classList.Count == 0)
            {
                return;
            }

            var autoService = new AutoService(new AutoServiceModel()
            {
                ClassList = classList
            });

            context.AddSource($"AutoServiceExtension.Class.g.cs", autoService.TransformText());
        }
    }
}