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
using System.Windows.Threading;

namespace DoritoPatcherWPF
{
    /// <summary>
    /// Interaction logic for FileShareMsg.xaml
    /// </summary>
    public partial class FileShareMsg : Window
    {
        public bool confirm = false;

        public FileShareMsg(string name, string author, string type)
        {
            InitializeComponent();
            if (type == "Forge")
            {
                fileType.Text = "map";
            }
            else
            {
                fileType.Text = type.ToLower();
            }
            fileName.Text = name.ToUpper();
            fileAuthor.Text = author;
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            confirm = true;
            Close();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            confirm = false;
            Close();
        }

    }
}
