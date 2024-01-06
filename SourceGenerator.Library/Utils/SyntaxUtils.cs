﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator.Library.Utils
{
    public class SyntaxUtils
    {
        public static bool HasModifier(MemberDeclarationSyntax syntax, params SyntaxKind[] modifiers)
        {
            return syntax.Modifiers.Any(m => modifiers.Contains(m.Kind()));
        }


        public static bool HasModifiers(MemberDeclarationSyntax syntax, params SyntaxKind[] modifiers)
        {
            var syntaxKinds = syntax.Modifiers.Select(n => n.Kind()).ToList();
            return modifiers.All(m => syntaxKinds.Contains(m));
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
                case NameEqualsSyntax nameEqualsSyntax:
                    return nameEqualsSyntax.Name.Identifier.Text;
                case ParameterSyntax parameterSyntax:
                    return parameterSyntax.Identifier.Text;
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool HasAttribute(MemberDeclarationSyntax classDeclaration, Func<string, bool> func)
        {
            return GetAttribute(classDeclaration, func) != null;
        }

        public static AttributeSyntax GetAttribute(MemberDeclarationSyntax classDeclaration, Func<string, bool> func)
        {
            return classDeclaration.AttributeLists.SelectMany(m => m.Attributes)
                .FirstOrDefault(m => func(m.Name.ToString()));
        }

        public static List<string> GetUsings(ClassDeclarationSyntax classDeclarationSyntax)
        {
            var compilationUnitSyntax = classDeclarationSyntax.SyntaxTree.GetRoot() as CompilationUnitSyntax;
            var usings = compilationUnitSyntax.Usings.Select(m => m.ToString()).ToList();
            return usings;
        }

        public static string GetNamespaceName(ClassDeclarationSyntax classDeclarationSyntax)
        {
            var namespaceDeclarationSyntax =
                classDeclarationSyntax.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();

            return GetName(namespaceDeclarationSyntax);
        }
    }
}