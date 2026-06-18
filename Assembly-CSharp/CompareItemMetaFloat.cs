using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class CompareItemMetaFloat : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		ItemValue itemValue = _params.ItemValue;
		if (itemValue == null || string.IsNullOrEmpty(this.metaKey))
		{
			return false;
		}
		float num;
		if (!itemValue.TryGetMetadata(this.metaKey, out num))
		{
			return false;
		}
		if (this.invert)
		{
			return !RequirementBase.compareValues(num, this.operation, this.value);
		}
		return RequirementBase.compareValues(num, this.operation, this.value);
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "key")
		{
			this.metaKey = _attribute.Value;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string metaKey;
}
