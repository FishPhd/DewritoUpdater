using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using DoritoPatcher;

namespace Dewritwo
{
    /// <summary>
    ///     Interaction logic for FileDownloadDialog.xaml
    /// </summary>
    public partial class FileDownloadDialog
    {
        private readonly WebClient wc = new WebClient();
        public Exception Error;
        public MainWindow Window;

        public FileDownloadDialog(MainWindow window, string url, string destPath)
        {
            InitializeComponent();
            Window = window;

            lblStatus.Content = "Downloading " + Path.GetFileName(destPath);

            // sketchy code to replace current exe
            if (File.Exists(destPath) && destPath.ToLower() == Process.GetCurrentProcess().MainModule.FileName.ToLower())
            {
                if (File.Exists(destPath + ".old"))
                    File.Delete(destPath + ".old");

                File.Move(destPath, destPath + ".old");
            }

            wc.DownloadProgressChanged += (s, e) => { DownloadProgress.Value = e.ProgressPercentage; };
            wc.DownloadFileCompleted += (s, e) =>
            {
                DownloadProgress.Value = 100;

                if (e.Error != null)
                {
                    DialogResult = false;
                    Error = e.Error;
                }
                else
                {
                    DialogResult = true;
                }
                var patchFileExtension = ".bspatch";
                if (destPath.EndsWith(patchFileExtension))
                {
                    var destFilePath = destPath.Substring(0, destPath.Length - patchFileExtension.Length);
                    var destFileName = destFilePath.Replace(Window.BasePath, "");
                    if (destFileName.StartsWith("\\") || destFileName.StartsWith("/"))
                        destFileName = destFileName.Substring(1);

                    // patch file
                    var backupFolder = Path.Combine(Window.BasePath, "_dewbackup");
                    var backupFile = Path.Combine(backupFolder, destFileName);
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

                    using (var input = new FileStream(backupFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var output = new FileStream(destFilePath, FileMode.Create))
                        BinaryPatchUtility.Apply(input,
                            () => new FileStream(destPath, FileMode.Open, FileAccess.Read, FileShare.Read), output);

                    File.Delete(destPath);
                }

                Close();
            };

            var filePath = Path.GetDirectoryName(destPath);
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            wc.DownloadFileAsync(new Uri(url), destPath);
        }
    }
}
