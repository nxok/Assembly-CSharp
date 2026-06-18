using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsBloodMoon : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		return base.IsValid(_params) && (SkyManager.IsBloodMoonVisible() ^ this.invert);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Is {0} blood moon", this.invert ? "NOT " : ""));
	}
}
