using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Awesomium;
using DoritoPatcherWPF;

namespace DoritoPatcherWPF
{

    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mime;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Xml.Serialization;
    using System.Windows.Media.Animation;
    using System.Windows.Navigation;
    using System.Windows.Threading;

    using DoritoPatcher;

    using Newtonsoft.Json;
    /// <summary>
    /// Interaction logic for MsgBox2.xaml
    /// </summary>
    public partial class MsgBox2 : Window
    {
        

        public MsgBox2(string text)
        {
            InitializeComponent();
            Msg.Text = text;
        }

        //Ok button (Close)
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //Titlebar control
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
