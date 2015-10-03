using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dewritwo
{
    /// <summary>
    ///     Interaction logic for MsgBox.xaml
    /// </summary>
    public partial class MsgBoxRestart
    {
        public MsgBoxRestart(string text)
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
