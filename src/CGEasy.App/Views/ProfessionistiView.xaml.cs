using System.Windows.Controls;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views;

public partial class ProfessionistiView : UserControl
{
    public ProfessionistiView(ProfessionistiViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}



