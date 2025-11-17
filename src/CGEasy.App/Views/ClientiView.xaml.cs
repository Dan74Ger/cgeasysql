using System.Windows.Controls;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views;

/// <summary>
/// Interaction logic for ClientiView.xaml
/// </summary>
public partial class ClientiView : UserControl
{
    public ClientiView(ClientiViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}



