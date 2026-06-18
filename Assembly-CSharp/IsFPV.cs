using System;
using UnityEngine.Scripting;

[Preserve]
public class IsFPV : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!(this.target as EntityPlayerLocal != null))
		{
			return this.invert;
		}
		if (!this.invert)
		{
			return (this.target as EntityPlayerLocal).bFirstPersonView;
		}
		return !(this.target as EntityPlayerLocal).bFirstPersonView;
	}
}
