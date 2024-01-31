using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace QCommonLib.QAccessor
{
    public struct QNode : IQEntity
    {
        internal readonly EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        public Entity m_Entity;
        internal float3 m_OriginPosition;
        internal bool m_IsTopLevel;
        internal QLookup m_Lookup;
        internal QTypes.Types m_Type;

        internal QNode(Entity e, SystemBase system, QTypes.Types type)
        {
            m_Lookup = QLookup.Get(system);

            m_Entity = e;
            m_OriginPosition = float.MaxValue;
            m_IsTopLevel = false;
            m_Type = type;
        }


        public readonly float3 Position
        {
            get
            {
                if (!m_Lookup.gnNode.HasComponent(m_Entity))
                {
                    throw new Exception($"Entity {m_Entity.D()} does not have Net.Node component");
                }

                return m_Lookup.gnNode.GetRefRO(m_Entity).ValueRO.m_Position;
            }
        }

        public readonly float Angle
        {
            get => Rotation.Y();
        }

        public readonly quaternion Rotation
        {
            get => m_Lookup.gnNode.GetRefRO(m_Entity).ValueRO.m_Rotation;
        }


        public readonly bool MoveBy(float3 delta)
        {
            return Move(Position + delta, delta);
        }

        public readonly bool Move(float3 newPosition, float3 delta)
        {
            if (!EntityManager.Exists(m_Entity)) return false;

            QLog.Debug($"Moving node {m_Entity.DX()}");

            return true;
        }


        public readonly bool RotateBy(float delta, ref Matrix4x4 matrix, float3 origin)
        {
            return RotateTo(((Quaternion)Rotation).eulerAngles.y + delta, ref matrix, origin);
        }

        public readonly bool RotateTo(float angle, ref Matrix4x4 matrix, float3 origin)
        {
            QLog.Debug($"Rotating node {m_Entity.DX()}");
            return false;
        }
    }
}
