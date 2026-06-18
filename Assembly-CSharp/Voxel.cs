using System;
using UnityEngine;

public class Voxel
{
	public static bool Raycast(World _worldData, Ray ray, float distance, int _hitmask, float _sphereSize)
	{
		return Voxel.Raycast(_worldData, ray, distance, -538488845, _hitmask, _sphereSize);
	}

	public static bool Raycast(World _worldData, Ray ray, float distance, bool bHitTransparentBlocks, bool bHitNotCollidableBlocks)
	{
		return Voxel.Raycast(_worldData, ray, distance, -538488845, 66 | (bHitTransparentBlocks ? 1 : 0) | (bHitNotCollidableBlocks ? 4 : 0), 0f);
	}

	public static bool Raycast(World _world, Ray ray, float distance, int _layerMask, int _hitMask, float _sphereRadius)
	{
		return Voxel.raycastNew(_world, ray, distance, _layerMask, _hitMask, _sphereRadius);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void terrainMeshHit(World _world, Vector3 dirNormalized, string phsxTag, Vector3 phsxHitpPoint, HitInfoDetails.VoxelData lastHitData, int _layerMask, int _hitMask)
	{
		Vector3 vector = phsxHitpPoint + dirNormalized * 0.01f;
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache == null)
		{
			return;
		}
		Vector3 vector2 = chunkCache.ToLocalPosition(vector);
		dirNormalized = chunkCache.ToLocalVector(dirNormalized);
		Voxel.voxelRayHitInfo.hit.blockPos = World.worldToBlockPos(vector2);
		Voxel.voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCache, Voxel.voxelRayHitInfo.hit.blockPos);
		if ((_layerMask & 262144) == 0 && Voxel.voxelRayHitInfo.hit.blockValue.Block.MeshIndex == 3)
		{
			Voxel.voxelRayHitInfo.hit.blockPos = Voxel.OneVoxelStep(World.worldToBlockPos(vector2), vector2, dirNormalized, out Voxel.voxelRayHitInfo.hit.pos, out Voxel.voxelRayHitInfo.hit.blockFace);
			Voxel.voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCache, Voxel.voxelRayHitInfo.hit.blockPos);
		}
		BlockValue blockValue = Voxel.voxelRayHitInfo.hit.blockValue;
		bool flag = phsxTag == "T_Mesh";
		bool flag2 = (!Voxel.voxelRayHitInfo.hit.voxelData.IsOnlyAir() && flag && !blockValue.Block.shape.IsTerrain()) || (!flag && blockValue.Block.shape.IsTerrain());
		if (flag2 || Voxel.voxelRayHitInfo.hit.voxelData.Equals(lastHitData) || ((_hitMask & 2) == 0 && Voxel.voxelRayHitInfo.hit.voxelData.IsOnlyWater()))
		{
			Voxel.voxelRayHitInfo.lastBlockPos = Voxel.voxelRayHitInfo.hit.blockPos;
			bool flag3 = true;
			if (Voxel.voxelRayHitInfo.hit.voxelData.Equals(lastHitData) && !flag2 && !Voxel.voxelRayHitInfo.hit.voxelData.IsOnlyWater())
			{
				lastHitData = Voxel.voxelRayHitInfo.hit.voxelData;
				Voxel.voxelRayHitInfo.hit.blockPos = Voxel.OneVoxelStep(World.worldToBlockPos(vector2), vector2, dirNormalized, out Voxel.voxelRayHitInfo.hit.pos, out Voxel.voxelRayHitInfo.hit.blockFace);
				Voxel.voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCache, Voxel.voxelRayHitInfo.hit.blockPos);
			}
			else
			{
				flag3 = false;
			}
			if (!flag3 || Voxel.voxelRayHitInfo.hit.voxelData.Equals(lastHitData))
			{
				if (Voxel.voxelRayHitInfo.hit.blockPos.y > 0)
				{
					HitInfoDetails.VoxelData voxelData = (Voxel.voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCache, Voxel.voxelRayHitInfo.hit.blockPos - Vector3i.up));
					if (!voxelData.Equals(lastHitData) && !Voxel.voxelRayHitInfo.hit.voxelData.IsOnlyWater() && ((flag && Voxel.voxelRayHitInfo.hit.blockValue.Block.shape.IsTerrain()) || (!flag && !Voxel.voxelRayHitInfo.hit.blockValue.Block.shape.IsTerrain())))
					{
						Voxel.voxelRayHitInfo.hit.blockFace = BlockFace.Top;
						Voxel.voxelRayHitInfo.hit.blockPos = Voxel.voxelRayHitInfo.hit.blockPos - Vector3i.up;
						return;
					}
				}
				if (Voxel.voxelRayHitInfo.hit.blockPos.y < 255)
				{
					HitInfoDetails.VoxelData voxelData = (Voxel.voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCache, Voxel.voxelRayHitInfo.hit.blockPos + Vector3i.up));
					if (!voxelData.Equals(lastHitData) && !Voxel.voxelRayHitInfo.hit.voxelData.IsOnlyWater() && ((flag && Voxel.voxelRayHitInfo.hit.blockValue.Block.shape.IsTerrain()) || (!flag && !Voxel.voxelRayHitInfo.hit.blockValue.Block.shape.IsTerrain())))
					{
						Voxel.voxelRayHitInfo.hit.blockFace = BlockFace.Bottom;
						Voxel.voxelRayHitInfo.hit.blockPos = Voxel.voxelRayHitInfo.hit.blockPos + Vector3i.up;
						return;
					}
				}
				int num = Voxel.calcBestNormalToRaycastHit(chunkCache);
				Voxel.voxelRayHitInfo.hit.blockPos = Voxel.voxelRayHitInfo.hit.blockPos - Voxel.normalsI[num];
				Voxel.voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCache, Voxel.voxelRayHitInfo.hit.blockPos);
				Voxel.voxelRayHitInfo.hit.blockFace = Voxel.normalToFaces[num];
				return;
			}
		}
		else
		{
			int num2 = Voxel.calcBestNormalToRaycastHit(chunkCache);
			Voxel.voxelRayHitInfo.hit.blockFace = Voxel.normalToFaces[num2];
			Ray ray;
			ray..ctor(vector2, -1f * dirNormalized);
			int num3 = 0;
			HitInfoDetails.VoxelData voxelData;
			HitInfoDetails.VoxelData voxelData2;
			do
			{
				Vector3 vector3;
				BlockFace blockFace;
				Voxel.voxelRayHitInfo.lastBlockPos = Voxel.OneVoxelStep(World.worldToBlockPos(ray.origin), ray.origin, ray.direction, out vector3, out blockFace);
				ray.origin = vector3 - dirNormalized * 0.01f;
				voxelData2 = (voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCache, Voxel.voxelRayHitInfo.lastBlockPos));
			}
			while (!voxelData.IsOnlyAir() && voxelData2.BlockValue.Block.isMultiBlock && !voxelData2.IsOnlyWater() && num3++ < 3);
			if (phsxTag == "T_Mesh_B" && MeshDescription.meshes[(int)Voxel.voxelRayHitInfo.hit.blockValue.Block.MeshIndex].Tag != "T_Mesh_B")
			{
				voxelData2 = (voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCache, Voxel.voxelRayHitInfo.hit.blockPos + Vector3i.up));
				if (!voxelData.IsOnlyAir() && MeshDescription.meshes[(int)voxelData2.BlockValue.Block.MeshIndex].Tag == "T_Mesh_B")
				{
					Voxel.voxelRayHitInfo.hit.blockPos = Voxel.voxelRayHitInfo.hit.blockPos + Vector3i.up;
					Voxel.voxelRayHitInfo.hit.voxelData = voxelData2;
					return;
				}
				voxelData2 = (voxelData = HitInfoDetails.VoxelData.GetFrom(chunkCache, Voxel.voxelRayHitInfo.hit.blockPos - Vector3i.up));
				if (!voxelData.IsOnlyAir() && MeshDescription.meshes[(int)voxelData2.BlockValue.Block.MeshIndex].Tag == "T_Mesh_B")
				{
					Voxel.voxelRayHitInfo.hit.blockPos = Voxel.voxelRayHitInfo.hit.blockPos - Vector3i.up;
					Voxel.voxelRayHitInfo.hit.voxelData = voxelData2;
				}
			}
		}
	}

	public static Vector3i GoBackOnVoxels(ChunkCluster cc, Ray newRay, out BlockValue bv)
	{
		bv = BlockValue.Air;
		Vector3i vector3i = Vector3i.zero;
		int num = 0;
		do
		{
			Vector3 vector;
			BlockFace blockFace;
			vector3i = Voxel.OneVoxelStep(World.worldToBlockPos(newRay.origin), newRay.origin, newRay.direction, out vector, out blockFace);
			newRay.origin = vector + newRay.direction * 0.01f;
		}
		while (!(bv = cc.GetBlock(vector3i)).isair && num++ < 3);
		return vector3i;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool raycastNew(World _world, Ray ray, float distance, int _layerMask, int _hitMask, float _sphereRadius)
	{
		bool flag = _sphereRadius > 0.01f;
		bool flag2 = (_hitMask & 1) != 0;
		bool flag3 = (_hitMask & 64) != 0;
		bool flag4 = (_hitMask & 4) != 0;
		bool flag5 = (_hitMask & 2) != 0;
		bool flag6 = (_hitMask & 8) != 0;
		bool flag7 = (_hitMask & 16) != 0;
		bool flag8 = (_hitMask & 128) != 0;
		bool flag9 = (_hitMask & 32) != 0;
		bool flag10 = (_hitMask & 4096) != 0;
		Voxel.voxelRayHitInfo.Clear();
		Voxel.voxelRayHitInfo.ray = ray;
		HitInfoDetails.VoxelData voxelData = default(HitInfoDetails.VoxelData);
		int num = 0;
		while (num++ < 10 && distance > 0f)
		{
			Ray ray2;
			ray2..ctor(ray.origin - Origin.position, ray.direction);
			if (!(flag ? Physics.SphereCast(ray2, _sphereRadius, ref Voxel.phyxRaycastHit, distance, _layerMask) : Physics.Raycast(ray2, ref Voxel.phyxRaycastHit, distance, _layerMask)))
			{
				break;
			}
			Transform transform = Voxel.phyxRaycastHit.collider.transform;
			Vector3 vector = Voxel.phyxRaycastHit.point + Origin.position;
			string text = Voxel.phyxRaycastHit.collider.transform.tag;
			Voxel.voxelRayHitInfo.hitCollider = Voxel.phyxRaycastHit.collider;
			Voxel.voxelRayHitInfo.hitTriangleIdx = Voxel.phyxRaycastHit.triangleIndex;
			Vector3 normalized = ray.direction.normalized;
			if (text == "T_Block")
			{
				GameUtils.FindMasterBlockForEntityModelBlock(_world, normalized, text, vector, transform, Voxel.voxelRayHitInfo);
				text = "B_Mesh";
				if (Voxel.voxelRayHitInfo.fmcHit.voxelData.IsOnlyAir())
				{
					Voxel.voxelRayHitInfo.fmcHit = Voxel.voxelRayHitInfo.hit;
					Voxel.voxelRayHitInfo.fmcHit.pos = vector - normalized * 0.01f;
				}
			}
			else if (text == "T_Deco")
			{
				DecoObject decoObject;
				if (DecoManager.Instance.GetParentBlockOfDecoration(transform, out Voxel.voxelRayHitInfo.hit.blockPos, out decoObject))
				{
					Voxel.voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(_world, Voxel.voxelRayHitInfo.hit.blockPos);
					Voxel.voxelRayHitInfo.hit.pos = vector - ray.direction * 0.1f;
					Voxel.voxelRayHitInfo.hit.distanceSq = ((vector != Vector3.zero) ? (ray.origin - vector).sqrMagnitude : float.MaxValue);
					if (Voxel.voxelRayHitInfo.hit.voxelData.IsOnlyAir())
					{
						BlockValue bv = decoObject.bv;
						bv.damage = decoObject.bv.Block.MaxDamage - 1;
						Voxel.voxelRayHitInfo.hit.voxelData.Set(bv, Voxel.voxelRayHitInfo.hit.voxelData.WaterValue);
					}
					Voxel.voxelRayHitInfo.fmcHit = Voxel.voxelRayHitInfo.hit;
				}
			}
			else
			{
				if (!GameUtils.IsBlockOrTerrain(text))
				{
					Voxel.voxelRayHitInfo.transform = transform;
					Voxel.voxelRayHitInfo.tag = text;
					Voxel.voxelRayHitInfo.bHitValid = true;
					Voxel.voxelRayHitInfo.hit.pos = vector;
					Voxel.voxelRayHitInfo.hit.distanceSq = ((vector != Vector3.zero) ? (ray.origin - vector).sqrMagnitude : float.MaxValue);
					Voxel.voxelRayHitInfo.fmcHit = Voxel.voxelRayHitInfo.hit;
					return true;
				}
				Voxel.terrainMeshHit(_world, normalized, text, vector, voxelData, _layerMask, _hitMask);
				if (Voxel.voxelRayHitInfo.fmcHit.voxelData.IsOnlyAir())
				{
					Voxel.voxelRayHitInfo.fmcHit = Voxel.voxelRayHitInfo.hit;
					Voxel.voxelRayHitInfo.fmcHit.blockPos = Voxel.voxelRayHitInfo.lastBlockPos;
					Voxel.voxelRayHitInfo.fmcHit.pos = vector - normalized * 0.01f;
				}
			}
			voxelData = Voxel.voxelRayHitInfo.hit.voxelData;
			Block block = Voxel.voxelRayHitInfo.hit.blockValue.Block;
			Block block2 = Voxel.voxelRayHitInfo.hit.propValue.blockValue.Block;
			if (Voxel.voxelRayHitInfo.hit.voxelData.IsOnlyWater() && Voxel.voxelRayHitInfo.fmcHit.voxelData.IsOnlyAir())
			{
				Voxel.voxelRayHitInfo.fmcHit.blockPos = Voxel.voxelRayHitInfo.hit.blockPos;
				Voxel.voxelRayHitInfo.fmcHit.voxelData = Voxel.voxelRayHitInfo.hit.voxelData;
				Voxel.voxelRayHitInfo.fmcHit.blockFace = BlockFace.Top;
				Voxel.voxelRayHitInfo.fmcHit.pos = vector;
			}
			bool flag11 = block.IsSeeThrough(_world, Voxel.voxelRayHitInfo.hit.blockPos, Voxel.voxelRayHitInfo.hit.blockValue);
			bool flag12 = block2.IsSeeThrough(_world, Voxel.voxelRayHitInfo.hit.blockPos, Voxel.voxelRayHitInfo.hit.blockValue);
			if (flag10 && block.MaxDamage <= 5)
			{
				if (!Voxel.voxelRayHitInfo.hit.voxelData.IsOnlyWater())
				{
					Voxel.voxelRayHitInfo.fmcHit.voxelData.Clear();
					voxelData.Clear();
				}
			}
			else
			{
				if ((flag3 && (block.IsCollideMovement || block2.IsCollideMovement) && (flag2 || !flag11)) || (flag4 && !block.IsCollideMovement && !block2.IsCollideMovement && !Voxel.voxelRayHitInfo.hit.voxelData.IsOnlyWater()) || (flag6 && (block.IsCollideBullets || block2.IsCollideBullets)) || (flag7 && (block.IsCollideRockets || block2.IsCollideRockets)) || (flag9 && (block.IsCollideArrows || block2.IsCollideArrows)) || (flag8 && (block.IsCollideMelee || block2.IsCollideMelee)) || (flag5 && Voxel.voxelRayHitInfo.hit.voxelData.IsOnlyWater()) || (flag2 && (flag11 || flag12)))
				{
					Voxel.voxelRayHitInfo.tag = text;
					Voxel.voxelRayHitInfo.bHitValid = true;
					Voxel.voxelRayHitInfo.hit.pos = vector;
					Voxel.voxelRayHitInfo.hit.distanceSq = ((vector != Vector3.zero) ? (Voxel.voxelRayHitInfo.ray.origin - vector).sqrMagnitude : float.MaxValue);
					return true;
				}
				if (!Voxel.voxelRayHitInfo.hit.voxelData.IsOnlyWater())
				{
					Voxel.voxelRayHitInfo.fmcHit.voxelData.Clear();
					voxelData.Clear();
				}
			}
			ray.origin = vector + normalized * 0.01f;
			if (Voxel.phyxRaycastHit.distance > 0.01f)
			{
				distance -= Voxel.phyxRaycastHit.distance;
			}
			else
			{
				distance -= 0.01f;
			}
		}
		return false;
	}

	public static Vector3i OneVoxelStep(Vector3i _voxelPos, Vector3 _origin, Vector3 _direction, out Vector3 hitPos, out BlockFace blockFace)
	{
		int num = _voxelPos.x;
		int num2 = _voxelPos.y;
		int num3 = _voxelPos.z;
		int num4 = Math.Sign(_direction.x);
		int num5 = Math.Sign(_direction.y);
		int num6 = Math.Sign(_direction.z);
		Vector3i vector3i = new Vector3i(num + ((num4 > 0) ? 1 : 0), num2 + ((num5 > 0) ? 1 : 0), num3 + ((num6 > 0) ? 1 : 0));
		Vector3 vector;
		vector..ctor((Mathf.Abs(_direction.x) > 1E-05f) ? (((float)vector3i.x - _origin.x) / _direction.x) : float.MaxValue, (Mathf.Abs(_direction.y) > 1E-05f) ? (((float)vector3i.y - _origin.y) / _direction.y) : float.MaxValue, (Mathf.Abs(_direction.z) > 1E-05f) ? (((float)vector3i.z - _origin.z) / _direction.z) : float.MaxValue);
		Vector3 vector2;
		vector2..ctor((Mathf.Abs(_direction.x) > 1E-05f) ? ((float)num4 / _direction.x) : float.MaxValue, (Mathf.Abs(_direction.y) > 1E-05f) ? ((float)num5 / _direction.y) : float.MaxValue, (Mathf.Abs(_direction.z) > 1E-05f) ? ((float)num6 / _direction.z) : float.MaxValue);
		hitPos = _origin;
		blockFace = BlockFace.Top;
		if (vector.x < vector.y && vector.x < vector.z && num4 != 0)
		{
			num += num4;
			hitPos = _origin + vector.x * _direction;
			vector.x += vector2.x;
			blockFace = ((num4 > 0) ? BlockFace.West : BlockFace.East);
		}
		else if (vector.y < vector.z && num5 != 0)
		{
			num2 += num5;
			hitPos = _origin + vector.y * _direction;
			vector.y += vector2.y;
			blockFace = ((num5 > 0) ? BlockFace.Bottom : BlockFace.Top);
		}
		else
		{
			if (num6 == 0)
			{
				Log.Error("Voxel error: GetNextBlockHit, tMax=" + vector.ToCultureInvariantString() + ", tDelta=" + vector2.ToCultureInvariantString());
				return Vector3i.zero;
			}
			num3 += num6;
			hitPos = _origin + vector.z * _direction;
			vector.z += vector2.z;
			blockFace = ((num6 > 0) ? BlockFace.South : BlockFace.North);
		}
		return new Vector3i(num, num2, num3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int calcBestNormalToRaycastHit(ChunkCluster _cc)
	{
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < Voxel.normals.Length; i++)
		{
			float num3 = Vector3.Dot(_cc.ToLocalVector(Voxel.phyxRaycastHit.normal), Voxel.normals[i]);
			if (num3 > 0f && num3 > num)
			{
				num = num3;
				num2 = i;
			}
		}
		return num2;
	}

	public static bool RaycastOnVoxels(World _world, Ray ray, float distance, int _layerMask, int _hitMask, float _sphereSize)
	{
		bool flag = _sphereSize > 0f;
		Vector3 vector = Vector3.zero;
		string text = string.Empty;
		if (!(flag ? Physics.SphereCast(new Ray(ray.origin - Origin.position, ray.direction), _sphereSize, ref Voxel.phyxRaycastHit, distance, _layerMask) : Physics.Raycast(new Ray(ray.origin - Origin.position, ray.direction), ref Voxel.phyxRaycastHit, distance, _layerMask)))
		{
			return false;
		}
		Transform transform = Voxel.phyxRaycastHit.collider.transform;
		vector = Voxel.phyxRaycastHit.point + Origin.position;
		text = Voxel.phyxRaycastHit.collider.transform.tag;
		if (!GameManager.bVolumeBlocksEditing)
		{
			Voxel.voxelRayHitInfo.bHitValid = true;
			Voxel.voxelRayHitInfo.tag = text;
			Voxel.voxelRayHitInfo.hit.pos = vector;
			Voxel.voxelRayHitInfo.transform = transform;
			return true;
		}
		if (transform.gameObject.layer == 28)
		{
			Voxel.voxelRayHitInfo.bHitValid = true;
			Voxel.voxelRayHitInfo.tag = text;
			Voxel.voxelRayHitInfo.hit.pos = vector;
			Voxel.voxelRayHitInfo.transform = transform;
			Voxel.voxelRayHitInfo.hit.blockPos = World.worldToBlockPos(vector - ray.direction * 0.1f);
			Voxel.voxelRayHitInfo.lastBlockPos = Voxel.voxelRayHitInfo.hit.blockPos;
			Voxel.voxelRayHitInfo.hit.voxelData.Clear();
			return true;
		}
		Vector3 vector2 = Vector3.zero;
		string text2 = string.Empty;
		if (Voxel.GetNextBlockHit(_world, ray, distance, _hitMask, flag))
		{
			text2 = "B_Mesh";
			vector2 = Voxel.voxelRayHitInfo.hit.pos;
		}
		Voxel.voxelRayHitInfo.ray = ray;
		float num = ((vector != Vector3.zero) ? (ray.origin - vector).sqrMagnitude : float.MaxValue);
		float num2 = ((vector2 != Vector3.zero) ? (ray.origin - vector2).sqrMagnitude : float.MaxValue);
		if (num < num2)
		{
			DecoObject decoObject;
			if (text == "T_Block")
			{
				Voxel.voxelRayHitInfo.tag = "B_Mesh";
				GameUtils.FindMasterBlockForEntityModelBlock(_world, ray.direction.normalized, text, vector, transform, Voxel.voxelRayHitInfo);
			}
			else if (text == "T_Deco" && DecoManager.Instance.GetParentBlockOfDecoration(transform, out Voxel.voxelRayHitInfo.hit.blockPos, out decoObject))
			{
				Voxel.voxelRayHitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(_world, Voxel.voxelRayHitInfo.hit.blockPos);
				Voxel.voxelRayHitInfo.tag = text;
				Voxel.voxelRayHitInfo.transform = transform;
				if (Voxel.voxelRayHitInfo.hit.voxelData.IsOnlyAir())
				{
					BlockValue bv = decoObject.bv;
					bv.damage = decoObject.bv.Block.MaxDamage - 1;
					Voxel.voxelRayHitInfo.hit.voxelData.Set(bv, Voxel.voxelRayHitInfo.hit.voxelData.WaterValue);
				}
			}
			else
			{
				Voxel.voxelRayHitInfo.tag = text;
				Voxel.voxelRayHitInfo.transform = transform;
			}
			Voxel.voxelRayHitInfo.bHitValid = true;
			Voxel.voxelRayHitInfo.hit.pos = vector;
			Voxel.voxelRayHitInfo.hit.distanceSq = num;
			Voxel.voxelRayHitInfo.fmcHit.blockPos = Voxel.voxelRayHitInfo.hit.blockPos;
			Voxel.voxelRayHitInfo.fmcHit.voxelData = Voxel.voxelRayHitInfo.hit.voxelData;
			Voxel.voxelRayHitInfo.fmcHit.pos = Voxel.voxelRayHitInfo.hit.pos;
			return true;
		}
		if (num2 != 3.4028235E+38f)
		{
			Voxel.voxelRayHitInfo.bHitValid = true;
			Voxel.voxelRayHitInfo.tag = text2;
			Voxel.voxelRayHitInfo.hit.pos = vector2;
			Voxel.voxelRayHitInfo.hit.distanceSq = num2;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool GetNextBlockHit(World _worldData, Ray ray, float _distance, int _hitMask, bool bCastSphere)
	{
		bool flag = (_hitMask & 1) != 0;
		bool flag2 = (_hitMask & 64) != 0;
		bool flag3 = (_hitMask & 4) != 0;
		bool flag4 = (_hitMask & 2) != 0;
		bool flag5 = (_hitMask & 8) != 0;
		bool flag6 = (_hitMask & 16) != 0;
		bool flag7 = (_hitMask & 32) != 0;
		bool flag8 = (_hitMask & 128) != 0;
		bool flag9 = (_hitMask & 256) != 0;
		Voxel.voxelRayHitInfo.Clear();
		Vector3i vector3i = new Vector3i(Utils.Fastfloor(ray.origin.x), Utils.Fastfloor(ray.origin.y), Utils.Fastfloor(ray.origin.z));
		int num = vector3i.x;
		int num2 = vector3i.y;
		int num3 = vector3i.z;
		int num4 = Math.Sign(ray.direction.x);
		int num5 = Math.Sign(ray.direction.y);
		int num6 = Math.Sign(ray.direction.z);
		Vector3i vector3i2 = new Vector3i(num + ((num4 > 0) ? 1 : 0), num2 + ((num5 > 0) ? 1 : 0), num3 + ((num6 > 0) ? 1 : 0));
		Vector3 vector;
		vector..ctor((Mathf.Abs(ray.direction.x) > 1E-05f) ? (((float)vector3i2.x - ray.origin.x) / ray.direction.x) : float.MaxValue, (Mathf.Abs(ray.direction.y) > 1E-05f) ? (((float)vector3i2.y - ray.origin.y) / ray.direction.y) : float.MaxValue, (Mathf.Abs(ray.direction.z) > 1E-05f) ? (((float)vector3i2.z - ray.origin.z) / ray.direction.z) : float.MaxValue);
		Vector3 vector2;
		vector2..ctor((Mathf.Abs(ray.direction.x) > 1E-05f) ? ((float)num4 / ray.direction.x) : float.MaxValue, (Mathf.Abs(ray.direction.y) > 1E-05f) ? ((float)num5 / ray.direction.y) : float.MaxValue, (Mathf.Abs(ray.direction.z) > 1E-05f) ? ((float)num6 / ray.direction.z) : float.MaxValue);
		Vector3 vector3 = ray.origin;
		float num7 = _distance * _distance;
		HitInfoDetails.VoxelData from;
		for (;;)
		{
			from = HitInfoDetails.VoxelData.GetFrom(_worldData, new Vector3i(num, num2, num3));
			if (!from.IsOnlyAir())
			{
				if (flag9)
				{
					break;
				}
				Block block = from.BlockValue.Block;
				if (from.IsOnlyWater() && Voxel.voxelRayHitInfo.fmcHit.voxelData.IsOnlyAir())
				{
					Voxel.voxelRayHitInfo.fmcHit.blockPos = new Vector3i(num, num2, num3);
					Voxel.voxelRayHitInfo.fmcHit.voxelData = from;
					Voxel.voxelRayHitInfo.fmcHit.blockFace = Voxel.voxelRayHitInfo.hit.blockFace;
					Voxel.voxelRayHitInfo.fmcHit.pos = vector3;
				}
				bool flag10 = block.IsSeeThrough(_worldData, new Vector3i(num, num2, num3), from.BlockValue);
				Vector3 vector4;
				if (((flag2 && block.IsCollideMovement && (flag || !flag10)) || (flag3 && !block.IsCollideMovement && !from.IsOnlyWater()) || (flag5 && block.IsCollideBullets) || (flag6 && block.IsCollideRockets) || (flag7 && block.IsCollideArrows) || (flag8 && block.IsCollideMelee) || (flag4 && from.IsOnlyWater()) || (flag && flag10)) && block.intersectRayWithBlock(from.BlockValue, num, num2, num3, ray, out vector4, _worldData))
				{
					goto Block_24;
				}
			}
			if ((vector3 - ray.origin).sqrMagnitude > num7)
			{
				return false;
			}
			Voxel.voxelRayHitInfo.lastBlockPos = new Vector3i(num, num2, num3);
			if (vector.x < vector.y && vector.x < vector.z && num4 != 0)
			{
				num += num4;
				vector3 = ray.origin + vector.x * ray.direction;
				vector.x += vector2.x;
				Voxel.voxelRayHitInfo.hit.blockFace = ((num4 > 0) ? BlockFace.West : BlockFace.East);
			}
			else if (vector.y < vector.z && num5 != 0)
			{
				num2 += num5;
				vector3 = ray.origin + vector.y * ray.direction;
				vector.y += vector2.y;
				Voxel.voxelRayHitInfo.hit.blockFace = ((num5 > 0) ? BlockFace.Bottom : BlockFace.Top);
			}
			else
			{
				if (num6 == 0)
				{
					goto IL_0644;
				}
				num3 += num6;
				vector3 = ray.origin + vector.z * ray.direction;
				vector.z += vector2.z;
				Voxel.voxelRayHitInfo.hit.blockFace = ((num6 > 0) ? BlockFace.South : BlockFace.North);
			}
		}
		Voxel.voxelRayHitInfo.hit.blockPos = new Vector3i(num, num2, num3);
		Voxel.voxelRayHitInfo.hit.voxelData = from;
		Voxel.voxelRayHitInfo.bHitValid = true;
		Voxel.voxelRayHitInfo.hit.pos = vector3;
		return true;
		Block_24:
		Voxel.voxelRayHitInfo.hit.blockPos = new Vector3i(num, num2, num3);
		Voxel.voxelRayHitInfo.hit.voxelData = from;
		Voxel.voxelRayHitInfo.bHitValid = true;
		Voxel.voxelRayHitInfo.hit.pos = vector3;
		if (Voxel.voxelRayHitInfo.fmcHit.voxelData.IsOnlyAir())
		{
			Voxel.voxelRayHitInfo.fmcHit.blockPos = new Vector3i(num, num2, num3);
			Voxel.voxelRayHitInfo.fmcHit.voxelData = from;
			Voxel.voxelRayHitInfo.fmcHit.blockFace = Voxel.voxelRayHitInfo.hit.blockFace;
			Voxel.voxelRayHitInfo.fmcHit.pos = vector3;
		}
		return true;
		IL_0644:
		Log.Error("Voxel error: GetNextBlockHit, tMax=" + vector.ToCultureInvariantString() + ", tDelta=" + vector2.ToCultureInvariantString());
		return false;
	}

	public static void GetCellsOnRay(Ray ray, Voxel.DeletageNextBlockHit _delegateCallback)
	{
		Vector3i vector3i = new Vector3i(Utils.Fastfloor(ray.origin.x), Utils.Fastfloor(ray.origin.y), Utils.Fastfloor(ray.origin.z));
		int num = vector3i.x;
		int num2 = vector3i.y;
		int num3 = vector3i.z;
		int num4 = Math.Sign(ray.direction.x);
		int num5 = Math.Sign(ray.direction.y);
		int num6 = Math.Sign(ray.direction.z);
		Vector3i vector3i2 = new Vector3i(num + ((num4 > 0) ? 1 : 0), num2 + ((num5 > 0) ? 1 : 0), num3 + ((num6 > 0) ? 1 : 0));
		Vector3 vector;
		vector..ctor((Mathf.Abs(ray.direction.x) > 1E-05f) ? (((float)vector3i2.x - ray.origin.x) / ray.direction.x) : float.MaxValue, (Mathf.Abs(ray.direction.y) > 1E-05f) ? (((float)vector3i2.y - ray.origin.y) / ray.direction.y) : float.MaxValue, (Mathf.Abs(ray.direction.z) > 1E-05f) ? (((float)vector3i2.z - ray.origin.z) / ray.direction.z) : float.MaxValue);
		Vector3 vector2;
		vector2..ctor((Mathf.Abs(ray.direction.x) > 1E-05f) ? ((float)num4 / ray.direction.x) : float.MaxValue, (Mathf.Abs(ray.direction.y) > 1E-05f) ? ((float)num5 / ray.direction.y) : float.MaxValue, (Mathf.Abs(ray.direction.z) > 1E-05f) ? ((float)num6 / ray.direction.z) : float.MaxValue);
		while (_delegateCallback(num, num2, num3))
		{
			if (vector.x < vector.y && vector.x < vector.z && num4 != 0)
			{
				num += num4;
				vector.x += vector2.x;
			}
			else if (vector.y < vector.z && num5 != 0)
			{
				num2 += num5;
				vector.y += vector2.y;
			}
			else
			{
				if (num6 == 0)
				{
					Log.Error("Voxel error: GetCellsOnRay, tMax=" + vector.ToCultureInvariantString() + ", tDelta=" + vector2.ToCultureInvariantString());
					return;
				}
				num3 += num6;
				vector.z += vector2.z;
			}
		}
	}

	public static bool BlockHit(WorldRayHitInfo hitInfo, Vector3i blockPos)
	{
		HitInfoDetails.VoxelData from = HitInfoDetails.VoxelData.GetFrom(GameManager.Instance.World, blockPos);
		if (from.IsOnlyAir())
		{
			hitInfo.bHitValid = false;
			return false;
		}
		hitInfo.bHitValid = true;
		hitInfo.tag = "B_Mesh";
		hitInfo.hit.pos = World.blockToTransformPos(blockPos);
		hitInfo.hit.blockPos = blockPos;
		hitInfo.hit.voxelData = from;
		hitInfo.fmcHit = hitInfo.hit;
		return true;
	}

	public static int ToHitMask(string _maskNames)
	{
		int num = 0;
		if (_maskNames.Length > 0)
		{
			foreach (string text in _maskNames.Split(Voxel.hitMaskSeparator, StringSplitOptions.RemoveEmptyEntries))
			{
				uint num2 = <PrivateImplementationDetails>.ComputeStringHash(text);
				if (num2 <= 1191965900U)
				{
					if (num2 <= 727447887U)
					{
						if (num2 != 281207590U)
						{
							if (num2 == 727447887U)
							{
								if (text == "LiquidOnly")
								{
									num |= 2;
								}
							}
						}
						else if (text == "Arrow")
						{
							num |= 32;
						}
					}
					else if (num2 != 747600501U)
					{
						if (num2 == 1191965900U)
						{
							if (text == "Moveable")
							{
								num |= 4;
							}
						}
					}
					else if (text == "Rocket")
					{
						num |= 16;
					}
				}
				else if (num2 <= 2422620547U)
				{
					if (num2 != 1271195163U)
					{
						if (num2 == 2422620547U)
						{
							if (text == "NotMoveable")
							{
								num |= 64;
							}
						}
					}
					else if (text == "Melee")
					{
						num |= 128;
					}
				}
				else if (num2 != 2993278417U)
				{
					if (num2 == 3589267609U)
					{
						if (text == "Bullet")
						{
							num |= 8;
						}
					}
				}
				else if (text == "Transparent")
				{
					num |= 1;
				}
			}
		}
		return num;
	}

	public const int HM_All = 4095;

	public const int HM_None = 0;

	public const int HM_Transparent = 1;

	public const int HM_LiquidOnly = 2;

	public const int HM_Moveable = 4;

	public const int HM_Bullet = 8;

	public const int HM_Rocket = 16;

	public const int HM_Arrows = 32;

	public const int HM_NotMoveable = 64;

	public const int HM_Melee = 128;

	public const int HM_FirstNotEmptyBlock = 256;

	public const int HM_IgnoreFragile = 4096;

	public static RaycastHit phyxRaycastHit;

	public static WorldRayHitInfo voxelRayHitInfo = new WorldRayHitInfo();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3i[] normalsI = new Vector3i[]
	{
		new Vector3i(0, -1, 0),
		new Vector3i(0, 0, -1),
		new Vector3i(-1, 0, 0),
		new Vector3i(0, 1, 0),
		new Vector3i(0, 0, 1),
		new Vector3i(1, 0, 0)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3[] normals = new Vector3[]
	{
		new Vector3(0f, -1f, 0f),
		new Vector3(0f, 0f, -1f),
		new Vector3(-1f, 0f, 0f),
		new Vector3(0f, 1f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 0f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockFace[] normalToFaces = new BlockFace[]
	{
		BlockFace.Bottom,
		BlockFace.South,
		BlockFace.West,
		BlockFace.Top,
		BlockFace.North,
		BlockFace.East
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] hitMaskSeparator = new char[] { ',' };

	public delegate bool DeletageNextBlockHit(int _x, int _y, int _z);
}
