using Microsoft.CodeAnalysis;

namespace SourceGenerator.Library.Models;

public record GeneratedModel<T>
{
    public ValueArray<Diagnostic> Diagnostics { get; set; } = [];

    public bool HasError => Diagnostics.Count > 0;

    public T Data { get; set; }
}