using Colossal.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;

namespace QCommonLib
{
    internal static class QExtensions
    {
        public static string ToStringNoTrace(this Exception e)
        {
            StringBuilder stringBuilder = new(e.GetType().ToString());
            stringBuilder.Append(": ").Append(e.Message);
            return stringBuilder.ToString();
        }

        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

        public static string D(this Entity e)
        {
            return $"E{e.Index}.{e.Version}";
        }

        public static void Encapsulate(ref this Bounds3 a, Bounds3 b)
        {
            a.min.x = Math.Min(a.min.x, b.min.x);
            a.min.y = Math.Min(a.min.y, b.min.y);
            a.min.z = Math.Min(a.min.z, b.min.z);
            a.max.x = Math.Max(a.max.x, b.max.x);
            a.max.y = Math.Max(a.max.y, b.max.y);
            a.max.z = Math.Max(a.max.z, b.max.z);
        }

        public static float3 Center(this Bounds3 bounds)
        {
            float x = bounds.x.min + (bounds.x.max - bounds.x.min) / 2;
            float y = bounds.y.min + (bounds.y.max - bounds.y.min) / 2;
            float z = bounds.z.min + (bounds.z.max - bounds.z.min) / 2;
            //QLoggerStatic.Debug($"{bounds.x.min},{bounds.y.min},{bounds.z.min} - {bounds.x.max},{bounds.y.max},{bounds.z.max}\nCenter:{x},{y},{z}");
            return new float3(x, y, z);
        }

        public static void SetInvalid(this float3 f)
        {
            f.x = -9999.69f;
            f.y = -9999.69f;
            f.z = -9999.69f;
        }

        public static bool IsValid(this float3 f)
        {
            if (f.x == -9999.69f && f.y == -9999.69f && f.z == -9999.69f) return false;
            return true;
        }

        public static string D(this float3 f)
        {
            return $"{f.x},{f.z}";
            //return $"{f.x},{f.y},{f.z}";
        }

        public static string DX(this float3 f)
        {
            return $"{f.x},{f.y},{f.z}";
        }

        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }


        public static float3 ToEulerDegrees(this quaternion quat)
        {
            float4 q1 = quat.value;

            float sqw = q1.w * q1.w;
            float sqx = q1.x * q1.x;
            float sqy = q1.y * q1.y;
            float sqz = q1.z * q1.z;
            float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float test = q1.x * q1.w - q1.y * q1.z;
            float3 v;

            if (test > 0.4995f * unit)
            { // north pole
                v.y = 2f * math.atan2(q1.y, q1.x);
                v.x = math.PI / 2;
                v.z = 0;
                return ClampDegreesAll(math.degrees(v));
            }
            if (test < -0.4995f * unit)
            { // south pole
                v.y = -2f * math.atan2(q1.y, q1.x);
                v.x = -math.PI / 2;
                v.z = 0;
                return ClampDegreesAll(math.degrees(v));
            }

            quaternion q3 = new(q1.w, q1.z, q1.x, q1.y);
            float4 q = q3.value;

            v.y = math.atan2(2f * q.x * q.w + 2f * q.y * q.z, 1 - 2f * (q.z * q.z + q.w * q.w));
            v.x = math.asin(2f * (q.x * q.z - q.w * q.y));
            v.z = math.atan2(2f * q.x * q.y + 2f * q.z * q.w, 1 - 2f * (q.y * q.y + q.z * q.z));

            return ClampDegreesAll(math.degrees(v));
        }

        static float3 ClampDegreesAll(float3 angles)
        {
            angles.x = ClampDegrees(angles.x);
            angles.y = ClampDegrees(angles.y);
            angles.z = ClampDegrees(angles.z);
            return angles;
        }

        static float ClampDegrees(float angle)
        {
            angle %= 360;
            if (angle < 0) angle += 360;
            return angle;
        }

        // By/adapted from Krzychu124

        //public static void AddComponentDataByType(this Type type, EntityManager entityManager, Entity e, IComponentData data)
        //{
        //    MethodInfo generic = MIT.EM_Methods["AddComponentData"].MakeGenericMethod(type);
        //    generic.Invoke(entityManager, new object[] { e, data });
        //}

