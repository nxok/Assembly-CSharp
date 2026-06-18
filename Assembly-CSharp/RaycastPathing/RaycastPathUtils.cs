using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing
{
	[Preserve]
	public class RaycastPathUtils
	{
		public static bool IsPositionBlocked(Vector3 start, Vector3 end, out RaycastHit hit, int layerMask = 0, bool debugDraw = false)
		{
			Vector3 vector = end - start;
			return RaycastPathUtils.IsPositionBlocked(new Ray(start - Origin.position, vector), out hit, layerMask, debugDraw, vector.magnitude + 1f);
		}

		public static bool IsPositionBlocked(Vector3 start, Vector3 end, int layerMask = 0, bool debugDraw = false)
		{
			RaycastHit raycastHit;
			return RaycastPathUtils.IsPositionBlocked(start, end, out raycastHit, layerMask, debugDraw);
		}

		public static bool IsPointBlocked(Vector3 start, Vector3 end, int layerMask = 0, bool debugDraw = false, float duration = 0f)
		{
			RaycastHit raycastHit;
			return RaycastPathUtils.CheckPositionBlocked(start, end, out raycastHit, layerMask, debugDraw, duration);
		}

		public static bool CheckPositionBlocked(Vector3 start, Vector3 end, out RaycastHit hit, int layerMask = 0, bool debugDraw = false, float duration = 0f)
		{
			Vector3 vector = end - start;
			return RaycastPathUtils.CheckPositionBlocked(new Ray(start - Origin.position, vector), out hit, layerMask, debugDraw, vector.magnitude, duration);
		}

		public static bool CheckPositionBlocked(Ray ray, out RaycastHit hit, int layerMask = 0, bool debugDraw = false, float maxDist = 100f, float duration = 0f)
		{
			bool flag = Physics.Raycast(ray, ref hit, maxDist, layerMask);
			if (debugDraw)
			{
				if (flag)
				{
					Utils.DrawLine(ray.origin, ray.origin + ray.direction * hit.distance, Color.magenta, Color.red, 1, duration);
				}
				else
				{
					Utils.DrawLine(ray.origin, ray.origin + ray.direction * maxDist, Color.cyan, Color.blue, 1, duration);
				}
			}
			return flag;
		}

		public static bool IsPositionBlocked(Ray ray, out RaycastHit hit, int layerMask = 0, bool debugDraw = false, float maxDist = 100f)
		{
			bool flag = Physics.Raycast(ray, ref hit, maxDist, layerMask);
			if (debugDraw)
			{
				if (flag)
				{
					Utils.DrawLine(ray.origin, ray.origin + ray.direction * hit.distance, Color.magenta, Color.red, 1, 5f);
				}
				else
				{
					Utils.DrawLine(ray.origin, ray.origin + ray.direction * maxDist, Color.cyan, Color.blue, 1, 5f);
				}
			}
			return flag;
		}

		public static bool IsPositionBlocked(Ray ray, int layerMask = 0, bool debugDraw = false, float maxDist = 100f)
		{
			RaycastHit raycastHit;
			return RaycastPathUtils.IsPositionBlocked(ray, out raycastHit, layerMask, debugDraw, 100f);
		}

		public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 1f)
		{
			Utils.DrawLine(start - Origin.position, end - Origin.position, color, color, 10, duration);
		}

		public static void DrawBounds(Vector3i pos, Color color, float duration, float scale = 1f)
		{
			Utils.DrawBoxLines(new Vector3((float)pos.x, (float)pos.y, (float)pos.z) - Origin.position, new Vector3((float)pos.x + scale, (float)pos.y + scale, (float)pos.z + scale) - Origin.position, color, duration);
		}

		public static void DrawBounds(Vector3 pos, Color color, float duration, float scale = 1f)
		{
			Utils.DrawBoxLines(new Vector3i(pos.x, pos.y, pos.z) - Origin.position, new Vector3i(pos.x + scale, pos.y + scale, pos.z + scale) - Origin.position, color, duration);
		}

		public static void DrawNode(RaycastNode node, Color color, float duration)
		{
			RaycastPathUtils.DrawNode(node.Min, node.Max, color, duration);
		}

		public static void DrawNode(Vector3 min, Vector3 max, Color color, float duration)
		{
			RaycastPathUtils.DrawVolume(min - Origin.position, max - Origin.position, color, duration);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void DrawVolume(Vector3 min, Vector3 max, Color color, float duration)
		{
			Vector3 vector;
			vector..ctor(max.x, min.y, min.z);
			Vector3 vector2;
			vector2..ctor(min.x, max.y, min.z);
			Vector3 vector3;
			vector3..ctor(min.x, min.y, max.z);
			Vector3 vector4;
			vector4..ctor(min.x, max.y, max.z);
			Vector3 vector5;
			vector5..ctor(max.x, min.y, max.z);
			Vector3 vector6;
			vector6..ctor(max.x, max.y, min.z);
			Debug.DrawLine(min, vector, color, duration);
			Debug.DrawLine(min, vector2, color, duration);
			Debug.DrawLine(min, vector3, color, duration);
			Debug.DrawLine(max, vector4, color, duration);
			Debug.DrawLine(max, vector5, color, duration);
			Debug.DrawLine(max, vector6, color, duration);
			Debug.DrawLine(vector4, vector2, color, duration);
			Debug.DrawLine(vector, vector5, color, duration);
			Debug.DrawLine(vector5, vector3, color, duration);
			Debug.DrawLine(vector2, vector6, color, duration);
			Debug.DrawLine(vector4, vector3, color, duration);
			Debug.DrawLine(vector, vector6, color, duration);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cMaxRayDist = 100;
	}
}
