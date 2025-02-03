using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.ViewModels;

public abstract partial class Screen : ViewModelBase
{
    [ObservableProperty]
    private string _title = string.Empty;

    public abstract void SetParent(MainWindowViewModel parent);
}

public class ViewModelBase : ObservableValidator
{
}