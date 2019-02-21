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
        List<Imagess> DuplicatesList = new List<Imagess>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void cancelSetName_Click(object sender, RoutedEventArgs e)
        {
            newFileNameInput.Text = "";
            changename.Visibility = Visibility.Hidden;
            appOptions.Visibility = Visibility.Visible;
        }

        private void saveSetName_Click(object sender, RoutedEventArgs e)
        {
            string oldFile = setNamePath.Content.ToString();
            string newFile = newFileNameInput.Text.ToString();
            if (newFile.Length >= 3)
            {
                string path = oldFile.Substring(0, oldFile.LastIndexOf('\\'));
                string mime = oldFile.Substring(oldFile.LastIndexOf('.') + 1);
                System.IO.File.Move(oldFile, path + "\\" + newFile + "." + mime);

                System.Threading.Thread.Sleep(500);
                newFileNameInput.Text = "";
                changename.Visibility = Visibility.Hidden;
                appOptions.Visibility = Visibility.Visible;

                TreeViewItem item = duplicatesTree.SelectedItem as TreeViewItem;
                string itemPathToImg = item.Header.ToString();
                refreshTree();
                DrawImages(selectedDir.Text);
            }
            else
            {
                System.Windows.MessageBox.Show("Nowa nazwa jest zbyt krótka!");
            }
        }

        private void DeleteSelected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = duplicatesTree.SelectedItem as TreeViewItem;
            if (item == null)
            {
                System.Windows.MessageBox.Show("Wybierz plik!");
            }
            else
            {
                string itemPathToImg = item.Header.ToString();
                DeleteImg(itemPathToImg);
                refreshTree();
                DrawImages(selectedDir.Text);
            }
        }

        private void DeleteDuplicates(object sender, RoutedEventArgs e)
        {
            if(DuplicatesList.Count > 0)
            {
                for (var i = 0; i < DuplicatesList.Count; i++)
                {
                    if (i + 1 < DuplicatesList.Count)
                    {
                        if (CompareImages(DuplicatesList[i].hashCode, DuplicatesList[i + 1].hashCode))
                        {
                            string filepath = DuplicatesList[i].filePath;
                            DeleteImg(filepath);
                        }
                    }
                }
                refreshTree();
                DrawImages(selectedDir.Text);
            }
            else
            {
                System.Windows.MessageBox.Show("Wybierz plik!");
            }
        }

        private void DeleteImg(string filepath)
        {
            if (File.Exists(filepath))
            {
                try
                {
                    File.Delete(filepath);
                }
                catch (Exception ex)
                {

                }
            }
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

        private void ChangeFileName(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = duplicatesTree.SelectedItem as TreeViewItem;
            if (item == null)
            {
                System.Windows.MessageBox.Show("Wybierz plik!");
            }
            else
            {
                string itemPathToImg = item.Header.ToString();
                appOptions.Visibility = Visibility.Hidden;
                changename.Visibility = Visibility.Visible;
                setNamePath.Content = itemPathToImg;
            }
        }

        private void DrawImages(string path)
        {
            DuplicatesList = new List<Imagess>();

            List<Imagess> hashedImages = new List<Imagess>();
            var acceptedFiles = new String[] { "jpg", "jpeg", "png", "bmp", "svg", "gif" };
            var files = GetImages(path, acceptedFiles, false);
            foreach (var f in files)
            {
                byte[] a = HashImg(f);
                hashedImages.Add(new Imagess { filePath = f, hashCode = a });
            }
            CompareToList(hashedImages, DuplicatesList);

            duplicatesTree.Items.Clear();
            if(DuplicatesList.Count < 1)
            {
                var item = new TreeViewItem();
                item.Header = "Brak duplikatów";
                duplicatesTree.Items.Add(item);
            }

            foreach (var d in DuplicatesList)
            {
                var item = new TreeViewItem();
                item.Header = d.filePath;
                duplicatesTree.Items.Add(item);
            }
        }
        private void refreshTree()
        {
            duplicatesTree.Items.Clear();
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
        private static TreeViewItem[] getTreeViewItems(TreeView treeView)
        {
            List<TreeViewItem> returnItems = new List<TreeViewItem>();
            for (int x = 0; x < treeView.Items.Count; x++)
            {
                returnItems.AddRange(getTreeViewItems((TreeViewItem)treeView.Items[x]));
            }
            return returnItems.ToArray();
        }
        private static TreeViewItem[] getTreeViewItems(TreeViewItem currentTreeViewItem)
        {
            List<TreeViewItem> returnItems = new List<TreeViewItem>();
            returnItems.Add(currentTreeViewItem);
            for (int x = 0; x < currentTreeViewItem.Items.Count; x++)
            {
                returnItems.AddRange(getTreeViewItems((TreeViewItem)currentTreeViewItem.Items[x]));
            }
            return returnItems.ToArray();
        }
    }
}
