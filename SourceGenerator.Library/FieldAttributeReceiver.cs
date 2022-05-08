using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator.Library
{
    public class FieldAttributeReceiver : ISyntaxReceiver
    {
        public readonly List<string> Names;

        public FieldAttributeReceiver(List<string> names)
        {
            this.Names = names;
        }

        private ConcurrentDictionary<ClassDeclarationSyntax, object> AttributeSyntaxDict { get; } =
            new ConcurrentDictionary<ClassDeclarationSyntax, object>();

        public ICollection<ClassDeclarationSyntax> AttributeSyntaxList => AttributeSyntaxDict.Keys;

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is AttributeSyntax cds && cds.Name is IdentifierNameSyntax identifierName &&
                Names.Contains(identifierName.Identifier.ValueText))
            {
                var syntax = cds.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                if (syntax == null) return;
                AttributeSyntaxDict[syntax] = null;
            }
        }
    }
}