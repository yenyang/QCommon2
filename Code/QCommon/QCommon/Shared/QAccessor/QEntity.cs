using Colossal.Mathematics;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;

namespace QCommonLib.QAccessor
{
    public interface IQLookupContainer
    {
        void Init(SystemBase system);
        void Update(SystemBase system);
    }

    public struct QLookup : IQLookupContainer
    {
        internal BufferLookup<Game.Areas.Node> gaNode;
        internal ComponentLookup<Game.Areas.Geometry> gaGeometry;
        internal ComponentLookup<Game.Common.Updated> gcUpdated;
        internal ComponentLookup<Game.Net.Curve> gnCurve;
        internal ComponentLookup<Game.Net.EdgeGeometry> gnEdgeGeometry;
        internal ComponentLookup<Game.Net.EndNodeGeometry> gnEndNodeGeometry;
        internal ComponentLookup<Game.Net.Node> gnNode;
        internal ComponentLookup<Game.Net.NodeGeometry> gnNodeGeometry;
        internal ComponentLookup<Game.Net.StartNodeGeometry> gnStartNodeGeometry;
        internal ComponentLookup<Game.Objects.Transform> goTransform;
        internal ComponentLookup<Game.Prefabs.ObjectGeometryData> gpObjectGeometryData;
        internal ComponentLookup<Game.Rendering.CullingInfo> grCullingInfo;

        public void Init(SystemBase system)
        {
            gaNode = system.GetBufferLookup<Game.Areas.Node>();
            gaGeometry = system.GetComponentLookup<Game.Areas.Geometry>();
            gcUpdated = system.GetComponentLookup<Game.Common.Updated>();
            gnCurve = system.GetComponentLookup<Game.Net.Curve>();
            gnEdgeGeometry = system.GetComponentLookup<Game.Net.EdgeGeometry>();
            gnEndNodeGeometry = system.GetComponentLookup<Game.Net.EndNodeGeometry>();
            gnNode = system.GetComponentLookup<Game.Net.Node>();
            gnNodeGeometry = system.GetComponentLookup<Game.Net.NodeGeometry>();
            gnStartNodeGeometry = system.GetComponentLookup<Game.Net.StartNodeGeometry>();
            goTransform = system.GetComponentLookup<Game.Objects.Transform>();
            gpObjectGeometryData = system.GetComponentLookup<Game.Prefabs.ObjectGeometryData>();
            grCullingInfo = system.GetComponentLookup<Game.Rendering.CullingInfo>();
        }

        public void Update(SystemBase system)
        {
            gaNode.Update(system);
            gaGeometry.Update(system);
            gcUpdated.Update(system);
            gnCurve.Update(system);
            gnEdgeGeometry.Update(system);
            gnEndNodeGeometry.Update(system);
            gnNode.Update(system);
            gnNodeGeometry.Update(system);
            gnStartNodeGeometry.Update(system);
            goTransform.Update(system);
            gpObjectGeometryData.Update(system);
            grCullingInfo.Update(system);
        }
    }

    public struct QEntity
    {
        internal readonly EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        public Entity m_Entity;
        internal float3 m_OriginPosition;
        internal bool m_IsTopLevel;
        internal QLookup m_Lookup;

        internal QEntity(QLookup lookup, Entity e)
        {
            m_Lookup = lookup;

            m_Entity = e;
            m_OriginPosition = float.MaxValue;
            m_IsTopLevel = false;
        }

