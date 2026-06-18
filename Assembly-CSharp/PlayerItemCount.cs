using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class PlayerItemCount : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (this.item_name != null && this.item == null)
		{
			this.item = ItemClass.GetItem(this.item_name, true);
		}
		if (this.item == null)
		{
			return false;
		}
		int num = this.target.inventory.GetItemCount(this.item, false, -1, -1, true);
		num += this.target.bag.GetItemCount(this.item, -1, -1, true);
		if (this.invert)
		{
			return !RequirementBase.compareValues((float)num, this.operation, this.value);
		}
		return RequirementBase.compareValues((float)num, this.operation, this.value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Item count {0}{1} {2}", this.invert ? "NOT " : "", this.operation.ToStringCached<RequirementBase.OperationTypes>(), this.value.ToCultureInvariantString()));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "item_name")
		{
			this.item_name = _attribute.Value;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string item_name;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue item;
}
