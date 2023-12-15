using Game.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace QCommonLib
{
    internal class QKeyEventReaction
    {
        private readonly float _clickDelay = 0.1f;
        private static float _timeLastClicked;

        internal KeyCode m_Code;
        internal EventModifiers m_Modifiers;
        internal QKeyListenerContexts m_Context;
        public ToolBaseSystem m_Tool = null;

        internal QKeyEventReaction(KeyCode code, EventModifiers modifiers, QKeyListenerContexts context, ToolBaseSystem tool)
        {
            m_Code = code;
            m_Modifiers = modifiers;
            m_Context = context;
            m_Tool = tool;
        }

        internal bool Alt { get => (m_Modifiers & EventModifiers.Alt) > 0; }
        internal bool Control { get => (m_Modifiers & EventModifiers.Control) > 0; }
        internal bool Shift { get => (m_Modifiers & EventModifiers.Shift) > 0; }

        public override string ToString()
        {
            return $"QKeyEvent:{(Alt ? "A" : "")}{(Control ? "C" : "")}{(Shift ? "S" : "")}{(Alt || Control || Shift ? "-" : "")}{m_Code} ({m_Context})";
        }

        public bool IsPressed(bool useModifiers = true)
        {
            if (Event.current.type != EventType.KeyDown)
            {
                return false;
            }

            Event current = Event.current;

            if (m_Tool != null)
            {
                if (m_Context == QKeyListenerContexts.Default && QCommon.ActiveTool != QCommon.DefaultTool)
                {
                    return false;
                }
                if (m_Context == QKeyListenerContexts.InTool && QCommon.ActiveTool != m_Tool)
                {
                    return false;
                }
            }

            if (useModifiers && (Alt != current.alt || Control != current.control || Shift != current.shift))
            {
                return false;
            }

            if (m_Code != current.keyCode)
            {
                return false;
            }

            if (Time.time - _timeLastClicked > _clickDelay)
            {
                _timeLastClicked = Time.time;
                return true;
            }

            return false;
        }
    }

    internal class QKeyEventAction : QKeyEventReaction
    {
        internal QKeyListener.Trigger m_trigger;

        internal QKeyEventAction(KeyCode code, EventModifiers modifiers, QKeyListenerContexts context, ToolBaseSystem tool, QKeyListener.Trigger trigger) : base(code, modifiers, context, tool)
        {
            m_trigger = trigger;
        }
    }

    internal enum QKeyListenerContexts
    {
        Default,
        InTool
    }

    internal class QKeyListener : MonoBehaviour
    {
        private List<QKeyEventAction> actions = new List<QKeyEventAction>();

        private QKeyEventAction clicked = null;

        public delegate void Trigger();
        public ToolBaseSystem m_tool = null;

        internal void RegisterKeyAction(KeyCode code, EventModifiers modifiers, QKeyListenerContexts context, Trigger trigger)
        {
            RegisterKeyAction(new QKeyEventAction(code, modifiers, context, m_tool, trigger));
        }

        internal void RegisterKeyAction(QKeyEventAction key)
        {
            actions.Add(key);
        }

        public void OnGUI()
        {
            foreach (QKeyEventAction action in actions)
            {
                if (action.IsPressed())
                {
                    clicked = action;
                    break;
                }
            }
        }

        public void Update()
        {
            if (clicked != null)
            {
                clicked.m_trigger();
                clicked = null;
            }
        }
    }
}
