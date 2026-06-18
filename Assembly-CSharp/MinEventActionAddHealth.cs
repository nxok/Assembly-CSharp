using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddHealth : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		if (this.targets == null)
		{
			return;
		}
		for (int i = 0; i < this.targets.Count; i++)
		{
			int num = ((!this.cvarRef) ? this.health : ((int)this.targets[i].Buffs.GetCustomVar(this.refCvarName)));
			this.targets[i].AddHealth(num);
			if (this.showSplatter && num < 0)
			{
				EntityPlayerLocal entityPlayerLocal = this.targets[i] as EntityPlayerLocal;
				if (entityPlayerLocal != null)
				{
					entityPlayerLocal.ForceBloodSplatter();
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
			if (!(localName == "health"))
			{
				if (localName == "show_splatter")
				{
					this.showSplatter = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
				}
			}
			else if (_attribute.Value.StartsWith("@"))
			{
				this.cvarRef = true;
				this.refCvarName = _attribute.Value.Substring(1);
			}
			else
			{
				this.health = StringParsers.ParseSInt32(_attribute.Value, 0, -1, NumberStyles.Integer);
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cvarRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int health;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showSplatter = true;
}
