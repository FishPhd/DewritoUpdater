using System.Collections.Generic;

namespace Dewritwo
{
    internal class Dictionaries
    {
        public static Dictionary<string, string> GetArmor()
        {
            var Armor = new Dictionary<string, string>();
            Armor.Add("Air Assault", "air_assault");
            Armor.Add("Stealth", "stealth");
            Armor.Add("Renegade", "renegade");
            Armor.Add("Nihard", "nihard");
            Armor.Add("Gladiator", "gladiator");
            Armor.Add("Mac", "mac");
            Armor.Add("Shark", "shark");
            Armor.Add("Juggernaut", "juggernaut");
            Armor.Add("Dutch", "dutch");
            Armor.Add("Chameleon", "chameleon");
            Armor.Add("Halberd", "halberd");
            Armor.Add("Cyclops", "cyclops");
            Armor.Add("Scanner", "scanner");
            Armor.Add("Mercenary", "mercenary");
            Armor.Add("Hoplite", "hoplite");
            Armor.Add("Ballista", "ballista");
            Armor.Add("Strider", "strider");
            Armor.Add("Demo", "demo");
            Armor.Add("Orbital", "orbital");
            Armor.Add("Spectrum", "spectrum");
            Armor.Add("Gungnir", "gungnir");
            Armor.Add("Hammerhead", "hammerhead");
            Armor.Add("Omni", "omni");
            Armor.Add("Oracle", "oracle");
            Armor.Add("Silverback", "silverback");
            Armor.Add("Widow Maker", "widow_maker");
            return Armor;
        }

        public static Dictionary<string, string> GetColor()
        {
            var Colors = new Dictionary<string, string>();
            Colors.Add("Blue", "blue");
            Colors.Add("Red", "red");
            Colors.Add("Green", "green");
            Colors.Add("Purple", "purple");
            Colors.Add("Orange", "orange");
            Colors.Add("Lime", "lime");
            Colors.Add("Emerald", "emerald");
            Colors.Add("Teal", "teal");
            Colors.Add("Cyan", "cyan");
            Colors.Add("Cobalt", "cobalt");
            Colors.Add("Indigo", "indigo");
            Colors.Add("Violet", "violet");
            Colors.Add("Pink", "pink");
            Colors.Add("Magenta", "magenta");
            Colors.Add("Crimson", "crimson");
            Colors.Add("Amber", "amber");
            Colors.Add("Yellow", "yellow");
            Colors.Add("Brown", "brown");
            Colors.Add("Olive", "olive");
            Colors.Add("Steel", "steel");
            Colors.Add("Mauve", "mauve");
            Colors.Add("Taupe", "taupe");
            Colors.Add("Sienna", "sienna");
            return Colors;
        }
        public static Dictionary<string, string> GetAction()
        {
            var Actions = new Dictionary<string, string>();
            Actions.Add("Bind", "bind");
            Actions.Add("Commands", "command");
            return Actions;
        }

        public static Dictionary<string, string> GetCommand()
        {
            // Order does matter and command must match commands
            var CommandLine = new Dictionary<string, string>();
            CommandLine.Add("Announce Server", "Server.Announce");
            CommandLine.Add("Announce Stats", "Server.AnnounceStats");
            CommandLine.Add("Automatically Announce", "Server.ShouldAnnounce");
            CommandLine.Add("Bloom Value", "Graphics.Bloom");
            CommandLine.Add("Blue Hue Value", "Graphics.BlueHue");
            CommandLine.Add("Camera Mode", "Camera.Mode");
            CommandLine.Add("Camera Speed", "Camera.Speed");
            CommandLine.Add("Cinematic Letterbox", "Graphics.Letterbox");
            CommandLine.Add("Connect to a Server", "Server.Connect");
            CommandLine.Add("Debug Log File Name", "Game.LogName");
            CommandLine.Add("Debug Log Mode", "Game.LogMode");
            CommandLine.Add("Depth of Field Value", "Graphics.DepthOfField");
            CommandLine.Add("Execute", "Execute");
            CommandLine.Add("Exit Game", "Game.Exit");
            CommandLine.Add("Filter Debug Log", "Game.LogFilter");
            CommandLine.Add("Force Load Map", "Game.ForceLoad");
            CommandLine.Add("Forge (Delete)", "Game.DeleteForgeItem");
            CommandLine.Add("Game Info", "Game.Info");
            CommandLine.Add("Game Speed", "Time.GameSpeed");
            CommandLine.Add("Global Chat Channel", "IRC.GlobalChannel");
            CommandLine.Add("Global Chat Port", "IRC.ServerPort");
            CommandLine.Add("Global Chat Server", "IRC.Server");
            CommandLine.Add("Green Hue Value", "Graphics.GreenHue");
            CommandLine.Add("HTML Menu Open", "Game.SetMenuEnabled");
            CommandLine.Add("HTML Menu URL", "Game.MenuURL");
            CommandLine.Add("HTTP Server Port", "Server.Port");
            CommandLine.Add("Help", "Help");
            CommandLine.Add("Hide HUD", "Camera.HideHUD");
            CommandLine.Add("Kick Player (Host Only)", "Server.KickPlayer");
            CommandLine.Add("Language", "Game.LanguageID");
            CommandLine.Add("List Players (Host Only)", "Server.ListPlayers");
            CommandLine.Add("Load Forge/Map", "Game.Map");
            CommandLine.Add("Load GameType", "Game.GameType");
            CommandLine.Add("Open Specific UI", "Game.ShowUI");
            CommandLine.Add("Print Player UID", "Player.PrintUID");
            CommandLine.Add("Print Private Stats Key", "Player.PrivKey");
            CommandLine.Add("Print Public Stats Key", "Player.PubKey");
            CommandLine.Add("Push to Talk Key", "+VoIP.Talk");
            CommandLine.Add("Red Hue Value", "Graphics.RedHue");
            CommandLine.Add("Saturation Value", "Graphics.Saturation");
            CommandLine.Add("Show Game Version", "Game.Version");
            CommandLine.Add("Start/Restart Game", "Game.Start");
            CommandLine.Add("Write Config File", "WriteConfig"); 
            return CommandLine;
        }

