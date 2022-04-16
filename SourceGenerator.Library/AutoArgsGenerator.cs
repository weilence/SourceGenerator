using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.Common;
using SourceGenerator.Library.Template;

namespace SourceGenerator.Library
{
    [Generator]
    public class AutoArgsGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new FieldAttributeReceiver(new List<string> { nameof(ArgsAttribute), ArgsAttribute.Name }));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (FieldAttributeReceiver)context.SyntaxReceiver;
            var syntaxList = receiver.AttributeSyntaxList;

            if (syntaxList.Count == 0)
            {
                return;
            }

            foreach (var classDeclarationSyntax in syntaxList)
            {
                var namespaceDeclarationSyntax =
                    classDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();

                var fieldDeclarationList = classDeclarationSyntax.Members.OfType<FieldDeclarationSyntax>().ToList();
                if (fieldDeclarationList.Count == 0)
                {
                    continue;
                }

                var classHasAttribute =
                    SyntaxUtils.HasAttribute(classDeclarationSyntax, name => receiver.Names.Contains(name));

                var fields = new List<Field>();
                foreach (var fieldDeclaration in fieldDeclarationList)
                {
                    if (SyntaxUtils.HasModifier(fieldDeclaration, SyntaxKind.StaticKeyword, SyntaxKind.ConstKeyword))
                    {
                        continue;
                    }

                    if (!SyntaxUtils.HasModifier(fieldDeclaration, SyntaxKind.PrivateKeyword))
                    {
                        continue;
                    }

                    var fieldHasAttribute =
                        SyntaxUtils.HasAttribute(fieldDeclaration, name => receiver.Names.Contains(name));
                    var fieldIgnoreAttribute = SyntaxUtils.HasAttribute(fieldDeclaration,
                        name => new[] { nameof(ArgsIgnoreAttribute), ArgsIgnoreAttribute.Name }.Contains(name));

                    if (!(fieldHasAttribute || classHasAttribute && !fieldIgnoreAttribute))
                    {
                        continue;
                    }

                    var type = fieldDeclaration.Declaration.Type.ToString();

                    foreach (var declarationVariable in fieldDeclaration.Declaration.Variables)
                    {
                        var name = SyntaxUtils.GetName(declarationVariable);
                        fields.Add(new Field()
                        {
                            Name = name,
                            Type = type
                        });
                    }
                }

                if (fields.Count == 0)
                {
                    continue;
                }

                var usings = SyntaxUtils.GetUsings(classDeclarationSyntax);
                var model = new ClassModel()
                {
                    Usings = usings,
                    Namespace = SyntaxUtils.GetName(namespaceDeclarationSyntax),
                    Class = SyntaxUtils.GetName(classDeclarationSyntax),
                    Fields = fields
                };
                var autoProperty = new AutoArgs(model);

                context.AddSource($"{model.Class}.g.cs", autoProperty.TransformText());
            }
        }
    }
}