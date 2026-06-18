using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRemoveCVar : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < this.targets.Count; i++)
		{
			for (int j = 0; j < this.cvarNames.Length; j++)
			{
				this.targets[i].Buffs.SetCustomVar(this.cvarNames[j], 0f, (this.targets[i].isEntityRemote && !_params.Self.isEntityRemote) || _params.IsLocal, CVarOperation.set);
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "cvar")
		{
			this.cvarNames = _attribute.Value.Split(',', StringSplitOptions.None);
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] cvarNames;
}
