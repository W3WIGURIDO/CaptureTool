using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CaptureTool
{
    /// <summary>
    /// InputDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class InputDialog : Window
    {
        InputDialogContext inputDialogContext;
        string inputedText = null;
        public InputDialog(string windowName, string textMessage, string buttonMessage, string boxText)
        {
            inputDialogContext = new InputDialogContext(windowName, textMessage, buttonMessage, boxText);
            this.DataContext = inputDialogContext;
            InitializeComponent();
        }

        public new string ShowDialog()
        {
            base.ShowDialog();
            return inputedText;
        }

        private void dialogButton_Click(object sender, RoutedEventArgs e)
        {
            inputedText = inputDialogTextBox.Text;
            Close();
        }
    }


    public class InputDialogContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public InputDialogContext(string windowName, string textMessage, string buttonMessage, string boxText)
        {
            _WindowName = windowName;
            _TextMessage = textMessage;
            _ButtonMessage = buttonMessage;
            _BoxText = boxText;
        }

        private string _WindowName;
        public string WindowName
        {
            get => _WindowName;
            set
            {
                if (value != null)
                {
                    _WindowName = value;
                }
            }
        }

        private string _TextMessage;
        public string TextMessage
        {
            get => _TextMessage;
            set
            {
                if (value != null)
                {
                    _TextMessage = value;
                }
            }
        }

        private string _ButtonMessage;
        public string ButtonMessage
        {
            get => _ButtonMessage;
            set
            {
                if (value != null)
                {
                    _ButtonMessage = value;
                }
            }
        }

        private string _BoxText;
        public string BoxText
        {
            get => _BoxText;
            set
            {
                if (value != null)
                {
                    _BoxText = value;
                }
            }
        }
    }
}
