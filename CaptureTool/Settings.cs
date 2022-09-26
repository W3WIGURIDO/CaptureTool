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

namespace CaptureTool
{
    public class Settings : INotifyPropertyChanged
    {
        private string defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\Capture";
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private Keys _Key;
        public Keys Key
        {
            get => _Key;
            set
            {
                _Key = value;
                _KeyText = Enum.GetName(typeof(Keys), value);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(KeyText));
            }
        }

        private Keys _PreKey;
        public Keys PreKey
        {
            get => _PreKey;
            set
            {
                _PreKey = value;
                _PreKeyText = Enum.GetName(typeof(Keys), value);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(PreKeyText));
            }
        }

        private string _KeyText;
        public string KeyText
        {
            get => _KeyText;
        }

        private string _PreKeyText;
        public string PreKeyText
        {
            get => _PreKeyText;
        }

        private Keys _ScreenKey;
        public Keys ScreenKey
        {
            get => _ScreenKey;
            set
            {
                _ScreenKey = value;
                _ScreenKeyText = Enum.GetName(typeof(Keys), value);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ScreenKeyText));
            }
        }

        private Keys _ScreenPreKey;
        public Keys ScreenPreKey
        {
            get => _ScreenPreKey;
            set
            {
                _ScreenPreKey = value;
                _ScreenPreKeyText = Enum.GetName(typeof(Keys), value);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ScreenPreKeyText));
            }
        }

        private string _ScreenKeyText;
        public string ScreenKeyText
        {
            get => _ScreenKeyText;
        }

        private string _ScreenPreKeyText;
        public string ScreenPreKeyText
        {
            get => _ScreenPreKeyText;
        }

        private Keys _SelectKey;
        public Keys SelectKey
        {
            get => _SelectKey;
            set
            {
                _SelectKey = value;
                _SelectKeyText = Enum.GetName(typeof(Keys), value);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SelectKeyText));
            }
        }

        private Keys _SelectPreKey;
        public Keys SelectPreKey
        {
            get => _SelectPreKey;
            set
            {
                _SelectPreKey = value;
                _SelectPreKeyText = Enum.GetName(typeof(Keys), value);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SelectPreKeyText));
            }
        }

        private string _SelectKeyText;
        public string SelectKeyText
        {
            get => _SelectKeyText;
        }

        private string _SelectPreKeyText;
        public string SelectPreKeyText
        {
            get => _SelectPreKeyText;
        }

        private string _Directory;
        public string Directory
        {
            get => _Directory;
            set
            {
                _Directory = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Directory));
            }
        }

        private string _FileName;
        public string FileName
        {
            get => _FileName;
            set
            {
                //string tmpValue = System.Text.RegularExpressions.Regex.Replace(value, "[\\\\/:*?\"<>|\r\n]", string.Empty);
                string tmpValue = System.Text.RegularExpressions.Regex.Replace(value, "[/:*?\"<>|\r\n]", string.Empty);
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
                RaisePropertyChanged(nameof(OverlayTime));
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
                RaisePropertyChanged(nameof(PositionIndex));
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

        private bool? _EnableAero;
        public bool? EnableAero
        {
            get => _EnableAero;
            set
            {
                _EnableAero = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(EnableAero));
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
                RaisePropertyChanged(nameof(EnableAutoSave));
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
                RaisePropertyChanged(nameof(WindowOpacity));
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
                RaisePropertyChanged(nameof(EnableCursor));
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

        private RoutedCommand _RefFolderCom = new RoutedCommand();
        public RoutedCommand RefFolderCom
        {
            get => _RefFolderCom;
            set
            {
                _RefFolderCom = value;
            }
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

        private string CreateSampleFileName()
        {
            string numberFormat = "{0:D" + NumberDigits + "}";
            string formatedNumber = string.Format(numberFormat, NumberCount);
            string tmpSampleFileName;
            if (EnableNumber == true)
            {
                tmpSampleFileName = FileName + formatedNumber;
            }
            else
            {
                tmpSampleFileName = FileName;
            }
            _SampleFileName = tmpSampleFileName + "." + SaveFormats[(SaveFormat)SaveFormatIndex];
            RaisePropertyChanged(nameof(SampleFileName));
            return tmpSampleFileName;
        }

        const string SettingFile = "setting.xml";
        public Settings()
        {
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
            }
            else
            {
                ResetSettings();
            }
        }

        public void SaveSettings()
        {
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
                new XElement(nameof(EnableVisibilityControl), EnableVisibilityControl.ToString())
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
            EnableCursor = false;
            EnableChangeCapture = false;
            OverlayX = 200;
            OverlayY = 150;
            EnableSetArrow = false;
            PixelFormatIndex = 0;
            CaptureModeIndex = 1;
            EnableVisibilityControl = true;
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
}
