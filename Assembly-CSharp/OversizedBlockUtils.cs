using System;
using System.Collections.Generic;
using UnityEngine;

public static class OversizedBlockUtils
{
	public static Bounds GetLocalStabilityBounds(Bounds localBounds, Quaternion rotation)
	{
		Vector3 vector = Quaternion.Inverse(rotation) * Vector3.down;
		float num = Vector3.Dot(vector, Vector3.one);
		if (!Mathf.Approximately(Mathf.Abs(num), 1f))
		{
			Debug.LogError(string.Format("Error in GetLocalStabilityBounds: relativeDown is not an axis-aligned unit vector. Value: {0}. This could suggest an oversized block has been used with an unsupported rotation.", vector));
		}
		Vector3 vector2;
		Vector3 vector3;
		if (num < 0f)
		{
			vector2 = localBounds.min + vector;
			vector3 = localBounds.max + Vector3.Scale(vector, localBounds.size);
		}
		else
		{
			vector2 = localBounds.min + Vector3.Scale(vector, localBounds.size);
			vector3 = localBounds.max + vector;
		}
		Bounds bounds = default(Bounds);
		bounds.SetMinMax(vector2, vector3);
		bounds.extents += new Vector3(-0.05f, -0.05f, -0.05f);
		return bounds;
	}

	public unsafe static void GetWorldAlignedBoundsExtents(Vector3i position, Quaternion rotation, Bounds localBounds, out Vector3i min, out Vector3i max)
	{
		Span<Vector3> span2;
		checked
		{
			Span<Vector3> span = new Span<Vector3>(stackalloc byte[unchecked((UIntPtr)8) * (UIntPtr)sizeof(Vector3)], 8);
			OversizedBlockUtils.GetWorldCorners(ref span, position, rotation, localBounds);
			min = new Vector3i(int.MaxValue, int.MaxValue, int.MaxValue);
			max = new Vector3i(int.MinValue, int.MinValue, int.MinValue);
			span2 = span;
		}
		for (int i = 0; i < span2.Length; i++)
		{
			Vector3 vector = *span2[i];
			min.x = Mathf.Min(min.x, Mathf.FloorToInt(vector.x));
			min.y = Mathf.Min(min.y, Mathf.FloorToInt(vector.y));
			min.z = Mathf.Min(min.z, Mathf.FloorToInt(vector.z));
			max.x = Mathf.Max(max.x, Mathf.CeilToInt(vector.x));
			max.y = Mathf.Max(max.y, Mathf.CeilToInt(vector.y));
			max.z = Mathf.Max(max.z, Mathf.CeilToInt(vector.z));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static void GetWorldCorners(ref Span<Vector3> corners, Vector3i position, Quaternion rotation, Bounds localBounds)
	{
		Vector3 extents = localBounds.extents;
		for (int i = 0; i < 8; i++)
		{
			*corners[i] = localBounds.center + new Vector3(((i & 1) == 0) ? extents.x : (-extents.x), ((i & 2) == 0) ? extents.y : (-extents.y), ((i & 4) == 0) ? extents.z : (-extents.z));
			*corners[i] = position + rotation * *corners[i];
		}
	}

	public static Matrix4x4 GetBlockWorldToLocalMatrix(Vector3i position, Quaternion rotation)
	{
		return Matrix4x4.TRS(position + new Vector3(0.5f, 0.5f, 0.5f), rotation, Vector3.one).inverse;
	}

	public static bool IsBlockCenterWithinBounds(Vector3i blockPosition, Bounds localBounds, Matrix4x4 worldToLocalMatrix)
	{
		Vector3 vector = worldToLocalMatrix.MultiplyPoint3x4(blockPosition + new Vector3(0.5f, 0.5f, 0.5f));
		return localBounds.Contains(vector);
	}

	public static IEnumerable<Vector3i> EnumerateOverlappingCells(Vector3i blockPos, Bounds localBounds, byte rotation, Bounds clipBounds)
	{
		Vector3 vector = BlockShapeNew.GetRotationStatic((int)rotation) * Vector3.right;
		float cos = vector.x;
		float sin = vector.z;
		float centerX = (float)blockPos.x + 0.5f + localBounds.center.x * cos - localBounds.center.z * sin;
		float centerZ = (float)blockPos.z + 0.5f + localBounds.center.x * sin + localBounds.center.z * cos;
		float extentX = localBounds.extents.x;
		float extentZ = localBounds.extents.z;
		float num = Math.Abs(cos);
		float num2 = Math.Abs(sin);
		float num3 = extentX * num + extentZ * num2;
		float num4 = extentX * num2 + extentZ * num;
		int xMin = Math.Max(Mathf.FloorToInt(centerX - num3), Mathf.FloorToInt(clipBounds.min.x));
		int num5 = Math.Max(Mathf.FloorToInt(centerZ - num4), Mathf.FloorToInt(clipBounds.min.z));
		int xMax = Math.Min(Mathf.CeilToInt(centerX + num3) - 1, Mathf.FloorToInt(clipBounds.max.x));
		int zMax = Math.Min(Mathf.CeilToInt(centerZ + num4) - 1, Mathf.FloorToInt(clipBounds.max.z));
		if (xMin > xMax || num5 > zMax)
		{
			yield break;
		}
		int num6;
		if (rotation < 24 || rotation > 27)
		{
			for (int z = num5; z <= zMax; z = num6 + 1)
			{
				for (int x = xMin; x <= xMax; x = num6 + 1)
				{
					yield return new Vector3i(x, blockPos.y, z);
					num6 = x;
				}
				num6 = z;
			}
			yield break;
		}
		for (int z = num5; z <= zMax; z = num6 + 1)
		{
			for (int x = xMin; x <= xMax; x = num6 + 1)
			{
				float num7 = (float)x + 0.5f - centerX;
				float num8 = (float)z + 0.5f - centerZ;
				if (Math.Abs(num7 * cos + num8 * sin) <= extentX + 0.70710677f && Math.Abs(-num7 * sin + num8 * cos) <= extentZ + 0.70710677f)
				{
					yield return new Vector3i(x, blockPos.y, z);
				}
				num6 = x;
			}
			num6 = z;
		}
		yield break;
	}
}
