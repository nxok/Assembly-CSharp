using System;
using UnityEngine;

public class GeometryUtils
{
	public static bool IntersectRayTriangle(Ray ray, GeometryUtils.Triangle tri, out Vector3 outNormal, out float hitDistance)
	{
		outNormal = Vector3.zero;
		hitDistance = 0f;
		Vector3 vector = tri.v1 - tri.v0;
		Vector3 vector2 = tri.v2 - tri.v0;
		Vector3 normal = tri.normal;
		float num = Vector3.Dot(normal, ray.direction);
		if (Mathf.Abs(num) < Mathf.Epsilon)
		{
			return false;
		}
		float num2 = (Vector3.Dot(normal, tri.v0) - Vector3.Dot(normal, ray.origin)) / num;
		if (num2 < 0f)
		{
			return false;
		}
		Vector3 vector3 = ray.origin + ray.direction * num2;
		if (Vector3.Dot(normal, Vector3.Cross(vector, vector3 - tri.v0)) > 0f || Vector3.Dot(normal, Vector3.Cross(vector2, tri.v2 - vector3)) > 0f || Vector3.Dot(normal, Vector3.Cross(tri.v2 - tri.v1, vector3 - tri.v1)) > 0f)
		{
			return false;
		}
		hitDistance = num2;
		outNormal = normal.normalized;
		return true;
	}

	public static Vector3 NearestPointOnLine(Vector3 fromPoint, Vector3 lineStart, Vector3 lineEnd)
	{
		Vector3 vector = fromPoint - lineStart;
		Vector3 vector2 = lineEnd - lineStart;
		float num = Mathf.Clamp01(Vector3.Dot(vector, vector2) / Vector3.Dot(vector2, vector2));
		return lineStart + num * vector2;
	}

	public static Vector3 NearestPointOnEdgeLoop(Vector3 fromPoint, Vector3[] loopPoints, int loopPointCount)
	{
		ValueTuple<float, Vector3> valueTuple = new ValueTuple<float, Vector3>(float.MaxValue, fromPoint);
		for (int i = 0; i < loopPointCount; i++)
		{
			Vector3 vector = loopPoints[i];
			Vector3 vector2 = loopPoints[(i + 1) % loopPointCount];
			Vector3 vector3 = GeometryUtils.NearestPointOnLine(fromPoint, vector, vector2);
			float num = Vector3.Distance(fromPoint, vector3);
			if (num < valueTuple.Item1)
			{
				valueTuple = new ValueTuple<float, Vector3>(num, vector3);
			}
		}
		return valueTuple.Item2;
	}

	public static void RotatePlaneAroundPoint(ref Plane plane, Vector3 pivot, Quaternion rotation)
	{
		if (rotation == Quaternion.identity)
		{
			return;
		}
		Vector3 vector = plane.normal;
		Vector3 vector2 = plane.normal * -plane.distance;
		vector2 = rotation * (vector2 - pivot) + pivot;
		vector = rotation * vector;
		plane.SetNormalAndPosition(vector, vector2);
	}

	public static Rect RotateRectAboutY(Rect rect, float yRot)
	{
		Quaternion quaternion = Quaternion.AngleAxis(yRot, Vector3.up);
		Vector3 vector = Vector3.zero;
		vector.x = rect.min.x - rect.center.x;
		vector.z = rect.min.y - rect.center.y;
		Vector3 vector2 = Vector3.zero;
		vector2.x = rect.max.x - rect.center.x;
		vector2.z = rect.max.y - rect.center.y;
		vector = quaternion * vector;
		vector2 = quaternion * vector2;
		vector.x += rect.center.x;
		vector.z += rect.center.y;
		vector2.x += rect.center.x;
		vector2.z += rect.center.y;
		if (vector2.x < vector.x)
		{
			float x = vector.x;
			vector.x = vector2.x;
			vector2.x = x;
		}
		if (vector2.z < vector.z)
		{
			float z = vector.z;
			vector.z = vector2.z;
			vector2.z = z;
		}
		return new Rect(new Vector2(vector.x, vector.z), new Vector2(vector2.x - vector.x, vector2.x - vector.x));
	}

	public struct Triangle
	{
		public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
		{
			this.v0 = v0;
			this.v1 = v1;
			this.v2 = v2;
			this.normal = Vector3.Normalize(Vector3.Cross(v0 - v2, v0 - v1));
		}

		public Vector3 v0;

		public Vector3 v1;

		public Vector3 v2;

		public Vector3 normal;
	}
}
