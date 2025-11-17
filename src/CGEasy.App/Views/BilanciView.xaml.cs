using System.Windows.Controls;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views;

public partial class BilanciView : UserControl
{
    public BilanciView()
    {
        InitializeComponent();
        DataContext = new BilanciViewModel();
    }
}

