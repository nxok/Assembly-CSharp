using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class RecipeUnlocked : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (this.item_name == null)
		{
			return false;
		}
		List<Recipe> nonScrapableRecipes = CraftingManager.GetNonScrapableRecipes(this.item_name);
		bool flag = false;
		for (int i = 0; i < nonScrapableRecipes.Count; i++)
		{
			if (nonScrapableRecipes[i].IsUnlocked(this.target as EntityPlayer))
			{
				flag = true;
				break;
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
		list.Add(string.Format("is recipe {0} {1} unlocked", this.item_name, this.invert ? "NOT " : ""));
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
}
