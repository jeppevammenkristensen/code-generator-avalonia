using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Client.Services.CodeToGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Client.ViewModels.CodeToGrid;

public partial class CodeToGridViewModel : Screen
{
    [ObservableProperty]
    private bool _paneOpen;
    
    [ObservableProperty][Required] private string _source;

    [ObservableProperty][Required] private string _dataProperty;
    
    private MainWindowViewModel? _parent;
    private readonly CodeToGridService _service;
    
    [ObservableProperty]
    public partial string GeneratedCode { get; set; }
    
    [ObservableProperty]
    private CodeToGridContext? _codeToGridContext;

    [ObservableProperty] private ObservableCollection<CodeToGridPropertyViewModel> _properties = new();

    partial void OnCodeToGridContextChanged(CodeToGridContext? value)
    {
        if (value is { })
        {
            UpdateProperties(value);
        }
        else
        {
            Properties = new();
        }
    }

    private void UpdateProperties(CodeToGridContext value)
    {
        if (Properties.Count == 0)
        {
            Properties =
                new ObservableCollection<CodeToGridPropertyViewModel>(value.Properties.Select(x => x.MapToViewModel()));
        }
        else
        {
            var res = from newProperty in value.Properties
                join prop in Properties on newProperty.Name equals prop.Name into groupmatch
                from groupProp in groupmatch.DefaultIfEmpty()
                select new {newProperty, groupProp};

            foreach (var property in res)
            {
                property.groupProp?.ApplyChanges(property.newProperty);
            }
            
            Properties =  new ObservableCollection<CodeToGridPropertyViewModel>(value.Properties.Select(x => x.MapToViewModel()));
        }
    }


    public CodeToGridViewModel()
    {
        _service = new CodeToGridService();
        Title = "Code to grid";
        Source = @"public class Something { public int[] Items {get;set; } public string Name {get;set;} public List<string> Names {get;set; }}";
        SelectedTemplate = Templates[0];
    }

    [ObservableProperty]
    public partial string SelectedTemplate { get; set; }

    public override void SetParent(MainWindowViewModel parent)
    {
        _parent = parent;
    }

    public string[] Templates { get; } = CodeToGridService.Operations.Select(x => x.Name).ToArray();

    [RelayCommand]
    private void Format()
    {
        Source = SyntaxFactory.ParseCompilationUnit(Source).NormalizeWhitespace().ToFullString();
    }

    [RelayCommand]
    private void TogglePane()
    {
        PaneOpen = !PaneOpen;
    }
    
    [RelayCommand]
    private async Task Generate()
    {
        if (CodeToGridContext is { })
        {
            foreach (var code in Properties)
            {
                code.ApplyChanges();
            }

            GeneratedCode = (await _service.Process(CodeToGridContext, SelectedTemplate)) ?? string.Empty;
        }
    }
    
    [RelayCommand]
    private void Analyze()
    { 
        ClearErrors();
         ValidateProperty(Source, nameof(Source));
         ValidateProperty(DataProperty, nameof(DataProperty));

         if (HasErrors)
         {
             return;
         }
        
         CodeToGridContext = _service.Analyze(Source, DataProperty);
         
    }
}