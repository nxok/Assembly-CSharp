using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionUnregisterSequenceLink : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < this.targets.Count; i++)
		{
			if (this.targets[i] != null)
			{
				GameEventManager.Current.UnRegisterLink(_params.Self as EntityPlayer, this.sequenceLink);
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "sequence_link")
		{
			this.sequenceLink = _attribute.Value;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string sequenceLink = "";
}
