using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetNavObject : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		if (this.navObjectName == "")
		{
			return;
		}
		for (int i = 0; i < this.targets.Count; i++)
		{
			EntityAlive entityAlive = this.targets[i];
			if (this.isAdd)
			{
				entityAlive.AddNavObject(this.navObjectName, this.overrideSprite, (this.cvarToText != "") ? entityAlive.GetCVar(this.cvarToText).ToString() : this.overrideText);
			}
			else
			{
				entityAlive.RemoveNavObject(this.navObjectName);
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "nav_object")
			{
				this.navObjectName = _attribute.Value;
				return true;
			}
			if (localName == "sprite")
			{
				this.overrideSprite = _attribute.Value;
				return true;
			}
			if (localName == "text")
			{
				this.overrideText = _attribute.Value;
				return true;
			}
			if (localName == "cvar_to_text")
			{
				this.cvarToText = _attribute.Value;
				return true;
			}
			if (localName == "add")
			{
				this.isAdd = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string navObjectName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string overrideSprite = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string overrideText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string cvarToText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAdd = true;
}
