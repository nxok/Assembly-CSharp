using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionShowToolbeltMessage : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < this.targets.Count; i++)
		{
			if (this.targets[i] as EntityPlayerLocal != null)
			{
				if (this.sound != null)
				{
					GameManager.ShowTooltip(this.targets[i] as EntityPlayerLocal, this.message, string.Empty, this.sound, null, false, false, 0f);
				}
				else
				{
					GameManager.ShowTooltip(this.targets[i] as EntityPlayerLocal, this.message, false, false, 0f);
				}
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		return base.CanExecute(_eventType, _params) && this.message != null;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "message")
			{
				if (this.message == null || this.message == "")
				{
					this.message = _attribute.Value;
				}
				return true;
			}
			if (localName == "message_key")
			{
				if (Localization.Exists(_attribute.Value, false))
				{
					this.message = Localization.Get(_attribute.Value, false, null);
				}
				return true;
			}
			if (localName == "sound")
			{
				if (_attribute.Value != "")
				{
					this.sound = _attribute.Value;
				}
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string message;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sound;
}
