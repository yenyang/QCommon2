using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;

namespace QCommonLib
{
    internal class QByType
    {
        internal static EntityManager EM => World.DefaultGameObjectInjectionWorld.EntityManager;

        public static bool HasBuffer(Type type, Entity e)
        {
            MethodInfo hasBuffer = typeof(EntityManager).GetMethod(nameof(EntityManager.HasBuffer), new Type[] { typeof(Entity) });
            MethodInfo generic = hasBuffer.MakeGenericMethod(type);
            return (bool)generic.Invoke(EM, new object[] { e });
        }

        public static void GetRefBufferComponents(Type type, Entity e, out List<IBufferElementData> components, bool isReadOnly = true)
        {
            GetRefBufferComponentsAsDynBuffer(type, e, out object rawDynamicBuffer, isReadOnly);
            IEnumerable data = (IEnumerable)rawDynamicBuffer.GetType().GetMethod("AsNativeArray", BindingFlags.Instance | BindingFlags.Public).Invoke(rawDynamicBuffer, null);
            components = new List<IBufferElementData>();
            foreach (object o in data)
            {
                components.Add((IBufferElementData)o);
            }
            IDisposable disposable = data as IDisposable;
            disposable.Dispose();
        }

        public static void GetRefBufferComponentsAsDynBuffer(Type type, Entity e, out object bufferValue, bool isReadOnly = true)
        {
            MethodInfo method = typeof(EntityManager).GetMethod(nameof(EntityManager.GetBuffer), new Type[] { typeof(Entity), typeof(bool) });
            MethodInfo generic = method.MakeGenericMethod(type);
            bufferValue = generic.Invoke(EM, new object[] { e, isReadOnly });
        }

        public static int GetRefBufferLength(Type type, Entity e)
        {
            GetRefBufferComponents(type, e, out var buffer);
            return buffer.Count;
        }
    }
}
