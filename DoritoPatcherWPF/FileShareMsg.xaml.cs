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
using System.Windows.Shapes;

namespace DoritoPatcherWPF
{
    /// <summary>
    /// Interaction logic for FileShareMsg.xaml
    /// </summary>
    public partial class FileShareMsg : Window
    {

        public FileShareMsg(string text)
        {
            InitializeComponent();
            FileShareText.Text = text;
        }

    }
}
