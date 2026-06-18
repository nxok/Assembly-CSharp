using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionLogMessage : MinEventActionBase
{
	public override void Execute(MinEventParams _params)
	{
		Log.Out("MinEventLogMessage: {0}", new object[] { this.message });
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "message")
		{
			this.message = _attribute.Value;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string message;
}
