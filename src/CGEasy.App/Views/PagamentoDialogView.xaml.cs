using System.Windows;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views
{
    public partial class PagamentoDialogView : Window
    {
        public PagamentoDialogView()
        {
            InitializeComponent();
            
            // Quando la finestra si chiude, imposta DialogResult in base al ViewModel
            Closing += (s, e) =>
            {
                if (DataContext is PagamentoDialogViewModel vm)
                {
                    DialogResult = vm.DialogResult;
                }
            };
        }
    }
}

