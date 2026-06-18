using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionGiveSkillExp : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		if (this.targets == null)
		{
			return;
		}
		for (int i = 0; i < this.targets.Count; i++)
		{
			if (this.exp != -1)
			{
				this.targets[i].Progression.AddLevelExp(this.exp, "_xpOther", Progression.XPTypes.Other, true, true, -1, null);
				this.targets[i].Progression.bProgressionStatsChanged = !this.targets[i].isEntityRemote;
				this.targets[i].bPlayerStatsChanged |= !this.targets[i].isEntityRemote;
			}
			else if (this.level_percent != -1f)
			{
				this.targets[i].Progression.AddLevelExp(this.exp, "_xpOther", Progression.XPTypes.Other, true, true, -1, null);
				this.targets[i].Progression.bProgressionStatsChanged = !this.targets[i].isEntityRemote;
				this.targets[i].bPlayerStatsChanged |= !this.targets[i].isEntityRemote;
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		return base.CanExecute(_eventType, _params) && this.skill != null && (this.exp != -1 || this.level_percent != -1f);
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (!(localName == "skill"))
			{
				if (!(localName == "experience"))
				{
					if (localName == "level_percentage")
					{
						this.level_percent = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
					}
				}
				else
				{
					this.exp = StringParsers.ParseSInt32(_attribute.Value, 0, -1, NumberStyles.Integer);
				}
			}
			else
			{
				this.skill = _attribute.Value;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string skill;

	[PublicizedFrom(EAccessModifier.Private)]
	public int exp = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float level_percent = -1f;
}
