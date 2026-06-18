using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionCallGameEvent : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || this.allowClientCall)
		{
			for (int i = 0; i < this.targets.Count; i++)
			{
				if (this.targets[i] != null)
				{
					GameEventManager.Current.HandleAction(this.eventName, _params.Self as EntityPlayer, this.targets[i], false, "", "", false, true, this.sequenceLink, null);
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
			if (!(localName == "event"))
			{
				if (!(localName == "sequence_link"))
				{
					if (localName == "allow_client_call")
					{
						this.allowClientCall = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
					}
				}
				else
				{
					this.sequenceLink = _attribute.Value;
				}
			}
			else
			{
				this.eventName = _attribute.Value;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string eventName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string sequenceLink = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool allowClientCall;
}
