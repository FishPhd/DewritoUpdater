using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using DoritoPatcher;
using Xdelta;

namespace Dewritwo
{
  /// <summary>
  ///   Interaction logic for FileDownloadDialog.xaml
  /// </summary>
  public partial class FileDownloadDialog
  {
    private readonly WebClient _wc = new WebClient();
    private readonly MainWindow _window;
    public Exception Error;

    public FileDownloadDialog(MainWindow window, string url, string destPath)
    {
      InitializeComponent();
      _window = window;

      LblStatus.Content = "Downloading " + Path.GetFileName(destPath);

      // sketchy code to replace current exe
      if (File.Exists(destPath) && destPath.ToLower() == Process.GetCurrentProcess().MainModule.FileName.ToLower())
      {
        if (File.Exists(destPath + ".old"))
          File.Delete(destPath + ".old");

        File.Move(destPath, destPath + ".old");
      }

      _wc.DownloadProgressChanged += (s, e) => { DownloadProgress.Value = e.ProgressPercentage; };

      _wc.DownloadFileCompleted += (s, e) =>
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

          string patchFileExtension;
          if (destPath.EndsWith(".xdelta"))
            patchFileExtension = ".xdelta";
          else if (destPath.EndsWith(".bspatch"))
            patchFileExtension = ".bspatch";
          else
            patchFileExtension = null;

          if (patchFileExtension != null && destPath.EndsWith(patchFileExtension))
          {
            var destFilePath = destPath.Substring(0, destPath.Length - patchFileExtension.Length);
            var destFileName = destFilePath.Replace(_window.BasePath, "");
            if (destFileName.StartsWith("\\") || destFileName.StartsWith("/"))
              destFileName = destFileName.Substring(1);

            // patch file
            var backupFolder = Path.Combine(_window.BasePath, "_dewbackup");
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
              if (backupFolder != null && !Directory.Exists(backupFolder))
                Directory.CreateDirectory(backupFolder);

              File.Copy(destFilePath, backupFile);
              File.Delete(destFilePath);
            }

            LblStatus.Content = "Applying patch file...";
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
              new ThreadStart(delegate { }));

            using (var input = new FileStream(backupFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var output = new FileStream(destFilePath, FileMode.Create))
            {
              if (patchFileExtension == ".bspatch")
                BinaryPatchUtility.Apply(input,
                  () => new FileStream(destPath, FileMode.Open, FileAccess.Read, FileShare.Read), output);
              else if (patchFileExtension == ".xdelta")
                new Decoder(input, new FileStream(destPath, FileMode.Open, FileAccess.Read, FileShare.Read), output).Run
                  ();
            }

            File.Delete(destPath);
          }

          Close();
        }

        var filePath = Path.GetDirectoryName(destPath);
        if (filePath != null && !Directory.Exists(filePath))
          Directory.CreateDirectory(filePath);

        _wc.DownloadFileAsync(new Uri(url), destPath);
      };
    }
  }
}