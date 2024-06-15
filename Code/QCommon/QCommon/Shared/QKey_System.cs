using Game.Input;
using Game.SceneFlow;
using System.Collections.Generic;
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
        internal ProxyAction m_Action;
        internal QKey_Contexts m_Context;
        internal QKey_System.Trigger m_Trigger;

        internal bool WhenToolDisabled  => (m_Context & QKey_Contexts.ToolDisabled) != 0;
        internal bool WhenToolEnabled   => (m_Context & QKey_Contexts.ToolEnabled) != 0;
        internal bool WhenAlways        => (m_Context & QKey_Contexts.Always) != 0;
    }

    internal partial class QKey_System : SystemBase
    {
        private readonly List<QKey_Binding> _Bindings = new();

        public delegate void Trigger();

        internal void RegisterBinding(QKey_Binding binding)
        {
            binding.m_Action.shouldBeEnabled = binding.WhenToolDisabled;
            _Bindings.Add(binding);
            QLog.Debug($"Key registered: {binding.m_Action.name}, context: {binding.m_Context}, enabled: {binding.m_Action.enabled}, registered:{_Bindings.Count}");
        }

        internal void Initialise()
        {
            Enabled = true;
            //string msg = $"Initialising {_Bindings.Count} bindings";
            foreach (var binding in _Bindings)
            {
                binding.m_Action.shouldBeEnabled = binding.WhenToolDisabled;
                //msg += $"\n    {binding.m_Action.name}: {binding.m_Action.enabled} (should be: {binding.WhenToolDisabled})";
            }
            //foreach (var binding in _Bindings)
            //{
            //    msg += $"\n    After: {binding.m_Action.name}: {binding.m_Action.enabled} (should be: {binding.WhenToolDisabled})";
            //}
            //QLog.Debug(msg);
        }

        internal void OnToolEnable() => OnToolToggle(true);
        internal void OnToolDisable() => OnToolToggle(false);

        private void OnToolToggle(bool enabled)
        {
            string msg = $"Toggling up to {_Bindings.Count} bindings";
            foreach (var binding in _Bindings)
            {
                if (!binding.WhenToolDisabled)
                {
                    binding.m_Action.shouldBeEnabled = enabled;
                    msg += $"\n    {binding.m_Action.name}: {binding.m_Action.enabled}";
                }
            }
            //foreach (var binding in _Bindings)
            //{
            //    msg += $"\n    After: {binding.m_Action.name}: {binding.m_Action.enabled}";
            //}
            //MoveIt.Mod.Settings.GetAction(MoveIt.Systems.MIT_HotkeySystem.KEY_TOGGLEMARQUEE).shouldBeEnabled = enabled;
            //MoveIt.Mod.Settings.GetAction(MoveIt.Systems.MIT_HotkeySystem.KEY_TOGGLEMANIP).shouldBeEnabled = enabled;
            //foreach (var binding in _Bindings)
            //{
            //    msg += $"\n    After2: {binding.m_Action.name}: {binding.m_Action.enabled}";
            //}
            //msg += $"\n    After3.1: {MoveIt.Mod.Settings.GetAction(MoveIt.Systems.MIT_HotkeySystem.KEY_TOGGLEMARQUEE).name}: {MoveIt.Mod.Settings.GetAction(MoveIt.Systems.MIT_HotkeySystem.KEY_TOGGLEMARQUEE).enabled}";
            //msg += $"\n    After3.2: {MoveIt.Mod.Settings.GetAction(MoveIt.Systems.MIT_HotkeySystem.KEY_TOGGLEMANIP).name}: {MoveIt.Mod.Settings.GetAction(MoveIt.Systems.MIT_HotkeySystem.KEY_TOGGLEMANIP).enabled}";
            QLog.Debug(msg);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            if ((GameManager.instance.gameMode & Game.GameMode.GameOrEditor) == 0) return;
            if (!GameManager.instance.inputManager.controlOverWorld) return;

            //string msg = $"Processing {_Bindings.Count} bindings";
            foreach (var binding in _Bindings)
            {
                //msg += $"\n    {binding.m_Action.name}: {binding.m_Action.enabled}";
                if (binding.m_Action.WasPressedThisFrame())
                {
                    QLog.Debug($"Key pressed: {binding.m_Action.name} (enabled: {binding.m_Action.enabled})");
                    binding.m_Trigger();
                }
            }
            //QLog.Bundle("KEYS", msg);
        }
    }
}
