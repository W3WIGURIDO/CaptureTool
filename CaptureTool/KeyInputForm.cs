using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaptureTool
{
    public partial class KeyInputForm : Form
    {
        private Keys _Key;
        public bool PreMode { get; set; } = false;
        public Keys Key { get => _Key; }

        public KeyInputForm()
        {
            InitializeComponent();
        }

        private void KeyInputForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (PreMode)
            {
                if (CheckModificationKey(e.Modifiers))
                {
                    _Key = e.Modifiers;
                    Close();
                }
            }
            else
            {
                if (!CheckModificationKey(e.Modifiers))
                {
                    _Key = e.KeyCode;
                    Close();
                }
            }
        }

        private bool CheckModificationKey(Keys key)
        {
            if (key == Keys.LControlKey || key == Keys.RControlKey || key == Keys.Control || key == Keys.ControlKey || key == Keys.Alt ||
                key == Keys.LShiftKey || key == Keys.RShiftKey || key == Keys.ShiftKey || key == Keys.Shift)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
