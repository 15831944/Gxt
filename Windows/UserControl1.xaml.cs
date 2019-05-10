
using System.Windows;
using System.Windows.Controls;


namespace Gxt.Windows
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ModalDialog : Window
    {
        public ModalDialog()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

    }
}
