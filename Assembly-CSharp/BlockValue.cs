using System;
using System.IO;
using System.Runtime.CompilerServices;

[Serializable]
public struct BlockValue : IEquatable<BlockValue>
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockValue(uint _rawData)
	{
		this.rawData = _rawData;
		this.damage = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockValue(uint _rawData, int _damage)
	{
		this.rawData = _rawData;
		this.damage = _damage;
	}

	public BlockValue set(int _type, byte _meta, byte _damage, byte _rotation)
	{
		this.type = _type;
		this.meta = _meta;
		this.damage = (int)_damage;
		this.rotation = _rotation;
		return this;
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
		return _v & 65535U;
	}

	public bool isair
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.type == 0;
		}
	}

	public bool isTerrain
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.type - 1 < 239;
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

	public bool validCover
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return !this.isair && !this.isWater && this.Block.MaxDamage != 1;
		}
	}

	public int type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (int)(this.rawData & 65535U);
		}
		set
		{
			this.rawData = (this.rawData & 4294901760U) | (uint)(value & 65535);
		}
	}

	public byte rotation
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((this.rawData >> 16) & 31U);
		}
		set
		{
			this.rawData = (this.rawData & 4292935679U) | (uint)((uint)(value & 31) << 16);
		}
	}

	public bool isRotated45Degrees
	{
		get
		{
			return this.rotation >= 24 && this.rotation <= 27;
		}
	}

	public byte meta
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((this.rawData >> 22) & 15U);
		}
		set
		{
			this.rawData = (this.rawData & 4232052735U) | (uint)((uint)(value & 15) << 22);
		}
	}

	public byte meta2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((this.rawData >> 26) & 15U);
		}
		set
		{
			this.rawData = (this.rawData & 3288334335U) | (uint)((uint)(value & 15) << 26);
		}
	}

	public byte meta2and1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((this.rawData >> 22) & 255U);
		}
		set
		{
			this.rawData = (this.rawData & 3225419775U) | (uint)((uint)value << 22);
		}
	}

	public byte meta3
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return (byte)((this.rawData >> 21) & 1U);
		}
	}

	public byte rotationAndMeta3
	{
		[PublicizedFrom(EAccessModifier.Private)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((this.rawData >> 16) & 63U);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.rawData = (this.rawData & 4290838527U) | (uint)((uint)(value & 63) << 16);
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
			return (BlockFace)((this.rawData >> 22) & 15U);
		}
		set
		{
			this.rawData = (this.rawData & 4232052735U) | (uint)((uint)(value & (BlockFace)15) << 22);
		}
	}

	public byte decaltex
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)((this.rawData >> 26) & 15U);
		}
		set
		{
			this.rawData = (this.rawData & 3288334335U) | (uint)((uint)(value & 15) << 26);
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (int)(this.meta - 8);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			this.meta = (byte)(value + 8);
		}
	}

	public int parenty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (int)(this.rotationAndMeta3 - 32);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			this.rotationAndMeta3 = (byte)(value + 32);
		}
	}

	public int parentz
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (int)(this.meta2 - 8);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			this.meta2 = (byte)(value + 8);
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

	public static BlockValue Read(BinaryReader br)
	{
		return new BlockValue
		{
			rawData = br.ReadUInt32(),
			damage = (int)br.ReadUInt16()
		};
	}

	public readonly void Write(BinaryWriter bw)
	{
		bw.Write(this.rawData);
		bw.Write((ushort)this.damage);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetForceToOtherBlock(BlockValue _other)
	{
		return Utils.FastMin(this.Block.blockMaterial.StabilityGlue, _other.Block.blockMaterial.StabilityGlue);
	}

	public int ToItemType()
	{
		return this.type;
	}

	public ItemValue ToItemValue()
	{
		return new ItemValue
		{
			type = this.type
		};
	}

	public override int GetHashCode()
	{
		return this.type;
	}

	public override bool Equals(object _other)
	{
		return _other is BlockValue && ((BlockValue)_other).type == this.type;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(BlockValue _other)
	{
		return _other.type == this.type;
	}

	public bool EqualsExceptRotation(BlockValue _other)
	{
		uint num = this.rawData & 4292935679U;
		uint num2 = _other.rawData & 4292935679U;
		return num == num2;
	}

	public override string ToString()
	{
		if (!this.ischild)
		{
			string text = "id={0} r={1} d={2} m={3} m2={4} m3={5} name={6}";
			object[] array = new object[7];
			array[0] = this.type;
			array[1] = this.rotation;
			array[2] = this.damage;
			array[3] = this.meta;
			array[4] = this.meta2;
			array[5] = this.meta3;
			int num = 6;
			Block block = this.Block;
			array[num] = ((block != null) ? block.GetBlockName() : null) ?? "-null-";
			return string.Format(text, array);
		}
		string text2 = "id={0} px={1} py={2} pz={3} name={4}";
		object[] array2 = new object[5];
		array2[0] = this.type;
		array2[1] = this.parentx;
		array2[2] = this.parenty;
		array2[3] = this.parentz;
		int num2 = 4;
		Block block2 = this.Block;
		array2[num2] = ((block2 != null) ? block2.GetBlockName() : null) ?? "-null-";
		return string.Format(text2, array2);
	}

	public const uint TypeMask = 65535U;

	public const uint RotationMax = 31U;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const uint RotationMask = 2031616U;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int RotationShift = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const uint Metadata3Max = 1U;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const uint Metadata3Mask = 2097152U;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int Metadata3Shift = 21;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const uint RotationMeta3Max = 63U;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const uint RotationMeta3Mask = 4128768U;

	public const uint MetadataMax = 15U;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const uint Metadata1Mask = 62914560U;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int Metadata1Shift = 22;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const uint Metadata2Mask = 1006632960U;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int Metadata2Shift = 26;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const uint Metadata12Max = 255U;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const uint ChildMask = 1073741824U;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int ChildShift = 30;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const uint HasDecalMask = 2147483648U;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int HasDecalShift = 31;

	public static BlockValue Air;

	public uint rawData;

	public int damage;
}
