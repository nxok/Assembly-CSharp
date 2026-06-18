using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetPartActive : MinEventActionBase
{
	public override void Execute(MinEventParams _params)
	{
		bool flag = this.isActive;
		if (this.cVarName != null)
		{
			flag = _params.Self.GetCVar(this.cVarName) != 0f;
			if (this.isInvert)
			{
				flag = !flag;
			}
		}
		_params.Self.SetPartActive(this.partName, flag);
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		return base.CanExecute(_eventType, _params) && _params.Self != null && _params.ItemValue != null && this.partName != null;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "active")
			{
				if (_attribute.Value.Length >= 2 && _attribute.Value[0] == '@')
				{
					int num = 1;
					if (_attribute.Value[1] == '!')
					{
						num++;
						this.isInvert = true;
					}
					this.cVarName = _attribute.Value.Substring(num);
				}
				else
				{
					this.isActive = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
				}
				return true;
			}
			if (localName == "part")
			{
				this.partName = _attribute.Value;
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string partName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInvert;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cVarName;
}
