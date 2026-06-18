using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class BlockStandingOn : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		bool flag;
		if (this.hasAllTags)
		{
			flag = this.target.blockValueStandingOn.Block.HasAllFastTags(this.blockTags);
		}
		else
		{
			flag = this.target.blockValueStandingOn.Block.HasAnyFastTags(this.blockTags);
		}
		if (!this.invert)
		{
			return flag;
		}
		return !flag;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Is {0}Male", this.invert ? "NOT " : ""));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (localName == "tags")
			{
				this.blockTags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
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
	public FastTags<TagGroup.Global> blockTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAllTags;
}
