using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionRemoveParticleEffectFromEntity : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		if (_params.Self == null)
		{
			return;
		}
		_params.Self.RemoveParticle(this.particleEffectName);
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		return base.CanExecute(_eventType, _params) && _params.Self != null && this.particleEffectName != null;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "particle")
		{
			this.particleEffectName = "Ptl_" + _attribute.Value;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string particleEffectName;
}
