using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Dewritwo.Resources
{
    internal class Cfg
    {
        #region Variables

        public static Dictionary<string, string> configFile;
        public static Dictionary<string, string> launcherConfigFile;

        #endregion

        #region cfg Loading and Saving

        public static void SetVariable(string varName, string varValue, ref Dictionary<string, string> configDict)
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

        public static bool SaveConfigFile(string CfgFileName, Dictionary<string, string> configDict)
        {
            try
            {
                if (File.Exists(CfgFileName))
                    File.Delete(CfgFileName);

                var lines = new List<string>();
                foreach (var kvp in configDict)
                    lines.Add(kvp.Key + " \"" + kvp.Value + "\"");

                File.WriteAllLines(CfgFileName, lines.ToArray());


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
                return "Is Eldorito Running?";
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

        private static bool LoadConfigFile(string CfgFileName, ref Dictionary<string, string> returnDict)
        {
            returnDict = new Dictionary<string, string>();
            if (!File.Exists(CfgFileName))
                return false;

            var lines = File.ReadAllLines(CfgFileName);
            foreach (var line in lines)
            {
                var splitIdx = line.IndexOf(" ");
                if (splitIdx < 0 || splitIdx + 1 >= line.Length)
                    continue; // line isn't valid?
                var varName = line.Substring(0, splitIdx);
                var varValue = line.Substring(splitIdx + 1);

                // remove quotes
                if (varValue.StartsWith("\""))
                    varValue = varValue.Substring(1);
                if (varValue.EndsWith("\""))
                    varValue = varValue.Substring(0, varValue.Length - 1);

                SetVariable(varName, varValue, ref returnDict);
            }
            return true;
        }

        public static void Initial(bool error)
        {
            var CfgFileExists = LoadConfigFile("dewrito_prefs.cfg", ref configFile);
            var LauncherCfgFileExists = LoadConfigFile("launcher_prefs.cfg", ref launcherConfigFile);

            if (!CfgFileExists)
            {
                SetVariable("Launcher.Color", "blue", ref configFile);
                SetVariable("Launcher.Theme", "BaseDark", ref configFile);
                SetVariable("Launcher.Close", "0", ref configFile);
                SetVariable("Launcher.Random", "0", ref configFile);
                SetVariable("Game.MedalsZip", "halo3", ref configFile);
                SetVariable("Game.LanguageID", "0", ref configFile);
                SetVariable("Game.SkipLauncher", "0", ref configFile);
                SetVariable("Player.Name", "", ref configFile);
                SetVariable("Player.Armor.Helmet", "air_assault", ref configFile);
                SetVariable("Player.Armor.Chest", "air_assault", ref configFile);
                SetVariable("Player.Armor.Shoulders", "air_assault", ref configFile);
                SetVariable("Player.Armor.Arms", "air_assault", ref configFile);
                SetVariable("Player.Armor.Legs", "air_assault", ref configFile);
                SetVariable("Player.Armor.Accessory", "air_assault", ref configFile);
                SetVariable("Player.Colors.Primary", "#000000", ref configFile);
                SetVariable("Player.Colors.Secondary", "#000000", ref configFile);
                SetVariable("Player.Colors.Lights", "#000000", ref configFile);
                SetVariable("Player.Colors.Holo", "#000000", ref configFile);
                SetVariable("Player.Colors.Visor", "#000000", ref configFile);
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
                Console.WriteLine("New CFG Created");
            }
            else if (error)
            {
                SetVariable("Video.IntroSkip", "1", ref configFile);
            }

            if (!LauncherCfgFileExists)
            {
                SetVariable("Launcher.Color", "blue", ref launcherConfigFile);
                SetVariable("Launcher.Theme", "BaseDark", ref launcherConfigFile);
                SetVariable("Launcher.Close", "0", ref launcherConfigFile);
                SetVariable("Launcher.Random", "0", ref launcherConfigFile);
                SetVariable("Launcher.IntroSkip", "1", ref launcherConfigFile);
                Console.WriteLine("Launcher.Added");
            }

            SaveConfigFile("launcher_prefs.cfg", launcherConfigFile);
            SaveConfigFile("dewrito_prefs.cfg", configFile);
        }

        #endregion
    }
}