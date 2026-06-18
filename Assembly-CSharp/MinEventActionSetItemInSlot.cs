using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetItemInSlot : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		ItemValue item = ItemClass.GetItem(this.itemName, false);
		ItemClassArmor itemClassArmor = item.ItemClass as ItemClassArmor;
		if (itemClassArmor != null && itemClassArmor.EquipSlot == this.slot)
		{
			for (int i = 0; i < this.targets.Count; i++)
			{
				this.targets[i].equipment.SetSlotItem((int)this.slot, item, true);
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "item_name")
			{
				this.itemName = _attribute.Value;
				return true;
			}
			if (localName == "equip_slot")
			{
				Enum.TryParse<EquipmentSlots>(_attribute.Value, out this.slot);
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string itemName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EquipmentSlots slot = EquipmentSlots.BiomeBadge;
}
