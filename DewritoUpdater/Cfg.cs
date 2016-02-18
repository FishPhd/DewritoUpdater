using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Dewritwo
{
  internal static class Cfg
  {
    public static Dictionary<string, string> ConfigFile = new Dictionary<string, string>();
    public static Dictionary<string, string> LauncherConfigFile = new Dictionary<string, string>();

    public static void SetVariable(string varName, string varValue, ref Dictionary<string, string> configDict,
      bool rcon = true)
    {
      if (configDict.ContainsKey(varName))
        configDict[varName] = varValue;
      else
        configDict.Add(varName, varValue);

      if (CheckIfProcessIsRunning("eldorado") && rcon)
        AsyncRcon(varName, varValue);
    }

    public static bool CheckIfProcessIsRunning(string nameSubstring)
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
        }
        else
        {
          File.WriteAllLines(cfgFileName, lines.ToArray());
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
        return "Is ElDorito Running?";
      }

      var ns = server.GetStream();

      var recv = ns.Read(data, 0, data.Length);
      var stringData = Encoding.ASCII.GetString(data, 0, recv);

      ns.Write(Encoding.ASCII.GetBytes(cmd), 0, cmd.Length);

      /*
      Console.WriteLine(cmd);
      Console.WriteLine(stringData);
      */

      ns.Flush();

      ns.Close();
      server.Close();

      return stringData;
    }

    private static async void AsyncRcon(string varName, string varValue)
    {
      await Task.Run(() => DewCmd(varName + ' ' + varValue));
    }

    private static bool LoadConfigFile(string cfgFileName, ref Dictionary<string, string> returnDict)
    {
      if (returnDict == null) throw new ArgumentNullException(nameof(returnDict));

      if (!File.Exists(cfgFileName))
        return false;

      var lines = File.ReadAllLines(cfgFileName);
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

      if (!cfgFileExists || error == "cfg")
      {
        SetVariable("Game.MenuURL", "http://scooterpsu.github.io/", ref ConfigFile, false);
        SetVariable("Game.LanguageID", "0", ref ConfigFile, false);
        SetVariable("Game.SkipLauncher", "0", ref ConfigFile, false);
        SetVariable("Game.LogName", "dorito.log", ref ConfigFile, false);
        SetVariable("Player.Armor.Accessory", "air_assault", ref ConfigFile, false);
        SetVariable("Player.Armor.Arms", "air_assault", ref ConfigFile, false);
        SetVariable("Player.Armor.Chest", "air_assault", ref ConfigFile, false);
        SetVariable("Player.Armor.Helmet", "air_assault", ref ConfigFile, false);
        SetVariable("Player.Armor.Legs", "air_assault", ref ConfigFile, false);
        SetVariable("Player.Armor.Pelvis", "", ref ConfigFile, false);
        SetVariable("Player.Armor.Shoulders", "air_assault", ref ConfigFile, false);
        SetVariable("Player.Colors.Primary", "#698029", ref ConfigFile, false);
        SetVariable("Player.Colors.Secondary", "#698029", ref ConfigFile, false);
        SetVariable("Player.Colors.Visor", "#FFA000", ref ConfigFile, false);
        SetVariable("Player.Colors.Lights", "#000000", ref ConfigFile, false);
        SetVariable("Player.Colors.Holo", "#000000", ref ConfigFile, false);
        SetVariable("Player.Name", "", ref ConfigFile, false);
        SetVariable("Player.PrivKeyNote",
          "The PrivKey below is used to keep your stats safe.Treat it like a password and don't share it with anyone!",
          ref ConfigFile, false);
        SetVariable("Player.PrivKey", "", ref ConfigFile, false);
        SetVariable("Player.PubKey", "", ref ConfigFile, false);
        SetVariable("Server.Name", "Halo Online Server", ref ConfigFile, false);
        SetVariable("Server.Password", "", ref ConfigFile, false);
        SetVariable("Server.Countdown", "5", ref ConfigFile, false);
        SetVariable("Server.MaxPlayers", "16", ref ConfigFile, false);
        SetVariable("Server.Port", "11775", ref ConfigFile, false);
        SetVariable("Server.ShouldAnnounce", "1", ref ConfigFile, false);
        SetVariable("Server.SprintEnabled", "0", ref ConfigFile, false);
        SetVariable("Server.AssassinationEnabled", "0", ref ConfigFile, false);
        SetVariable("Server.UnlimitedSprint", "0", ref ConfigFile, false);
        SetVariable("Camera.Crosshair", "0", ref ConfigFile, false);
        SetVariable("Camera.FOV", "90.000000", ref ConfigFile, false);
        SetVariable("Camera.HideHUD", "0", ref ConfigFile, false);
        SetVariable("Camera.Speed", "0.100000", ref ConfigFile, false);
        SetVariable("Input.RawInput", "1", ref ConfigFile, false);
        SetVariable("VoIP.PushToTalkKey", "capital", ref ConfigFile, false);
        SetVariable("VoIP.PushToTalk", "1", ref ConfigFile, false);
        SetVariable("VoIP.VolumeModifier", "6", ref ConfigFile, false);
        SetVariable("VoIP.AGC", "1", ref ConfigFile, false);
        SetVariable("VoIP.EchoCancellation", "1", ref ConfigFile, false);
        SetVariable("VoIP.VoiceActivationLevel", "-45.000000", ref ConfigFile, false);
        SetVariable("VoIP.ServerEnabled", "1", ref ConfigFile, false);
        SetVariable("VoIP.Enabled", "1", ref ConfigFile, false);
        SetVariable("Graphics.Saturation", "1.000000", ref ConfigFile, false);
        SetVariable("Graphics.Bloom", "0.000000", ref ConfigFile, false);
        //Launcher settings (moved to avoid the try catch in initial)
        SetVariable("Launcher.Color", "blue", ref LauncherConfigFile, false);
        SetVariable("Launcher.Theme", "BaseDark", ref LauncherConfigFile, false);
        SetVariable("Launcher.Close", "0", ref LauncherConfigFile, false);
        SetVariable("Launcher.Random", "0", ref LauncherConfigFile, false);
        SetVariable("Launcher.IntroSkip", "1", ref LauncherConfigFile, false);
        SetVariable("Launcher.AutoDebug", "0", ref LauncherConfigFile, false);
        SetVariable("Launcher.PlayerMessage", "0", ref LauncherConfigFile, false);
      }

      if (!launcherCfgFileExists)
      {
        SetVariable("Launcher.Color", "blue", ref LauncherConfigFile, false);
        SetVariable("Launcher.Theme", "BaseDark", ref LauncherConfigFile, false);
        SetVariable("Launcher.Close", "0", ref LauncherConfigFile, false);
        SetVariable("Launcher.Random", "0", ref LauncherConfigFile, false);
        SetVariable("Launcher.IntroSkip", "1", ref LauncherConfigFile, false);
        SetVariable("Launcher.AutoDebug", "0", ref LauncherConfigFile, false);
        SetVariable("Launcher.PlayerMessage", "0", ref LauncherConfigFile, false);
      }

      if (!SaveConfigFile("launcher_prefs.cfg", LauncherConfigFile))
        Console.WriteLine(@"Failed to save dew_prefs.cfg");

      if (!SaveConfigFile("launcher_prefs.cfg", LauncherConfigFile))
        Console.WriteLine(@"Failed to save launcher_prefs.cfg");
    }
  }
}