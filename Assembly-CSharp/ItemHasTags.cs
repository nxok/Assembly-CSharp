using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class ItemHasTags : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		bool flag = false;
		if (!_params.ItemValue.IsEmpty() && _params.ItemValue.ItemClass != null)
		{
			if (!this.hasAllTags)
			{
				flag = _params.ItemValue.ItemClass.HasAnyTags(this.currentItemTags);
			}
			else
			{
				flag = _params.ItemValue.ItemClass.HasAllTags(this.currentItemTags);
			}
		}
		if (!this.invert)
		{
			return flag;
		}
		return !flag;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "tags")
			{
				this.currentItemTags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
				return true;
			}
			if (localName == "has_all_tags")
			{
				this.hasAllTags = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> currentItemTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAllTags;
}
