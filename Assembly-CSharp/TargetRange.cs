using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class TargetRange : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (_params.ItemValue.IsEmpty())
		{
			return false;
		}
		if (!(_params.Self != null) || !(_params.Other != null))
		{
			return false;
		}
		if (!(_params.Self != _params.Other))
		{
			return false;
		}
		if (this.invert)
		{
			return !RequirementBase.compareValues(_params.Self.GetDistance(_params.Other), this.operation, this.value);
		}
		return RequirementBase.compareValues(_params.Self.GetDistance(_params.Other), this.operation, this.value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("TargetRange: {0}{1} {2}", this.invert ? "NOT " : "", this.operation.ToStringCached<RequirementBase.OperationTypes>(), this.value.ToCultureInvariantString()));
	}
}
