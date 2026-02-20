using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;
using static CaptureTool.Settings;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace CaptureTool
{
    public class Settings : INotifyPropertyChanged
    {
        const string WordDir = "<Dir>";
        private string defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\Capture";
        const string etcHosts = @"C:\Windows\System32\drivers\etc\hosts";
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private Keys _Key;
        public Keys Key
        {
            get => _Key;
            set
            {
                _Key = value;
                KeyText = Enum.GetName(typeof(Keys), value);
                RaisePropertyChanged();
            }
        }

        private Keys _PreKey;
        public Keys PreKey
        {
            get => _PreKey;
            set
            {
                _PreKey = value;
                PreKeyText = Enum.GetName(typeof(Keys), value);
                RaisePropertyChanged();
            }
        }

        private string _KeyText;
        public string KeyText
        {
            get => _KeyText;
            set
            {
                _KeyText = value;
                RaisePropertyChanged();
            }
        }

        private string _PreKeyText;
        public string PreKeyText
        {
            get => _PreKeyText;
            set
            {
                _PreKeyText = value;
                RaisePropertyChanged();
            }
        }

        private Keys _ScreenKey;
        public Keys ScreenKey
        {
            get => _ScreenKey;
            set
            {
                _ScreenKey = value;
                ScreenKeyText = Enum.GetName(typeof(Keys), value);
                RaisePropertyChanged();
            }
        }

        private Keys _ScreenPreKey;
        public Keys ScreenPreKey
        {
            get => _ScreenPreKey;
            set
            {
                _ScreenPreKey = value;
                ScreenPreKeyText = Enum.GetName(typeof(Keys), value);
                RaisePropertyChanged();
            }
        }

        private string _ScreenKeyText;
        public string ScreenKeyText
        {
            get => _ScreenKeyText;
            set
            {
                _ScreenKeyText = value;
                RaisePropertyChanged();
            }
        }

        private string _ScreenPreKeyText;
        public string ScreenPreKeyText
        {
            get => _ScreenPreKeyText;
            set
            {
                _ScreenPreKeyText = value;
                RaisePropertyChanged();
            }
        }

        private Keys _SelectKey;
        public Keys SelectKey
        {
            get => _SelectKey;
            set
            {
                _SelectKey = value;
                SelectKeyText = Enum.GetName(typeof(Keys), value);
                RaisePropertyChanged();
            }
        }

        private Keys _SelectPreKey;
        public Keys SelectPreKey
        {
            get => _SelectPreKey;
            set
            {
                _SelectPreKey = value;
                SelectPreKeyText = Enum.GetName(typeof(Keys), value);
                RaisePropertyChanged();
            }
        }

        private string _SelectKeyText;
        public string SelectKeyText
        {
            get => _SelectKeyText;
            set
            {
                _SelectKeyText = value;
                RaisePropertyChanged();
            }
        }

        private string _SelectPreKeyText;
        public string SelectPreKeyText
        {
            get => _SelectPreKeyText;
            set
            {
                _SelectPreKeyText = value;
                RaisePropertyChanged();
            }
        }

        private bool _WindowCaptureEnabled;
        public bool WindowCaptureEnabled
        {
            get => _WindowCaptureEnabled;
            set
            {
                var preVal = _WindowCaptureEnabled;
                _WindowCaptureEnabled = value;
                RaisePropertyChanged();
                if (preVal != _WindowCaptureEnabled)
                {
                    HotKeySettings?.ResetWindowHotKey();
                }
            }
        }

        private bool _ScreenCaptureEnabled;
        public bool ScreenCaptureEnabled
        {
            get => _ScreenCaptureEnabled;
            set
            {
                var preVal = _ScreenCaptureEnabled;
                _ScreenCaptureEnabled = value;
                RaisePropertyChanged();
                if (preVal != _ScreenCaptureEnabled)
                {
                    HotKeySettings?.ResetScreenHotKey();
                }
            }
        }

        private bool _SelectEnabled;
        public bool SelectEnabled
        {
            get => _SelectEnabled;
            set
            {
                var preVal = _SelectEnabled;
                _SelectEnabled = value;
                RaisePropertyChanged();
                if (preVal != _SelectEnabled)
                {
                    HotKeySettings?.ResetSelectHotKey();
                }
            }
        }

        public bool ContainsHotKeyPair(Keys preKey, Keys key)
        {
            if ((preKey == PreKey && key == Key) || (preKey == ScreenPreKey && key == ScreenKey) || (preKey == SelectPreKey && key == SelectKey))
            {
                return true;
            }
            return false;
        }

        private string _Directory;
        public string Directory
        {
            get => _Directory;
            set
            {
                if (_Directory != null && !_Directory.Equals(value))
                {
                    NumberCount = 0;
                }
                _Directory = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Directory));
                if (FileName != null && FileName.Contains(WordDir))
                {
                    CreateSampleFileName();
                }
            }
        }

        private string _FileName;
        public string FileName
        {
            get => _FileName;
            set
            {
                try
                {
                    //string tmpValue = System.Text.RegularExpressions.Regex.Replace(value, "[/:*?\"<>|\r\n]", string.Empty);
                    //独自の正規表現で<>を使用するため、<>を除外
                    string tmpValue = System.Text.RegularExpressions.Regex.Replace(value, "[/:*?\"|\r\n]", string.Empty);
                    if (tmpValue.Length > 259)
                    {
                        MessageBox.Show("保存パスはフォルダパス、ファイルパス含めて260字未満になるように設定してください。" + Environment.NewLine + "設定しようとした値：" + Environment.NewLine + value);
                        RaisePropertyChanged();
                        RaisePropertyChanged(nameof(FileName));
                        return;
                    }
                    string[] enSplited = tmpValue.Split('\\');
                    if (enSplited.Length == 1)
                    {
                        _FileName = tmpValue;
                    }
                    else
                    {
                        _FileName = enSplited.Last();
                        if (Directory.Last() != '\\')
                        {
                            Directory += "\\";
                        }
                        Directory += string.Join("\\", enSplited.Take(enSplited.Length - 1));
                    }
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(FileName));
                    CreateSampleFileName();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + "設定しようとした値：" + Environment.NewLine + value);
                }
            }
        }

        private string _SampleFileName;
        public string SampleFileName
        {
            get => _SampleFileName;
        }

        private bool? _EnableNumber;
        public bool? EnableNumber
        {
            get => _EnableNumber;
            set
            {
                _EnableNumber = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(EnableNumber));
                CreateSampleFileName();
            }
        }

        private int _NumberDigits;
        public int NumberDigits
        {
            get => _NumberDigits;
        }

        private string _DigitsText;
        public string DigitsText
        {
            get => _DigitsText;
            set
            {
                _DigitsText = value;
                if (int.TryParse(value, out int result))
                {
                    _NumberDigits = result;
                }
                else
                {
                    _NumberDigits = 1;
                    _DigitsText = 1.ToString();
                }
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(DigitsText));
                RaisePropertyChanged(nameof(NumberDigits));
                CreateSampleFileName();
            }
        }

        private int _NumberCount;
        public int NumberCount
        {
            get => _NumberCount;
            set
            {
                _NumberCount = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NumberCount));
                CreateSampleFileName();
            }
        }

        private int _SaveFormatIndex;
        public int SaveFormatIndex
        {
            get => _SaveFormatIndex;
            set
            {
                _SaveFormatIndex = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SaveFormatIndex));
                CreateSampleFileName();
            }
        }

        private readonly Dictionary<SaveFormat, string> _SaveFormats = new Dictionary<SaveFormat, string>() { { SaveFormat.PNG, "PNG" }, { SaveFormat.JPG, "JPG" } };
        public Dictionary<SaveFormat, string> SaveFormats
        {
            get => _SaveFormats;
        }

        private bool? _EnableTray;
        public bool? EnableTray
        {
            get => _EnableTray;
            set
            {
                _EnableTray = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(EnableTray));
            }
        }

        private bool? _EnableOverlay;
        public bool? EnableOverlay
        {
            get => _EnableOverlay;
            set
            {
                _EnableOverlay = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(EnableOverlay));
            }
        }

        private string _OverlayTime;
        public string OverlayTime
        {
            get => _OverlayTime;
            set
            {
                _OverlayTime = value;
                if (int.TryParse(value, out int result))
                {
                    _OverlayTimeInt = result;
                }
                else
                {
                    _OverlayTimeInt = 3000;
                    _OverlayTime = 3000.ToString();
                }
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(OverlayTimeInt));
            }
        }

        private int _OverlayTimeInt;
        public int OverlayTimeInt
        {
            get => _OverlayTimeInt;
        }

        private int _PositionIndex;
        public int PositionIndex
        {
            get => _PositionIndex;
            set
            {
                _PositionIndex = value;
                RaisePropertyChanged();
            }
        }

        private readonly Dictionary<string, PositionSet> _ViewPosition = new Dictionary<string, PositionSet>() {
            { "左上", new PositionSet(System.Windows.HorizontalAlignment.Left, System.Windows.VerticalAlignment.Top) },
            { "右上", new PositionSet(System.Windows.HorizontalAlignment.Right, System.Windows.VerticalAlignment.Top) },
            { "左下", new PositionSet(System.Windows.HorizontalAlignment.Left, System.Windows.VerticalAlignment.Bottom) },
            { "右下", new PositionSet(System.Windows.HorizontalAlignment.Right, System.Windows.VerticalAlignment.Bottom) }
        };
        public Dictionary<string, PositionSet> ViewPosition
        {
            get => _ViewPosition;
        }

        private bool? _OverlayTabNameEnabled;
        public bool? OverlayTabNameEnabled
        {
            get => _OverlayTabNameEnabled;
            set
            {
                _OverlayTabNameEnabled = value;
                RaisePropertyChanged();
            }
        }

        private bool? _OverlayFileNameEnabled;
        public bool? OverlayFileNameEnabled
        {
            get => _OverlayFileNameEnabled;
            set
            {
                _OverlayFileNameEnabled = value;
                RaisePropertyChanged();
            }
        }

        private bool? _EnableAero;
        public bool? EnableAero
        {
            get => _EnableAero;
            set
            {
                _EnableAero = value;
                RaisePropertyChanged();
            }
        }

        private bool? _EnableAutoSave;
        public bool? EnableAutoSave
        {
            get => _EnableAutoSave;
            set
            {
                _EnableAutoSave = value;
                RaisePropertyChanged();
            }
        }

        private double _WindowOpacity = 1.0;
        public double WindowOpacity
        {
            get => _WindowOpacity;
            set
            {
                _WindowOpacity = value;
                RaisePropertyChanged();
            }
        }

        private bool? _EnableCursor;
        public bool? EnableCursor
        {
            get => _EnableCursor;
            set
            {
                _EnableCursor = value;
                RaisePropertyChanged();
            }
        }

        private bool? _EnableChangeCapture;
        public bool? EnableChangeCapture
        {
            get => _EnableChangeCapture;
            set
            {
                _EnableChangeCapture = value;
                DisableAero = !value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(EnableChangeCapture));
            }
        }

        private bool? _DisableAero;
        public bool? DisableAero
        {
            get => _DisableAero;
            set
            {
                _DisableAero = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(DisableAero));
            }
        }

        private int _OverlayX;
        public int OverlayX
        {
            get => _OverlayX;
            set
            {
                _OverlayX = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(OverlayX));
            }
        }

        private int _OverlayY;
        public int OverlayY
        {
            get => _OverlayY;
            set
            {
                _OverlayY = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(OverlayY));
            }
        }

        private bool? _EnableSetArrow;
        public bool? EnableSetArrow
        {
            get => _EnableSetArrow;
            set
            {
                _EnableSetArrow = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(EnableSetArrow));
            }
        }

        private int _PixelFormatIndex;
        public int PixelFormatIndex
        {
            get => _PixelFormatIndex;
            set
            {
                _PixelFormatIndex = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(PixelFormatIndex));
            }
        }

        private readonly Dictionary<System.Drawing.Imaging.PixelFormat, string> _PixelFormats = new Dictionary<System.Drawing.Imaging.PixelFormat, string>() { { System.Drawing.Imaging.PixelFormat.Format32bppArgb, "32bppArgb" }, { System.Drawing.Imaging.PixelFormat.Format24bppRgb, "24bppRgb" }, { System.Drawing.Imaging.PixelFormat.Format16bppRgb555, "16bppRgb555" }, { System.Drawing.Imaging.PixelFormat.Format8bppIndexed, "8bppIndexed" } };
        public Dictionary<System.Drawing.Imaging.PixelFormat, string> PixelFormats
        {
            get => _PixelFormats;
        }

        private int _CaptureModeIndex;
        public int CaptureModeIndex
        {
            get => _CaptureModeIndex;
            set
            {
                _CaptureModeIndex = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CaptureModeIndex));
            }
        }

        private readonly Dictionary<int, string> _CaptureModes = new Dictionary<int, string>() { { 0, "GDI(Vista以前)" }, { 1, "通常(デフォルト)" }, { 2, "GDI" }, { 3, "DesktopDuplication" } };
        public Dictionary<int, string> CaptureModes
        {
            get => _CaptureModes;
        }

        private bool? _EnableVisibilityControl;
        public bool? EnableVisibilityControl
        {
            get => _EnableVisibilityControl;
            set
            {
                _EnableVisibilityControl = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(EnableVisibilityControl));
            }
        }

        private string _CountConju;
        public string CountConju
        {
            get => _CountConju;
            set
            {
                string tmpValue = System.Text.RegularExpressions.Regex.Replace(value, "[/:*?\"<>|\r\n]", string.Empty);
                _CountConju = tmpValue;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CountConju));
                CreateSampleFileName();
            }
        }

        private readonly System.Collections.ObjectModel.ObservableCollection<string> _FavDirNames = new System.Collections.ObjectModel.ObservableCollection<string>();
        public System.Collections.ObjectModel.ObservableCollection<string> FavDirNames
        {
            get => _FavDirNames;
        }

        private readonly Dictionary<string, string> _FavDirs = new Dictionary<string, string>();
        public Dictionary<string, string> FavDirs
        {
            get => _FavDirs;
        }

        public void AddFavDir(string key, string value)
        {
            _FavDirs.Add(key, value);
            _FavDirNames.Add(value);
            RaisePropertyChanged(nameof(FavDirNames));
        }

        public void RemoveFavDir(string key)
        {
            int index = _FavDirs.Keys.ToList().IndexOf(key);
            _FavDirs.Remove(key);
            _FavDirNames.RemoveAt(index);
            RaisePropertyChanged(nameof(FavDirNames));
        }

        public void RemoveFavDir(int index)
        {
            string key = _FavDirs.ElementAt(index).Key;
            _FavDirs.Remove(key);
            _FavDirNames.RemoveAt(index);
            RaisePropertyChanged(nameof(FavDirNames));
        }

        public void ChangeFavDirName(string key, string value)
        {
            int index = _FavDirs.Keys.ToList().IndexOf(key);
            _FavDirs[key] = value;
            _FavDirNames[index] = value;
            RaisePropertyChanged(nameof(FavDirNames));
        }

        public void ChangeFavDirName(int index, string value)
        {
            string key = _FavDirs.ElementAt(index).Key;
            _FavDirs[key] = value;
            _FavDirNames[index] = value;
            RaisePropertyChanged(nameof(FavDirNames));
        }

        private string ToStringFavDirKeys()
        {
            return string.Join("\n", _FavDirs.Keys);
        }

        private string ToStringFavDirValues()
        {
            return string.Join("\n", _FavDirs.Values);
        }

        private int _CompressIndex;
        public int CompressIndex
        {
            get => _CompressIndex;
            set
            {
                _CompressIndex = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CompressIndex));
            }
        }

        private readonly Dictionary<string, string> _CompressNums = new Dictionary<string, string>() { { "-o1", "1" }, { "-o2", "2" }, { "-o3", "3" }, { "-o4", "4" }, { "-o5", "5" }, { "-o6", "6" }, { "-o7", "7" }, { "-o7 -zm1-9", "8" } };
        public Dictionary<string, string> CompressNums
        {
            get => _CompressNums;
        }

        private int _CompressIndexZopfli;
        public int CompressIndexZopfli
        {
            get => _CompressIndexZopfli;
            set
            {
                _CompressIndexZopfli = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CompressIndexZopfli));
            }
        }

        private readonly Dictionary<string, string> _CompressNumsZopfli = new Dictionary<string, string>() { { "-y", "1" }, { "--filters=0me -y", "2" }, { "--filters=01234me -y", "3" }, { "--filters=01234mepb -y", "4" }, { "--lossy_transparent --lossy_8bit --iterations=20 --filters=0me -y", "5" }, { "--lossy_transparent --lossy_8bit --iterations=20 --filters=01234me -y", "6" }, { "--lossy_transparent --lossy_8bit --iterations=20 --filters=01234mepb -y", "7" } };
        public Dictionary<string, string> CompressNumsZopfli
        {
            get => _CompressNumsZopfli;
        }

        private CompressType _CompressSelect;
        public CompressType CompressSelect
        {
            get => _CompressSelect;
            set
            {
                _CompressSelect = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CompressSelect));
            }
        }

        public HotKeySettings HotKeySettings { get; set; }

        public int TabNumber { get; set; }

        public MainInstance OwnerInstance { get; set; }

        private bool _TopMost = false;
        public bool TopMost
        {
            get => _TopMost;
            set
            {
                _TopMost = value;
                RaisePropertyChanged();
                MainWindow.GetMainWindowDataContext().RaisePropertyChanged(nameof(TopMost));
            }
        }

        private bool? _EnableViewKeyBind;
        public bool? EnableViewKeyBind
        {
            get => _EnableViewKeyBind;
            set
            {
                _EnableViewKeyBind = value;
                RaisePropertyChanged();
            }
        }

        private bool? _EnableViewKeyBindingViewer;
        public bool? EnableViewKeyBindingViewer
        {
            get => _EnableViewKeyBindingViewer;
            set
            {
                _EnableViewKeyBindingViewer = value;
                RaisePropertyChanged();
            }
        }

        private bool? _EnableViewSaveFormat;
        public bool? EnableViewSaveFormat
        {
            get => _EnableViewSaveFormat;
            set
            {
                _EnableViewSaveFormat = value;
                RaisePropertyChanged();
            }
        }

        private bool? _EnableViewSaveDir;
        public bool? EnableViewSaveDir
        {
            get => _EnableViewSaveDir;
            set
            {
                _EnableViewSaveDir = value;
                RaisePropertyChanged();
            }
        }

        private bool? _EnableViewOverlay;
        public bool? EnableViewOverlay
        {
            get => _EnableViewOverlay;
            set
            {
                _EnableViewOverlay = value;
                RaisePropertyChanged();
            }
        }

        private bool? _EnableViewCapture;
        public bool? EnableViewCapture
        {
            get => _EnableViewCapture;
            set
            {
                _EnableViewCapture = value;
                RaisePropertyChanged();
            }
        }

        private bool? _EnableViewOther;
        public bool? EnableViewOther
        {
            get => _EnableViewOther;
            set
            {
                _EnableViewOther = value;
                RaisePropertyChanged();
            }
        }

        private bool? _EnableOptionFileNameOption;
        public bool? EnableOptionFileNameOption
        {
            get => _EnableOptionFileNameOption;
            set
            {
                _EnableOptionFileNameOption = value;
                RaisePropertyChanged();
            }
        }


        public System.Windows.Controls.Orientation DefaultOrientation { get; set; } = System.Windows.Controls.Orientation.Horizontal;

        public string FileNameDirRegexConvert(string origStr, string dirName)
        {
            string tmpStr = origStr;
            if (origStr.Contains(WordDir))
            {
                tmpStr = tmpStr.Replace(WordDir, dirName);
            }
            return tmpStr;
        }

        private string CreateSampleFileName()
        {
            _SampleFileName = GetSampleFileName(NumberCount);
            RaisePropertyChanged(nameof(SampleFileName));
            return _SampleFileName;
        }

        public string GetSampleFileName(int srcNum)
        {
            string numberFormat = "{0:D" + NumberDigits + "}";
            string formatedNumber = string.Format(numberFormat, srcNum);
            var convResult = MainProcess.doNameRegexConverts(FileName, Directory, false, IntPtr.Zero, this);
            string replacedFileName = convResult.Item1;
            string tmpSampleFileName;
            if (EnableNumber == true)
            {
                tmpSampleFileName = replacedFileName + CountConju + formatedNumber;
            }
            else
            {
                tmpSampleFileName = replacedFileName;
            }

            //string dirName;
            //try
            //{
            //    dirName = System.IO.Path.GetFileNameWithoutExtension(Directory);
            //}
            //catch
            //{
            //    dirName = string.Empty;
            //}
            //tmpSampleFileName = FileNameDirRegexConvert(tmpSampleFileName, dirName);
            //tmpSampleFileName = System.Text.RegularExpressions.Regex.Replace(tmpSampleFileName, "[<>]", string.Empty);
            return tmpSampleFileName + "." + SaveFormats[(SaveFormat)SaveFormatIndex];
        }

        public string GetSampleFileName(int srcNum, string fileName)
        {
            string numberFormat = "{0:D" + NumberDigits + "}";
            string formatedNumber = string.Format(numberFormat, srcNum);
            string tmpSampleFileName;
            if (EnableNumber == true)
            {
                tmpSampleFileName = fileName + CountConju + formatedNumber;
            }
            else
            {
                tmpSampleFileName = fileName;
            }
            return tmpSampleFileName + "." + SaveFormats[(SaveFormat)SaveFormatIndex];
        }

        public class WindowTitleReplaceSetting
        {
            public string pattern { get; set; } = string.Empty;
            public string replacement { get; set; } = string.Empty;
            public bool? isRegex { get; set; } = false;
        }

        private System.Collections.ObjectModel.ObservableCollection<WindowTitleReplaceSetting> _WindowTitleReplaceSettings = new System.Collections.ObjectModel.ObservableCollection<WindowTitleReplaceSetting>();
        public System.Collections.ObjectModel.ObservableCollection<WindowTitleReplaceSetting> WindowTitleReplaceSettings
        {
            get => _WindowTitleReplaceSettings;
            set
            {
                _WindowTitleReplaceSettings = value;
                RaisePropertyChanged(nameof(WindowTitleReplaceSettings));
            }
        }

        private bool? _EnableIpHostTrans;
        public bool? EnableIpHostTrans
        {
            get => _EnableIpHostTrans;
            set
            {
                _EnableIpHostTrans = value;
                RaisePropertyChanged();
            }
        }

        private string _IpHostTransHosts;
        public string IpHostTransHosts
        {
            get => _IpHostTransHosts;
            set
            {
                var preValue = _IpHostTransHosts;
                _IpHostTransHosts = value;
                RaisePropertyChanged();
                if (value != preValue)
                {
                    ReadHosts();
                }
            }
        }

        private int _IpHostTransSettingIndex;
        public int IpHostTransSettingIndex
        {
            get => _IpHostTransSettingIndex;
            set
            {
                _IpHostTransSettingIndex = value;
                RaisePropertyChanged();
            }
        }

        public Dictionary<string, List<string>> HostsData { get; set; } = new Dictionary<string, List<string>>();

        public void ReadHosts()
        {
            try
            {
                if (!string.IsNullOrEmpty(IpHostTransHosts) && System.IO.File.Exists(IpHostTransHosts))
                {
                    HostsData = new Dictionary<string, List<string>>();
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(IpHostTransHosts))
                    {
                        while (sr.Peek() != -1)
                        {
                            string hostsLine = sr.ReadLine();
                            hostsLine = System.Text.RegularExpressions.Regex.Replace(hostsLine, "#.*", string.Empty);
                            hostsLine = hostsLine.Trim();
                            if (!string.IsNullOrEmpty(hostsLine))
                            {
                                var splited = hostsLine.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                                if (splited.Length >= 2)
                                {
                                    var keyIp = splited[0];
                                    var names = splited.Skip(1).ToList();
                                    names.Sort((a, b) =>
                                    {
                                        if (!a.Contains(".")) { return -1; }
                                        if (!b.Contains(".")) { return 1; }
                                        return a.Length - b.Length;
                                    });
                                    if (HostsData.ContainsKey(keyIp))
                                    {
                                        HostsData[keyIp].AddRange(names);
                                        HostsData[keyIp].Sort((a, b) =>
                                        {
                                            if (!a.Contains(".")) { return -1; }
                                            if (!b.Contains(".")) { return 1; }
                                            return a.Length - b.Length;
                                        });
                                    }
                                    else
                                    {
                                        HostsData.Add(keyIp, names);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        private readonly Dictionary<string, string> _IpHostTransSetting = new Dictionary<string, string>() {
            { "ホスト名のみ", "<hostname>" },
            { "ホスト名+IPアドレス", "<hostname>(<IPAddress>)" }
        };
        public Dictionary<string, string> IpHostTransSetting
        {
            get => _IpHostTransSetting;
        }

        private bool? _EnableAutoContinueCount;
        public bool? EnableAutoContinueCount
        {
            get => _EnableAutoContinueCount;
            set
            {
                _EnableAutoContinueCount = value;
                RaisePropertyChanged();
            }
        }

        private bool? _EnableSetFileNameOnCapture;
        public bool? EnableSetFileNameOnCapture
        {
            get => _EnableSetFileNameOnCapture;
            set
            {
                _EnableSetFileNameOnCapture = value;
                RaisePropertyChanged();
            }
        }

        private string _FileNameComboSourceText;
        public string FileNameComboSourceText
        {
            get => _FileNameComboSourceText;
            set
            {
                _FileNameComboSourceText = value;
                CreateFileNameComboSource(_FileNameComboSourceText);
                RaisePropertyChanged();
            }
        }


        private System.Collections.ObjectModel.ObservableCollection<string> _FileNameComboSource = new System.Collections.ObjectModel.ObservableCollection<string>();
        public System.Collections.ObjectModel.ObservableCollection<string> FileNameComboSource
        {
            get => _FileNameComboSource;
            set
            {
                _FileNameComboSource = value;
                RaisePropertyChanged();
            }
        }

        public void CreateFileNameComboSource(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                var lines = source.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                FileNameComboSource = new System.Collections.ObjectModel.ObservableCollection<string>(lines);
            }
        }


        private string SettingFile = "setting.xml";
        const string SettingPreName = "setting";
        const string SettingExtension = ".xml";
        const string DefaultSettingName = "DefaultSetting.xml";
        public Settings(int tabNumber)
        {
            TabNumber = tabNumber;
            if (tabNumber == -1)
            {
                SettingFile = DefaultSettingName;
            }
            else if (tabNumber > 0)
            {
                SettingFile = SettingPreName + tabNumber.ToString() + SettingExtension;
            }
            string fullPath = AppDomain.CurrentDomain.BaseDirectory + SettingFile;
            if (System.IO.File.Exists(fullPath))
            {
                var xml = XDocument.Load(fullPath);
                XElement tmpel = xml.Element("Settings");
                KeysConverter keysConverter = new KeysConverter();
                Keys GetKeyFromString(string name, Keys defaultKey)
                {
                    try
                    {
                        return (Keys)keysConverter.ConvertFromString(tmpel.Element(name).Value);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        return defaultKey;
                    }
                }
                Key = GetKeyFromString(nameof(Key), Keys.Q);
                PreKey = GetKeyFromString(nameof(PreKey), Keys.Control);
                ScreenKey = GetKeyFromString(nameof(ScreenKey), Keys.Q);
                ScreenPreKey = GetKeyFromString(nameof(ScreenPreKey), Keys.Alt);
                SelectKey = GetKeyFromString(nameof(SelectKey), Keys.S);
                SelectPreKey = GetKeyFromString(nameof(SelectPreKey), Keys.Alt);

                string GetStringFromSettingFile(string name, string defaultString)
                {
                    string tmpStr = tmpel.Element(name)?.Value;
                    if (string.IsNullOrEmpty(tmpStr))
                    {
                        return defaultString;
                    }
                    else
                    {
                        return tmpStr;
                    }
                }
                Directory = GetStringFromSettingFile(nameof(Directory), defaultDirectory);
                FileName = GetStringFromSettingFile(nameof(FileName), "Capture");
                DigitsText = GetStringFromSettingFile(nameof(DigitsText), "3");
                OverlayTime = GetStringFromSettingFile(nameof(OverlayTime), "3000");
                CountConju = GetStringFromSettingFile(nameof(CountConju), "_");
                IpHostTransHosts = GetStringFromSettingFile(nameof(IpHostTransHosts), etcHosts);
                FileNameComboSourceText = GetStringFromSettingFile(nameof(FileNameComboSourceText), "");

                string tmpWindowTitleReplaceSettings = GetStringFromSettingFile(nameof(WindowTitleReplaceSettings), null);
                if (!string.IsNullOrEmpty(tmpWindowTitleReplaceSettings))
                {
                    try
                    {
                        System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(System.Collections.ObjectModel.ObservableCollection<WindowTitleReplaceSetting>));
                        WindowTitleReplaceSettings = xmlSerializer.Deserialize(new System.IO.StringReader(tmpWindowTitleReplaceSettings)) as System.Collections.ObjectModel.ObservableCollection<WindowTitleReplaceSetting>;
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                if (WindowTitleReplaceSettings == null)
                {
                    WindowTitleReplaceSettings = new System.Collections.ObjectModel.ObservableCollection<WindowTitleReplaceSetting>() { new WindowTitleReplaceSetting() { pattern = "", replacement = "", isRegex = false } };
                }

                int GetIntFromString(string name, int defaultInt)
                {
                    if (int.TryParse(tmpel.Element(name)?.Value, out int result))
                    {
                        return result;
                    }
                    else
                    {
                        return defaultInt;
                    }
                }
                NumberCount = GetIntFromString(nameof(NumberCount), 0);
                SaveFormatIndex = GetIntFromString(nameof(SaveFormatIndex), 0);
                PositionIndex = GetIntFromString(nameof(PositionIndex), 0);
                OverlayX = GetIntFromString(nameof(OverlayX), 200);
                OverlayY = GetIntFromString(nameof(OverlayY), 150);
                PixelFormatIndex = GetIntFromString(nameof(PixelFormatIndex), 0);
                CaptureModeIndex = GetIntFromString(nameof(CaptureModeIndex), 1);
                CompressIndex = GetIntFromString(nameof(CompressIndex), 0);
                CompressIndexZopfli = GetIntFromString(nameof(CompressIndexZopfli), 0);
                IpHostTransSettingIndex = GetIntFromString(nameof(IpHostTransSettingIndex), 0);

                bool GetBoolFromString(string name, bool defaultBool)
                {
                    if (bool.TryParse(tmpel.Element(name)?.Value, out bool result))
                    {
                        return result;
                    }
                    else
                    {
                        return defaultBool;
                    }
                }
                EnableNumber = GetBoolFromString(nameof(EnableNumber), true);
                EnableTray = GetBoolFromString(nameof(EnableTray), true);
                EnableOverlay = GetBoolFromString(nameof(EnableOverlay), true);
                EnableAero = GetBoolFromString(nameof(EnableAero), true);
                EnableAutoSave = GetBoolFromString(nameof(EnableAutoSave), true);
                EnableCursor = GetBoolFromString(nameof(EnableCursor), false);
                EnableChangeCapture = GetBoolFromString(nameof(EnableChangeCapture), false);
                EnableSetArrow = GetBoolFromString(nameof(EnableSetArrow), false);
                EnableVisibilityControl = GetBoolFromString(nameof(EnableVisibilityControl), true);
                TopMost = GetBoolFromString(nameof(TopMost), false);
                EnableViewKeyBind = GetBoolFromString(nameof(EnableViewKeyBind), true);
                EnableViewKeyBindingViewer = GetBoolFromString(nameof(EnableViewKeyBindingViewer), false);
                EnableViewSaveFormat = GetBoolFromString(nameof(EnableViewSaveFormat), false);
                EnableViewSaveDir = GetBoolFromString(nameof(EnableViewSaveDir), true);
                EnableViewOverlay = GetBoolFromString(nameof(EnableViewOverlay), false);
                EnableViewCapture = GetBoolFromString(nameof(EnableViewCapture), false);
                EnableViewOther = GetBoolFromString(nameof(EnableViewOther), false);
                EnableOptionFileNameOption = GetBoolFromString(nameof(EnableOptionFileNameOption), false);
                WindowCaptureEnabled = GetBoolFromString(nameof(WindowCaptureEnabled), true);
                ScreenCaptureEnabled = GetBoolFromString(nameof(ScreenCaptureEnabled), true);
                SelectEnabled = GetBoolFromString(nameof(SelectEnabled), false);
                OverlayTabNameEnabled = GetBoolFromString(nameof(OverlayTabNameEnabled), true);
                OverlayFileNameEnabled = GetBoolFromString(nameof(OverlayFileNameEnabled), true);
                EnableIpHostTrans = GetBoolFromString(nameof(EnableIpHostTrans), true);
                EnableAutoContinueCount = GetBoolFromString(nameof(EnableAutoContinueCount), true);
                EnableSetFileNameOnCapture = GetBoolFromString(nameof(EnableSetFileNameOnCapture), false);

                string[] GetArrayFromString(string name, string[] defaultList, string separator)
                {
                    string tmpStr = tmpel.Element(name)?.Value;
                    if (string.IsNullOrEmpty(tmpStr))
                    {
                        return defaultList;
                    }
                    else
                    {
                        return tmpStr.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                    }
                }
                string[] keys = GetArrayFromString("FavDirKeys", new string[0], "\n");
                string[] values = GetArrayFromString("FavDirValues", new string[0], "\n");
                _FavDirs = keys.ToDictionary(key => key, key => values[Array.IndexOf(keys, key)]);
                _FavDirNames = new System.Collections.ObjectModel.ObservableCollection<string>(values);

                T GetEnumFromString<T>(string name, T defaultEnum)
                {
                    try
                    {
                        string tmpStr = tmpel.Element(name)?.Value;
                        return (T)Enum.Parse(typeof(T), tmpStr);
                    }
                    catch
                    {
                        return defaultEnum;
                    }
                }
                CompressSelect = GetEnumFromString(nameof(CompressSelect), CompressType.None);
            }
            else
            {
                ResetSettings();
            }
        }

        public void SaveSettings()
        {
            System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(System.Collections.ObjectModel.ObservableCollection<WindowTitleReplaceSetting>));
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            xmlSerializer.Serialize(stringWriter, WindowTitleReplaceSettings);
            string serialized = stringWriter.ToString();

            XElement tmpel = new XElement("Settings",
                new XElement(nameof(Key), Key.ToString()),
                new XElement(nameof(PreKey), PreKey.ToString()),
                new XElement(nameof(Directory), Directory),
                new XElement(nameof(FileName), FileName),
                new XElement(nameof(EnableNumber), EnableNumber.ToString()),
                new XElement(nameof(DigitsText), DigitsText),
                new XElement(nameof(NumberCount), NumberCount.ToString()),
                new XElement(nameof(SaveFormatIndex), SaveFormatIndex.ToString()),
                new XElement(nameof(EnableTray), EnableTray.ToString()),
                new XElement(nameof(OverlayTime), OverlayTime),
                new XElement(nameof(PositionIndex), PositionIndex.ToString()),
                new XElement(nameof(ScreenKey), ScreenKey.ToString()),
                new XElement(nameof(ScreenPreKey), ScreenPreKey.ToString()),
                new XElement(nameof(EnableAero), EnableAero.ToString()),
                new XElement(nameof(EnableAutoSave), EnableAutoSave.ToString()),
                new XElement(nameof(SelectKey), SelectKey.ToString()),
                new XElement(nameof(SelectPreKey), SelectPreKey.ToString()),
                new XElement(nameof(EnableCursor), EnableCursor.ToString()),
                new XElement(nameof(EnableChangeCapture), EnableChangeCapture.ToString()),
                new XElement(nameof(OverlayX), OverlayX.ToString()),
                new XElement(nameof(OverlayY), OverlayY.ToString()),
                new XElement(nameof(EnableSetArrow), EnableSetArrow.ToString()),
                new XElement(nameof(PixelFormatIndex), PixelFormatIndex.ToString()),
                new XElement(nameof(CaptureModeIndex), CaptureModeIndex.ToString()),
                new XElement(nameof(EnableVisibilityControl), EnableVisibilityControl.ToString()),
                new XElement(nameof(CountConju), CountConju),
                new XElement("FavDirKeys", ToStringFavDirKeys()),
                new XElement("FavDirValues", ToStringFavDirValues()),
                new XElement(nameof(CompressIndex), CompressIndex.ToString()),
                new XElement(nameof(CompressIndexZopfli), CompressIndexZopfli.ToString()),
                new XElement(nameof(CompressSelect), Enum.GetName(typeof(CompressType), CompressSelect)),
                new XElement(nameof(TopMost), TopMost.ToString()),
                new XElement(nameof(EnableViewKeyBind), EnableViewKeyBind.ToString()),
                new XElement(nameof(EnableViewKeyBindingViewer), EnableViewKeyBindingViewer.ToString()),
                new XElement(nameof(EnableViewSaveFormat), EnableViewSaveFormat.ToString()),
                new XElement(nameof(EnableViewSaveDir), EnableViewSaveDir.ToString()),
                new XElement(nameof(EnableViewOverlay), EnableViewOverlay.ToString()),
                new XElement(nameof(EnableViewCapture), EnableViewCapture.ToString()),
                new XElement(nameof(EnableViewOther), EnableViewOther.ToString()),
                new XElement(nameof(EnableOptionFileNameOption), EnableOptionFileNameOption.ToString()),
                new XElement(nameof(WindowCaptureEnabled), WindowCaptureEnabled.ToString()),
                new XElement(nameof(ScreenCaptureEnabled), ScreenCaptureEnabled.ToString()),
                new XElement(nameof(SelectEnabled), SelectEnabled.ToString()),
                new XElement(nameof(OverlayTabNameEnabled), OverlayTabNameEnabled.ToString()),
                new XElement(nameof(OverlayFileNameEnabled), OverlayFileNameEnabled.ToString()),
                new XElement(nameof(WindowTitleReplaceSettings), serialized),
                new XElement(nameof(IpHostTransSettingIndex), IpHostTransSettingIndex.ToString()),
                new XElement(nameof(EnableIpHostTrans), EnableIpHostTrans.ToString()),
                new XElement(nameof(IpHostTransHosts), IpHostTransHosts.ToString()),
                new XElement(nameof(EnableAutoContinueCount), EnableAutoContinueCount.ToString()),
                new XElement(nameof(FileNameComboSourceText), FileNameComboSourceText.ToString()),
                new XElement(nameof(EnableSetFileNameOnCapture), EnableSetFileNameOnCapture.ToString())
                );
            XDocument xml = new XDocument(new XDeclaration("1.0", "utf-8", "true"), tmpel);
            xml.Save(AppDomain.CurrentDomain.BaseDirectory + SettingFile);
        }

        public void ResetSettings()
        {
            Key = Keys.Q;
            PreKey = Keys.Control;
            Directory = defaultDirectory;
            FileName = "Capture";
            EnableNumber = true;
            DigitsText = "3";
            NumberCount = 0;
            SaveFormatIndex = 0;
            EnableTray = true;
            EnableOverlay = true;
            OverlayTime = "3000";
            ScreenKey = Keys.Q;
            ScreenPreKey = Keys.Alt;
            SelectKey = Keys.S;
            SelectPreKey = Keys.Alt;
            EnableCursor = false;
            EnableChangeCapture = false;
            OverlayX = 200;
            OverlayY = 150;
            EnableSetArrow = false;
            PixelFormatIndex = 1;
            CaptureModeIndex = 1;
            EnableVisibilityControl = true;
            CountConju = "_";
            CompressIndex = 0;
            EnableAutoSave = true;
            EnableAero = true;
            TopMost = false;
            EnableViewKeyBind = true;
            EnableViewKeyBindingViewer = false;
            EnableViewSaveFormat = false;
            EnableViewSaveDir = true;
            EnableViewOverlay = false;
            EnableViewCapture = false;
            EnableViewOther = false;
            EnableOptionFileNameOption = false;
            WindowCaptureEnabled = true;
            ScreenCaptureEnabled = true;
            SelectEnabled = false;
            OverlayTabNameEnabled = true;
            OverlayFileNameEnabled = true;
            WindowTitleReplaceSettings = new System.Collections.ObjectModel.ObservableCollection<WindowTitleReplaceSetting>() { new WindowTitleReplaceSetting() { pattern = "", replacement = "", isRegex = false } };
            IpHostTransSettingIndex = 0;
            EnableIpHostTrans = false;
            IpHostTransHosts = etcHosts;
            EnableAutoContinueCount = true;
            FileNameComboSourceText = "";
            EnableSetFileNameOnCapture = false;
        }
    }

    public enum SaveFormat
    {
        PNG, JPG
    }

    public struct PositionSet
    {
        public System.Windows.HorizontalAlignment HorizontalAlignment { get; }
        public System.Windows.VerticalAlignment VerticalAlignment { get; }
        public PositionSet(System.Windows.HorizontalAlignment horizontalAlignment, System.Windows.VerticalAlignment verticalAlignment)
        {
            this.HorizontalAlignment = horizontalAlignment;
            this.VerticalAlignment = verticalAlignment;
        }
    }

    public enum CompressType
    {
        None, Optipng, Zopfli
    }
}