        public static Dictionary<string, string> GetCommandLine()
        {
            //Order doesn't matter as long as command is correct
            var Commands = new Dictionary<string, string>();
            Commands.Add("Server.Announce", "Announces Your Server to the Master Servers [No Value]");
            Commands.Add("Server.AnnounceStats", "Announces Stats to Servers at End of Game [No Value]");
            Commands.Add("Server.ShouldAnnounce", "Server Should Be Announced On Lobby Creation [0 or 1]");
            Commands.Add("Graphics.Bloom", "Atmosphere Bloom Value [Value]");
            Commands.Add("Graphics.BlueHue", "Blue Hue Value [Value]");
            Commands.Add("Camera.Mode", "Camera Mode [default/first/flying/static/spectate]");
            Commands.Add("Camera.Speed", "Camera Speed [Value]");
            Commands.Add("Graphics.Letterbox", "Adds Cinematic Letterbox [0 or 1]");
            Commands.Add("Execute", "Execute a list of commands [Filename]");
            Commands.Add("Game.DeleteForgeItem", "Binds Forge Delete [No Value]");
            Commands.Add("+VoIP.Talk", "Binds Push to Talk Key [No Value]");
            Commands.Add("Camera.HideHUD", "Hides Game Hud [0 or 1]");
            Commands.Add("Game.Exit", "Ends Halo Online Process [No Value]");
            Commands.Add("Game.ForceLoad", "Force Loads a Map [Map Name]");
            Commands.Add("Game.GameType", "Loads a GameType [GameType Name]");
            Commands.Add("Game.Info", "Displays Information About the Game [No Value]");
            Commands.Add("Game.LanguageID", "Language Index to Use [0-11]");
            Commands.Add("Game.LogFilter", "Sets Filters to Apply to Debug Messages [Filter]");
            Commands.Add("Game.LogMode", "Chooses Which Debug Messages to Print Log [Filter]");
            Commands.Add("Game.LogName", "Debug Log File Name [FileName]");
            Commands.Add("Game.Map", "Loads a Map or Variant File [Map Name]");
            Commands.Add("Game.MenuURL", "Url of HTML Page to Open [URL]");
            Commands.Add("Game.SetMenuEnabled", "Opens the HTML Menu [0 or 1]");
            Commands.Add("Game.ShowUI", "Opens a Specific UI [UI Value]");
            Commands.Add("Game.Start", "Starts a Game or Restarts Current Game [No Value]");
            Commands.Add("Game.Version", "Displays the Current Game Version [No Value]");
            Commands.Add("Graphics.RedHue", "Red Hue Value [Value]");
            Commands.Add("Graphics.GreenHue", "Green Hue Value [Value]");
            Commands.Add("Graphics.DepthOfField", "Cameras Depth of Field [Value]");
            Commands.Add("Graphics.Saturation", "Graphics Saturation [Value]");
            Commands.Add("IRC.GlobalChannel", "The IRC Channel for global chat [#channel]");
            Commands.Add("IRC.Server", "The IRC Server for the Global and Game Chat [IP or URL]");
            Commands.Add("IRC.ServerPort", "IRC Chat Server Port [Port]");
            Commands.Add("Player.PrintUID", "Prints Your UID [No Value]");
            Commands.Add("Player.PrivKey", "Prints Your Unique Stats Private Key [No Value]");
            Commands.Add("Player.PubKey", "Prints Your Unique Stats Public Key [No Value]");
            Commands.Add("Server.Connect", "Connect to a Server [IP]");
            Commands.Add("Server.KickPlayer", "Kick a Player From a Game (Host Only) [Player Name]");
            Commands.Add("Server.ListPlayers", "Lists All Players in the Game (Host Only) [No Value]");
            Commands.Add("Server.Port", "HTTP Port that Servers Run on [Port]");
            Commands.Add("Time.GameSpeed", "ADjusts Games Speed [Value]");
            Commands.Add("WriteConfig", "Writes ElDewrito Settings to Cfg [FileName]");
            Commands.Add("Help", "Displays All Console Commands [No Value]");
            return Commands;
        }

        public static Dictionary<string, string> GetTheme()
        {
            var Themes = new Dictionary<string, string>();
            Themes.Add("Dark", "BaseDark");
            Themes.Add("Light", "BaseLight");
            return Themes;
        }
    }
}