using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace ProjektWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DisplayDialog(object sender, RoutedEventArgs e)
        {
            var dialog = new WinForms.FolderBrowserDialog();

            if(dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                appOptions.Visibility = Visibility.Visible;
                selectedDir.Text = dialog.SelectedPath;
                DrawImages(dialog.SelectedPath);
                
            }
        }

        private void DrawImages(string path)
        {
            var acceptedFiles = new String[] { "jpg", "jpeg", "png", "bmp", "svg", "gif" };
            var files = GetImages(path, acceptedFiles, false);
        }

        public static String[] GetImages(string folder, String[] filter, bool isRec)
        {
            List<String> files = new List<string>();
            var opt = isRec ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var f in filter)
            {
                files.AddRange(Directory.GetFiles(folder, String.Format("*.{0}", f), opt));
            }
            return files.ToArray();
        }
    }
}
