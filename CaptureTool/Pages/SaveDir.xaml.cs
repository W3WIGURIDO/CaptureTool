using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
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

namespace CaptureTool.Pages
{
    /// <summary>
    /// SaveDir.xaml の相互作用ロジック
    /// </summary>
    public partial class SaveDir : UserControl
    {
        //private bool loadFinished = false;
        private Settings settings
        {
            get
            {
                if (DataContext is Settings dsettings)
                {
                    return dsettings;
                }
                else
                {
                    return null;
                }
            }
        }

        public SaveDir()
        {
            InitializeComponent();
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //loadFinished = true;
        }


        public void ClickRef(object sender, RoutedEventArgs e)
        {
            using (var cofd = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください",
                InitialDirectory = settings.Directory,
                // フォルダ選択モードにする
                IsFolderPicker = true,
            })
            {
                if (cofd.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return;
                }

                // FileNameで選択されたフォルダを取得する
                settings.Directory = cofd.FileName;
            }
        }

        private void NumberResetClick(object sender, RoutedEventArgs e)
        {
            settings.NumberCount = 0;
        }

        private void FileNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (loadFinished)
            //{
            //    settings.FileName = fileNameBox.Text;
            //    settings.NumberCount = 0;
            //}
            MainProcess.CreateFileNameNumberCountButtons(settings.FileName, countButtonPanel, settings);
        }

        private void SaveFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            //settings.Directory = saveFolder.Text;
            MainProcess.CreateFolderNameNumberCountButtons(settings.Directory, folderCountButtonPanel, settings);
        }

        private void DigitsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            settings.DigitsText = digitsTextBox.Text;
        }

        private void CountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(countTextBox.Text, out int result))
            {
                settings.NumberCount = result;
            }
            else
            {
                countTextBox.Text = 0.ToString();
            }
        }

        private void CountUpButton_Click(object sender, RoutedEventArgs e)
        {
            settings.NumberCount++;
        }

        private void CountDownButton_Click(object sender, RoutedEventArgs e)
        {
            settings.NumberCount--;
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            settings.NumberCount = MainProcess.GetContinueFileName(settings);
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("Explorer", settings.Directory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void ParentFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string tmpDir = settings.Directory;
            if (tmpDir.Last() == '\\')
            {
                tmpDir = tmpDir.Substring(0, tmpDir.Length - 1);
            }
            string[] enSplited = tmpDir.Split('\\');
            if (enSplited.Length > 1)
            {
                settings.Directory = string.Join("\\", enSplited.Take(enSplited.Length - 1));
            }
        }

        private void fileNameLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            fileNameBox.Focus();
            fileNameBox.SelectAll();
        }

        private void SaveFolderLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            saveFolder.Focus();
            saveFolder.SelectAll();
        }

        private void AddFavDirButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (settings.FavDirs.ContainsKey(settings.Directory))
                {
                    WpfFolderBrowser.CustomMessageBox.Show(MainWindow.ActiveWindow, "登録済みのディレクトリです。", "メッセージ");
                    return;
                }
                string dirName = System.IO.Path.GetFileNameWithoutExtension(settings.Directory);
                settings.AddFavDir(settings.Directory, dirName);
                favcombo.SelectedIndex = settings.FavDirs.Count - 1;
            }
            catch (Exception ex)
            {
                WpfFolderBrowser.CustomMessageBox.Show(MainWindow.ActiveWindow, ex.Message + Environment.NewLine + ex.StackTrace, "メッセージ");
            }
        }

        private void RemoveFavDirButton_Click(object sender, RoutedEventArgs e)
        {
            if (favcombo.SelectedIndex >= 0)
            {
                try
                {
                    settings.RemoveFavDir(favcombo.SelectedIndex);
                    favcombo.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    WpfFolderBrowser.CustomMessageBox.Show(MainWindow.ActiveWindow, ex.Message + Environment.NewLine + ex.StackTrace, "メッセージ");
                }
            }
        }

        private void setFromFavDir_Click(object sender, RoutedEventArgs e)
        {
            if (favcombo.SelectedIndex >= 0)
            {
                settings.Directory = settings.FavDirs.ElementAt(favcombo.SelectedIndex).Key;
            }
        }

        private void chgFavDirNameButton_Click(object sender, RoutedEventArgs e)
        {
            if (favcombo.SelectedIndex >= 0)
            {
                int selectedIndex = favcombo.SelectedIndex;
                string favdirName;
                favdirName = new InputDialog("名前変更", "新しい名前を入力", "決定", settings.FavDirNames.ElementAt(selectedIndex), MainWindow.ActiveWindow).ShowDialog();
                if (!string.IsNullOrEmpty(favdirName))
                {
                    settings.ChangeFavDirName(favcombo.SelectedIndex, favdirName);
                    favcombo.SelectedIndex = selectedIndex;
                }
            }
        }
    }
}
