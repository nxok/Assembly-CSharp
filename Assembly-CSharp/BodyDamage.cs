using System;
using System.IO;
using System.Runtime.CompilerServices;

public struct BodyDamage
{
	public bool HasLeftLeg
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (this.Flags & 96U) == 0U;
		}
	}

	public bool HasRightLeg
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (this.Flags & 384U) == 0U;
		}
	}

	public bool HasLimbs
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (this.Flags & 330U) != 330U;
		}
	}

	public bool IsAnyLegMissing
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (this.Flags & 480U) > 0U;
		}
	}

	public bool IsAnyArmOrLegMissing
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (this.Flags & 510U) > 0U;
		}
	}

	public bool IsCrippled
	{
		get
		{
			return (this.Flags & 12288U) > 0U;
		}
	}

	public static BodyDamage Read(BinaryReader _br, int _version)
	{
		if (_version > 21)
		{
			return BodyDamage.ReadData(_br, _br.ReadInt32());
		}
		if (_version > 20)
		{
			return BodyDamage.ReadData(_br, 0);
		}
		if (_version > 19)
		{
			_br.ReadInt32();
		}
		return default(BodyDamage);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BodyDamage ReadData(BinaryReader br, int version)
	{
		BodyDamage bodyDamage = default(BodyDamage);
		if (version >= 4)
		{
			bodyDamage.damageType = (EnumDamageTypes)br.ReadInt32();
		}
		if (version >= 3)
		{
			bodyDamage.Flags = br.ReadUInt32();
		}
		else
		{
			br.ReadInt16();
			br.ReadInt16();
			br.ReadInt16();
			br.ReadInt16();
			br.ReadInt16();
			br.ReadInt16();
			if (br.ReadBoolean())
			{
				bodyDamage.Flags |= 2U;
			}
			if (br.ReadBoolean())
			{
				bodyDamage.Flags |= 8U;
			}
			if (br.ReadBoolean())
			{
				bodyDamage.Flags |= 1U;
			}
			if (br.ReadBoolean())
			{
				bodyDamage.Flags |= 128U;
			}
			if (br.ReadBoolean())
			{
				bodyDamage.Flags |= 8192U;
			}
			if (version >= 1)
			{
				br.ReadInt16();
				br.ReadInt16();
				br.ReadInt16();
				br.ReadInt16();
				if (br.ReadBoolean())
				{
					bodyDamage.Flags |= 4U;
				}
				if (br.ReadBoolean())
				{
					bodyDamage.Flags |= 16U;
				}
				if (br.ReadBoolean())
				{
					bodyDamage.Flags |= 64U;
				}
				if (br.ReadBoolean())
				{
					bodyDamage.Flags |= 256U;
				}
				if (version >= 2 && br.ReadBoolean())
				{
					bodyDamage.Flags |= 32U;
				}
				if (br.ReadBoolean())
				{
					bodyDamage.Flags |= 4096U;
				}
			}
		}
		bodyDamage.ShouldBeCrawler = !bodyDamage.HasLeftLeg || !bodyDamage.HasRightLeg;
		return bodyDamage;
	}

	public void Write(BinaryWriter bw)
	{
		bw.Write(BodyDamage.cBinaryVersion);
		bw.Write((int)this.damageType);
		bw.Write(this.Flags);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int cBinaryVersion = 4;

	public int StunKnee;

	public int StunProne;

	public float StunDuration;

	public EnumEntityStunType CurrentStun;

	public bool ShouldBeCrawler;

	public const uint cNoHead = 1U;

	public const uint cNoArmLUpper = 2U;

	public const uint cNoArmLLower = 4U;

	public const uint cNoArmRUpper = 8U;

	public const uint cNoArmRLower = 16U;

	public const uint cNoArm = 30U;

	public const uint cNoLegLUpper = 32U;

	public const uint cNoLegLLower = 64U;

	public const uint cNoLegRUpper = 128U;

	public const uint cNoLegRLower = 256U;

	public const uint cNoLeg = 480U;

	public const uint cCrippledLegL = 4096U;

	public const uint cCrippledLegR = 8192U;

	public uint Flags;

	public EnumDamageTypes damageType;

	public EnumBodyPartHit bodyPartHit;
}
