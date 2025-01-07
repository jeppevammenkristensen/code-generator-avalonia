using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.ViewModels;

public partial class Screen : ViewModelBase
{
    [ObservableProperty]
    private string _title = string.Empty;
}

public class ViewModelBase : ObservableObject
{
}