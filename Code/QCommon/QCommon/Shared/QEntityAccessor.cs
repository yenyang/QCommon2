using Colossal.Mathematics;
using System;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace QCommonLib
{
    public struct QEntityAccessor
    {
        internal readonly EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

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

        //internal bool m_hasPositionsRead;
        //internal bool m_hasPositionsWrite;
        //internal bool m_hasRotationsRead;
        //internal bool m_hasRotationsWrite;
        //internal bool m_hasPositionRead;
        //internal bool m_hasPositionWrite;
        //internal bool m_hasRotationRead;
        //internal bool m_hasRotationWrite;

        public Entity m_Entity;
        internal float3 m_ParentPosition;
        internal bool m_IsTopLevel;

        internal QEntityAccessor(BufferLookup<Game.Areas.Node> areaNode, ComponentLookup<Game.Areas.Geometry> areaGeometry, ComponentLookup<Game.Common.Updated> commonUpdated, ComponentLookup<Game.Net.Curve> netCurve, ComponentLookup<Game.Net.EdgeGeometry> netEdgeGeometry, ComponentLookup<Game.Net.EndNodeGeometry> netEndNodeGeometry, ComponentLookup<Game.Net.Node> netNode, ComponentLookup<Game.Net.NodeGeometry> netNodeGeometry, ComponentLookup<Game.Net.StartNodeGeometry> netStartNodeGeometry, ComponentLookup<Game.Objects.Transform> objectsTransform, ComponentLookup<Game.Prefabs.ObjectGeometryData> prefabsObjectGeometryData, ComponentLookup<Game.Rendering.CullingInfo> renderingCullingInfo, Entity e)
        {
            gaNode = areaNode;
            gaGeometry = areaGeometry;
            gcUpdated = commonUpdated;
            gnCurve = netCurve;
            gnEdgeGeometry = netEdgeGeometry;
            gnEndNodeGeometry = netEndNodeGeometry;
            gnNode = netNode;
            gnNodeGeometry = netNodeGeometry;
            gnStartNodeGeometry = netStartNodeGeometry;
            goTransform = objectsTransform;
            gpObjectGeometryData = prefabsObjectGeometryData;
            grCullingInfo = renderingCullingInfo;

            m_Entity = e;
            m_ParentPosition = float.MaxValue;
            m_IsTopLevel = false;

            //m_hasPositionsRead = m_hasPositionsWrite = false;
            //m_hasRotationsRead = m_hasRotationsWrite = false;
            //m_hasPositionRead = m_hasPositionWrite = false;
            //m_hasRotationRead = m_hasRotationWrite = false;

            //if (areaNode.HasBuffer(e))
            //{
            //    m_hasPositionsRead = true;
            //    m_hasPositionsWrite = true;
            //    m_hasRotationsWrite = true;
            //}
            //else
            //{
            //    if (objectsTransform.HasComponent(e) || netNode.HasComponent(e))
            //    {
            //        m_hasPositionRead = true;
            //        m_hasPositionWrite = true;
            //        m_hasRotationRead = true;
            //        m_hasRotationWrite = true;
            //        return;
            //    }
            //    if (areaGeometry.HasComponent(e))
            //    {
            //        m_hasPositionRead = true;
            //        m_hasPositionWrite = true;
            //    }
            //    if (netEdgeGeometry.HasComponent(e) || netEndNodeGeometry.HasComponent(e) || netNodeGeometry.HasComponent(e) || netStartNodeGeometry.HasComponent(e))
            //    {
            //        m_hasPositionRead = true;
            //        m_hasPositionWrite = true;
            //    }
            //    if (netCurve.HasComponent(e) || prefabsObjectGeometryData.HasComponent(e) || renderingCullingInfo.HasComponent(e))
            //    {
            //        m_hasPositionRead = true;
            //        m_hasPositionWrite = true;
            //    }
            //}
        }

        //internal readonly bool Valid => m_hasPositionsRead || m_hasPositionsWrite || m_hasRotationsRead || m_hasRotationsWrite || m_hasPositionRead || m_hasPositionWrite || m_hasRotationRead || m_hasRotationWrite;
        //internal readonly bool ValidFlat => m_hasPositionRead || m_hasPositionWrite || m_hasRotationRead || m_hasRotationWrite;
        //internal readonly string ValidDebug => new(new char[]
        //{
        //    m_hasPositionsRead  ? '1' : '0',
        //    m_hasPositionsWrite ? '1' : '0',
        //    m_hasRotationsRead  ? '1' : '0',
        //    m_hasRotationsWrite ? '1' : '0',
        //    m_hasPositionRead   ? '1' : '0',
        //    m_hasPositionWrite  ? '1' : '0',
        //    m_hasRotationRead   ? '1' : '0',
        //    m_hasRotationWrite  ? '1' : '0',
        //});

        //public NativeArray<float3> Positions
        //{
        //    get
        //    {
        //        NativeArray<float3> result;

        //        //StringBuilder sb = new($"Positions.Get for {e.D()} '{QCommon.GetPrefabName(EntityManager, e)}': ");

        //        if (gaNode.HasBuffer(m_Entity))
        //        {
        //            if (gaNode.TryGetBuffer(m_Entity, out DynamicBuffer<Game.Areas.Node> buffer))
        //            {
        //                result = new NativeArray<float3>(buffer.Length, Allocator.TempJob);
        //                for (int i = 0; i < buffer.Length; i++)
        //                {
        //                    result[i] = buffer[i].m_Position;
        //                }
        //                //sb.Append($"gaNode(yes)");
        //            }
        //            else
        //            {
        //                result = new();
        //                //sb.Append($"gaNode(no)");
        //            }
        //        }
        //        else
        //        {
        //            result = new();
        //            //sb.Append($"notFound");
        //        }
        //        //QLog.Debug(sb.ToString());

        //        return result;
        //    }

        //    set
        //    {
        //        //StringBuilder sb = new($"Positions.Set for {m_Entity.D()} '{QCommon.GetPrefabName(EntityManager, m_Entity)}': ");
        //        if (gaNode.HasBuffer(m_Entity))
        //        {
        //            if (gaNode.TryGetBuffer(m_Entity, out DynamicBuffer<Game.Areas.Node> buffer))
        //            {
        //                if (buffer.Length != value.Length)
        //                {
        //                    throw new Exception($"Invalid Positions.set data (length:{value.Length}, should be {buffer.Length} for {m_Entity.D()}.");
        //                }

        //                for (int i = 0; i < buffer.Length; i++)
        //                {
        //                    Game.Areas.Node node = buffer[i];
        //                    node.m_Position = value[i];
        //                    buffer[i] = node;
        //                }
        //                //sb.Append($"gaNode");
        //            }
        //            else
        //            {
        //                throw new Exception($"Failed to save Game.Areas.Nodes Positions.set data for {m_Entity.D()}");
        //            }
        //        }
        //        //QLog.Debug(sb.ToString());
        //    }
        //}

        public float3 Position
        {
            get
            {
                StringBuilder sb = new($"Pos.GET " + m_Entity.D() + ": ");
                float3 result;

                if (goTransform.HasComponent(m_Entity))
                {
                    sb.Append($"goTransform");
                    result = goTransform.GetRefRO(m_Entity).ValueRO.m_Position;
                }
                else if (gaGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gaGeometry");
                    result = gaGeometry.GetRefRO(m_Entity).ValueRO.m_CenterPosition;
                }
                else if (gpObjectGeometryData.HasComponent(m_Entity))
                {
                    sb.Append($"gpObjectGeometryData");
                    result = gpObjectGeometryData.GetRefRO(m_Entity).ValueRO.m_Pivot;
                }
                else if (gnNode.HasComponent(m_Entity))
                {
                    sb.Append($"gnNode");
                    result = gnNode.GetRefRO(m_Entity).ValueRO.m_Position;
                }
                else if (gnNodeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnNodeGeometry");
                    result = gnNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Position;
                }

                // The following are not central and should only be used for calculating movement delta
                else if (grCullingInfo.HasComponent(m_Entity))
                {
                    sb.Append($"grCullingInfo");
                    result = grCullingInfo.GetRefRO(m_Entity).ValueRO.m_Bounds.min;
                }
                else if (gnEdgeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnEdgeGeometry");
                    result = gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds.min;
                }
                else if (gnEndNodeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnEndNodeGeometry");
                    result = gnEndNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry.m_Bounds.min;
                }
                else if (gnNodeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnNodeGeometry");
                    result = gnNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds.min;
                }
                else if (gnStartNodeGeometry.HasComponent(m_Entity))
                {
                    sb.Append($"gnStartNodeGeometry");
                    result = gnStartNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry.m_Bounds.min;
                }
                else if (gpObjectGeometryData.HasComponent(m_Entity))
                {
                    sb.Append($"gpObjectGeometryData");
                    result = gpObjectGeometryData.GetRefRO(m_Entity).ValueRO.m_Bounds.min;
                }
                else if (gnCurve.HasComponent(m_Entity))
                {
                    sb.Append($"gnCurve");
                    result = gnCurve.GetRefRO(m_Entity).ValueRO.m_Bezier.a;
                }

                // The following are buffers, using first element, very approximate
                else if (gaNode.HasBuffer(m_Entity) && gaNode.TryGetBuffer(m_Entity, out DynamicBuffer<Game.Areas.Node> buffer) && buffer.Length > 0)
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

            //set
            //{
            //    if (!EntityManager.Exists(m_Entity)) return;

            //    float3 delta = value - Position;

            //    StringBuilder sb = new($"Pos.Set {m_Entity.D()} (value:{value.D()}, delta:{delta.D()}, oldPos:{Position.D()}): ");

            //    if (gaGeometry.HasComponent(m_Entity))
            //    {
            //        sb.Append($"gaGeometry, ");
            //        gaGeometry.GetRefRW(m_Entity).ValueRW.m_CenterPosition = value;
            //        gaGeometry.GetRefRW(m_Entity).ValueRW.m_Bounds = MoveBounds3(gaGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
            //    }

            //    if (gnCurve.HasComponent(m_Entity))
            //    {
            //        sb.Append($"gnCurve, ");
            //        gnCurve.GetRefRW(m_Entity).ValueRW.m_Bezier = MoveBezier4x3(gnCurve.GetRefRO(m_Entity).ValueRO.m_Bezier, delta);
            //    }

            //    if (gnNode.HasComponent(m_Entity))
            //    {
            //        sb.Append($"gnNode, ");
            //        gnNode.GetRefRW(m_Entity).ValueRW.m_Position = value;
            //    }

            //    if (gnNodeGeometry.HasComponent(m_Entity))
            //    {
            //        sb.Append($"gnNodeGeometry, ");
            //        gnNodeGeometry.GetRefRW(m_Entity).ValueRW.m_Bounds = MoveBounds3(gnNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
            //    }

            //    if (gnEdgeGeometry.HasComponent(m_Entity))
            //    {
            //        sb.Append($"gnEdgeGeometry, ");
            //        gnEdgeGeometry.GetRefRW(m_Entity).ValueRW.m_Start = MoveSegment(gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_Start, delta);
            //        gnEdgeGeometry.GetRefRW(m_Entity).ValueRW.m_End = MoveSegment(gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_End, delta);
            //        gnEdgeGeometry.GetRefRW(m_Entity).ValueRW.m_Bounds = MoveBounds3(gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
            //    }

            //    if (gnEndNodeGeometry.HasComponent(m_Entity))
            //    {
            //        sb.Append($"gnEndNodeGeometry, ");
            //        gnEndNodeGeometry.GetRefRW(m_Entity).ValueRW.m_Geometry = MoveEdgeNodeGeometry(gnEndNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry, delta);
            //    }

            //    if (gnStartNodeGeometry.HasComponent(m_Entity))
            //    {
            //        sb.Append($"gnStartNodeGeometry, ");
            //        gnStartNodeGeometry.GetRefRW(m_Entity).ValueRW.m_Geometry = MoveEdgeNodeGeometry(gnStartNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry, delta);
            //    }

            //    if (goTransform.HasComponent(m_Entity))
            //    {
            //        sb.Append($"goTransform, ");
            //        goTransform.GetRefRW(m_Entity).ValueRW.m_Position = value;
            //    }

            //    if (grCullingInfo.HasComponent(m_Entity))
            //    {
            //        sb.Append($"grCullingInfo, ");
            //        grCullingInfo.GetRefRW(m_Entity).ValueRW.m_Bounds = MoveBounds3(grCullingInfo.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
            //    }

            //    if (gpObjectGeometryData.HasComponent(m_Entity))
            //    {
            //        sb.Append($"gpObjectGeometryData, ");
            //        gpObjectGeometryData.GetRefRW(m_Entity).ValueRW.m_Pivot = value;
            //    }

            //    //QLog.Debug(sb.ToString());
            //}
        }

        public float AngleD
        {
            get
            {
                //QLog.Debug($"AngleD.Get for {m_Entity.D()} '{QCommon.GetPrefabName(EntityManager, m_Entity)}': {Rotation.ToEulerDegrees().y}");
                return Rotation.ToEulerDegrees().y;
            }
            //set
            //{
            //    float3 a = Rotation.ToEulerDegrees();
            //    a.y = value;
            //    Rotation = quaternion.EulerXYZ(a * Mathf.Deg2Rad);
            //    //QLog.Debug($"AngleD.Set for {m_Entity.D()} '{QCommon.GetPrefabName(EntityManager, m_Entity)}': {a.y}, {Rotation.ToEulerDegrees().y}");
            //}
        }

        //public NativeArray<quaternion> Rotations
        //{
        //    get
        //    {
        //        // Presently no readable or writable rotating buffers
        //        return new NativeArray<quaternion>();
        //    }
        //}

        public quaternion Rotation
        {
            get
            {
                //StringBuilder sb = new($"Rotation.Get for {m_Entity.D()} '{QCommon.GetPrefabName(EntityManager, m_Entity)}': ");
                quaternion result;

                if (goTransform.HasComponent(m_Entity))
                {
                    //sb.Append($"goTransform");
                    result = goTransform.GetRefRO(m_Entity).ValueRO.m_Rotation;
                }
                else if (gnNode.HasComponent(m_Entity))
                {
                    //sb.Append($"gnNode");
                    result = gnNode.GetRefRO(m_Entity).ValueRO.m_Rotation;
                }
                else
                {
                    //sb.Append($"notFound");
                    //QLog.Debug($"Failed to find position for entity {e.D()} '{QCommonLib.QCommon.GetPrefabName(EntityManager, e)}'");
                    result = quaternion.identity;
                }

                //QLog.Debug(sb.ToString());
                return result;
            }

            //set
            //{
            //    //StringBuilder sb = new($"Rotation.Set for {m_Entity.D()} '{QCommon.GetPrefabName(EntityManager, m_Entity)}': ");
            //    if (goTransform.HasComponent(m_Entity))
            //    {
            //        //sb.Append($"goTransform");
            //        goTransform.GetRefRW(m_Entity).ValueRW.m_Rotation = value;
            //    }
            //    if (gnNode.HasComponent(m_Entity))
            //    {
            //        //sb.Append($"gnNode");
            //        gnNode.GetRefRW(m_Entity).ValueRW.m_Rotation = value;
            //    }
            //    //QLog.Debug(sb.ToString());
            //}
        }

        public bool MoveBy(float3 delta)
        {

            return false;
        }

        public bool RotateBy(float delta)
        {

            return false;
        }

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
