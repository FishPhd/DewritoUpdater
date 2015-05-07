using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Win32;
using DoritoPatcherWPF.Utils;
using System.Runtime.Serialization;
using System.Windows;

namespace DoritoPatcherWPF
{
    public static class GameFileShare
    {
        public const string ProtocolName = "URL: H:O FileShare Protocol";
        public const string Protocol = "blamfile";

        /// <summary>
        /// Add or update the blamfile protocol in the user's registery.
        /// </summary>
        public static void RegisterProtocol()
        {
            RegistryKey rKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\" + Protocol, true);
            if (rKey == null)
                rKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + Protocol);

            rKey.SetValue("", ProtocolName);
            rKey.SetValue("URL Protocol", "");

            rKey = rKey.CreateSubKey(@"shell\open\command");
            rKey.SetValue("", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\" \"%1\"");

            if (rKey != null)
                rKey.Close();
        }

        public static Uri ParseUri(string arg)
        {
            Uri uri;
            if (Uri.TryCreate(arg, UriKind.Absolute, out uri))
            {
                if (uri.Scheme != Protocol)
                    return null;
                return uri;
            }
            return null;
        }

        public static Model FetchVariant(Uri blamUri)
        {
            /* =======================================
             * URI Schemes
             * =======================================
             * 
             * blamfile:
             *      blamfile://<host>?type=<type>&id=<id>
             * api:
             *      https://<host>/api/<type>/<id>
             * ---------------------------------------
             * host:
             *      Passing the host will allow for different domains
             *      Without having to update the launcher to accomodate
             *      for the domain changes. HTTPS is REQUIRED.
             * type:
             *      Type is type of variant the scheme is representing.
             *      Allowed Types are: Forge, GameType.
             * id:
             *      The identifier of the variant type.
             */

            var queries = blamUri.ParseQueryString();
            string id = queries["id"].ToLower();
            string type = queries["type"].ToLower();

            if (!(type == "forge" || type == "gametype"))
                throw new ApplicationException("Variant type is invalid.");

            WebClient wc = new WebClient();
            string json = wc.DownloadString(string.Format("https://{0}/api/{1}/{2}", blamUri.Host, type, id));
            
            
            return Utils.JSONSerializer<Model>.DeSerialize(json);
        }

        [DataContract]
        public class Model
        {
            /// <summary>
            /// Type of variant can either be Forge or GameType.
            /// </summary>
            [DataMember]
            public string Type { get; set; }
            /// <summary>
            /// Name of the variant, this can be the map name or gametype name.
            /// </summary>
            [DataMember]
            public string TypeName { get; set; }
            /// <summary>
            /// Author of the variant.
            /// </summary>
            [DataMember]
            public string Author { get; set; }
            /// <summary>
            /// Unique Identifier of the variant.
            /// </summary>
            [DataMember]
            public int Id { get; set; }
            /// <summary>
            /// Variant name.
            /// </summary>
            [DataMember]
            public string Name { get; set; }
            /// <summary>
            /// Short variant description.
            /// </summary>
            [DataMember]
            public string Description { get; set; }
            /// <summary>
            /// Icon of the specific variant.
            /// </summary>
            [DataMember]
            public string Icon { get; set; }
            /// <summary>
            /// Direct download path to the variant, excluding host.
            /// </summary>
            [DataMember]
            public string Download { get; set; }
            /// <summary>
            /// Name of the FileShare provider.
            /// </summary>
            [DataMember]
            public string Provider { get; set; }
            /// <summary>
            /// Icon of the FileShare provider.
            /// </summary>
            [DataMember]
            public string ProviderIcon { get; set; }
        }
    }
}
