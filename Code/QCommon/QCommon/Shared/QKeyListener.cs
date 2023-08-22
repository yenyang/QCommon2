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
    internal struct QKeyEvent
    {
        internal KeyCode m_code;
        internal EventModifiers m_modifiers;
        internal bool clicked;
        internal float timeLastClicked;

        internal QKeyEvent(KeyCode code, EventModifiers modifiers)
        {
            m_code = code;
            m_modifiers = modifiers;
            clicked = false;
            timeLastClicked = 0;
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
        private bool clicked;
        private float timeLastClicked;
        private List<QKeyEvent> keys;


        public static readonly KeyCode keyCode = KeyCode.M;
        public static readonly EventModifiers modifiers = EventModifiers.Control;

        public event Action<EventModifiers, KeyCode> keyHitEvent = delegate
        {
        };

        public void OnGUI()
        {
            if (UnityEngine.Event.current.type != EventType.KeyDown)
            {
                return;
            }

            UnityEngine.Event current = UnityEngine.Event.current;
            if (current == null)
            {
                QLoggerStatic.Debug($"Event: <null>");
            }
            else
            {
                QLoggerStatic.Debug($"Event: {current.type} {current.keyCode}");
            }
            if (current.type == EventType.KeyDown && current.control && current.keyCode == keyCode && Time.time - timeLastClicked > clickDelay)
            {
                clicked = true;
                timeLastClicked = Time.time;
            }
        }

        public void Update()
        {
            if (clicked)
            {
                clicked = false;
                this.keyHitEvent(modifiers, keyCode);
            }
        }

        public void OnDestroy()
        {
            keyHitEvent = null;
        }

        public QKeyListener()
        {
        }

    }
}
