using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;

namespace Dewritwo
{
  public partial class MainWindow
  {
    #region Variables/Dictionaries

    private Dictionary<int, string> _doritoKey;
    private bool _updateText = true;
    private string _keyValue;
    private string _eldoritoLatestVersion;
    private FileVersionInfo _eldoritoVersion;
    private string _localEldoritoVersion;
    private const int Entrycollectionsize = 100;
    private readonly string[] _tempVars = new string[Entrycollectionsize];
    private int _tempCount;

    private readonly SHA1 _hasher = SHA1.Create();
    private readonly string[] _skipFileExtensions = {".bik"};

    private readonly string[] _skipFiles =
    {
      "eldorado.exe", "game.cfg", "tags.dat", "binkw32.dll",
      "crash_reporter.exe", "game.cfg_local.cfg"
    };

    private readonly string[] _skipFolders = {".inn.meta.dir", ".inn.tmp.dir", "Frost", "tpi", "bink", "logs"};
    public readonly string BasePath = Directory.GetCurrentDirectory();
    private Dictionary<string, string> _fileHashes;
    private List<string> _filesToDownload;
    private JToken _latestUpdate;
    private string _latestUpdateVersion;
    private JObject _settingsJson;
    private JObject _updateJson;
    private Thread _validateThread;

    #endregion

    #region Main

    public MainWindow()
    {
      try
      {
        InitializeComponent();
      }
      catch
      {
        MessageBox.Show("Install latest .NET (4.5.2)");
        Application.Current.Shutdown();
      }
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      try
      {
        Cfg.Initial("n/a");
        Load();
        AppendDebugLine("Cfg Load Complete", Color.FromRgb(0, 255, 0), DebugLogger);
      }
      catch
      {
        AppendDebugLine("Cfg Load Error: Resetting Launcher Specific Settings", Color.FromRgb(255, 0, 0), DebugLogger);
        Cfg.Initial("cfg");
        Load();
        AppendDebugLine("Cfg Reload Complete", Color.FromRgb(0, 255, 0), DebugLogger);
      }

      if (Directory.Exists("bink") && Cfg.LauncherConfigFile["Launcher.IntroSkip"] == "1")
        Directory.Move("bink", "bink_disabled");

      try
      {
        using (var wc = new WebClient())
        {
          var url = wc.DownloadString("http://eldewrito.anvilonline.net/update.json");
          var update = JObject.Parse(url);
          foreach (var pair in update)
          {
            _eldoritoLatestVersion = pair.Key;
          }
          var data = wc.DownloadString("http://eldewrito.anvilonline.net/" + _eldoritoLatestVersion + "/dewrito.json");
          //Console.WriteLine(update["baseUrl"]);
          _settingsJson = JObject.Parse(data);

          if (_settingsJson["gameFiles"] == null || _settingsJson["updateServiceUrl"] == null)
          {
            AppendDebugLine("Error reading json: gameFiles or updateServiceUrl is missing.", Color.FromRgb(255, 0, 0), DebugLogger);
            Dispatcher.Invoke(() =>
            {
              BtnAction.Content = "Error";
              BtnSkip.Content = "Ignore";
              if (Cfg.LauncherConfigFile.ContainsKey("Launcher.AutoDebug") &&
                  Cfg.LauncherConfigFile["Launcher.AutoDebug"] == "0")
              {
                FlyoutHandler(FlyoutDebug);
              }
            });
            return;
          }
        }
      }
      catch
      {
        AppendDebugLine("Failed to read json", Color.FromRgb(255, 0, 0), DebugLogger);
        Dispatcher.Invoke(() =>
        {
          BtnAction.Content = "Error";
          BtnSkip.Content = "Ignore";
          if (Cfg.LauncherConfigFile.ContainsKey("Launcher.AutoDebug") &&
              Cfg.LauncherConfigFile["Launcher.AutoDebug"] == "0")
          {
            FlyoutHandler(FlyoutDebug);
          }
        });
        return;
      }

      var fade = (Storyboard) TryFindResource("Fade");
      fade.Begin(); // Start animation

      using (var wc = new WebClient())
      {
        try
        {
          ChangelogContent.Text =
            wc.DownloadString("https://raw.githubusercontent.com/FishPhd/DewritoUpdater/master/changelog.data");
        }
        catch
        {
          ChangelogContent.Text = "You are offline. No changelog available.";
        }
      }

      if (VersionCheck())
      {
        BtnSkip.Visibility = Visibility.Hidden;
        BtnAction.Content = "Play Game";

        fade.Stop(); // Start animation

        AppendDebugLine(
          "You have the latest version: " + _eldoritoVersion.ProductVersion,
          Color.FromRgb(0, 255, 0), DebugLogger);

        LblVersion.Content = "Your Version: " + _localEldoritoVersion + "    Latest Version: " + _eldoritoLatestVersion;
      }
      else
      {
        _validateThread = new Thread(BackgroundThread);
        _validateThread.Start();

        if (_localEldoritoVersion == null)
        {
          AppendDebugLine(
            "Your version: Unknown",
            Color.FromRgb(255, 255, 0), DebugLogger);

          AppendDebugLine(
            "Latest Version: " + _eldoritoLatestVersion,
            Color.FromRgb(255, 255, 0), DebugLogger);
          LblVersion.Content = "Your Version: Unknown" + "    Latest Version: " + _eldoritoLatestVersion;
        }
        else
        {
          LblVersion.Content = "Your Version: " + _localEldoritoVersion + "    Latest Version: " +
                               _eldoritoLatestVersion;

          AppendDebugLine(
            "Your version: " + _localEldoritoVersion,
            Color.FromRgb(255, 255, 0), DebugLogger);

          AppendDebugLine(
            "Latest Version: " + _eldoritoLatestVersion,
            Color.FromRgb(255, 255, 0), DebugLogger);
        }
      }
    }

    private bool VersionCheck()
    {
      if (File.Exists(Environment.CurrentDirectory + "\\mtndew.dll"))
      {
        _eldoritoVersion = FileVersionInfo.GetVersionInfo(Environment.CurrentDirectory + "\\mtndew.dll");
        _localEldoritoVersion = _eldoritoVersion.ProductVersion;
      }
      else
      {
        _eldoritoVersion = null;
      }
      if (_localEldoritoVersion == _eldoritoLatestVersion)
        return true;
      return false;
    }

    private void BackgroundThread()
    {
      if (!CompareHashesWithJson)
      {
        return;
      }

      AppendDebugLine("Game files validated, contacting update server...", Color.FromRgb(255, 255, 255), DebugLogger);
      var fade = (Storyboard) TryFindResource("Fade");

      if (!ProcessUpdateData())
      {
        //var confirm = false;
        AppendDebugLine(
          "Failed to retrieve update information from set update server: " + _settingsJson["updateServiceUrl"],
          Color.FromRgb(255, 0, 0), DebugLogger);
        Dispatcher.Invoke(() =>
        {
          BtnAction.Content = "Error";
          BtnSkip.Content = "Ignore";
          if (Cfg.LauncherConfigFile.ContainsKey("Launcher.AutoDebug") &&
              Cfg.LauncherConfigFile["Launcher.AutoDebug"] == "0")
          {
            FlyoutHandler(FlyoutDebug);
          }
        });
      }

      if (_filesToDownload.Count <= 0)
      {
        AppendDebugLine("You have the latest version! (" + _latestUpdateVersion + ")", Color.FromRgb(0, 255, 0), DebugLogger);

        BtnAction.Dispatcher.Invoke(
          () =>
          {
            BtnAction.Content = "Play Game";
            BtnSkip.Visibility = Visibility.Hidden;
            fade.Stop(); // Start animation
          });
        return;
      }

      AppendDebugLine("An update is available. (" + _latestUpdateVersion + ")", Color.FromRgb(255, 255, 0), DebugLogger);

      BtnAction.Dispatcher.Invoke(
        () =>
        {
          BtnAction.Content = "Update";
          fade.Stop(); // Start animation
        });
    }

