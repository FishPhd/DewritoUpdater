using System;
using System.ComponentModel;
using System.Windows;

namespace DoritoPatcherWPF
{
    /// <summary>
    ///     Interaction logic for FileShareWindow.xaml
    /// </summary>
    public partial class FileShareWindow : Window
    {
        private readonly GameFileShare.Model variant;

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

            if (!GameFileShare.Download(url, variant, onProgress, onCompleted, onDuplicate))
            {
                Close();
            }
        }

        private bool onDuplicate()
        {
            var confirm = false;
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