using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _status;
    
    [ObservableProperty]
    private ObservableCollection<Screen> _screens = new();
    
    public string Greeting { get; } = "Welcome to Avalonia!";

    [RelayCommand]
    private void CloseTab(Screen screen)
    {
        var index = Screens.IndexOf(screen);
        
        if (index > 0)
        {
            Screen = Screens[index - 1];
        }
        else if (index == 0 && Screens.Count > 1)
        {
            Screen = Screens[1];
        }
        
        Screens.Remove(screen);
    }

    [ObservableProperty]
    private Screen _screen;
    
    public MainWindowViewModel()
    {
        Screens = [new CodeToFormViewModel(this)];
        Screen = Screens[0];
    }

    public void New()
    {
        var newScreen = new CodeToFormViewModel(this);
        Screens.Add(newScreen);
        Screen = newScreen;
    }
}