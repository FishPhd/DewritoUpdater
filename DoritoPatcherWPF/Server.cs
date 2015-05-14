using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DoritoPatcherWPF
{
    internal class Server
    {
        public static string dewCmd(string cmd)
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
                return "Error";
            }

            NetworkStream ns = server.GetStream();

            int recv = ns.Read(data, 0, data.Length);
            stringData = Encoding.ASCII.GetString(data, 0, recv);

            ns.Write(Encoding.ASCII.GetBytes(cmd), 0, cmd.Length);
            ns.Flush();

            ns.Close();
            server.Close();
            return "Done";
        }
    }
}

/* USAGE
string result = Eldewrito.dewCmd("command to run"); if (result == "Error") { MessageBox.Show("Error communicating with Eldewrito. Please make sure Eldewrito is running and try again."); }
 * */
