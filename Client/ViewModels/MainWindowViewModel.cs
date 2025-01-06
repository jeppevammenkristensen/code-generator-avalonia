using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";

    [ObservableProperty]
    private CodeToFormViewModel _screen;
    
    public MainWindowViewModel()
    {
        Screen = new CodeToFormViewModel();
    }
}