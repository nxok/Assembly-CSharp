using System;
using UnityEngine.Scripting;

[Preserve]
public class StatComparePercModMaxToMax : StatCompareAbs
{
	public override bool Compare(MinEventParams _params)
	{
		StatCompareAbs.StatTypes stat = this.stat;
		if (stat != StatCompareAbs.StatTypes.Health)
		{
			return stat == StatCompareAbs.StatTypes.Stamina && this.invert != RequirementBase.compareValues(base.Stats.Stamina.ModifiedMaxPercent, this.operation, this.value);
		}
		return this.invert != RequirementBase.compareValues(base.Stats.Health.ModifiedMaxPercent, this.operation, this.value);
	}
}
