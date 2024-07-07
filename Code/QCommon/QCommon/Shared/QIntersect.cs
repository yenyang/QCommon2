using Colossal.Mathematics;
using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace QCommonLib
{
    internal class QIntersect
    {
        public static bool DoesLineIntersectCircle2(Line3.Segment line, Circle2 circle, float height, out float2 hit)
        {
            hit = GetLinePointAtHeight(line, height);
            return MathUtils.Intersect(circle, hit);
        }

        public static bool DoesLineIntersectQuad2(Line3.Segment line, Quad2 quad, float height, out float2 hit)
        {
            hit = GetLinePointAtHeight(line, height);
            return MathUtils.Intersect(quad, hit);
        }

        public static bool DoesLineIntersectQuad3(Line3.Segment line, Quad3 quad, out float hit)
        {
            return MathUtils.Intersect(quad, line, out hit);
        }

        public static bool DoesLineIntersectBounds2(Line3.Segment line, Bounds2 bounds, float height, out float2 hit)
        {
            hit = GetLinePointAtHeight(line, height);
            return MathUtils.Intersect(bounds, hit);
        }

        public static bool DoesLineIntersectBounds3(Line3.Segment line, Bounds3 bounds, out float2 hit)
        {
            return MathUtils.Intersect(bounds, line, out hit);
        }

        public static bool DoesLineIntersectCylinder(Line3.Segment line, Circle2 cylinderCircle, Bounds1 cylinderHeight)//, out List<float3> hit)
        {
            //hit = new();
            line = GetLineVerticalSlice(line, cylinderHeight);

            if (MathUtils.Intersect(cylinderCircle, line.xz.a) && MathUtils.Intersect(cylinderCircle, line.xz.b))
            { // Is the line fully contained in the cylinder (i.e. near-vertical line)
                //hit.Add(line.a);
                //hit.Add(line.b);
                return true;
            }

            int hits = IntersectionsBetweenLineAndCircle(cylinderCircle, line.xz, out float2 a, out float2 b);

            float heightA = 0f;
            float heightB = 0f;
            if (hits > 0)
            {
                heightA = GetLineHeightAtPoint(line, a.x);
            }
            if (hits > 1)
            {
                heightA = GetLineHeightAtPoint(line, a.x);
                heightB = GetLineHeightAtPoint(line, b.x);
                if (heightB > heightA)
                {
                    (heightB, heightA) = (heightA, heightB);
                    //(b, a) = (a, b);
                }
            }

            bool result = false;
            if (hits > 0)
            {
                //float3 hitPos = new(a.x, heightA, a.y);
                //if (heightA > line.a.y) hitPos = line.a;
                //hit.Add(hitPos);
                if (heightA > cylinderHeight.min && heightA < cylinderHeight.max) result = true;
            }
            if (hits > 1)
            {
                //float3 hitPos = new(b.x, heightB, b.y);
                //if (heightB < line.b.y) hitPos = line.b;
                //hit.Add(hitPos);
                if (heightB > cylinderHeight.min && heightB < cylinderHeight.max) result = true;
            }

            return result;
        }

        /// <summary>
        /// Get the part of a line that is between the two heights in bounds.
        /// </summary>
        /// <param name="line">The line to slice</param>
        /// <param name="bounds">The bottom (min) and top (max) of the desired slice</param>
        /// <returns></returns>
        public static Line3.Segment GetLineVerticalSlice(Line3.Segment line, Bounds1 bounds)
        {
            if (line.a.y == line.b.y) line.b.y -= 1; // Fudge the numbers if the line is truly horizontal
            if (line.a.y < line.b.y) (line.b, line.a) = (line.a, line.b); // Ensure B is the lower end
            if (bounds.max > line.a.y) bounds.max = line.a.y;
            if (bounds.min < line.b.y) bounds.min = line.b.y;

            float3 mag = line.b - line.a;
            float t1 = (bounds.max - line.a.y) / mag.y;
            float t2 = (bounds.min - line.a.y) / mag.y;
            float3 a = math.lerp(line.a, line.b, t1);
            float3 b = math.lerp(line.a, line.b, t2);
            return new(a, b);
        }

        /// <summary>
        /// Get the height of a line at a given X coordinate
        /// </summary>
        /// <param name="line">The line to test</param>
        /// <param name="x">The X coordinate on the world scale</param>
        /// <returns></returns>
        public static float GetLineHeightAtPoint(Line3.Segment line, float x)
        {
            float3 mag = line.b - line.a;
            float distX = x - line.a.x;
            float distY = mag.y * (distX / mag.x);
            return line.a.y + distY;
        }

        /// <summary>
        /// Get the point at which a line is at a given Y coordinate
        /// </summary>
        /// <param name="line">The line to test</param>
        /// <param name="y">The Y coordinate on the world scale</param>
        /// <returns>The X,Z coordinates at height Y</returns>
        public static float2 GetLinePointAtHeight(Line3.Segment line, float y)
        {
            // needs to handle if line is never at height
            float3 mag = line.a - line.b;
            float distY = line.a.y - y;
            float t = distY / mag.y;
            float3 point = math.lerp(line.a, line.b, t);
            float2 result = new(point.x, point.z);
            return result;
        }

        public static bool GetLinesIntersection(Line2 line1, Line2 line2, out float2 point)
        {
            point = default;
            float a1 = line1.b.y - line1.a.y;
            float b1 = line1.a.x - line1.b.x;
            float c1 = a1 * line1.a.x + b1 * line1.a.y;

            float a2 = line2.b.y - line2.a.y;
            float b2 = line2.a.x - line2.b.x;
            float c2 = a2 * line2.a.x + b2 * line2.a.y;

            float delta = a1 * b2 - a2 * b1;
            if (delta > -1 && delta < 1)
            {
                return false;
            }
            point.x = (b2 * c1 - b1 * c2) / delta;
            point.y = (a1 * c2 - a2 * c1) / delta;
            return true;
        }

        public static Line3.Segment GetLinePart(Line3.Segment line, float cutStart, float cutEnd)
        {
            float3 mag = line.b - line.a;
            float start = cutStart / math.length(mag);
            float end = cutEnd / math.length(mag);
            return new(line.a + (mag * start), line.a + (mag * end));
        }

        public static float IntersectionsBetweenLineAndCircleCut(Circle2 circle, Line3.Segment line3, bool isCircleAtStart, float fallback = 0f)
        {
            return IntersectionsBetweenLineAndCircleCut(circle, new Line2(line3.a.XZ(), line3.b.XZ()), isCircleAtStart, fallback);
        }

        public static float IntersectionsBetweenLineAndCircleCut(Circle2 circle, Line2 line, bool isCircleAtStart, float fallback = 0f)
        {
            float result = 0;
            if (IntersectionsBetweenLineAndCircle(circle, line, out float2 a, out float2 b) > 0)
            {
                if (isCircleAtStart)
                {
                    result = math.distance(a, line.a);

                    //float invert = math.distance(a, line.b);
                    //float maxDist = math.distance(line.a, line.b);
                    //bool isHit = invert < maxDist;
                    //QLog.Debug($"S C:{circle.position.D()}/{circle.radius:0.00}  L:{line.a.D(),-15}:" +
                    //    $" {line.b.D(),-15}:" +
                    //    $"X:{a.D()} ({IntersectionsBetweenLineAndCircle(circle, line, out float2 _, out float2 _)}),  " +
                    //    $"dist:{result:0.##} max:{maxDist:0.##} inv:{invert:0.##}  {isHit}/{fallback}");

                    // If the distance from opposite end of line is more than line length, use fallback value
                    if (math.distance(a, line.b) > math.distance(line.a, line.b)) result = fallback;
                }
                else
                {
                    result = math.distance(b, line.b);

                    //float invert = math.distance(b, line.a);
                    //float maxDist = math.distance(line.b, line.a);
                    //bool isHit = invert < maxDist;
                    //QLog.Debug($"E C:{circle.position.D()}/{circle.radius:0.00}  L:{line.a.D(),-15}:" +
                    //    $" {line.b.D(),-15}:" +
                    //    $"X:{b.D()} ({IntersectionsBetweenLineAndCircle(circle, line, out float2 _, out float2 _)}),  " +
                    //    $"dist:{result:0.##} max:{maxDist:0.##} inv:{invert:0.##}  {isHit}/{fallback}");

                    // If the distance from opposite end of line is more than line length, use fallback value
                    if (math.distance(b, line.a) > math.distance(line.b, line.a)) result = fallback;
                }
            }
            return result;
        }

        // Thanks to vladibo on the Unity forum
        public static int IntersectionsBetweenLineAndCircle(Circle2 circle, Line2 line, out float2 intersect1, out float2 intersect2)
        {
            float t;
            float2 magnitude = line.b - line.a;

            var a = magnitude.x * magnitude.x + magnitude.y * magnitude.y;
            var b = 2 * (magnitude.x * (line.a.x - circle.position.x) + magnitude.y * (line.a.y - circle.position.y));
            var c = (line.a.x - circle.position.x) * (line.a.x - circle.position.x) + (line.a.y - circle.position.y) * (line.a.y - circle.position.y) - circle.radius * circle.radius;

            var determinate = b * b - 4 * a * c;
            if ((a <= 0.0000001) || (determinate < -0.0000001))
            {
                // No real solutions.
                intersect1 = float2.zero;
                intersect2 = float2.zero;
                return 0;
            }
            if (determinate < 0.0000001 && determinate > -0.0000001)
            {
                // One solution.
                t = -b / (2 * a);
                intersect1 = new float2(line.a.x + t * magnitude.x, line.a.y + t * magnitude.y);
                intersect2 = float2.zero;
                return 1;
            }

            // Two solutions.
            t = (float)((-b + Math.Sqrt(determinate)) / (2 * a));
            intersect1 = new float2(line.a.x + t * magnitude.x, line.a.y + t * magnitude.y);
            t = (float)((-b - Math.Sqrt(determinate)) / (2 * a));
            intersect2 = new float2(line.a.x + t * magnitude.x, line.a.y + t * magnitude.y);

            return 2;
        }
    }
}
