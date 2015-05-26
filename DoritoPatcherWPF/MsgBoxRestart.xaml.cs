using System.Diagnostics;
using System.Windows;

namespace DoritoPatcherWPF
{
    /// <summary>
    ///     Interaction logic for MsgBox.xaml
    /// </summary>
    public partial class MsgBox : Window
    {
        public MsgBox(string text)
        {
            InitializeComponent();
            Msg.Text = text;
        }

        //Restart button
        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        //Titlebar control
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}