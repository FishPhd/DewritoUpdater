using System.Collections.Generic;

namespace Dewritwo
{
  internal class Dictionaries
  {
    public static Dictionary<string, string> GetArmor() =>
      new Dictionary<string, string>
      {
        ["Air Assault"] = "air_assault",
        ["Stealth"] = "stealth",
        ["Renegade"] = "renegade",
        ["Nihard"] = "nihard",
        ["Gladiator"] = "gladiator",
        ["Mac"] = "mac",
        ["Shark"] = "shark",
        ["Juggernaut"] = "juggernaut",
        ["Dutch"] = "dutch",
        ["Chameleon"] = "chameleon",
        ["Halberd"] = "halberd",
        ["Cyclops"] = "cyclops",
        ["Scanner"] = "scanner",
        ["Mercenary"] = "mercenary",
        ["Hoplite"] = "hoplite",
        ["Ballista"] = "ballista",
        ["Strider"] = "strider",
        ["Demo"] = "demo",
        ["Orbital"] = "orbital",
        ["Spectrum"] = "spectrum",
        ["Gungnir"] = "gungnir",
        ["Hammerhead"] = "hammerhead",
        ["Omni"] = "omni",
        ["Oracle"] = "oracle",
        ["Silverback"] = "silverback",
        ["Widow Maker"] = "widow_maker"
      };

    public static Dictionary<string, string> GetColor() =>
      new Dictionary<string, string>
      {
        ["Blue"] = "blue",
        ["Red"] = "red",
        ["Green"] = "green",
        ["Purple"] = "purple",
        ["Orange"] = "orange",
        ["Lime"] = "lime",
        ["Emerald"] = "emerald",
        ["Teal"] = "teal",
        ["Cyan"] = "cyan",
        ["Cobalt"] = "cobalt",
        ["Indigo"] = "indigo",
        ["Violet"] = "violet",
        ["Pink"] = "pink",
        ["Magenta"] = "magenta",
        ["Crimson"] = "crimson",
        ["Amber"] = "amber",
        ["Yellow"] = "yellow",
        ["Brown"] = "brown",
        ["Olive"] = "olive",
        ["Steel"] = "steel",
        ["Mauve"] = "mauve",
        ["Taupe"] = "taupe",
        ["Sienna"] = "sienna"
      };

    public static Dictionary<string, string> GetWeapons() =>
      new Dictionary<string, string>
      {
        ["Assault Rifle"] = "assault_rifle",
        ["DMG Assault Rifle"] = "ar_variant_2",
        ["ROF Assault Rifle"] = "ar_variant_3",
        ["ACC Assault Rifle"] = "ar_variant_5",
        ["PWR Assault Rifle"] = "ar_variant_6",
        ["Battle Rifle"] = "battle_rifle",
        ["ROF Battle Rifle"] = "br_variant_1",
        ["ACC Battle Rifle"] = "br_variant_2",
        ["MAG Battle Rifle"] = "br_variant_3",
        ["DMG Battle Rifle"] = "br_variant_4",
        ["RNG Battle Rifle"] = "br_variant_5",
        ["PWR Battle Rifle"] = "br_variant_6",
        ["Covenant Carbine"] = "covenant_carbine",
        ["MAG Covenant Carbine"] = "covenant_carbine_variant_1",
        ["DMG Covenant Carbine"] = "covenant_carbine_variant_2",
        ["ACC Covenant Carbine"] = "covenant_carbine_variant_3",
        ["ROF Covenant Carbine"] = "covenant_carbine_variant_4",
        ["RNG Covenant Carbine"] = "covenant_carbine_variant_5",
        ["PWR Covenant Carbine"] = "covenant_carbine_variant_6",
        ["DMR "] = "dmr",
        ["MAG DMR"] = "dmr_variant_1",
        ["ACC DMR"] = "dmr_variant_2",
        ["ROF DMR"] = "dmr_variant_3",
        ["DMG DMR"] = "dmr_variant_4",
        ["RNG DMR"] = "dmr_variant_5",
        ["PWR DMR"] = "dmr_variant_6",
        ["Plasma Rifle"] = "plasma_rifle_variant_6",
        ["PWR Plasma Rifle"] = "plasma_rifle_variant_6",
        ["SMG"] = "smg",
        ["ROF SMG"] = "smg_variant_1",
        ["ACC SMG"] = "smg_variant_2",
        ["DMG SMG"] = "smg_variant_4",
        ["PWR SMG"] = "smg_variant_6"
      };

    public static Dictionary<string, string> GetAction() =>
      new Dictionary<string, string>
      {
        ["Bind"] = "bind",
        ["Commands"] = "command"
      };

