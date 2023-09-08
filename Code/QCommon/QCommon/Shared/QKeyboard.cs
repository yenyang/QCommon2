using UnityEngine.InputSystem;

namespace QCommonLib
{
    internal static class QKeyboard
    {
        internal static Keyboard m_Keyboard;
        private static UnityEngine.InputSystem.Controls.ButtonControl _alt, _shift, _control;

        internal static void Init()
        {
            m_Keyboard = InputSystem.GetDevice<Keyboard>();
            _alt = m_Keyboard.altKey;
            _control = m_Keyboard.ctrlKey;
            _shift = m_Keyboard.shiftKey;
        }

        internal static bool Alt => _alt.isPressed;

        internal static bool Control => _control.isPressed;

        internal static bool Shift => _shift.isPressed;
    }
}
