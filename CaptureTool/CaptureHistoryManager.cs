using System.Collections.ObjectModel;

namespace CaptureTool
{
    public static class CaptureHistoryManager
    {
        // 上限件数（後から設定対応予定）
        public const int DefaultMaxCount = 10;

        public static ObservableCollection<CaptureHistoryItem> Items { get; }
            = new ObservableCollection<CaptureHistoryItem>();

        // UIスレッドから呼ぶこと（CaptureScreenはDispatcher.Invoke内で実行される）
        public static void Add(CaptureHistoryItem item)
        {
            Items.Insert(0, item);
            while (Items.Count > DefaultMaxCount)
                Items.RemoveAt(Items.Count - 1);
        }
    }
}