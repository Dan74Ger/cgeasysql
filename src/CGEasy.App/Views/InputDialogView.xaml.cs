using System.Windows;

namespace CGEasy.App.Views
{
    public partial class InputDialogView : Window
    {
        public string Prompt
        {
            get => PromptTextBlock.Text;
            set => PromptTextBlock.Text = value;
        }

        public string Value
        {
            get => ValueTextBox.Text;
            set => ValueTextBox.Text = value;
        }

        public InputDialogView()
        {
            InitializeComponent();
            Loaded += (s, e) => ValueTextBox.SelectAll();
            Loaded += (s, e) => ValueTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

