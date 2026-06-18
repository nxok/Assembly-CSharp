using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemClassArmor : ItemClass
{
	public override bool KeepOnDeath()
	{
		return this.keepOnDeath;
	}

	public override void Init()
	{
		base.Init();
		string text = "";
		this.Properties.ParseString("ArmorGroup", ref text);
		this.ArmorGroup = text.Split(',', StringSplitOptions.None);
		if (this.Properties.Values.ContainsKey("EquipSlot"))
		{
			this.EquipSlot = EnumUtils.Parse<EquipmentSlots>(this.Properties.Values["EquipSlot"], false);
		}
		this.Properties.ParseBool("IsCosmetic", ref this.IsCosmetic);
		this.Properties.ParseBool("KeepOnDeath", ref this.keepOnDeath);
		this.Properties.ParseBool("AllowUnEquip", ref this.AllowUnEquip);
		this.Properties.ParseBool("AutoEquip", ref this.AutoEquip);
		this.Properties.ParseString("ReplaceByTag", ref this.ReplaceByTag);
	}

	public override bool CanEquip()
	{
		return (World.TemperatureSurvival || World.StormFrequency != 0f || this.EquipSlot < EquipmentSlots.ClothingHead || this.EquipSlot > EquipmentSlots.ClothingFeet) && (World.BiomeProgressionEnabled || this.EquipSlot < EquipmentSlots.BiomeBadge || this.EquipSlot > EquipmentSlots.BiomeBadge4);
	}

	public const string PropEquipSlot = "EquipSlot";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropArmorGroup = "ArmorGroup";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropIsCosmetic = "IsCosmetic";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropKeepOnDeath = "KeepOnDeath";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropAllowUnEquip = "AllowUnEquip";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropAutoEquip = "AutoEquip";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropReplaceByTag = "ReplaceByTag";

	public EquipmentSlots EquipSlot = EquipmentSlots.Count;

	public string[] ArmorGroup;

	public bool IsCosmetic = true;

	public int CosmeticID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool keepOnDeath;

	public bool AllowUnEquip = true;

	public bool AutoEquip;

	public string ReplaceByTag;
}
