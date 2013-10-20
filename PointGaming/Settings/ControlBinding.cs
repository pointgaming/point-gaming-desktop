using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Newtonsoft.Json;

namespace PointGaming.Settings
{
    [Flags]
    public enum PermittedControlBinding
    {
        KeyboardKeys = 1,
        MouseButtons = 2,
        KeyboardKeysAndMouseButtons = 3,
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ControlBinding
    {
        public const MouseButton MouseButtonNone = (MouseButton)(-1);
        public const Key KeyboardKeyNone = (Key)(-1);

        private Key _KeyboardKey = KeyboardKeyNone;
        [JsonProperty]
        public Key KeyboardKey { get { return _KeyboardKey; } set { _KeyboardKey = value; } }

        private MouseButton _MButton = MouseButtonNone;
        [JsonProperty]
        public MouseButton MButton { get { return _MButton; } set { _MButton = value; } }

        public bool IsKeyboardKeySet { get { return KeyboardKey != KeyboardKeyNone; } }
        public bool IsMouseButtonSet { get { return MButton != MouseButtonNone; } }

        public bool IsDown
        {
            get
            {
                bool result = false;
                if (IsKeyboardKeySet)
                {
                    result = System.Windows.Input.Keyboard.IsKeyDown(KeyboardKey);
                }
                else if (IsMouseButtonSet)
                {
                    result = ControlState.IsMouseButtonDown(MButton);
                }
                return result;
            }
        }

        public override string ToString()
        {
            string result = null;
            if (IsKeyboardKeySet)
                result = "" + KeyboardKey;

            if (IsMouseButtonSet)
            {
                if (result != null)
                    result += " & ";
                result += MButton;
            }

            if (result == null)
                result = "(not bound)";

            return result;
        }

        public override bool Equals(object obj)
        {
            var o = obj as ControlBinding;
            if (o == null)
                return false;
            if (KeyboardKey != o.KeyboardKey)
                return false;
            if (MButton != o.MButton)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            int hash = 0;
            if (IsKeyboardKeySet)
                hash = KeyboardKey.GetHashCode();
            if (IsMouseButtonSet)
                hash ^= MButton.GetHashCode();
            return hash;
        }
    }
}
