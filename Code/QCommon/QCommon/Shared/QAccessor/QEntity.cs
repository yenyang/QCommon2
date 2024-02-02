using Colossal.Mathematics;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace QCommonLib.QAccessor
{
    public struct QEntity : IQEntity
    {
        internal readonly EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        public Entity m_Entity;
        internal float3 m_OriginPosition;
        internal bool m_IsTopLevel;
        internal QLookup m_Lookup;
        internal QTypes.Types m_Type;

        internal QEntity(Entity e, SystemBase system, QTypes.Types type)
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
                StringBuilder sb = new($"Pos.GET " + m_Entity.DX() + ": ");
                float3 result;

                if (m_Type == QTypes.Types.NetSegment)
                {
                    Bezier4x3 bezier = m_Lookup.gnCurve.GetRefRO(m_Entity).ValueRO.m_Bezier;
                    result = (bezier.a + bezier.b + bezier.c + bezier.d) / 4;
                    //sb.AppendFormat("Segment:{0}", result.DX());
                    //QLog.Bundle("GET", sb.ToString());
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

                // The following might not be central and should only be used for calculating movement delta
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
                    //QLog.Debug($"Failed to find position for entity {e.DX()}");
                    result = float3.zero;
                }

                sb.AppendFormat(" ({0})", result.DX());

                //QLog.Bundle("GET", sb.ToString());

                return result;
            }
        }

        public readonly float Angle
        {
            get
            {
                return ((Quaternion)Rotation).eulerAngles.y;
            }
        }

        public readonly quaternion Rotation
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


        public readonly bool MoveBy(float3 delta)
        {
            return Move(Position + delta, delta);
        }

        public readonly bool Move(float3 newPosition, float3 delta)
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

            /*
             * Networks would need gnCurve, gnNode, gnNodeGeometry, gnEdgeGeometry, gnEndNodeGeometry, and gnStartNodeGeometry
             */

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

            EntityManager.AddComponent<Game.Common.Updated>(m_Entity);
            EntityManager.AddComponent<Game.Common.BatchesUpdated>(m_Entity);

            //QLog.Debug(sb.ToString());

            return true;
        }


        public readonly bool RotateBy(float delta, ref Matrix4x4 matrix, float3 origin)
        {
            return RotateTo(((Quaternion)Rotation).eulerAngles.y + delta, ref matrix, origin);
        }

        public readonly bool RotateTo(float angle, ref Matrix4x4 matrix, float3 origin)
        {
            StringBuilder sb = new();
            sb.AppendFormat("Rotation.Set for {0} '{1}': ", m_Entity.D(), QCommon.GetPrefabName(EntityManager, m_Entity));

            quaternion newRotation = Quaternion.Euler(0f, angle, 0f);
            if (m_Lookup.goTransform.HasComponent(m_Entity))
            {
                sb.Append($"goTransform, ");
                m_Lookup.goTransform.GetRefRW(m_Entity).ValueRW.m_Rotation = newRotation;
                m_Lookup.goTransform.GetRefRW(m_Entity).ValueRW.m_Position = matrix.MultiplyPoint(m_Lookup.goTransform.GetRefRO(m_Entity).ValueRO.m_Position - origin);
            }
            if (m_Lookup.gnNode.HasComponent(m_Entity))
            {
                sb.Append($"gnNode, ");
                m_Lookup.gnNode.GetRefRW(m_Entity).ValueRW.m_Rotation = newRotation;
                m_Lookup.gnNode.GetRefRW(m_Entity).ValueRW.m_Position = matrix.MultiplyPoint(m_Lookup.gnNode.GetRefRO(m_Entity).ValueRO.m_Position - origin);
            }
            if (m_Lookup.gnCurve.HasComponent(m_Entity))
            {
                sb.Append("gnCurve, ");
                Bezier4x3 bezier = m_Lookup.gnCurve.GetRefRO(m_Entity).ValueRO.m_Bezier;
                bezier = RotateBezier4x3(bezier, ref matrix, origin);
                m_Lookup.gnCurve.GetRefRW(m_Entity).ValueRW.m_Bezier = bezier;
            }
            if (m_Lookup.gaNode.HasBuffer(m_Entity))
            {
                sb.Append("gaNode, ");
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


        internal static Game.Net.EdgeNodeGeometry MoveEdgeNodeGeometry(Game.Net.EdgeNodeGeometry input, float3 delta, bool? isStart = null)
        {
            input.m_Left = MoveSegment(input.m_Left, delta, isStart);
            input.m_Right = MoveSegment(input.m_Right, delta, isStart);
            input.m_Middle = MoveBezier4x3(input.m_Middle, delta, isStart);
            input.m_Bounds = UpdateBounds3(input);
            return input;
        }

        internal static Game.Net.EdgeNodeGeometry RotateEdgeNodeGeometry(Game.Net.EdgeNodeGeometry input, ref Matrix4x4 matrix, float3 origin, bool? isStart = null)
        {
            input.m_Left = RotateSegment(input.m_Left, ref matrix, origin, isStart);
            input.m_Right = RotateSegment(input.m_Right, ref matrix, origin, isStart);
            input.m_Middle = RotateBezier4x3(input.m_Middle, ref matrix, origin, isStart);
            input.m_Bounds = UpdateBounds3(input);
            return input;
        }

        internal static Game.Net.Segment MoveSegment(Game.Net.Segment input, float3 delta, bool? isStart = null)
        {
            input.m_Left = MoveBezier4x3(input.m_Left, delta, isStart);
            input.m_Right = MoveBezier4x3(input.m_Right, delta, isStart);
            return input;
        }

        internal static Game.Net.Segment RotateSegment(Game.Net.Segment input, ref Matrix4x4 matrix, float3 origin, bool? isStart = null)
        {
            input.m_Left = RotateBezier4x3(input.m_Left, ref matrix, origin, isStart);
            input.m_Right = RotateBezier4x3(input.m_Right, ref matrix, origin, isStart);
            return input;
        }

        internal static Bezier4x3 MoveBezier4x3(Bezier4x3 input, float3 delta, bool? isStart = null)
        {
            if (isStart != false)
            {
                input.a += delta;
                input.b += delta;
            }
            if (isStart != true)
            {
                input.c += delta;
                input.d += delta;
            }
            return input;
        }

        internal static Bezier4x3 RotateBezier4x3(Bezier4x3 input, ref Matrix4x4 matrix, float3 origin, bool? isStart = null)
        {
            if (isStart != false)
            {
                input.a = (float3)matrix.MultiplyPoint(input.a - origin);
                input.b = (float3)matrix.MultiplyPoint(input.b - origin);
            }
            if (isStart != true)
            {
                input.c = (float3)matrix.MultiplyPoint(input.c - origin);
                input.d = (float3)matrix.MultiplyPoint(input.d - origin);
            }
            return input;
        }

        internal static Bounds3 MoveBounds3(Bounds3 input, float3 delta)
        {
            input.min += delta;
            input.max += delta;
            return input;
        }

        /// <summary>
        /// Must be called after the m_Left and m_Right values are updated!
        /// </summary>
        /// <param name="input">The EdgeNodeGeometry to calculate from</param>
        /// <returns></returns>
        internal static Bounds3 UpdateBounds3(Game.Net.EdgeNodeGeometry input)
        {
            Bounds3 leftLeft = MathUtils.Bounds(input.m_Left.m_Left);
            Bounds3 leftRight = MathUtils.Bounds(input.m_Left.m_Right);
            Bounds3 rightLeft = MathUtils.Bounds(input.m_Right.m_Left);
            Bounds3 rightRight = MathUtils.Bounds(input.m_Right.m_Right);
            return leftLeft.Encapsulate(leftRight.Encapsulate(rightLeft.Encapsulate(rightRight)));
        }


        /// <summary>
        /// Must be called after the m_Start and m_End values are updated!
        /// </summary>
        /// <param name="input">The EdgeGeometry to calculate from</param>
        /// <returns></returns>
        internal static Bounds3 UpdateBounds3(Game.Net.EdgeGeometry input)
        {
            Bounds3 startLeft = MathUtils.Bounds(input.m_Start.m_Left);
            Bounds3 startRight = MathUtils.Bounds(input.m_Start.m_Right);
            Bounds3 endLeft = MathUtils.Bounds(input.m_End.m_Left);
            Bounds3 endRight = MathUtils.Bounds(input.m_End.m_Right);
            return startLeft.Encapsulate(startRight.Encapsulate(endLeft.Encapsulate(endRight)));
        }
    }
}
