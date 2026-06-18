using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class InSafeZone : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		EntityPlayer entityPlayer = this.target as EntityPlayer;
		if (entityPlayer == null)
		{
			return false;
		}
		if (!this.invert)
		{
			return entityPlayer.TwitchSafe;
		}
		return !entityPlayer.TwitchSafe;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Is {0} In Safe Zone", this.invert ? "NOT " : ""));
	}
}
