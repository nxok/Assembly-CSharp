using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetProgressionLevel : MinEventActionTargetedBase
{
	public string progressionName
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int level
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Execute(MinEventParams _params)
	{
		if (this.targets == null)
		{
			return;
		}
		for (int i = 0; i < this.targets.Count; i++)
		{
			if (this.targets[i].Progression != null)
			{
				ProgressionValue progressionValue = this.targets[i].Progression.GetProgressionValue(this.progressionName);
				if (progressionValue != null)
				{
					if (this.level != -1)
					{
						progressionValue.Level = this.level;
						this.targets[i].Progression.bProgressionStatsChanged = !this.targets[i].isEntityRemote;
						this.targets[i].bPlayerStatsChanged |= !this.targets[i].isEntityRemote;
					}
					else
					{
						progressionValue.Level = progressionValue.ProgressionClass.MaxLevel;
						this.targets[i].Progression.bProgressionStatsChanged = !this.targets[i].isEntityRemote;
						this.targets[i].bPlayerStatsChanged |= !this.targets[i].isEntityRemote;
					}
				}
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		return base.CanExecute(_eventType, _params) && this.progressionName != null && this.level >= -1;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (!(localName == "progression_name"))
			{
				if (localName == "level")
				{
					this.level = StringParsers.ParseSInt32(_attribute.Value, 0, -1, NumberStyles.Integer);
				}
			}
			else
			{
				this.progressionName = _attribute.Value;
			}
		}
		return flag;
	}
}
