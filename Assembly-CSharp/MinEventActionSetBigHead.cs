using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetBigHead : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < this.targets.Count; i++)
		{
			EntityAlive entityAlive = this.targets[i];
			if (this.enabled)
			{
				if (this.instant)
				{
					entityAlive.ForceBigHead();
				}
				else
				{
					entityAlive.SetBigHead();
				}
			}
			else if (this.instant)
			{
				entityAlive.ForceResetHead();
			}
			else
			{
				entityAlive.ResetHead();
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (!(localName == "instant"))
			{
				if (localName == "enabled")
				{
					this.enabled = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
				}
			}
			else
			{
				this.instant = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool instant;
}
