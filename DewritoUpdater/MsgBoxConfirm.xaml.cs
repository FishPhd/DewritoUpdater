using System.Windows;

namespace DewritoUpdater
{
    /// <summary>
    ///     Interaction logic for MsgBoxConfirm.xaml
    /// </summary>
    public partial class MsgBoxConfirm : Window
    {
        public bool confirm;

        public MsgBoxConfirm(string text)
        {
            InitializeComponent();
            Msg.Text = text;
        }

        //Ok button
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            confirm = true;
            Close();
        }

        //Cancel Button
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            confirm = false;
            Close();
        }

        //Titlebar control
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            confirm = false;
            Close();
        }
    }
}