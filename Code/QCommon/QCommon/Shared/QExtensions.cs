using Colossal.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace QCommonLib
{
    internal static class QExtensions
    {
        public static float3 Center(this Bounds3 bounds)
        {
            float x = bounds.x.min + (bounds.x.max - bounds.x.min) / 2;
            float y = bounds.y.min + (bounds.y.max - bounds.y.min) / 2;
            float z = bounds.z.min + (bounds.z.max - bounds.z.min) / 2;
            return new float3(x, y, z);
        }

        public static float2 Center(this Bounds2 bounds)
        {
            float x = bounds.x.min + (bounds.x.max - bounds.x.min) / 2;
            float y = bounds.y.min + (bounds.y.max - bounds.y.min) / 2;
            return new float2(x, y);
        }

        public static float Center(this Bounds1 bounds)
        {
            return bounds.min + (bounds.max - bounds.min) / 2;;
        }

        public static float2 Center2D(this Bounds3 bounds)
        {
            float x = bounds.x.min + (bounds.x.max - bounds.x.min) / 2;
            float z = bounds.z.min + (bounds.z.max - bounds.z.min) / 2;
            return new float2(x, z);
        }

        public static string D(this Entity e)
        {
            return $"E{e.Index}.{e.Version}";
        }

        public static string D(this Game.Objects.Transform t)
        {
            return $"{t.m_Position.DX()}/{t.m_Rotation.Y():0.##}";
        }

        public static string D(this int2 i)
        {
            return $"{i.x},{i.y}";
        }

        public static string D(this float2 f)
        {
            return $"{f.x:0.##},{f.y:0.##}";
        }

        public static string D(this float3 f)
        {
            return $"{f.x:0.0},{f.z:0.0}";
        }

        public static string DX(this float3 f)
        {
            return $"{f.x,7:0.00},{f.y,7:0.00},{f.z,7:0.00}";
        }

        public static string D(this Quad2 q)
        {
            return $"({q.a.x:0.##},{q.a.y:0.##}),({q.b.x:0.##},{q.b.y:0.##}),({q.c.x:0.##},{q.c.y:0.##}),({q.d.x:0.##},{q.d.y:0.##})";
        }

        public static float2 XZ(this float3 f)
        {
            return new float2(f.x, f.z);
        }

        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }

        public static float DistanceXZ(this float3 a, float3 b)
        {
            return math.abs(math.distance(new float2(a.x, a.z), new float2(b.x, b.z)));
        }

        public static Bounds3 Encapsulate(this Bounds3 a, Bounds3 b)
        {
            a.min.x = Math.Min(a.min.x, b.min.x);
            a.min.y = Math.Min(a.min.y, b.min.y);
            a.min.z = Math.Min(a.min.z, b.min.z);
            a.max.x = Math.Max(a.max.x, b.max.x);
            a.max.y = Math.Max(a.max.y, b.max.y);
            a.max.z = Math.Max(a.max.z, b.max.z);
            return a;
        }

        public static Bounds2 Encapsulate(this Bounds2 a, Bounds2 b)
        {
            a.min.x = Math.Min(a.min.x, b.min.x);
            a.min.y = Math.Min(a.min.y, b.min.y);
            a.max.x = Math.Max(a.max.x, b.max.x);
            a.max.y = Math.Max(a.max.y, b.max.y);
            return a;
        }

        public static Bounds3 Expand(this Bounds3 b, float3 size)
        {
            return new Bounds3(
                b.min - size,
                b.max + size
            );
        }

        public static float4 Expand(this float4 area, float distance)
        {
            return new float4(area.x - distance, area.y - distance, area.z + distance, area.w + distance);
        }

        public static quaternion Inverse(this quaternion q)
        {
            float num = q.value.x * q.value.x + q.value.y * q.value.y + q.value.z * q.value.z + q.value.w * q.value.w;
            float num2 = 1f / num;
            quaternion result = default;
            result.value.x = (0f - q.value.x) * num2;
            result.value.y = (0f - q.value.y) * num2;
            result.value.z = (0f - q.value.z) * num2;
            result.value.w = q.value.w * num2;
            return result;
        }

        public static Quad3 ToQuad3(this Bounds3 b)
        {
            return new(
                new(b.min.x, b.min.y, b.min.z),
                new(b.max.x, b.min.y, b.min.z),
                new(b.max.x, b.max.y, b.max.z),
                new(b.min.x, b.max.y, b.max.z));
        }

        public static float3 Lerp(this float3 a, float3 b, float t)
        {
            return new float3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        public static float3 LerpAbs(this float3 a, float3 b, float t)
        {
            float length = math.distance(a, b);
            t = (length - t) / length;
            return Lerp(a, b, t);
        }

        public static float3 Max(this Quad3 q)
        {
            return (q.a.Max(q.b)).Max(q.c.Max(q.d));
        }

        public static float3 Max(this float3 a, float3 b)
        {
            return new(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
        }

        public static float3 Min(this Quad3 q)
        {
            return (q.a.Min(q.b)).Min(q.c.Min(q.d));
        }

        public static float3 Min(this float3 a, float3 b)
        {
            return new(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));
        }

        public static quaternion Multiply(this quaternion a, quaternion b)
        {
            return math.normalize(math.mul(a, b));
        }

        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
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

        public static string ToStringNoTrace(this Exception e)
        {
            StringBuilder sb = new(e.GetType().ToString());
            sb.Append(": ").Append(e.Message);
            return sb.ToString();
        }


        public static float X(this Game.Objects.Transform transform)
        {
            return transform.m_Rotation.ToEulerDegrees().x;
        }

        public static float Y(this Game.Objects.Transform transform)
        {
            return transform.m_Rotation.ToEulerDegrees().y;
        }

        public static float Z(this Game.Objects.Transform transform)
        {
            return transform.m_Rotation.ToEulerDegrees().z;
        }


        public static float X(this quaternion quat)
        {
            return quat.ToEulerDegrees().x;
        }

        public static float Y(this quaternion quat)
        {
            return quat.ToEulerDegrees().y;
        }

        public static float Z(this quaternion quat)
        {
            return quat.ToEulerDegrees().z;
        }
    }
}
