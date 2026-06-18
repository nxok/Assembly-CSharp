using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRage : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < this.targets.Count; i++)
		{
			EntityHuman entityHuman = this.targets[i] as EntityHuman;
			if (entityHuman != null)
			{
				if (this.enabled)
				{
					entityHuman.StartRage(this.speedPercent, this.rageTime + 1f);
				}
				else
				{
					entityHuman.StopRage();
				}
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "speed")
			{
				this.speedPercent = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
				return true;
			}
			if (localName == "time")
			{
				this.rageTime = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
				return true;
			}
			if (localName == "enabled")
			{
				this.enabled = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public float speedPercent = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float rageTime = 60f;
}
