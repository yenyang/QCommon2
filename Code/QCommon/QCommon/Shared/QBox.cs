using Colossal.Entities;
using Game.Prefabs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;

namespace QCommonLib
{
    public class QBox
    {
        internal static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        private static bool s_Initialised = false;
        internal static Dictionary<string, MethodInfo> s_EntityManagerMethods = new()
        {
            { "AddComponentData", null },
            { "SetComponentData", null }
        };

        public Entity m_Entity
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
            if (s_Initialised) return;

            // Generate MethodInfo data for EntityManager
            string[] keys = s_EntityManagerMethods.Keys.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                foreach (MethodInfo mi in typeof(EntityManager).GetMethods().Where(m => m.Name == keys[i]))
                {
                    ParameterInfo[] parameters = mi.GetParameters();
                    if (parameters.Length != 2) continue;
                    if (parameters[0].ParameterType == typeof(Entity) || parameters[1].ParameterType == typeof(Entity))
                    {
                        s_EntityManagerMethods[keys[i]] = mi;
                        break;
                    }
                }
                if (s_EntityManagerMethods[keys[i]] == null)
                {
                    throw new Exception($"Failed to find {keys[i]} method!");
                }
            }
            s_Initialised = true;
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

        #region Component Has/Get/Set

        public bool Has<T>() where T : unmanaged, IComponentData
        {
            return Has<T>(_entity);
        }
        public bool Has(Type type)
        {
            return Has(type, _entity);
        }
        public T Get<T>() where T : unmanaged, IComponentData
        {
            return Get<T>(_entity);
        }
        public IComponentData Get(Type type)
        {
            return Get(type, _entity);
        }
        public void Set<T>(T comp) where T : unmanaged, IComponentData
        {
            Set<T>(_entity, comp);
        }
        public void Set(Type type, IComponentData comp)
        {
            Set(type, _entity, comp);
        }

        public static bool Has<T>(Entity e) where T : unmanaged, IComponentData
        {
            return EntityManager.HasComponent<T>(e);
        }
        public static bool Has(Type type, Entity e)
        {
            return HasComponentByType(type, e);
        }
        public static T Get<T>(Entity e) where T : unmanaged, IComponentData
        {
            if (!Has<T>(e))
            {
                throw new Exception($"Component type {typeof(T)} not valid for entity {e} (Get<T>).");
            }
            return EntityManager.GetComponentData<T>(e);
        }
        public static IComponentData Get(Type type, Entity e)
        {
            if (!Has(type, e))
            {
                throw new Exception($"Component type {type} not valid for entity {e} (Get(Type)).");
            }
            return GetComponentDataByType(type, e);
        }
        public static void Set<T>(Entity e, T comp) where T : unmanaged, IComponentData
        {
            if (!Has<T>(e))
            {
                throw new Exception($"Component type {typeof(T)} not valid for entity {e} (Set<T>).");
            }
            EntityManager.SetComponentData<T>(e, comp);
        }
        public static void Set(Type type, Entity e, IComponentData comp)
        {
            if (!Has(type, e))
            {
                throw new Exception($"Component type {type} not valid for entity {e} (Set(Type)).");
            }
            SetComponentDataByType(type, e, comp);
        }

        #endregion

        #region Component Add/Remove

        public void Add<T>() where T : unmanaged, IComponentData
        {
            Add(new T());
        }
        public void Add<T>(T data) where T : unmanaged, IComponentData
        {
            EntityManager.AddComponentData(_entity, data);
        }
        public void Add(Type type)
        {
            Add(type, (IComponentData)Activator.CreateInstance(type));
        }
        public void Add(Type type, IComponentData data)
        {
            AddComponentDataByType(type, _entity, data);
        }

        public static void Add<T>(Entity e) where T : unmanaged, IComponentData
        {
            Add(e, new T());
        }
        public static void Add<T>(Entity e, T data) where T : unmanaged, IComponentData
        {
            EntityManager.AddComponentData(e, data);
        }
        public static void Add(Type type, Entity e)
        {
            Add(type, e, (IComponentData)Activator.CreateInstance(type));
        }
        public static void Add(Type type, Entity e, IComponentData data)
        {
            AddComponentDataByType(type, e, data);
        }


