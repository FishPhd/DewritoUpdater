using System.Windows;

namespace DewritoUpdater
{
    /// <summary>
    ///     Interaction logic for MsgBoxOk.xaml
    /// </summary>
    public partial class MsgBoxOk : Window
    {
        public MsgBoxOk(string text)
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