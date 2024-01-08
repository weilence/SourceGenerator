﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.Common;
using SourceGenerator.Library.Receivers;
using SourceGenerator.Library.Templates;
using SourceGenerator.Library.Utils;

namespace SourceGenerator.Library.Generators
{
    [Generator]
    public class AutoServiceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new ClassSyntaxReceiver(new List<string>
                {
                    nameof(ServiceAttribute), ServiceAttribute.Name
                }));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (ClassSyntaxReceiver)context.SyntaxReceiver;
            var syntaxList = receiver.AttributeSyntaxList;

            if (syntaxList.Count == 0)
            {
                return;
            }

            var classList = new List<AutoServiceItem>();
            foreach (var classDeclarationSyntax in syntaxList)
            {
                var attributeSyntax =
                    SyntaxUtils.GetAttribute(classDeclarationSyntax, name => receiver.AttributeNames.Contains(name));
                if (attributeSyntax == null)
                {
                    continue;
                }

                var baseNamespaceDeclarationSyntax =
                    classDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();
                var namespaceName = SyntaxUtils.GetName(baseNamespaceDeclarationSyntax);
                var className = SyntaxUtils.GetName(classDeclarationSyntax);

                var semanticModel = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

                var modelItem = new AutoServiceItem()
                {
                    Class = namespaceName + "." + className, Types = new List<string>()
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

            if (classList.Count == 0)
            {
                return;
            }

            context.AddSource($"AutoServiceExtension.Class.g.cs", new AutoService(classList).TransformText());
        }
    }
}