using System;
using UnityEngine;

namespace Megalith
{
    public class MegalithSplineUtils
    {
        //Display a spline between 2 points derived with the Catmull-Rom spline algorithm
        public static void DisplayCatmullRomSpline(Vector3[] path, Color color)
        {
            for (int i = 0; i < path.Length; i++)
            {
                //The 4 points we need to form a spline between p1 and p2
                Vector3 p0 = path[ClampListPos(i - 1, path)];
                Vector3 p1 = path[i];
                Vector3 p2 = path[ClampListPos(i + 1, path)];
                Vector3 p3 = path[ClampListPos(i + 2, path)];

                //The start position of the line
                Vector3 lastPos = p1;

                //The spline's resolution
                //Make sure it's is adding up to 1, so 0.3 will give a gap, but 0.2 will work
                float resolution = 0.1f;

                //How many times should we loop?
                int loops = Mathf.FloorToInt(1f / resolution);

                for (int j = 1; j <= loops; j++)
                {
                    //Which t position are we at?
                    float t = j * resolution;

                    //Find the coordinate between the end points with a Catmull-Rom spline
                    Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

                    //Draw this line segment
#if UNITY_EDITOR
                    UnityEditor.Handles.color = color;
                    UnityEditor.Handles.DrawLine(lastPos, newPos);
#else
                    Gizmos.color = color;
                    Gizmos.DrawLine(lastPos, newPos);
#endif

                    //Save this pos so we can draw the next line segment
                    lastPos = newPos;
                }
            }            
        }

        public static Quaternion GetForwardRotationAtPoint(int point, Vector3 up, Vector3[] path)
        {
            if(path.Length < 2)
            {
                throw new InvalidOperationException("Not enough nodes for path.");
            }
            if(path.Length == 2)
            {
                return Quaternion.LookRotation(path[1] - path[0], up);
            }
            if(path.Length == 3)
            {
                if(point == 0)
                    return Quaternion.LookRotation(path[1] - path[0], up);
                else
                    return Quaternion.LookRotation(path[2] - path[1], up);
            }
            if(point == path.Length - 1)
            {
                Vector3[] newPath = new Vector3[path.Length + 1];
                Array.Copy(path, newPath, path.Length);
                newPath[point + 1] = newPath[point];

                Vector3 p0 = newPath[point - 2];
                Vector3 p1 = newPath[point - 1];
                Vector3 p2 = newPath[point];
                Vector3 p3 = newPath[point + 1];

                Vector3 previousPos = GetCatmullRomPosition(0.9f, p0, p1, p2, p3);
                return Quaternion.LookRotation(newPath[point] - previousPos, up);

            }

            if(point == 0)
            {
                Vector3[] newPath = new Vector3[path.Length + 1];
                Array.Copy(path, 0, newPath, 1, path.Length);
                newPath[0] = path[point];

                Vector3 p0 = newPath[0];
                Vector3 p1 = newPath[1];
                Vector3 p2 = newPath[2];
                Vector3 p3 = newPath[3];

                Vector3 nextPos = GetCatmullRomPosition(0.1f, p0, p1, p2, p3);
                return Quaternion.LookRotation(nextPos - path[point], up);                
            }

            {
                Vector3 p0 = path[ClampListPos(point - 1, path)];
                Vector3 p1 = path[point];
                Vector3 p2 = path[ClampListPos(point + 1, path)];
                Vector3 p3 = path[ClampListPos(point + 2, path)];

                Vector3 nextPos = GetCatmullRomPosition(0.1f, p0, p1, p2, p3);
                return Quaternion.LookRotation(nextPos - path[point], up);
            }

        }

        //Clamp the list positions to allow looping
        private static int ClampListPos(int pos, Vector3[] path)
        {
            if (pos < 0)
            {
                pos = 0;
            }
            
            else if (pos > path.Length - 1)
            {
                pos = path.Length - 1;
            }

            return pos;
        }

        private static Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
            Vector3 a = 2f * p1;
            Vector3 b = p2 - p0;
            Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

            //The cubic polynomial: a + b * t + c * t^2 + d * t^3
            Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

            return pos;
        }
    }

}
