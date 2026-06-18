using System;
using UnityEngine.Scripting;

[Preserve]
public class IsEquipped : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (base.IsValid(_params))
		{
			ItemValue itemValue = _params.ItemValue;
			if (itemValue != null && this.target != null)
			{
				if (itemValue.IsMod)
				{
					foreach (ItemValue itemValue2 in this.target.equipment.GetItems())
					{
						if (itemValue2 != null && itemValue2.HasModSlots)
						{
							foreach (ItemValue itemValue3 in itemValue2.Modifications)
							{
								if (itemValue3 != null && itemValue3 == _params.ItemValue)
								{
									return true;
								}
							}
						}
					}
				}
				else
				{
					ItemValue[] array = this.target.equipment.GetItems();
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] == itemValue)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}
}
