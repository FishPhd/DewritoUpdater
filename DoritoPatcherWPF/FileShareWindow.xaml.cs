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
            if (variant.Type == "Forge")
            {
                VariantType.Text = "MAP: ";
            }
            else
            {
                VariantType.Text = variant.Type.ToUpper() + ": ";
            }
            VariantTypeName.Text = variant.TypeName.ToUpper();
            VariantIcon.DataContext = "https://" + url.Host + variant.Icon;

            if(!GameFileShare.Download(url, variant, onProgress, onCompleted, onDuplicate))
            { 
                Close();
            }
        }

        private bool onDuplicate()
        {
            bool confirm = false;
            var duplicateWindow = new FileShareMsg(variant.Name, variant.Author, variant.Type);

            if (duplicateWindow.ShowDialog() == false)
            {
                confirm = duplicateWindow.confirm;
            }

            return confirm;
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
