using UnityEngine.InputSystem;

namespace QCommonLib
{
    internal static class QKeyboard
    {
        private static Keyboard _Keyboard;
        private static UnityEngine.InputSystem.Controls.ButtonControl _Alt, _Shift, _Control;

        internal static void Init()
        {
            _Keyboard   = InputSystem.GetDevice<Keyboard>();
            _Alt        = _Keyboard.altKey;
            _Control    = _Keyboard.ctrlKey;
            _Shift      = _Keyboard.shiftKey;
        }

        internal static bool Alt => _Alt.isPressed;

        internal static bool Control => _Control.isPressed;

        internal static bool Shift => _Shift.isPressed;
    }
}
