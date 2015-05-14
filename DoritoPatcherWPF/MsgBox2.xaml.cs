using System.Windows;

namespace DoritoPatcherWPF
{
    /// <summary>
    ///     Interaction logic for MsgBox2.xaml
    /// </summary>
    public partial class MsgBox2 : Window
    {
        public MsgBox2(string text)
        {
            InitializeComponent();
            Msg.Text = text;
        }

        //Ok button (Close)
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //Titlebar control
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}