        public void Remove<T>() where T : unmanaged, IComponentData
        {
            EntityManager.RemoveComponent<T>(_entity);
        }
        public void Remove(Type type)
        {
            RemoveComponentByType(type, _entity);
        }

        public static void Remove<T>(Entity e) where T : unmanaged, IComponentData
        {
            EntityManager.RemoveComponent<T>(e);
        }
        public static void Remove(Type type, Entity e)
        {
            RemoveComponentByType(type, e);
        }

        #endregion


        #region Buffer Has/Get/Set

        #region Has

        internal bool HasBuffer<T>() where T : unmanaged, IBufferElementData
        {
            return EntityManager.HasBuffer<T>(_entity);
        }

        internal static bool HasBuffer<T>(Entity entity) where T : unmanaged, IBufferElementData
        {
            return EntityManager.HasBuffer<T>(entity);
        }

        internal bool HasBuffer(Type type)
        {
            return HasBufferByType(type, _entity);
        }

        internal static bool HasBuffer(Entity entity, Type type)
        {
            return HasBufferByType(type, entity);
        }

        #endregion

        #region Get

        internal void GetBuffer<T>(out List<T> buffer, bool isReadOnly = true) where T : unmanaged, IBufferElementData
        {
            GetBuffer(_entity, out buffer, isReadOnly);
        }

        internal static void GetBuffer<T>(Entity entity, out List<T> buffer, bool isReadOnly = true) where T : unmanaged, IBufferElementData
        {
            buffer = EntityManager.GetBuffer<T>(entity, isReadOnly).ToList();
        }

        internal static void GetBuffer<T>(Entity entity, out NativeArray<T> buffer, bool isReadOnly = true) where T : unmanaged, IBufferElementData
        {
            buffer = EntityManager.GetBuffer<T>(entity, isReadOnly).ToNativeArray(Allocator.Temp);
        }

        internal void GetBuffer(Type type, out List<IBufferElementData> buffer, bool isReadOnly = true)
        {
            GetRefBufferComponentsByType(type, out buffer, isReadOnly);
        }

        internal static void GetBuffer(Entity entity, Type type, out List<IBufferElementData> buffer, bool isReadOnly = true)
        {
            GetRefBufferComponentsByType(type, entity, out buffer, isReadOnly);
        }

        #endregion

        #region Set

        // Can't get this fucking bullshit to work

        //internal void SetBuffer<T>(List<T> data) where T : unmanaged, IBufferElementData
        //{
        //    var buffer = new NativeArray<T>(data.ToArray(), Allocator.Temp);
        //    SetBuffer<T>(_entity, buffer);
        //    buffer.Dispose();
        //}

        //internal void SetBuffer<T>(NativeArray<T> data) where T : unmanaged, IBufferElementData
        //{
        //    SetBuffer<T>(_entity, data);
        //    data.Dispose();
        //}

        //internal static void SetBuffer<T>(Entity entity, NativeArray<T> data) where T : unmanaged, IBufferElementData
        //{
        //    EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        //    DynamicBuffer<T> buffer = ecb.SetBuffer<T>(entity);
        //    buffer.Length = data.Length;
        //    for (int i = 0; i < data.Length; i++)
        //    {
        //        buffer[i] = data[i];
        //    }
        //    data.Dispose();
        //}

        //internal void SetBuffer(Type type, List<IBufferElementData> data)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

        #endregion

        #region Buffer Add/Remove

        internal void AddBuffer<T>(List<T> data) where T : unmanaged, IBufferElementData
        {
            if (HasBuffer<T>())
            {
                throw new Exception($"AddBuffer<T>: Buffer type <{typeof(T)}> already exists in {_entity}");
            }

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            DynamicBuffer<T> buffer = ecb.AddBuffer<T>(_entity);
            buffer.Length = data.Count;
            for (int i = 0; i < data.Count; i++)
            {
                buffer[i] = data[i];
            }
        }

        internal void AddBuffer(Type type, List<IBufferElementData> data)
        {
            throw new NotImplementedException();
        }

        internal void RemoveBuffer<T>() where T : unmanaged, IBufferElementData
        {
            if (!HasBuffer<T>())
            {
                throw new Exception($"RemoveBuffer<T>: Buffer type <{typeof(T)}> doesn't exists in {_entity}");
            }

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            ecb.RemoveComponent<T>(_entity);
        }

        internal void RemoveBuffer(Type type)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region Entity reference buffers

