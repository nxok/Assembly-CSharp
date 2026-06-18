using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class LootEntryRequirementProgression : BaseOperationLootEntryRequirement
{
	public override void Init(XElement e)
	{
		base.Init(e);
		e.ParseAttribute("name", ref this.progressionName);
		e.ParseAttribute("value", ref this.valueText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float LeftSide(EntityPlayer player)
	{
		if (player != null)
		{
			ProgressionValue progressionValue = player.Progression.GetProgressionValue(this.progressionName);
			if (progressionValue != null)
			{
				return progressionValue.GetCalculatedLevel(player);
			}
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float RightSide(EntityPlayer player)
	{
		return StringParsers.ParseFloat(this.valueText, 0, -1, NumberStyles.Any);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string progressionName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string valueText;
}
