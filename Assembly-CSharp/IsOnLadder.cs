using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsOnLadder : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!this.invert)
		{
			return this.target.IsInElevator();
		}
		return !this.target.IsInElevator();
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Is {0} On Ladder", this.invert ? "NOT " : ""));
	}
}
