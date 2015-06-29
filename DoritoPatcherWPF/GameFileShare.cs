using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using DewritoUpdater.Utils;
using Microsoft.Win32;

namespace DewritoUpdater
{
    public static class GameFileShare
    {
        public const string ProtocolName = "URL: H:O FileShare Protocol";
        public const string Protocol = "blamfile";

        /// <summary>
        ///     Add or update the blamfile protocol in the user's registery.
        /// </summary>
        public static void RegisterProtocol()
        {
            var rKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\" + Protocol, true);
            if (rKey == null)
                rKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + Protocol);

            rKey.SetValue("", ProtocolName);
            rKey.SetValue("URL Protocol", "");

            rKey = rKey.CreateSubKey(@"shell\open\command");
            rKey.SetValue("", "\"" + Assembly.GetExecutingAssembly().Location + "\" \"%1\"");

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
            var id = queries["id"].ToLower();
            var type = queries["type"].ToLower();

            if (!(type == "forge" || type == "gametype"))
                throw new ApplicationException("Variant type is invalid.");

            var wc = new WebClient();
            var json = wc.DownloadString(string.Format("https://{0}/api/{1}/{2}", blamUri.Host, type, id));


            return JSONSerializer<Model>.DeSerialize(json);
        }

        /// <summary>
        ///     Download a game
        /// </summary>
        /// <param name="url"></param>
        /// <param name="variant"></param>
        /// <param name="onProgress"></param>
        /// <param name="onCompleted"></param>
        /// <param name="onDuplicate"></param>
        public static bool Download(Uri url, Model variant, Action<int> onProgress,
            Action<AsyncCompletedEventArgs> onCompleted, Func<bool> onDuplicate)
        {
            var wc = new WebClient();

            wc.DownloadProgressChanged += (s, e) => onProgress(e.ProgressPercentage);
            wc.DownloadFileCompleted += (s, e) => onCompleted(e);

            var path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            switch (variant.Type)
            {
                case "Forge":
                    path = Path.Combine(path, "mods/forge/");
                    break;
                case "GameType":
                    path = Path.Combine(path, "mods/variants/");
                    break;
            }
            Directory.CreateDirectory(path);


            var filePath =
                Path.Combine(path, string.Format("{0} ({1}).bin", variant.Name, variant.Author));


            // Ask the user if they want to overwrite the variant if it already exists.
            if (File.Exists(filePath))
            {
                if (!onDuplicate())
                {
                    return false;
                }
            }
            wc.DownloadFileAsync(new Uri("https://" + url.Host + variant.Download), filePath);
            return true;
        }

        [DataContract]
        public class Model
        {
            /// <summary>
            ///     Type of variant can either be Forge or GameType.
            /// </summary>
            [DataMember]
            public string Type { get; set; }

            /// <summary>
            ///     Name of the variant, this can be the map name or gametype name.
            /// </summary>
            [DataMember]
            public string TypeName { get; set; }

            /// <summary>
            ///     Author of the variant.
            /// </summary>
            [DataMember]
            public string Author { get; set; }

            /// <summary>
            ///     Unique Identifier of the variant.
            /// </summary>
            [DataMember]
            public int Id { get; set; }

            /// <summary>
            ///     Variant name.
            /// </summary>
            [DataMember]
            public string Name { get; set; }

            /// <summary>
            ///     Short variant description.
            /// </summary>
            [DataMember]
            public string Description { get; set; }

            /// <summary>
            ///     Icon of the specific variant.
            /// </summary>
            [DataMember]
            public string Icon { get; set; }

            /// <summary>
            ///     Direct download path to the variant, excluding host.
            /// </summary>
            [DataMember]
            public string Download { get; set; }

            /// <summary>
            ///     Name of the FileShare provider.
            /// </summary>
            [DataMember]
            public string Provider { get; set; }

            /// <summary>
            ///     Icon of the FileShare provider.
            /// </summary>
            [DataMember]
            public string ProviderIcon { get; set; }
        }
    }
}