        /// <summary>
        /// Iterate through a buffer of type T, pass each buffer element to lamda, save result
        /// T lamda(T element, int i index in buffer)
        /// </summary>
        /// <typeparam name="T">The buffer to iterate through</typeparam>
        /// <param name="lambda">The code that each element is passed to</param>
        public void BufferUpdateIterate<T>(Func<T, int, T> lambda) where T : unmanaged, IBufferElementData
        {
            BufferUpdateIterate(_entity, lambda);
        }

        public static void BufferUpdateIterate<T>(Entity entity, Func<T, int, T> lambda) where T : unmanaged, IBufferElementData
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

        //public void IterateBufferUpdateByType(Type type, Func<Type, int, Type> lambda)
        //{
        //    IterateBufferUpdateByType(_entity, type, lambda);
        //}

        //public static void IterateBufferUpdateByType(Entity entity, Type type, Func<Type, int, Type> lambda)
        //{
        //    Type tBuffer = typeof(DynamicBuffer<>);
        //    Type tGenericBuffer = tBuffer.GetGenericTypeDefinition();
        //    Type tConstructed = tGenericBuffer.MakeGenericType(new Type[] { type });
        //    IEnumerable buffer = (IEnumerable)Activator.CreateInstance(tConstructed);

        //    Type[] tParam = new Type[] { typeof(EntityManager), typeof(Entity),typeof(bool), tConstructed };
        //    ParameterModifier[] mods = new ParameterModifier[] {  };

        //    MethodInfo method = typeof(EntityManager).GetMethod(nameof(EntitiesExtensions.TryGetBuffer));

        //    object[] args = new object[] { EntityManager, entity, false, buffer };
        //    bool result = (bool)method.Invoke(null, args);

        //    if (result)
        //    {
        //        for (int i = 0; i < buffer.; i++)
        //        {
        //            var field = buffer[i];
        //            field = lambda(field, i);
        //            buffer[i] = field;
        //        }
        //    }
        //}

        /// <summary>
        /// Gets a NativeArray of all the entities in an IBufferElementData reference buffer component
        /// </summary>
        /// <param name="type">Type - The IBufferElementData struct</param>
        /// <param name="entities">out NativeArray<Entity> - the resulting array of entities</param>
        public void GetRefBufferEntitiesByType(Type type, out NativeArray<Entity> entities)
        {
            GetRefBufferEntitiesByType(type, m_Entity, out entities);
        }
        public static void GetRefBufferEntitiesByType(Type type, Entity e, out NativeArray<Entity> entities)
        {
            FieldInfo field = GetEntityReferenceField(type);

            GetRefBufferComponentsByType(type, e, out List<IBufferElementData> buffer);
            Entity[] list = new Entity[buffer.Count];
            for (int i = 0; i < buffer.Count; i++)
            {
                object obj = field.GetValue(buffer[i]);
                list[i] = (Entity)obj;
            }

            entities = new NativeArray<Entity>(list, Allocator.Temp);
        }

        // WIP
        public void SetComponentBufferRefByType(Type type, int index, Entity element)
        {
            SetComponentBufferRefByType(_entity, type, index, element);
        }

        public static void SetComponentBufferRefByType(Entity entity, Type type, int index, Entity element)
        {
            FieldInfo field = GetEntityReferenceField(type);

            MethodInfo method = typeof(EntityManager).GetMethod(nameof(EntityManager.GetBuffer), new Type[] { typeof(Entity), typeof(bool) });
            MethodInfo generic = method.MakeGenericMethod(type);
            try
            {
                DynamicBuffer<Game.Areas.SubArea> bufferValue = (DynamicBuffer<Game.Areas.SubArea>)generic.Invoke(EntityManager, new object[] { entity, false });

                BufferUpdateIterate<Game.Areas.SubArea>(entity, (subArea, i) =>
                {
                    subArea.m_Area = element;
                    return subArea;
                });
            }
            catch (Exception ex)
            {
                QLog.Debug(ex.Message);
            }
        }

