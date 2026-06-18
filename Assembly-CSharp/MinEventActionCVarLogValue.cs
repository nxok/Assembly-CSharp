using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionCVarLogValue : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < this.targets.Count; i++)
		{
			Log.Out("CVarLogValue: {0} == {1}", new object[]
			{
				this.cvarName,
				this.targets[i].Buffs.GetCustomVar(this.cvarName).ToCultureInvariantString()
			});
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "cvar")
		{
			this.cvarName = _attribute.Value;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string cvarName;
}
