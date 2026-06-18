using System;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainAlignmentUtils
{
	public static bool AlignToTerrain(Block block, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd, TerrainAlignmentMode alignmentMode)
	{
		BlockShape shape = _blockValue.Block.shape;
		BlockShapeModelEntity blockShapeModelEntity = shape as BlockShapeModelEntity;
		if (blockShapeModelEntity == null)
		{
			return false;
		}
		Bounds blockPlacementBounds = GameUtils.GetBlockPlacementBounds(_blockValue.Block);
		if (_ebcd == null || !_ebcd.bHasTransform || (blockPlacementBounds.size.x <= 1f && blockPlacementBounds.size.z <= 1f))
		{
			return false;
		}
		if (_blockValue.ischild)
		{
			_blockPos += new Vector3i(_blockValue.parentx, _blockValue.parenty, _blockValue.parentz);
		}
		Quaternion rotation = shape.GetRotation(_blockValue);
		Vector3 rotatedOffset = blockShapeModelEntity.GetRotatedOffset(block, rotation);
		Transform transform = _ebcd.transform;
		transform.gameObject.SetActive(false);
		blockPlacementBounds.size -= new Vector3(1f, 0f, 1f);
		if (alignmentMode == TerrainAlignmentMode.Vehicle)
		{
			if (blockPlacementBounds.size.x > blockPlacementBounds.size.z)
			{
				if (blockPlacementBounds.size.x > 2.5f)
				{
					blockPlacementBounds.size = new Vector3(blockPlacementBounds.size.x - 2f, blockPlacementBounds.size.y, blockPlacementBounds.size.z);
				}
			}
			else if (blockPlacementBounds.size.z > 2.5f)
			{
				blockPlacementBounds.size = new Vector3(blockPlacementBounds.size.x, blockPlacementBounds.size.y, blockPlacementBounds.size.z - 2f);
			}
		}
		Vector3 vector = World.blockToTransformPos(_blockPos) - Origin.position + new Vector3(0f, 0.5f, 0f) + rotation * new Vector3(blockPlacementBounds.center.x, 0f, blockPlacementBounds.center.z);
		Vector3 zero = Vector3.zero;
		float num = ((alignmentMode == TerrainAlignmentMode.Vehicle) ? 0.2f : 0f);
		Vector3 vector2;
		vector2..ctor(0f, 1f, 0f);
		uint num2 = 0U;
		for (float num3 = -blockPlacementBounds.extents.z - 1f; num3 <= blockPlacementBounds.extents.z + 1.01f; num3 += 1f)
		{
			zero.z = Utils.FastClamp(num3, -blockPlacementBounds.extents.z - 0.45f, blockPlacementBounds.extents.z + 0.45f);
			bool flag = Mathf.Abs(num3) > blockPlacementBounds.extents.z;
			for (float num4 = -blockPlacementBounds.extents.x - 1f; num4 <= blockPlacementBounds.extents.x + 1.01f; num4 += 1f)
			{
				zero.x = Utils.FastClamp(num4, -blockPlacementBounds.extents.x - 0.45f, blockPlacementBounds.extents.x + 0.45f);
				bool flag2 = Mathf.Abs(num4) > blockPlacementBounds.extents.x;
				RaycastHit raycastHit;
				if (Physics.Raycast(vector + rotation * zero + vector2, Vector3.down, ref raycastHit, 5f, 1082195968))
				{
					if (raycastHit.distance >= 0.5f && raycastHit.distance <= 1.95f)
					{
						num2 += 1U;
					}
					Vector3 point = raycastHit.point;
					if (!flag2 || !flag)
					{
						point.y -= num;
					}
					TerrainAlignmentUtils.alignHighest.Add(point);
				}
			}
		}
		Quaternion quaternion = transform.rotation;
		Vector3 vector3 = transform.position;
		if (num2 > 0U && TerrainAlignmentUtils.alignHighest.Count >= 3)
		{
			TerrainAlignmentUtils.alignHighest.Sort((Vector3 v1, Vector3 v2) => v2.y.CompareTo(v1.y));
			Quaternion quaternion2 = Quaternion.Inverse(rotation);
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < TerrainAlignmentUtils.alignHighest.Count; j++)
				{
					Vector3 vector4 = quaternion2 * (TerrainAlignmentUtils.alignHighest[j] - vector);
					float num5 = (Mathf.Atan2(vector4.z, vector4.x) + 3.1415927f) * 0.6366198f;
					if (num5 >= (float)i && num5 < (float)(i + 1))
					{
						TerrainAlignmentUtils.alignHighest4.Add(TerrainAlignmentUtils.alignHighest[j]);
						break;
					}
				}
			}
			if (TerrainAlignmentUtils.alignHighest4.Count >= 3)
			{
				TerrainAlignmentUtils.alignHighest4.Sort((Vector3 v1, Vector3 v2) => v2.y.CompareTo(v1.y));
				Vector3 vector5 = TerrainAlignmentUtils.alignHighest4[0];
				Vector3 vector6 = Vector3.Cross(TerrainAlignmentUtils.alignHighest4[2] - vector5, TerrainAlignmentUtils.alignHighest4[1] - vector5);
				if (vector6.y > 0.1f || vector6.y < -0.1f)
				{
					vector6.Normalize();
					if (vector6.y < 0f)
					{
						vector6 *= -1f;
					}
					quaternion = Quaternion.FromToRotation(Vector3.up, vector6) * rotation;
					Vector3 vector7 = _blockPos.ToVector3Center() - Origin.position;
					vector7 += rotatedOffset;
					Plane plane;
					plane..ctor(vector6, vector5);
					Ray ray;
					ray..ctor(vector7, Vector3.down);
					float num6;
					if (plane.Raycast(ray, ref num6))
					{
						Vector3 point2 = ray.GetPoint(num6);
						vector7.y = point2.y + rotatedOffset.y;
						vector3 = vector7;
					}
				}
			}
		}
		transform.gameObject.SetActive(true);
		TerrainAlignmentUtils.alignHighest.Clear();
		TerrainAlignmentUtils.alignHighest4.Clear();
		if (transform.position != vector3 || transform.rotation != quaternion)
		{
			transform.SetPositionAndRotation(vector3, quaternion);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector3> alignHighest = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Vector3> alignHighest4 = new List<Vector3>();
}
