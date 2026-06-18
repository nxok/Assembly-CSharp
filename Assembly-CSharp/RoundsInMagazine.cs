using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class RoundsInMagazine : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (_params.ItemValue.IsEmpty() || !(_params.ItemValue.ItemClass.Actions[0] is ItemActionRanged))
		{
			return false;
		}
		if (this.invert)
		{
			return !RequirementBase.compareValues((float)_params.ItemValue.Meta, this.operation, this.value);
		}
		return RequirementBase.compareValues((float)_params.ItemValue.Meta, this.operation, this.value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Rounds in Magazine: {0}{1} {2}", this.invert ? "NOT " : "", this.operation.ToStringCached<RequirementBase.OperationTypes>(), this.value.ToCultureInvariantString()));
	}
}
