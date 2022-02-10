using UnityEngine;

namespace Megalith
{
	public struct Sphere
	{
		public Vector3 point;
		public float radius;

		public Sphere(Vector3 point, float radius)
		{
			this.point = point;
			this.radius = radius;
		}
	}

	public struct Box
	{
		public Vector3 rightBottomBack;
		public Vector3 rightBottomFront;
		public Vector3 rightUpBack;
		public Vector3 rightUpFront;
		public Vector3 leftBottomBack;
		public Vector3 leftBottomFront;
		public Vector3 leftUpBack;
		public Vector3 leftUpFront;

        public Vector3 center;
        public float maxExtents;

        public Box(Vector3 rightBottomBack, Vector3 rightBottomFront, Vector3 rightUpBack, Vector3 rightUpFront, Vector3 leftBottomBack, Vector3 leftBottomFront, Vector3 leftUpBack, Vector3 leftUpFront)
		{
			this.rightBottomBack = rightBottomBack;
			this.rightBottomFront = rightBottomFront;
			this.rightUpBack = rightUpBack;
			this.rightUpFront = rightUpFront;
			this.leftBottomBack = leftBottomBack;
			this.leftBottomFront = leftBottomFront;
			this.leftUpBack = leftUpBack;
			this.leftUpFront = leftUpFront;

            center = (rightBottomBack + rightBottomFront + rightUpBack + rightUpFront + leftBottomBack + leftBottomFront + leftUpBack + leftUpFront) / 8;            
            maxExtents = Vector3.Distance(center, rightBottomBack);
            float currentExtents = Vector3.Distance(center, rightBottomFront);
            if (currentExtents > maxExtents)
                maxExtents = currentExtents;
            currentExtents = Vector3.Distance(center, rightUpBack);
            if (currentExtents > maxExtents)
                maxExtents = currentExtents;
            currentExtents = Vector3.Distance(center, rightUpFront);
            if (currentExtents > maxExtents)
                maxExtents = currentExtents;
            currentExtents = Vector3.Distance(center, leftBottomBack);
            if (currentExtents > maxExtents)
                maxExtents = currentExtents;
            currentExtents = Vector3.Distance(center, leftBottomFront);
            if (currentExtents > maxExtents)
                maxExtents = currentExtents;
            currentExtents = Vector3.Distance(center, leftUpBack);
            if (currentExtents > maxExtents)
                maxExtents = currentExtents;
            currentExtents = Vector3.Distance(center, leftUpFront);
            if (currentExtents > maxExtents)
                maxExtents = currentExtents;
        }

       
        public Vector3[] Vertices
		{
			get
			{
				var vertices = new Vector3[8];
				vertices[0] = rightBottomBack;
				vertices[1] = rightBottomFront;
				vertices[2] = rightUpBack;
				vertices[3] = rightUpFront;
				vertices[4] = leftBottomBack;
				vertices[5] = leftBottomFront;
				vertices[6] = leftUpBack;
				vertices[7] = leftUpFront;
				return vertices;
			}
			set
			{
				if (value == null || value.Length != 8)
					return;
				rightBottomBack = value[0];
				rightBottomFront = value[1];
				rightUpBack = value[2];
				rightUpFront = value[3];
				leftBottomBack = value[4];
				leftBottomFront = value[5];
				leftUpBack = value[6];
				leftUpFront = value[7];
			}
		}
	}

	public class MegalithMath
	{
		/// <summary>
		/// Returns randomized Vector3 between values of v1 and v2
		/// </summary>
		public static Vector3 RandomVector3(Vector3 v1, Vector3 v2)
		{
			return new Vector3
				(
					Random.Range(v1.x, v2.x),
					Random.Range(v1.y, v2.y),
					Random.Range(v1.z, v2.z)
				);
		}

		/// <summary>
		/// Returns a rotation with the supplied normal vector, looking towards the aligned axis
		/// </summary>
		/// <param name="alignAxis"></param>
		/// <param name="normal"></param>
		/// <returns></returns>
		public static Quaternion GetAlignedRotationFromNormal(Vector3 alignAxis, Vector3 normal)
		{
			var parentRot = Quaternion.FromToRotation(Vector3.down, -normal);
			var downRot = Quaternion.LookRotation(alignAxis);
			var localRot = Quaternion.Inverse(parentRot) * downRot;
			localRot.eulerAngles = new Vector3(0f, localRot.eulerAngles.y, 0f);
			return parentRot * localRot;
		}

		/// <summary>
		/// Returns a signed value of the angle θ between two Vectors
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static float SignedAngle(Vector3 a, Vector3 b)
		{
			var angle = Vector3.Angle(a, b);
			return angle * Mathf.Sign(Vector3.Cross(a, b).y);
		}

		/// <summary>
		/// Finds closest point on Line
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		/// <returns></returns>
		public static Vector3 GetClosestPointOnLine(Vector3 a, Vector3 b, Vector3 c)
		{
			return a + Vector3.Project(c - a, b - a);
		}

