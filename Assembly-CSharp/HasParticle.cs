using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class HasParticle : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!this.invert)
		{
			return _params.Self.HasParticle(this.particleName);
		}
		return !_params.Self.HasParticle(this.particleName);
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "particle")
		{
			this.particleName = _attribute.Value;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string particleName = "";
}
