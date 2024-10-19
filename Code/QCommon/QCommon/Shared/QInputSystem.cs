using Game.Input;
using Game.Modding;
using Game.SceneFlow;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace QCommonLib
{
    internal enum QInput_Contexts
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
        /// When this input binding should be active
        /// </summary>
        internal QInput_Contexts m_Context;

        /// <summary>
        /// The function to call (null for Passive bindings)
        /// </summary>
        internal QInputSystem.Trigger m_Trigger;

        /// <summary>
        /// This keybind does not Trigger, it is read as needed
        /// </summary>
        internal bool m_IsPassive;

        internal QKey_Binding(ProxyAction action, QInput_Contexts context, QInputSystem.Trigger trigger, bool isPassive = false)
        {
            m_Action    = action;
            m_Context   = context;
            m_Trigger   = trigger;
            m_IsPassive = isPassive;
        }

        internal bool IsPressed => m_Action.IsPressed();

        internal bool Enabled
        {
            get => m_Action.enabled;
            set => m_Action.shouldBeEnabled = value;
        }

        internal bool WhenToolDisabled  => (m_Context & QInput_Contexts.ToolDisabled) != 0;
        internal bool WhenToolEnabled   => (m_Context & QInput_Contexts.ToolEnabled) != 0;
        internal bool WhenAlways        => (m_Context & QInput_Contexts.Always) != 0;
    }

    internal abstract partial class QInputSystem : SystemBase
    {
        public const string MOUSE_APPLY = "Mouse_Apply";
        public const string MOUSE_CANCEL = "Mouse_Cancel";

        private readonly List<QKey_Binding> _KeyBindings = new();
        public ProxyAction MouseApply => _MouseApplyMimic;
        private ProxyAction _MouseApplyMimic;
        public ProxyAction MouseCancel => _MouseCancelMimic;
        private ProxyAction _MouseCancelMimic;

        public delegate void Trigger();

        internal void RegisterBinding(QKey_Binding binding)
        {
            _KeyBindings.Add(binding);
        }

        internal void Initialise(ModSetting settings)
        {
            Enabled = true;

            _MouseApplyMimic = settings.GetAction(MOUSE_APPLY);
            _MouseCancelMimic = settings.GetAction(MOUSE_CANCEL);

            foreach (var binding in _KeyBindings)
            {
                binding.Enabled = binding.WhenToolDisabled;
            }
        }

        internal QKey_Binding GetBinding(string name) => _KeyBindings.First(b => b.m_Action.name == name);

        internal void OnToolEnable() => OnToolToggle(true);
        internal void OnToolDisable() => OnToolToggle(false);

        private void OnToolToggle(bool enabled)
        {
            foreach (var binding in _KeyBindings)
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

            foreach (var binding in _KeyBindings)
            {
                if (!binding.m_IsPassive && binding.m_Action.WasPressedThisFrame())
                {
                    binding.m_Trigger();
                }
            }
        }


        internal string DebugAllBindings()
        {
            string msg = $"Bindings: {_KeyBindings.Count}\n   Mimic Apply:{_MouseApplyMimic}\n  Mimic Cancel:{_MouseCancelMimic}";

            foreach (QKey_Binding binding in _KeyBindings)
            {
                msg += $"\n{binding.m_Action.name,20}: enabled:{binding.Enabled,-5} {binding.m_Context} bindings:{binding.m_Action.bindings.Count()}";
                //foreach (ProxyBinding pb in binding.m_Action.bindings)
                //{
                //    msg += $"\n    {pb}, mapName:{pb.m_MapName}, path:{pb.path},";
                //    msg += $"\n    + mods:{pb.modifiers.Count}, action:{pb.m_ActionName}, {pb.ToHumanReadablePath().Count()}:{string.Join(",", pb.ToHumanReadablePath())}";
                //}
            }

            return msg;
        }

        internal void DebugDumpAllBindings(string prefix = "")
        {
            QLog.Debug(prefix + DebugAllBindings());
        }
    }
}
