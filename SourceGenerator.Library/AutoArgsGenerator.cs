using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
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

                var model = new ClassModel()
                {
                    Namespace = SyntaxUtils.GetName(namespaceDeclarationSyntax),
                    Class = SyntaxUtils.GetName(classDeclarationSyntax),
                    Fields = new List<Field>()
                };
                foreach (var fieldDeclaration in fieldDeclarationList)
                {
                    var type = fieldDeclaration.Declaration.Type.ToString();

                    foreach (var declarationVariable in fieldDeclaration.Declaration.Variables)
                    {
                        var name = SyntaxUtils.GetName(declarationVariable);
                        model.Fields.Add(new Field()
                        {
                            Name = name,
                            Type = type
                        });
                    }
                }

                var autoProperty = new AutoArgs(model);

                context.AddSource($"{model.Class}.g.cs", autoProperty.TransformText());
            }
        }
    }
}