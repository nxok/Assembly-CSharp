using System;

public class Lighting3DArray : Array3DWithOffset<Lighting>
{
	public Lighting3DArray()
		: base(3, 3, 3)
	{
	}

	public void SetBlockCache(INeighborBlockCache _nBlocks)
	{
		this.nBlocks = _nBlocks;
	}

	public void SetPosition(Vector3i _blockPos)
	{
		this.stab = this.nBlocks.GetChunk(0, 0).GetStability(_blockPos.x, _blockPos.y, _blockPos.z);
		if (this.blockPos.x != _blockPos.x || this.blockPos.z != _blockPos.z || this.blockPos.y != _blockPos.y + 1)
		{
			this.available = 0;
		}
		else
		{
			int num = 26;
			for (int i = 0; i < 18; i++)
			{
				this.data[num] = this.data[num - 9];
				num--;
			}
			this.available <<= 9;
		}
		this.blockPos = _blockPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Lighting GetLight(IChunk _c, int _x, int _y, int _z)
	{
		if (_c == null)
		{
			return Lighting.one;
		}
		_x &= 15;
		_z &= 15;
		return new Lighting(_c.GetLight(_x, _y, _z, Chunk.LIGHT_TYPE.SUN), 0, this.stab);
	}

	public override Lighting this[int _x, int _y, int _z]
	{
		get
		{
			int index = base.GetIndex(_x, _y, _z);
			int num = 1 << index;
			if ((this.available & num) == 0)
			{
				this.data[index] = this.GetLight(this.nBlocks.GetChunk(_x, _z), this.blockPos.x + _x, this.blockPos.y + _y, this.blockPos.z + _z);
				this.available |= num;
			}
			return this.data[index];
		}
		set
		{
			int index = base.GetIndex(_x, _y, _z);
			this.data[index] = value;
			this.available |= 1 << index;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSize = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSize2d = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSize3d = 27;

	[PublicizedFrom(EAccessModifier.Private)]
	public int available;

	[PublicizedFrom(EAccessModifier.Private)]
	public INeighborBlockCache nBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte stab;
}
