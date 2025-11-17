using System.Windows.Controls;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views;

public partial class UtentiView : UserControl
{
    public UtentiView(UtentiViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}



