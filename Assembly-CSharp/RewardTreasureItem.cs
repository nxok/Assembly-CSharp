using System;
using UnityEngine.Scripting;

[Preserve]
public class RewardTreasureItem : BaseReward
{
	public override void SetupReward()
	{
		base.Description = ItemClass.GetItemClass(base.ID, false).Name;
		base.ValueText = base.Value;
		base.Icon = "ui_game_symbol_hand";
		base.IconAtlas = "ItemIconAtlas";
	}

	public override void GiveReward(EntityPlayer player)
	{
		if (base.OwnerQuest == null)
		{
			return;
		}
		ItemValue item = ItemClass.GetItem(base.ID, false);
		ItemStack empty = ItemStack.Empty;
		ItemValue itemValue = new ItemValue(item.type, true);
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
			else if (base.Value.Contains("-"))
			{
				string[] array = base.Value.Split('-', StringSplitOptions.None);
				int num2 = Convert.ToInt32(array[0]);
				int num3 = Convert.ToInt32(array[1]);
				if (itemValue.HasQuality)
				{
					itemValue = new ItemValue(item.type, num2, num3, true, null, 1f);
					num = 1;
				}
				else
				{
					WorldBase world = GameManager.Instance.World;
					itemValue = new ItemValue(item.type, true);
					num = world.GetGameRandom().RandomRange(num2, num3);
				}
			}
		}
		string[] array2 = base.OwnerQuest.DataVariables["treasurecontainer"].Split(',', StringSplitOptions.None);
		Vector3i zero = Vector3i.zero;
		if (array2.Length == 3)
		{
			zero = new Vector3i(Convert.ToInt32(array2[0]), Convert.ToInt32(array2[1]), Convert.ToInt32(array2[2]));
			GameManager.Instance.World.GetTileEntity(zero).GetSelfOrFeature<TEFeatureStorage>().AddItem(new ItemStack(itemValue, num));
		}
	}

	public override BaseReward Clone()
	{
		RewardTreasureItem rewardTreasureItem = new RewardTreasureItem();
		base.CopyValues(rewardTreasureItem);
		return rewardTreasureItem;
	}
}
