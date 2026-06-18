using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsCorpse : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (base.IsValid(_params) && this.target.IsCorpse())
		{
			return !this.invert;
		}
		return this.invert;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Entity {0}IsCorpse", this.invert ? "NOT " : ""));
	}
}
