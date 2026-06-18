using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class BlockHasTags : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		bool flag = false;
		if (!_params.BlockValue.isair && _params.BlockValue.Block != null)
		{
			if (!this.hasAllTags)
			{
				flag = _params.BlockValue.Block.Tags.Test_AnySet(this.currentBlockTags);
			}
			else
			{
				flag = _params.BlockValue.Block.Tags.Test_AllSet(this.currentBlockTags);
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
				this.currentBlockTags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
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
	public FastTags<TagGroup.Global> currentBlockTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasAllTags;
}