		/// <summary>
		/// Returns a point snapped to the closest intersection 
		/// of invisible grid with segment size equal to grid size
		/// </summary>
		/// <param name="point"></param>
		/// <param name="gridSize"></param>
		/// <returns></returns>
		public static Vector3 SnapToGrid(Vector3 point, float gridSize)
		{
			return new Vector3
				(
					gridSize * Mathf.Round(point.x / gridSize),
					gridSize * Mathf.Round(point.y / gridSize),
					gridSize * Mathf.Round(point.z / gridSize)
				);
		}

		/// <summary>
		/// Returns -1 if a ray does not intersect the sphere
		/// or the distance along the ray where the first intersection happens
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="sphere"></param>
		/// <returns></returns>
		public static float RaySphereIntersection(Ray ray, Sphere sphere)
		{
			Vector3 q = sphere.point - ray.origin;
			float c = q.magnitude;
			float v = Vector3.Dot(q, ray.direction);
			float d = sphere.radius * sphere.radius - (c * c - v * v);

			// If there was no intersection, return -1
			if (d < 0)
				return (-1.0f);
			// Return the distance to the [first] intersecting point    
			return (v - Mathf.Sqrt(d));
		}


		/// <summary>
		/// Casts a sphere with specific radius along a ray and tests if it intersects another sphere with specific radius.
		/// Returns true when intersecting and outputs the position of the contact point (closest contact point to the ray's base)
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="castRadius"></param>
		/// <param name="maxDistance"></param>
		/// <param name="spherePosition"></param>
		/// <param name="sphereRadius"></param>
		/// <param name="hitPosition"></param>
		/// <returns></returns>
		public static bool SphereCastAgainstSphere(Ray ray, float castRadius, float maxDistance, Sphere targetSphere, out Vector3 hitPosition)
		{
			hitPosition = Vector3.zero;
			var p = GetClosestPointOnLine(ray.origin, ray.origin + ray.direction.normalized * maxDistance, targetSphere.point);
			Vector3 ab = targetSphere.point - p;
			if (ab.magnitude > (castRadius + targetSphere.radius))
			{
				hitPosition = Vector3.zero;
				return false;
			}
			hitPosition = p + ab * castRadius / ab.magnitude;
			return true;
		}

		/// <summary>
		/// Casts a sphere with specific radius along a ray and tests if it intersects with specified box.
		/// Returns true when intersecting and outputs the position of the contact point (closest contact point to the ray's base)
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="castRadius"></param>
		/// <param name="maxDistance"></param>
		/// <param name="bounds"></param>
		/// <param name="hitPosition"></param>
		/// <returns></returns>
		public static bool SphereCastAgainstBox(Ray ray, float castRadius, float maxDistance, Box box, out Vector3 hitPosition)
		{
			hitPosition = Vector3.zero;
			var verts = box.Vertices;
			foreach (var vert in verts)
			{
				var p = GetClosestPointOnLine(ray.origin, ray.origin + ray.direction.normalized * maxDistance, vert);
				Vector3 ab = vert - p;
				if (ab.magnitude > castRadius)
				{
					continue;
				}
				hitPosition = p + ab * castRadius / ab.magnitude;
				return true;
			}
			hitPosition = Vector3.zero;
			return false;
		}

	}

	public static class MegalithMathExtensions
	{
		public static Vector3 LeftBottomBack(this Bounds bounds)
		{
			return bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
		}

		public static Vector3 RightBottomBack(this Bounds bounds)
		{
			return bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
		}

		public static Vector3 LeftUpBack(this Bounds bounds)
		{
			return bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z);
		}

		public static Vector3 RightUpBack(this Bounds bounds)
		{
			return bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z);
		}

		public static Vector3 LeftBottomFront(this Bounds bounds)
		{
			return bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z);
		}

		public static Vector3 RightBottomFront(this Bounds bounds)
		{
			return bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z);
		}

		public static Vector3 LeftUpFront(this Bounds bounds)
		{
			return bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z);
		}

		public static Vector3 RightUpFront(this Bounds bounds)
		{
			return bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
		}

		public static bool IsCube(this Bounds bounds)
		{
			return bounds.extents.x == bounds.extents.y && bounds.extents.y == bounds.extents.z;
		}

		public static Vector3[] GetVertices(this Bounds bounds)
		{
			var vertices = new Vector3[8];
			vertices[0] = bounds.RightBottomBack();
			vertices[1] = bounds.RightBottomFront();
			vertices[2] = bounds.RightUpBack();
			vertices[3] = bounds.RightUpFront();
			vertices[4] = bounds.LeftBottomBack();
			vertices[5] = bounds.LeftBottomFront();
			vertices[6] = bounds.LeftUpBack();
			vertices[7] = bounds.LeftUpFront();
			return vertices;
		}

		public static Box ToBox(this Bounds bounds)
		{
			return new Box(bounds.RightBottomBack(), bounds.RightBottomFront(), bounds.RightUpBack(), bounds.RightUpFront(), bounds.LeftBottomBack(), bounds.LeftBottomFront(), bounds.LeftUpBack(), bounds.LeftUpFront());
		}

		public static Box GetBox(this Transform transform, Mesh mesh)
		{
			var box = mesh.bounds.ToBox();
			var verts = box.Vertices;
			for (int i = 0; i < verts.Length; i++)
			{
				var vert = verts[i];
				verts[i] = transform.TransformPoint(vert);
			}
			box.Vertices = verts;
			return box;
		}		
	}
}