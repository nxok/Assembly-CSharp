using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionModifyScreenEffect : MinEventActionBase
{
	public override void Execute(MinEventParams _params)
	{
		EntityPlayerLocal entityPlayerLocal = _params.Self as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			entityPlayerLocal.ScreenEffectManager.SetScreenEffect(this.effect_name, this.intensity, this.fade);
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		return base.CanExecute(_eventType, _params) && _params.Self is EntityPlayerLocal && this.effect_name != "";
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "effect_name")
			{
				this.effect_name = _attribute.Value;
				return true;
			}
			if (localName == "intensity")
			{
				this.intensity = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
				return true;
			}
			if (localName == "fade")
			{
				this.fade = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string effect_name = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public float intensity = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fade = 4f;
}
