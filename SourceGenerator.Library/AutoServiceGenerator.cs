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
                var attributeSyntax =
                    SyntaxUtils.GetAttribute(classDeclarationSyntax, name => receiver.Names.Contains(name));
                if (attributeSyntax == null)
                {
                    continue;
                }

                var baseNamespaceDeclarationSyntax =
                    classDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();
                var namespaceName = SyntaxUtils.GetName(baseNamespaceDeclarationSyntax);
                var className = SyntaxUtils.GetName(classDeclarationSyntax);

                var semanticModel = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

                var modelItem = new AutoServiceModelItem()
                {
                    Class = namespaceName + "." + className,
                };

                var argumentListArguments = attributeSyntax.ArgumentList?.Arguments;
                if (argumentListArguments != null)
                {
                    foreach (var argumentSyntax in argumentListArguments)
                    {
                        if (SyntaxUtils.GetName(argumentSyntax.NameEquals) == nameof(ServiceAttribute.Type))
                        {
                            if (argumentSyntax.Expression is TypeOfExpressionSyntax typeOfExpressionSyntax)
                            {
                                var typeSyntax = typeOfExpressionSyntax.Type;
                                var typeInfo = semanticModel.GetTypeInfo(typeSyntax);
                                modelItem.Type = typeInfo.Type.ToString();
                            }
                        }
                    }
                }

                classList.Add(modelItem);
            }

            if (classList.Count == 0)
            {
                return;
            }

            var model = new AutoServiceModel()
            {
                ClassList = classList
            };

            context.AddSource($"AutoServiceExtension.Class.g.cs", RenderUtils.Render("AutoService", model));
        }
    }
}