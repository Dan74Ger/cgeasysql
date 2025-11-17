using CGEasy.App.ViewModels;
using System.Windows.Controls;

namespace CGEasy.App.Views
{
    public partial class RiepilogoBancheView : UserControl
    {
        public RiepilogoBancheView()
        {
            InitializeComponent();
            DataContext = new RiepilogoBancheViewModel();
        }
    }
}
