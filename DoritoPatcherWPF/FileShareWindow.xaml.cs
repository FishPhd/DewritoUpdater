using System;
using System.Collections.Generic;
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
        WebClient wc = new WebClient();

        public FileShareWindow(Uri url)
        {
            InitializeComponent();

            var variant = GameFileShare.FetchVariant(url);

        
            VariantName.Content = variant.Name;

            wc.DownloadProgressChanged += (s, e) =>
            {
                //DownloadProgress.Value = e.ProgressPercentage;
            };
            wc.DownloadFileCompleted += (s, e) =>
            {
                //DownloadProgress.Value = 100;

                if (e.Error != null)
                {
                    MessageBox.Show(e.Error.Message, "FileShare Downloader", MessageBoxButton.OK);
                    this.Close();
                }

                MessageBox.Show("Downloaded.");
                this.Close();
            };

            string path = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            switch (variant.Type)
            {
                case "Forge":
                    path = System.IO.Path.Combine(path, "mods/forge/");
                    break;
                case "GameType":
                    path = System.IO.Path.Combine(path, "mods/variants/");
                    break;
            }
            System.IO.Directory.CreateDirectory(path);


            string filePath = 
                System.IO.Path.Combine(path, string.Format("{0} ({1}).bin", variant.Name, variant.Author));


            // Ask the user if they want to overwrite the variant if it already exists.
            if (System.IO.File.Exists(filePath))
            {
                string existsMessage = 
                    string.Format("The variant '{0}' by {1} already exist. Do you Want to overide it?", variant.Name, variant.Author);
                if (MessageBox.Show(existsMessage, "Duplicate", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    this.Close();
                }
            }
            wc.DownloadFileAsync(new Uri("https://" + url.Host + variant.Download), filePath);
        }
    }
}
