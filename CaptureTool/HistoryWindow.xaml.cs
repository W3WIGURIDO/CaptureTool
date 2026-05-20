using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace CaptureTool
{
    public partial class HistoryWindow : Window
    {
        private static HistoryWindow _instance;

        public static void ShowOrActivate(bool topMost)
        {
            if (_instance == null || !_instance.IsLoaded)
            {
                _instance = new HistoryWindow(topMost);
                _instance.Show();
            }
            else
            {
                _instance.Activate();
            }
        }

        private readonly ObservableCollection<HistoryItemViewModel> _viewModels
            = new ObservableCollection<HistoryItemViewModel>();

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private HistoryWindow(bool topMost)
        {
            InitializeComponent();
            Topmost = topMost;
            historyListBox.ItemsSource = _viewModels;

            // 既存アイテムのViewModel生成（最新が先頭のため順序はそのまま）
            foreach (var item in CaptureHistoryManager.Items)
                _viewModels.Add(new HistoryItemViewModel(item));

            UpdateCountLabel();

            // 以後の追加・削除をViewModelコレクションに同期する
            CaptureHistoryManager.Items.CollectionChanged += OnSourceCollectionChanged;

            // 全件のサムネイルを非同期ロード
            LoadAllThumbnailsAsync();
        }

        // ウィンドウ表示中に新たなキャプチャが追加された場合の同期処理
        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CaptureHistoryItem item in e.NewItems)
                {
                    var vm = new HistoryItemViewModel(item);
                    _viewModels.Insert(0, vm);

                    // ViewModelも上限に合わせて末尾を削除
                    while (_viewModels.Count > CaptureHistoryManager.DefaultMaxCount)
                        _viewModels.RemoveAt(_viewModels.Count - 1);

                    // 追加分のサムネイルを非同期ロード（fire-and-forget）
                    _ = vm.LoadThumbnailAsync(_cts.Token);
                }
                UpdateCountLabel();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (CaptureHistoryItem item in e.OldItems)
                {
                    var vm = _viewModels.FirstOrDefault(v => v.Item == item);
                    if (vm != null) _viewModels.Remove(vm);
                }
                UpdateCountLabel();
            }
        }

        private async void LoadAllThumbnailsAsync()
        {
            var tasks = _viewModels.Select(vm => vm.LoadThumbnailAsync(_cts.Token)).ToList();
            try { await System.Threading.Tasks.Task.WhenAll(tasks); }
            catch (OperationCanceledException) { }
        }

        private void UpdateCountLabel()
        {
            countLabel.Text = $"{_viewModels.Count}件";
        }

        private HistoryItemViewModel SelectedSingle
            => historyListBox.SelectedItem as HistoryItemViewModel;

        private void HistoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenImage(SelectedSingle);
        }

        private void OpenImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenImage(SelectedSingle);
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = SelectedSingle;
            if (vm == null) return;
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{vm.FilePath}\"");
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = SelectedSingle;
            if (vm == null) return;
            if (!File.Exists(vm.FilePath))
            {
                ShowError("ファイルが見つかりません。\n" + vm.FilePath);
                return;
            }

            string ext = Path.GetExtension(vm.FilePath);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(vm.FilePath);
            string dir = Path.GetDirectoryName(vm.FilePath);

            string newName = new InputDialog("改名", "新しいファイル名（拡張子不要）",
                "決定", nameWithoutExt, this).ShowDialog();

            if (string.IsNullOrWhiteSpace(newName) || newName == nameWithoutExt) return;

            newName = Regex.Replace(newName, @"[\\/:*?""<>|]", "").Trim();
            if (string.IsNullOrEmpty(newName)) return;

            string newPath = Path.Combine(dir, newName + ext);
            try
            {
                File.Move(vm.FilePath, newPath);
                vm.FilePath = newPath;  // ViewModelのFilePath経由でItemも更新される
            }
            catch (Exception ex) { ShowError("改名に失敗しました。\n" + ex.Message); }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = historyListBox.SelectedItems
                .Cast<HistoryItemViewModel>().ToList();
            if (selected.Count == 0) return;

            string msg = selected.Count == 1
                ? $"{selected[0].FileName} を削除しますか？"
                : $"選択した {selected.Count} 件を削除しますか？";

            if (WpfFolderBrowser.CustomMessageBox.Show(this, msg, "削除確認",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning,
                    MessageBoxResult.Cancel, MessageBoxOptions.None) != MessageBoxResult.OK) return;

            foreach (var vm in selected)
            {
                if (vm.FileExists)
                {
                    try { File.Delete(vm.FilePath); }
                    catch (Exception ex)
                    {
                        ShowError($"{vm.FileName} の削除に失敗しました。\n" + ex.Message);
                        continue; // ファイル削除失敗時はリストからも除去しない
                    }
                }
                // ファイルなし状態 or ファイル削除成功 → マネージャーから除去
                // OnSourceCollectionChanged が ViewModelも除去する
                CaptureHistoryManager.Items.Remove(vm.Item);
            }
        }

        private void OpenImage(HistoryItemViewModel vm)
        {
            if (vm == null) return;
            if (!File.Exists(vm.FilePath))
            {
                ShowError("ファイルが見つかりません。\n" + vm.FilePath);
                return;
            }
            try { System.Diagnostics.Process.Start(vm.FilePath); }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CaptureHistoryManager.Items.CollectionChanged -= OnSourceCollectionChanged;
            _cts.Cancel();
            _cts.Dispose();
            _instance = null;
        }

        private void ShowError(string message)
        {
            WpfFolderBrowser.CustomMessageBox.Show(this, message, "エラー",
                MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None);
        }
    }
}