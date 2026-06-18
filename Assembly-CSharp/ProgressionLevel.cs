using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class ProgressionLevel : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (this.target.Progression != null)
		{
			this.pv = this.target.Progression.GetProgressionValue(this.progressionId);
			if (this.pv != null)
			{
				if (this.invert)
				{
					return !RequirementBase.compareValues(this.pv.GetCalculatedLevel(this.target), this.operation, this.value);
				}
				return RequirementBase.compareValues(this.pv.GetCalculatedLevel(this.target), this.operation, this.value);
			}
		}
		return false;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("'{1}' level {0} {2} {3}", new object[]
		{
			this.invert ? "NOT" : "",
			this.progressionName,
			this.operation.ToStringCached<RequirementBase.OperationTypes>(),
			this.value.ToCultureInvariantString()
		}));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "progression_name")
		{
			this.progressionName = _attribute.Value;
			this.progressionId = Progression.CalcId(this.progressionName);
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string progressionName = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public int progressionId;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionValue pv;
}
