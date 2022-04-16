using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using SourceGenerator.Common;
using SourceGenerator.Library.Template;

namespace SourceGenerator.Library
{
    [Generator]
    public class AutoPropertyGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new FieldAttributeReceiver(new List<string> { nameof(PropertyAttribute), PropertyAttribute.Name }));
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

                var compilationUnitSyntax = classDeclarationSyntax.SyntaxTree.GetRoot() as CompilationUnitSyntax;
                var usings = compilationUnitSyntax.Usings.Select(m => m.ToString()).ToList();
                var model = new ClassModel()
                {
                    Usings = usings,
                    Namespace = SyntaxUtils.GetName(namespaceDeclarationSyntax),
                    Class = SyntaxUtils.GetName(classDeclarationSyntax),
                    Fields = new List<Field>()
                };
                foreach (var fieldDeclaration in fieldDeclarationList)
                {
                    if (!SyntaxUtils.HasAttribute(fieldDeclaration, name => receiver.Names.Contains(name)))
                    {
                        continue;
                    }

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

                var autoProperty = new AutoProperty(model);

                context.AddSource($"{model.Class}.g.cs", autoProperty.TransformText());
            }
        }
    }
}