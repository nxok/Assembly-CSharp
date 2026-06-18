using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class TimeOfDay : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (!this.isSetup)
		{
			this.timeValue = GameUtils.DayTimeToWorldTime(1, (int)this.value / 100, (int)this.value % 100);
			this.isSetup = true;
		}
		ulong num = GameManager.Instance.World.worldTime % 24000UL;
		if (this.invert)
		{
			return !RequirementBase.compareValues(num, this.operation, this.timeValue);
		}
		return RequirementBase.compareValues(num, this.operation, this.timeValue);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("time of day {0}{1} {2}", this.invert ? "NOT " : "", this.operation.ToStringCached<RequirementBase.OperationTypes>(), this.value.ToCultureInvariantString()));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSetup;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong timeValue;
}
