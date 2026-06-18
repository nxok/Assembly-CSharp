using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class CVarCompare : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		return base.IsValid(_params) && this.invert != RequirementBase.compareValues(this.target.Buffs.GetCustomVar(this.cvarCompareName), this.operation, this.value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("cvar.{0} {1} {2}", this.cvarCompareName, this.operation.ToStringCached<RequirementBase.OperationTypes>(), this.value.ToCultureInvariantString()));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "cvar")
		{
			this.cvarCompareName = _attribute.Value;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string cvarCompareName;
}
