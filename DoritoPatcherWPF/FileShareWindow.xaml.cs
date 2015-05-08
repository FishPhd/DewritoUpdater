using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
    /// Interaction logic for FileShareWindow.xaml
    /// </summary>
    public partial class FileShareWindow : Window
    {
        private GameFileShare.Model variant;

        public FileShareWindow(Uri url)
        {
            InitializeComponent();

            variant = GameFileShare.FetchVariant(url);

            VariantName.Text = variant.Name.ToUpper();
            VariantAuthor.Text = variant.Author;
            VariantDescription.Text = variant.Description;
            VariantTypeName.Text = variant.TypeName.ToUpper();
            VariantType.Text = variant.Type.ToUpper() + ": ";
            //VariantIcon.DataContext = "https://haloshare.net/Content/Images/" + variant.Type + "s/" + variant.TypeName + ".jpg"; //This doesn't work because wombarly doesn't consistently name shit
            VariantIcon.DataContext = "/Resources/" + variant.Type + "_" + variant.TypeName + ".png";

            GameFileShare.Download(url, variant, onProgress, onCompleted, onDuplicate);         
        }

        private bool onDuplicate()
        {
            string existsMessage =
                string.Format("The variant '{0}' by {1} already exist. Do you Want to overide it?", variant.Name, variant.Author);
            return MessageBox.Show(existsMessage, "Duplicate", MessageBoxButton.YesNo) != MessageBoxResult.Yes;

            //var MainWindow = new FileShareMsg("The variant '{0}' by {1} already exist. Do you Want to overide it?", variant.Name, variant.Author);

            //MainWindow.Show();
            //MainWindow.Focus();
        }

        private void onProgress(int progress)
        {
            FileShareProgress.Value = progress;
        }

        private void onCompleted(AsyncCompletedEventArgs args)
        {
            Complete.Visibility = Visibility.Visible;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
