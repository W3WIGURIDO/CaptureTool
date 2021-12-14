using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaptureTool
{
    class HotKey : IDisposable
    {
        HotKeyForm form;
        public event EventHandler HotKeyPush;
        public string HotKeyName { get; set; }

        public HotKey(MOD_KEY modKey, Keys key)
        {
            form = new HotKeyForm(modKey, key, raiseHotKeyPush);
        }

        private void raiseHotKeyPush()
        {
            if (HotKeyPush != null)
            {
                HotKeyPush(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            form.Dispose();
        }

        private class HotKeyForm : Form
        {
            [DllImport("user32.dll")]
            extern static int RegisterHotKey(IntPtr HWnd, int ID, MOD_KEY MOD_KEY, Keys KEY);

            [DllImport("user32.dll")]
            extern static int UnregisterHotKey(IntPtr HWnd, int ID);

            const int WM_HOTKEY = 0x0312;
            int id;
            ThreadStart proc;

            public HotKeyForm(MOD_KEY modKey, Keys key, ThreadStart proc)
            {
                this.proc = proc;
                for (int i = 0x0000; i <= 0xbfff; i++)
                {
                    if (RegisterHotKey(this.Handle, i, modKey, key) != 0)
                    {
                        id = i;
                        break;
                    }
                }
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == WM_HOTKEY)
                {
                    if ((int)m.WParam == id)
                    {
                        proc();
                    }
                }
            }

            protected override void Dispose(bool disposing)
            {
                UnregisterHotKey(this.Handle, id);
                base.Dispose(disposing);
            }
        }
    }

    public enum MOD_KEY : int
    {
        ALT = 0x0001,
        CONTROL = 0x0002,
        SHIFT = 0x0004,
    }

    public static class EnumScan
    {
        public static Keys[] GetKeyModFlags(Keys key)
        {
            List<Keys> keyList = new List<Keys>();
            void FlagCheck(Keys flagKey)
            {
                if (key.HasFlag(flagKey))
                {
                    keyList.Add(flagKey);
                }
            }

            FlagCheck(Keys.LControlKey);
            FlagCheck(Keys.RControlKey);
            FlagCheck(Keys.Control);
            FlagCheck(Keys.ControlKey);
            FlagCheck(Keys.Alt);
            FlagCheck(Keys.LShiftKey);
            FlagCheck(Keys.RShiftKey);
            FlagCheck(Keys.ShiftKey);
            FlagCheck(Keys.Shift);

            return keyList.ToArray();
        }

        public static MOD_KEY FlagToMOD_KEY(Keys key)
        {
            Keys[] flagKeys = GetKeyModFlags(key);
            if (flagKeys.Contains(Keys.LControlKey) || flagKeys.Contains(Keys.RControlKey) || flagKeys.Contains(Keys.Control) || flagKeys.Contains(Keys.ControlKey))
            {
                return MOD_KEY.CONTROL;
            }
            else if (flagKeys.Contains(Keys.Alt))
            {
                return MOD_KEY.ALT;
            }
            else if (flagKeys.Contains(Keys.LShiftKey) || flagKeys.Contains(Keys.RShiftKey) || flagKeys.Contains(Keys.ShiftKey) || flagKeys.Contains(Keys.Shift))
            {
                return MOD_KEY.SHIFT;
            }
            else
            {
                return MOD_KEY.CONTROL;
            }
        }
    }
}
