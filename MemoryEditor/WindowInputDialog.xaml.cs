using System.Windows;

namespace MemoryEditor
{
    public partial class WindowInputDialog : Window
    {
        // This will be accessed when the dialog is closed
        public string InputValue { get { return TextBoxInput.Text; } }

        public WindowInputDialog(string initialValue = "")
        {
            InitializeComponent();

            // Set the input text box to the specified initial value
            TextBoxInput.Text = initialValue;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}