using System;
using System.IO;

public sealed class BlockChangeInfo
{
	public BlockChangeInfo()
	{
		this.blockValueRef = BlockValueRef.None;
		this.blockValue = BlockValue.Air;
		this.density = MarchingCubes.DensityAir;
		this.changedByEntityId = -1;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue)
	{
		this.blockValueRef = _bvRef;
		this.blockValue = _blockValue;
		this.bChangeBlockValue = true;
		this.bUpdateLight = false;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, bool _updateLight, bool _bOnlyDamage = false)
		: this(_bvRef, _blockValue, _updateLight)
	{
		this.bChangeDamage = _bOnlyDamage;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, int _changedEntityId)
		: this(_bvRef, _blockValue)
	{
		this.changedByEntityId = _changedEntityId;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, bool _updateLight)
		: this(_bvRef, _blockValue)
	{
		this.bUpdateLight = _updateLight;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, bool _updateLight, int _changingEntityId)
		: this(_bvRef, _blockValue, _updateLight)
	{
		this.changedByEntityId = _changingEntityId;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, sbyte _density, bool _bForceDensityChange = false)
	{
		this.blockValueRef = _bvRef;
		this.density = _density;
		this.bChangeDensity = true;
		this.bForceDensity = _bForceDensityChange;
		this.changedByEntityId = -1;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, sbyte _density)
	{
		this.blockValueRef = _bvRef;
		this.blockValue = _blockValue;
		this.bChangeBlockValue = true;
		this.density = _density;
		this.bChangeDensity = true;
		this.bUpdateLight = true;
		this.changedByEntityId = -1;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, sbyte _density, int _changedByEntityId)
		: this(_bvRef, _blockValue, _density)
	{
		this.changedByEntityId = _changedByEntityId;
	}

	public BlockChangeInfo(BlockValueRef _bvRef, BlockValue _blockValue, sbyte _density, TextureFullArray _tex)
		: this(_bvRef, _blockValue, _density)
	{
		this.bChangeTexture = true;
		this.textureFull = _tex;
		this.changedByEntityId = -1;
	}

	public void Read(BinaryReader _br)
	{
		this.blockValueRef = BlockValueRef.Read(_br);
		this.changedByEntityId = _br.ReadInt32();
		BlockChangeInfo.Flags flags = (BlockChangeInfo.Flags)_br.ReadByte();
		this.bChangeBlockValue = (flags & BlockChangeInfo.Flags.ChangeBlockValue) > (BlockChangeInfo.Flags)0;
		this.bChangeDensity = (flags & BlockChangeInfo.Flags.ChangeDensity) > (BlockChangeInfo.Flags)0;
		this.bForceDensity = (flags & BlockChangeInfo.Flags.ForceDensity) > (BlockChangeInfo.Flags)0;
		this.bUpdateLight = (flags & BlockChangeInfo.Flags.UpdateLight) > (BlockChangeInfo.Flags)0;
		this.bChangeDamage = (flags & BlockChangeInfo.Flags.ChangeDamage) > (BlockChangeInfo.Flags)0;
		this.bChangeTexture = (flags & BlockChangeInfo.Flags.ChangeTexture) > (BlockChangeInfo.Flags)0;
		if (this.bChangeBlockValue)
		{
			this.blockValue = BlockValue.Read(_br);
		}
		if (this.bChangeDensity)
		{
			this.density = _br.ReadSByte();
		}
		if (this.bChangeTexture)
		{
			this.textureFull.Read(_br, 1);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		this.blockValueRef.Write(_bw);
		_bw.Write(this.changedByEntityId);
		BlockChangeInfo.Flags flags = (BlockChangeInfo.Flags)0;
		flags |= (this.bChangeBlockValue ? BlockChangeInfo.Flags.ChangeBlockValue : ((BlockChangeInfo.Flags)0));
		flags |= (this.bChangeDensity ? BlockChangeInfo.Flags.ChangeDensity : ((BlockChangeInfo.Flags)0));
		flags |= (this.bForceDensity ? BlockChangeInfo.Flags.ForceDensity : ((BlockChangeInfo.Flags)0));
		flags |= (this.bUpdateLight ? BlockChangeInfo.Flags.UpdateLight : ((BlockChangeInfo.Flags)0));
		flags |= (this.bChangeDamage ? BlockChangeInfo.Flags.ChangeDamage : ((BlockChangeInfo.Flags)0));
		flags |= (this.bChangeTexture ? BlockChangeInfo.Flags.ChangeTexture : ((BlockChangeInfo.Flags)0));
		_bw.Write((byte)flags);
		if (this.bChangeBlockValue)
		{
			this.blockValue.Write(_bw);
		}
		if (this.bChangeDensity)
		{
			_bw.Write(this.density);
		}
		if (this.bChangeTexture)
		{
			this.textureFull.Write(_bw);
		}
	}

	public static BlockChangeInfo Empty = new BlockChangeInfo
	{
		blockValueRef = BlockValueRef.None
	};

	public BlockValueRef blockValueRef;

	public bool bChangeBlockValue;

	public bool bChangeDamage;

	public BlockValue blockValue;

	public bool bChangeDensity;

	public bool bForceDensity;

	public sbyte density;

	public bool bUpdateLight;

	public bool bChangeTexture;

	public TextureFullArray textureFull;

	public int changedByEntityId = -1;

	[Flags]
	[PublicizedFrom(EAccessModifier.Private)]
	public enum Flags : byte
	{
		ChangeBlockValue = 1,
		ChangeDamage = 2,
		ChangeDensity = 4,
		ForceDensity = 8,
		UpdateLight = 16,
		ChangeTexture = 32
	}
}
