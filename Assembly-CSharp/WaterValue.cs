using System;
using System.IO;
using System.Runtime.CompilerServices;

public struct WaterValue
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool HasMass()
	{
		return this.mass > 195;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetMass()
	{
		return (int)this.mass;
	}

	public float GetMassPercent()
	{
		if (this.mass <= 195)
		{
			return 0f;
		}
		if (this.mass >= 15600)
		{
			return 1f;
		}
		return (float)(this.mass - 195) / 15405f;
	}

	public void SetMass(int value)
	{
		this.mass = (ushort)Utils.FastClamp(value, 0, 65535);
	}

	public override string ToString()
	{
		return string.Format("Raw Mass: {0:d}", this.mass);
	}

	public long RawData
	{
		get
		{
			return (long)((ulong)this.mass);
		}
	}

	public static WaterValue FromRawData(long rawData)
	{
		return new WaterValue
		{
			mass = (ushort)rawData
		};
	}

	public static WaterValue FromBlockType(int type)
	{
		if (type == 240 || type == 241 || type == 242)
		{
			return new WaterValue(19500);
		}
		return WaterValue.Empty;
	}

	public static WaterValue FromStream(BinaryReader _reader)
	{
		WaterValue waterValue = default(WaterValue);
		waterValue.Read(_reader);
		return waterValue;
	}

	public WaterValue(BlockValue _bv)
	{
		this.mass = (_bv.isWater ? 19500 : 0);
	}

	public WaterValue(int mass)
	{
		this.mass = (ushort)Utils.FastClamp(mass, 0, 65535);
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(this.mass);
	}

	public void Read(BinaryReader reader)
	{
		this.mass = reader.ReadUInt16();
	}

	public static int SerializedLength()
	{
		return 2;
	}

	public const float cTopPerCap = 0.6f;

	public const int MAX_MASS_VALUE = 65535;

	public static readonly WaterValue Empty = new WaterValue(0);

	public static readonly WaterValue Full = new WaterValue(19500);

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort mass;
}
