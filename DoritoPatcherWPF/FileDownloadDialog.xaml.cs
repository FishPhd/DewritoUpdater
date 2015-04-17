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

namespace DoritoPatcherWPF
{
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Windows.Threading;

    using DoritoPatcher;

    /// <summary>
    /// Interaction logic for FileDownloadDialog.xaml
    /// </summary>
    public partial class FileDownloadDialog : Window
    {
        WebClient wc = new WebClient();
        public Exception Error;
        public MainWindow Window;

        public FileDownloadDialog(MainWindow window, string url, string destPath)
        {
            InitializeComponent();
            this.Window = window;

            lblStatus.Content = "Downloading " + System.IO.Path.GetFileName(destPath);

            // sketchy code to replace current exe
            if (File.Exists(destPath) && destPath.ToLower() == Process.GetCurrentProcess().MainModule.FileName.ToLower())
            {
                if (File.Exists(destPath + ".old"))
                    File.Delete(destPath + ".old");

                File.Move(destPath, destPath + ".old");
            }

            wc.DownloadProgressChanged += (s, e) =>
            {
                DownloadProgress.Value = e.ProgressPercentage;
            };
            wc.DownloadFileCompleted += (s, e) =>
            {
                DownloadProgress.Value = 100;
                
                if (e.Error != null)
                {
                    this.DialogResult = false;
                    Error = e.Error;
                }
                else
                {
                    this.DialogResult = true;
                }
                var patchFileExtension = ".bspatch";
                if (destPath.EndsWith(patchFileExtension))
                {
                    string destFilePath = destPath.Substring(0, destPath.Length - patchFileExtension.Length);
                    string destFileName = destFilePath.Replace(Window.BasePath, "");
                    if (destFileName.StartsWith("\\") || destFileName.StartsWith("/"))
                        destFileName = destFileName.Substring(1);

                    // patch file
                    string backupFolder = Path.Combine(Window.BasePath, "_dewbackup");
                    string backupFile = Path.Combine(backupFolder, destFileName);
                    if (File.Exists(backupFile))
                    {
                        // backup exists, copy backup over original file and patch orig
                        if (File.Exists(destFilePath))
                            File.Delete(destFilePath);

                        //File.Copy(backupFile, destFilePath);
                    }
                    else
                    {
                        // if backup don't exist we're assuming the main form made sure current one is fine
                        // b-b-b-b-back it up
                        backupFolder = Path.GetDirectoryName(backupFile);
                        if (!Directory.Exists(backupFolder))
                            Directory.CreateDirectory(backupFolder);

                        File.Copy(destFilePath, backupFile);
                        File.Delete(destFilePath);
                    }

                    lblStatus.Content = "Applying patch file...";
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                          new ThreadStart(delegate { }));

                    using (FileStream input = new FileStream(backupFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (FileStream output = new FileStream(destFilePath, FileMode.Create))
                        BinaryPatchUtility.Apply(input, () => new FileStream(destPath, FileMode.Open, FileAccess.Read, FileShare.Read), output);

                    File.Delete(destPath);
                }

                this.Close();
            };
            wc.DownloadFileAsync(new Uri(url), destPath);
        }
    }
}
