using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class DialogActionAddItem : BaseDialogAction
{
	public override BaseDialogAction.ActionTypes ActionType
	{
		get
		{
			return BaseDialogAction.ActionTypes.AddItem;
		}
	}

	public override void PerformAction(EntityPlayer player)
	{
		ItemValue item = ItemClass.GetItem(base.ID, false);
		ItemValue itemValue = new ItemValue(ItemClass.GetItem(base.ID, false).type, true);
		int num = 1;
		if (base.Value != null && base.Value != "")
		{
			if (int.TryParse(base.Value, out num))
			{
				if (itemValue.HasQuality)
				{
					itemValue = new ItemValue(item.type, num, num, true, null, 1f);
					num = 1;
				}
				else
				{
					itemValue = new ItemValue(item.type, true);
				}
			}
			else if (base.Value.Contains(","))
			{
				string[] array = base.Value.Split(',', StringSplitOptions.None);
				int num2 = Convert.ToInt32(array[0]);
				int num3 = Convert.ToInt32(array[1]);
				if (itemValue.HasQuality)
				{
					itemValue = new ItemValue(item.type, num2, num3, true, null, 1f);
					num = 1;
				}
				else
				{
					itemValue = new ItemValue(item.type, true);
					num = Random.Range(num2, num3);
				}
			}
		}
		LocalPlayerUI.primaryUI.xui.PlayerInventory.AddItem(new ItemStack(itemValue, num));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";
}
