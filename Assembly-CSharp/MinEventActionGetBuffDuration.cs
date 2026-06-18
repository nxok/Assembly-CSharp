using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionGetBuffDuration : MinEventActionTargetedBase
{
	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params))
		{
			BuffValue buff = _params.Buff;
			if (((buff != null) ? buff.BuffClass : null) != null)
			{
				return !string.IsNullOrEmpty(this.reference);
			}
		}
		return false;
	}

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < this.targets.Count; i++)
		{
			this.targets[i].Buffs.SetCustomVar(this.reference, _params.Buff.BuffClass.DurationMax, true, CVarOperation.set);
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "reference")
		{
			this.reference = _attribute.Value;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string reference;
}