        public static void CreateNewRefBuffer(Entity entity, Type type, NativeArray<Entity> input, out List<IBufferElementData> output)
        {
            object a = new DynamicBuffer<Game.Areas.SubArea>();
            Type dDynBuf = a.GetType().MakeGenericType(type);//, new Type[] { typeof(int), typeof(Allocator), typeof(NativeArrayOptions) });
            object dynBuf = Activator.CreateInstance(dDynBuf, new object[] { input.Length, Allocator.Temp, NativeArrayOptions.ClearMemory });
            QLog.Debug($"\n{dynBuf}\n{dynBuf.GetType()}");

            output = new List<IBufferElementData>();

            FieldInfo field = GetEntityReferenceField(type);
            foreach (Entity e in input)
            {
                object o = Activator.CreateInstance(type);
                field.SetValue(o, e);
                output.Add((IBufferElementData)o);
            }
        }

        #endregion

        #region Reflection

        #region Basic Components

        public static bool HasComponentByType(Type type, Entity e)
        {
            MethodInfo hasComponent = typeof(EntityManager).GetMethod(nameof(EntityManager.HasComponent), new Type[] { typeof(Entity) });
            MethodInfo generic = hasComponent.MakeGenericMethod(type);
            return (bool)generic.Invoke(EntityManager, new object[] { e });
        }

        public static IComponentData GetComponentDataByType(Type type, Entity e)
        {
            MethodInfo getComponentData = typeof(EntityManager).GetMethod(nameof(EntityManager.GetComponentData), new Type[] { typeof(Entity) });
            MethodInfo generic = getComponentData.MakeGenericMethod(type);
            return (IComponentData)generic.Invoke(EntityManager, new object[] { e });
        }

        public static void SetComponentDataByType(Type type, Entity e, IComponentData data)
        {
            MethodInfo generic = s_EntityManagerMethods["SetComponentData"].MakeGenericMethod(type);
            generic.Invoke(EntityManager, new object[] { e, data });
        }

        public static void AddComponentDataByType(Type type, Entity e, IComponentData data)
        {
            MethodInfo generic = s_EntityManagerMethods["AddComponentData"].MakeGenericMethod(type);
            generic.Invoke(EntityManager, new object[] { e, data });
        }

        public static void RemoveComponentByType(Type type, Entity e)
        {
            MethodInfo removeComponent = typeof(EntityManager).GetMethod(nameof(EntityManager.RemoveComponent), new Type[] { typeof(Entity) });
            MethodInfo generic = removeComponent.MakeGenericMethod(type);
            generic.Invoke(EntityManager, new object[] { e });
        }

        #endregion

        #region Buffers

        public static bool HasBufferByType(Type type, Entity e)
        {
            MethodInfo hasBuffer = typeof(EntityManager).GetMethod(nameof(EntityManager.HasBuffer), new Type[] { typeof(Entity) });
            MethodInfo generic = hasBuffer.MakeGenericMethod(type);
            return (bool)generic.Invoke(EntityManager, new object[] { e });
        }

        /// <summary>
        /// Gets a list of all the components in reference buffer
        /// </summary>
        /// <param name="type">Type - Extension for the IBufferElementData struct</param>
        /// <param name="components">out List<IBufferElementData> - The reference buffer's components</param>
        /// <param name="isReadOnly">Bool - Should these buffer components be read only?</param>
        /// <returns>List<IBufferElementData> - The reference buffer's components</returns>
        public void GetRefBufferComponentsByType(Type type, out List<IBufferElementData> components, bool isReadOnly = true)
        {
            GetRefBufferComponentsByType(type, _entity, out components, isReadOnly);
        }

        public static void GetRefBufferComponentsByType(Type type, Entity e, out List<IBufferElementData> components, bool isReadOnly = true)
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
            bufferValue = generic.Invoke(EntityManager, new object[] { e, isReadOnly });
        }

        public static bool GetRefBufferComponentByType(Type type, Entity e, out IBufferElementData comp, bool isReadOnly = true)
        {
            MethodInfo method = typeof(EntityManager).GetMethod(nameof(EntityManager.GetBuffer), new Type[] { typeof(Entity), typeof(bool) });
            MethodInfo generic = method.MakeGenericMethod(type);
            object bufferValue = generic.Invoke(EntityManager, new object[] { e, isReadOnly });
            FieldInfo[] fields = bufferValue.GetType().GetFields();
            if (fields.Length == 0)
            {
                comp = null;
                return false;
            }
            comp = (IBufferElementData)fields[0].GetValue(bufferValue);

            return true;
        }

        #endregion

        #endregion
    }
}
