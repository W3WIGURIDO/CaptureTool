using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CaptureTool
{
    public class HistoryItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private const int ThumbWidth = 120;
        private const int ThumbHeight = 90;

        public CaptureHistoryItem Item { get; }

        // リネーム時にItemのFilePathも更新し、FileName表示に反映する
        public string FilePath
        {
            get => Item.FilePath;
            set
            {
                Item.FilePath = value;
                Notify();
                Notify(nameof(FileName));
            }
        }

        public string FileName => Path.GetFileName(Item.FilePath);
        public string TabLabel => $"Tab {Item.TabNumber + 1}";
        public string CapturedAtText => Item.CapturedAt.ToString("HH:mm:ss");

        private BitmapSource _thumbnail;
        public BitmapSource Thumbnail
        {
            get => _thumbnail;
            private set { _thumbnail = value; Notify(); }
        }

        private bool _fileExists = true;
        public bool FileExists
        {
            get => _fileExists;
            private set { _fileExists = value; Notify(); }
        }

        public HistoryItemViewModel(CaptureHistoryItem item)
        {
            Item = item;
        }

        public async Task LoadThumbnailAsync(CancellationToken ct)
        {
            if (!File.Exists(Item.FilePath))
            {
                FileExists = false;
                return;
            }

            // Task.Runでバックグラウンドスレッドにてサムネイル取得
            // awaitにより継続はUIスレッドで実行されるためThumbnailへの代入は安全
            var result = await Task.Run(
                () => ct.IsCancellationRequested
                    ? null
                    : ShellThumbnail.GetThumbnail(Item.FilePath, ThumbWidth, ThumbHeight),
                ct);

            if (!ct.IsCancellationRequested)
                Thumbnail = result;
        }
    }
}