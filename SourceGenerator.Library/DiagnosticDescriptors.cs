using Microsoft.CodeAnalysis;

namespace SourceGenerator.Library
{
    class DiagnosticDescriptors
    {
        internal static readonly DiagnosticDescriptor SGL001 = new DiagnosticDescriptor(
            id: "SGL001",
            title: "class must be partial",
            messageFormat: "Type '{0}' must be partial",
            category: "AutoPropertyGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}