    private bool ProcessUpdateData()
    {
      try
      {
        var updateData = _settingsJson["updateServiceUrl"].ToString().Replace("\"", "");
        //Console.WriteLine(updateData);
        if (updateData.StartsWith("http"))
        {
          using (var wc = new WebClient())
          {
            try
            {
              updateData = wc.DownloadString(updateData);
            }
            catch
            {
              return false;
            }
          }
        }
        else
        {
          if (!File.Exists(updateData))
            return false;

          updateData = File.ReadAllText(updateData);
        }

        _updateJson = JObject.Parse(updateData);

        _latestUpdate = null;
        foreach (var x in _updateJson)
        {
          if (x.Value["releaseNo"] == null || x.Value["gitRevision"] == null ||
              x.Value["baseUrl"] == null || x.Value["files"] == null)
          {
            return false; // invalid update data
          }

          if (_latestUpdate == null ||
              int.Parse(x.Value["releaseNo"].ToString().Replace("\"", "")) >
              int.Parse(_latestUpdate["releaseNo"].ToString().Replace("\"", "")))
          {
            _latestUpdate = x.Value;
            _latestUpdateVersion = x.Key + "-" + _latestUpdate["gitRevision"].ToString().Replace("\"", "");
          }
        }

        if (_latestUpdate == null)
          return false;

        var patchFiles = new List<string>();
        if (!GetPatchFiles("patchFiles", ref patchFiles))
          return false;

        var xdeltaFiles = new List<string>();
        if (_latestUpdate["xdeltaFiles"] != null && !GetPatchFiles("xdeltaFiles", ref xdeltaFiles))
          return false;

        IDictionary<string, JToken> files = (JObject) _latestUpdate["files"];

        _filesToDownload = new List<string>();
        foreach (var x in files)
        {
          var keyName = x.Key;
          if (!_fileHashes.ContainsKey(keyName) && _fileHashes.ContainsKey(keyName.Replace(@"\", @"/")))
            // linux system maybe?
            keyName = keyName.Replace(@"\", @"/");

          if (!_fileHashes.ContainsKey(keyName) || _fileHashes[keyName] != x.Value.ToString().Replace("\"", ""))
          {
            AppendDebugLine("File \"" + keyName + "\" is missing or a newer version was found.",
              Color.FromRgb(255, 0, 0), DebugLogger);
            Dispatcher.Invoke(() =>
            {
              BtnAction.Content = "Error";
              BtnSkip.Content = "Ignore";
              if (Cfg.LauncherConfigFile.ContainsKey("Launcher.AutoDebug") &&
                  Cfg.LauncherConfigFile["Launcher.AutoDebug"] == "0")
              {
                FlyoutHandler(FlyoutDebug);
              }
            });
            var name = x.Key;
            if (patchFiles.Contains(keyName))
              name += ".bspatch";
            else if (xdeltaFiles.Contains(keyName))
              name += ".xdelta";
            _filesToDownload.Add(name);
          }
        }

        return true;
      }
      catch (WebException)
      {
        return false;
      }
      catch (NullReferenceException)
      {
        return false;
      }
    }

    private bool GetPatchFiles(string node, ref List<string> files)
    {
      foreach (var file in _latestUpdate[node])
      {
        var fileName = (string) file;
        var fileHash = (string) _settingsJson["gameFiles"][fileName];
        if (!_fileHashes.ContainsKey(fileName) &&
            !_fileHashes.ContainsKey(Path.Combine("_dewbackup", fileName)))
        {
          AppendDebugLine("Original file data for file \"" + fileName + "\" not found.",
            Color.FromRgb(255, 0, 0), DebugLogger);
          AppendDebugLine("Please redo your ElDorito installation",
            Color.FromRgb(255, 0, 0), DebugLogger);
          Dispatcher.Invoke(() =>
          {
            BtnAction.Content = "Error";
            BtnSkip.Content = "Ignore";
            if (Cfg.LauncherConfigFile.ContainsKey("Launcher.AutoDebug") &&
                Cfg.LauncherConfigFile["Launcher.AutoDebug"] == "0")
            {
              FlyoutHandler(FlyoutDebug);
            }
          });
          return false;
        }

        if (_fileHashes.ContainsKey(fileName)) // we have the file
        {
          if (_fileHashes[fileName] != fileHash &&
              (!_fileHashes.ContainsKey(Path.Combine("_dewbackup", fileName)) ||
               _fileHashes[Path.Combine("_dewbackup", fileName)] != fileHash))
          {
            AppendDebugLine(
              "File \"" + fileName +
              "\" was found but isn't original, and a valid backup of the original data wasn't found.",
              Color.FromRgb(255, 0, 0), DebugLogger);
            AppendDebugLine("Please redo your ElDorito installation",
              Color.FromRgb(255, 0, 0), DebugLogger);
            Dispatcher.Invoke(() =>
            {
              BtnAction.Content = "Error";
              BtnSkip.Content = "Ignore";
              if (Cfg.LauncherConfigFile.ContainsKey("Launcher.AutoDebug") &&
                  Cfg.LauncherConfigFile["Launcher.AutoDebug"] == "0")
              {
                FlyoutHandler(FlyoutDebug);
              }
            });
            return false;
          }
        }
        else
        {
          // we don't have the file
          if (!_fileHashes.ContainsKey(fileName + ".orig") &&
              (!_fileHashes.ContainsKey(Path.Combine("_dewbackup", fileName)) ||
               _fileHashes[Path.Combine("_dewbackup", fileName)] != fileHash))
          {
            AppendDebugLine("Original file data for file \"" + fileName + "\" not found.",
              Color.FromRgb(255, 0, 0), DebugLogger);
            AppendDebugLine("Please redo your ElDorito installation",
              Color.FromRgb(255, 0, 0), DebugLogger);
            Dispatcher.Invoke(() =>
            {
              BtnAction.Content = "Error";
              BtnSkip.Content = "Ignore";
              if (Cfg.LauncherConfigFile.ContainsKey("Launcher.AutoDebug") &&
                  Cfg.LauncherConfigFile["Launcher.AutoDebug"] == "0")
              {
                FlyoutHandler(FlyoutDebug);
              }
            });
            return false;
          }
        }

        files.Add(fileName);
      }

      return true;
    }

    private bool CompareHashesWithJson
    {
      get
      {
        var watch = Stopwatch.StartNew();
        if (_fileHashes == null)
          HashFilesInFolder(BasePath);
        watch.Stop();
        var seconds = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
        AppendDebugLine("Hash Complete in: " + seconds + " Seconds", Color.FromRgb(0, 255, 0), DebugLogger);

        IDictionary<string, JToken> files = (JObject) _settingsJson["gameFiles"];

        foreach (var x in files)
        {
          var keyName = x.Key;
          if (_fileHashes != null && !_fileHashes.ContainsKey(keyName) &&
              _fileHashes.ContainsKey(keyName.Replace(@"\", @"/")))
            keyName = keyName.Replace(@"\", @"/");

          if (_fileHashes != null && !_fileHashes.ContainsKey(keyName))
          {
            if (_skipFileExtensions.Contains(Path.GetExtension(keyName)))
              continue;

            AppendDebugLine("Failed to find required game file \"" + x.Key + "\"", Color.FromRgb(255, 0, 0), DebugLogger);
            AppendDebugLine("Please redo your ElDorito installation", Color.FromRgb(255, 0, 0), DebugLogger);
            Dispatcher.Invoke(() =>
            {
              BtnAction.Content = "Error";
              BtnSkip.Content = "Ignore";
              if (Cfg.LauncherConfigFile.ContainsKey("Launcher.AutoDebug") &&
                  Cfg.LauncherConfigFile["Launcher.AutoDebug"] == "0")
              {
                FlyoutHandler(FlyoutDebug);
              }
            });
            return false;
          }

          if (_fileHashes == null || _fileHashes[keyName] == x.Value.ToString().Replace("\"", "")) continue;
          if (_skipFileExtensions.Contains(Path.GetExtension(keyName)) ||
              _skipFiles.Contains(Path.GetFileName(keyName)))
            continue;

          AppendDebugLine("Game file \"" + keyName + "\" data is invalid.", Color.FromRgb(255, 0, 0), DebugLogger);
          AppendDebugLine("Your hash: " + _fileHashes[keyName], Color.FromRgb(255, 0, 0), DebugLogger);
          AppendDebugLine("Expected hash: " + x.Value.ToString().Replace("\"", ""), Color.FromRgb(255, 0, 0), DebugLogger);
          AppendDebugLine("Please redo your ElDorito installation", Color.FromRgb(255, 0, 0), DebugLogger);
          Dispatcher.Invoke(() =>
          {
            BtnAction.Content = "Error";
            BtnSkip.Content = "Ignore";
            if (Cfg.LauncherConfigFile.ContainsKey("Launcher.AutoDebug") &&
                Cfg.LauncherConfigFile["Launcher.AutoDebug"] == "0")
            {
              FlyoutHandler(FlyoutDebug);
            }
          });
          return false;
        }

        return true;
      }
    }

    private void HashFilesInFolder(string basePath, string dirPath = "")
    {
      if (_fileHashes == null)
        _fileHashes = new Dictionary<string, string>();

      if (string.IsNullOrEmpty(dirPath))
      {
        dirPath = basePath;
        AppendDebugLine("Validating game files...", Color.FromRgb(255, 255, 255), DebugLogger);
      }

      foreach (var folder in Directory.GetDirectories(dirPath))
      {
        var dirName = Path.GetFileName(folder);
        if (_skipFolders.Contains(dirName))
          continue;
        HashFilesInFolder(basePath, folder);
      }

      foreach (var file in Directory.GetFiles(dirPath))
      {
        try
        {
          using (var stream = File.OpenRead(file))
          {
            var hash = _hasher.ComputeHash(stream);
            var hashStr = BitConverter.ToString(hash).Replace("-", "");

            var fileKey = file.Replace(basePath, "");
            if ((fileKey.StartsWith(@"\") || fileKey.StartsWith("/")) && fileKey.Length > 1)
              fileKey = fileKey.Substring(1);
            if (!_fileHashes.ContainsKey(fileKey))
              _fileHashes.Add(fileKey, hashStr);
          }
        }
        catch
        {
          AppendDebugLine("Game file validation Error", Color.FromRgb(255, 0, 0), DebugLogger);
          Dispatcher.Invoke(() =>
          {
            BtnAction.Content = "Error";
            BtnSkip.Content = "Ignore";
            if (Cfg.LauncherConfigFile.ContainsKey("Launcher.AutoDebug") &&
                Cfg.LauncherConfigFile["Launcher.AutoDebug"] == "0")
            {
              FlyoutHandler(FlyoutDebug);
            }
          });
        }
      }
    }

    #endregion

    #region Flyout Controls

    private async void FlyoutHandler(Flyout sender)
    {
      await Task.Run(() => AsyncFlyoutHandler(sender));
    }

    private void AsyncFlyoutHandler(Flyout sender)
    {
      Dispatcher.Invoke(() =>
      {
        sender.IsOpen = true;
        foreach (var fly in AllFlyouts.FindChildren<Flyout>())
          if (fly.Header != sender.Header)
            fly.IsOpen = false;
        sender.IsOpen = true;
      });
    }

    private void LauncherSettings_Click(object sender, RoutedEventArgs e)
    {
      if (FlyoutLauncherSettings.IsOpen)
        FlyoutLauncherSettings.IsOpen = false;
      else
        FlyoutHandler(FlyoutLauncherSettings);
    }

    private void d_Click(object sender, RoutedEventArgs e)
    {
      Process.Start("http://i.imgur.com/cc9ZcIO.gif");
    }

    private void forceUpdate_Click(object sender, RoutedEventArgs e)
    {
      _validateThread = new Thread(BackgroundThread);
      _validateThread.Start();
    }

    private void Custom_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (Cfg.LauncherConfigFile["Launcher.PlayerMessage"] == "0")
        {
          var messageWindow =
            new MsgBox("This will update your spartan live!",
              "Just edit settings in the launcher while the game is open.");

          messageWindow.Show();
          messageWindow.Focus();

          Cfg.SetVariable("Launcher.PlayerMessage", "1", ref Cfg.LauncherConfigFile);
          Cfg.SaveConfigFile("launcher_prefs.cfg", Cfg.LauncherConfigFile);
        }
      }
      catch
      {
        Cfg.SetVariable("Launcher.PlayerMessage", "0", ref Cfg.LauncherConfigFile);
        Cfg.SaveConfigFile("launcher_prefs.cfg", Cfg.LauncherConfigFile);
        if (Cfg.LauncherConfigFile["Launcher.PlayerMessage"] == "0")
        {
          var messageWindow =
            new MsgBox("This will update your spartan live!",
              "Just edit settings in the launcher while the game is open.");

          messageWindow.Show();
          messageWindow.Focus();

          Cfg.SetVariable("Launcher.PlayerMessage", "1", ref Cfg.LauncherConfigFile);
          Cfg.SaveConfigFile("launcher_prefs.cfg", Cfg.LauncherConfigFile);
        }
      }
      FlyoutHandler(FlyoutCustom);
    }

    private void Changelog_OnClick(object sender, RoutedEventArgs e)
    {
      if (FlyoutChangelog.IsOpen)
        FlyoutChangelog.IsOpen = false;
      else
        FlyoutHandler(FlyoutChangelog);
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
      FlyoutHandler(FlyoutSettings);
    }

    private void Voip_Click(object sender, RoutedEventArgs e)
    {
      FlyoutHandler(FlyoutVoipSettings);
    }

    private void AutoExec_Click(object sender, RoutedEventArgs e)
    {
      var dict = Dictionaries.GetCommandLine();
      CommandLine.SetValue(TextBoxHelper.WatermarkProperty, dict[Convert.ToString(Command.SelectedValue)]);
      FlyoutHandler(FlyoutAutoExec);
      Preview.Text = File.ReadAllText("autoexec.cfg");
    }

    private void Console_Click(object sender, RoutedEventArgs e)
    {
      FlyoutHandler(FlyoutConsole);
    }

    private void Debug_OnClick(object sender, RoutedEventArgs e)
    {
      if (FlyoutDebug.IsOpen)
        FlyoutDebug.IsOpen = false;
      else
        FlyoutHandler(FlyoutDebug);
    }

    #endregion

    #region Controls

    #region Menu

    private void Action_OnClick(object sender, RoutedEventArgs e)
    {
      var startInfo = new ProcessStartInfo
      {
        FileName = "eldorado.exe",
        Arguments = "-launcher"
      };

      if (BtnAction.Content.Equals("Play Game"))
      {
        try
        {
          Process.Start(startInfo);
        }
        catch
        {
          Dispatcher.Invoke(() =>
          {
            AppendDebugLine("Cannot locate eldorado.exe. Are you running in the right location?",
              Color.FromRgb(255, 0, 0), DebugLogger);
            if (Cfg.LauncherConfigFile.ContainsKey("Launcher.AutoDebug") &&
                Cfg.LauncherConfigFile["Launcher.AutoDebug"] == "0")
            {
              FlyoutHandler(FlyoutDebug);
            }
          });
        }

        if (Cfg.LauncherConfigFile["Launcher.Random"] == "1")
          RandomArmor();
        if (Cfg.LauncherConfigFile["Launcher.Close"] == "1")
          Application.Current.Shutdown();
      }
      else if (BtnAction.Content.Equals("Update"))
      {
        foreach (var file in _filesToDownload)
        {
          AppendDebugLine("Downloading file \"" + file + "\"...", Color.FromRgb(255, 255, 0), DebugLogger);
          var url = "http://eldewrito.anvilonline.net/" + _eldoritoLatestVersion + "/" + file;
          var destPath = Path.Combine(BasePath, file);
          var dialog = new FileDownloadDialog(this, url, destPath);
          //var result = dialog.ShowDialog();
          //Update(destPath, url, file);

          AppendDebugLine("Download for file \"" + file + "\" failed.", Color.FromRgb(255, 0, 0), DebugLogger);
          AppendDebugLine("Error: " + dialog.Error.Message, Color.FromRgb(255, 0, 0), DebugLogger);
          Dispatcher.Invoke(() =>
          {
            BtnAction.Content = "Error";
            BtnSkip.Content = "Ignore";
            if (Cfg.LauncherConfigFile.ContainsKey("Launcher.AutoDebug") &&
                Cfg.LauncherConfigFile["Launcher.AutoDebug"] == "0")
            {
              FlyoutHandler(FlyoutDebug);
            }
          });

          if (dialog.Error.InnerException != null)
            AppendDebugLine("Error: " + dialog.Error.InnerException.Message, Color.FromRgb(255, 0, 0), DebugLogger);
          return;
        }

        if (_filesToDownload.Contains("DewritoUpdater.exe"))
        {
          var restartWindow = new MsgBoxRestart("Update complete! Please restart the launcher.");

          restartWindow.Show();
          restartWindow.Focus();
        }
        BtnAction.Content = "Play Game";
        BtnSkip.Visibility = Visibility.Hidden;
        AppendDebugLine("Update successful. You have the latest version! (" + _latestUpdateVersion + ")",
          Color.FromRgb(0, 255, 0), DebugLogger);
        LblVersion.Content = "Your Version: " + _eldoritoLatestVersion + "    Latest Version: " + _eldoritoLatestVersion;
      }
    }

    private void BTNSkip_OnClick(object sender, RoutedEventArgs e)
    {
      if (BtnSkip.Content.Equals("Ignore"))
        AppendDebugLine("Error ignored. You may now play (with possibility of problems)", Color.FromRgb(255, 255, 255), DebugLogger);
      else if (BtnSkip.Content.Equals("Skip"))
        AppendDebugLine("Validating skipped. You may now play (with possibility of problems)",
          Color.FromRgb(255, 255, 255), DebugLogger);
      var fade = (Storyboard) TryFindResource("Fade");
      fade.Stop();
      BtnAction.Content = "Play Game";
      BtnSkip.Visibility = Visibility.Hidden;
    }

    private void Reddit_OnClick(object sender, RoutedEventArgs e)
    {
      Process.Start("https://www.reddit.com/r/HaloOnline/");
    }

    private void Twitter_OnClick(object sender, RoutedEventArgs e)
    {
      Process.Start("https://twitter.com/FishPhdOfficial");
    }

    private void Github_OnClick(object sender, RoutedEventArgs e)
    {
      Process.Start("https://github.com/fishphd");
    }

    #endregion

    #region Console

    private void UserBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Return)
      {
        if (_tempCount < Entrycollectionsize && ConsoleInput.Text != string.Empty && _tempVars[_tempCount] != string.Empty)
        {
          //Console.WriteLine(_tempVars[_tempCount] + @" " + _tempCount);
          while (_tempVars[_tempCount] != null)
            _tempCount++;

          _tempVars[_tempCount] = ConsoleInput.Text;
          _tempCount++;
        }
        AppendDebugLine(Environment.NewLine + Environment.NewLine + ConsoleInput.Text, Color.FromRgb(255, 255, 255), RconConsole, true);

        if (!ConsoleInput.Text.Contains("start"))
          AppendDebugLine(Rcon.DewCon(ConsoleInput.Text), Color.FromRgb(150, 150, 150), RconConsole, true);

        if (ConsoleInput.Text.Contains("start"))
        {
          var startInfo = new ProcessStartInfo
          {
            FileName = "eldorado.exe",
            Arguments = "-launcher"
          };

          try
          {
            Process.Start(startInfo);
            AppendDebugLine(Environment.NewLine + "Starting ElDorito..." + Environment.NewLine, Color.FromRgb(0, 255, 0), RconConsole, true);
          }
          catch
          {
            AppendDebugLine(Environment.NewLine + "Could not find eldorado.exe. Make sure the launcher is in your install location." + Environment.NewLine,
              Color.FromRgb(255, 255, 0), RconConsole, true);
          }
        }

        ConsoleInput.Clear();
        RconConsole.ScrollToEnd();
      }


      if (e.Key == Key.Up)
      {
        var temp = _tempCount - 1;
        if (temp >= 0)
        {
          Console.WriteLine(_tempVars[temp] + @" " + temp);
          ConsoleInput.Clear();
          ConsoleInput.Text = _tempVars[temp];
          ConsoleInput.CaretIndex = ConsoleInput.Text.Length;
          _tempCount--;
        }
        else
        {
          Console.WriteLine(@"Can't go up (Last entry)");
        }
      }

      if (e.Key == Key.Down)
      {
        var temp = _tempCount + 1;
        if (temp < Entrycollectionsize && _tempVars[temp - 1] != null)
        {
          Console.WriteLine(_tempVars[temp] + @" " + temp);
          ConsoleInput.Clear();
          ConsoleInput.Text = _tempVars[temp];
          ConsoleInput.CaretIndex = ConsoleInput.Text.Length;
          _tempCount++;
        }
        else
        {
          Console.WriteLine(@"Can't go down (First entry)");
        }
      }
    }

    #endregion

    #region AutoExec

    private void BindButton_OnGotFocus(object sender, RoutedEventArgs e)
    {
      BindButton.Text = "Press Key";
    }

    private void BindButton_OnLostFocus(object sender, RoutedEventArgs e)
    {
      if (BindButton.Text == "Press Key")
      {
        BindButton.Text = "Unbound";
      }
    }

    private void BindButton_OnKeyDown(object sender, KeyEventArgs e)
    {
      var keyPressed = KeyInterop.VirtualKeyFromKey(e.Key);
      BindButton.Text = Convert.ToString(e.Key);
      _keyValue = _doritoKey.FirstOrDefault(x => x.Key == keyPressed).Value;
      if (!_doritoKey.ContainsKey(keyPressed))
      {
        BindButton.Text = "Invalid Key";
      }

      if (BindButton.Text != "Invalid Key" && BindButton.Text != "Unbound")
      {
        AutoExecWrite(
          "bind " + _keyValue + " " + Command.SelectedValue + (CommandLine.IsEnabled ? " " + CommandLine.Text : ""),
          new Regex("^\\s*bind\\s+[a-z0-9]+\\s+" + Regex.Escape(Convert.ToString(Command.SelectedValue))
                    + "(\\s+.*)?$", RegexOptions.IgnoreCase | RegexOptions.Multiline));
      }
    }

    private void AutoExecWrite(string write, Regex replace)
    {
      if (replace != null && replace.IsMatch(Preview.Text))
      {
        Preview.Text = replace.Replace(Preview.Text, write);
      }
      else
      {
        if (Preview.Text != "") Preview.AppendText(Environment.NewLine);
        Preview.AppendText(write);
      }
      File.WriteAllText("autoexec.cfg", Preview.Text);
    }

    private void Preview_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      File.WriteAllText("autoexec.cfg", Preview.Text);
    }

    private void Action_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      if (Action.SelectedValue.Equals("command"))
      {
        _keyValue = "Unbound";
        BindButton.Text = _keyValue;
        _updateText = false;
        CommandLine.Text = "";
        _updateText = true;
        To.Visibility = Visibility.Hidden;
        BindButton.Visibility = Visibility.Hidden;
        CommandLine.Visibility = Visibility.Visible;
        Command.Width = 258;
        PreviewPanel.Margin = new Thickness(-3, 0, 0, 0);
        PreviewLabel.Margin = new Thickness(4, 5, 0, 0);
      }

      if (Action.SelectedValue.Equals("bind"))
      {
        Command.ItemsSource = Dictionaries.GetCommand();
        _updateText = false;
        CommandLine.Text = "";
        _updateText = true;
        To.Visibility = Visibility.Visible;
        BindButton.Visibility = Visibility.Visible;
        CommandLine.Visibility = Visibility.Visible;
        Command.Width = 150;
        PreviewPanel.Margin = new Thickness(5, 0, 0, 0);
        PreviewLabel.Margin = new Thickness(-4, 5, 0, 0);
      }
    }

    private void CommandLine_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      if (_updateText && Action.SelectedValue.Equals("command"))
      {
        AutoExecWrite(Command.SelectedValue + " " + CommandLine.Text,
          new Regex("^\\s*" + Regex.Escape(Convert.ToString(Command.SelectedValue))
                    + "(\\s+.*)?$", RegexOptions.IgnoreCase | RegexOptions.Multiline));
      }
      else if (_updateText && BindButton.Text != "Invalid Key" && BindButton.Text != "Unbound")
      {
        AutoExecWrite(
          "bind " + _keyValue + " " + Command.SelectedValue + " " + CommandLine.Text,
          new Regex("^\\s*bind\\s+[a-z0-9]+\\s+" + Regex.Escape(Convert.ToString(Command.SelectedValue))
                    + "(\\s+.*)?$", RegexOptions.IgnoreCase | RegexOptions.Multiline));
      }
    }

    private void Command_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      var selection = Dictionaries.GetCommandLine();
      if (selection[Convert.ToString(Command.SelectedValue)].Contains("[No Value]"))
      {
        BindButton.Text = "Unbound";
        CommandLine.IsEnabled = false;
        CommandLine.Text = string.Empty;
      }
      else
      {
        BindButton.Text = "Unbound";
        _updateText = false;
        CommandLine.Text = string.Empty;
        _updateText = true;
        CommandLine.IsEnabled = true;
      }
      CommandLine.SetValue(TextBoxHelper.WatermarkProperty, selection[Convert.ToString(Command.SelectedValue)]);
    }

    #endregion

    #region Debug

    private void AppendDebugLine(string status, Color color, RichTextBox output, bool isConsole = false)
    {
      if (!isConsole)
        status = status + "\u2028";

      Dispatcher.Invoke(() =>
      {
        var tr = new TextRange(output.Document.ContentEnd, output.Document.ContentEnd)
        {
          Text = status
        };

        tr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
      });
    }

    #endregion

    #endregion

    #region Saving/Loading

    #region Customization

    private void Name_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Player.Name", NameBox.Text, ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void Weapon_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!IsLoaded)
        return;

      Cfg.SetVariable("Player.RenderWeapon", Convert.ToString(Weapon.SelectedValue), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void Helmet_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Player.Armor.Helmet", Convert.ToString(Helmet.SelectedValue), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void Chest_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Player.Armor.Chest", Convert.ToString(Chest.SelectedValue), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void Shoulders_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Player.Armor.Shoulders", Convert.ToString(Shoulders.SelectedValue), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void Arms_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Player.Armor.Arms", Convert.ToString(Arms.SelectedValue), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void Legs_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Player.Armor.Legs", Convert.ToString(Legs.SelectedValue), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void clrPrimary_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
      if (!IsLoaded)
      {
        return;
      }
      var color = Convert.ToString(ClrPrimary.SelectedColor).Remove(1, 2);
      Cfg.SetVariable("Player.Colors.Primary", color, ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void clrSecondary_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
      if (!IsLoaded)
      {
        return;
      }
      var color = Convert.ToString(ClrSecondary.SelectedColor).Remove(1, 2);
      Cfg.SetVariable("Player.Colors.Secondary", color, ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void clrVisor_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
      if (!IsLoaded)
      {
        return;
      }
      var color = Convert.ToString(ClrVisor.SelectedColor).Remove(1, 2);
      Cfg.SetVariable("Player.Colors.Visor", color, ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void clrLights_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
      if (!IsLoaded)
      {
        return;
      }
      var color = Convert.ToString(ClrLights.SelectedColor).Remove(1, 2);
      Cfg.SetVariable("Player.Colors.Lights", color, ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void clrHolo_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
      if (!IsLoaded)
      {
        return;
      }
      var color = Convert.ToString(ClrHolo.SelectedColor).Remove(1, 2);
      Cfg.SetVariable("Player.Colors.Holo", color, ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void btnRandom_Click(object sender, RoutedEventArgs e)
    {
      RandomArmor();
    }

    private void RandomArmor()
    {
      var r = new Random();
      var helmet = r.Next(0, 25);
      var chest = r.Next(0, 25);
      var shoulders = r.Next(0, 25);
      var arms = r.Next(0, 25);
      var legs = r.Next(0, 25);

      var randomColor = new Random();
      var primary = $"#{randomColor.Next(0x1000000):X6}";
      var secondary = $"#{randomColor.Next(0x1000000):X6}";
      var visor = $"#{randomColor.Next(0x1000000):X6}";
      var lights = $"#{randomColor.Next(0x1000000):X6}";
      var holo = $"#{randomColor.Next(0x1000000):X6}";

      Helmet.SelectedIndex = helmet;
      Chest.SelectedIndex = chest;
      Shoulders.SelectedIndex = shoulders;
      Arms.SelectedIndex = arms;
      Legs.SelectedIndex = legs;

      var fromString2 = ColorConverter.ConvertFromString(primary);
      if (fromString2 != null)
        ClrPrimary.SelectedColor = (Color) fromString2;
      var convertFromString2 = ColorConverter.ConvertFromString(secondary);
      if (convertFromString2 != null)
        ClrSecondary.SelectedColor = (Color) convertFromString2;
      var o1 = ColorConverter.ConvertFromString(visor);
      if (o1 != null)
        ClrVisor.SelectedColor = (Color) o1;
      var s1 = ColorConverter.ConvertFromString(lights);
      if (s1 != null)
        ClrLights.SelectedColor = (Color) s1;
      var fromString1 = ColorConverter.ConvertFromString(holo);
      if (fromString1 != null)
        ClrHolo.SelectedColor = (Color) fromString1;
      var convertFromString1 = ColorConverter.ConvertFromString(primary);
      if (convertFromString1 != null)
        ClrPrimary.SelectedColor = (Color) convertFromString1;
      var o = ColorConverter.ConvertFromString(secondary);
      if (o != null)
        ClrSecondary.SelectedColor = (Color) o;
      var s = ColorConverter.ConvertFromString(visor);
      if (s != null)
        ClrVisor.SelectedColor = (Color) s;
      var fromString = ColorConverter.ConvertFromString(lights);
      if (fromString != null)
        ClrLights.SelectedColor = (Color) fromString;
      var convertFromString = ColorConverter.ConvertFromString(holo);
      if (convertFromString != null)
        ClrHolo.SelectedColor = (Color) convertFromString;

      Cfg.SetVariable("Player.Armor.Chest", Convert.ToString(Chest.SelectedValue), ref Cfg.ConfigFile);
      Cfg.SetVariable("Player.Armor.Shoulders", Convert.ToString(Shoulders.SelectedValue), ref Cfg.ConfigFile);
      Cfg.SetVariable("Player.Armor.Helmet", Convert.ToString(Helmet.SelectedValue), ref Cfg.ConfigFile);
      Cfg.SetVariable("Player.Armor.Arms", Convert.ToString(Arms.SelectedValue), ref Cfg.ConfigFile);
      Cfg.SetVariable("Player.Armor.Legs", Convert.ToString(Legs.SelectedValue), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void RandomCheck_Changed(object sender, RoutedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Launcher.Random", Convert.ToString(Convert.ToInt32(RandomCheck.IsChecked)),
        ref Cfg.LauncherConfigFile);
      Cfg.SaveConfigFile("launcher_prefs.cfg", Cfg.LauncherConfigFile);
    }

    #endregion

    #region Settings

    private void Fov_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Camera.FOV", Convert.ToString(Fov.Value, CultureInfo.InvariantCulture), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void CrosshairCenter_Changed(object sender, RoutedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Camera.Crosshair", Convert.ToString(Convert.ToInt32(CrosshairCenter.IsChecked)),
        ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void RawInput_Changed(object sender, RoutedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Input.RawInput", Convert.ToString(Convert.ToInt32(RawInput.IsChecked)), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void ServerName_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Server.Name", ServerName.Text, ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void ServerPassword_OnTextChanged(object sender, RoutedEventArgs args)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Server.Password", ServerPassword.Password, ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void MaxPlayer_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Server.MaxPlayers", Convert.ToString(MaxPlayer.Value, CultureInfo.InvariantCulture),
        ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void chkSprint_Changed(object sender, RoutedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Server.SprintEnabled", Convert.ToString(Convert.ToInt32(ChkSprint.IsChecked)), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void chkAss_Changed(object sender, RoutedEventArgs e)
    {
      if (!IsLoaded)
        return;
      Cfg.SetVariable("Server.AssassinationEnabled", Convert.ToString(Convert.ToInt32(ChkAss.IsChecked)), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void StartTimer_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Server.Countdown", Convert.ToString(StartTimer.Value, CultureInfo.InvariantCulture),
        ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
      Fov.Value = 90;
      CrosshairCenter.IsChecked = false;
      RawInput.IsChecked = true;
      ServerName.Text = "HaloOnline Server";
      MaxPlayer.Value = 16;
      StartTimer.Value = 5;
      ChkIntro.IsChecked = true;
    }

    private void chkIntro_Changed(object sender, RoutedEventArgs e)
    {
      if (ChkIntro.IsChecked == true && Directory.Exists("bink"))
      {
        Directory.Move("bink", "bink_disabled");
      }
      else if (ChkIntro.IsChecked == false && Directory.Exists("bink_disabled"))
      {
        Directory.Move("bink_disabled", "bink");
      }
    }

    #endregion

    #region Launcher Settings

    private void Color_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(Colors.SelectedValue.ToString()),
        ThemeManager.GetAppTheme(Cfg.LauncherConfigFile["Launcher.Theme"]));
      Cfg.LauncherConfigFile["Launcher.Color"] = Colors.SelectedValue.ToString();
      Cfg.SaveConfigFile("launcher_prefs.cfg", Cfg.LauncherConfigFile);

      if (Cfg.LauncherConfigFile["Launcher.Color"] == "yellow")
      {
        var convertFromString = ColorConverter.ConvertFromString("#252525");
        if (convertFromString != null)
        {
          var dark = (Color) convertFromString;
          CustomIcon.Fill = new SolidColorBrush(dark);
          SettingsIcon.Fill = new SolidColorBrush(dark);
          VoipIcon.Fill = new SolidColorBrush(dark);
          AutoExecIcon.Fill = new SolidColorBrush(dark);
          ConsoleIcon.Fill = new SolidColorBrush(dark);
          TitleLabel.SetResourceReference(ForegroundProperty, "AccentColorBrush");
          L.Fill = new SolidColorBrush(dark);
          E.Fill = new SolidColorBrush(dark);
        }
        TitleLabel.Content = "ELDEWRITO";
      }
      else
      {
        var convertFromString = ColorConverter.ConvertFromString("#FFFFFF");
        if (convertFromString != null)
        {
          var light = (Color) convertFromString;
          CustomIcon.Fill = new SolidColorBrush(light);
          SettingsIcon.Fill = new SolidColorBrush(light);
          TitleLabel.Foreground = new SolidColorBrush(light);
          TitleLabel.SetResourceReference(ForegroundProperty, "AccentColorBrush");
          VoipIcon.Fill = new SolidColorBrush(light);
          AutoExecIcon.Fill = new SolidColorBrush(light);
          ConsoleIcon.Fill = new SolidColorBrush(light);
          L.Fill = new SolidColorBrush(light);
          E.Fill = new SolidColorBrush(light);
        }
        TitleLabel.Content = "ELDEWRITO";
      }

      if (Cfg.LauncherConfigFile["Launcher.Theme"] == "BaseLight" &&
          Cfg.LauncherConfigFile["Launcher.Color"] == "yellow")
      {
        var convertFromString = ColorConverter.ConvertFromString("#FFFFFF");
        if (convertFromString != null)
        {
          var light = (Color) convertFromString;
          CustomIcon.Fill = new SolidColorBrush(light);
          SettingsIcon.Fill = new SolidColorBrush(light);
          VoipIcon.Fill = new SolidColorBrush(light);
          AutoExecIcon.Fill = new SolidColorBrush(light);
          ConsoleIcon.Fill = new SolidColorBrush(light);
          TitleLabel.SetResourceReference(ForegroundProperty, "AccentColorBrush");
          L.Fill = new SolidColorBrush(light);
          E.Fill = new SolidColorBrush(light);
        }
        TitleLabel.Content = "Where is your god now";
      }
    }

    private void Theme_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(Cfg.LauncherConfigFile["Launcher.Color"]),
        ThemeManager.GetAppTheme(Themes.SelectedValue.ToString()));
      Cfg.LauncherConfigFile["Launcher.Theme"] = Themes.SelectedValue.ToString();
      Cfg.SaveConfigFile("launcher_prefs.cfg", Cfg.LauncherConfigFile);

      if (Cfg.LauncherConfigFile["Launcher.Theme"] == "BaseLight")
      {
        TitleLabel.SetResourceReference(ForegroundProperty, "AccentColorBrush");
      }
      else
      {
        var convertFromString = ColorConverter.ConvertFromString("#FFFFFF");
        if (convertFromString != null)
        {
          var light = (Color) convertFromString;
          TitleLabel.Foreground = new SolidColorBrush(light);
        }
      }

      if (Cfg.LauncherConfigFile["Launcher.Theme"] == "BaseLight" &&
          Cfg.LauncherConfigFile["Launcher.Color"] == "yellow")
      {
        var convertFromString = ColorConverter.ConvertFromString("#FFFFFF");
        if (convertFromString != null)
        {
          var light = (Color) convertFromString;
          CustomIcon.Fill = new SolidColorBrush(light);
          SettingsIcon.Fill = new SolidColorBrush(light);
          VoipIcon.Fill = new SolidColorBrush(light);
          AutoExecIcon.Fill = new SolidColorBrush(light);
          ConsoleIcon.Fill = new SolidColorBrush(light);
          TitleLabel.SetResourceReference(ForegroundProperty, "AccentColorBrush");
          L.Fill = new SolidColorBrush(light);
          E.Fill = new SolidColorBrush(light);
        }
        TitleLabel.Content = "Where is your god now";
      }

      if (Cfg.LauncherConfigFile["Launcher.Theme"] == "BaseDark" && Cfg.LauncherConfigFile["Launcher.Color"] == "yellow")
      {
        var convertFromString = ColorConverter.ConvertFromString("#252525");
        if (convertFromString != null)
        {
          var dark = (Color) convertFromString;
          CustomIcon.Fill = new SolidColorBrush(dark);
          SettingsIcon.Fill = new SolidColorBrush(dark);
          VoipIcon.Fill = new SolidColorBrush(dark);
          AutoExecIcon.Fill = new SolidColorBrush(dark);
          ConsoleIcon.Fill = new SolidColorBrush(dark);
          TitleLabel.SetResourceReference(ForegroundProperty, "AccentColorBrush");
          L.Fill = new SolidColorBrush(dark);
          E.Fill = new SolidColorBrush(dark);
        }
        TitleLabel.Content = "ELDEWRITO";
      }
    }

    private void Launch_Changed(object sender, RoutedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Launcher.Close", Convert.ToString(Convert.ToInt32(Launch.IsChecked)), ref Cfg.LauncherConfigFile);
      Cfg.SaveConfigFile("launcher_prefs.cfg", Cfg.LauncherConfigFile);
    }

    private void AutoDebug_Changed(object sender, RoutedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Launcher.AutoDebug", Convert.ToString(Convert.ToInt32(AutoDebug.IsChecked)),
        ref Cfg.LauncherConfigFile);
      Cfg.SaveConfigFile("launcher_prefs.cfg", Cfg.LauncherConfigFile);
    }

    private void SkipLauncher_Changed(object sender, RoutedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("Game.SkipLauncher", Convert.ToString(Convert.ToInt32(SkipLauncher.IsChecked)), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    #endregion

    #region VOIP Settings

    private void AGC_Changed(object sender, RoutedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("VoIP.AGC", Convert.ToString(Convert.ToInt32(Agc.IsChecked)), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void Echo_Changed(object sender, RoutedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("VoIP.EchoCancellation", Convert.ToString(Convert.ToInt32(Echo.IsChecked)), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void chkVoIPEnabled_Changed(object sender, RoutedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("VoIP.Enabled", Convert.ToString(Convert.ToInt32(ChkVoIpEnabled.IsChecked)), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void VolumeModifier_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("VoIP.VolumeModifier", Convert.ToString(VolumeModifier.Value, CultureInfo.InvariantCulture),
        ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void PTT_Changed(object sender, RoutedEventArgs e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("VoIP.PushToTalk", Convert.ToString(Convert.ToInt32(Ptt.IsChecked)), ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void VAL_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (!IsLoaded)
      {
        return;
      }
      Cfg.SetVariable("VoIP.VoiceActivationLevel", Convert.ToString(Val.Value, CultureInfo.InvariantCulture),
        ref Cfg.ConfigFile);
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    private void btnReset2_Click(object sender, EventArgs e)
    {
      Agc.IsChecked = true;
      Echo.IsChecked = true;
      VolumeModifier.Value = 6;
      Ptt.IsChecked = true;
      Val.Value = -45;
    }

    #endregion

    #region Extra

    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
      Cfg.SaveConfigFile("dewrito_prefs.cfg", Cfg.ConfigFile);
    }

    #endregion

    #region Loading

    private void Load()
    {
      //Customization
      if (Cfg.ConfigFile["Player.Name"] == "Forgot")
        Cfg.SetVariable("Player.Name", "", ref Cfg.ConfigFile);
      NameBox.Text = Cfg.ConfigFile["Player.Name"];
      Weapon.SelectedValue = Cfg.ConfigFile.ContainsKey("Player.RenderWeapon")
        ? Cfg.ConfigFile["Player.RenderWeapon"]
        : Cfg.ConfigFile["Player.RenderWeapon"] = "assault_rifle";
      Helmet.SelectedValue = Cfg.ConfigFile["Player.Armor.Helmet"];
      Chest.SelectedValue = Cfg.ConfigFile["Player.Armor.Chest"];
      Shoulders.SelectedValue = Cfg.ConfigFile["Player.Armor.Shoulders"];
      Arms.SelectedValue = Cfg.ConfigFile["Player.Armor.Arms"];
      Legs.SelectedValue = Cfg.ConfigFile["Player.Armor.Legs"];
      var convertFromString1 = ColorConverter.ConvertFromString(Cfg.ConfigFile["Player.Colors.Primary"]);
      if (convertFromString1 != null)
        ClrPrimary.SelectedColor = (Color) convertFromString1;
      var o = ColorConverter.ConvertFromString(Cfg.ConfigFile["Player.Colors.Secondary"]);
      if (o != null)
        ClrSecondary.SelectedColor = (Color) o;
      var s = ColorConverter.ConvertFromString(Cfg.ConfigFile["Player.Colors.Visor"]);
      if (s != null)
        ClrVisor.SelectedColor = (Color) s;
      var fromString = ColorConverter.ConvertFromString(Cfg.ConfigFile["Player.Colors.Lights"]);
      if (fromString != null)
        ClrLights.SelectedColor = (Color) fromString;
      var convertFromString = ColorConverter.ConvertFromString(Cfg.ConfigFile["Player.Colors.Holo"]);
      if (convertFromString != null)
        ClrHolo.SelectedColor = (Color) convertFromString;
      //Settings
      Fov.Value = Convert.ToDouble(Cfg.ConfigFile["Camera.FOV"]);
      CrosshairCenter.IsChecked = Convert.ToBoolean(Convert.ToInt32(Cfg.ConfigFile["Camera.Crosshair"]));
      RawInput.IsChecked = Convert.ToBoolean(Convert.ToInt32(Cfg.ConfigFile["Input.RawInput"]));
      ServerName.Text = Cfg.ConfigFile["Server.Name"];
      ServerPassword.Password = Cfg.ConfigFile["Server.Password"];
      MaxPlayer.Value = Convert.ToDouble(Cfg.ConfigFile["Server.MaxPlayers"]);
      StartTimer.Value = Convert.ToDouble(Cfg.ConfigFile["Server.Countdown"]);
      ChkSprint.IsChecked = Convert.ToBoolean(Convert.ToInt32(Cfg.ConfigFile["Server.SprintEnabled"]));
      //Launcher Settings
      Colors.SelectedValue = Cfg.LauncherConfigFile["Launcher.Color"];
      Themes.SelectedValue = Cfg.LauncherConfigFile["Launcher.Theme"];
      Launch.IsChecked = Convert.ToBoolean(Convert.ToInt32(Cfg.LauncherConfigFile["Launcher.Close"]));
      RandomCheck.IsChecked = Convert.ToBoolean(Convert.ToInt32(Cfg.LauncherConfigFile["Launcher.Random"]));
      AutoDebug.IsChecked = Convert.ToBoolean(Convert.ToInt32(Cfg.LauncherConfigFile["Launcher.AutoDebug"]));

      //VoIP Settings
      ChkVoIpEnabled.IsChecked = Convert.ToBoolean(Convert.ToInt32(Cfg.ConfigFile["VoIP.Enabled"]));
      ChkAss.IsChecked = Convert.ToBoolean(Convert.ToInt32(Cfg.ConfigFile["Server.AssassinationEnabled"]));
      Agc.IsChecked = Convert.ToBoolean(Convert.ToInt32(Cfg.ConfigFile["VoIP.AGC"]));
      Echo.IsChecked = Convert.ToBoolean(Convert.ToInt32(Cfg.ConfigFile["VoIP.EchoCancellation"]));
      VolumeModifier.Value = Convert.ToDouble(Cfg.ConfigFile["VoIP.VolumeModifier"]);
      Ptt.IsChecked = Convert.ToBoolean(Convert.ToInt32(Cfg.ConfigFile["VoIP.PushToTalk"]));
      Val.Value = Convert.ToDouble(Cfg.ConfigFile["VoIP.VoiceActivationLevel"]);

      //Auto Exec
      if (!File.Exists("autoexec.cfg"))
        File.Create("autoexec.cfg");
      if (!Directory.Exists("mods/medals"))
        Directory.CreateDirectory("mods/medals");
      if (!Directory.Exists("mods/maps"))
        Directory.CreateDirectory("mods/maps");
      if (!Directory.Exists("mods/variants"))
        Directory.CreateDirectory("mods/variants");
      if (Directory.Exists("bink_disabled"))
        ChkIntro.IsChecked = true;

      if(Cfg.CheckIfProcessIsRunning("eldorado"))
        AppendDebugLine(Environment.NewLine + "ElDorito is running!" + Environment.NewLine, Color.FromRgb(0, 255, 0), RconConsole, true);
      else
        AppendDebugLine(Environment.NewLine + "ElDorito is not running. You can start eldorito by typing 'start' or by starting ElDorito normally." + Environment.NewLine,
              Color.FromRgb(255, 255, 0), RconConsole, true);

      _doritoKey = new Dictionary<int, string>
      {
        {0x1B, "escape"},
        {0x70, "f1"},
        {0x71, "f2"},
        {0x72, "f3"},
        {0x73, "f4"},
        {0x74, "f5"},
        {0x75, "f6"},
        {0x76, "f7"},
        {0x77, "f8"},
        {0x78, "f9"},
        {0x79, "f10"},
        {0x7A, "f11"},
        {0x7B, "f12"},
        {0x2C, "printscreen"},
        {0x7D, "f14"},
        {0x7E, "f15"},
        {0xC0, "tilde"},
        {0x31, "1"},
        {0x32, "2"},
        {0x33, "3"},
        {0x34, "4"},
        {0x35, "5"},
        {0x36, "6"},
        {0x37, "7"},
        {0x38, "8"},
        {0x39, "9"},
        {0x30, "0"},
        {0xBD, "minus"},
        {0xBB, "plus"},
        {0x8, "back"},
        {0x9, "tab"},
        {0x51, "Q"},
        {0x57, "W"},
        {0x45, "E"},
        {0x52, "R"},
        {0x54, "T"},
        {0x59, "Y"},
        {0x55, "U"},
        {0x49, "I"},
        {0x4F, "O"},
        {0x50, "P"},
        {0xDB, "lbracket"},
        {0xDD, "rbracket"},
        {0xDC, "pipe"},
        {0x14, "capital"},
        {0x41, "A"},
        {0x53, "S"},
        {0x44, "D"},
        {0x46, "F"},
        {0x47, "G"},
        {0x48, "H"},
        {0x4A, "J"},
        {0x4B, "K"},
        {0x4C, "L"},
        {0xBA, "colon"},
        {0xDE, "quote"},
        {0xD, "enter"},
        {0xA0, "lshift"},
        {0x5A, "Z"},
        {0x58, "X"},
        {0x43, "C"},
        {0x56, "V"},
        {0x42, "B"},
        {0x4E, "N"},
        {0x4D, "M"},
        {0xBC, "comma"},
        {0xBE, "period"},
        {0xBF, "question"},
        {0xA1, "rshift"},
        {0xA2, "lcontrol"},
        {0xA4, "lmenu"},
        {0x20, "space"},
        {0xA5, "rmenu"},
        {0x5D, "apps"},
        {0xA3, "rcontrol"},
        {0x26, "up"},
        {0x28, "down"},
        {0x25, "left"},
        {0x27, "right"},
        {0x2D, "insert"},
        {0x24, "home"},
        {0x21, "pageup"},
        {0x2E, "delete"},
        {0x23, "end"},
        {0x22, "pagedown"},
        {0x90, "numlock"},
        {0x6F, "divide"},
        {0x6A, "multiply"},
        {0x60, "numpad0"},
        {0x61, "numpad1"},
        {0x62, "numpad2"},
        {0x63, "numpad3"},
        {0x64, "numpad4"},
        {0x65, "numpad5"},
        {0x66, "numpad6"},
        {0x67, "numpad7"},
        {0x68, "numpad8"},
        {0x69, "numpad9"},
        {0x6D, "subtract"},
        {0x6B, "add"},
        {0x6E, "decimal"}
      };

      ThemeManager.ChangeAppStyle(Application.Current,
        ThemeManager.GetAccent(Cfg.LauncherConfigFile["Launcher.Color"]),
        ThemeManager.GetAppTheme(Cfg.LauncherConfigFile["Launcher.Theme"]));

      if (Cfg.LauncherConfigFile["Launcher.Color"] == "yellow")
      {
        var dark = (Color) ColorConverter.ConvertFromString("#252525");
        CustomIcon.Fill = new SolidColorBrush(dark);
        SettingsIcon.Fill = new SolidColorBrush(dark);
        VoipIcon.Fill = new SolidColorBrush(dark);
        AutoExecIcon.Fill = new SolidColorBrush(dark);
        ConsoleIcon.Fill = new SolidColorBrush(dark);
        TitleLabel.SetResourceReference(ForegroundProperty, "AccentColorBrush");
        L.Fill = new SolidColorBrush(dark);
        E.Fill = new SolidColorBrush(dark);
      }
      if (Cfg.LauncherConfigFile["Launcher.Theme"] == "BaseLight")
      {
        TitleLabel.SetResourceReference(ForegroundProperty, "AccentColorBrush");
      }
      if (Cfg.LauncherConfigFile["Launcher.Theme"] == "BaseLight" &&
          Cfg.LauncherConfigFile["Launcher.Color"] == "yellow")
      {
        var light = (Color) ColorConverter.ConvertFromString("#FFFFFF");
        CustomIcon.Fill = new SolidColorBrush(light);
        SettingsIcon.Fill = new SolidColorBrush(light);
        VoipIcon.Fill = new SolidColorBrush(light);
        AutoExecIcon.Fill = new SolidColorBrush(light);
        ConsoleIcon.Fill = new SolidColorBrush(light);
        TitleLabel.SetResourceReference(ForegroundProperty, "AccentColorBrush");
        L.Fill = new SolidColorBrush(light);
        E.Fill = new SolidColorBrush(light);
        TitleLabel.Content = "Where is your god now";
      }
    }

    #endregion

    #endregion
  }
}