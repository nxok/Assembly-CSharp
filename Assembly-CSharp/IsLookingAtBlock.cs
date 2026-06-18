using System;
using System.Xml.Linq;

public class IsLookingAtBlock : RequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		return base.IsValid(_params);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void raycast()
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
				this.tagsToCompare = FastTags<TagGroup.Global>.Parse(_attribute.Value);
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
	public FastTags<TagGroup.Global> tagsToCompare;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAllTags;
}
