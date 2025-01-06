using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.ViewModels;

public partial class CodeToFormViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Code to Form";
}