using System.Windows;

namespace CGEasy.App.Views
{
    public partial class DettaglioAssociazioniDialogView : Window
    {
        public DettaglioAssociazioniDialogView()
        {
            InitializeComponent();
        }

        private void ChiudiButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

