using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DoritoPatcherWPF
{
    /// <summary>
    /// Interaction logic for MsgBox.xaml
    /// </summary>
    public partial class MsgBox : Window
    {
        public MsgBox(string text)
        {
            InitializeComponent();
            Msg.Content = text;
        }

        //Ok button (Shutdown)
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
