using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace DewritoUpdater
{
    public partial class MainWindow : Window
    {
        private static Dictionary<string, string> configFile;
        //private readonly bool embedded = true;
        private readonly SHA1 hasher = SHA1.Create();
        private readonly bool silentStart;
        private readonly string[] skipFileExtensions = {".bik"};

        private readonly string[] skipFiles =
        {
            "eldorado.exe", "game.cfg", "tags.dat", "binkw32.dll",
            "crash_reporter.exe", "game.cfg_local.cfg"
        };

        private readonly string[] skipFolders = {".inn.meta.dir", ".inn.tmp.dir", "Frost", "tpi", "bink", "logs"};
        public string BasePath = Directory.GetCurrentDirectory();
        private Dictionary<string, string> fileHashes;
        private Dictionary<int, string> doritoKey;
        private List<string> filesToDownload;
        private bool isPlayEnabled;
        private JToken latestUpdate;
        private string latestUpdateVersion;
        private JObject settingsJson;
        private JObject updateJson;
        private Thread validateThread;
        
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

            // proceed starting app...

            InitializeComponent();
        }

        //RCON
        public static string dewCmd(string cmd)
        {
            var data = new byte[1024];
            string stringData;
            TcpClient server;
            try
            {
                server = new TcpClient("127.0.0.1", 2448);
            }
            catch (SocketException)
            {
                return "Unable to talk to Eldewrito, is it running?";
            }
            var ns = server.GetStream();

            var recv = ns.Read(data, 0, data.Length);
            stringData = Encoding.ASCII.GetString(data, 0, recv);

            ns.Write(Encoding.ASCII.GetBytes(cmd), 0, cmd.Length);
            ns.Flush();

            ns.Close();
            server.Close();
            return "Done";
        }

        /* --- Titlebar Control --- */

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            clrPrimary.SelectedColor = (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Primary"]);
            clrSecondary.SelectedColor =
                (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Secondary"]);
            clrLights.SelectedColor = (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Lights"]);
            clrHolo.SelectedColor = (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Holo"]);
            clrVisor.SelectedColor = (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Visor"]);
            cmbLegs.SelectedValue = configFile["Player.Armor.Legs"];
            cmbArms.SelectedValue = configFile["Player.Armor.Arms"];
            cmbHelmet.SelectedValue = configFile["Player.Armor.Helmet"];
            cmbChest.SelectedValue = configFile["Player.Armor.Chest"];
            cmbShoulders.SelectedValue = configFile["Player.Armor.Shoulders"];
            plrName.Text = configFile["Player.Name"];
            SaveConfigFile("dewrito_prefs.cfg", configFile);

            Application.Current.Shutdown();
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /* --- Content Panel Control --- */

        private void switchPanel(string panel, bool animationComplete)
        {
            if (!animationComplete)
            {
                var fadePanels = (Storyboard) TryFindResource("fadePanels");
                fadePanels.Completed += (sender, e) => switchPanelAnimationComplete(sender, e, panel);
                fadePanels.Begin(); // Start fadeout
            }
            else
            {
                ChangelogGrid.Visibility = Visibility.Hidden;
                Settings.Visibility = Visibility.Hidden;
                Customization.Visibility = Visibility.Hidden;
                mainButtons.Visibility = Visibility.Hidden;
                voipsettings.Visibility = Visibility.Hidden;
                Debug.Visibility = Visibility.Hidden;

                switch (panel)
                {
                    case "main":
                        mainButtons.Visibility = Visibility.Visible;
                        break;
                    case "settings":
                        Settings.Visibility = Visibility.Visible;
                        break;
                    case "voipsettings":
                        voipsettings.Visibility = Visibility.Visible;
                        break;
                    case "custom":
                        Customization.Visibility = Visibility.Visible;
                        break;
                    case "changelog":
                        ChangelogGrid.Visibility = Visibility.Visible;
                        break;
                    case "debug":
                        Debug.Visibility = Visibility.Visible;
                        break;
                    default:
                        mainButtons.Visibility = Visibility.Visible;
                        break;
                }
                var showPanels = (Storyboard) TryFindResource("showPanels");
                showPanels.Begin(); // Start fadein
            }
        }

        private void switchPanelAnimationComplete(object sender, EventArgs e, string panel)
        {
            switchPanel(panel, true);
        }

        private void btnChangelog_Click(object sender, EventArgs e)
        {
            switchPanel("changelog", false);
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
            switchPanel("debug", false);
        }

        private void btnOkDebug_Click(object sender, EventArgs e)
        {
            switchPanel("main", false);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            switchPanel("main", false);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            try {
            //Settings
            sldFov.Value = Convert.ToDouble(configFile["Camera.FOV"]);
            sldMax.Value = Convert.ToDouble(configFile["Server.MaxPlayers"]);
            sldTimer.Value = Convert.ToDouble(configFile["Server.Countdown"]);
            chkCenter.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Camera.Crosshair"]));
            chkRaw.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Input.RawInput"]));

            chkFull.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Video.FullScreen"]));
            chkFPS.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Video.FPSCounter"]));
            chkIntro.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Video.IntroSkip"]));
            chkVSync.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Video.VSync"]));
            chkWin.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Video.Window"]));
            lblServerName.Text = configFile["Server.Name"];
            lblServerPassword.Password = configFile["Server.Password"];
            //chkBeta.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Game.BetaFiles"]));

            //Video
            SaveConfigFile("dewrito_prefs.cfg", configFile);
            }
            catch
            {
                SetVariable("Server.Name", "HaloOnline Server", ref configFile);
                SetVariable("Server.Password", "", ref configFile);
                SetVariable("Server.Countdown", "5", ref configFile);
                SetVariable("Server.MaxPlayers", "16", ref configFile);
                SetVariable("Server.Port", "11775", ref configFile);
                SetVariable("Camera.Crosshair", "0", ref configFile);
                SetVariable("Camera.FOV", "90", ref configFile);
                SetVariable("Camera.HideHUD", "0", ref configFile);
                SetVariable("Input.RawInput", "1", ref configFile);
                SetVariable("Video.Height", Convert.ToString(Convert.ToInt32(SystemParameters.PrimaryScreenHeight)),
                    ref configFile);
                SetVariable("Video.Width", Convert.ToString(Convert.ToInt32(SystemParameters.PrimaryScreenWidth)),
                    ref configFile);
                SetVariable("Video.Window", "0", ref configFile);
                SetVariable("Video.FullScreen", "1", ref configFile);
                SetVariable("Video.VSync", "1", ref configFile);
                SetVariable("Video.FPSCounter", "0", ref configFile);
                SetVariable("Video.IntroSkip", "1", ref configFile);
                SaveConfigFile("dewrito_prefs.cfg", configFile);

                //Settings
                sldFov.Value = Convert.ToDouble(configFile["Camera.FOV"]);
                sldMax.Value = Convert.ToDouble(configFile["Server.MaxPlayers"]);
                sldTimer.Value = Convert.ToDouble(configFile["Server.Countdown"]);
                chkCenter.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Camera.Crosshair"]));
                chkRaw.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Input.RawInput"]));

                chkFull.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Video.FullScreen"]));
                chkFPS.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Video.FPSCounter"]));
                chkIntro.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Video.IntroSkip"]));
                chkVSync.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Video.VSync"]));
                chkWin.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Video.Window"]));
                lblServerName.Text = configFile["Server.Name"];
                lblServerPassword.Password = configFile["Server.Password"];
                //chkBeta.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["Game.BetaFiles"]));
            }
            switchPanel("settings", false);
        }

        private void BtnVoip_OnClick(object sender, RoutedEventArgs e)
        {
            doritoKey = new Dictionary<int, string>()
            {
                { 0x1B, "escape" },
                { 0x70, "f1" },
                { 0x71, "f2" },
                { 0x72, "f3" },
                { 0x73, "f4" },
                { 0x74, "f5" },
                { 0x75, "f6" },
                { 0x76, "f7" },
                { 0x77, "f8" },
                { 0x78, "f9" },
                { 0x79, "f10" },
                { 0x7A, "f11" },
                { 0x7B, "f12" },
                { 0x2C, "printscreen" },
                { 0x7D, "f14" },
                { 0x7E, "f15" },
                { 0xC0, "tilde" },
                { 0x31, "1" },
                { 0x32, "2" },
                { 0x33, "3" },
                { 0x34, "4" },
                { 0x35, "5" },
                { 0x36, "6" },
                { 0x37, "7" },
                { 0x38, "8" },
                { 0x39, "9" },
                { 0x30, "0" },
                { 0xBD, "minus" },
                { 0xBB, "plus" },
                { 0x8, "back" },
                { 0x9, "tab" },
                { 0x51, "Q" },
                { 0x57, "W" },
                { 0x45, "E" },
                { 0x52, "R" },
                { 0x54, "T" },
                { 0x59, "Y" },
                { 0x55, "U" },
                { 0x49, "I" },
                { 0x4F, "O" },
                { 0x50, "P" },
                { 0xDB, "lbracket" },
                { 0xDD, "rbracket" },
                { 0xDC, "pipe" },
                { 0x14, "capital" },
                { 0x41, "A" },
                { 0x53, "S" },
                { 0x44, "D" },
                { 0x46, "F" },
                { 0x47, "G" },
                { 0x48, "H" },
                { 0x4A, "J" },
                { 0x4B, "K" },
                { 0x4C, "L" },
                { 0xBA, "colon" },
                { 0xDE, "quote" },
                { 0xD, "enter" },
                { 0xA0, "lshift" },
                { 0x5A, "Z" },
                { 0x58, "X" },
                { 0x43, "C" },
                { 0x56, "V" },
                { 0x42, "B" },
                { 0x4E, "N" },
                { 0x4D, "M" },
                { 0xBC, "comma" },
                { 0xBE, "period" },
                { 0xBF, "question" },
                { 0xA1, "rshift" },
                { 0xA2, "lcontrol" },
                { 0xA4, "lmenu" },
                { 0x20, "space" },
                { 0xA5, "rmenu" },
                { 0x5D, "apps" },
                { 0xA3, "rcontrol" },
                { 0x26, "up" },
                { 0x28, "down" },
                { 0x25, "left" },
                { 0x27, "right" },
                { 0x2D, "insert" },
                { 0x24, "home" },
                { 0x21, "pageup" },
                { 0x2E, "delete" },
                { 0x23, "end" },
                { 0x22, "pagedown" },
                { 0x90, "numlock" },
                { 0x6F, "divide" },
                { 0x6A, "multiply" },
                { 0x60, "numpad0" },
                { 0x61, "numpad1" },
                { 0x62, "numpad2" },
                { 0x63, "numpad3" },
                { 0x64, "numpad4" },
                { 0x65, "numpad5" },
                { 0x66, "numpad6" },
                { 0x67, "numpad7" },
                { 0x68, "numpad8" },
                { 0x69, "numpad9" },
                { 0x6D, "subtract" },
                { 0x6B, "add" },
                { 0x6E, "decimal" },
            };

            try
            {
                //Voip
                string capital = configFile["VoIP.PushToTalkKey"].First().ToString().ToUpper() + configFile["VoIP.PushToTalkKey"].Remove(0, 1).ToLower();

                voipKey.Text = capital;
                sldAudio.Value = Convert.ToDouble(configFile["VoIP.VoiceActivationLevel"]);
                sldModifier.Value = Convert.ToDouble(configFile["VoIP.VolumeModifier"]);
                chkPTT.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["VoIP.PushToTalk"]));
                chkEC.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["VoIP.EchoCancellation"]));
                chkAGC.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["VoIP.AGC"]));
                SaveConfigFile("dewrito_prefs.cfg", configFile);
            }
            catch
            {
                SetVariable("VoIP.PushToTalkKey", "capital", ref configFile);
                SetVariable("VoIP.VoiceActivationLevel", "-45", ref configFile);
                SetVariable("VoIP.VolumeModifier", "6", ref configFile);
                SetVariable("VoIP.PushToTalk", "1", ref configFile);
                SetVariable("VoIP.EchoCancellation", "1", ref configFile);
                SetVariable("VoIP.AGC", "1", ref configFile);
                SaveConfigFile("dewrito_prefs.cfg", configFile);

                Console.WriteLine(Convert.ToString(KeyInterop.KeyFromVirtualKey(Convert.ToInt32(configFile["VoIP.PushToTalkKey"]))));
                voipKey.Text =
                    Convert.ToString(KeyInterop.KeyFromVirtualKey(Convert.ToInt32(configFile["VoIP.PushToTalkKey"])));
                sldAudio.Value = Convert.ToDouble(configFile["VoIP.VoiceActivationLevel"]);
                sldModifier.Value = Convert.ToDouble(configFile["VoIP.VolumeModifier"]);
                chkPTT.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["VoIP.PushToTalk"]));
                chkEC.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["VoIP.EchoCancellation"]));
                chkAGC.IsChecked = Convert.ToBoolean(Convert.ToInt32(configFile["VoIP.AGC"]));
            }
            switchPanel("voipsettings", false);
        }

        private void btnCustomization_Click(object sender, EventArgs e)
        {
            //Customization
            clrPrimary.SelectedColor = (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Primary"]);
            clrSecondary.SelectedColor =
                (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Secondary"]);
            clrLights.SelectedColor = (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Lights"]);
            clrHolo.SelectedColor = (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Holo"]);
            clrVisor.SelectedColor = (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Visor"]);
            cmbLegs.SelectedValue = configFile["Player.Armor.Legs"];
            cmbArms.SelectedValue = configFile["Player.Armor.Arms"];
            cmbHelmet.SelectedValue = configFile["Player.Armor.Helmet"];
            cmbChest.SelectedValue = configFile["Player.Armor.Chest"];
            cmbShoulders.SelectedValue = configFile["Player.Armor.Shoulders"];
            plrName.Text = configFile["Player.Name"];
            SaveConfigFile("dewrito_prefs.cfg", configFile);
            switchPanel("custom", false);
        }

        private void helmetOpen(object sender, EventArgs e)
        {
            cmbChest.Visibility = Visibility.Hidden;
            cmbShoulders.Visibility = Visibility.Hidden;
            cmbArms.Visibility = Visibility.Hidden;
            cmbLegs.Visibility = Visibility.Hidden;
        }

        private void helmetClose(object sender, EventArgs e)
        {
            cmbChest.Visibility = Visibility.Visible;
            cmbShoulders.Visibility = Visibility.Visible;
            cmbArms.Visibility = Visibility.Visible;
            cmbLegs.Visibility = Visibility.Visible;
        }

        private void chestOpen(object sender, EventArgs e)
        {
            cmbShoulders.Visibility = Visibility.Hidden;
            cmbArms.Visibility = Visibility.Hidden;
            cmbLegs.Visibility = Visibility.Hidden;
        }

        private void chestClose(object sender, EventArgs e)
        {
            cmbShoulders.Visibility = Visibility.Visible;
            cmbArms.Visibility = Visibility.Visible;
            cmbLegs.Visibility = Visibility.Visible;
        }

        private void shouldersOpen(object sender, EventArgs e)
        {
            cmbArms.Visibility = Visibility.Hidden;
            cmbLegs.Visibility = Visibility.Hidden;
        }

        private void shouldersClose(object sender, EventArgs e)
        {
            cmbArms.Visibility = Visibility.Visible;
            cmbLegs.Visibility = Visibility.Visible;
        }

        private void armsOpen(object sender, EventArgs e)
        {
            cmbLegs.Visibility = Visibility.Hidden;
        }

        private void armsClose(object sender, EventArgs e)
        {
            cmbLegs.Visibility = Visibility.Visible;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (silentStart)
            {
                WindowState = WindowState.Minimized;
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

            if (!Directory.Exists("mods/medals"))
                Directory.CreateDirectory("mods/medals");

            try
            {
                Initial(false);
            }
            catch
            {
                Initial(true);
            }

            try
            {
                settingsJson = JObject.Parse(File.ReadAllText("dewrito.json"));
                if (settingsJson["gameFiles"] == null || settingsJson["updateServiceUrl"] == null)
                {
                    lblVersion.Text = "Error";
                    AppendDebugLine("Error reading dewrito.json: gameFiles or updateServiceUrl is missing.",
                        Color.FromRgb(255, 0, 0));
                    SetButtonText("ERROR", true);

                    var AlertWindow = new MsgBoxOk("Could not read the dewrito.json updater configuration.");
                    AlertWindow.Show();
                    AlertWindow.Focus();
                    return;
                }
            }
            catch
            {
                AppendDebugLine("Failed to read dewrito.json updater configuration.", Color.FromRgb(255, 0, 0));
                SetButtonText("ERROR", true);

                var AlertWindow = new MsgBoxOk("Could not read the dewrito.json updater configuration.");
                AlertWindow.Show();
                AlertWindow.Focus();
                return;
            }

            var fade = (Storyboard)TryFindResource("fade");
            fade.Begin(); // Start animation

            // CreateHashJson();
            validateThread = new Thread(BackgroundThread);
            validateThread.Start();
        }

        private void BackgroundThread()
        {
            if (!CompareHashesWithJson())
            {
                return;
            }

            AppendDebugLine("Game files validated, contacting update server...", Color.FromRgb(255, 255, 255));

            if (!ProcessUpdateData())
            {
                var confirm = false;

                AppendDebugLine(
                    "Failed to retrieve update information from set update server: " + settingsJson["updateServiceUrl"],
                    Color.FromRgb(255, 0, 0));

                if (settingsJson["updateServiceUrl"].ToString() != "http://167.114.156.21:81/honline/update.json" ||
                    settingsJson["updateServiceUrl"].ToString() !=
                    "http://167.114.156.21:81/honline/update_publicbeta.json")
                {
                    AppendDebugLine("Set update server is not default server...", Color.FromRgb(255, 255, 255));
                    AppendDebugLine("Attempting to contact the default update server...", Color.FromRgb(255, 255, 255));

                    Application.Current.Dispatcher.Invoke((Action) delegate
                    {
                        var confirmWindow =
                            new MsgBoxConfirm(
                                "Failed to retrieve update information. Do you want to try updating from the default server?");
                        var ConfirmWindow =
                            new MsgBoxConfirm(
                                "Failed to retrieve update information. Do you want to try updating from the default server?");

                        if (ConfirmWindow.ShowDialog() == false)
                        {
                            if (ConfirmWindow.confirm)
                            {
                                settingsJson["updateServiceUrl"] = "http://167.114.156.21:80/honline/update.json";

                                if (!ProcessUpdateData())
                                {
                                    AppendDebugLine("Failed to connect to the default update server.",
                                        Color.FromRgb(255, 0, 0));
                                    btnAction.Content = "PLAY GAME";
                                    GridSkip.Visibility = Visibility.Hidden;
                                    isPlayEnabled = true;
                                    btnAction.IsEnabled = true;

                                    var MainWindow =
                                        new MsgBoxOk(
                                            "Failed to connect to the default update server, you can still play the game if your files are valid.");
                                    MainWindow.Show();
                                    MainWindow.Focus();
                                    var AlertWindow =
                                        new MsgBoxOk(
                                            "Failed to connect to the default update server, you can still play the game if your files are valid.");
                                    AlertWindow.Show();
                                    AlertWindow.Focus();
                                }
                                else
                                {
                                    confirm = true;
                                }
                            }
                            else
                            {
                                AppendDebugLine("Update server connection manually canceled.", Color.FromRgb(255, 0, 0));
                                btnAction.Content = "PLAY GAME";
                                GridSkip.Visibility = Visibility.Hidden;
                                isPlayEnabled = true;
                                btnAction.IsEnabled = true;

                                var MainWindow =
                                    new MsgBoxOk(
                                        "Update server connection manually canceled, you can still play the game if your files are valid.");
                                MainWindow.Show();
                                MainWindow.Focus();
                                var AlertWindow =
                                    new MsgBoxOk(
                                        "Update server connection manually canceled, you can still play the game if your files are valid.");
                                AlertWindow.Show();
                                AlertWindow.Focus();
                            }
                        }
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke((Action) delegate
                    {
                        AppendDebugLine("Failed to retrieve update information from the default update server.",
                            Color.FromRgb(255, 0, 0));
                        btnAction.Content = "PLAY";
                        isPlayEnabled = true;
                        btnAction.IsEnabled = true;

                        var MainWindow =
                            new MsgBoxOk(
                                "Could not connect to the default update server, you can still play the game if your files are valid.");
                        MainWindow.Show();
                        MainWindow.Focus();
                        var AlertWindow =
                            new MsgBoxOk(
                                "Could not connect to the default update server, you can still play the game if your files are valid.");
                        AlertWindow.Show();
                        AlertWindow.Focus();
                    });
                }

                if (!confirm)
                {
                    return;
                }
            }

            if (filesToDownload.Count <= 0)
            {
                AppendDebugLine("You have the latest version! (" + latestUpdateVersion + ")", Color.FromRgb(0, 255, 0));


                btnAction.Dispatcher.Invoke(
                    new Action(
                        () =>
                        {
                            btnAction.Content = "PLAY GAME";
                            GridSkip.Visibility = Visibility.Hidden;
                            isPlayEnabled = true;

                            var fade = (Storyboard) TryFindResource("fade");
                            fade.Stop(); // Start animation
                            btnAction.IsEnabled = true;
                        }));

                if (silentStart)
                {
                    btnAction_Click(new object(), new RoutedEventArgs());
                }
                return;
            }

            AppendDebugLine("An update is available. (" + latestUpdateVersion + ")", Color.FromRgb(255, 255, 0));

            btnAction.Dispatcher.Invoke(
                new Action(
                    () =>
                    {
                        btnAction.Content = "UPDATE";

                        var fade = (Storyboard) TryFindResource("fade");
                        fade.Stop(); // Stop
                        btnAction.IsEnabled = true;
                        GridSkip.Visibility = Visibility.Visible;
                    }));
            if (silentStart)
            {
                //MessageBox.Show("Sorry, you need to update before the game can be started silently.", "ElDewrito Launcher");
                var AlertWindow = new MsgBoxOk("Sorry, you need to update before the game can be started silently.");

                AlertWindow.Show();
                AlertWindow.Focus();
            }
        }

        private bool ProcessUpdateData()
        {
            try
            {
                var updateData = settingsJson["updateServiceUrl"].ToString().Replace("\"", "");
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
                        int.Parse(x.Value["releaseNo"].ToString().Replace("\"", "")) >
                        int.Parse(latestUpdate["releaseNo"].ToString().Replace("\"", "")))
                    {
                        latestUpdate = x.Value;
                        latestUpdateVersion = x.Key + "-" + latestUpdate["gitRevision"].ToString().Replace("\"", "");
                    }
                }

                if (latestUpdate == null)
                    return false;

                var patchFiles = new List<string>();
                foreach (var file in latestUpdate["patchFiles"])
                    // each file mentioned here must match original hash or have a file in the _dewbackup folder that does
                {
                    var fileName = (string) file;
                    var fileHash = (string) settingsJson["gameFiles"][fileName];
                    if (!fileHashes.ContainsKey(fileName) &&
                        !fileHashes.ContainsKey(Path.Combine("_dewbackup", fileName)))
                    {
                        AppendDebugLine("Original file data for file \"" + fileName + "\" not found.",
                            Color.FromRgb(255, 0, 0));
                        AppendDebugLine("Please redo your Halo Online installation with the original HO files.",
                            Color.FromRgb(255, 0, 0), false);
                        return false;
                    }

                    if (fileHashes.ContainsKey(fileName)) // we have the file
                    {
                        if (fileHashes[fileName] != fileHash &&
                            (!fileHashes.ContainsKey(Path.Combine("_dewbackup", fileName)) ||
                             fileHashes[Path.Combine("_dewbackup", fileName)] != fileHash))
                        {
                            AppendDebugLine(
                                "File \"" + fileName +
                                "\" was found but isn't original, and a valid backup of the original data wasn't found.",
                                Color.FromRgb(255, 0, 0));
                            AppendDebugLine("Please redo your Halo Online installation with the original HO files.",
                                Color.FromRgb(255, 0, 0), false);
                            return false;
                        }
                    }
                    else
                    {
                        // we don't have the file
                        if (!fileHashes.ContainsKey(fileName + ".orig") &&
                            (!fileHashes.ContainsKey(Path.Combine("_dewbackup", fileName)) ||
                             fileHashes[Path.Combine("_dewbackup", fileName)] != fileHash))
                        {
                            AppendDebugLine("Original file data for file \"" + fileName + "\" not found.",
                                Color.FromRgb(255, 0, 0));
                            AppendDebugLine("Please redo your Halo Online installation with the original HO files.",
                                Color.FromRgb(255, 0, 0), false);
                            return false;
                        }
                    }

                    patchFiles.Add(fileName);
                }

                IDictionary<string, JToken> files = (JObject) latestUpdate["files"];

                filesToDownload = new List<string>();
                foreach (var x in files)
                {
                    var keyName = x.Key;
                    if (!fileHashes.ContainsKey(keyName) && fileHashes.ContainsKey(keyName.Replace(@"\", @"/")))
                        // linux system maybe?
                        keyName = keyName.Replace(@"\", @"/");

                    if (!fileHashes.ContainsKey(keyName) || fileHashes[keyName] != x.Value.ToString().Replace("\"", ""))
                    {
                        AppendDebugLine("File \"" + keyName + "\" is missing or a newer version was found.",
                            Color.FromRgb(255, 0, 0));
                        var name = x.Key;
                        if (patchFiles.Contains(keyName))
                            name += ".bspatch";
                        filesToDownload.Add(name);
                    }
                }

                SetVersionLabel(latestUpdateVersion);

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

            IDictionary<string, JToken> files = (JObject) settingsJson["gameFiles"];

            foreach (var x in files)
            {
                var keyName = x.Key;
                if (!fileHashes.ContainsKey(keyName) && fileHashes.ContainsKey(keyName.Replace(@"\", @"/")))
                    // linux system maybe?
                    keyName = keyName.Replace(@"\", @"/");

                if (!fileHashes.ContainsKey(keyName))
                {
                    if (skipFileExtensions.Contains(Path.GetExtension(keyName)))
                        continue;

                    AppendDebugLine("Failed to find required game file \"" + x.Key + "\"", Color.FromRgb(255, 0, 0));
                    AppendDebugLine("Please redo your Halo Online installation with the original HO files.",
                        Color.FromRgb(255, 0, 0), false);
                    SetButtonText("Error", true);

                    var fade = (Storyboard) TryFindResource("fade");
                    fade.Stop(); // Stop animation
                    SetButtonText("Error", true);

                    return false;
                }

                if (fileHashes[keyName] != x.Value.ToString().Replace("\"", ""))
                {
                    if (skipFileExtensions.Contains(Path.GetExtension(keyName)) ||
                        skipFiles.Contains(Path.GetFileName(keyName)))
                        continue;

                    AppendDebugLine("Game file \"" + keyName + "\" data is invalid.", Color.FromRgb(255, 0, 0));
                    AppendDebugLine("Your hash: " + fileHashes[keyName], Color.FromRgb(255, 0, 0), false);
                    AppendDebugLine("Expected hash: " + x.Value.ToString().Replace("\"", ""), Color.FromRgb(255, 0, 0),
                        false);
                    AppendDebugLine("Please redo your Halo Online installation with the original HO files.",
                        Color.FromRgb(255, 0, 0), false);
                    return false;
                }
            }

            return true;
        }

        private void CreateHashJson()
        {
            if (fileHashes == null)
                HashFilesInFolder(BasePath);

            var builder = new StringBuilder();
            builder.AppendLine("{");
            foreach (var kvp in fileHashes)
            {
                builder.AppendLine(String.Format("    \"{0}\": \"{1}\",", kvp.Key.Replace(@"\", @"\\"), kvp.Value));
            }
            builder.AppendLine("}");
            var json = builder.ToString();

            AppendDebugLine(json, Color.FromRgb(255, 255, 0));
        }

        private void HashFilesInFolder(string basePath, string dirPath = "")
        {
            if (fileHashes == null)
                fileHashes = new Dictionary<string, string>();

            if (String.IsNullOrEmpty(dirPath))
            {
                dirPath = basePath;
                AppendDebugLine("Validating game files...", Color.FromRgb(255, 255, 255));
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
                catch
                {
                }
            }
        }

        private void AppendDebugLine(string status, Color color, bool updateLabel = true)
        {
            if (DebugLogger.Dispatcher.CheckAccess())
            {
                DebugLogger.Document.Blocks.Add(
                    new Paragraph(new Run(status) {Foreground = new SolidColorBrush(color)}));
            }
            else
            {
                DebugLogger.Dispatcher.Invoke(new Action(() => AppendDebugLine(status, color, updateLabel)));
            }
        }

        private void SetButtonText(string status, bool error, bool updateLabel = true)
        {
            if (btnAction.Dispatcher.CheckAccess())
            {
                btnAction.Content = status.ToUpper();

                if (error)
                {
                    btnAction.Foreground = Brushes.Gray;
                    SetVersionLabel("Error");
                }
            }
            else
            {
                btnAction.Dispatcher.Invoke(new Action(() => SetButtonText(status, true, updateLabel)));
            }
        }

        private void SetVersionLabel(string version)
        {
            if (lblVersion.Dispatcher.CheckAccess())
            {
                lblVersion.Text = version;
            }
            else
            {
                lblVersion.Dispatcher.Invoke(new Action(() => SetVersionLabel(version)));
            }
        }

        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            if (isPlayEnabled)
            {
                ProcessStartInfo sInfo = new ProcessStartInfo(BasePath + "/eldorado.exe");
                sInfo.Arguments = "-launcher";

                Process process = new Process();
                process.StartInfo = sInfo;

                if (!process.Start())
                {
                    SetVariable("Video.Window", "0", ref configFile);
                    SetVariable("Video.FullScreen", "1", ref configFile);
                    SetVariable("Video.VSync", "1", ref configFile);
                    SetVariable("Video.FPSCounter", "0", ref configFile);
                    SaveConfigFile("dewrito_prefs.cfg", configFile);

                    var AlertWindow = new MsgBoxOk("Your game crashed. Your launch settings have been reset please try again");

                    AlertWindow.Show();
                    AlertWindow.Focus();
                }

                if (configFile["Video.Window"] == "1")
                {
                    sInfo.Arguments += " -window";
                }
                if (configFile["Video.FullScreen"] == "1")
                {
                    sInfo.Arguments += " -fullscreen";
                }
                if (configFile["Video.VSync"] == "1")
                {
                    sInfo.Arguments += " -no_vsync";
                }
                if (configFile["Video.FPSCounter"] == "1")
                {
                    sInfo.Arguments += " -show_fps";
                }
            }
            else if (btnAction.Content.ToString() == "UPDATE")
            {
                foreach (var file in filesToDownload)
                {
                    AppendDebugLine("Downloading file \"" + file + "\"...", Color.FromRgb(255, 255, 0));
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
                        AppendDebugLine("Download for file \"" + file + "\" failed.", Color.FromRgb(255, 0, 0));
                        AppendDebugLine("Error: " + dialog.Error.Message, Color.FromRgb(255, 0, 0), false);
                        SetButtonText("Error", true);
                        if (dialog.Error.InnerException != null)
                            AppendDebugLine("Error: " + dialog.Error.InnerException.Message, Color.FromRgb(255, 0, 0),
                                false);
                        return;
                    }
                }

                if (filesToDownload.Contains("DewritoUpdater.exe"))
                {
                    //MessageBox.Show("Update complete! Please restart the launcher.", "ElDewrito Launcher");
                    //Application.Current.Shutdown();
                    var RestartWindow = new MsgBoxRestart("Update complete! Please restart the launcher.");

                    RestartWindow.Show();
                    RestartWindow.Focus();
                }

                btnAction.Content = "PLAY GAME";
                isPlayEnabled = true;
                GridSkip.Visibility = Visibility.Hidden;
                //imgAction.Source = new BitmapImage(new Uri(@"/Resourves/playEnabled.png", UriKind.Relative));
                AppendDebugLine("Update successful. You have the latest version! (" + latestUpdateVersion + ")",
                    Color.FromRgb(0, 255, 0));
            }
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            var r = new Random();
            var helmet = r.Next(0, 25);
            var chest = r.Next(0, 25);
            var shoulders = r.Next(0, 25);
            var arms = r.Next(0, 25);
            var legs = r.Next(0, 25);

            var randomColor = new Random();
            var primary = String.Format("#{0:X6}", randomColor.Next(0x1000000));
            var secondary = String.Format("#{0:X6}", randomColor.Next(0x1000000));
            var visor = String.Format("#{0:X6}", randomColor.Next(0x1000000));
            var lights = String.Format("#{0:X6}", randomColor.Next(0x1000000));
            var holo = String.Format("#{0:X6}", randomColor.Next(0x1000000));

            cmbHelmet.SelectedIndex = helmet;
            cmbChest.SelectedIndex = chest;
            cmbShoulders.SelectedIndex = shoulders;
            cmbArms.SelectedIndex = arms;
            cmbLegs.SelectedIndex = legs;

            clrPrimary.SelectedColor = (Color) ColorConverter.ConvertFromString(primary);
            clrSecondary.SelectedColor = (Color) ColorConverter.ConvertFromString(secondary);
            clrVisor.SelectedColor = (Color) ColorConverter.ConvertFromString(visor);
            clrLights.SelectedColor = (Color) ColorConverter.ConvertFromString(lights);
            clrHolo.SelectedColor = (Color) ColorConverter.ConvertFromString(holo);
            clrPrimary.SelectedColor = (Color) ColorConverter.ConvertFromString(primary);
            clrSecondary.SelectedColor = (Color) ColorConverter.ConvertFromString(secondary);
            clrVisor.SelectedColor = (Color) ColorConverter.ConvertFromString(visor);
            clrLights.SelectedColor = (Color) ColorConverter.ConvertFromString(lights);
            clrHolo.SelectedColor = (Color) ColorConverter.ConvertFromString(holo);

            SetVariable("Player.Armor.Chest", Convert.ToString(cmbChest.SelectedValue), ref configFile);
            SetVariable("Player.Armor.Shoulders", Convert.ToString(cmbShoulders.SelectedValue), ref configFile);
            SetVariable("Player.Armor.Helmet", Convert.ToString(cmbHelmet.SelectedValue), ref configFile);
            SetVariable("Player.Armor.Arms", Convert.ToString(cmbArms.SelectedValue), ref configFile);
            SetVariable("Player.Armor.Legs", Convert.ToString(cmbLegs.SelectedValue), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void Initial(bool Error)
        {
            var cfgFileExists = LoadConfigFile("dewrito_prefs.cfg", ref configFile);

            if (!cfgFileExists)
            {
                SetVariable("Game.MedalsZip", "halo3", ref configFile);
                SetVariable("Game.LanguageID", "0", ref configFile);
                SetVariable("Game.SkipLauncher", "0", ref configFile);
                //SetVariable("Game.BetaFiles", "0", ref configFile);
                //SetVariable("Game.Protocol", "0", ref configFile);
                SetVariable("Player.Armor.Accessory", "air_assault", ref configFile);
                SetVariable("Player.Armor.Arms", "air_assault", ref configFile);
                SetVariable("Player.Armor.Chest", "air_assault", ref configFile);
                SetVariable("Player.Armor.Helmet", "air_assault", ref configFile);
                SetVariable("Player.Armor.Legs", "air_assault", ref configFile);
                SetVariable("Player.Armor.Shoulders", "air_assault", ref configFile);
                SetVariable("Player.Colors.Primary", "#000000", ref configFile);
                SetVariable("Player.Colors.Secondary", "#000000", ref configFile);
                SetVariable("Player.Colors.Lights", "#000000", ref configFile);
                SetVariable("Player.Colors.Holo", "#000000", ref configFile);
                SetVariable("Player.Colors.Visor", "#000000", ref configFile);
                SetVariable("Player.Name", "Forgot", ref configFile);
                SetVariable("Player.UserID", "0", ref configFile);
                SetVariable("Server.Name", "HaloOnline Server", ref configFile);
                SetVariable("Server.Password", "", ref configFile);
                SetVariable("Server.Countdown", "5", ref configFile);
                SetVariable("Server.MaxPlayers", "16", ref configFile);
                SetVariable("Server.Port", "11775", ref configFile);
                SetVariable("Camera.Crosshair", "0", ref configFile);
                SetVariable("Camera.FOV", "90", ref configFile);
                SetVariable("Camera.HideHUD", "0", ref configFile);
                SetVariable("Input.RawInput", "1", ref configFile);
                SetVariable("Video.Height", Convert.ToString(Convert.ToInt32(SystemParameters.PrimaryScreenHeight)),
                    ref configFile);
                SetVariable("Video.Width", Convert.ToString(Convert.ToInt32(SystemParameters.PrimaryScreenWidth)),
                    ref configFile);
                SetVariable("Video.Window", "0", ref configFile);
                SetVariable("Video.FullScreen", "1", ref configFile);
                SetVariable("Video.VSync", "1", ref configFile);
                SetVariable("Video.FPSCounter", "0", ref configFile);
                SetVariable("Video.IntroSkip", "1", ref configFile);
                SetVariable("VoIP.PushToTalkKey", "capital", ref configFile);
                SetVariable("VoIP.VoiceActivationLevel", "-45", ref configFile);
                SetVariable("VoIP.VolumeModifier", "6", ref configFile);
                SetVariable("VoIP.PushToTalk", "1", ref configFile);
                SetVariable("VoIP.EchoCancellation", "1", ref configFile);
                SetVariable("VoIP.AGC", "1", ref configFile);
            }
            else if (Error)
            {
                SetVariable("Server.Name", "HaloOnline Server", ref configFile);
                SetVariable("Server.Password", "", ref configFile);
                SetVariable("Server.Countdown", "5", ref configFile);
                SetVariable("Server.MaxPlayers", "16", ref configFile);
                SetVariable("Server.Port", "11775", ref configFile);
                SetVariable("Camera.Crosshair", "0", ref configFile);
                SetVariable("Camera.FOV", "90", ref configFile);
                SetVariable("Camera.HideHUD", "0", ref configFile);
                SetVariable("Input.RawInput", "1", ref configFile);
                SetVariable("Video.Height", Convert.ToString(Convert.ToInt32(SystemParameters.PrimaryScreenHeight)),
                    ref configFile);
                SetVariable("Video.Width", Convert.ToString(Convert.ToInt32(SystemParameters.PrimaryScreenWidth)),
                    ref configFile);
                SetVariable("Video.Window", "0", ref configFile);
                SetVariable("Video.FullScreen", "1", ref configFile);
                SetVariable("Video.VSync", "1", ref configFile);
                SetVariable("Video.DX9Ex", "1", ref configFile);
                SetVariable("Video.FPSCounter", "0", ref configFile);
                SetVariable("Video.IntroSkip", "0", ref configFile);
                SetVariable("VoIP.PushToTalkKey", "capital", ref configFile);
                SetVariable("VoIP.VoiceActivationLevel", "-45", ref configFile);
                SetVariable("VoIP.VolumeModifier", "6", ref configFile);
                SetVariable("VoIP.PushToTalk", "1", ref configFile);
                SetVariable("VoIP.EchoCancellation", "1", ref configFile);
                SetVariable("VoIP.AGC", "1", ref configFile);
            }

            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void plrName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SetVariable("Player.Name", plrName.Text, ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void clrPrimary_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            var String = Convert.ToString(clrPrimary.SelectedColor).Remove(1, 2);
            SetVariable("Player.Colors.Primary", String, ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void clrSecondary_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            var String = Convert.ToString(clrSecondary.SelectedColor).Remove(1, 2);
            SetVariable("Player.Colors.Secondary", String, ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void clrLights_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            var String = Convert.ToString(clrLights.SelectedColor).Remove(1, 2);
            SetVariable("Player.Colors.Lights", String, ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void clrHolo_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            var String = Convert.ToString(clrHolo.SelectedColor).Remove(1, 2);
            SetVariable("Player.Colors.Holo", String, ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void clrVisor_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            var String = Convert.ToString(clrVisor.SelectedColor).Remove(1, 2);
            SetVariable("Player.Colors.Visor", String, ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void CmbHelmet_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetVariable("Player.Armor.Helmet", Convert.ToString(cmbHelmet.SelectedValue), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void CmbChest_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetVariable("Player.Armor.Chest", Convert.ToString(cmbChest.SelectedValue), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void CmbShoulders_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetVariable("Player.Armor.Shoulders", Convert.ToString(cmbShoulders.SelectedValue), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void CmbArms_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetVariable("Player.Armor.Arms", Convert.ToString(cmbArms.SelectedValue), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void CmbLegs_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetVariable("Player.Armor.Legs", Convert.ToString(cmbLegs.SelectedValue), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void chkCenter_Changed(object sender, RoutedEventArgs e)
        {
            SetVariable("Camera.Crosshair", Convert.ToString(Convert.ToInt32(chkCenter.IsChecked)), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void chkRaw_Changed(object sender, RoutedEventArgs e)
        {
            SetVariable("Input.RawInput", Convert.ToString(Convert.ToInt32(chkRaw.IsChecked)), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void chkWin_Changed(object sender, RoutedEventArgs e)
        {
            SetVariable("Video.Window", Convert.ToString(Convert.ToInt32(chkWin.IsChecked)), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void chkFull_Changed(object sender, RoutedEventArgs e)
        {
            SetVariable("Video.FullScreen", Convert.ToString(Convert.ToInt32(chkFull.IsChecked)), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void chkVSync_Changed(object sender, RoutedEventArgs e)
        {
            SetVariable("Video.VSync", Convert.ToString(Convert.ToInt32(chkVSync.IsChecked)), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        /*
        private void chkDX9Ex_Changed(object sender, RoutedEventArgs e)
        {
            SetVariable("Video.DX9Ex", Convert.ToString(Convert.ToInt32(chkDX9Ex.IsChecked)), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }
         */

        private void chkFPS_Changed(object sender, RoutedEventArgs e)
        {
            SetVariable("Video.FPSCounter", Convert.ToString(Convert.ToInt32(chkFPS.IsChecked)), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void chkIntro_Changed(object sender, RoutedEventArgs e)
        {
            SetVariable("Video.IntroSkip", Convert.ToString(Convert.ToInt32(chkIntro.IsChecked)), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);

            if (configFile["Video.IntroSkip"] == "1" && Directory.Exists("bink"))
            {
                Directory.Move("bink", "bink_disabled");
            }
            else if (configFile["Video.IntroSkip"] == "0" && Directory.Exists("bink_disabled"))
            {
                Directory.Move("bink_disabled", "bink");
            }
        }

        private void btnApply2_Click(object sender, EventArgs e)
        {
            switchPanel("main", false);
        }

        private void sldMax_LostMouseCapture(object sender, MouseEventArgs e)
        {
            SetVariable("Server.MaxPlayers", Convert.ToString(sldMax.Value), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void sldFov_LostMouseCapture(object sender, MouseEventArgs e)
        {
            SetVariable("Camera.FOV", Convert.ToString(sldFov.Value), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void txtSld_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetVariable("Camera.FOV", Convert.ToString(sldFov.Value), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void sldTimer_LostMouseCapture(object sender, MouseEventArgs e)
        {
            SetVariable("Server.Countdown", Convert.ToString(sldTimer.Value), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            //Customization
            clrPrimary.SelectedColor = (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Primary"]);
            clrSecondary.SelectedColor =
                (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Secondary"]);
            clrLights.SelectedColor = (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Lights"]);
            clrHolo.SelectedColor = (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Holo"]);
            clrVisor.SelectedColor = (Color)ColorConverter.ConvertFromString(configFile["Player.Colors.Visor"]);
            cmbLegs.SelectedValue = configFile["Player.Armor.Legs"];
            cmbArms.SelectedValue = configFile["Player.Armor.Arms"];
            cmbHelmet.SelectedValue = configFile["Player.Armor.Helmet"];
            cmbChest.SelectedValue = configFile["Player.Armor.Chest"];
            cmbShoulders.SelectedValue = configFile["Player.Armor.Shoulders"];
            plrName.Text = configFile["Player.Name"];
            SaveConfigFile("dewrito_prefs.cfg", configFile);
            switchPanel("main", false);
        }

        private void D_click(object sender, EventArgs e)
        {
            var sInfo = new ProcessStartInfo("https://halo.click/8Znpho");
            Process.Start(sInfo);
        }

        /*
        private void sldFov_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetVariable("Camera.FOV", Convert.ToString(sldFov.Value), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        
        private void sldTimer_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetVariable("Server.Countdown", Convert.ToString(sldTimer), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }
        */


        private static bool LoadConfigFile(string cfgFileName, ref Dictionary<string, string> returnDict)
        {
            returnDict = new Dictionary<string, string>();
            if (!File.Exists(cfgFileName))
                return false;

            var lines = File.ReadAllLines(cfgFileName);
            foreach (var line in lines)
            {
                var splitIdx = line.IndexOf(" ");
                if (splitIdx < 0 || splitIdx + 1 >= line.Length)
                    continue; // line isn't valid?
                var varName = line.Substring(0, splitIdx);
                var varValue = line.Substring(splitIdx + 1);

                // remove quotes from variable values
                if (varValue.StartsWith("\""))
                    varValue = varValue.Substring(1);
                if (varValue.EndsWith("\""))
                    varValue = varValue.Substring(0, varValue.Length - 1);

                SetVariable(varName, varValue, ref returnDict);
            }
            return true;
        }

        private static void SetVariable(string varName, string varValue, ref Dictionary<string, string> configDict)
        {
            if (configDict.ContainsKey(varName))
                configDict[varName] = varValue;
            else
                configDict.Add(varName, varValue);
        }

        private static bool CheckIfProcessIsRunning(string nameSubstring)
        {
            return Process.GetProcesses().Any(p => p.ProcessName.Contains(nameSubstring));
        }

        private static bool SaveConfigFile(string cfgFileName, Dictionary<string, string> configDict)
        {
            try
            {
                if (File.Exists(cfgFileName))
                    File.Delete(cfgFileName);

                var lines = new List<string>();
                foreach (var kvp in configDict)
                    lines.Add(kvp.Key + " \"" + kvp.Value + "\"");

                File.WriteAllLines(cfgFileName, lines.ToArray());


                var running = CheckIfProcessIsRunning("eldorado");
                if (running)
                {
                    dewCmd("Execute dewrito_prefs.cfg");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            sldFov.Value = 90;
            chkCenter.IsChecked = false;
            chkRaw.IsChecked = true;
            //chkBeta.IsChecked = false;
            sldTimer.Value = 5;
            sldMax.Value = 16;
            chkWin.IsChecked = false;
            chkFull.IsChecked = true;
            chkVSync.IsChecked = false;
            //chkDX9Ex.IsChecked = true;
            chkFPS.IsChecked = false;
            chkIntro.IsChecked = false;
        }

        private void BtnSkip_OnClick(object sender, RoutedEventArgs e)
        {
            btnAction.Content = "PLAY GAME";

            isPlayEnabled = true;

            var fade = (Storyboard)TryFindResource("fade");
            fade.Stop(); // Start animation
            btnAction.IsEnabled = true;
            GridSkip.Visibility = Visibility.Hidden;
        }

        private void VoipKey_OnKeyDown(object sender, KeyEventArgs e)
        {

            var keyPressed = KeyInterop.VirtualKeyFromKey(e.Key);
            //Console.WriteLine(keyPressed);
            voipKey.Text = Convert.ToString(e.Key);

            var hex = keyPressed.ToString("X4");
            var myValue = doritoKey.FirstOrDefault(x => x.Key == keyPressed).Value;
            //Console.WriteLine(myValue);

            SetVariable("VoIP.PushToTalkKey", myValue, ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void sldAudio_LostMouseCapture(object sender, MouseEventArgs e)
        {
            SetVariable("VoIP.VoiceActivationLevel", Convert.ToString(sldAudio.Value), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void chkPTT_Changed(object sender, RoutedEventArgs e)
        {
            SetVariable("VoIP.PushToTalk", Convert.ToString(Convert.ToInt32(chkPTT.IsChecked)), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void chkEC_Changed(object sender, RoutedEventArgs e)
        {
            SetVariable("VoIP.EchoCancellation", Convert.ToString(Convert.ToInt32(chkEC.IsChecked)), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void chkAGC_Changed(object sender, RoutedEventArgs e)
        {
            SetVariable("VoIP.AGC", Convert.ToString(Convert.ToInt32(chkAGC.IsChecked)), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void sldModifier_LostMouseCapture(object sender, MouseEventArgs e)
        {
            SetVariable("VoIP.VolumeModifier", Convert.ToString(sldModifier.Value), ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void BtnReset2_OnClick(object sender, RoutedEventArgs e)
        {
            voipKey.Text = Convert.ToString(KeyInterop.KeyFromVirtualKey(Convert.ToInt32(20)));
            sldAudio.Value = -45;
            sldModifier.Value = 6;
            chkPTT.IsChecked = true;
            chkEC.IsChecked = true;
            chkAGC.IsChecked = true;

            SetVariable("VoIP.PushToTalkKey", "capital", ref configFile);
            SetVariable("VoIP.VoiceActivationLevel", "-45", ref configFile);
            SetVariable("VoIP.VolumeModifier", "6", ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void BtnReddit_OnClick(object sender, RoutedEventArgs e)
        {
            var sInfo = new ProcessStartInfo("https://www.reddit.com/r/HaloOnline/");
            Process.Start(sInfo);
        }

        private void BtnGithub_OnClick(object sender, RoutedEventArgs e)
        {
            var sInfo = new ProcessStartInfo("https://github.com/FishPhd/DewritoUpdater");
            Process.Start(sInfo);
        }

        private void BtnTwitter_OnClick(object sender, RoutedEventArgs e)
        {
            var sInfo = new ProcessStartInfo("https://twitter.com/FishPhdOfficial");
            Process.Start(sInfo);
        }

        private void LblServerName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SetVariable("Server.Name", lblServerName.Text, ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void LblServerPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            SetVariable("Server.Password", lblServerPassword.Password, ref configFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}