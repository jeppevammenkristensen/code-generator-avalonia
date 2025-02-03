using System.Collections.ObjectModel;
using Client.Services;
using Client.ViewModels.CodeToGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceLocator _locator;

    [ObservableProperty] private string? _status;

    [ObservableProperty] private ObservableCollection<Screen> _screens = new();

    public void NewCodeToGrid()
    {
        var newScreen = Init<CodeToGridViewModel>();
        Screens.Add(newScreen);
        Screen = newScreen;
    }

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

    [ObservableProperty] private Screen _screen;

    public MainWindowViewModel(IServiceLocator locator)
    {
        _locator = locator;
        Screens = [Init<CodeToFormViewModel>()];
        Screen = Screens[0];
    }

    public void NewCodeToForm()
    {
        var newScreen = Init<CodeToFormViewModel>();
        Screens.Add(newScreen);
        Screen = newScreen;
    }

    private T Init<T>() where T : Screen
    {
        var result = _locator.GetRequiredService<T>();
        result.SetParent(this);
        return result;
    }
}