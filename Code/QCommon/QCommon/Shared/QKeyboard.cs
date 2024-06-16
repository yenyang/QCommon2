using UnityEngine.InputSystem;

namespace QCommonLib
{
    internal static class QKeyboard
    {
        internal static Keyboard m_Keyboard;
        private static UnityEngine.InputSystem.Controls.ButtonControl _Alt, _Shift, _Control;

        internal static void Init()
        {
            m_Keyboard = InputSystem.GetDevice<Keyboard>();
            _Alt = m_Keyboard.altKey;
            _Control = m_Keyboard.ctrlKey;
            _Shift = m_Keyboard.shiftKey;
        }

        internal static bool Alt => _Alt.isPressed;

        internal static bool Control => _Control.isPressed;

        internal static bool Shift => _Shift.isPressed;
    }
}
