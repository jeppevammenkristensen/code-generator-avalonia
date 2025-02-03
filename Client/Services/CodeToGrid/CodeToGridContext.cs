using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Client.Services.CodeToGrid;

public class CodeToGridContext(INamedTypeSymbol code, string sourceProperty, List<CodeToGridProperty> properties)
{
    public INamedTypeSymbol Code { get; set; } = code;
    public string SourceProperty { get; } = sourceProperty;
    public List<CodeToGridProperty> Properties { get; set; } = properties;
}