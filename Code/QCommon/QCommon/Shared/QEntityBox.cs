using Colossal.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace QCommonLib
{
    internal struct QEntityBox
    {
        internal Entity Entity { get => _entity; set => _entity = value; }
        private Entity _entity;

        internal static EntityManager s_manager;
        private static bool s_initialised = false;
        internal static Dictionary<string, MethodInfo> s_EntityManagerMethods = new Dictionary<string, MethodInfo>()
        {
            { "AddComponentData", null },
            { "SetComponentData", null }
        };

        internal QEntityBox(Entity e)
        {
            _entity = e;

            // Set static fields
            if (s_initialised) return;

            s_manager = World.DefaultGameObjectInjectionWorld.EntityManager;

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
            s_initialised = true;
        }

        #region Component Has/Get/Set

        public bool Has<T>() where T : unmanaged, IComponentData
        {
            return s_manager.HasComponent<T>(_entity);
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
            return s_manager.GetComponentData<T>(_entity);
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
            s_manager.SetComponentData<T>(_entity, comp);
        }

        public void Set(Type type, IComponentData comp)
        {
            if (!Has(type))
            {
                throw new Exception($"Component type {type} not valid for entity {_entity} (Set(Type)).");
            }
            SetComponentDataByType(type, comp);
        }

        #endregion


        #region Component Add/Remove

        public void Add<T>() where T : unmanaged, IComponentData
        {
            Add(new T());
        }
        public void Add<T>(T data) where T : unmanaged, IComponentData
        {
            s_manager.AddComponentData(_entity, data);
        }
        public void Add(Type type)
        {
            Add(type, (IComponentData)Activator.CreateInstance(type));
        }
        public void Add(Type type, IComponentData data)
        {
            AddComponentDataByType(type, data);
        }


        public void Remove<T>() where T : unmanaged, IComponentData
        {
            s_manager.RemoveComponent<T>(_entity);
        }
        public void Remove(Type type)
        {
            RemoveComponentByType(type);
        }

        #endregion


        #region Buffer Has/Get/Set

        #region Has

        internal bool HasBuffer<T>() where T : unmanaged, IBufferElementData
        {
            return s_manager.HasBuffer<T>(_entity);
        }

        internal static bool HasBuffer<T>(Entity entity) where T : unmanaged, IBufferElementData
        {
            return s_manager.HasBuffer<T>(entity);
        }

        internal bool HasBuffer(Type type)
        {
            return HasBufferByType(_entity, type);
        }

        internal static bool HasBuffer(Entity entity, Type type)
        {
            return HasBufferByType(entity, type);
        }

        #endregion

        #region Get

        internal void GetBuffer<T>(out List<T> buffer, bool isReadOnly = true) where T : unmanaged, IBufferElementData
        {
            GetBuffer(_entity, out buffer, isReadOnly);
        }

        internal static void GetBuffer<T>(Entity entity, out List<T> buffer, bool isReadOnly = true) where T : unmanaged, IBufferElementData
        {
            buffer = s_manager.GetBuffer<T>(entity, isReadOnly).ToList();
        }

        internal void GetBuffer(Type type, out List<IBufferElementData> buffer, bool isReadOnly = true)
        {
            GetBufferComponentsByType(type, out buffer, isReadOnly);
        }

        internal static void GetBuffer(Entity entity, Type type, out List<IBufferElementData> buffer, bool isReadOnly = true)
        {
            GetBufferComponentsByType(entity, type, out buffer, isReadOnly);
        }

        #endregion

        #region Set

        internal void SetBuffer<T>(List<T> data) where T : unmanaged, IBufferElementData
        {
            SetBuffer<T>(_entity, data);
        }

        internal static void SetBuffer<T>(Entity entity, List<T> data) where T : unmanaged, IBufferElementData
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            DynamicBuffer<T> buffer = ecb.SetBuffer<T>(entity);
            buffer.Length = data.Count;
            for (int i = 0; i < data.Count; i++)
            {
                buffer[i] = data[i];
            }
        }

        internal void SetBuffer(Type type, List<IBufferElementData> data)
        {
            throw new NotImplementedException();
        }

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
        public void IterateBufferUpdate<T>(Func<T, int, T> lambda) where T : unmanaged, IBufferElementData
        {
            BufferUpdateIterate(_entity, lambda);
        }

        public static void BufferUpdateIterate<T>(Entity entity, Func<T, int, T> lambda) where T : unmanaged, IBufferElementData
        {
            if (s_manager.TryGetBuffer(entity, false, out DynamicBuffer<T> buffer))
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
        /// Gets a NativeArray of all the entities in an IBufferElementData reference buffer component
        /// </summary>
        /// <param name="type">Type - The IBufferElementData struct</param>
        /// <param name="entities">out NativeArray<Entity> - the resulting array of entities</param>
        public void GetReferenceBufferEntitiesByType(Type type, out NativeArray<Entity> entities)
        {
            FieldInfo field = GetEntityReferenceField(type);

            GetBufferComponentsByType(type, out List<IBufferElementData> buffer);
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
            SetComponentBufferReferenceByType(_entity, type, index, element);
        }

        public static void SetComponentBufferReferenceByType(Entity entity, Type type, int index, Entity element)
        {
            FieldInfo field = GetEntityReferenceField(type);

            MethodInfo method = typeof(EntityManager).GetMethod(nameof(s_manager.GetBuffer), new Type[] { typeof(Entity), typeof(bool) });
            MethodInfo generic = method.MakeGenericMethod(type);
            try
            {
                DynamicBuffer<Game.Areas.SubArea> bufferValue = (DynamicBuffer<Game.Areas.SubArea>)generic.Invoke(s_manager, new object[] { entity, false });

                BufferUpdateIterate<Game.Areas.SubArea>(entity, (subArea, i) =>
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

        #region Basic Components

        public bool HasComponentByType(Type type)
        {
            MethodInfo hasComponent = typeof(EntityManager).GetMethod(nameof(s_manager.HasComponent), new Type[] { typeof(Entity) });
            MethodInfo generic = hasComponent.MakeGenericMethod(type);
            return (bool)generic.Invoke(s_manager, new object[] { _entity });
        }

        public IComponentData GetComponentDataByType(Type type)
        {
            MethodInfo getComponentData = typeof(EntityManager).GetMethod(nameof(s_manager.GetComponentData), new Type[] { typeof(Entity) });
            MethodInfo generic = getComponentData.MakeGenericMethod(type);
            return (IComponentData)generic.Invoke(s_manager, new object[] { _entity });
        }

        public void SetComponentDataByType(Type type, IComponentData comp)
        {
            MethodInfo generic = s_EntityManagerMethods["SetComponentData"].MakeGenericMethod(type);
            generic.Invoke(s_manager, new object[] { _entity, comp });
        }

        public void AddComponentDataByType(Type type, IComponentData data)
        {
            MethodInfo generic = s_EntityManagerMethods["AddComponentData"].MakeGenericMethod(type);
            generic.Invoke(s_manager, new object[] { _entity, data });
        }

        public void RemoveComponentByType(Type type)
        {
            MethodInfo removeComponent = typeof(EntityManager).GetMethod(nameof(s_manager.RemoveComponent), new Type[] { typeof(Entity) });
            MethodInfo generic = removeComponent.MakeGenericMethod(type);
            generic.Invoke(s_manager, new object[] { _entity });
        }

        #endregion

        #region Buffers

        public static bool HasBufferByType(Entity entity, Type type)
        {
            MethodInfo hasBuffer = typeof(EntityManager).GetMethod(nameof(s_manager.HasBuffer), new Type[] { typeof(Entity) });
            MethodInfo generic = hasBuffer.MakeGenericMethod(type);
            return (bool)generic.Invoke(s_manager, new object[] { entity });
        }

        /// <summary>
        /// Gets a list of all the components in reference buffer
        /// </summary>
        /// <param name="type">Type - Extension for the IBufferElementData struct</param>
        /// <param name="components">out List<IBufferElementData> - The reference buffer's components</param>
        /// <param name="isReadOnly">Bool - Should these buffer components be read only?</param>
        /// <returns>List<IBufferElementData> - The reference buffer's components</returns>
        public void GetBufferComponentsByType(Type type, out List<IBufferElementData> components, bool isReadOnly = true)
        {
            GetBufferComponentsByType(_entity, type, out components, isReadOnly);
        }

        public static void GetBufferComponentsByType(Entity entity, Type type, out List<IBufferElementData> components, bool isReadOnly = true)
        {
            MethodInfo method = typeof(EntityManager).GetMethod(nameof(s_manager.GetBuffer), new Type[] { typeof(Entity), typeof(bool) });
            MethodInfo generic = method.MakeGenericMethod(type);
            object bufferValue = generic.Invoke(s_manager, new object[] { entity, isReadOnly });
            IEnumerable data = (IEnumerable)bufferValue.GetType().GetMethod("AsNativeArray", BindingFlags.Instance | BindingFlags.Public).Invoke(bufferValue, null);
            components = new List<IBufferElementData>();
            foreach (object o in data)
            {
                components.Add((IBufferElementData)o);
            }
            IDisposable disposable = data as IDisposable;
            disposable.Dispose();
        }

        #endregion

        #endregion
    }
}