    public static Dictionary<string, string> GetCommand() =>
      new Dictionary<string, string>
      {
        // Order does matter and command must match commands
        ["Announce Server"] = "Server.Announce",
        ["Announce Stats"] = "Server.AnnounceStats",
        ["Automatically Announce"] = "Server.ShouldAnnounce",
        ["Bloom Value"] = "Graphics.Bloom",
        ["Blue Hue Value"] = "Graphics.BlueHue",
        ["Camera Mode"] = "Camera.Mode",
        ["Camera Speed"] = "Camera.Speed",
        ["Cinematic Letterbox"] = "Graphics.Letterbox",
        ["Connect to a Server"] = "Server.Connect",
        ["Debug Log File Name"] = "Game.LogName",
        ["Debug Log Mode"] = "Game.LogMode",
        ["Depth of Field Value"] = "Graphics.DepthOfField",
        ["Execute"] = "Execute",
        ["Exit Game"] = "Game.Exit",
        ["Filter Debug Log"] = "Game.LogFilter",
        ["Force Load Map"] = "Game.ForceLoad",
        ["Forge (Delete)"] = "Game.DeleteForgeItem",
        ["Game Info"] = "Game.Info",
        ["Game Speed"] = "Time.GameSpeed",
        ["Global Chat Channel"] = "IRC.GlobalChannel",
        ["Global Chat Port"] = "IRC.ServerPort",
        ["Global Chat Server"] = "IRC.Server",
        ["Green Hue Value"] = "Graphics.GreenHue",
        ["HTML Menu Open"] = "Game.SetMenuEnabled",
        ["HTML Menu URL"] = "Game.MenuURL",
        ["HTTP Server Port"] = "Server.Port",
        ["Help"] = "Help",
        ["Hide HUD"] = "Camera.HideHUD",
        ["Kick Player (Host Only)"] = "Server.KickPlayer",
        ["Language"] = "Game.LanguageID",
        ["List Players (Host Only)"] = "Server.ListPlayers",
        ["Load Forge/Map"] = "Game.Map",
        ["Load GameType"] = "Game.GameType",
        ["Open Specific UI"] = "Game.ShowUI",
        ["Print Player UID"] = "Player.PrintUID",
        ["Print Private Stats Key"] = "Player.PrivKey",
        ["Print Public Stats Key"] = "Player.PubKey",
        ["Push to Talk Key"] = "+VoIP.Talk",
        ["Red Hue Value"] = "Graphics.RedHue",
        ["Saturation Value"] = "Graphics.Saturation",
        ["Show Game Version"] = "Game.Version",
        ["Start/Restart Game"] = "Game.Start",
        ["Stop/Returns to Lobby"] = "Game.Stop",
        ["Write Config File"] = "WriteConfig"
      };

    public static Dictionary<string, string> GetCommandLine() =>
      new Dictionary<string, string>
      {
        //Order doesn't matter as long as command is correct
        ["Server.Announce"] = "Announces Your Server to the Master Servers [No Value]",
        ["Server.AnnounceStats"] = "Announces Stats to Servers at End of Game [No Value]",
        ["Server.ShouldAnnounce"] = "Server Should Be Announced On Lobby Creation [0 or 1]",
        ["Graphics.Bloom"] = "Atmosphere Bloom Value [Value]",
        ["Graphics.BlueHue"] = "Blue Hue Value [Value]",
        ["Camera.Mode"] = "Camera Mode [default/first/flying/static/spectate]",
        ["Camera.Speed"] = "Camera Speed [Value]",
        ["Graphics.Letterbox"] = "Adds Cinematic Letterbox [0 or 1]",
        ["Execute"] = "Execute a list of commands [Filename]",
        ["Game.DeleteForgeItem"] = "Binds Forge Delete [No Value]",
        ["+VoIP.Talk"] = "Binds Push to Talk Key [No Value]",
        ["Camera.HideHUD"] = "Hides Game Hud [0 or 1]",
        ["Game.Exit"] = "Ends Halo Online Process [No Value]",
        ["Game.ForceLoad"] = "Force Loads a Map [Map Name]",
        ["Game.GameType"] = "Loads a GameType [GameType Name]",
        ["Game.Info"] = "Displays Information About the Game [No Value]",
        ["Game.LanguageID"] = "Language Index to Use [0-11]",
        ["Game.LogFilter"] = "Sets Filters to Apply to Debug Messages [Filter]",
        ["Game.LogMode"] = "Chooses Which Debug Messages to Print Log [Filter]",
        ["Game.LogName"] = "Debug Log File Name [FileName]",
        ["Game.Map"] = "Loads a Map or Variant File [Map Name]",
        ["Game.MenuURL"] = "Url of HTML Page to Open [URL]",
        ["Game.SetMenuEnabled"] = "Opens the HTML Menu [0 or 1]",
        ["Game.ShowUI"] = "Opens a Specific UI [UI Value]",
        ["Game.Start"] = "Starts a Game or Restarts Current Game [No Value]",
        ["Game.Stop"] = "Stops a Game or Returns Current Game to Lobby [No Value]",
        ["Game.Version"] = "Displays the Current Game Version [No Value]",
        ["Graphics.RedHue"] = "Red Hue Value [Value]",
        ["Graphics.GreenHue"] = "Green Hue Value [Value]",
        ["Graphics.DepthOfField"] = "Cameras Depth of Field [Value]",
        ["Graphics.Saturation"] = "Graphics Saturation [Value]",
        ["IRC.GlobalChannel"] = "The IRC Channel for global chat [#channel]",
        ["IRC.Server"] = "The IRC Server for the Global and Game Chat [IP or URL]",
        ["IRC.ServerPort"] = "IRC Chat Server Port [Port]",
        ["Player.PrintUID"] = "Prints Your UID [No Value]",
        ["Player.PrivKey"] = "Prints Your Unique Stats Private Key [No Value]",
        ["Player.PubKey"] = "Prints Your Unique Stats Public Key [No Value]",
        ["Server.Connect"] = "Connect to a Server [IP]",
        ["Server.KickPlayer"] = "Kick a Player From a Game (Host Only) [Player Name]",
        ["Server.ListPlayers"] = "Lists All Players in the Game (Host Only) [No Value]",
        ["Server.Port"] = "HTTP Port that Servers Run on [Port]",
        ["Time.GameSpeed"] = "Adjusts Games Speed [Value]",
        ["WriteConfig"] = "Writes ElDewrito Settings to Cfg [FileName]",
        ["Help"] = "Displays All Console Commands [No Value]"
      };

    public static Dictionary<string, string> GetTheme() =>
      new Dictionary<string, string>
      {
        ["Dark"] = "BaseDark",
        ["Light"] = "BaseLight"
      };
  }
}