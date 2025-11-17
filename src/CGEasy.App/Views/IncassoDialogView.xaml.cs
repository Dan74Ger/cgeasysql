using System.Windows;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views
{
    public partial class IncassoDialogView : Window
    {
        public IncassoDialogView()
        {
            InitializeComponent();
            
            // Quando la finestra si chiude, imposta DialogResult in base al ViewModel
            Closing += (s, e) =>
            {
                if (DataContext is IncassoDialogViewModel vm)
                {
                    DialogResult = vm.DialogResult;
                }
            };
        }
    }
}

