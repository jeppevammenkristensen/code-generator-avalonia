using Client.Services.CodeToGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CodeAnalysis.Operations;
using Riok.Mapperly.Abstractions;

namespace Client.ViewModels.CodeToGrid;

[Mapper]
public static partial class CodeToGridColumnMapper
{
    //[ObjectFactory]
    
    [MapperIgnoreSource(nameof(CodeToGridProperty.PropertySymbol))]
    private static partial CodeToGridPropertyViewModel ExecuteMapToViewModel(CodeToGridProperty property);

    
    [UserMapping(Default = true)]
    public static CodeToGridPropertyViewModel MapToViewModel(this CodeToGridProperty property)
    {
        var dto = ExecuteMapToViewModel(property);
        dto.SetSource(property);
        // custom after map code...
        return dto;
    }
    
}


public partial class CodeToGridPropertyViewModel : ObservableObject
{
    private CodeToGridProperty _property;
    
    [ObservableProperty] public partial string Name { get; set; } 
    
    [ObservableProperty]
    public partial string Type { get; set; }
    
    [ObservableProperty]
    public partial CodeToGridColumnType ColumnType { get; set; }

    public string UnderlyingTypeAsString { get; private set; } = string.Empty;

    public bool IsChecked { get; set; } = true;
    
    public bool IsEnumerable { get; set; }
    public string HeaderName { get; set; }

    public void SetSource(CodeToGridProperty property)
    {
        _property = property;
    }

    public void ApplyChanges(CodeToGridProperty? property = null)
    {   
        property ??= _property;

        property.HeaderName = HeaderName;
        property.ColumnType = ColumnType;
        property.IsChecked = IsChecked;
        property.IsReadonly = IsReadonly;
    }

    [ObservableProperty] 
    public partial bool IsReadonly { get; set; }
}