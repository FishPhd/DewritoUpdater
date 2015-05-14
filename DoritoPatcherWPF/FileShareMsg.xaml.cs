using System.Windows;

namespace DoritoPatcherWPF
{
    /// <summary>
    ///     Interaction logic for FileShareMsg.xaml
    /// </summary>
    public partial class FileShareMsg : Window
    {
        public bool confirm;

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