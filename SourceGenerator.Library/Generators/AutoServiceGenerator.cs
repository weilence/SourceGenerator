using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.Common;
using SourceGenerator.Library.Templates;
using SourceGenerator.Library.Utils;

namespace SourceGenerator.Library.Generators
{
    [Generator]
    public class AutoServiceGenerator : BaseGenerator
    {
        private readonly List<AutoServiceItem> classList = new List<AutoServiceItem>();

        public AutoServiceGenerator() : base(new[]
        {
            nameof(ServiceAttribute),
            ServiceAttribute.Name,
        })
        {
        }

        protected override void Execute(GeneratorExecutionContext context, AttributeSyntax attributeSyntax)
        {
            var classDeclarationSyntax = attributeSyntax.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclarationSyntax == null)
            {
                return;
            }

            var baseNamespaceDeclarationSyntax =
                classDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();
            var namespaceName = SyntaxUtils.GetName(baseNamespaceDeclarationSyntax);
            var className = SyntaxUtils.GetName(classDeclarationSyntax);

            var semanticModel = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

            var modelItem = new AutoServiceItem()
            {
                Class = namespaceName + "." + className, Types = new List<string>(),
            };

            var argumentListArguments = attributeSyntax.ArgumentList?.Arguments;
            if (argumentListArguments != null)
            {
                foreach (var argumentSyntax in argumentListArguments)
                {
                    var propertyName = SyntaxUtils.GetName(argumentSyntax.NameEquals);
                    switch (propertyName)
                    {
                        case nameof(ServiceAttribute.Type):
                        {
                            if (argumentSyntax.Expression is TypeOfExpressionSyntax typeOfExpressionSyntax)
                            {
                                var type = semanticModel.GetTypeInfo(typeOfExpressionSyntax.Type).Type;

                                if (type != null)
                                {
                                    modelItem.Types.Add(type.ToString());
                                }
                            }

                            break;
                        }
                        case nameof(ServiceAttribute.Lifetime):
                        {
                            if (argumentSyntax.Expression is MemberAccessExpressionSyntax
                                memberAccessExpressionSyntax)
                            {
                                modelItem.Lifetime = memberAccessExpressionSyntax.ToString();
                            }

                            break;
                        }
                    }
                }
            }

            if (modelItem.Types.Count == 0 && classDeclarationSyntax.BaseList != null)
            {
                foreach (var baseTypeSyntax in classDeclarationSyntax.BaseList.Types)
                {
                    var type = semanticModel.GetTypeInfo(baseTypeSyntax.Type).Type;
                    if (type != null)
                    {
                        modelItem.Types.Add(type.ToString());
                    }
                }
            }

            classList.Add(modelItem);
        }

        protected override void AfterExecute(GeneratorExecutionContext context)
        {
            context.AddSource("AutoServiceExtension.Class.g.cs", new AutoService(classList).TransformText());
        }
    }
}