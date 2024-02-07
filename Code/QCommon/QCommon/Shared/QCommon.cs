using Colossal.Entities;
using Colossal.Mathematics;
using Game.Prefabs;
using Game.Tools;
using System;
using System.Diagnostics;
using Unity.Entities;
using Unity.Mathematics;

namespace QCommonLib
{
    public class QCommon
    {
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

        // Thanks to vladibo on the Unity forum
        public static int IntersectionsBetweenLineAndCircle(Circle2 circle, Line2 line, out float2 intersect1, out float2 intersect2)
        {
            float t;
            float2 magnitude = line.b - line.a;

            var a = magnitude.x * magnitude.x + magnitude.y * magnitude.y;
            var b = 2 * (magnitude.x * (line.a.x - circle.position.x) + magnitude.y * (line.a.y - circle.position.y));
            var c = (line.a.x - circle.position.x) * (line.a.x - circle.position.x) + (line.a.y - circle.position.y) * (line.a.y - circle.position.y) - circle.radius * circle.radius;

            var determinate = b * b - 4 * a * c;
            if ((a <= 0.0000001) || (determinate < -0.0000001))
            {
                // No real solutions.
                intersect1 = float2.zero;
                intersect2 = float2.zero;
                return 0;
            }
            if (determinate < 0.0000001 && determinate > -0.0000001)
            {
                // One solution.
                t = -b / (2 * a);
                intersect1 = new float2(line.a.x + t * magnitude.x, line.a.y + t * magnitude.y);
                intersect2 = float2.zero;
                return 1;
            }

            // Two solutions.
            t = (float)((-b + Math.Sqrt(determinate)) / (2 * a));
            intersect1 = new float2(line.a.x + t * magnitude.x, line.a.y + t * magnitude.y);
            t = (float)((-b - Math.Sqrt(determinate)) / (2 * a));
            intersect2 = new float2(line.a.x + t * magnitude.x, line.a.y + t * magnitude.y);

            return 2;
        }
    }
}
