using Colossal.Entities;
using System;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace QCommonLib.QAccessor
{
    public struct QSegmentEnd
    {
        public Entity m_Entity;
        public bool m_IsStart;
    }

    public struct QNode : IQEntity, IDisposable
    {
        internal readonly EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        public Entity m_Entity;
        internal float3 m_OriginPosition;
        internal bool m_IsTopLevel;
        internal QLookup m_Lookup;
        internal QTypes.Types m_Type;

        internal NativeList<QSegmentEnd> m_Segments;

        internal QNode(Entity e, SystemBase system, QTypes.Types type)
        {
            m_Lookup = QLookup.Get(system);

            m_Entity = e;
            m_OriginPosition = float.MaxValue;
            m_IsTopLevel = false;
            m_Type = type;

            Entity owner = EntityManager.GetComponentData<Game.Common.Owner>(e).m_Owner;

            m_Segments = new NativeList<QSegmentEnd>(0, Allocator.Persistent);
            if (EntityManager.TryGetBuffer<Game.Net.ConnectedEdge>(e, true, out var buffer))
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    Entity segOwner = Entity.Null;
                    if (EntityManager.TryGetComponent(buffer[i].m_Edge, out Game.Common.Owner segOwnerData))
                    {
                        segOwner = segOwnerData.m_Owner;
                    }
                    if (!owner.Equals(segOwner)) continue;

                    Game.Net.Edge edge = EntityManager.GetComponentData<Game.Net.Edge>(buffer[i].m_Edge);
                    if (!e.Equals(edge.m_Start) && !e.Equals(edge.m_End)) continue;

                    QSegmentEnd end = new()
                    {
                        m_Entity = buffer[i].m_Edge,
                        m_IsStart = edge.m_Start.Equals(e),
                    };
                    m_Segments.Add(end);
                }
            }
        }


        public readonly float3 Position
        {
            get
            {
                if (!m_Lookup.gnNode.HasComponent(m_Entity))
                {
                    throw new Exception($"Entity {m_Entity.D()} does not have Net.Node component");
                }
                //QLog.Bundle("NODE", $"Node {m_Entity.D()} is at {m_Lookup.gnNode.GetRefRO(m_Entity).ValueRO.m_Position.DX()}");
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

            for (int i = 0; i < m_Segments.Length; i++)
            {
                MoveSegmentEnd(newPosition, delta, m_Segments[i]);
            }
            EntityManager.AddComponent<Game.Common.Updated>(m_Entity);

            return true;
        }

        private readonly bool MoveSegmentEnd(float3 newPosition, float3 delta, QSegmentEnd end)
        {
            Entity e = end.m_Entity;
            StringBuilder sb = new();
            sb.AppendFormat("SegEnd.Set {0} ({1}, delta:{2}, old:{3}): ", e.D(), newPosition.DX(), delta.DX(), Position.DX());

            Game.Net.Edge edge = EntityManager.GetComponentData<Game.Net.Edge>(e);

            if (m_Lookup.gnCurve.HasComponent(e))
            {
                sb.Append($"gnCurve, ");
                m_Lookup.gnCurve.GetRefRW(e).ValueRW.m_Bezier = QEntity.MoveBezier4x3(m_Lookup.gnCurve.GetRefRO(e).ValueRO.m_Bezier, delta, end.m_IsStart);
            }

            if (m_Lookup.gnEdgeGeometry.HasComponent(e))
            {
                sb.Append($"gnEdgeGeo, ");
                if (end.m_IsStart) m_Lookup.gnEdgeGeometry.GetRefRW(e).ValueRW.m_Start = QEntity.MoveSegment(m_Lookup.gnEdgeGeometry.GetRefRO(e).ValueRO.m_Start, delta, end.m_IsStart);
                if (!end.m_IsStart) m_Lookup.gnEdgeGeometry.GetRefRW(e).ValueRW.m_End = QEntity.MoveSegment(m_Lookup.gnEdgeGeometry.GetRefRO(e).ValueRO.m_End, delta, end.m_IsStart);
                m_Lookup.gnEdgeGeometry.GetRefRW(e).ValueRW.m_Bounds = QEntity.UpdateBounds3(m_Lookup.gnEdgeGeometry.GetRefRO(e).ValueRO);//.m_Bounds, delta, end.m_IsStart);
            }

            if (!end.m_IsStart && m_Lookup.gnEndNodeGeometry.HasComponent(e))
            {
                sb.Append($"gnEndNodeGeo, ");
                m_Lookup.gnEndNodeGeometry.GetRefRW(e).ValueRW.m_Geometry = QEntity.MoveEdgeNodeGeometry(m_Lookup.gnEndNodeGeometry.GetRefRO(e).ValueRO.m_Geometry, delta, end.m_IsStart);
            }
            else if (end.m_IsStart && m_Lookup.gnStartNodeGeometry.HasComponent(e))
            {
                sb.Append($"gnStartNodeGeo, ");
                m_Lookup.gnStartNodeGeometry.GetRefRW(e).ValueRW.m_Geometry = QEntity.MoveEdgeNodeGeometry(m_Lookup.gnStartNodeGeometry.GetRefRO(e).ValueRO.m_Geometry, delta, end.m_IsStart);
            }

            EntityManager.AddComponent<Game.Common.Updated>(e);
            EntityManager.AddComponent<Game.Common.Updated>(end.m_IsStart ? edge.m_End : edge.m_Start);

            //QLog.Debug(sb.ToString());

            return true;
        }


        public readonly bool RotateBy(float delta, ref Matrix4x4 matrix, float3 origin)
        {
            return RotateTo(((Quaternion)Rotation).eulerAngles.y + delta, ref matrix, origin);
        }

        public readonly bool RotateTo(float angle, ref Matrix4x4 matrix, float3 origin)
        {
            //QLog.Debug($"Rotating node {m_Entity.DX()}");
            return false;
        }


        public void Dispose()
        {
            m_Segments.Dispose();
        }
    }
}
