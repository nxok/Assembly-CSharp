using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionGiveExp : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		if (this.targets == null)
		{
			return;
		}
		for (int i = 0; i < this.targets.Count; i++)
		{
			EntityAlive entityAlive = this.targets[i];
			if (entityAlive.Progression != null)
			{
				entityAlive.Progression.AddLevelExp((!this.cvarRef) ? this.exp : ((int)entityAlive.Buffs.GetCustomVar(this.refCvarName)), "_xpOther", Progression.XPTypes.Other, true, true, -1, null);
				entityAlive.Progression.bProgressionStatsChanged = !entityAlive.isEntityRemote;
			}
			entityAlive.bPlayerStatsChanged |= !entityAlive.isEntityRemote;
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		return base.CanExecute(_eventType, _params) && (this.cvarRef || this.exp > 0);
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "experience" || localName == "exp")
			{
				if (_attribute.Value.StartsWith("@"))
				{
					this.cvarRef = true;
					this.refCvarName = _attribute.Value.Substring(1);
				}
				else
				{
					this.exp = StringParsers.ParseSInt32(_attribute.Value, 0, -1, NumberStyles.Integer);
				}
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int exp = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cvarRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarName;
}
