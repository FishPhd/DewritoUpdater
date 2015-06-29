using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Net.Sockets;
using System.Diagnostics;

namespace DewritoUpdater
{
    class Skeet
    {
        public static void Start(string[] args)
        {
            if (args.Length <= 0)
            {
                // register our skeet launcher as dorito: protocol handler
                RegisterHandler();
                Console.WriteLine("URI scheme registered maybe, press enter to exit");
                Console.ReadLine();
                return;
            }

            if (!args[0].StartsWith("dorito:"))
            {
                Console.WriteLine("Invalid usage, press enter to exit");
                Console.ReadLine();
                return;
            }

            string ip = args[0].Substring("dorito:".Length);
            if (!dewCmd("Server.Connect " + ip))
            {
                Process halo = new Process();
                halo.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "eldorado.exe";
                halo.StartInfo.Arguments = "-launcher -Server.Connect=" + ip;
                halo.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                halo.Start();
            }
        }

        public static bool dewCmd(string cmd)
        {
            byte[] data = new byte[1024];
            string stringData;
            TcpClient server;
            try
            {
                server = new TcpClient("127.0.0.1", 2448);
            }
            catch (SocketException)
            {
                return false;
            }
            NetworkStream ns = server.GetStream();

            int recv = ns.Read(data, 0, data.Length);
            stringData = Encoding.ASCII.GetString(data, 0, recv);

            ns.Write(Encoding.ASCII.GetBytes(cmd), 0, cmd.Length);
            ns.Flush();

            ns.Close();
            server.Close();
            return true;
        }

        static void RegisterHandler()
        {
            // Register as the default handler for the tel: protocol.
            const string protocolValue = "dorito:Dorito Invocation";
            Registry.SetValue(
                @"HKEY_CLASSES_ROOT\dorito",
                string.Empty,
                protocolValue,
                RegistryValueKind.String);
            Registry.SetValue(
                @"HKEY_CLASSES_ROOT\dorito",
                "URL Protocol",
                String.Empty,
                RegistryValueKind.String);

            const string binaryName = "DewritoUpdater.exe";
            string command = string.Format("\"{0}{1}\" \"%1\"", AppDomain.CurrentDomain.BaseDirectory, binaryName);
            Registry.SetValue(@"HKEY_CLASSES_ROOT\dorito\shell\open\command", string.Empty, command, RegistryValueKind.String);

            // For Windows 8+, register as a choosable protocol handler.

            // Version detection from http://stackoverflow.com/a/17796139/259953
            Version win8Version = new Version(6, 2, 9200, 0);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Environment.OSVersion.Version >= win8Version)
            {
                Registry.SetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\DoritoProtocolHandler",
                    string.Empty,
                    protocolValue,
                    RegistryValueKind.String);
                Registry.SetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\DoritoProtocolHandler\shell\open\command",
                    string.Empty,
                    command,
                    RegistryValueKind.String);

                Registry.SetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\DoritoProtocolHandler\Capabilities\URLAssociations",
                    "dorito",
                    "DoritoProtocolHandler",
                    RegistryValueKind.String);
                Registry.SetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\RegisteredApplications",
                    "DoritoProtocolHandler",
                    @"SOFTWARE\TelProtocolHandler\Capabilities",
                    RegistryValueKind.String);
            }
        }
    }
}