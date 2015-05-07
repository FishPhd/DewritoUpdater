using System;
using System.Windows;

namespace DoritoPatcherWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Check if the application launched with any arguments. 
            // If the application was launched with arguments check if it's the proper scheme
            // If the proper scheme is detected launch a FileShare download window.
            if (e.Args.Length > 0) 
            {
                Uri uri = GameFileShare.ParseUri(e.Args[0]);
                if (uri != null)
                {
                    FileShareWindow shareWindow = new FileShareWindow(uri);
                    shareWindow.Show();
                    return;
                }
            }

            // Register the fileshare protocol after checking arguments.
            // No need to alter Registry if we launched through the protocol.
            GameFileShare.RegisterProtocol();

            MainWindow mainWindow = new MainWindow();
            MainWindow.Show();
        }
    }
}
