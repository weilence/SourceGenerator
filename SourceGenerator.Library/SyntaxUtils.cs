using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator.Library
{
    public class SyntaxUtils
    {
        public static bool HasModifier(MemberDeclarationSyntax syntax, SyntaxKind modifier)
        {
            return syntax.Modifiers.Any(m => m.IsKind(modifier));
        }

        public static string GetName(SyntaxNode syntaxNode)
        {
            switch (syntaxNode)
            {
                case BaseTypeDeclarationSyntax baseTypeDeclarationSyntax:
                    return baseTypeDeclarationSyntax.Identifier.Text;
                case BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax:
                    return baseNamespaceDeclarationSyntax.Name.ToString();
                case VariableDeclaratorSyntax variableDeclaratorSyntax:
                    return variableDeclaratorSyntax.Identifier.Text;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}