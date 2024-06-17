using Colossal.Mathematics;
using System.Collections.Generic;
using Unity.Mathematics;

namespace QCommonLib
{
    /// <summary>
    /// Adapted from GeeksForGeeks
    /// https://www.geeksforgeeks.org/minimum-enclosing-circle-using-welzls-algorithm/
    /// </summary>
    public class QMinimumEnclosingCircle
    {
        /// <summary>
        /// Get the minimum enclosing circle (MEC) for passed float2 list
        /// </summary>
        /// <param name="P">List of float2 to enclose</param>
        /// <returns>The minimum enclosing circle (MEC)</returns>
        public static Circle2 Welzl(List<float2> P)
        {
            //_Rcount = 0;
            Circle2 result = WelzlHelper(P, new float2x3(), P.Count, 0);
            //QLog.Debug($"R set created {_Rcount} times");
            return result;
        }

        /// <summary>
        /// Returns the MEC using Welzl's algorithm
        /// </summary>
        /// <param name="P">Input points</param>
        /// <param name="R">Set for current MEC candidate</param>
        /// <param name="n">The number of points in P that are not yet processed</param>
        /// <returns>New MEC candidate</returns>
        private static Circle2 WelzlHelper(List<float2> P, float2x3 R, int n, int rIdx)
        {
            // Base case when all points processed or |R| = 3
            if (n == 0 || rIdx == 3)
            {
                return MinCircleTrivial(R, rIdx);
            }

            float2 p = P[n - 1];

            // Get the MEC circle from the set of points P - {p}}
            Circle2 mec = WelzlHelper(P, R, n - 1, rIdx);

            // If d contains p, return mec
            if (IsInside(mec, p))
            {
                return mec;
            }

            // Otherwise, must be on the boundary of the MEC
            R[rIdx++] = p;

            // Return the MEC for P - {p} and R U {p}
            Circle2 next = WelzlHelper(P, R, n - 1, rIdx);
            return next;
        }

        // Return the minimum enclosing circle for N <= 3
        private static Circle2 MinCircleTrivial(float2x3 R, int rIdx)
        {
            if (rIdx == 0) return new Circle2(0, new float2(0, 0));
            else if (rIdx == 1) return new Circle2(0, R[0]);
            else if (rIdx == 2) return CircleFrom(R[0], R[1]);

            // To check if MEC can be determined by 2 points only
            for (int i = 0; i < 3; i++)
            {
                for (int j = i + 1; j < 3; j++)
                {
                    Circle2 circle = CircleFrom(R[i], R[j]);
                    if (IsValidCircle(circle, R, rIdx))
                    {
                        return circle;
                    }
                }
            }
            return CircleFrom(R[0], R[1], R[2]);
        }

        // Get Circle from 2 points
        private static Circle2 CircleFrom(float2 A, float2 B)
        {
            float2 C = (A + B) / 2f; //new((A.x + B.x) / 2.0f, (A.y + B.y) / 2.0f);
            return new Circle2(math.distance(A, C), C);
        }

        // Get Circle from 3 points
        private static Circle2 CircleFrom(float2 A, float2 B, float2 C)
        {
            float2 center = GetCircleCenter(B.x - A.x, B.y - A.y, C.x - A.x, C.y - A.y);
            center.x += A.x;
            center.y += A.y;
            return new Circle2(math.distance(center, A), center);
        }

        // Helper method to get a circle defined by 3 points
        private static float2 GetCircleCenter(float bx, float by, float cx, float cy)
        {
            float B = bx * bx + by * by;
            float C = cx * cx + cy * cy;
            float D = bx * cy - by * cx;
            return new float2((cy * B - by * C) / (2f * D), (bx * C - cx * B) / (2f * D));
        }

        // Check whether a circle encloses the given points
        private static bool IsValidCircle(Circle2 c, float2x3 P, int rIdx)
        {
            // Iterating through all the points to check whether the points lie inside the circle or not
            for (int i = 0; i < rIdx; i++)
            {
                if (!IsInside(c, P[i])) return false;
            }
            return true;
        }

        // Check whether a point lies inside or on the boundaries of the circle
        private static bool IsInside(Circle2 circle, float2 p)
        {
            return math.distance(circle.position, p) <= circle.radius;
        }
    }
}
