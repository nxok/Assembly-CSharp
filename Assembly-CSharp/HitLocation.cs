using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class HitLocation : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!this.invert)
		{
			return (this.bodyParts & _params.DamageResponse.HitBodyPart) > EnumBodyPartHit.None;
		}
		return (this.bodyParts & _params.DamageResponse.HitBodyPart) == EnumBodyPartHit.None;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("{0} hit location: ", this.invert ? "NOT " : "", this.bodyPartNames));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "body_parts")
		{
			this.bodyPartNames = _attribute.Value;
			string[] array = this.bodyPartNames.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				this.bodyParts |= EnumUtils.Parse<EnumBodyPartHit>(array[i], true);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string bodyPartNames = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumBodyPartHit bodyParts;
}
