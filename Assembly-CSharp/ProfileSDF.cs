using System;
using System.IO;
using Platform;
using SDF;
using UnityEngine;

public static class ProfileSDF
{
	public static void InitializeStatics()
	{
		string text = Path.Combine(GameIO.GetSaveGameRootDir(), "sdcs_profiles.sdf");
		ProfileSDF.profileSDF = new SdfFile(text);
		if (SdFile.Exists(text))
		{
			ProfileSDF.profileSDF.Load();
			return;
		}
		string text2 = Path.Combine(GameIO.GetSaveGameRootDir(UserDataStorageType.DeviceLocal), "sdcs_profiles.sdf");
		if (SdFile.Exists(text2))
		{
			Log.Out("ProfileSDF not found at " + text + ". Migrating from " + text2);
			SdfFile sdfFile = new SdfFile(text2);
			sdfFile.Load();
			ProfileSDF.profileSDF.CopyFrom(sdfFile);
		}
	}

	public static string CurrentProfileName()
	{
		return ProfileSDF.profileSDF.GetString("selectedProfile") ?? "";
	}

	public static void Save()
	{
		ProfileSDF.profileSDF.Save();
	}

	public static bool ProfileExists(string _profileName)
	{
		string @string = ProfileSDF.profileSDF.GetString("profileNames");
		if (@string == null)
		{
			return false;
		}
		if (!@string.Contains(","))
		{
			return string.Equals(@string, _profileName, StringComparison.OrdinalIgnoreCase);
		}
		string[] array = @string.Split(',', StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			if (string.Equals(array[i], _profileName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public static void DeleteProfile(string _profileName)
	{
		if (!ProfileSDF.ProfileExists(_profileName))
		{
			return;
		}
		foreach (string text in ProfileSDF.profileSDF.GetKeys())
		{
			if (text.StartsWith(_profileName + "."))
			{
				ProfileSDF.profileSDF.Remove(text);
				ProfileSDF.Save();
			}
		}
		ProfileSDF.removeProfileName(_profileName);
		ProfileSDF.Save();
	}

	public static void SaveArchetype(string _archetype, bool _isMale)
	{
		if (!ProfileSDF.ProfileExists(_archetype))
		{
			ProfileSDF.addProfileName(_archetype);
			ProfileSDF.setSex(_archetype, _isMale);
			ProfileSDF.setArchetype(_archetype, _archetype);
		}
		ProfileSDF.Save();
	}

	public static void SaveProfile(string _profileName, string _archetype, bool _isMale, string _raceName, int _variantNumber, string _eyeColorName, string _hairName, string _hairColor, string _mustacheName, string _chopsName, string _beardName)
	{
		if (!ProfileSDF.ProfileExists(_profileName))
		{
			ProfileSDF.addProfileName(_profileName);
		}
		if (_archetype == "")
		{
			if (_isMale)
			{
				_archetype = "BaseMale";
			}
			else
			{
				_archetype = "BaseFemale";
			}
		}
		ProfileSDF.setSex(_profileName, _isMale);
		ProfileSDF.setRace(_profileName, _raceName);
		ProfileSDF.setArchetype(_profileName, _archetype);
		ProfileSDF.setVariant(_profileName, _variantNumber);
		ProfileSDF.setEyeColor(_profileName, _eyeColorName);
		ProfileSDF.setHairName(_profileName, _hairName);
		ProfileSDF.setHairColor(_profileName, _hairColor);
		ProfileSDF.setMustacheName(_profileName, _mustacheName);
		ProfileSDF.setChopsName(_profileName, _chopsName);
		ProfileSDF.setBeardName(_profileName, _beardName);
		ProfileSDF.SetSelectedProfile(_profileName);
		ProfileSDF.Save();
	}

	public static Archetype CreateTempArchetype(string _profileName)
	{
		Archetype archetype = new Archetype(_profileName, ProfileSDF.GetIsMale(_profileName), true);
		archetype.Race = ProfileSDF.GetRaceName(_profileName);
		archetype.Variant = ProfileSDF.GetVariantNumber(_profileName);
		archetype.Hair = ProfileSDF.GetHairName(_profileName);
		archetype.HairColor = ProfileSDF.GetHairColorName(_profileName);
		archetype.MustacheName = ProfileSDF.GetMustacheName(_profileName);
		archetype.ChopsName = ProfileSDF.GetChopsName(_profileName);
		archetype.BeardName = ProfileSDF.GetBeardName(_profileName);
		archetype.EyeColorName = ProfileSDF.GetEyeColorName(_profileName);
		archetype.AddEquipmentSlot(new SDCSUtils.SlotData
		{
			PartName = "body",
			PrefabName = "@:Entities/Player/{sex}/Gear/LumberJack/gear{sex}LumberJackPrefab.prefab",
			BaseToTurnOff = "body"
		});
		archetype.AddEquipmentSlot(new SDCSUtils.SlotData
		{
			PartName = "hands",
			PrefabName = "@:Entities/Player/{sex}/Gear/LumberJack/gear{sex}LumberJackPrefab.prefab",
			BaseToTurnOff = "hands"
		});
		archetype.AddEquipmentSlot(new SDCSUtils.SlotData
		{
			PartName = "feet",
			PrefabName = "@:Entities/Player/{sex}/Gear/LumberJack/gear{sex}LumberJackPrefab.prefab",
			BaseToTurnOff = "feet"
		});
		return archetype;
	}

	public static void SetSelectedProfile(string _profileName)
	{
		ProfileSDF.profileSDF.Set("selectedProfile", _profileName);
	}

	public static void addProfileName(string _profileName)
	{
		string @string = ProfileSDF.profileSDF.GetString("profileNames");
		if (@string == null || @string.Length == 0)
		{
			ProfileSDF.profileSDF.Set("profileNames", _profileName, false);
			return;
		}
		ProfileSDF.profileSDF.Set("profileNames", ProfileSDF.profileSDF.GetString("profileNames") + "," + _profileName, false);
	}

	public static void removeProfileName(string _profileName)
	{
		string @string = ProfileSDF.profileSDF.GetString("profileNames");
		if (@string == null || @string.Length == 0)
		{
			ProfileSDF.profileSDF.Set("profileNames", "", false);
			return;
		}
		string text = "";
		bool flag = true;
		foreach (string text2 in @string.Split(',', StringSplitOptions.None))
		{
			if (text2 != _profileName)
			{
				if (!flag)
				{
					text += ",";
				}
				text += text2;
				flag = false;
			}
		}
		ProfileSDF.profileSDF.Set("profileNames", text, false);
	}

	public static void setSex(string _profileName, bool _isMale)
	{
		ProfileSDF.profileSDF.Set(_profileName + ".isMale", _isMale);
	}

	public static void setRace(string _profileName, string _raceName)
	{
		ProfileSDF.profileSDF.Set(_profileName + ".race", _raceName);
	}

	public static void setVariant(string _profileName, int _variantNumber)
	{
		ProfileSDF.profileSDF.Set(_profileName + ".variant", _variantNumber);
	}

	public static void setColor(string _profileName, string _colorName, Color _color)
	{
		ProfileSDF.profileSDF.Set(_profileName + "." + _colorName + ".r", _color.r);
		ProfileSDF.profileSDF.Set(_profileName + "." + _colorName + ".g", _color.g);
		ProfileSDF.profileSDF.Set(_profileName + "." + _colorName + ".b", _color.b);
		ProfileSDF.profileSDF.Set(_profileName + "." + _colorName + ".a", _color.a);
	}

	public static void setEyeColor(string _profileName, string _eyeColorName)
	{
		ProfileSDF.profileSDF.Set(_profileName + ".eyeColor", _eyeColorName);
	}

	public static void setHairName(string _profileName, string _hairName)
	{
		ProfileSDF.profileSDF.Set(_profileName + ".hairName", _hairName);
	}

	public static void setHairColor(string _profileName, string _hairColorName)
	{
		ProfileSDF.profileSDF.Set(_profileName + ".hairColor", _hairColorName);
	}

	public static void setMustacheName(string _profileName, string _mustacheName)
	{
		ProfileSDF.profileSDF.Set(_profileName + ".mustacheName", _mustacheName);
	}

	public static void setChopsName(string _profileName, string _chopsName)
	{
		ProfileSDF.profileSDF.Set(_profileName + ".chopsName", _chopsName);
	}

	public static void setBeardName(string _profileName, string _beardName)
	{
		ProfileSDF.profileSDF.Set(_profileName + ".beardName", _beardName);
	}

	public static void setEyebrowName(string _profileName, string _eyebrowName)
	{
		ProfileSDF.profileSDF.Set(_profileName + ".eyebrowName", _eyebrowName);
	}

	public static void setArchetype(string _profileName, string _archetype)
	{
		ProfileSDF.profileSDF.Set(_profileName + ".archetype", _archetype);
	}

	public static Color GetSkinColor(string _profileName)
	{
		float num = ProfileSDF.profileSDF.GetFloat(_profileName + ".skin.r").GetValueOrDefault();
		float num2 = ProfileSDF.profileSDF.GetFloat(_profileName + ".skin.g").GetValueOrDefault();
		float num3 = ProfileSDF.profileSDF.GetFloat(_profileName + ".skin.b").GetValueOrDefault();
		float num4 = ProfileSDF.profileSDF.GetFloat(_profileName + ".skin.a").GetValueOrDefault();
		return new Color(num, num2, num3, num4);
	}

	public static Color GetEyebrowColor(string _profileName)
	{
		float num = ProfileSDF.profileSDF.GetFloat(_profileName + ".eyebrow.r").GetValueOrDefault();
		float num2 = ProfileSDF.profileSDF.GetFloat(_profileName + ".eyebrow.g").GetValueOrDefault();
		float num3 = ProfileSDF.profileSDF.GetFloat(_profileName + ".eyebrow.b").GetValueOrDefault();
		float num4 = ProfileSDF.profileSDF.GetFloat(_profileName + ".eyebrow.a").GetValueOrDefault();
		return new Color(num, num2, num3, num4);
	}

	public static bool GetIsMale(string _profileName)
	{
		return ProfileSDF.profileSDF.GetBool(_profileName + ".isMale").GetValueOrDefault();
	}

	public static string GetRaceName(string _profileName)
	{
		string @string = ProfileSDF.profileSDF.GetString(_profileName + ".race");
		if (@string == null)
		{
			return "White";
		}
		return @string;
	}

	public static int GetVariantNumber(string _profileName)
	{
		return ProfileSDF.profileSDF.GetInt(_profileName + ".variant").GetValueOrDefault();
	}

	public static string GetArchetype(string _profileName)
	{
		string @string = ProfileSDF.profileSDF.GetString(_profileName + ".archetype");
		if (@string != null)
		{
			return @string;
		}
		if (!ProfileSDF.GetIsMale(_profileName))
		{
			return "BaseFemale";
		}
		return "BaseMale";
	}

	public static float GetBodyDna(string _profileName, string _bodyPartName)
	{
		return Mathf.Clamp01(ProfileSDF.profileSDF.GetFloat(_profileName + ".bodyData." + _bodyPartName).GetValueOrDefault());
	}

	public static string GetEyeColorName(string _profileName)
	{
		return ProfileSDF.profileSDF.GetString(_profileName + ".eyeColor");
	}

	public static string GetHairName(string _profileName)
	{
		return ProfileSDF.profileSDF.GetString(_profileName + ".hairName");
	}

	public static string GetHairColorName(string _profileName)
	{
		return ProfileSDF.profileSDF.GetString(_profileName + ".hairColor");
	}

	public static string GetMustacheName(string _profileName)
	{
		return ProfileSDF.profileSDF.GetString(_profileName + ".mustacheName");
	}

	public static string GetChopsName(string _profileName)
	{
		return ProfileSDF.profileSDF.GetString(_profileName + ".chopsName");
	}

	public static string GetBeardName(string _profileName)
	{
		return ProfileSDF.profileSDF.GetString(_profileName + ".beardName");
	}

	public static string GetEyebrowName(string _profileName)
	{
		return ProfileSDF.profileSDF.GetString(_profileName + ".eyebrowName");
	}

	public static string[] GetProfiles()
	{
		string @string = ProfileSDF.profileSDF.GetString("profileNames");
		if (@string == null)
		{
			return new string[0];
		}
		if (@string.Contains(","))
		{
			return ProfileSDF.profileSDF.GetString("profileNames").Split(',', StringSplitOptions.None);
		}
		return new string[] { @string };
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SdfFile profileSDF;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PROFILE_NAMES = "profileNames";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PROFILE_FILENAME = "sdcs_profiles.sdf";
}
