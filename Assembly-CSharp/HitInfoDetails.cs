using System;
using UnityEngine;

public struct HitInfoDetails
{
	public BlockValue blockValue
	{
		get
		{
			return this.voxelData.BlockValue;
		}
	}

	public WaterValue waterValue
	{
		get
		{
			return this.voxelData.WaterValue;
		}
	}

	public PropRef propRef
	{
		get
		{
			return this.propData.PropRef;
		}
	}

	public PropValue propValue
	{
		get
		{
			return this.propData.PropValue;
		}
	}

	public BlockValueRef blockValueRef
	{
		get
		{
			if (!this.propValue.IsAir)
			{
				return new BlockValueRef(this.propData.PropRef);
			}
			if (!this.blockValue.isair)
			{
				return new BlockValueRef(this.blockPos);
			}
			return BlockValueRef.None;
		}
	}

	public void Clear()
	{
		this.pos = Vector3.zero;
		this.blockPos = Vector3i.zero;
		this.blockFace = BlockFace.Top;
		this.voxelData.Clear();
		this.propData.Clear();
		this.distanceSq = 0f;
	}

	public void CopyFrom(HitInfoDetails _other)
	{
		this.pos = _other.pos;
		this.blockPos = _other.blockPos;
		this.blockFace = _other.blockFace;
		this.voxelData = _other.voxelData;
		this.propData = _other.propData;
		this.distanceSq = _other.distanceSq;
	}

	public HitInfoDetails Clone()
	{
		HitInfoDetails hitInfoDetails = default(HitInfoDetails);
		hitInfoDetails.CopyFrom(this);
		return hitInfoDetails;
	}

	public Vector3 pos;

	public Vector3i blockPos;

	public HitInfoDetails.VoxelData voxelData;

	public HitInfoDetails.PropData propData;

	public BlockFace blockFace;

	public float distanceSq;

	public struct VoxelData : IEquatable<HitInfoDetails.VoxelData>
	{
		public void Set(BlockValue _bv, WaterValue _wv)
		{
			this.BlockValue = _bv;
			this.WaterValue = _wv;
		}

		public static HitInfoDetails.VoxelData GetFrom(ChunkCluster _cc, Vector3i _blockPos)
		{
			return new HitInfoDetails.VoxelData
			{
				BlockValue = _cc.GetBlock(_blockPos),
				WaterValue = _cc.GetWater(_blockPos)
			};
		}

		public static HitInfoDetails.VoxelData GetFrom(World _world, Vector3i _blockPos)
		{
			return new HitInfoDetails.VoxelData
			{
				BlockValue = _world.GetBlock(_blockPos),
				WaterValue = _world.GetWater(_blockPos)
			};
		}

		public static HitInfoDetails.VoxelData GetFrom(IChunk _chunk, int _x, int _y, int _z)
		{
			return new HitInfoDetails.VoxelData
			{
				BlockValue = _chunk.GetBlock(_x, _y, _z),
				WaterValue = _chunk.GetWater(_x, _y, _z)
			};
		}

		public bool IsOnlyAir()
		{
			return this.BlockValue.isair && !this.WaterValue.HasMass();
		}

		public bool IsOnlyWater()
		{
			return this.BlockValue.isair && this.WaterValue.HasMass();
		}

		public bool Equals(HitInfoDetails.VoxelData _other)
		{
			return this.BlockValue.Equals(_other.BlockValue) && this.WaterValue.HasMass() == _other.WaterValue.HasMass();
		}

		public void Clear()
		{
			this.BlockValue = BlockValue.Air;
			this.WaterValue = WaterValue.Empty;
		}

		public BlockValue BlockValue;

		public WaterValue WaterValue;
	}

	public struct PropData : IEquatable<HitInfoDetails.PropData>
	{
		public PropRef PropRef
		{
			readonly get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public PropValue PropValue
		{
			readonly get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public static HitInfoDetails.PropData GetFrom(ChunkCluster cc, Transform transform)
		{
			PropReference propReference;
			if (!transform || !transform.TryGetComponent<PropReference>(ref propReference))
			{
				return HitInfoDetails.PropData.Empty;
			}
			PropRef propRef = propReference.PropRef;
			PropValue prop = cc.GetProp(propRef);
			if (prop.IsAir)
			{
				return HitInfoDetails.PropData.Empty;
			}
			return new HitInfoDetails.PropData
			{
				PropRef = propRef,
				PropValue = prop
			};
		}

		public override string ToString()
		{
			return string.Format("PropRef=[{0}], PropValue=[{1}]", this.PropRef, this.PropValue);
		}

		public void Clear()
		{
			this.PropRef = PropRef.Default;
			this.PropValue = HitInfoDetails.PropData.Empty.PropValue;
		}

		public bool Equals(HitInfoDetails.PropData other)
		{
			return this.PropRef.Equals(other.PropRef) && this.PropValue.Equals(other.PropValue);
		}

		public override bool Equals(object obj)
		{
			if (obj is HitInfoDetails.PropData)
			{
				HitInfoDetails.PropData propData = (HitInfoDetails.PropData)obj;
				return this.Equals(propData);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine<PropRef, PropValue>(this.PropRef, this.PropValue);
		}

		public static bool operator ==(HitInfoDetails.PropData left, HitInfoDetails.PropData right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(HitInfoDetails.PropData left, HitInfoDetails.PropData right)
		{
			return !left.Equals(right);
		}

		public static readonly HitInfoDetails.PropData Empty = new HitInfoDetails.PropData
		{
			PropRef = PropRef.Default,
			PropValue = PropValue.AIR
		};
	}
}
