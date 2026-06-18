using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class IsStatAtMax : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		switch (this.stat)
		{
		case IsStatAtMax.StatTypes.Health:
			if (this.target.Stats.Health.Max - this.target.Stats.Health.Value < 0.1f)
			{
				return !this.invert;
			}
			return this.invert;
		case IsStatAtMax.StatTypes.Stamina:
			if (this.target.Stats.Stamina.Max - this.target.Stats.Stamina.Value < 0.1f)
			{
				return !this.invert;
			}
			return this.invert;
		case IsStatAtMax.StatTypes.Food:
			if (this.target.Stats.Food.Max - this.target.Stats.Food.Value < 0.1f)
			{
				return !this.invert;
			}
			return this.invert;
		case IsStatAtMax.StatTypes.Water:
			if (this.target.Stats.Water.Max - this.target.Stats.Water.Value < 0.1f)
			{
				return !this.invert;
			}
			return this.invert;
		default:
			return false;
		}
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "stat")
		{
			this.stat = EnumUtils.Parse<IsStatAtMax.StatTypes>(_attribute.Value, true);
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IsStatAtMax.StatTypes stat;

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum StatTypes
	{
		None,
		Health,
		Stamina,
		Food,
		Water
	}
}
