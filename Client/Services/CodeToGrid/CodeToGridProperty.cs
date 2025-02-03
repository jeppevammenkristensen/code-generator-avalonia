using Microsoft.CodeAnalysis;

namespace Client.Services.CodeToGrid;

public class CodeToGridProperty
{
    public string Name { get;  }
    public string Type { get; }
    public CodeToGridColumnType ColumnType { get; set; }
    
    private ITypeSymbol UnderlyingType { get; set; }
    
    public string UnderlyingTypeAsString => UnderlyingType?.ToString();
    public bool IsEnumerable { get; private set; }
    
    public bool IsChecked { get; set; }
    
    public bool IsBoolean() => PropertySymbol.Type.SpecialType == SpecialType.System_Boolean;

    public CodeToGridProperty(IPropertySymbol propertySymbol, CodeToGridColumnType columnType)
    {
        PropertySymbol = propertySymbol;
        HeaderName = Name = propertySymbol.Name;
        Type = propertySymbol.Type.ToString();
        ColumnType = columnType;
        (IsEnumerable, UnderlyingType) = propertySymbol.Type.IsEnumerableOfTypeButNotString();
        IsChecked = true;


    }

    public IPropertySymbol PropertySymbol { get;  }
    public string? HeaderName { get; set; }
    public bool IsReadonly { get; set; }
}