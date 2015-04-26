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
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mime;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Xml.Serialization;
    using System.Windows.Media.Animation;
    using System.Windows.Navigation;
    using System.Windows.Threading;

    using DoritoPatcher;

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
        DewritoSettings HelmetSettings;

        List<string> filesToDownload;

        Thread validateThread;

        public string BasePath = Directory.GetCurrentDirectory();

        private bool silentStart = false;

        public void HideScriptErrors(WebBrowser wb, bool Hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            object objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null) return;
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { Hide });
        }

        //Titlebar control
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

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

            Storyboard fade = (Storyboard)TryFindResource("fade");
            fade.Begin();	// Start animation
            Storyboard fadeServer = (Storyboard)TryFindResource("fadeServer");
            fadeServer.Begin();	// Start animation
            Storyboard fadeStat = (Storyboard)TryFindResource("fadeStat");
            fadeStat.Begin();	// Start animation

            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION");
            key.SetValue("DoritoPatcherWPF.exe", 0x00002af9, RegistryValueKind.DWord);
            key.Close();
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
                    ChangelogContent.Text = wc.DownloadString("http://167.114.156.21:81/honline/changelog.data");
                }
                catch
                {
                    ChangelogContent.Text = "You are offline. No changelog available.";
                }

            }

            if (!File.Exists(BasePath + @"\playername.txt"))
            {
                this.SaveSettings(true);
            }

            /*if (!File.Exists(BasePath + @"\playername.txt"))
            {
                this.SaveSettings(true);
            }
             */

            this.LoadSettings();

            try
            {
                settingsJson = JObject.Parse(File.ReadAllText("dewrito.json"));

                if (settingsJson["gameFiles"] == null || settingsJson["updateServiceUrl"] == null)
                {
                    SetStatus("Failed to read Dewrito updater configuration.", Color.FromRgb(255,0,0));
                    btnAction.Content = "Error";
                    btnAction.Foreground = Brushes.Red;
                    lblVerify.Content = "Error";
                    lblVerify.Foreground = Brushes.Red;
                    lblVerify2.Content = "Error";
                    lblVerify2.Foreground = Brushes.Red;
                    
                    return;
                }
            }
            catch
            {
                SetStatus("Failed to read Dewrito updater configuration.", Color.FromRgb(255,0,0));
                btnAction.Content = "Error";
                btnAction.Foreground = Brushes.Red;
                lblVerify.Content = "Error";
                lblVerify.Foreground = Brushes.Red;
                lblVerify2.Content = "Error";
                lblVerify2.Foreground = Brushes.Red;
                return;
            }

            // CreateHashJson();
            validateThread = new Thread(new ThreadStart(BackgroundThread));
            validateThread.Start();
            
        }

        void server_LoadCompleted(object sender, NavigationEventArgs e)
        {
        }

        private void TabControl_SelectionChanged(object sender, EventArgs e)
        {
            if (Stats != null && Stats.IsSelected && btnAction.Content != "Error" && btnAction.Content != "Update Game")
	        {
		        WebBrowserStats.Navigate("https://hos.llf.to/");
		        HideScriptErrors(WebBrowserStats, true);
	        }
            else if (Server != null && Server.IsSelected && btnAction.Content != "Error" && btnAction.Content != "Update Game")
			{
				WebBrowserServer.Navigate("https://hos.llf.to/");
                HideScriptErrors(WebBrowserServer, true);
            }
        }

        private void cmbHelmetOpen(object sender, EventArgs e)
        {
            cmbChest.Visibility = System.Windows.Visibility.Hidden;
            cmbShoulder.Visibility = System.Windows.Visibility.Hidden;
            cmbArm.Visibility = System.Windows.Visibility.Hidden;
            cmbLegs.Visibility = System.Windows.Visibility.Hidden;
        }

        private void cmbHelmetClosed(object sender, EventArgs e)
        {
            cmbChest.Visibility = System.Windows.Visibility.Visible;
            cmbShoulder.Visibility = System.Windows.Visibility.Visible;
            cmbArm.Visibility = System.Windows.Visibility.Visible;
            cmbLegs.Visibility = System.Windows.Visibility.Visible;
        }

        private void cmbChestOpen(object sender, EventArgs e)
        {
            cmbShoulder.Visibility = System.Windows.Visibility.Hidden;
            cmbArm.Visibility = System.Windows.Visibility.Hidden;
            cmbLegs.Visibility = System.Windows.Visibility.Hidden;
        }

        private void cmbChestClosed(object sender, EventArgs e)
        {
            cmbShoulder.Visibility = System.Windows.Visibility.Visible;
            cmbArm.Visibility = System.Windows.Visibility.Visible;
            cmbLegs.Visibility = System.Windows.Visibility.Visible;
        }

        private void cmbShoulderOpen(object sender, EventArgs e)
        {
            cmbArm.Visibility = System.Windows.Visibility.Hidden;
            cmbLegs.Visibility = System.Windows.Visibility.Hidden;
        }

        private void cmbShoulderClosed(object sender, EventArgs e)
        {
            cmbArm.Visibility = System.Windows.Visibility.Visible;
            cmbLegs.Visibility = System.Windows.Visibility.Visible;
        }

        private void cmbArmOpen(object sender, EventArgs e)
        {
            cmbLegs.Visibility = System.Windows.Visibility.Hidden;
        }

        private void cmbArmClosed(object sender, EventArgs e)
        {
            cmbLegs.Visibility = System.Windows.Visibility.Visible;
        }

        private void cmbLegsOpen(object sender, EventArgs e)
        {
        }

        private void cmbLegsClosed(object sender, EventArgs e)
        {
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
                lblVerify.Content = "Error";
                lblVerify.Foreground = Brushes.Red;
                btnAction.Content = "Error";
                btnAction.Foreground = Brushes.Red;
                lblVerify2.Content = "Error";
                lblVerify2.Foreground = Brushes.Red;
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

                            WebBrowserServer.Visibility = System.Windows.Visibility.Visible;
                            WebBrowserStats.Visibility = System.Windows.Visibility.Visible;

                            Storyboard fade = (Storyboard)TryFindResource("fade");
                            fade.Stop();	// Start animation
                            btnAction.IsEnabled = true;
                        }));

                if (silentStart)
                {
                    this.btnAction_Click(new object(), new RoutedEventArgs());
                }
                return;
            }

            SetStatus("An update is available. (" + latestUpdateVersion + ")", Color.FromRgb(255, 255, 0));
            
            btnAction.Dispatcher.Invoke(
                new Action(
                    () =>
                        {
                            lblVerify.Content = "Update Game";
                            lblVerify2.Content = "Update Game";
                            btnAction.Content = "Update Game";
                            Storyboard fadeServer = (Storyboard)TryFindResource("fadeServer");
                            fadeServer.Stop();	// Start 
                            Storyboard fade = (Storyboard)TryFindResource("fade");
                            fade.Stop();	// Start 
                            Storyboard fadeStat = (Storyboard)TryFindResource("fadeStat");
                            fadeStat.Stop();	// Start 
                            btnAction.IsEnabled = true;
                        }));
            if (silentStart)
            {
                //MessageBox.Show("Sorry, you need to update before the game can be started silently.", "ElDewrito Launcher");
                MsgBox2 MainWindow = new MsgBox2("Sorry, you need to update before the game can be started silently.");
                MainWindow.Show();
                MainWindow.Focus();

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
                    lblVerify.Content = "Error";
                    lblVerify.Foreground = Brushes.Red;
                    btnAction.Content = "Error";
                    btnAction.Foreground = Brushes.Red;
                    lblVerify2.Content = "Error";
                    lblVerify2.Foreground = Brushes.Red;
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

                UpdateContent.Document.Blocks.Add(new Paragraph(new Run(status){Foreground = new SolidColorBrush(color)}));
            }
            else
            {
                UpdateContent.Dispatcher.Invoke(new Action(() => SetStatus(status, color, updateLabel)));
            }
        }

        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            if (btnAction.Content == "Play Game")
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
                    MsgBox2 MainWindow = new MsgBox2("Game executable not found.");
                    MainWindow.Show();
                    MainWindow.Focus();
                }
            }
            else if (btnAction.Content == "Update Game")
            {
                foreach (var file in filesToDownload)
                {
                    SetStatus("Downloading file \"" + file + "\"...", Color.FromRgb(255,255,0));
                    var url = latestUpdate["baseUrl"].ToString().Replace("\"", "") + file;
                    var destPath = Path.Combine(BasePath, file);
                    var dialog = new FileDownloadDialog(this, url, destPath);
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
                    MainWindow.Focus();
                    
                }

                btnAction.Content = "Play Game";
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

            PlayerSettings.config = textbox.Text;
            this.SaveSettings();
        }

        private void SaveSettings(bool overwrite = false)
        {
            if (overwrite || PlayerSettings == null || HelmetSettings == null)
            {
                PlayerSettings = new DewritoSettings();
                PlayerSettings.config = "";

                HelmetSettings = new DewritoSettings();
                HelmetSettings.config = "";
            }

            using (var writer = new StreamWriter(BasePath + @"\playername.txt", append: false))
            {
                writer.Write(PlayerSettings.config);
                writer.Write(HelmetSettings.config);
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
                PlayerSettings.config = reader.ReadLine();
            }

            txtPlayername.Text = PlayerSettings.config;

            //using (var reader = new StreamReader(BasePath + @"\dewrito.cfg"))
            //{
            //    PlayerSettings = serializer.Deserialize(reader) as DewritoSettings;
            //}

            //txtPlayername.Text = PlayerSettings.Playername;
        }
    }
}