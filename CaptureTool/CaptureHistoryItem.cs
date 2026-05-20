using System;

namespace CaptureTool
{
    public class CaptureHistoryItem
    {
        public string FilePath { get; set; }
        public int TabNumber { get; }
        public DateTime CapturedAt { get; }

        public CaptureHistoryItem(string filePath, int tabNumber, DateTime capturedAt)
        {
            FilePath = filePath;
            TabNumber = tabNumber;
            CapturedAt = capturedAt;
        }
    }
}