using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Dewritwo
{
  internal static class Cfg
  {
    #region Variables

    public static Dictionary<string, string> ConfigFile;
    public static Dictionary<string, string> LauncherConfigFile;

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

    public static bool SaveConfigFile(string cfgFileName, Dictionary<string, string> configDict)
    {
      try
      {
        var lines = configDict.Select(kvp => kvp.Key + " \"" + kvp.Value + "\"").ToList();
        
        /*
        if (File.Exists(cfgFileName))
          File.Delete(cfgFileName);
        */

        if (File.Exists(cfgFileName))
        {
          File.WriteAllLines(cfgFileName + ".temp", lines.ToArray());
          File.Replace(cfgFileName + ".temp", cfgFileName, cfgFileName + ".bak");
        }else{
          File.WriteAllLines(cfgFileName, lines.ToArray());
        }

        if (CheckIfProcessIsRunning("eldorado"))
        {
          DewCmd("Execute dewrito_prefs.cfg");
        }
        return true;
      }
      catch
      {
        return false;
      }
    }

    private static string DewCmd(string cmd)
    {
      var data = new byte[1024];
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
      var stringData = Encoding.ASCII.GetString(data, 0, recv);

      ns.Write(Encoding.ASCII.GetBytes(cmd), 0, cmd.Length);
      ns.Flush();

      ns.Close();
      server.Close();
      return stringData;
    }

    private static bool LoadConfigFile(string CfgFileName, ref Dictionary<string, string> returnDict)
    {
      returnDict = new Dictionary<string, string>();
      if (!File.Exists(CfgFileName))
        return false;

      var lines = File.ReadAllLines(CfgFileName);
      foreach (var line in lines)
      {
        var splitIdx = line.IndexOf(" ", StringComparison.Ordinal);
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

    public static void Initial(string error)
    {
      var cfgFileExists = LoadConfigFile("dewrito_prefs.cfg", ref ConfigFile);
      var launcherCfgFileExists = LoadConfigFile("launcher_prefs.cfg", ref LauncherConfigFile);

      if (!cfgFileExists || error == "Cfg Error")
      {
        SetVariable("Game.MenuURL", "http://scooterpsu.github.io/", ref ConfigFile);
        SetVariable("Game.LanguageID", "0", ref ConfigFile);
        SetVariable("Game.SkipLauncher", "0", ref ConfigFile);
        SetVariable("Game.LogName", "dorito.log", ref ConfigFile);
        SetVariable("Player.Armor.Accessory", "air_assault", ref ConfigFile);
        SetVariable("Player.Armor.Arms", "air_assault", ref ConfigFile);
        SetVariable("Player.Armor.Chest", "air_assault", ref ConfigFile);
        SetVariable("Player.Armor.Helmet", "air_assault", ref ConfigFile);
        SetVariable("Player.Armor.Legs", "air_assault", ref ConfigFile);
        SetVariable("Player.Armor.Pelvis", "", ref ConfigFile);
        SetVariable("Player.Armor.Shoulders", "air_assault", ref ConfigFile);
        SetVariable("Player.Colors.Primary", "#698029", ref ConfigFile);
        SetVariable("Player.Colors.Secondary", "#698029", ref ConfigFile);
        SetVariable("Player.Colors.Visor", "#FFA000", ref ConfigFile);
        SetVariable("Player.Colors.Lights", "#000000", ref ConfigFile);
        SetVariable("Player.Colors.Holo", "#000000", ref ConfigFile);
        SetVariable("Player.Name", "", ref ConfigFile);
        SetVariable("Player.PrivKeyNote",
          "The PrivKey below is used to keep your stats safe.Treat it like a password and don't share it with anyone!",
          ref ConfigFile);
        SetVariable("Player.PrivKey", "", ref ConfigFile);
        SetVariable("Player.PubKey", "", ref ConfigFile);
        SetVariable("Server.Name", "Halo Online Server", ref ConfigFile);
        SetVariable("Server.Password", "", ref ConfigFile);
        SetVariable("Server.Countdown", "5", ref ConfigFile);
        SetVariable("Server.MaxPlayers", "16", ref ConfigFile);
        SetVariable("Server.Port", "11775", ref ConfigFile);
        SetVariable("Server.ShouldAnnounce", "1", ref ConfigFile);
        SetVariable("Server.SprintEnabled", "0", ref ConfigFile);
        SetVariable("Server.UnlimitedSprint", "0", ref ConfigFile);
        SetVariable("Camera.Crosshair", "0", ref ConfigFile);
        SetVariable("Camera.FOV", "90.000000", ref ConfigFile);
        SetVariable("Camera.HideHUD", "0", ref ConfigFile);
        SetVariable("Camera.Speed", "0.100000", ref ConfigFile);
        SetVariable("Input.RawInput", "1", ref ConfigFile);
        SetVariable("IRC.Server", "irc.snoonet.org", ref ConfigFile);
        SetVariable("IRC.ServerPort", "6667", ref ConfigFile);
        SetVariable("IRC.GlobalChannel", "#haloonline", ref ConfigFile);
        SetVariable("VoIP.PushToTalkKey", "capital", ref ConfigFile);
        SetVariable("VoIP.PushToTalk", "1", ref ConfigFile);
        SetVariable("VoIP.VolumeModifier", "6", ref ConfigFile);
        SetVariable("VoIP.AGC", "1", ref ConfigFile);
        SetVariable("VoIP.EchoCancellation", "1", ref ConfigFile);
        SetVariable("VoIP.VoiceActivationLevel", "-45.000000", ref ConfigFile);
        SetVariable("VoIP.ServerEnabled", "1", ref ConfigFile);
        SetVariable("VoIP.Enabled", "1", ref ConfigFile);
        SetVariable("Graphics.Saturation", "1.000000", ref ConfigFile);
        SetVariable("Graphics.Bloom", "0.000000", ref ConfigFile);
        Console.WriteLine("New CFG Created");
      }

      if (!launcherCfgFileExists || error == "launcher")
      {
        SetVariable("Launcher.Color", "blue", ref LauncherConfigFile);
        SetVariable("Launcher.Theme", "BaseDark", ref LauncherConfigFile);
        SetVariable("Launcher.Close", "0", ref LauncherConfigFile);
        SetVariable("Launcher.Random", "0", ref LauncherConfigFile);
        SetVariable("Launcher.IntroSkip", "1", ref LauncherConfigFile);
        SetVariable("Launcher.AutoDebug", "0", ref LauncherConfigFile);
        SetVariable("Launcher.PlayerMessage", "0", ref LauncherConfigFile);
        Console.WriteLine("New Launcher CFG Created");
      }

      SaveConfigFile("launcher_prefs.cfg", LauncherConfigFile);
      SaveConfigFile("dewrito_prefs.cfg", ConfigFile);
    }

    #endregion
  }
}