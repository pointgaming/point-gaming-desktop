using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PointGaming
{
    public class InputBinding
    {
        public Key? KeyboardKey { get; set; }
        public MouseButton? MButton { get; set; }

        public bool IsDown
        {
            get
            {
                bool result = false;
                if (KeyboardKey.HasValue)
                    result = System.Windows.Input.Keyboard.IsKeyDown(KeyboardKey.Value);
                else if (MButton.HasValue)
                {
                    switch (MButton.Value)
                    {
                        case MouseButton.Left:
                            result = Mouse.LeftButton == MouseButtonState.Pressed;
                            break;
                        case MouseButton.Right:
                            result = Mouse.RightButton == MouseButtonState.Pressed;
                            break;
                        case MouseButton.Middle:
                            result = Mouse.MiddleButton == MouseButtonState.Pressed;
                            break;
                        case MouseButton.XButton1:
                            result = Mouse.XButton1 == MouseButtonState.Pressed;
                            break;
                        case MouseButton.XButton2:
                            result = Mouse.XButton2 == MouseButtonState.Pressed;
                            break;
                    }
                }
                return result;
            }
        }

        public override string ToString()
        {
            string result = null;
            if (KeyboardKey.HasValue)
                result = "" + KeyboardKey.Value;

            if (MButton.HasValue)
            {
                if (result != null)
                    result += " & ";
                result += MButton.Value;
            }

            if (result == null)
                result = "(not bound)";

            return result;
        }

        public override bool Equals(object obj)
        {
            var o = obj as InputBinding;
            if (o == null)
                return false;
            return KeyboardKey == o.KeyboardKey && MButton == o.MButton;
        }
        public override int GetHashCode()
        {
            int hash = 0;
            if (KeyboardKey != null)
                hash = KeyboardKey.Value.GetHashCode();
            if (MButton != null)
                hash ^= MButton.Value.GetHashCode();
            return hash;
        }
    }

    [Flags]
    public enum PermittedInputBindings
    {
        KeyboardKeys = 1,
        MouseButtons = 2,
        KeyboardKeysAndMouseButtons = 3,
    }

    public partial class KeySelectDialog : Window
    {
        public InputBinding Binding;

        public KeySelectDialog()
        {
            InitializeComponent();
        }

        public string Message { get { return textBoxMessage.Text; } set { textBoxMessage.Text = value; } }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public static InputBinding Show(Window owner, string title, string message, PermittedInputBindings permits)
        {
            var md = new KeySelectDialog
            {
                Owner = owner,
                Title = title,
                Message = message,
            };
            if (permits.HasFlag(PermittedInputBindings.KeyboardKeys))
                md.PreviewKeyUp += md.MyPreviewKeyUp;
            if (permits.HasFlag(PermittedInputBindings.MouseButtons))
                md.textBoxMessage.PreviewMouseUp += md.textBoxMessage_PreviewMouseUp;
            md.ShowDialog();
            return md.Binding;
        }

        void textBoxMessage_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Binding = new InputBinding { MButton = e.ChangedButton, };
            DialogResult = true;
            Close();
            e.Handled = true;
        }


        private void MyPreviewKeyUp(object sender, KeyEventArgs e)
        {
            Binding = new InputBinding { KeyboardKey = e.Key, };
            DialogResult = true;
            Close();
            e.Handled = true;
        }
    }
}
