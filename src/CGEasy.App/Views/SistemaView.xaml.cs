using System.Windows.Controls;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views;

public partial class SistemaView : UserControl
{
    public SistemaView(SistemaViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}




























