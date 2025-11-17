using System.Windows;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views
{
    public partial class AnticipoDialogView : Window
    {
        public AnticipoDialogView()
        {
            InitializeComponent();
            
            // Quando la finestra si chiude, imposta DialogResult in base al ViewModel
            Closing += (s, e) =>
            {
                if (DataContext is AnticipoDialogViewModel vm)
                {
                    DialogResult = vm.DialogResult;
                }
            };
        }
    }
}

