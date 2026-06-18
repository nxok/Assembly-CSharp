using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class PerksUnlocked : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (this.skill_name == null)
		{
			return false;
		}
		ProgressionValue progressionValue = this.target.Progression.GetProgressionValue(this.skill_name);
		int num = 0;
		for (int i = 0; i < progressionValue.ProgressionClass.Children.Count; i++)
		{
			num += this.target.Progression.GetProgressionValue(progressionValue.ProgressionClass.Children[i].Name).Level;
		}
		if (this.invert)
		{
			return !RequirementBase.compareValues((float)num, this.operation, this.value);
		}
		return RequirementBase.compareValues((float)num, this.operation, this.value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("perks unlocked count {0}{1} {2}", this.invert ? "NOT " : "", this.operation.ToStringCached<RequirementBase.OperationTypes>(), this.value.ToCultureInvariantString()));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "skill_name")
		{
			this.skill_name = _attribute.Value;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string skill_name;
}
