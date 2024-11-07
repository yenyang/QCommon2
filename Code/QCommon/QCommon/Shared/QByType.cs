using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;

namespace QCommonLib
{
    internal static class QByType
    {
        private static EntityManager _Manager => World.DefaultGameObjectInjectionWorld.EntityManager;

        public static bool HasBuffer(Type type, Entity e)
        {
            MethodInfo hasBuffer = typeof(EntityManager).GetMethod(nameof(EntityManager.HasBuffer), new Type[] { typeof(Entity) });
            if (hasBuffer is null) return false;
            MethodInfo generic = hasBuffer.MakeGenericMethod(type);
            return (bool)generic.Invoke(_Manager, new object[] { e });
        }

        public static void GetRefBufferComponents(Type type, Entity e, out List<IBufferElementData> components, bool isReadOnly = true)
        {
            components = new List<IBufferElementData>();
            GetRefBufferComponentsAsDynBuffer(type, e, out object rawDynamicBuffer, isReadOnly);
            var data = (IEnumerable)rawDynamicBuffer.GetType().GetMethod("AsNativeArray", BindingFlags.Instance | BindingFlags.Public)?.Invoke(rawDynamicBuffer, null);
            if (data is null) return;
            foreach (object o in data)
            {
                components.Add((IBufferElementData)o);
            }

            if (data is not IDisposable disposable) return;
            disposable.Dispose();
        }

        public static void GetRefBufferComponentsAsDynBuffer(Type type, Entity e, out object bufferValue, bool isReadOnly = true)
        {
            MethodInfo method = typeof(EntityManager).GetMethod(nameof(EntityManager.GetBuffer), new Type[] { typeof(Entity), typeof(bool) });
            if (method is null)
            {
                bufferValue = null;
                return;
            }

            MethodInfo generic = method.MakeGenericMethod(type);
            bufferValue = generic.Invoke(_Manager, new object[] { e, isReadOnly });
        }

        public static int GetRefBufferLength(Type type, Entity e)
        {
            GetRefBufferComponents(type, e, out var buffer);
            return buffer.Count;
        }
    }
}
