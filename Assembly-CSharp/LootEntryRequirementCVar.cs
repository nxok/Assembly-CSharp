using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class LootEntryRequirementCVar : BaseOperationLootEntryRequirement
{
	public override void Init(XElement e)
	{
		base.Init(e);
		e.ParseAttribute("cvar", ref this.cvar);
		e.ParseAttribute("value", ref this.valueText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float LeftSide(EntityPlayer player)
	{
		if (player != null)
		{
			return player.Buffs.GetCustomVar(this.cvar);
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float RightSide(EntityPlayer player)
	{
		return StringParsers.ParseFloat(this.valueText, 0, -1, NumberStyles.Any);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string cvar;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText;
}
