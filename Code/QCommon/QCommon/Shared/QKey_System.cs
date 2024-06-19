using Game.Input;
using Game.SceneFlow;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace QCommonLib
{
    internal enum QKey_Contexts
    {
        ToolDisabled    = 1,
        ToolEnabled     = 2,
        Always          = 3,
    }

    internal class QKey_Binding
    {
        /// <summary>
        /// The ProxyAction as defined in the mod's settings
        /// </summary>
        internal ProxyAction m_Action;

        /// <summary>
        /// When this keybinding should be active
        /// </summary>
        internal QKey_Contexts m_Context;

        /// <summary>
        /// The function to call (null for Passive bindings)
        /// </summary>
        internal QKey_System.Trigger m_Trigger;

        /// <summary>
        /// This keybind does not Trigger, it is read as needed
        /// </summary>
        internal bool m_IsPassive;
 
        /// <summary>
        /// Barrier to enable and disable this keybinding
        /// </summary>
        internal Barrier m_Barrier;

        internal QKey_Binding(ProxyAction action, QKey_Contexts context, QKey_System.Trigger trigger, bool isPassive = false)
        {
            m_Action = action;
            m_Context = context;
            m_Trigger = trigger;
            m_Barrier = new(action, true);
            m_IsPassive = isPassive;
        }
 
        ~QKey_Binding()
        {
            m_Barrier.Dispose();
        }

         internal bool IsPressed => m_Action.IsPressed();

        internal bool Enabled
        {
                get => !m_Barrier.blocked;
                set => m_Barrier.blocked = !value;
        }
 
        // For after the shouldBeEnabled bug is fixed
        //internal bool Enabled
        //{
        //    get => m_Action.enabled;
        //    set => m_Action.shouldBeEnabled = value;
        //}


        internal bool WhenToolDisabled  => (m_Context & QKey_Contexts.ToolDisabled) != 0;
        internal bool WhenToolEnabled   => (m_Context & QKey_Contexts.ToolEnabled) != 0;
        internal bool WhenAlways        => (m_Context & QKey_Contexts.Always) != 0;
    }

    internal abstract partial class QKey_System : SystemBase
    {
        private readonly List<QKey_Binding> _Bindings = new();

        public delegate void Trigger();

        internal void RegisterBinding(QKey_Binding binding)
        {
            _Bindings.Add(binding);
        }

        internal void Initialise()
        {
            Enabled = true;

            foreach (var binding in _Bindings)
            {
                binding.Enabled = binding.WhenToolDisabled;
            }
        }

        internal QKey_Binding GetBinding(string name) => _Bindings.First(b => b.m_Action.name == name);

        internal void OnToolEnable() => OnToolToggle(true);
        internal void OnToolDisable() => OnToolToggle(false);

        private void OnToolToggle(bool enabled)
        {
            foreach (var binding in _Bindings)
            {
                if (!binding.WhenToolDisabled)
                {
                    binding.Enabled = enabled;
                }
            }

            //DebugDumpAllBindings($"OnToolToggle {enabled}: ");
        }

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            if ((GameManager.instance.gameMode & Game.GameMode.GameOrEditor) == 0) return;
            if (!GameManager.instance.inputManager.controlOverWorld) return;

            foreach (var binding in _Bindings)
            {
                if (!binding.m_IsPassive && binding.m_Action.WasPressedThisFrame())
                {
                    binding.m_Trigger();
                }
            }
        }


        internal string DebugAllBindings()
        {
            string msg = $"Bindings: {_Bindings.Count}";
            foreach (QKey_Binding binding in _Bindings)
            {
                msg += $"\n{binding.m_Action.name,20}: enabled:{binding.Enabled,-5} {binding.m_Context}";
            }
            return msg;
        }

        internal void DebugDumpAllBindings(string prefix = "")
        {
            QLog.Debug(prefix + DebugAllBindings());
        }
    }
}
