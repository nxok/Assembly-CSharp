using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Platform;
using UnityEngine;

public class CharacterConfigurator : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		LoadManager.Init();
		PlatformManager.Init();
		this.PrepareHairColorArray();
		this.PrepareEyeColorArray();
		this.PopulateGearLookups();
		this.CreateCharacter();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrepareHairColorArray()
	{
		string[] files = Directory.GetFiles("Assets/AssetBundles/Player/Common/HairColorSwatches", "*.asset");
		this.hairColors = new string[files.Length];
		for (int i = 0; i < files.Length; i++)
		{
			this.hairColors[i] = Path.GetFileNameWithoutExtension(files[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrepareEyeColorArray()
	{
		string[] files = Directory.GetFiles("Assets/AssetBundles/Player/Common/Eyes/Materials", "*.mat");
		this.eyeColors = new string[files.Length];
		for (int i = 0; i < files.Length; i++)
		{
			this.eyeColors[i] = Path.GetFileNameWithoutExtension(files[i]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetItemsXmlPath()
	{
		return GameIO.GetGameDir("Data/Config") + "/items.xml";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnsureGearCatalogLoaded()
	{
		if (this.gearCatalog == null)
		{
			this.PopulateGearLookups();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PopulateGearLookups()
	{
		this.allGearKeys.Clear();
		this.gearCatalog = new CharacterConfigurator.SDCSGearXmlCatalog();
		string itemsXmlPath = this.GetItemsXmlPath();
		if (!File.Exists(itemsXmlPath))
		{
			Debug.LogWarning("CharacterConfigurator could not find items.xml at: " + itemsXmlPath);
			return;
		}
		try
		{
			string text = File.ReadAllText(itemsXmlPath);
			this.gearCatalog = CharacterConfigurator.SDCSGearXmlCatalog.BuildFromXml(text);
			this.allGearKeys = this.gearCatalog.GetOrderedKeys();
		}
		catch (Exception ex)
		{
			string text2 = "CharacterConfigurator failed to read SDCS gear data from items.xml: ";
			Exception ex2 = ex;
			Debug.LogError(text2 + ((ex2 != null) ? ex2.ToString() : null));
			this.gearCatalog = new CharacterConfigurator.SDCSGearXmlCatalog();
			this.allGearKeys.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateCharacter()
	{
		if (this.characterInstance != null)
		{
			Object.DestroyImmediate(this.characterInstance);
		}
		this.characterInstance = Object.Instantiate<GameObject>(this.baseRig, base.transform);
		this.boneCatalog = new SDCSUtils.TransformCatalog(this.characterInstance.transform);
		this.UpdateCharacter();
	}

	public void UpdateCharacter()
	{
		if (this.characterInstance == null)
		{
			return;
		}
		this.EnsureGearCatalogLoaded();
		CharacterConstructUtils.CreateViz(this.CreateCurrentArchetype(), ref this.characterInstance, ref this.boneCatalog);
		this.characterInstance.name = "Character";
		this.characterInstance.transform.localPosition = Vector3.zero;
		this.characterInstance.transform.localRotation = Quaternion.Euler(0f, this.rotationValue, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Archetype CreateCurrentArchetype()
	{
		this.EnsureGearCatalogLoaded();
		Archetype archetype = new Archetype("", this.selectedSexIndex == 0, true);
		archetype.Sex = this.sexes[this.selectedSexIndex];
		archetype.Race = this.races[this.selectedRaceIndex];
		archetype.Variant = this.variants[this.selectedVariantIndex];
		archetype.EyeColorName = this.eyeColors[this.selectedEyeColorIndex];
		if (this.selectedHairIndex >= 0 && this.selectedHairIndex < this.hairs.Length)
		{
			archetype.Hair = this.hairs[this.selectedHairIndex];
			archetype.HairColor = this.hairColors[this.selectedHairColorIndex];
		}
		else
		{
			archetype.Hair = "";
		}
		if (this.selectedSexIndex == 0 && this.selectedFacialHairIndex >= 0 && this.selectedFacialHairIndex < this.facialHairIndexes.Length)
		{
			int num = this.facialHairIndexes[this.selectedFacialHairIndex];
			archetype.MustacheName = num.ToString();
			archetype.ChopsName = num.ToString();
			archetype.BeardName = num.ToString();
		}
		if (this.IsValidGearIndex(this.selectedFeetGearIndex) || this.IsValidGearIndex(this.selectedHandsGearIndex) || this.IsValidGearIndex(this.selectedBodyGearIndex) || this.IsValidGearIndex(this.selectedHeadGearIndex))
		{
			archetype.Equipment = new List<SDCSUtils.SlotData>();
			string[] array = new string[]
			{
				this.IsValidGearIndex(this.selectedHeadGearIndex) ? this.allGearKeys[this.selectedHeadGearIndex] : string.Empty,
				this.IsValidGearIndex(this.selectedBodyGearIndex) ? this.allGearKeys[this.selectedBodyGearIndex] : string.Empty,
				this.IsValidGearIndex(this.selectedHandsGearIndex) ? this.allGearKeys[this.selectedHandsGearIndex] : string.Empty,
				this.IsValidGearIndex(this.selectedFeetGearIndex) ? this.allGearKeys[this.selectedFeetGearIndex] : string.Empty
			};
			for (int i = 0; i < CharacterConfigurator.baseParts.Length; i++)
			{
				string text = array[i];
				string text2 = CharacterConfigurator.baseParts[i];
				CharacterConfigurator.SDCSGearXmlCatalog.Entry entry;
				if (!string.IsNullOrEmpty(text) && this.gearCatalog.TryGetPart(text, text2, out entry))
				{
					SDCSUtils.SlotData slotData = new SDCSUtils.SlotData();
					slotData.PartName = entry.PartName;
					slotData.PrefabName = entry.PrefabName;
					slotData.BaseToTurnOff = entry.BaseToTurnOff;
					slotData.HairMaskType = SDCSUtils.SlotData.HairMaskTypes.Full;
					slotData.FacialHairMaskType = SDCSUtils.SlotData.HairMaskTypes.Full;
					if (slotData.PartName == "head")
					{
						slotData.HairMaskType = this.ParseHairMaskType(entry.HairMaskType);
						slotData.FacialHairMaskType = this.ParseHairMaskType(entry.FacialHairMaskType);
						if (slotData.HairMaskType == SDCSUtils.SlotData.HairMaskTypes.None)
						{
							archetype.Hair = "";
						}
						if (slotData.FacialHairMaskType == SDCSUtils.SlotData.HairMaskTypes.None)
						{
							archetype.MustacheName = "";
							archetype.ChopsName = "";
							archetype.BeardName = "";
						}
					}
					if (slotData.BaseToTurnOff == "head")
					{
						archetype.Hair = "";
						archetype.MustacheName = "";
						archetype.ChopsName = "";
						archetype.BeardName = "";
					}
					archetype.Equipment.Add(slotData);
				}
			}
		}
		return archetype;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsValidGearIndex(int gearIndex)
	{
		return gearIndex >= 0 && gearIndex < this.allGearKeys.Count;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.SlotData.HairMaskTypes ParseHairMaskType(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return SDCSUtils.SlotData.HairMaskTypes.Full;
		}
		SDCSUtils.SlotData.HairMaskTypes hairMaskTypes;
		if (Enum.TryParse<SDCSUtils.SlotData.HairMaskTypes>(value, true, out hairMaskTypes))
		{
			return hairMaskTypes;
		}
		Debug.LogWarning("Unknown SDCS HairMaskType '" + value + "'. Falling back to None.");
		return SDCSUtils.SlotData.HairMaskTypes.None;
	}

	public string[] GetSexTypes()
	{
		return this.sexes;
	}

	public string[] GetRaceTypes()
	{
		return this.races;
	}

	public string[] GetVariantTypes()
	{
		string[] array = new string[this.variants.Length];
		for (int i = 0; i < this.variants.Length; i++)
		{
			array[i] = this.variants[i].ToString();
		}
		return array;
	}

	public string[] GetHairTypes()
	{
		string[] array = new string[this.hairs.Length + 1];
		array[0] = "None";
		for (int i = 0; i < this.hairs.Length; i++)
		{
			array[i + 1] = this.hairs[i];
		}
		return array;
	}

	public string[] GetHairColors()
	{
		this.PrepareHairColorArray();
		return this.hairColors;
	}

	public string[] GetEyeColors()
	{
		this.PrepareEyeColorArray();
		return this.eyeColors;
	}

	public string[] GetGearTypes()
	{
		this.EnsureGearCatalogLoaded();
		string[] array = new string[this.allGearKeys.Count + 1];
		array[0] = "None";
		for (int i = 0; i < this.allGearKeys.Count; i++)
		{
			array[i + 1] = this.allGearKeys[i];
		}
		return array;
	}

	public string[] GetFacialHairTypes()
	{
		string[] array = new string[this.facialHairIndexes.Length + 1];
		array[0] = "None";
		for (int i = 0; i < this.facialHairIndexes.Length; i++)
		{
			array[i + 1] = "Style " + this.facialHairIndexes[i].ToString();
		}
		return array;
	}

	public void SetSex(int sexIndex)
	{
		if (sexIndex >= 0 && sexIndex < this.sexes.Length)
		{
			this.selectedSexIndex = sexIndex;
			if (sexIndex == 1)
			{
				this.selectedFacialHairIndex = -1;
			}
			this.UpdateCharacter();
		}
	}

	public void SetRace(int raceIndex)
	{
		if (raceIndex >= 0 && raceIndex < this.races.Length)
		{
			this.selectedRaceIndex = raceIndex;
			this.UpdateCharacter();
		}
	}

	public void SetVariant(int variantIndex)
	{
		if (variantIndex >= 0 && variantIndex < this.variants.Length)
		{
			this.selectedVariantIndex = variantIndex;
			this.UpdateCharacter();
		}
	}

	public void SetHair(int hairIndex)
	{
		this.selectedHairIndex = hairIndex - 1;
		this.UpdateCharacter();
	}

	public void SetHairColor(int colorIndex)
	{
		if (colorIndex >= 0 && colorIndex < this.hairColors.Length)
		{
			this.selectedHairColorIndex = colorIndex;
			this.UpdateCharacter();
		}
	}

	public void SetEyeColor(int colorIndex)
	{
		if (colorIndex >= 0 && colorIndex < this.eyeColors.Length)
		{
			this.selectedEyeColorIndex = colorIndex;
			this.UpdateCharacter();
		}
	}

	public void SetGear(int gearIndex)
	{
		this.selectedGearIndex = gearIndex - 1;
		this.selectedFeetGearIndex = (this.selectedHandsGearIndex = (this.selectedBodyGearIndex = (this.selectedHeadGearIndex = this.selectedGearIndex)));
		this.UpdateCharacter();
	}

	public void SetFeetGear(int gearIndex)
	{
		this.selectedFeetGearIndex = gearIndex - 1;
		this.UpdateCharacter();
	}

	public void SetHandsGear(int gearIndex)
	{
		this.selectedHandsGearIndex = gearIndex - 1;
		this.UpdateCharacter();
	}

	public void SetBodyGear(int gearIndex)
	{
		this.selectedBodyGearIndex = gearIndex - 1;
		this.UpdateCharacter();
	}

	public void SetHeadGear(int gearIndex)
	{
		this.selectedHeadGearIndex = gearIndex - 1;
		this.UpdateCharacter();
	}

	public void SetFacialHair(int facialHairIndex)
	{
		if (this.selectedSexIndex == 0)
		{
			this.selectedFacialHairIndex = facialHairIndex - 1;
			this.UpdateCharacter();
		}
	}

	public void ToggleJiggle()
	{
		this.isJiggling = !this.isJiggling;
		if (this.isJiggling)
		{
			base.StartCoroutine(this.JiggleCharacter());
		}
	}

	public IEnumerator JiggleCharacter()
	{
		float angle = 20f;
		float speed = 6f;
		while (this.isJiggling)
		{
			float num = angle * Mathf.SmoothStep(-1f, 1f, Mathf.Sin(Time.time * speed) * 0.5f + 0.5f);
			this.characterInstance.transform.localRotation = Quaternion.Euler(0f, this.rotationValue + num, 0f);
			yield return null;
		}
		this.characterInstance.transform.localRotation = Quaternion.Euler(0f, this.rotationValue, 0f);
		yield break;
	}

	public bool IsJiggleEnabled()
	{
		return this.isJiggling;
	}

	public void RotateCharacter(float value)
	{
		this.rotationValue = -value;
		this.characterInstance.transform.localRotation = Quaternion.Euler(0f, this.rotationValue, 0f);
	}

	public GameObject baseRig;

	[Header("Character Configuration")]
	public int selectedSexIndex;

	public int selectedRaceIndex;

	public int selectedVariantIndex;

	public int selectedHairIndex;

	public int selectedGearIndex = -1;

	public int selectedHeadGearIndex = -1;

	public int selectedBodyGearIndex = -1;

	public int selectedHandsGearIndex = -1;

	public int selectedFeetGearIndex = -1;

	public int selectedFacialHairIndex = -1;

	public int selectedHairColorIndex = 2;

	public int selectedEyeColorIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject characterInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SDCSUtils.TransformCatalog boneCatalog;

	public bool isJiggling;

	public float rotationValue = 180f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] sexes = new string[] { "Male", "Female" };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] races = new string[] { "White", "Black", "Asian", "Native" };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int[] variants = new int[] { 1, 2, 3, 4 };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] hairColors;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] eyeColors;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] hairs = new string[]
	{
		"buzzcut", "comb_over", "slicked_back", "slicked_back_long", "pixie_cut", "ponytail", "midpart_karen_messy", "midpart_long", "midpart_mid", "midpart_short",
		"midpart_shoulder", "sidepart_short", "sidepart_mid", "sidepart_long", "cornrows", "mohawk", "flattop_fro", "small_fro", "dreads", "afro_curly"
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int[] facialHairIndexes = new int[] { 1, 2, 3, 4, 5 };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<string> allGearKeys = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CharacterConfigurator.SDCSGearXmlCatalog gearCatalog;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string[] baseParts = new string[] { "head", "body", "hands", "feet" };

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class SDCSGearXmlCatalog
	{
		public static CharacterConfigurator.SDCSGearXmlCatalog BuildFromXml(string xmlText)
		{
			CharacterConfigurator.SDCSGearXmlCatalog sdcsgearXmlCatalog = new CharacterConfigurator.SDCSGearXmlCatalog();
			if (string.IsNullOrEmpty(xmlText))
			{
				return sdcsgearXmlCatalog;
			}
			foreach (XElement xelement in XDocument.Parse(xmlText, LoadOptions.PreserveWhitespace).Descendants("item"))
			{
				XElement xelement2 = xelement.Elements("property").FirstOrDefault((XElement p) => CharacterConfigurator.SDCSGearXmlCatalog.EqualsIgnoreCase(CharacterConfigurator.SDCSGearXmlCatalog.Attr(p, "class"), "SDCS"));
				if (xelement2 != null)
				{
					string text = CharacterConfigurator.SDCSGearXmlCatalog.DirectPropertyValue(xelement2, "Prefab");
					if (!string.IsNullOrEmpty(text))
					{
						string text2 = CharacterConfigurator.SDCSGearXmlCatalog.DirectPropertyValue(xelement, "EquipSlot");
						string text3 = CharacterConfigurator.SDCSGearXmlCatalog.NormalizePart(CharacterConfigurator.SDCSGearXmlCatalog.DirectPropertyValue(xelement2, "TransformName"), text2);
						if (!string.IsNullOrEmpty(text3))
						{
							string text4 = CharacterConfigurator.SDCSGearXmlCatalog.Attr(xelement, "name");
							string text5 = CharacterConfigurator.SDCSGearXmlCatalog.NormalizeGearKey(CharacterConfigurator.SDCSGearXmlCatalog.DirectPropertyValue(xelement, "ArmorGroup"), CharacterConfigurator.SDCSGearXmlCatalog.DirectPropertyValue(xelement, "DisplayType"), text4);
							if (!string.IsNullOrEmpty(text5))
							{
								string text6 = CharacterConfigurator.SDCSGearXmlCatalog.DirectPropertyValue(xelement2, "Excludes");
								sdcsgearXmlCatalog.Add(new CharacterConfigurator.SDCSGearXmlCatalog.Entry
								{
									ItemName = text4,
									GearKey = text5,
									PartName = text3,
									EquipSlot = text2,
									PrefabName = text,
									BaseToTurnOff = CharacterConfigurator.SDCSGearXmlCatalog.NormalizeBaseToTurnOff(text6, text3),
									HairMaskType = CharacterConfigurator.SDCSGearXmlCatalog.NormalizeOptional(CharacterConfigurator.SDCSGearXmlCatalog.DirectPropertyValue(xelement2, "HairMaskType")),
									FacialHairMaskType = CharacterConfigurator.SDCSGearXmlCatalog.NormalizeOptional(CharacterConfigurator.SDCSGearXmlCatalog.DirectPropertyValue(xelement2, "FacialHairMaskType"))
								});
							}
						}
					}
				}
			}
			return sdcsgearXmlCatalog;
		}

		public List<string> GetOrderedKeys()
		{
			return new List<string>(this.orderedKeys);
		}

		public bool TryGetPart(string gearKey, string partName, out CharacterConfigurator.SDCSGearXmlCatalog.Entry entry)
		{
			entry = null;
			CharacterConfigurator.SDCSGearXmlCatalog.GearSet gearSet;
			return !string.IsNullOrEmpty(gearKey) && !string.IsNullOrEmpty(partName) && this.sets.TryGetValue(gearKey, out gearSet) && gearSet.Parts.TryGetValue(partName, out entry);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Add(CharacterConfigurator.SDCSGearXmlCatalog.Entry entry)
		{
			if (entry == null || string.IsNullOrEmpty(entry.GearKey) || string.IsNullOrEmpty(entry.PartName))
			{
				return;
			}
			CharacterConfigurator.SDCSGearXmlCatalog.GearSet gearSet;
			if (!this.sets.TryGetValue(entry.GearKey, out gearSet))
			{
				gearSet = new CharacterConfigurator.SDCSGearXmlCatalog.GearSet();
				gearSet.Key = entry.GearKey;
				this.sets.Add(entry.GearKey, gearSet);
				this.orderedKeys.Add(entry.GearKey);
			}
			gearSet.Parts[entry.PartName] = entry;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string Attr(XElement e, string name)
		{
			XAttribute xattribute = ((e == null) ? null : e.Attribute(name));
			if (xattribute != null)
			{
				return xattribute.Value;
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string DirectPropertyValue(XElement parent, string name)
		{
			if (parent == null)
			{
				return null;
			}
			XElement xelement = parent.Elements("property").FirstOrDefault((XElement p) => CharacterConfigurator.SDCSGearXmlCatalog.EqualsIgnoreCase(CharacterConfigurator.SDCSGearXmlCatalog.Attr(p, "name"), name));
			XAttribute xattribute = ((xelement == null) ? null : xelement.Attribute("value"));
			if (xattribute != null)
			{
				return xattribute.Value;
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string NormalizeGearKey(string armorGroup, string displayType, string itemName)
		{
			if (!string.IsNullOrEmpty(armorGroup))
			{
				return CharacterConfigurator.SDCSGearXmlCatalog.StripPrefix(armorGroup.Trim(), "group");
			}
			return CharacterConfigurator.SDCSGearXmlCatalog.NormalizeOptional(CharacterConfigurator.SDCSGearXmlCatalog.StripSuffix(CharacterConfigurator.SDCSGearXmlCatalog.StripSuffix(CharacterConfigurator.SDCSGearXmlCatalog.StripSuffix(CharacterConfigurator.SDCSGearXmlCatalog.StripSuffix(CharacterConfigurator.SDCSGearXmlCatalog.StripPrefix(((!string.IsNullOrEmpty(displayType)) ? displayType : itemName) ?? string.Empty, "armor"), "Helmet"), "Outfit"), "Gloves"), "Boots"));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string NormalizePart(string transformName, string equipSlot)
		{
			string text = CharacterConfigurator.SDCSGearXmlCatalog.NormalizeOptional(transformName);
			if (!string.IsNullOrEmpty(text))
			{
				text = text.ToLowerInvariant();
				if (text == "head" || text == "body" || text == "hands" || text == "feet")
				{
					return text;
				}
			}
			string text2 = CharacterConfigurator.SDCSGearXmlCatalog.NormalizeOptional(equipSlot);
			if (string.IsNullOrEmpty(text2))
			{
				return null;
			}
			string text3 = text2.ToLowerInvariant();
			if (text3 == "head")
			{
				return "head";
			}
			if (text3 == "chest")
			{
				return "body";
			}
			if (text3 == "hands")
			{
				return "hands";
			}
			if (!(text3 == "feet"))
			{
				return null;
			}
			return "feet";
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string NormalizeBaseToTurnOff(string excludes, string preferredPart)
		{
			excludes = CharacterConfigurator.SDCSGearXmlCatalog.NormalizeOptional(excludes);
			if (string.IsNullOrEmpty(excludes))
			{
				return null;
			}
			string[] array = excludes.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				string text = CharacterConfigurator.SDCSGearXmlCatalog.NormalizePartName(array[i]);
				if (CharacterConfigurator.SDCSGearXmlCatalog.EqualsIgnoreCase(text, preferredPart))
				{
					return text;
				}
			}
			for (int j = 0; j < array.Length; j++)
			{
				string text2 = CharacterConfigurator.SDCSGearXmlCatalog.NormalizePartName(array[j]);
				if (!string.IsNullOrEmpty(text2))
				{
					return text2;
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string NormalizePartName(string value)
		{
			value = CharacterConfigurator.SDCSGearXmlCatalog.NormalizeOptional(value);
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}
			value = value.ToLowerInvariant();
			if (value == "head" || value == "body" || value == "hands" || value == "feet")
			{
				return value;
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string NormalizeOptional(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return null;
			}
			value = value.Trim();
			return value;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string StripPrefix(string value, string prefix)
		{
			if (!string.IsNullOrEmpty(value) && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				return value.Substring(prefix.Length);
			}
			return value;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string StripSuffix(string value, string suffix)
		{
			if (!string.IsNullOrEmpty(value) && value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
			{
				return value.Substring(0, value.Length - suffix.Length);
			}
			return value;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool EqualsIgnoreCase(string a, string b)
		{
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, CharacterConfigurator.SDCSGearXmlCatalog.GearSet> sets = new Dictionary<string, CharacterConfigurator.SDCSGearXmlCatalog.GearSet>(StringComparer.OrdinalIgnoreCase);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<string> orderedKeys = new List<string>();

		public sealed class Entry
		{
			public string ItemName;

			public string GearKey;

			public string PartName;

			public string EquipSlot;

			public string PrefabName;

			public string BaseToTurnOff;

			public string HairMaskType;

			public string FacialHairMaskType;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public sealed class GearSet
		{
			public string Key;

			public readonly Dictionary<string, CharacterConfigurator.SDCSGearXmlCatalog.Entry> Parts = new Dictionary<string, CharacterConfigurator.SDCSGearXmlCatalog.Entry>(StringComparer.OrdinalIgnoreCase);
		}
	}
}
