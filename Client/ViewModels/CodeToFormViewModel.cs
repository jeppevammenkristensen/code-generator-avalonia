using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Client.Services.CodeToForms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CodeAnalysis.Operations;

namespace Client.ViewModels;

public partial class CodeToFormViewModel : Screen
{
    private MainWindowViewModel? _parent;

    [NotifyCanExecuteChangedFor(nameof(GeneratePropertiesCommand))]
    [ObservableProperty] private string _source = """
public class Someclass
{
    public string Name { get; set; }
    public string SurName {get;set;}
    public int Age {get;set;}
}
""";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Properties))]
    private CodeToFormsContext? _context;

    public CodeToFormViewModel()
    {
        _service = new CodeToFormsService();
        Title = "Code to form";
    }

    partial void OnContextChanged(CodeToFormsContext? value) 
    {
        Properties = new ObservableCollection<CodeToFormProperty>(value?.Properties ?? []);
    }

    public ComponentType[] ComponentTypes { get; } = Enum.GetValues<ComponentType>();

    private bool CanGenerateProperties => !string.IsNullOrWhiteSpace(Source);
    

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BuildCommand))]
    private ObservableCollection<CodeToFormProperty> _properties = new ();

    private readonly CodeToFormsService _service;

    [ObservableProperty]
    private string? _xaml;

    [RelayCommand(CanExecute = nameof(CanGenerateProperties))]
    private void GenerateProperties()
    {
       
        Context = _service.Process(Source, Context);
    }

    private bool CanBuild()
    {
        return Context is {IsValid: true, Properties: {Length: > 0}};
    }
    
    [RelayCommand(CanExecute = nameof(CanBuild))]
    private void Build()
    {
        Xaml = _service.Build(Context);
    }

    public void UpdateStatus(string statusText)
    {
        _parent.Status = statusText;
    }

    public override void SetParent(MainWindowViewModel parent)
    {
        _parent = parent;
    }
}