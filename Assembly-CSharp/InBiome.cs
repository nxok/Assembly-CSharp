using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class InBiome : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (_params.Biome == null)
		{
			return false;
		}
		if (!this.invert)
		{
			return this.biomeID == (int)_params.Biome.m_Id;
		}
		return this.biomeID != (int)_params.Biome.m_Id;
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("{0}in biome {1}", this.invert ? "NOT " : "", this.biomeID));
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "biome")
		{
			this.biomeID = StringParsers.ParseSInt32(_attribute.Value, 0, -1, NumberStyles.Integer);
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomeID;
}
