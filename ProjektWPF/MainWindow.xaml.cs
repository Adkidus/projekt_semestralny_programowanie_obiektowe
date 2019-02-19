using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
using System.Drawing;
using WinForms = System.Windows.Forms;
using System.Drawing.Imaging;

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

            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                appOptions.Visibility = Visibility.Visible;
                selectedDir.Text = dialog.SelectedPath;
                DrawImages(dialog.SelectedPath);

            }
        }

        private void DrawImages(string path)
        {
            List<Imagess> hashedImages = new List<Imagess>();
            List<Imagess> DuplicatesList = new List<Imagess>();
            var acceptedFiles = new String[] { "jpg", "jpeg", "png", "bmp", "svg", "gif" };
            var files = GetImages(path, acceptedFiles, false);
            foreach (var f in files)
            {
                byte[] a = HashImg(f);
                hashedImages.Add(new Imagess { filePath = f, hashCode = a });
            }
            CompareToList(hashedImages, DuplicatesList);

            duplicatesTree.Items.Clear();
            foreach (var d in DuplicatesList)
            {
                var item = new TreeViewItem();
                item.Header = d.filePath;
                duplicatesTree.Items.Add(item);
            }
        }

        private static void CompareToList(List<Imagess> hashedImages, List<Imagess> DuplicatesList)
        {
            for (int i = 0; i <= hashedImages.Count; i++)
            {
                for (int j = i + 1; j < hashedImages.Count; j++)
                {
                    if (CompareImages(hashedImages[i].hashCode, hashedImages[j].hashCode))
                    {
                        bool isInList = false;
                        bool isInList_j = false;
                        foreach (var d in DuplicatesList)
                        {
                            if (d.filePath == hashedImages[i].filePath)
                                isInList = true;
                            else if (d.filePath == hashedImages[j].filePath)
                                isInList_j = true;
                            else { }
                        }
                        if (!isInList)
                            DuplicatesList.Add(hashedImages[i]);
                        if (!isInList_j)
                            DuplicatesList.Add(hashedImages[j]);
                    }
                }
            }
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

        public static byte[] HashImg(string fileName)
        {
            using (var image = new Bitmap(fileName))
            {
                var sha256 = SHA256.Create();

                var rect = new System.Drawing.Rectangle(0, 0, image.Width, image.Height);
                var data = image.LockBits(rect, ImageLockMode.ReadOnly, image.PixelFormat);

                var dataPtr = data.Scan0;

                var totalBytes = (int)Math.Abs(data.Stride) * data.Height;
                var rawData = new byte[totalBytes];
                System.Runtime.InteropServices.Marshal.Copy(dataPtr, rawData, 0, totalBytes);

                image.UnlockBits(data);

                return sha256.ComputeHash(rawData);
            }
        }
        public static bool CompareImages(byte[] img1, byte[] img2)
        {
            if (img1 == img2) return true;
            if (img1 == null || img2 == null) return false;
            if (img1.Length != img2.Length) return false;
            for (int i = 0; i < img1.Length; i++)
            {
                if (img1[i] != img2[i]) return false;
            }
            return true;
        }

    }
}
