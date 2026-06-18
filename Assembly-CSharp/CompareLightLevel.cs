using System;
using System.Collections.Generic;

public class CompareLightLevel : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (this.target == null)
		{
			return false;
		}
		if (!this.invert)
		{
			return RequirementBase.compareValues(this.target.GetLightBrightness(), this.operation, this.value);
		}
		return !RequirementBase.compareValues(this.target.GetLightBrightness(), this.operation, this.value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("light level '{0}'% {1}{2} {3}", new object[]
		{
			this.target.GetLightBrightness().ToCultureInvariantString(),
			this.invert ? "NOT " : "",
			this.operation.ToStringCached<RequirementBase.OperationTypes>(),
			this.value.ToCultureInvariantString()
		}));
	}
}
