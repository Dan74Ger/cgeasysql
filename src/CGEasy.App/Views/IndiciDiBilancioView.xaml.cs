using System.Windows.Controls;

namespace CGEasy.App.Views;

public partial class IndiciDiBilancioView : UserControl
{
    public IndiciDiBilancioView()
    {
        InitializeComponent();
        DataContext = new ViewModels.IndiciDiBilancioViewModel();
    }
}




