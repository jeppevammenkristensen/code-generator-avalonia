using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Client.ViewModels;

namespace Client.Views;

public partial class CodeToForm : UserControl
{
    public CodeToForm()
    {
        InitializeComponent();
    }

    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CodeToFormViewModel viewModel)
        {
            TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(viewModel.Xaml);
            viewModel.UpdateStatus("Xaml copied to clipboard");
        }
    }
}