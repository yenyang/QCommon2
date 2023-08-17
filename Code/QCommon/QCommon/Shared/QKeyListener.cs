using Colossal.Serialization.Entities;
using Game;
using Game.Common;
using System;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Game.Input;
using Game.Prefabs;
using Game.Tools;
using Unity.Jobs;

namespace QCommonLib
{
    internal class QKeyListener : MonoBehaviour
    {
        private readonly float clickDelay = 0.3f;
        private bool clicked;
        private float timeLastClicked;
        public static readonly KeyCode keyCode = KeyCode.M;
        public static readonly EventModifiers modifiers = EventModifiers.Control;

        public event Action<EventModifiers, KeyCode> keyHitEvent = delegate
        {
        };

        public void OnGUI()
        {
            UnityEngine.Event current = UnityEngine.Event.current;
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
