using System;
using Unity.Mathematics;

public struct WaterVoxelState : IEquatable<WaterVoxelState>
{
	public WaterVoxelState(byte stateBits)
	{
		this.stateBits = stateBits;
	}

	public WaterVoxelState(WaterVoxelState other)
	{
		this.stateBits = other.stateBits;
	}

	public bool IsDefault()
	{
		return this.stateBits == 0;
	}

	public bool IsSolidYPos()
	{
		return (this.stateBits & 1) > 0;
	}

	public bool IsSolidYNeg()
	{
		return (this.stateBits & 2) > 0;
	}

	public bool IsSolidXPos()
	{
		return (this.stateBits & 32) > 0;
	}

	public bool IsSolidXNeg()
	{
		return (this.stateBits & 8) > 0;
	}

	public bool IsSolidZPos()
	{
		return (this.stateBits & 4) > 0;
	}

	public bool IsSolidZNeg()
	{
		return (this.stateBits & 16) > 0;
	}

	public bool IsSolidXZ(int2 side)
	{
		if (side.x > 0)
		{
			return this.IsSolidXPos();
		}
		if (side.x < 0)
		{
			return this.IsSolidXNeg();
		}
		if (side.y > 0)
		{
			return this.IsSolidZPos();
		}
		if (side.y < 0)
		{
			return this.IsSolidZNeg();
		}
		return this.IsSolid();
	}

	public bool IsSolid()
	{
		return this.stateBits != 0 && (~this.stateBits & 63) == 0;
	}

	public void SetSolid(BlockFaceFlag flags)
	{
		this.stateBits = (byte)flags;
	}

	public void SetSolidMask(BlockFaceFlag mask, bool value)
	{
		if (value)
		{
			this.stateBits |= (byte)mask;
			return;
		}
		this.stateBits &= (byte)(~(byte)mask);
	}

	public bool Equals(WaterVoxelState other)
	{
		return this.stateBits == other.stateBits;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte stateBits;
}
