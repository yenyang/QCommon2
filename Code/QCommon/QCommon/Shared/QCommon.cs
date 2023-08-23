using Colossal.Entities;
using Game.Prefabs;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace QCommonLib
{
    public class QCommon
    {
        public static ToolBaseSystem ActiveTool { get => QCommon.ToolSystem.activeTool; }

        public static ToolSystem ToolSystem
        {
            get
            {
                if (_toolSystem == null)
                {
                    _toolSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ToolSystem>();
                }
                return _toolSystem;
            }
        }
        private static ToolSystem _toolSystem = null;

        public static DefaultToolSystem DefaultTool
        {
            get
            {
                if (_defaultTool == null)
                {
                    _defaultTool = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<DefaultToolSystem>();
                }
                return _defaultTool;
            }
        }
        private static DefaultToolSystem _defaultTool = null;

        public static PrefabSystem PrefabSystem
        {
            get
            {
                if (_prefabSystem == null)
                {
                    _prefabSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PrefabSystem>();
                }
                return _prefabSystem;
            }
        }
        private static PrefabSystem _prefabSystem = null;

        public static string GetPrefabName(EntityManager Manager, Entity e)
        {
            string name = Manager.GetName(e);

            if (Manager.TryGetComponent(e, out PrefabRef prefab))
            {
                PrefabBase prefabBase = QCommon.PrefabSystem.GetPrefab<PrefabBase>(prefab);
                if (prefabBase != null)
                {
                    name = prefabBase.prefab ? prefabBase.prefab.name : prefabBase.name;
                }
            }

            return name;
        }
    }
}
