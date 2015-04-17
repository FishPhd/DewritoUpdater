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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DoritoPatcherWPF
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mime;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Xml.Serialization;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SHA1 hasher = SHA1.Create();
        Dictionary<string, string> fileHashes;
        string[] skipFolders = { ".inn.meta.dir", ".inn.tmp.dir", "Frost", "tpi" };
        string[] skipFileExtensions = { ".bik" };
        string[] skipFiles = { "eldorado.exe", "tags.dat", "game.cfg", "font_package.bin" };

        JObject settingsJson;
        JObject updateJson;
        string latestUpdateVersion;
        JToken latestUpdate;

        DewritoSettings PlayerSettings;

        List<string> filesToDownload;

        Thread validateThread;

        public string BasePath = Directory.GetCurrentDirectory();

        private bool silentStart = false;

        

        //Titlebar control
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Drag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        //

        public MainWindow()
        {
            var e = Environment.GetCommandLineArgs();

            foreach (var entry in e)
            {
                if (entry.StartsWith("-"))
                {
                    switch (entry)
                    {
                        case "-silent":
                            silentStart = true;
                            break;
                    }
                }
            }

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (silentStart)
            {
                this.WindowState = WindowState.Minimized;
            }

            using (var wc = new WebClient())
            {
                try
                {
                    NewsContent.Text = wc.DownloadString("http://167.114.156.21:81/honline/news.data");
                    ChangelogContent.Text = wc.DownloadString("http://167.114.156.21:81/honline/changelog.data");
                }
                catch
                {
                    NewsContent.Text = "You are offline. No news available.";
                    ChangelogContent.Text = "You are offline. No changelog available.";
                }

            }

            //if (!File.Exists(BasePath + @"\dewrito.cfg"))
            //{
            //    this.SaveSettings(true);
            //}
            if (!File.Exists(BasePath + @"\playername.txt"))
            {
                this.SaveSettings(true);
            }

            this.LoadSettings();

            try
            {
                settingsJson = JObject.Parse(File.ReadAllText("dewrito.json"));

                if (settingsJson["gameFiles"] == null || settingsJson["updateServiceUrl"] == null)
                {
                    SetStatus("Failed to read Dewrito updater configuration.", Color.FromRgb(255,0,0));
                    return;
                }
            }
            catch
            {
                SetStatus("Failed to read Dewrito updater configuration.", Color.FromRgb(255,0,0));
                return;
            }

            // CreateHashJson();
            validateThread = new Thread(new ThreadStart(BackgroundThread));
            validateThread.Start();
        }

        private void BackgroundThread()
        {
            if (!CompareHashesWithJson())
            {
                return;
            }

            SetStatus("Game files validated, contacting update server...", Color.FromRgb(255, 255, 255));

            if (!ProcessUpdateData())
            {
                SetStatus("Failed to retrieve update information.", Color.FromRgb(255, 0, 0));
                return;
            }

            if (filesToDownload.Count <= 0)
            {
                SetStatus("You have the latest version! (" + latestUpdateVersion + ")", Color.FromRgb(0, 255, 0));
                btnAction.Dispatcher.Invoke(
                    new Action(
                        () =>
                        {
                            btnAction.Content = "Play Game";
                            btnAction.IsEnabled = true;
                        }));

                if (silentStart)
                {
                    this.btnAction_Click(new object(), new RoutedEventArgs());
                }
                return;
            }

            SetStatus("An update is available. (" + latestUpdateVersion + ")", Color.FromRgb(0, 255, 0));
            btnAction.Dispatcher.Invoke(
                new Action(
                    () =>
                        {
                            btnAction.Content = "Update Game";
                            btnAction.IsEnabled = true;
                        }));
            if (silentStart)
            {
                //MessageBox.Show("Sorry, you need to update before the game can be started silently.", "ElDewrito Launcher");
                MsgBox MainWindow = new MsgBox("Sorry, you need to update before the game can be started silently.");
                MainWindow.Show();

            }
        }

        private bool ProcessUpdateData()
        {
            try
            {
                string updateData = settingsJson["updateServiceUrl"].ToString().Replace("\"", "");
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

                updateJson = JObject.Parse(updateData);

                latestUpdate = null;
                foreach (var x in updateJson)
                {
                    if (x.Value["releaseNo"] == null || x.Value["gitRevision"] == null ||
                        x.Value["baseUrl"] == null || x.Value["files"] == null)
                    {
                        return false; // invalid update data
                    }

                    if (latestUpdate == null ||
                        int.Parse(x.Value["releaseNo"].ToString().Replace("\"", "")) > int.Parse(latestUpdate["releaseNo"].ToString().Replace("\"", "")))
                    {
                        latestUpdate = x.Value;
                        latestUpdateVersion = x.Key + "-" + latestUpdate["gitRevision"].ToString().Replace("\"", "");
                    }
                }

                if (latestUpdate == null)
                    return false;

                List<string> patchFiles = new List<string>();
                foreach (var file in latestUpdate["patchFiles"]) // each file mentioned here must match original hash or have a file in the _dewbackup folder that does
                {
                    string fileName = (string)file;
                    string fileHash = (string)settingsJson["gameFiles"][fileName];
                    if (!fileHashes.ContainsKey(fileName) && !fileHashes.ContainsKey(Path.Combine("_dewbackup", fileName)))
                    {
                        SetStatus("Original file data for file \"" + fileName + "\" not found.", Color.FromRgb(255,0,0));
                        SetStatus("Please redo your Halo Online installation with the original HO files.", Color.FromRgb(255,0,0), false);
                        return false;
                    }

                    if (fileHashes.ContainsKey(fileName)) // we have the file
                    {
                        if (fileHashes[fileName] != fileHash &&
                            (!fileHashes.ContainsKey(Path.Combine("_dewbackup", fileName)) || fileHashes[Path.Combine("_dewbackup", fileName)] != fileHash))
                        {
                            SetStatus("File \"" + fileName + "\" was found but isn't original, and a valid backup of the original data wasn't found.", Color.FromRgb(255, 0, 0));
                            SetStatus("Please redo your Halo Online installation with the original HO files.", Color.FromRgb(255, 0, 0), false);
                            return false;
                        }
                    }
                    else
                    {
                        // we don't have the file
                        if (!fileHashes.ContainsKey(fileName + ".orig") &&
                            (!fileHashes.ContainsKey(Path.Combine("_dewbackup", fileName)) || fileHashes[Path.Combine("_dewbackup", fileName)] != fileHash))
                        {
                            SetStatus("Original file data for file \"" + fileName + "\" not found.", Color.FromRgb(255, 0, 0));
                            SetStatus("Please redo your Halo Online installation with the original HO files.", Color.FromRgb(255, 0, 0), false);
                            return false;
                        }
                    }

                    patchFiles.Add(fileName);
                }

                IDictionary<string, JToken> files = (JObject)latestUpdate["files"];

                filesToDownload = new List<string>();
                foreach (var x in files)
                {
                    string keyName = x.Key;
                    if (!fileHashes.ContainsKey(keyName) && fileHashes.ContainsKey(keyName.Replace(@"\", @"/"))) // linux system maybe?
                        keyName = keyName.Replace(@"\", @"/");

                    if (!fileHashes.ContainsKey(keyName) || fileHashes[keyName] != x.Value.ToString().Replace("\"", ""))
                    {
                        SetStatus("File \"" + keyName + "\" is missing or a newer version was found.", Color.FromRgb(255, 0, 0));
                        var name = x.Key;
                        if (patchFiles.Contains(keyName))
                            name += ".bspatch";
                        filesToDownload.Add(name);
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

        private bool CompareHashesWithJson()
        {
            if (fileHashes == null)
                HashFilesInFolder(BasePath);

            IDictionary<string, JToken> files = (JObject)settingsJson["gameFiles"];

            foreach (var x in files)
            {
                string keyName = x.Key;
                if (!fileHashes.ContainsKey(keyName) && fileHashes.ContainsKey(keyName.Replace(@"\", @"/"))) // linux system maybe?
                    keyName = keyName.Replace(@"\", @"/");

                if (!fileHashes.ContainsKey(keyName))
                {
                    if (skipFileExtensions.Contains(Path.GetExtension(keyName)))
                        continue;

                    SetStatus("Failed to find required game file \"" + x.Key + "\"", Color.FromRgb(255, 0, 0));
                    SetStatus("Please redo your Halo Online installation with the original HO files.", Color.FromRgb(255, 0, 0), false);
                    return false;
                }

                if (fileHashes[keyName] != x.Value.ToString().Replace("\"", ""))
                {
                    if (skipFileExtensions.Contains(Path.GetExtension(keyName)) || skipFiles.Contains(Path.GetFileName(keyName)))
                        continue;

                    SetStatus("Game file \"" + keyName + "\" data is invalid.", Color.FromRgb(255, 0, 0));
                    SetStatus("Your hash: " + fileHashes[keyName], Color.FromRgb(255, 0, 0), false);
                    SetStatus("Expected hash: " + x.Value.ToString().Replace("\"", ""), Color.FromRgb(255, 0, 0), false);
                    SetStatus("Please redo your Halo Online installation with the original HO files.", Color.FromRgb(255,0,0), false);
                    return false;
                }
            }

            return true;
        }

        private void CreateHashJson()
        {
            if (fileHashes == null)
                HashFilesInFolder(BasePath);

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("{");
            foreach (var kvp in fileHashes)
            {
                builder.AppendLine(String.Format("    \"{0}\": \"{1}\",", kvp.Key.Replace(@"\", @"\\"), kvp.Value));
            }
            builder.AppendLine("}");
            var json = builder.ToString();
            json = json;
        }

        private void HashFilesInFolder(string basePath, string dirPath = "")
        {
            if (fileHashes == null)
                fileHashes = new Dictionary<string, string>();

            if (String.IsNullOrEmpty(dirPath))
            {
                dirPath = basePath;
                SetStatus("Validating game files...", Color.FromRgb(255,255,255));
            }

            foreach (var folder in Directory.GetDirectories(dirPath))
            {
                var dirName = Path.GetFileName(folder);
                if (skipFolders.Contains(dirName))
                    continue;
                HashFilesInFolder(basePath, folder);
            }

            foreach (var file in Directory.GetFiles(dirPath))
            {
                try
                {
                    using (var stream = File.OpenRead(file))
                    {
                        var hash = hasher.ComputeHash(stream);
                        var hashStr = BitConverter.ToString(hash).Replace("-", "");

                        var fileKey = file.Replace(basePath, "");
                        if ((fileKey.StartsWith(@"\") || fileKey.StartsWith("/")) && fileKey.Length > 1)
                            fileKey = fileKey.Substring(1);
                        if (!fileHashes.ContainsKey(fileKey))
                            fileHashes.Add(fileKey, hashStr);
                    }
                }
                catch { }
            }
        }

        private void SetStatus(string status, Color color, bool updateLabel = true)
        {
            if (UpdateContent.Dispatcher.CheckAccess())
            {
                if (updateLabel)
                {
                    lblStatus.Foreground = new SolidColorBrush(color);
                    lblStatus.Content = status;
                }

                UpdateContent.Document.Blocks.Add(new Paragraph(new Run(status){Foreground = new SolidColorBrush(color)}));
            }
            else
            {
                UpdateContent.Dispatcher.Invoke(new Action(() => SetStatus(status, color, updateLabel)));
            }
        }

        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            if (button.Content == "Play Game")
            {
                ProcessStartInfo sInfo = new ProcessStartInfo(BasePath + "/eldorado.exe");
                sInfo.Arguments = "-launcher";

                try
                {
                    Process.Start(sInfo);
                    
                }
                catch
                {
                    //MessageBox.Show("Game executable not found.");
                    MsgBox MainWindow = new MsgBox("Game executable not found.");
                    MainWindow.Show();
                }
            }
            else if (button.Content == "Update Game")
            {
                foreach (var file in filesToDownload)
                {
                    SetStatus("Downloading file \"" + file + "\"...", Color.FromRgb(255,255,255));
                    var url = latestUpdate["baseUrl"].ToString().Replace("\"", "") + file;
                    var destPath = Path.Combine(BasePath, file);
                    FileDownloadDialog dialog = new FileDownloadDialog(this, url, destPath);
                    var result = dialog.ShowDialog();
                    if (result.HasValue && result.Value)
                    {
                        // TOD: Refactor this. It's hacky
                    }
                    else
                    {
                        SetStatus("Download for file \"" + file + "\" failed.", Color.FromRgb(255, 0, 0));
                        SetStatus("Error: " + dialog.Error.Message, Color.FromRgb(255, 0, 0), false);
                        if (dialog.Error.InnerException != null)
                            SetStatus("Error: " + dialog.Error.InnerException.Message, Color.FromRgb(255, 0, 0), false);
                        return;
                    }
                }

                if (filesToDownload.Contains("DewritoUpdater.exe"))
                {
                    //MessageBox.Show("Update complete! Please restart the launcher.", "ElDewrito Launcher");
                    //Application.Current.Shutdown();
                    MsgBox MainWindow = new MsgBox("Update complete! Please restart the launcher.");
                    MainWindow.Show();
                    
                }

                button.Content = "Play Game";
                SetStatus("Update successful. You have the latest version! (" + latestUpdateVersion + ")", Color.FromRgb(0, 255, 0));
            }
        }

        private void btnIRC_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("http://irc.lc/gamesurge/eldorito");
            Process.Start(sInfo);
        }

        private void btnBug_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://gitlab.com/emoose/ElDorito/issues");
            Process.Start(sInfo);
        }

        private void txtPlayername_LostFocus(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;

            PlayerSettings.Playername = textbox.Text;
            this.SaveSettings();
        }

        private void SaveSettings(bool overwrite = false)
        {
            if (overwrite)
            {
                PlayerSettings = new DewritoSettings();

                PlayerSettings.Playername = "";
            }

            using (var writer = new StreamWriter(BasePath + @"\playername.txt", append: false))
            {
                writer.Write(PlayerSettings.Playername);
            }

            //if (overwrite)
            //{
            //    PlayerSettings = new DewritoSettings();

            //    PlayerSettings.Playername = "Spartan";
            //}

            //using (var writer = new StreamWriter(BasePath + @"\dewrito.cfg", append: false))
            //{
            //    JsonConvert.SerializeObject(PlayerSettings);
            //}
        }

        private void LoadSettings()
        {
            using (var reader = new StreamReader(BasePath + @"\playername.txt"))
            {
                PlayerSettings = new DewritoSettings();
                PlayerSettings.Playername = reader.ReadLine();
            }

            txtPlayername.Text = PlayerSettings.Playername;

            //using (var reader = new StreamReader(BasePath + @"\dewrito.cfg"))
            //{
            //    PlayerSettings = serializer.Deserialize(reader) as DewritoSettings;
            //}

            //txtPlayername.Text = PlayerSettings.Playername;
        }
    }
}