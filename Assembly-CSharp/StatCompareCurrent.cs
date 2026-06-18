using System;
using UnityEngine.Scripting;

[Preserve]
public class StatCompareCurrent : StatCompareAbs
{
	public override bool Compare(MinEventParams _params)
	{
		float num;
		switch (this.stat)
		{
		case StatCompareAbs.StatTypes.Health:
			num = base.Stats.Health.Value;
			break;
		case StatCompareAbs.StatTypes.Stamina:
			num = base.Stats.Stamina.Value;
			break;
		case StatCompareAbs.StatTypes.Food:
			num = base.Stats.Food.Value;
			break;
		case StatCompareAbs.StatTypes.Water:
			num = base.Stats.Water.Value;
			break;
		case StatCompareAbs.StatTypes.Armor:
			num = this.target.equipment.CurrentLowestDurability;
			break;
		default:
			return false;
		}
		return this.invert != RequirementBase.compareValues(num, this.operation, this.value);
	}
}
