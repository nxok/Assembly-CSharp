using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class IsSheltered : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		EntityPlayerLocal entityPlayerLocal = this.target as EntityPlayerLocal;
		return entityPlayerLocal != null && this.invert != entityPlayerLocal.shelterPercent > 0f;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("{0}sheltered", this.invert ? "NOT " : ""));
	}
}
