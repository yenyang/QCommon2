using Colossal.Entities;
using Game.Net;
using Game.Prefabs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace QCommonLib
{
    public class QBox
    {
        internal class Log : QLoggerStatic { }
        internal static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        private static bool _initialised = false;
        internal static Dictionary<string, MethodInfo> m_EntityManagerMethods = new Dictionary<string, MethodInfo>()
        {
            { "AddComponentData", null },
            { "SetComponentData", null }
        };

        public Entity Entity
        {
            get
            {
                return _entity;
            }
            set
            {
                _entity = value;
            }
        }
        protected Entity _entity;

        internal PrefabBase Prefab => QCommon.GetPrefab(EntityManager, _entity);
        internal string PrefabName => QCommon.GetPrefabName(EntityManager, _entity);

        public QBox()
        {
            if (_initialised) return;

            // Generate MethodInfo data for EntityManager
            string[] keys = m_EntityManagerMethods.Keys.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                foreach (MethodInfo mi in typeof(EntityManager).GetMethods().Where(m => m.Name == keys[i]))
                {
                    ParameterInfo[] parameters = mi.GetParameters();
                    if (parameters.Length != 2) continue;
                    if (parameters[0].ParameterType == typeof(Entity) || parameters[1].ParameterType == typeof(Entity))
                    {
                        m_EntityManagerMethods[keys[i]] = mi;
                        break;
                    }
                }
                if (m_EntityManagerMethods[keys[i]] == null)
                {
                    throw new Exception($"Failed to find {keys[i]} method!");
                }
            }
            _initialised = true;
        }


        //public string TryGetPrefabName(PrefabSystem prefabSystem)
        //{
        //    if (Has<PrefabRef>())
        //    {
        //        PrefabRef prefabRef = Get<PrefabRef>();
        //        if (prefabSystem.TryGetPrefab(prefabRef, out PrefabBase prefab))
        //        {
        //            return prefab.name;
        //        }
        //    }
        //    else if (Has<PrefabData>())
        //    {
        //        PrefabData prefabData = Get<PrefabData>();
        //        if (prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefab))
        //        {
        //            return prefab.name;
        //        }
        //    }
        //    return null;
        //}


        #region Component Has/Get/Set

        public bool Has<T>() where T : unmanaged, IComponentData
        {
            return EntityManager.HasComponent<T>(_entity);
        }

        public bool Has(Type type)
        {
            return HasComponentByType(type);
        }

        public T Get<T>() where T : unmanaged, IComponentData
        {
            if (!Has<T>())
            {
                throw new Exception($"Component type {typeof(T)} not valid for entity {_entity} (Get<T>).");
            }
            return EntityManager.GetComponentData<T>(_entity);
        }

        public IComponentData Get(Type type)
        {
            if (!Has(type))
            {
                throw new Exception($"Component type {type} not valid for entity {_entity} (Get(Type)).");
            }
            return GetComponentDataByType(type);
        }

        public void Set<T>(T comp) where T : unmanaged, IComponentData
        {
            if (!Has<T>())
            {
                throw new Exception($"Component type {typeof(T)} not valid for entity {_entity} (Set<T>).");
            }
            EntityManager.SetComponentData<T>(_entity, comp);
        }

        public void Set(Type type, IComponentData comp)
        {
            if (!Has(type))
            {
                throw new Exception($"Component type {type} not valid for entity {_entity} (Set(Type)).");
            }
            SetComponentDataByType(type, comp);
        }

        // WIP Set

        #endregion


        #region Component Add/Remove

        // WIP Add/Remove
        public void Add<T>() where T : unmanaged, IComponentData
        {
            EntityManager.AddComponent<T>(_entity);
        }
        public void Add(Type type)
        {
            throw new NotImplementedException();
        }

        public void Remove<T>() where T : unmanaged, IComponentData
        {
            EntityManager.RemoveComponent<T>(_entity);
        }
        public void Remove(Type type)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region Buffer Has/Get/Set

        internal bool HasBuffer<T>() where T : unmanaged, IBufferElementData
        {
            return EntityManager.HasBuffer<T>(_entity);
        }

        internal bool HasBuffer(Type type)
        {
            return HasBufferByType(type);
        }

        // WIP Get/Set

        #endregion

        #region Buffer Add/Remove

        // WIP Add/Remove

        #endregion

        #region Entity reference buffers

        /// <summary>
        /// Iterate through a buffer of type T, pass each buffer element to lamda, save result
        /// T lamda(T element, int i index in buffer)
        /// </summary>
        /// <typeparam name="T">The buffer to iterate through</typeparam>
        /// <param name="lambda">The code that each element is passed to</param>
        public void IterateBuffer<T>(Func<T, int, T> lambda) where T : unmanaged, IBufferElementData
        {
            IterateBuffer(_entity, lambda);
        }

        public static void IterateBuffer<T>(Entity entity, Func<T, int, T> lambda) where T : unmanaged, IBufferElementData
        {
            if (EntityManager.TryGetBuffer(entity, false, out DynamicBuffer<T> buffer))
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    T field = buffer[i];
                    field = lambda(field, i);
                    buffer[i] = field;
                }
            }
        }

        /// <summary>
        /// Get the field in an IBufferElementData reference buffer component that holds the actual entity reference
        /// For example for Game.Areas.SubArea.m_area, it returns m_area
        /// </summary>
        /// <param name="type">The IBufferElementData struct type to search</param>
        /// <returns>FieldInfo of this field</returns>
        /// <exception cref="Exception">If no such field is found</exception>
        public static FieldInfo GetEntityReferenceField(Type type)
        {
            FieldInfo field = null;
            foreach (FieldInfo f in type.GetFields())
            {
                if (f.FieldType == typeof(Entity))
                {
                    field = f;
                    break;
                }
            }
            if (field == null) throw new Exception($"Entity field not found for type {type}");

            return field;
        }

        /// <summary>
        /// Gets a list of all the components in reference buffer
        /// </summary>
        /// <param name="type">Type - Extension for the IBufferElementData struct</param>
        /// <param name="components">out List<IBufferElementData> - The reference buffer's components</param>
        /// <param name="isReadOnly">Bool - Should these buffer components be read only?</param>
        /// <returns>List<IBufferElementData> - The reference buffer's components</returns>
        public void GetReferenceBufferComponentsByType(Type type, out List<IBufferElementData> components, bool isReadOnly = true)
        {
            GetReferenceBufferComponentsByType(EntityManager, _entity, type, out components, isReadOnly);
        }

        public static void GetReferenceBufferComponentsByType(EntityManager entityManager, Entity entity, Type type, out List<IBufferElementData> components, bool isReadOnly = true)
        {
            MethodInfo method = typeof(EntityManager).GetMethod(nameof(entityManager.GetBuffer), new Type[] { typeof(Entity), typeof(bool) });
            MethodInfo generic = method.MakeGenericMethod(type);
            object bufferValue = generic.Invoke(entityManager, new object[] { entity, isReadOnly });
            IEnumerable data = (IEnumerable)bufferValue.GetType().GetMethod("AsNativeArray", BindingFlags.Instance | BindingFlags.Public).Invoke(bufferValue, null);
            components = new List<IBufferElementData>();
            foreach (object o in data)
            {
                components.Add((IBufferElementData)o);
            }
            IDisposable disposable = data as IDisposable;
            disposable.Dispose();
        }

        /// <summary>
        /// Gets a NativeArray of all the entities in an IBufferElementData reference buffer component
        /// </summary>
        /// <param name="type">Type - The IBufferElementData struct</param>
        /// <param name="entities">out NativeArray<Entity> - the resulting array of entities</param>
        public void GetReferenceBufferEntitiesByType(Type type, out NativeArray<Entity> entities)
        {
            FieldInfo field = GetEntityReferenceField(type);

            GetReferenceBufferComponentsByType(type, out List<IBufferElementData> buffer);
            Entity[] list = new Entity[buffer.Count];
            for (int i = 0; i < buffer.Count; i++)
            {
                object obj = field.GetValue(buffer[i]);
                list[i] = (Entity)obj;
            }

            entities = new NativeArray<Entity>(list, Allocator.Temp);
        }

        // WIP
        public void SetComponentBufferReferenceByType(Type type, int index, Entity element)
        {
            SetComponentBufferReferenceByType(EntityManager, _entity, type, index, element);
        }

        public static void SetComponentBufferReferenceByType(EntityManager entityManager, Entity entity, Type type, int index, Entity element)
        {
            FieldInfo field = GetEntityReferenceField(type);

            MethodInfo method = typeof(EntityManager).GetMethod(nameof(EntityManager.GetBuffer), new Type[] { typeof(Entity), typeof(bool) });
            MethodInfo generic = method.MakeGenericMethod(type);
            try
            {
                DynamicBuffer<Game.Areas.SubArea> bufferValue = (DynamicBuffer<Game.Areas.SubArea>)generic.Invoke(entityManager, new object[] { entity, false });

                IterateBuffer<Game.Areas.SubArea>(entity, (subArea, i) =>
                {
                    subArea.m_Area = element;
                    return subArea;
                });
            }
            catch (Exception ex)
            {
                QLoggerStatic.Debug(ex.Message);
            }
        }

        #endregion

        #region Reflection

        public bool HasComponentByType(Type type)
        {
            MethodInfo hasComponent = typeof(EntityManager).GetMethod(nameof(EntityManager.HasComponent), new Type[] { typeof(Entity) });
            MethodInfo generic = hasComponent.MakeGenericMethod(type);
            return (bool)generic.Invoke(EntityManager, new object[] { _entity });
        }

        public IComponentData GetComponentDataByType(Type type)
        {
            MethodInfo getComponentData = typeof(EntityManager).GetMethod(nameof(EntityManager.GetComponentData), new Type[] { typeof(Entity) });
            MethodInfo generic = getComponentData.MakeGenericMethod(type);
            return (IComponentData)generic.Invoke(EntityManager, new object[] { _entity });
        }

        public void SetComponentDataByType(Type type, IComponentData comp)
        {
            MethodInfo generic = m_EntityManagerMethods["SetComponentData"].MakeGenericMethod(type);
            generic.Invoke(EntityManager, new object[] { _entity, comp });
        }

        public void AddComponentDataByType(Type type, IComponentData data)
        {
            MethodInfo generic = m_EntityManagerMethods["AddComponentData"].MakeGenericMethod(type);
            generic.Invoke(EntityManager, new object[] { _entity, data });
        }

        public void RemoveComponentByType(Type type)
        {
            MethodInfo removeComponent = typeof(EntityManager).GetMethod(nameof(EntityManager.RemoveComponent), new Type[] { typeof(Entity) });
            MethodInfo generic = removeComponent.MakeGenericMethod(type);
            generic.Invoke(EntityManager, new object[] { _entity });
        }



        public bool HasBufferByType(Type type)
        {
            MethodInfo hasBuffer = typeof(EntityManager).GetMethod(nameof(EntityManager.HasBuffer), new Type[] { typeof(Entity) });
            MethodInfo generic = hasBuffer.MakeGenericMethod(type);
            return (bool)generic.Invoke(EntityManager, new object[] { _entity });
        }

        #endregion
    }
}
