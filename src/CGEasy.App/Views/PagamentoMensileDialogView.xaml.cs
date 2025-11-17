using System.Windows;

namespace CGEasy.App.Views
{
    public partial class PagamentoMensileDialogView : Window
    {
        public PagamentoMensileDialogView()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ViewModels.PagamentoMensileDialogViewModel vm)
            {
                DialogResult = vm.DialogResult;
            }
        }
    }
}

