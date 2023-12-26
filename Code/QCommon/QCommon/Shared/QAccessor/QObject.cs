using System.Collections.Generic;
using System;
using Unity.Collections;
using Unity.Entities;
using System.Reflection;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace QCommonLib.QAccessor
{
    public struct QReferenceBufferType
    {
        /// <summary>
        /// The type of the buffer component
        /// </summary>
        public Type tParentComponent;
        /// <summary>
        /// The FieldInfo of the buffer component's Entity field
        /// </summary>
        public FieldInfo m_FieldInfo;
    }

    public struct QObject : IDisposable
    {
        internal readonly EntityManager EM => World.DefaultGameObjectInjectionWorld.EntityManager;

        public Entity m_Entity;
        public QEntity m_Accessor;
        internal QLookup m_Lookup;
        internal NativeArray<QEntity> m_Children;
        private readonly List<QReferenceBufferType> _ReferenceBufferTypes;

        internal QObject(QLookup lookup, Entity e)
        {
            _ReferenceBufferTypes = new();
            m_Lookup = lookup;
            m_Entity = e;
            m_Accessor = new(lookup, e);
            m_Children = new();

            _ReferenceBufferTypes = new()
            {
                new() { tParentComponent = typeof(Game.Areas.SubArea),  m_FieldInfo = GetEntityReferenceField(typeof(Game.Areas.SubArea)) },
                new() { tParentComponent = typeof(Game.Net.SubNet),     m_FieldInfo = GetEntityReferenceField(typeof(Game.Net.SubNet)) },
                new() { tParentComponent = typeof(Game.Net.SubLane),    m_FieldInfo = GetEntityReferenceField(typeof(Game.Net.SubLane)) },
            };
            m_Children.Dispose();
            var subEntities = GetSubEntities();

            m_Children = new(subEntities.Count, Allocator.Persistent);
            for (int i = 0; i < subEntities.Count; i++)
            {
                m_Children[i] = new(lookup, subEntities[i]);
            }
        }

        public void Dispose()
        {
            m_Children.Dispose();
        }

        public void MoveBy(float3 delta)
        {
            m_Accessor.MoveBy(delta);
            
            for (int i = 0; i < m_Children.Length; i++)
            {
                m_Children[i].MoveBy(delta);
            }
        }

        public void MoveTo(float3 newPosition)
        {
            m_Accessor.MoveTo(newPosition);

            for (int i = 0; i < m_Children.Length; i++)
            {
                m_Children[i].MoveTo(newPosition);
            }
        }

        public void RotateBy(quaternion delta)
        {
            m_Accessor.RotateBy(delta);

            if (m_Children.Length > 0)
            {
                Matrix4x4 matrix = GetMatrix(delta);
                for (int i = 0; i < m_Children.Length; i++)
                {
                    m_Children[i].RotateBy(delta);
                    float3 newPos = (float3)matrix.MultiplyPoint(m_Children[i].Position);
                    m_Children[i].MoveTo(newPos);
                }
            }
        }

        public void RotateTo(quaternion newRotation)
        {
            m_Accessor.RotateTo(newRotation);

            if (m_Children.Length > 0)
            {
                quaternion delta = newRotation.Multiply(m_Accessor.Rotation.Inverse());
                Matrix4x4 matrix = GetMatrix(delta);
                for (int i = 0; i < m_Children.Length; i++)
                {
                    m_Children[i].RotateBy(delta);
                    float3 newPos = (float3)matrix.MultiplyPoint(m_Children[i].Position);
                    m_Children[i].MoveTo(newPos);
                }
            }
        }

        private Matrix4x4 GetMatrix(quaternion delta)
        {
            float3 origin = m_Accessor.Position;
            Matrix4x4 matrix = default;
            matrix.SetTRS(origin, delta, Vector3.one);
            return matrix;
        }

        internal void DebugDumpAll()
        {
            StringBuilder sb = new("Parent:" + m_Entity.D() + ", children: " + m_Children.Length);

            for (int i = 0; i < m_Children.Length; i++)
            {
                sb.AppendFormat("\n    {0}: \"{1}\"",
                    m_Children[i].m_Entity.D(), 
                    QCommon.GetPrefabName(EM, m_Children[i].m_Entity)
                );
            }

            QLog.Debug(sb.ToString());
        }

        #region Load children
        private readonly List<Entity> GetSubEntities()
        {
            List<Entity> entities = IterateSubEntities(m_Entity, m_Entity, 0);

            return entities;
        }

        private readonly List<Entity> IterateSubEntities(Entity top, Entity e, int depth)
        {
            if (depth > 3) throw new Exception($"Moveable.IterateSubEntities depth ({depth}) too deep for E{top.D()}/E{e.D()}");
            depth++;

            List<Entity> entities = new();

            foreach (QReferenceBufferType type in _ReferenceBufferTypes)
            {
                if (QByType.HasBuffer(type.tParentComponent, e))
                {
                    QByType.GetRefBufferComponents(type.tParentComponent, e, out List<IBufferElementData> buffer, true);
                    foreach (IBufferElementData element in buffer)
                    {
                        Entity sub = (Entity)type.m_FieldInfo.GetValue(element);
                        if (!entities.Contains(sub))
                        {
                            entities.Add(sub);
                            entities.AddRange(IterateSubEntities(top, sub, depth));
                        }
                        else
                        {
                            QLog.Debug($"Duplicate subEntity found: {sub.D()}");
                        }
                    }
                }
            }

            return entities;
        }
        #endregion

        #region Low level entity access
        /// <summary>
        /// Get the field in an IBufferElementData reference buffer component that holds the actual entity reference
        /// For example for Game.Areas.SubArea.m_area, it returns m_area
        /// </summary>
        /// <param name="type">The IBufferElementData struct type to search</param>
        /// <param name="index">How many entity fields to skip over</param>
        /// <returns>FieldInfo of this field</returns>
        /// <exception cref="Exception">If no such field is found</exception>
        public readonly FieldInfo GetEntityReferenceField(Type type, int index = 0)
        {
            int c = 0;
            FieldInfo field = null;
            foreach (FieldInfo f in type.GetFields())
            {
                if (f.FieldType == typeof(Entity))
                {
                    if (c == index)
                    {
                        field = f;
                        break;
                    }
                    else
                    {
                        c++;
                    }
                }
            }
            if (field == null) throw new Exception($"Entity field not found for type {type}");
            return field;
        }
        #endregion
    }
}
