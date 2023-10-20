using Colossal.Mathematics;
using System;
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

        internal bool m_hasPositionsRead;
        internal bool m_hasPositionsWrite;
        internal bool m_hasRotationsRead;
        internal bool m_hasRotationsWrite;
        internal bool m_hasPositionRead;
        internal bool m_hasPositionWrite;
        internal bool m_hasRotationRead;
        internal bool m_hasRotationWrite;

        public Entity m_Entity;
        internal float3 m_ParentPosition;

        internal QEntityAccessor(BufferLookup<Game.Areas.Node> gaNode, ComponentLookup<Game.Areas.Geometry> gaGeometry, ComponentLookup<Game.Common.Updated> gcUpdated, ComponentLookup<Game.Net.Curve> gnCurve, ComponentLookup<Game.Net.EdgeGeometry> gnEdgeGeometry, ComponentLookup<Game.Net.EndNodeGeometry> gnEndNodeGeometry, ComponentLookup<Game.Net.Node> gnNode, ComponentLookup<Game.Net.NodeGeometry> gnNodeGeometry, ComponentLookup<Game.Net.StartNodeGeometry> gnStartNodeGeometry, ComponentLookup<Game.Objects.Transform> goTransform, ComponentLookup<Game.Prefabs.ObjectGeometryData> gpObjectGeometryData, ComponentLookup<Game.Rendering.CullingInfo> grCullingInfo, Entity e, float3 parentPosition)
        {
            this.gaNode = gaNode;
            this.gaGeometry = gaGeometry;
            this.gcUpdated = gcUpdated;
            this.gnCurve = gnCurve;
            this.gnEdgeGeometry = gnEdgeGeometry;
            this.gnEndNodeGeometry = gnEndNodeGeometry;
            this.gnNode = gnNode;
            this.gnNodeGeometry = gnNodeGeometry;
            this.gnStartNodeGeometry = gnStartNodeGeometry;
            this.goTransform = goTransform;
            this.gpObjectGeometryData = gpObjectGeometryData;
            this.grCullingInfo = grCullingInfo;
            m_hasPositionRead = m_hasPositionWrite = false;
            m_hasRotationRead = m_hasRotationWrite = false;
            this.m_Entity = e;

            m_ParentPosition = parentPosition;

            m_hasPositionsRead = m_hasPositionsWrite = false;
            m_hasRotationsRead = m_hasRotationsWrite = false;
            m_hasPositionRead = m_hasPositionWrite = false;
            m_hasRotationRead = m_hasRotationWrite = false;

            if (gaNode.HasBuffer(e))
            {
                m_hasPositionsRead = true;
                m_hasPositionsWrite = true;
                m_hasRotationsWrite = true;
            }
            else
            {
                if (goTransform.HasComponent(e))
                {
                    m_hasPositionRead = true;
                    m_hasPositionWrite = true;
                    m_hasRotationRead = true;
                    m_hasRotationWrite = true;
                    return;
                }
                if (gaGeometry.HasComponent(e))
                {
                    m_hasPositionRead = true;
                    m_hasPositionWrite = true;
                }
                if (gnEdgeGeometry.HasComponent(e) || gnEndNodeGeometry.HasComponent(e) || gnNodeGeometry.HasComponent(e) || gnStartNodeGeometry.HasComponent(e))
                {
                    m_hasPositionWrite = true;
                }
                if (gnNode.HasComponent(e))
                {
                    m_hasPositionRead = true;
                    m_hasPositionWrite = true;
                }
                if (gnCurve.HasComponent(e) || gpObjectGeometryData.HasComponent(e) || grCullingInfo.HasComponent(e))
                {
                    m_hasPositionWrite = true;
                }
            }
        }

        internal readonly bool Valid => m_hasPositionsRead || m_hasPositionsWrite || m_hasRotationsRead || m_hasRotationsWrite || m_hasPositionRead || m_hasPositionWrite || m_hasRotationRead || m_hasRotationWrite;
        internal readonly bool ValidFlat => m_hasPositionRead || m_hasPositionWrite || m_hasRotationRead || m_hasRotationWrite;
        internal readonly string ValidDebug => new(new char[]
        {
            m_hasPositionsRead  ? '1' : '0',
            m_hasPositionsWrite ? '1' : '0',
            m_hasRotationsRead  ? '1' : '0',
            m_hasRotationsWrite ? '1' : '0',
            m_hasPositionRead   ? '1' : '0',
            m_hasPositionWrite  ? '1' : '0',
            m_hasRotationRead   ? '1' : '0',
            m_hasRotationWrite  ? '1' : '0',
        });

        public NativeArray<float3> Positions
        {
            get
            {
                NativeArray<float3> result;

                //StringBuilder sb = new($"Positions.Get for {e.D()} '{QCommon.GetPrefabName(EntityManager, e)}': ");

                if (gaNode.HasBuffer(m_Entity))
                {
                    if (gaNode.TryGetBuffer(m_Entity, out DynamicBuffer<Game.Areas.Node> buffer))
                    {
                        result = new NativeArray<float3>(buffer.Length, Allocator.TempJob);
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            result[i] = buffer[i].m_Position;
                        }
                        //sb.Append($"gaNode(yes)");
                    }
                    else
                    {
                        result = new();
                        //sb.Append($"gaNode(no)");
                    }
                }
                else
                {
                    result = new();
                    //sb.Append($"notFound");
                }
                //QLog.Debug(sb.ToString());

                return result;
            }

            set
            {
                //StringBuilder sb = new($"Positions.Set for {m_Entity.D()} '{QCommon.GetPrefabName(EntityManager, m_Entity)}': ");
                if (gaNode.HasBuffer(m_Entity))
                {
                    if (gaNode.TryGetBuffer(m_Entity, out DynamicBuffer<Game.Areas.Node> buffer))
                    {
                        if (buffer.Length != value.Length)
                        {
                            throw new Exception($"Invalid Positions.set data (length:{value.Length}, should be {buffer.Length} for {m_Entity.D()}.");
                        }

                        for (int i = 0; i < buffer.Length; i++)
                        {
                            Game.Areas.Node node = buffer[i];
                            node.m_Position = value[i];
                            buffer[i] = node;
                        }
                        //sb.Append($"gaNode");
                    }
                    else
                    {
                        throw new Exception($"Failed to save Game.Areas.Nodes Positions.set data for {m_Entity.D()}");
                    }
                }
                //QLog.Debug(sb.ToString());
            }
        }

        public float3 Position
        {
            get
            {
                //StringBuilder sb = new($"Position.Get for {e.D()} '{QCommon.GetPrefabName(EntityManager, e)}': ");
                float3 result;

                if (goTransform.HasComponent(m_Entity))
                {
                    //sb.Append($"goTransform");
                    result = goTransform.GetRefRO(m_Entity).ValueRO.m_Position;
                }
                else if (gaGeometry.HasComponent(m_Entity))
                {
                    //sb.Append($"gaGeometry");
                    result = gaGeometry.GetRefRO(m_Entity).ValueRO.m_CenterPosition;
                }
                else if (gpObjectGeometryData.HasComponent(m_Entity))
                {
                    //sb.Append($"gpObjectGeometryData");
                    result = gpObjectGeometryData.GetRefRO(m_Entity).ValueRO.m_Pivot;
                }
                else if (gnNode.HasComponent(m_Entity))
                {
                    //sb.Append($"gnNode");
                    result = gnNode.GetRefRO(m_Entity).ValueRO.m_Position;
                }
                else if (gnNodeGeometry.HasComponent(m_Entity))
                {
                    //sb.Append($"gnNodeGeometry");
                    result = gnNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Position;
                }
                else
                {
                    //sb.Append($"notFound");
                    //QLog.Debug($"Failed to find position for entity {e.D()} '{QCommonLib.QCommon.GetPrefabName(EntityManager, e)}'");
                    result = float3.zero;
                }

                //QLog.Debug(sb.ToString());
                return result;
            }

            set
            {
                if (!EntityManager.Exists(m_Entity)) return;

                //StringBuilder sb = new($"Position.Set for {m_Entity.D()} '{QCommon.GetPrefabName(EntityManager, m_Entity)}': ");

                float3 delta = value - Position;

                if (gaGeometry.HasComponent(m_Entity))
                {
                    //sb.Append($"gaGeometry, ");
                    gaGeometry.GetRefRW(m_Entity).ValueRW.m_CenterPosition = value;
                    gaGeometry.GetRefRW(m_Entity).ValueRW.m_Bounds = _MoveBounds3(gaGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
                }

                if (gnCurve.HasComponent(m_Entity))
                {
                    //sb.Append($"gnCurve, ");
                    gnCurve.GetRefRW(m_Entity).ValueRW.m_Bezier = _MoveBezier4x3(gnCurve.GetRefRO(m_Entity).ValueRO.m_Bezier, delta);
                }

                if (gnNode.HasComponent(m_Entity))
                {
                    //sb.Append($"gnNode, ");
                    gnNode.GetRefRW(m_Entity).ValueRW.m_Position = value;
                }

                if (gnNodeGeometry.HasComponent(m_Entity))
                {
                    //sb.Append($"gnNodeGeometry, ");
                    gnNodeGeometry.GetRefRW(m_Entity).ValueRW.m_Bounds = _MoveBounds3(gnNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
                }

                if (gnEdgeGeometry.HasComponent(m_Entity))
                {
                    //sb.Append($"gnEdgeGeometry, ");
                    gnEdgeGeometry.GetRefRW(m_Entity).ValueRW.m_Start = _MoveSegment(gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_Start, delta);
                    gnEdgeGeometry.GetRefRW(m_Entity).ValueRW.m_End = _MoveSegment(gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_End, delta);
                    gnEdgeGeometry.GetRefRW(m_Entity).ValueRW.m_Bounds = _MoveBounds3(gnEdgeGeometry.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
                }

                if (gnEndNodeGeometry.HasComponent(m_Entity))
                {
                    //sb.Append($"gnEndNodeGeometry, ");
                    gnEndNodeGeometry.GetRefRW(m_Entity).ValueRW.m_Geometry.m_Left = _MoveSegment(gnEndNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry.m_Left, delta);
                    gnEndNodeGeometry.GetRefRW(m_Entity).ValueRW.m_Geometry.m_Right = _MoveSegment(gnEndNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry.m_Right, delta);
                }

                if (gnStartNodeGeometry.HasComponent(m_Entity))
                {
                    //sb.Append($"gnStartNodeGeometry, ");
                    gnStartNodeGeometry.GetRefRW(m_Entity).ValueRW.m_Geometry.m_Left = _MoveSegment(gnStartNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry.m_Left, delta);
                    gnStartNodeGeometry.GetRefRW(m_Entity).ValueRW.m_Geometry.m_Right = _MoveSegment(gnStartNodeGeometry.GetRefRO(m_Entity).ValueRO.m_Geometry.m_Right, delta);
                }

                if (goTransform.HasComponent(m_Entity))
                {
                    //sb.Append($"goTransform, ");
                    goTransform.GetRefRW(m_Entity).ValueRW.m_Position = value;
                    if (grCullingInfo.HasComponent(m_Entity))
                    {
                        //sb.Append($"grCullingInfo, ");
                        grCullingInfo.GetRefRW(m_Entity).ValueRW.m_Bounds = _MoveBounds3(grCullingInfo.GetRefRO(m_Entity).ValueRO.m_Bounds, delta);
                    }
                }

                if (gpObjectGeometryData.HasComponent(m_Entity))
                {
                    //sb.Append($"gpObjectGeometryData, ");
                    gpObjectGeometryData.GetRefRW(m_Entity).ValueRW.m_Pivot = value;
                }
                //QLog.Debug(sb.ToString());
            }
        }

        public NativeArray<float> AnglesD
        {
            get
            {
                // Presently no readable or writable rotating buffers
                return new NativeArray<float>();
            }
        }

        public float AngleD
        {
            get
            {
                //QLog.Debug($"AngleD.Get for {m_Entity.D()} '{QCommon.GetPrefabName(EntityManager, m_Entity)}': {Rotation.ToEulerDegrees().y}");
                return Rotation.ToEulerDegrees().y;
            }
            set
            {
                float3 a = Rotation.ToEulerDegrees();
                a.y = value;
                Rotation = quaternion.EulerXYZ(a * Mathf.Deg2Rad);
                //QLog.Debug($"AngleD.Set for {m_Entity.D()} '{QCommon.GetPrefabName(EntityManager, m_Entity)}': {a.y}, {Rotation.ToEulerDegrees().y}");
            }
        }

        public NativeArray<quaternion> Rotations
        {
            get
            {
                // Presently no readable or writable rotating buffers
                return new NativeArray<quaternion>();
            }
        }

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

            set
            {
                //StringBuilder sb = new($"Rotation.Set for {m_Entity.D()} '{QCommon.GetPrefabName(EntityManager, m_Entity)}': ");
                if (goTransform.HasComponent(m_Entity))
                {
                    //sb.Append($"goTransform");
                    goTransform.GetRefRW(m_Entity).ValueRW.m_Rotation = value;
                }
                if (gnNode.HasComponent(m_Entity))
                {
                    //sb.Append($"gnNode");
                    gnNode.GetRefRW(m_Entity).ValueRW.m_Rotation = value;
                }
                //QLog.Debug(sb.ToString());
            }
        }

        private readonly Bounds3 _MoveBounds3(Bounds3 input, float3 delta)
        {
            input.min += delta;
            input.max += delta;
            return input;
        }

        private readonly Game.Net.Segment _MoveSegment(Game.Net.Segment input, float3 delta)
        {
            input.m_Left = _MoveBezier4x3(input.m_Left, delta);
            input.m_Right = _MoveBezier4x3(input.m_Right, delta);
            return input;
        }

        private readonly Bezier4x3 _MoveBezier4x3(Bezier4x3 input, float3 delta)
        {
            input.a += delta;
            input.b += delta;
            input.c += delta;
            input.d += delta;
            return input;
        }
    }
}
