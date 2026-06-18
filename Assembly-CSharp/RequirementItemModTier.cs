using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class RequirementItemModTier : RequirementBase
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
		if (!_params.ItemValue.HasModSlots)
		{
			return false;
		}
		ItemValue itemValue = ItemValue.None;
		for (int i = 0; i < _params.ItemValue.Modifications.Length; i++)
		{
			ItemValue itemValue2 = _params.ItemValue.Modifications[i];
			if (itemValue2 != ItemValue.None && itemValue2.ItemClass != null && itemValue2.ItemClass.Name != null && itemValue2.ItemClass.Name.EqualsCaseInsensitive(this.modName))
			{
				itemValue = itemValue2;
				break;
			}
		}
		if (!this.invert)
		{
			return RequirementBase.compareValues((float)itemValue.Quality, this.operation, this.value);
		}
		return !RequirementBase.compareValues((float)itemValue.Quality, this.operation, this.value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Item Mod tier {0}{1} {2}", this.invert ? "NOT " : "", this.operation.ToStringCached<RequirementBase.OperationTypes>(), this.value.ToCultureInvariantString()));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "mod_name")
		{
			this.modName = _attribute.Value;
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string modName;
}
