using System;
using System.Collections.Generic;

public class NPCIsAlert : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!this.target.IsAlive())
		{
			return false;
		}
		if (this.invert)
		{
			return !this.target.IsAlert;
		}
		return this.target.IsAlert;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Entity {0}Alert", this.invert ? "NOT " : ""));
	}
}
