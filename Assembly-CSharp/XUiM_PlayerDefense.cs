using System;

public class XUiM_PlayerDefense : XUiModel
{
	public static string GetBashing(EntityPlayer _player)
	{
		return XUiM_PlayerDefense.GetDefenseFromPlayer(_player, EnumDamageTypes.Bashing) + "%";
	}

	public static string GetPiercing(EntityPlayer _player)
	{
		return XUiM_PlayerDefense.GetDefenseFromPlayer(_player, EnumDamageTypes.Piercing) + "%";
	}

	public static string GetRadiation(EntityPlayer _player)
	{
		return XUiM_PlayerDefense.GetDefenseFromPlayer(_player, EnumDamageTypes.Radiation) + "%";
	}

	public static string GetWaterproof(EntityPlayer _player)
	{
		return string.Format("{0}%", (int)(_player.equipment.GetTotalWaterproof() * 100f));
	}

	public static string GetFireproof(EntityPlayer _player)
	{
		return XUiM_PlayerDefense.GetDefenseFromPlayer(_player, EnumDamageTypes.Heat) + "%";
	}

	public static string GetElectrical(EntityPlayer _player)
	{
		return XUiM_PlayerDefense.GetDefenseFromPlayer(_player, EnumDamageTypes.Electrical) + "%";
	}

	public static string GetInsulation(EntityPlayer _player)
	{
		return ValueDisplayFormatters.TemperatureRelative(_player.equipment.GetTotalInsulation(), 0);
	}

	public static string GetWeight(EntityPlayer _player)
	{
		float num = 0f;
		int slotCount = _player.equipment.GetSlotCount();
		for (int i = 0; i < slotCount; i++)
		{
			ItemValue slotItem = _player.equipment.GetSlotItem(i);
			ItemClass itemClass = ((slotItem != null) ? slotItem.ItemClass : null);
			if (itemClass != null)
			{
				num += itemClass.Encumbrance * 100f;
			}
		}
		return num.ToCultureInvariantString() + "%";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetDefenseFromPlayer(EntityPlayer _player, EnumDamageTypes _armorType)
	{
		float num = 0f;
		if (_armorType != EnumDamageTypes.None && _armorType < EnumDamageTypes.Disease)
		{
			if (_armorType <= EnumDamageTypes.Crushing)
			{
				num = EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, null, 0f, _player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			}
			else if (_armorType <= EnumDamageTypes.Electrical)
			{
				num = EffectManager.GetValue(PassiveEffects.ElementalDamageResist, null, 0f, _player, null, FastTags<TagGroup.Global>.Parse(_armorType.ToStringCached<EnumDamageTypes>()), true, true, true, true, true, 1, true, false);
			}
		}
		return num.ToCultureInvariantString("00");
	}

	public static string GetBashing(ItemValue _itemValue, XUi _xui)
	{
		return XUiM_PlayerDefense.GetDefenseFromItemValue(_itemValue, EnumDamageTypes.Bashing, _xui) + "%";
	}

	public static string GetPiercing(ItemValue _itemValue, XUi _xui)
	{
		return XUiM_PlayerDefense.GetDefenseFromItemValue(_itemValue, EnumDamageTypes.Piercing, _xui) + "%";
	}

	public static string GetRadiation(ItemValue _itemValue, XUi _xui)
	{
		return XUiM_PlayerDefense.GetDefenseFromItemValue(_itemValue, EnumDamageTypes.Radiation, _xui) + "%";
	}

	public static string GetWaterproof(ItemValue _itemValue)
	{
		float num = 0f;
		ItemClass itemClass = _itemValue.ItemClass;
		if (itemClass != null)
		{
			num = itemClass.WaterProof;
		}
		return string.Format("{0}%", (int)(num * 100f));
	}

	public static string GetFireproof(ItemValue _itemValue, XUi _xui)
	{
		return XUiM_PlayerDefense.GetDefenseFromItemValue(_itemValue, EnumDamageTypes.Heat, _xui) + "%";
	}

	public static string GetElectrical(ItemValue _itemValue, XUi _xui)
	{
		return XUiM_PlayerDefense.GetDefenseFromItemValue(_itemValue, EnumDamageTypes.Electrical, _xui) + "%";
	}

	public static string GetInsulation(ItemValue _itemValue)
	{
		float num = 0f;
		ItemClass itemClass = _itemValue.ItemClass;
		if (itemClass != null)
		{
			num = itemClass.Insulation;
		}
		return ValueDisplayFormatters.TemperatureRelative(num, 0);
	}

	public static string GetWeight(ItemValue _itemValue)
	{
		float num = 0f;
		ItemClass itemClass = _itemValue.ItemClass;
		if (itemClass != null)
		{
			num = itemClass.Encumbrance * 100f;
		}
		return num.ToCultureInvariantString() + "%";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetDefenseFromItemValue(ItemValue _itemValue, EnumDamageTypes _damageType, XUi _xui)
	{
		return (EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, _itemValue, 0f, _xui.playerUI.entityPlayer, null, FastTags<TagGroup.Global>.Parse(_damageType.ToStringCached<EnumDamageTypes>()), true, true, true, true, true, 1, true, false) * 100f).ToCultureInvariantString("00");
	}
}
