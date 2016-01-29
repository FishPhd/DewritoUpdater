using System.Diagnostics;
using System.Windows;

namespace Dewritwo
{
  /// <summary>
  ///   Interaction logic for MsgBox.xaml
  /// </summary>
  public partial class MsgBox
  {
    public MsgBox(string header, string text)
    {
      InitializeComponent();
      Msg.Text = text;
      Header.Text = header;
    }

    //Titlebar control
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }
  }
}