        //public static void RemoveComponentByType(this Type type, EntityManager entityManager, Entity e)
        //{
        //    MethodInfo removeComponent = typeof(EntityManager).GetMethod(nameof(EntityManager.RemoveComponent), new Type[] { typeof(Entity) });
        //    MethodInfo generic = removeComponent.MakeGenericMethod(type);
        //    generic.Invoke(entityManager, new object[] { e });
        //}

        //public static bool HasComponentByType(this Type type, EntityManager entityManager, Entity e)
        //{
        //    MethodInfo hasComponent = typeof(EntityManager).GetMethod(nameof(EntityManager.HasComponent), new Type[] { typeof(Entity) });
        //    MethodInfo generic = hasComponent.MakeGenericMethod(type);
        //    return (bool)generic.Invoke(entityManager, new object[] { e });
        //}

        //public static object GetComponentDataByType(this Type type, EntityManager entityManager, Entity e)
        //{
        //    MethodInfo getComponentData = typeof(EntityManager).GetMethod(nameof(EntityManager.GetComponentData), new Type[] { typeof(Entity) });
        //    MethodInfo generic = getComponentData.MakeGenericMethod(type);
        //    return generic.Invoke(entityManager, new object[] { e });
        //}

        //public static void SetComponentDataByType(this Type type, EntityManager entityManager, Entity e, IComponentData comp)
        //{
        //    MethodInfo generic = MIT.EM_Methods["SetComponentData"].MakeGenericMethod(type);
        //    generic.Invoke(entityManager, new object[] { e, comp });
        //}


        //public static bool HasBufferByType(this Type type, EntityManager entityManager, Entity e)
        //{
        //    MethodInfo hasBuffer = typeof(EntityManager).GetMethod(nameof(EntityManager.HasBuffer), new Type[] { typeof(Entity) });
        //    MethodInfo generic = hasBuffer.MakeGenericMethod(type);
        //    return (bool)generic.Invoke(entityManager, new object[] { e });
        //}

        ///// <summary>
        ///// Get the field in an IBufferElementData reference buffer component that holds the actual entity reference
        ///// For example for Game.Areas.SubArea.m_area, it returns m_area
        ///// </summary>
        ///// <param name="type">Extension for the IBufferElementData struct type to search</param>
        ///// <returns>FieldInfo of this field</returns>
        ///// <exception cref="Exception">If no such field is found</exception>
        //public static FieldInfo GetEntityReferenceFieldInfo(this Type type)
        //{
        //    FieldInfo field = null;
        //    foreach (FieldInfo f in type.GetFields())
        //    {
        //        if (f.FieldType == typeof(Entity))
        //        {
        //            field = f;
        //            break;
        //        }
        //    }
        //    if (field == null) throw new Exception($"Entity field not found for type {type}");

        //    return field;
        //}


        //public static string TryGetPrefabName(this Entity e, EntityManager manager, PrefabSystem prefabSystem)
        //{
        //    if (e == Entity.Null || !manager.Exists(e))
        //    {
        //        return null;
        //    }

        //    if (manager.HasComponent<PrefabRef>(e))
        //    {
        //        PrefabRef prefabRef = manager.GetComponentData<PrefabRef>(e);
        //        if (prefabSystem.TryGetPrefab(prefabRef, out PrefabBase prefab))
        //        {
        //            return prefab.name;
        //        }
        //    }
        //    else if (manager.HasComponent<PrefabData>(e))
        //    {
        //        PrefabData prefabData = manager.GetComponentData<PrefabData>(e);
        //        if (prefabSystem.TryGetPrefab(prefabData, out PrefabBase prefab))
        //        {
        //            return prefab.name;
        //        }
        //    }
        //    return null;
        //}

        public static bool IsNumericType(this object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNumericType(this Type type)
        {
            if (type.Namespace == "Unity.Mathematics")
            {
                return true;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsEnumerable(object myProperty)
        {
            return typeof(IEnumerable).IsInstanceOfType(myProperty)
                || typeof(IEnumerable<>).IsInstanceOfType(myProperty);
        }

        public static bool IsCollection(Type type)
        {
            return typeof(ICollection).IsAssignableFrom(type)
                || typeof(ICollection<>).IsAssignableFrom(type);
        }

        public static bool IsList(Type type)
        {
            return typeof(IList).IsAssignableFrom(type)
                || typeof(IList<>).IsAssignableFrom(type);
        }
    }
}
