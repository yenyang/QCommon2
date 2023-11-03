using Colossal.Entities;
using Game.Prefabs;
using Game.Tools;
using System.Diagnostics;
using Unity.Entities;
using Unity.Mathematics;

namespace QCommonLib
{
    public class QCommon
    {
        public static float3 Float3Invalid()
        {
            float3 f = new();
            f.SetInvalid();
            return f;
        }

        public static long ElapsedMilliseconds(long startTime)
        {
            long elapsed = math.abs(Stopwatch.GetTimestamp() - startTime);

            return elapsed / (Stopwatch.Frequency / 1000);
        }

        public static ToolBaseSystem ActiveTool { get => QCommon.ToolSystem.activeTool; }

        public static ToolSystem ToolSystem
        {
            get
            {
                _toolSystem ??= World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ToolSystem>();
                return _toolSystem;
            }
        }
        private static ToolSystem _toolSystem = null;

        public static DefaultToolSystem DefaultTool
        {
            get
            {
                _defaultTool ??= World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<DefaultToolSystem>();
                return _defaultTool;
            }
        }
        private static DefaultToolSystem _defaultTool = null;

        public static PrefabSystem PrefabSystem
        {
            get
            {
                _prefabSystem ??= World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PrefabSystem>();
                return _prefabSystem;
            }
        }
        private static PrefabSystem _prefabSystem = null;

        public static PrefabBase GetPrefab(EntityManager Manager, Entity e)
        {
            if (Manager.TryGetComponent(e, out PrefabRef prefab))
            {
                return QCommon.PrefabSystem.GetPrefab<PrefabBase>(prefab);
            }

            return null;
        }

        public static string GetPrefabName(EntityManager Manager, Entity e)
        {
            string name = Manager.GetName(e);

            PrefabBase prefabBase = QCommon.GetPrefab(Manager, e);
            if (prefabBase != null)
            {
                name = prefabBase.prefab ? prefabBase.prefab.name : prefabBase.name;
            }

            return name;
        }
    }
}
