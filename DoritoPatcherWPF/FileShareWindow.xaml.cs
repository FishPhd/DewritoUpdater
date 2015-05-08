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

        
            VariantName.Content = variant.Name;


            GameFileShare.Download(url, variant, onProgress, onCompleted, onDuplicate);         
        }

        private bool onDuplicate()
        {
            string existsMessage =
                string.Format("The variant '{0}' by {1} already exist. Do you Want to overide it?", variant.Name, variant.Author);
            return MessageBox.Show(existsMessage, "Duplicate", MessageBoxButton.YesNo) != MessageBoxResult.Yes;
        }

        private void onProgress(int progress)
        {

        }

        private void onCompleted(AsyncCompletedEventArgs args)
        {
            MessageBox.Show(variant.Name + " is downloaded!");
        }
    }
}
