using Colossal.Mathematics;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace QCommonLib.QAccessor
{
    public struct QEntity
    {
        internal readonly EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        public Entity m_Entity;
        internal float3 m_OriginPosition;
        internal bool m_IsTopLevel;
        internal QLookup m_Lookup;
        internal QTypes.Types m_Type;

        internal QEntity(Entity e, SystemBase system)
        {
            m_Lookup = QLookup.Get(system);

            m_Entity = e;
            m_OriginPosition = float.MaxValue;
            m_IsTopLevel = false;
            m_Type = QTypes.GetEntityType(e);
        }

        public float3 Position
        {
            get
            {
                StringBuilder sb = new($"Pos.GET " + m_Entity.DX(EntityManager) + ": ");
                float3 result;

                if (m_Type == QTypes.Types.NetSegment)
                {
                    Bezier4x3 bezier = m_Lookup.gnCurve.GetRefRO(m_Entity).ValueRO.m_Bezier;
                    result = (bezier.a + bezier.b + bezier.c + bezier.d) / 4;
                    sb.AppendFormat("Segment:{0}", result.DX());
                    QLog.Bundle("GET", sb.ToString());
                    return result;
                }

                if (m_Lookup.goTransform.HasComponent(m_Entity))
                {
                    sb.Append($"goTransform");
                    result = m_Lookup.goTransform.GetRefRO(m_Entity).ValueRO.m_Position;
                }
                else if (m_Lookup.gnNode.HasComponent(m_Entity))
                {
                    sb.Append($"gnNode");
                    result = m_Lookup.gnNode.GetRefRO(m_Entity).ValueRO.m_Position;
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
                else if (m_Lookup.gnNodeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnNodeGeometry");
                    Game.Net.NodeGeometry nodeGeo = m_Lookup.gnNodeGeometry.GetRefRO(m_Entity).ValueRO;
                    result = nodeGeo.m_Bounds.Center();
                    result.y = nodeGeo.m_Position;
                }

                // The following are not central and should only be used for calculating movement delta
                else if (m_Lookup.grCullingInfo.HasComponent(m_Entity))
                {
                    sb.Append($"grCullingInfo");
                    result = m_Lookup.grCullingInfo.GetRefRO(m_Entity).ValueRO.m_Bounds.Center();
                }
                else if (m_Lookup.gnEdgeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnEdgeGeometry");
                    result = m_Lookup.gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds.Center();
                }
                else if (m_Lookup.gnEndNodeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnEndNodeGeometry");
                    result = m_Lookup.gnEndNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry.m_Bounds.Center();
                }
                else if (m_Lookup.gnNodeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnNodeGeometry");
                    result = m_Lookup.gnNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds.Center();
                }
                else if (m_Lookup.gnStartNodeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnStartNodeGeometry");
                    result = m_Lookup.gnStartNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry.m_Bounds.Center();
                }
                else if (m_Lookup.gpObjectGeometryData.HasComponent(m_Entity))
                {
                    sb.Append($"gpObjectGeometryData");
                    result = m_Lookup.gpObjectGeometryData.GetRefRO(m_Entity).ValueRO.m_Bounds.Center();
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

                sb.AppendFormat(" ({0})", result.DX());

                //QLog.Bundle("GET", sb.ToString());

                return result;
            }
        }

        public float Angle
        {
            get
            {
                return ((Quaternion)Rotation).eulerAngles.y;
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

            //if (m_Type == QTypes.Types.NetSegment) return false;

            StringBuilder sb = new();
            sb.AppendFormat("Pos.Set {0} ({1}, delta:{2}, old:{3}): ", m_Entity.D(), newPosition.DX(), delta.DX(), Position.DX());

            if (m_Lookup.gaGeometry.HasComponent(m_Entity))
            {
                sb.Append($"gaGeo, ");
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
                sb.Append($"gnNodeGeo, ");
                m_Lookup.gnNodeGeometry.GetRefRW(m_Entity).ValueRW.m_Bounds = MoveBounds3(m_Lookup.gnNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
            }

            if (m_Lookup.gnEdgeGeometry.HasComponent(m_Entity))
            {
                sb.Append($"gnEdgeGeo, ");
                m_Lookup.gnEdgeGeometry.GetRefRW(m_Entity).ValueRW.m_Start = MoveSegment(m_Lookup.gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_Start, delta);
                m_Lookup.gnEdgeGeometry.GetRefRW(m_Entity).ValueRW.m_End = MoveSegment(m_Lookup.gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_End, delta);
                m_Lookup.gnEdgeGeometry.GetRefRW(m_Entity).ValueRW.m_Bounds = MoveBounds3(m_Lookup.gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
            }

            if (m_Lookup.gnEndNodeGeometry.HasComponent(m_Entity))
            {
                sb.Append($"gnEndNodeGeo, ");
                m_Lookup.gnEndNodeGeometry.GetRefRW(m_Entity).ValueRW.m_Geometry = MoveEdgeNodeGeometry(m_Lookup.gnEndNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry, delta);
            }

            if (m_Lookup.gnStartNodeGeometry.HasComponent(m_Entity))
            {
                sb.Append($"gnStartNodeGeo, ");
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
                sb.Append($"gpObjGeoData, ");
                m_Lookup.gpObjectGeometryData.GetRefRW(m_Entity).ValueRW.m_Pivot = newPosition;
            }

            if (m_Lookup.gaNode.HasBuffer(m_Entity))
            {
                sb.Append("gaNode");
                if (m_Lookup.gaNode.TryGetBuffer(m_Entity, out var buffer))
                {
                    sb.AppendFormat("({0})", buffer.Length);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        var b = buffer[i];
                        b.m_Position += delta;
                        buffer[i] = b;
                    }
                }
                sb.Append(", ");
            }

            if (m_Lookup.gnConnectedEdge.HasBuffer(m_Entity))
            {
                sb.Append("gnConnEdge");
                if (m_Lookup.gnConnectedEdge.TryGetBuffer(m_Entity, out var buffer))
                {
                    sb.AppendFormat("({0})", buffer.Length);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        Entity edge = buffer[i].m_Edge;
                        var edgeData = EntityManager.GetComponentData<Game.Net.Edge>(edge);
                        bool isStart = edgeData.m_Start.Equals(m_Entity);
                        if (isStart)
                        {
                            m_Lookup.gnStartNodeGeometry.GetRefRW(edge).ValueRW.m_Geometry = MoveEdgeNodeGeometry(m_Lookup.gnStartNodeGeometry.GetRefRO(edge).ValueRO.m_Geometry, delta);
                            m_Lookup.gnEdgeGeometry.GetRefRW(edge).ValueRW.m_Start = MoveSegment(m_Lookup.gnEdgeGeometry.GetRefRO(edge).ValueRO.m_Start, delta);
                            sb.AppendFormat(" S-{0}", edge.D());
                        }
                        else
                        {
                            m_Lookup.gnEndNodeGeometry.GetRefRW(edge).ValueRW.m_Geometry = MoveEdgeNodeGeometry(m_Lookup.gnEndNodeGeometry.GetRefRO(edge).ValueRO.m_Geometry, delta);
                            m_Lookup.gnEdgeGeometry.GetRefRW(edge).ValueRW.m_End = MoveSegment(m_Lookup.gnEdgeGeometry.GetRefRO(edge).ValueRO.m_End, delta);
                            sb.AppendFormat(" E-{0}", edge.D());
                        }

                        /* Game.Net.Segment a, b;
                        a = m_Lookup.gnEdgeGeometry.GetRefRO(edge).ValueRO.m_Start;
                        b = m_Lookup.gnEdgeGeometry.GetRefRO(edge).ValueRO.m_End;
                        float3 min = float.MaxValue;
                        if (a.m_Left.a.x < min.x) min.x = a.m_Left.a.x;
                        if (a.m_Left.b.x < min.x) min.x = a.m_Left.b.x;
                        if (a.m_Left.c.x < min.x) min.x = a.m_Left.c.x;
                        if (a.m_Left.d.x < min.x) min.x = a.m_Left.d.x;
                        if (a.m_Right.a.x < min.x) min.x = a.m_Right.a.x;
                        if (a.m_Right.b.x < min.x) min.x = a.m_Right.b.x;
                        if (a.m_Right.c.x < min.x) min.x = a.m_Right.c.x;
                        if (a.m_Right.d.x < min.x) min.x = a.m_Right.d.x;
                        if (b.m_Left.a.x < min.x) min.x = b.m_Left.a.x;
                        if (b.m_Left.b.x < min.x) min.x = b.m_Left.b.x;
                        if (b.m_Left.c.x < min.x) min.x = b.m_Left.c.x;
                        if (b.m_Left.d.x < min.x) min.x = b.m_Left.d.x;
                        if (b.m_Right.a.x < min.x) min.x = b.m_Right.a.x;
                        if (b.m_Right.b.x < min.x) min.x = b.m_Right.b.x;
                        if (b.m_Right.c.x < min.x) min.x = b.m_Right.c.x;
                        if (b.m_Right.d.x < min.x) min.x = b.m_Right.d.x;
                        if (a.m_Left.a.y < min.y) min.y = a.m_Left.a.y;
                        if (a.m_Left.b.y < min.y) min.y = a.m_Left.b.y;
                        if (a.m_Left.c.y < min.y) min.y = a.m_Left.c.y;
                        if (a.m_Left.d.y < min.y) min.y = a.m_Left.d.y;
                        if (a.m_Right.a.y < min.y) min.y = a.m_Right.a.y;
                        if (a.m_Right.b.y < min.y) min.y = a.m_Right.b.y;
                        if (a.m_Right.c.y < min.y) min.y = a.m_Right.c.y;
                        if (a.m_Right.d.y < min.y) min.y = a.m_Right.d.y;
                        if (b.m_Left.a.y < min.y) min.y = b.m_Left.a.y;
                        if (b.m_Left.b.y < min.y) min.y = b.m_Left.b.y;
                        if (b.m_Left.c.y < min.y) min.y = b.m_Left.c.y;
                        if (b.m_Left.d.y < min.y) min.y = b.m_Left.d.y;
                        if (b.m_Right.a.y < min.y) min.y = b.m_Right.a.y;
                        if (b.m_Right.b.y < min.y) min.y = b.m_Right.b.y;
                        if (b.m_Right.c.y < min.y) min.y = b.m_Right.c.y;
                        if (b.m_Right.d.y < min.y) min.y = b.m_Right.d.y;
                        if (a.m_Left.a.z < min.z) min.z = a.m_Left.a.z;
                        if (a.m_Left.b.z < min.z) min.z = a.m_Left.b.z;
                        if (a.m_Left.c.z < min.z) min.z = a.m_Left.c.z;
                        if (a.m_Left.d.z < min.z) min.z = a.m_Left.d.z;
                        if (a.m_Right.a.z < min.z) min.z = a.m_Right.a.z;
                        if (a.m_Right.b.z < min.z) min.z = a.m_Right.b.z;
                        if (a.m_Right.c.z < min.z) min.z = a.m_Right.c.z;
                        if (a.m_Right.d.z < min.z) min.z = a.m_Right.d.z;
                        if (b.m_Left.a.z < min.z) min.z = b.m_Left.a.z;
                        if (b.m_Left.b.z < min.z) min.z = b.m_Left.b.z;
                        if (b.m_Left.c.z < min.z) min.z = b.m_Left.c.z;
                        if (b.m_Left.d.z < min.z) min.z = b.m_Left.d.z;
                        if (b.m_Right.a.z < min.z) min.z = b.m_Right.a.z;
                        if (b.m_Right.b.z < min.z) min.z = b.m_Right.b.z;
                        if (b.m_Right.c.z < min.z) min.z = b.m_Right.c.z;
                        if (b.m_Right.d.z < min.z) min.z = b.m_Right.d.z;
                        float3 max = float.MinValue;
                        if (a.m_Left.a.x > max.x) max.x = a.m_Left.a.x;
                        if (a.m_Left.b.x > max.x) max.x = a.m_Left.b.x;
                        if (a.m_Left.c.x > max.x) max.x = a.m_Left.c.x;
                        if (a.m_Left.d.x > max.x) max.x = a.m_Left.d.x;
                        if (a.m_Right.a.x > max.x) max.x = a.m_Right.a.x;
                        if (a.m_Right.b.x > max.x) max.x = a.m_Right.b.x;
                        if (a.m_Right.c.x > max.x) max.x = a.m_Right.c.x;
                        if (a.m_Right.d.x > max.x) max.x = a.m_Right.d.x;
                        if (b.m_Left.a.x > max.x) max.x = b.m_Left.a.x;
                        if (b.m_Left.b.x > max.x) max.x = b.m_Left.b.x;
                        if (b.m_Left.c.x > max.x) max.x = b.m_Left.c.x;
                        if (b.m_Left.d.x > max.x) max.x = b.m_Left.d.x;
                        if (b.m_Right.a.x > max.x) max.x = b.m_Right.a.x;
                        if (b.m_Right.b.x > max.x) max.x = b.m_Right.b.x;
                        if (b.m_Right.c.x > max.x) max.x = b.m_Right.c.x;
                        if (b.m_Right.d.x > max.x) max.x = b.m_Right.d.x;
                        if (a.m_Left.a.y > max.y) max.y = a.m_Left.a.y;
                        if (a.m_Left.b.y > max.y) max.y = a.m_Left.b.y;
                        if (a.m_Left.c.y > max.y) max.y = a.m_Left.c.y;
                        if (a.m_Left.d.y > max.y) max.y = a.m_Left.d.y;
                        if (a.m_Right.a.y > max.y) max.y = a.m_Right.a.y;
                        if (a.m_Right.b.y > max.y) max.y = a.m_Right.b.y;
                        if (a.m_Right.c.y > max.y) max.y = a.m_Right.c.y;
                        if (a.m_Right.d.y > max.y) max.y = a.m_Right.d.y;
                        if (b.m_Left.a.y > max.y) max.y = b.m_Left.a.y;
                        if (b.m_Left.b.y > max.y) max.y = b.m_Left.b.y;
                        if (b.m_Left.c.y > max.y) max.y = b.m_Left.c.y;
                        if (b.m_Left.d.y > max.y) max.y = b.m_Left.d.y;
                        if (b.m_Right.a.y > max.y) max.y = b.m_Right.a.y;
                        if (b.m_Right.b.y > max.y) max.y = b.m_Right.b.y;
                        if (b.m_Right.c.y > max.y) max.y = b.m_Right.c.y;
                        if (b.m_Right.d.y > max.y) max.y = b.m_Right.d.y;
                        if (a.m_Left.a.z > max.z) max.z = a.m_Left.a.z;
                        if (a.m_Left.b.z > max.z) max.z = a.m_Left.b.z;
                        if (a.m_Left.c.z > max.z) max.z = a.m_Left.c.z;
                        if (a.m_Left.d.z > max.z) max.z = a.m_Left.d.z;
                        if (a.m_Right.a.z > max.z) max.z = a.m_Right.a.z;
                        if (a.m_Right.b.z > max.z) max.z = a.m_Right.b.z;
                        if (a.m_Right.c.z > max.z) max.z = a.m_Right.c.z;
                        if (a.m_Right.d.z > max.z) max.z = a.m_Right.d.z;
                        if (b.m_Left.a.z > max.z) max.z = b.m_Left.a.z;
                        if (b.m_Left.b.z > max.z) max.z = b.m_Left.b.z;
                        if (b.m_Left.c.z > max.z) max.z = b.m_Left.c.z;
                        if (b.m_Left.d.z > max.z) max.z = b.m_Left.d.z;
                        if (b.m_Right.a.z > max.z) max.z = b.m_Right.a.z;
                        if (b.m_Right.b.z > max.z) max.z = b.m_Right.b.z;
                        if (b.m_Right.c.z > max.z) max.z = b.m_Right.c.z;
                        if (b.m_Right.d.z > max.z) max.z = b.m_Right.d.z;
                        Bounds3 newBounds = new(min, max);
                        m_Lookup.gnEdgeGeometry.GetRefRW(edge).ValueRW.m_Bounds = newBounds; */

                        if (!EntityManager.HasComponent<Game.Common.Updated>(edge)) EntityManager.AddComponent<Game.Common.Updated>(edge);
                    }
                }
                sb.Append(", ");
            }

            EntityManager.AddComponent<Game.Common.Updated>(m_Entity);
            EntityManager.AddComponent<Game.Common.BatchesUpdated>(m_Entity);

            //if (m_Lookup.gnNode.HasComponent(m_Entity))
                QLog.Debug(sb.ToString());

            return true;
        }

        public bool RotateBy(float delta, ref Matrix4x4 matrix, float3 origin)
        {
            return RotateTo(((Quaternion)Rotation).eulerAngles.y + delta, ref matrix, origin);
        }

        public bool RotateTo(float angle, ref Matrix4x4 matrix, float3 origin)
        {
            //StringBuilder sb = new();
            //sb.AppendFormat("Rotation.Set for {0} '{1}': ", m_Entity.D(), QCommon.GetPrefabName(EntityManager, m_Entity));

            quaternion newRotation = Quaternion.Euler(0f, angle, 0f);
            if (m_Lookup.goTransform.HasComponent(m_Entity))
            {
                //sb.Append($"goTransform, ");
                m_Lookup.goTransform.GetRefRW(m_Entity).ValueRW.m_Rotation = newRotation;
                m_Lookup.goTransform.GetRefRW(m_Entity).ValueRW.m_Position = matrix.MultiplyPoint(m_Lookup.goTransform.GetRefRO(m_Entity).ValueRO.m_Position - origin);
            }
            if (m_Lookup.gnNode.HasComponent(m_Entity))
            {
                //sb.Append($"gnNode, ");
                m_Lookup.gnNode.GetRefRW(m_Entity).ValueRW.m_Rotation = newRotation;
                m_Lookup.gnNode.GetRefRW(m_Entity).ValueRW.m_Position = matrix.MultiplyPoint(m_Lookup.gnNode.GetRefRO(m_Entity).ValueRO.m_Position - origin);
            }
            if (m_Lookup.gnCurve.HasComponent(m_Entity))
            {
                //sb.Append("gnCurve, ");
                Bezier4x3 bezier = m_Lookup.gnCurve.GetRefRO(m_Entity).ValueRO.m_Bezier;
                bezier = RotateBezier4x3(bezier, ref matrix, origin);
                m_Lookup.gnCurve.GetRefRW(m_Entity).ValueRW.m_Bezier = bezier;
            }
            if (m_Lookup.gaNode.HasBuffer(m_Entity))
            {
                //sb.Append("gaNode, ");
                if (m_Lookup.gaNode.TryGetBuffer(m_Entity, out var buffer))
                {
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        Game.Areas.Node node = buffer[i];
                        node.m_Position = (float3)matrix.MultiplyPoint(node.m_Position - origin);
                        buffer[i] = node;
                    }
                }
            }

            //QLog.Debug(sb.ToString());
            return true;
        }

        private readonly Game.Net.EdgeNodeGeometry MoveEdgeNodeGeometry(Game.Net.EdgeNodeGeometry input, float3 delta)
        {
            input.m_Left = MoveSegment(input.m_Left, delta);
            input.m_Right = MoveSegment(input.m_Right, delta);
            input.m_Middle = MoveBezier4x3(input.m_Middle, delta);
            input.m_Bounds = MoveBounds3(input.m_Bounds, delta);
            return input;
        }

        private readonly Game.Net.Segment MoveSegment(Game.Net.Segment input, float3 delta)
        {
            input.m_Left = MoveBezier4x3(input.m_Left, delta);
            input.m_Right = MoveBezier4x3(input.m_Right, delta);
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

        private readonly Bezier4x3 RotateBezier4x3(Bezier4x3 input, ref Matrix4x4 matrix, float3 origin)
        {
            input.a = (float3)matrix.MultiplyPoint(input.a - origin);
            input.b = (float3)matrix.MultiplyPoint(input.b - origin);
            input.c = (float3)matrix.MultiplyPoint(input.c - origin);
            input.d = (float3)matrix.MultiplyPoint(input.d - origin);
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
