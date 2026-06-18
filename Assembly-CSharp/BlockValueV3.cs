using System;
using System.Runtime.CompilerServices;

public struct BlockValueV3
{
	public static uint ConvertOldRawData(uint _rawData)
	{
		BlockValueV3.convertBV3.rawData = _rawData;
		int type = BlockValueV3.convertBV3.type;
		BlockValueV3.convertBV.type = type;
		if (!BlockValueV3.convertBV3.ischild)
		{
			BlockValueV3.convertBV.rotation = BlockValueV3.convertBV3.rotation;
			BlockValueV3.convertBV.meta = BlockValueV3.convertBV3.meta;
			BlockValueV3.convertBV.meta2 = BlockValueV3.convertBV3.meta2;
		}
		else
		{
			BlockValueV3.convertBV.parent = BlockValueV3.convertBV3.parent;
			BlockValueV3.convertBV.ischild = true;
		}
		BlockValueV3.convertBV.hasdecal = BlockValueV3.convertBV3.hasdecal;
		return BlockValueV3.convertBV.rawData;
	}

	public BlockValueV3(uint _rawData)
	{
		this.rawData = _rawData;
	}

	public Block Block
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Block.list[this.type];
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint GetTypeMasked(uint _v)
	{
		return _v & 32767U;
	}

	public bool isair
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.type == 0;
		}
	}

	public bool isWater
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.type == 240 || this.type == 241 || this.type == 242;
		}
	}

	public int type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (int)(this.rawData & 32767U);
		}
		set
		{
			this.rawData = (this.rawData & 4294934528U) | (uint)((long)value & 32767L);
		}
	}

	public byte rotation
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((this.rawData & 1015808U) >> 15);
		}
		set
		{
			this.rawData = (this.rawData & 4293951487U) | (uint)((uint)(value & 31) << 15);
		}
	}

	public byte meta
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((this.rawData & 15728640U) >> 20);
		}
		set
		{
			this.rawData = (this.rawData & 4279238655U) | (uint)((uint)(value & 15) << 20);
		}
	}

	public byte meta2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((this.rawData & 251658240U) >> 24);
		}
		set
		{
			this.rawData = (this.rawData & 4043309055U) | (uint)((uint)(value & 15) << 24);
		}
	}

	public byte meta3
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (byte)((this.rawData & 805306368U) >> 28);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.rawData = (this.rawData & 3489660927U) | (uint)((uint)(value & 3) << 28);
		}
	}

	public byte meta2and1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)(((int)this.meta2 << 4) | (int)this.meta);
		}
		set
		{
			this.meta2 = (byte)((value >> 4) & 15);
			this.meta = value & 15;
		}
	}

	public byte rotationAndMeta3
	{
		[PublicizedFrom(EAccessModifier.Private)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)(((int)this.rotation << 2) | (int)this.meta3);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.rotation = (byte)((long)(value >> 2) & 31L);
			this.meta3 = value;
		}
	}

	public bool hasdecal
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (this.rawData & 2147483648U) > 0U;
		}
		set
		{
			this.rawData = (this.rawData & 2147483647U) | (value ? 2147483648U : 0U);
		}
	}

	public BlockFaceFlag rotatedWaterFlowMask
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return BlockFaceFlags.RotateFlags(this.Block.WaterFlowMask, this.rotation);
		}
	}

	public BlockFace decalface
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (BlockFace)((this.rawData & 15728640U) >> 20);
		}
		set
		{
			this.rawData = (this.rawData & 4279238655U) | (uint)((uint)value << 20);
		}
	}

	public byte decaltex
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((this.rawData & 251658240U) >> 24);
		}
		set
		{
			this.rawData = (this.rawData & 4043309055U) | (uint)((uint)value << 24);
		}
	}

	public bool ischild
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (this.rawData & 1073741824U) > 0U;
		}
		set
		{
			this.rawData = (this.rawData & 3221225471U) | (value ? 1073741824U : 0U);
		}
	}

	public int parentx
	{
		get
		{
			int num = (int)((this.rawData & 251658240U) >> 24);
			return ((num & 8) != 0) ? (-(num & 7)) : (num & 7);
		}
		set
		{
			int num = ((value < 0) ? (8 | (-value & 7)) : (value & 7));
			this.rawData = (this.rawData & 4043309055U) | (uint)((uint)num << 24);
		}
	}

	public int parenty
	{
		get
		{
			int rotationAndMeta = (int)this.rotationAndMeta3;
			return ((rotationAndMeta & 32) != 0) ? (-(rotationAndMeta & 31)) : (rotationAndMeta & 31);
		}
		set
		{
			int num = ((value < 0) ? (32 | (-value & 31)) : (value & 31));
			this.rotationAndMeta3 = (byte)num;
		}
	}

	public int parentz
	{
		get
		{
			int num = (int)((this.rawData & 15728640U) >> 20);
			return ((num & 8) != 0) ? (-(num & 7)) : (num & 7);
		}
		set
		{
			int num = ((value < 0) ? (8 | (-value & 7)) : (value & 7));
			this.rawData = (this.rawData & 4279238655U) | (uint)((uint)num << 20);
		}
	}

	public Vector3i parent
	{
		get
		{
			return new Vector3i(this.parentx, this.parenty, this.parentz);
		}
		set
		{
			this.parentx = value.x;
			this.parenty = value.y;
			this.parentz = value.z;
		}
	}

	public const uint TypeMask = 32767U;

	public const uint RotationMax = 31U;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint RotationMask = 1015808U;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int RotationShift = 15;

	public const uint MetadataMax = 15U;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint MetadataMask = 15728640U;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MetadataShift = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint Metadata2Mask = 251658240U;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Metadata2Shift = 24;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint Metadata3Max = 3U;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint Metadata3Mask = 805306368U;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Metadata3Shift = 28;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint ChildMask = 1073741824U;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ChildShift = 30;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint HasDecalMask = 2147483648U;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int HasDecalShift = 31;

	public uint rawData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockValueV3 convertBV3;

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockValue convertBV;
}
