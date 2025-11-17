using System.Windows.Controls;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views;

public partial class TipoPraticaView : UserControl
{
    public TipoPraticaView(TipoPraticaViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}



