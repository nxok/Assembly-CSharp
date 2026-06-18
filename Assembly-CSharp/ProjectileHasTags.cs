using System;
using System.Xml.Linq;

public class ProjectileHasTags : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (_params.ItemValue.IsEmpty() || _params.ItemValue.ItemClass == null)
		{
			return false;
		}
		bool flag;
		if (!this.hasAllTags)
		{
			flag = _params.ItemValue.ItemClass.HasAnyTags(this.itemTags);
		}
		else
		{
			flag = _params.ItemValue.ItemClass.HasAllTags(this.itemTags);
		}
		if (!this.invert)
		{
			return flag;
		}
		return !flag;
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "tags")
			{
				this.itemTags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
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
	public FastTags<TagGroup.Global> itemTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAllTags;
}
