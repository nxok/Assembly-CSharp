using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetOverrideLoot : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		for (int i = 0; i < this.targets.Count; i++)
		{
			EntityPlayer entityPlayer = this.targets[i] as EntityPlayer;
			if (entityPlayer != null)
			{
				if (this.altLoot == "")
				{
					LootContainer.OverrideItems.Remove(entityPlayer);
				}
				else if (LootContainer.OverrideItems.ContainsKey(entityPlayer))
				{
					LootContainer.OverrideItems[entityPlayer] = this.altLoot.Split(',', StringSplitOptions.None);
				}
				else
				{
					LootContainer.OverrideItems.Add(entityPlayer, this.altLoot.Split(',', StringSplitOptions.None));
				}
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "items")
		{
			this.altLoot = _attribute.Value;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string altLoot = "";
}
