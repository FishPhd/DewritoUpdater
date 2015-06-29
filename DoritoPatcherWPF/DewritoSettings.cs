using System.Windows.Media;
using System.Windows;

namespace DewritoUpdater
{
    public class DewritoSettings
    {
        public DewritoSettings()
        {
            Player = new DewritoPlayerSettings();
            Video = new DewritoVideoSettings();
            Host = new DewritoHostSettings();
            Input = new DewritoInputSettings();
            //Beta = new DewritoDownloadSettings();
            LaunchParams = new DewritoLaunchParamsSettings();
        }

        public DewritoPlayerSettings Player { get; set; }
        public DewritoVideoSettings Video { get; set; }
        public DewritoHostSettings Host { get; set; }
        public DewritoInputSettings Input { get; set; }
        //public DewritoDownloadSettings Beta { get; set; }
        public DewritoLaunchParamsSettings LaunchParams { get; set; }
    }

    public class DewritoPlayerSettings
    {
        public DewritoPlayerSettings()
        {
            Name = "";
            Uid = "";
            Armor = new DewritoArmorSettings();
            Colors = new DewritoColorSettings();
        }

        public string Name { get; set; }
        public string Uid { get; set; }
        public DewritoArmorSettings Armor { get; set; }
        public DewritoColorSettings Colors { get; set; }
    }

    public class DewritoArmorSettings
    {
        public DewritoArmorSettings()
        {
            Helmet = "air_assault";
            Chest = "air_assault";
            Shoulders = "air_assault";
            Arms = "air_assault";
            Legs = "air_assault";
            Accessory = "base";
            Pelvis = "base";
        }

        public string Helmet { get; set; }
        public string Chest { get; set; }
        public string Shoulders { get; set; }
        public string Arms { get; set; }
        public string Legs { get; set; }
        public string Accessory { get; set; }
        public string Pelvis { get; set; }
    }

    public class DewritoColorSettings
    {
        public DewritoColorSettings()
        {
            Primary = new RgbColorSetting(Colors.Black);
            Secondary = new RgbColorSetting(Colors.Black);
            Visor = new RgbColorSetting(Colors.Black);
            Lights = new RgbColorSetting(Colors.Black);
            Holo = new RgbColorSetting(Colors.Black);
        }

        public RgbColorSetting Primary { get; set; }
        public RgbColorSetting Secondary { get; set; }
        public RgbColorSetting Visor { get; set; }
        public RgbColorSetting Lights { get; set; }
        public RgbColorSetting Holo { get; set; }
    }

    public class RgbColorSetting
    {
        public RgbColorSetting()
        {
        }

        public RgbColorSetting(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public RgbColorSetting(Color color)
            : this(color.R, color.G, color.B)
        {
        }

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public Color AsColor()
        {
            return new Color {A = 255, R = R, G = G, B = B};
        }
    }

    public class DewritoVideoSettings
    {
        public DewritoVideoSettings()
        {
            Fov = 90;
        }

        public float Fov { get; set; }
        public bool CrosshairCentered { get; set; }
        public bool IntroVideo { get; set; }
    }

    public class DewritoHostSettings
    {
        public DewritoHostSettings()
        {
            Countdown = 5;
            MaxPlayer = 16;
        }

        public int MaxPlayer { get; set; }
        public int Countdown { get; set; }
    }

    public class DewritoInputSettings
    {
        public DewritoInputSettings()
        {
            RawMouse = true;
        }

        public bool RawMouse { get; set; }
    }

    /*
    public class DewritoDownloadSettings
    {
        public DewritoDownloadSettings()
        {
            Beta = false;
        }

        public bool Beta { get; set; }
    }
    */

    public class DewritoLaunchParamsSettings
    {
        public DewritoLaunchParamsSettings()
        {
            WindowedMode = false;
            Fullscreen = true;
            NoVSync = false;
            DX9Ex = true;
            FPSCounter = false;
            Width = (int)SystemParameters.PrimaryScreenWidth;
            Height = (int)SystemParameters.PrimaryScreenHeight;
        }

        public bool WindowedMode { get; set; }
        public bool Fullscreen { get; set; }
        public bool NoVSync { get; set; }
        public bool DX9Ex { get; set; }
        public bool FPSCounter { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}