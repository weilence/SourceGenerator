using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator.Common;
using SourceGenerator.Library.Templates;
using SourceGenerator.Library.Utils;

namespace SourceGenerator.Library.Generators
{
    [Generator]
    public class AutoArgsGenerator : BaseGenerator
    {
        public AutoArgsGenerator(IEnumerable<string> attributeNames) : base(attributeNames)
        {
        }

        public AutoArgsGenerator() : this(new[]
        {
            nameof(ArgsAttribute),
            ArgsAttribute.Name,
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

            if (!ReportUtils.CheckPartial(context, classDeclarationSyntax))
            {
                return;
            }

            var hasLogger = HasLogger(classDeclarationSyntax);
            var fieldDeclarationList = classDeclarationSyntax.Members.OfType<FieldDeclarationSyntax>().ToList();
            if (!hasLogger && fieldDeclarationList.Count == 0)
            {
                return;
            }

            var hasOptions = false;

            var fields = new List<Field>();
            foreach (var fieldDeclaration in fieldDeclarationList)
            {
                if (SyntaxUtils.HasModifier(fieldDeclaration, SyntaxKind.StaticKeyword, SyntaxKind.ConstKeyword))
                {
                    continue;
                }

                if (!SyntaxUtils.HasModifiers(fieldDeclaration, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword))
                {
                    continue;
                }

                if (SyntaxUtils.HasAttribute(fieldDeclaration,
                        name => new[]
                        {
                            IgnoreAttribute.Name,
                            nameof(IgnoreAttribute),
                        }.Contains(name)))
                {
                    continue;
                }

                var type = fieldDeclaration.Declaration.Type.ToString();
                var isValue = SyntaxUtils.HasAttribute(fieldDeclaration,
                    name => name == ValueAttribute.Name || name == nameof(ValueAttribute));
                if (isValue)
                {
                    hasOptions = true;
                }

                foreach (var declarationVariable in fieldDeclaration.Declaration.Variables)
                {
                    if (declarationVariable.Initializer != null)
                    {
                        continue;
                    }

                    var name = SyntaxUtils.GetName(declarationVariable);
                    fields.Add(new Field()
                    {
                        Name = name,
                        Type = type,
                        IsOptions = isValue,
                    });
                }
            }

            var constructorDeclarationSyntax =
                classDeclarationSyntax.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault(m =>
                    SyntaxUtils.HasModifier(
                        m, SyntaxKind.PrivateKeyword));

            if (constructorDeclarationSyntax != null)
            {
                foreach (var parameterSyntax in constructorDeclarationSyntax.ParameterList.Parameters)
                {
                    var name = SyntaxUtils.GetName(parameterSyntax);
                    var type = parameterSyntax.Type.ToString();

                    var field = fields.FirstOrDefault(m => m.Type == type);
                    if (field != null)
                    {
                        field.InBase = true;
                    }
                    else
                    {
                        fields.Add(new Field()
                        {
                            Name = name,
                            Type = type,
                            Ignore = true,
                            InBase = true,
                        });
                    }
                }
            }

            if (!hasLogger && fields.Count == 0)
            {
                return;
            }

            var usings = SyntaxUtils.GetUsings(classDeclarationSyntax);
            if (hasOptions && !usings.Contains("using Microsoft.Extensions.Options;"))
            {
                usings.Add("using Microsoft.Extensions.Options;");
            }

            if (hasLogger)
            {
                usings.Add("using Microsoft.Extensions.Logging;");
            }

            var model = new AutoArgsModel()
            {
                Usings = usings,
                Namespace = SyntaxUtils.GetNamespaceName(classDeclarationSyntax),
                Class = SyntaxUtils.GetName(classDeclarationSyntax),
                Fields = fields,
                HasBase = constructorDeclarationSyntax != null,
                HasLogger = hasLogger,
            };

            context.AddSource($"{model.Namespace}.{model.Class}.g.cs", new AutoArgs(model).TransformText());
        }

        private bool HasLogger(ClassDeclarationSyntax classDeclarationSyntax)
        {
            return SyntaxUtils.HasAttribute(classDeclarationSyntax,
                name => new[]
                {
                    LoggerAttribute.Name,
                    nameof(LoggerAttribute),
                }.Contains(name));
        }
    }
}