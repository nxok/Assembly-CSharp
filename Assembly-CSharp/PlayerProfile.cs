using System;
using System.IO;

public class PlayerProfile
{
	public bool IsMale
	{
		get
		{
			return this.isMale;
		}
		set
		{
			this.isMale = value;
		}
	}

	public string RaceName
	{
		get
		{
			return this.raceName;
		}
		set
		{
			this.raceName = value;
		}
	}

	public string EyeColor
	{
		get
		{
			return this.eyeColor;
		}
		set
		{
			this.eyeColor = value;
		}
	}

	public string HairName
	{
		get
		{
			return this.hairName;
		}
		set
		{
			this.hairName = value;
		}
	}

	public string HairColor
	{
		get
		{
			return this.hairColor;
		}
		set
		{
			this.hairColor = value;
		}
	}

	public string MustacheName
	{
		get
		{
			return this.mustacheName;
		}
		set
		{
			this.mustacheName = value;
		}
	}

	public string ChopsName
	{
		get
		{
			return this.chopsName;
		}
		set
		{
			this.chopsName = value;
		}
	}

	public string BeardName
	{
		get
		{
			return this.beardName;
		}
		set
		{
			this.beardName = value;
		}
	}

	public int VariantNumber
	{
		get
		{
			return this.variantNumber;
		}
		set
		{
			this.variantNumber = value;
		}
	}

	public string ProfileArchetype
	{
		get
		{
			if (this.archetype == null || this.archetype == string.Empty)
			{
				if (this.isMale)
				{
					this.archetype = "BaseMale";
				}
				else
				{
					this.archetype = "BaseFemale";
				}
			}
			return this.archetype;
		}
		set
		{
			this.archetype = value;
		}
	}

	public Archetype CreateTempArchetype()
	{
		if (this.archetype != "BaseMale" && this.archetype != "BaseFemale")
		{
			return Archetype.GetArchetype(this.archetype);
		}
		return new Archetype(this.archetype, this.isMale, true)
		{
			Race = this.raceName,
			Variant = this.variantNumber,
			Hair = this.hairName,
			HairColor = this.hairColor,
			MustacheName = this.mustacheName,
			ChopsName = this.chopsName,
			BeardName = this.beardName,
			EyeColorName = this.EyeColor
		};
	}

	public string EntityClassName
	{
		get
		{
			if (!this.isMale)
			{
				return "playerFemale";
			}
			return "playerMale";
		}
	}

	public PlayerProfile()
	{
		this.raceName = "white";
		this.isMale = true;
		this.variantNumber = 1;
	}

	public PlayerProfile Clone()
	{
		return new PlayerProfile
		{
			raceName = this.raceName,
			isMale = this.isMale,
			variantNumber = this.variantNumber,
			archetype = this.archetype,
			hairName = this.hairName,
			hairColor = this.hairColor,
			mustacheName = this.mustacheName,
			chopsName = this.chopsName,
			beardName = this.beardName,
			EyeColor = this.EyeColor
		};
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(5);
		writer.Write(this.archetype);
		writer.Write(this.isMale);
		writer.Write(this.raceName);
		writer.Write((byte)this.variantNumber);
		writer.Write(this.hairName ?? "");
		writer.Write(this.hairColor ?? "");
		writer.Write(this.mustacheName ?? "");
		writer.Write(this.chopsName ?? "");
		writer.Write(this.beardName ?? "");
		writer.Write(this.eyeColor ?? "Blue01");
	}

	public static PlayerProfile Read(BinaryReader reader)
	{
		PlayerProfile playerProfile = new PlayerProfile();
		int num = reader.ReadInt32();
		playerProfile.archetype = reader.ReadString();
		playerProfile.IsMale = reader.ReadBoolean();
		playerProfile.RaceName = reader.ReadString();
		playerProfile.VariantNumber = (int)reader.ReadByte();
		if (num > 1)
		{
			playerProfile.HairName = reader.ReadString();
		}
		if (num > 2)
		{
			playerProfile.HairColor = reader.ReadString();
		}
		if (num > 3)
		{
			playerProfile.MustacheName = reader.ReadString();
			playerProfile.ChopsName = reader.ReadString();
			playerProfile.BeardName = reader.ReadString();
		}
		if (num > 4)
		{
			playerProfile.EyeColor = reader.ReadString();
		}
		return playerProfile;
	}

	public static PlayerProfile LoadLocalProfile()
	{
		return PlayerProfile.LoadProfile(ProfileSDF.CurrentProfileName());
	}

	public static PlayerProfile LoadProfile(string _profileName)
	{
		return new PlayerProfile
		{
			IsMale = ProfileSDF.GetIsMale(_profileName),
			RaceName = ProfileSDF.GetRaceName(_profileName),
			VariantNumber = ProfileSDF.GetVariantNumber(_profileName),
			ProfileArchetype = ProfileSDF.GetArchetype(_profileName),
			HairName = ProfileSDF.GetHairName(_profileName),
			HairColor = ProfileSDF.GetHairColorName(_profileName),
			MustacheName = ProfileSDF.GetMustacheName(_profileName),
			ChopsName = ProfileSDF.GetChopsName(_profileName),
			BeardName = ProfileSDF.GetBeardName(_profileName),
			EyeColor = ProfileSDF.GetEyeColorName(_profileName)
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isMale = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public string raceName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int variantNumber = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string archetype = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string eyeColor = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string hairName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string hairColor = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string mustacheName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string chopsName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string beardName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int version = 5;
}
