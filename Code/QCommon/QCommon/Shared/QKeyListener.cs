using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using Game.Input;
using Game.Prefabs;
using Game.Tools;
using System;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace QCommonLib
{
    internal class QKeyEvent
    {
        internal KeyCode m_code;
        internal EventModifiers m_modifiers;
        internal QKeyListerContexts m_context;
        internal QKeyListener.Trigger m_trigger;

        internal QKeyEvent(KeyCode code, EventModifiers modifiers, QKeyListerContexts context, QKeyListener.Trigger trigger)
        {
            m_code = code;
            m_modifiers = modifiers;
            m_context = context;
            m_trigger = trigger;
        }

        internal bool Alt { get => (m_modifiers & EventModifiers.Alt) > 0; }
        internal bool Control { get => (m_modifiers & EventModifiers.Control) > 0; }
        internal bool Shift { get => (m_modifiers & EventModifiers.Shift) > 0; }

        public override string ToString()
        {
            return $"QKeyEvent:{(Alt ? "A" : "")}{(Control ? "C" : "")}{(Shift ? "S" : "")}{(Alt || Control || Shift ? "-" : "")}{m_code} ({m_context})";
        }
    }

    internal enum QKeyListerContexts
    {
        DefaultTool,
        InTool
    }

    internal class QKeyListener : MonoBehaviour
    {
        private readonly float clickDelay = 0.3f;
        private List<QKeyEvent> keys = new List<QKeyEvent>();

        private QKeyEvent clicked = null;
        private float timeLastClicked;

        public delegate void Trigger();
        public ToolBaseSystem m_tool = null;

        internal void RegisterKey(KeyCode code, EventModifiers modifiers, QKeyListerContexts context, Trigger trigger)
        {
            RegisterKey(new QKeyEvent(code, modifiers, context, trigger));
        }

        internal void RegisterKey(QKeyEvent key)
        {
            keys.Add(key);
        }

        public void OnGUI()
        {
            if (UnityEngine.Event.current.type != EventType.KeyDown)
            {
                return;
            }

            UnityEngine.Event current = UnityEngine.Event.current;

            foreach (QKeyEvent key in keys)
            {
                if (m_tool != null)
                {
                    if (key.m_context == QKeyListerContexts.DefaultTool && QCommon.ActiveTool != QCommon.DefaultTool)
                    {
                        continue;
                    }
                    if (key.m_context == QKeyListerContexts.InTool && QCommon.ActiveTool != m_tool)
                    {
                        continue;
                    }
                }

                if (key.Alt != current.alt || key.Control != current.control || key.Shift != current.shift || key.m_code != current.keyCode)
                {
                    continue;
                }

                if (Time.time - timeLastClicked > clickDelay)
                {
                    QLoggerStatic.Debug($"Detected key: {key}");
                    clicked = key;
                    timeLastClicked = Time.time;
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