        public float3 Position
        {
            get
            {
                StringBuilder sb = new($"Pos.GET " + m_Entity.D() + ": ");
                float3 result;

                if (m_Lookup.goTransform.HasComponent(m_Entity))
                {
                    sb.Append($"goTransform");
                    result = m_Lookup.goTransform.GetRefRO(m_Entity).ValueRO.m_Position;
                }
                else if (m_Lookup.gaGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gaGeometry");
                    result = m_Lookup.gaGeometry.GetRefRO(m_Entity).ValueRO.m_CenterPosition;
                }
                else if (m_Lookup.gpObjectGeometryData.HasComponent(m_Entity))
                {
                    sb.Append($"gpObjectGeometryData");
                    result = m_Lookup.gpObjectGeometryData.GetRefRO(m_Entity).ValueRO.m_Pivot;
                }
                else if (m_Lookup.gnNode.HasComponent(m_Entity))
                {
                    sb.Append($"gnNode");
                    result = m_Lookup.gnNode.GetRefRO(m_Entity).ValueRO.m_Position;
                }
                else if (m_Lookup.gnNodeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnNodeGeometry");
                    result = m_Lookup.gnNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Position;
                }

                // The following are not central and should only be used for calculating movement delta
                else if (m_Lookup.grCullingInfo.HasComponent(m_Entity))
                {
                    sb.Append($"grCullingInfo");
                    result = m_Lookup.grCullingInfo.GetRefRO(m_Entity).ValueRO.m_Bounds.min;
                }
                else if (m_Lookup.gnEdgeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnEdgeGeometry");
                    result = m_Lookup.gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds.min;
                }
                else if (m_Lookup.gnEndNodeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnEndNodeGeometry");
                    result = m_Lookup.gnEndNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry.m_Bounds.min;
                }
                else if (m_Lookup.gnNodeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnNodeGeometry");
                    result = m_Lookup.gnNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds.min;
                }
                else if (m_Lookup.gnStartNodeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnStartNodeGeometry");
                    result = m_Lookup.gnStartNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry.m_Bounds.min;
                }
                else if (m_Lookup.gpObjectGeometryData.HasComponent(m_Entity))
                {
                    sb.Append($"gpObjectGeometryData");
                    result = m_Lookup.gpObjectGeometryData.GetRefRO(m_Entity).ValueRO.m_Bounds.min;
                }
                else if (m_Lookup.gnCurve.HasComponent(m_Entity))
                {
                    sb.Append($"gnCurve");
                    result = m_Lookup.gnCurve.GetRefRO(m_Entity).ValueRO.m_Bezier.a;
                }

                // The following are buffers, using first element, very approximate
                else if (m_Lookup.gaNode.HasBuffer(m_Entity) && m_Lookup.gaNode.TryGetBuffer(m_Entity, out DynamicBuffer<Game.Areas.Node> buffer) && buffer.Length > 0)
                {
                    sb.Append($"gaNode");
                    result = buffer[0].m_Position;
                }

                else
                {
                    sb.Append($"notFound");
                    //QLog.Debug($"Failed to find position for entity {e.D()} '{QCommonLib.QCommon.GetPrefabName(EntityManager, e)}'");
                    result = float3.zero;
                }

                //QLog.Debug(sb.ToString());

                return result;
            }
        }

        public quaternion Rotation
        {
            get
            {
                //StringBuilder sb = new($"Rotation.Get for {m_Entity.D()} '{QCommon.GetPrefabName(EntityManager, m_Entity)}': ");
                quaternion result;

                if (m_Lookup.goTransform.HasComponent(m_Entity))
                {
                    //sb.Append($"goTransform");
                    result = m_Lookup.goTransform.GetRefRO(m_Entity).ValueRO.m_Rotation;
                }
                else if (m_Lookup.gnNode.HasComponent(m_Entity))
                {
                    //sb.Append($"gnNode");
                    result = m_Lookup.gnNode.GetRefRO(m_Entity).ValueRO.m_Rotation;
                }
                else
                {
                    //sb.Append($"notFound");
                    result = quaternion.identity;
                }

                //QLog.Debug(sb.ToString());
                return result;
            }
        }

        public bool MoveBy(float3 delta)
        {
            return Move(Position + delta, delta);
        }

        public bool MoveTo(float3 newPosition)
        {
            return Move(newPosition, newPosition - Position);
        }

