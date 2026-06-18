using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsMale : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!this.invert)
		{
			return this.target.IsMale;
		}
		return !this.target.IsMale;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Is {0}Male", this.invert ? "NOT " : ""));
	}
}
