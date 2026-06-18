using System;
using System.Collections.Generic;
using UniLinq;

public class Archetype
{
	public string Sex
	{
		get
		{
			if (!this.IsMale)
			{
				return "Female";
			}
			return "Male";
		}
		set
		{
			this.IsMale = value.ToLower() == "male";
		}
	}

	public bool ShowInList
	{
		get
		{
			return this.Name != "BaseMale" && this.Name != "BaseFemale";
		}
	}

	public Archetype(string _name, bool _isMale, bool _canCustomize)
	{
		this.Name = _name;
		this.IsMale = _isMale;
		this.CanCustomize = _canCustomize;
	}

	public static void InitializeStatics()
	{
		Archetype.s_Archetypes = new CaseInsensitiveStringDictionary<Archetype>();
	}

	public static void SetArchetype(Archetype archetype)
	{
		if (Archetype.s_Archetypes == null)
		{
			return;
		}
		if (Archetype.s_Archetypes.ContainsKey(archetype.Name))
		{
			Archetype.s_Archetypes[archetype.Name] = archetype;
			return;
		}
		Archetype.s_Archetypes[archetype.Name] = archetype;
		if (!archetype.CanCustomize)
		{
			ProfileSDF.SaveArchetype(archetype.Name, archetype.IsMale);
		}
	}

	public static Archetype GetArchetype(string name)
	{
		if (Archetype.s_Archetypes == null)
		{
			return null;
		}
		if (!Archetype.s_Archetypes.ContainsKey(name))
		{
			return null;
		}
		return Archetype.s_Archetypes[name];
	}

	public void AddEquipmentSlot(SDCSUtils.SlotData slotData)
	{
		if (this.Equipment == null)
		{
			this.Equipment = new List<SDCSUtils.SlotData>();
		}
		this.Equipment.Add(slotData);
	}

	public Archetype Clone()
	{
		return new Archetype(this.Name, this.IsMale, this.CanCustomize)
		{
			CanCustomize = this.CanCustomize,
			IsMale = this.IsMale,
			Race = this.Race,
			Variant = this.Variant,
			Hair = this.Hair,
			HairColor = this.HairColor,
			MustacheName = this.MustacheName,
			ChopsName = this.ChopsName,
			BeardName = this.BeardName,
			EyeColorName = this.EyeColorName
		};
	}

	public static void SaveArchetypesToFile()
	{
		SDCSArchetypesFromXml.Save("archetypes", Archetype.s_Archetypes.Values.ToList<Archetype>());
	}

	public static Dictionary<string, Archetype> s_Archetypes;

	public string Name;

	public string Race;

	public int Variant;

	public string Hair = "";

	public string HairColor = "";

	public string MustacheName = "";

	public string ChopsName = "";

	public string BeardName = "";

	public string EyeColorName = "Blue01";

	public bool IsMale;

	public bool CanCustomize;

	public List<SDCSUtils.SlotData> Equipment;
}
