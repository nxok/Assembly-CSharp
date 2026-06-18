using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class RequirementItemTier : RequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (_params.ItemValue == null)
		{
			return false;
		}
		if (!this.invert)
		{
			return RequirementBase.compareValues((float)_params.ItemValue.Quality, this.operation, this.value);
		}
		return !RequirementBase.compareValues((float)_params.ItemValue.Quality, this.operation, this.value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Item tier {0}{1} {2}", this.invert ? "NOT " : "", this.operation.ToStringCached<RequirementBase.OperationTypes>(), this.value.ToCultureInvariantString()));
	}
}