        private bool Move(float3 newPosition, float3 delta)
        {
            if (!EntityManager.Exists(m_Entity)) return false;

            StringBuilder sb = new($"Pos.Set {m_Entity.D()} (value:{newPosition.D()}, delta:{delta.D()}, oldPos:{Position.D()}): ");

            if (m_Lookup.gaGeometry.HasComponent(m_Entity))
            {
                sb.Append($"gaGeometry, ");
                m_Lookup.gaGeometry.GetRefRW(m_Entity).ValueRW.m_CenterPosition = newPosition;
                m_Lookup.gaGeometry.GetRefRW(m_Entity).ValueRW.m_Bounds = MoveBounds3(m_Lookup.gaGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
            }

            if (m_Lookup.gnCurve.HasComponent(m_Entity))
            {
                sb.Append($"gnCurve, ");
                m_Lookup.gnCurve.GetRefRW(m_Entity).ValueRW.m_Bezier = MoveBezier4x3(m_Lookup.gnCurve.GetRefRO(m_Entity).ValueRO.m_Bezier, delta);
            }

            if (m_Lookup.gnNode.HasComponent(m_Entity))
            {
                sb.Append($"gnNode, ");
                m_Lookup.gnNode.GetRefRW(m_Entity).ValueRW.m_Position = newPosition;
            }

            if (m_Lookup.gnNodeGeometry.HasComponent(m_Entity))
            {
                sb.Append($"gnNodeGeometry, ");
                m_Lookup.gnNodeGeometry.GetRefRW(m_Entity).ValueRW.m_Bounds = MoveBounds3(m_Lookup.gnNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
            }

            if (m_Lookup.gnEdgeGeometry.HasComponent(m_Entity))
            {
                sb.Append($"gnEdgeGeometry, ");
                m_Lookup.gnEdgeGeometry.GetRefRW(m_Entity).ValueRW.m_Start = MoveSegment(m_Lookup.gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_Start, delta);
                m_Lookup.gnEdgeGeometry.GetRefRW(m_Entity).ValueRW.m_End = MoveSegment(m_Lookup.gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_End, delta);
                m_Lookup.gnEdgeGeometry.GetRefRW(m_Entity).ValueRW.m_Bounds = MoveBounds3(m_Lookup.gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
            }

            if (m_Lookup.gnEndNodeGeometry.HasComponent(m_Entity))
            {
                sb.Append($"gnEndNodeGeometry, ");
                m_Lookup.gnEndNodeGeometry.GetRefRW(m_Entity).ValueRW.m_Geometry = MoveEdgeNodeGeometry(m_Lookup.gnEndNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry, delta);
            }

            if (m_Lookup.gnStartNodeGeometry.HasComponent(m_Entity))
            {
                sb.Append($"gnStartNodeGeometry, ");
                m_Lookup.gnStartNodeGeometry.GetRefRW(m_Entity).ValueRW.m_Geometry = MoveEdgeNodeGeometry(m_Lookup.gnStartNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry, delta);
            }

            if (m_Lookup.goTransform.HasComponent(m_Entity))
            {
                sb.Append($"goTransform, ");
                m_Lookup.goTransform.GetRefRW(m_Entity).ValueRW.m_Position = newPosition;
            }

            if (m_Lookup.grCullingInfo.HasComponent(m_Entity))
            {
                sb.Append($"grCullingInfo, ");
                m_Lookup.grCullingInfo.GetRefRW(m_Entity).ValueRW.m_Bounds = MoveBounds3(m_Lookup.grCullingInfo.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
            }

            if (m_Lookup.gpObjectGeometryData.HasComponent(m_Entity))
            {
                sb.Append($"gpObjectGeometryData, ");
                m_Lookup.gpObjectGeometryData.GetRefRW(m_Entity).ValueRW.m_Pivot = newPosition;
            }

            //QLog.Debug(sb.ToString());

            return true;
        }

        public bool RotateBy(quaternion delta)
        {
            return RotateTo(Rotation.Multiply(delta));
        }

        public bool RotateTo(quaternion newRotation)
        {
            //StringBuilder sb = new($"Rotation.Set for {m_Entity.D()} '{QCommon.GetPrefabName(EntityManager, m_Entity)}': ");
            if (m_Lookup.goTransform.HasComponent(m_Entity))
            {
                //sb.Append($"goTransform");
                m_Lookup.goTransform.GetRefRW(m_Entity).ValueRW.m_Rotation = newRotation;
            }
            if (m_Lookup.gnNode.HasComponent(m_Entity))
            {
                //sb.Append($"gnNode");
                m_Lookup.gnNode.GetRefRW(m_Entity).ValueRW.m_Rotation = newRotation;
            }
            //QLog.Debug(sb.ToString());
            return true;
        }

        //public bool RotateBy(float delta)
        //{
        //    return Rotate(delta - AngleD, delta);
        //}

        //public bool RotateTo(float newAngle)
        //{
        //    return Rotate(newAngle, newAngle - AngleD);
        //}

        //private bool Rotate(float newAngle, float delta)
        //{
        //    return false;
        //}

        private readonly Game.Net.Segment MoveSegment(Game.Net.Segment input, float3 delta)
        {
            input.m_Left = MoveBezier4x3(input.m_Left, delta);
            input.m_Right = MoveBezier4x3(input.m_Right, delta);
            return input;
        }

        private readonly Game.Net.EdgeNodeGeometry MoveEdgeNodeGeometry(Game.Net.EdgeNodeGeometry input, float3 delta)
        {
            input.m_Left = MoveSegment(input.m_Left, delta);
            input.m_Right = MoveSegment(input.m_Right, delta);
            input.m_Middle = MoveBezier4x3(input.m_Middle, delta);
            input.m_Bounds = MoveBounds3(input.m_Bounds, delta);
            return input;
        }

        private readonly Bezier4x3 MoveBezier4x3(Bezier4x3 input, float3 delta)
        {
            input.a += delta;
            input.b += delta;
            input.c += delta;
            input.d += delta;
            return input;
        }
        
        private readonly Bounds3 MoveBounds3(Bounds3 input, float3 delta)
        {
            input.min += delta;
            input.max += delta;
            return input;
        }
    }
}
