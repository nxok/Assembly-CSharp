using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRefreshPerks : MinEventActionBase
{
	public override void Execute(MinEventParams _params)
	{
		if (_params.Self.Progression == null)
		{
			return;
		}
		_params.Self.Progression.RefreshPerks(this.attribute);
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "attribute")
		{
			this.attribute = _attribute.Value;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string attribute = "";
}
