using System;
using System.Globalization;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class RewardLootItem : BaseReward
{
	public override void SetupReward()
	{
		this.LootGameStage = Convert.ToInt32(base.Value);
		ItemClass itemClass = this.Item.itemValue.ItemClass;
		base.Description = itemClass.GetLocalizedItemName();
		base.ValueText = base.Value;
		string text = itemClass.Groups[0].ToLower();
		uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
		if (num <= 1893056498U)
		{
			if (num <= 332883202U)
			{
				if (num != 46554813U)
				{
					if (num != 131445777U)
					{
						if (num == 332883202U)
						{
							if (text == "food/cooking")
							{
								base.Icon = "ui_game_symbol_fork";
							}
						}
					}
					else if (text == "books")
					{
						base.Icon = "ui_game_symbol_book";
					}
				}
				else if (text == "science")
				{
					base.Icon = "ui_game_symbol_science";
				}
			}
			else if (num != 481379405U)
			{
				if (num != 954139509U)
				{
					if (num == 1893056498U)
					{
						if (text == "resources")
						{
							base.Icon = "ui_game_symbol_resource";
						}
					}
				}
				else if (text == "building")
				{
					base.Icon = "ui_game_symbol_map_house";
				}
			}
			else if (text == "clothing")
			{
				base.Icon = "ui_game_symbol_shirt";
			}
		}
		else if (num <= 2154995914U)
		{
			if (num != 1917176822U)
			{
				if (num != 2115735777U)
				{
					if (num == 2154995914U)
					{
						if (text == "basics")
						{
							base.Icon = "ui_game_symbol_shopping_cart";
						}
					}
				}
				else if (text == "decor/miscellaneous")
				{
					base.Icon = "ui_game_symbol_chair";
				}
			}
			else if (text == "chemicals")
			{
				base.Icon = "ui_game_symbol_water";
			}
		}
		else if (num <= 3292735525U)
		{
			if (num != 2816984135U)
			{
				if (num == 3292735525U)
				{
					if (text == "ammo/weapons")
					{
						base.Icon = "ui_game_symbol_knife";
					}
				}
			}
			else if (text == "tools/traps")
			{
				base.Icon = "ui_game_symbol_tool";
			}
		}
		else if (num != 4134465488U)
		{
			if (num == 4185622628U)
			{
				if (text == "special items")
				{
					base.Icon = "ui_game_symbol_book";
				}
			}
		}
		else if (text == "mods")
		{
			base.Icon = "ui_game_symbol_assemble";
		}
		base.IconAtlas = "ItemIconAtlas";
	}

	public ItemStack Item
	{
		get
		{
			if (this.item == null || this.item.IsEmpty())
			{
				this.SetupItem();
			}
			return this.item;
		}
	}

	public override void Read(BinaryReader _br)
	{
		base.Read(_br);
		this.item = new ItemStack();
		this.item.Read(_br);
	}

	public override void Write(BinaryWriter _bw)
	{
		base.Write(_bw);
		if (this.item == null)
		{
			this.item = ItemStack.Empty;
		}
		this.item.Write(_bw);
	}

	public override void GiveReward(EntityPlayer player)
	{
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		if (!playerInventory.AddItem(this.Item))
		{
			playerInventory.DropItem(this.Item);
		}
	}

	public override ItemStack GetRewardItem()
	{
		return this.item.Clone();
	}

	public void SetupItem()
	{
		string[] array = base.ID.Split(',', StringSplitOptions.None);
		int i = 10;
		if (!string.IsNullOrEmpty(base.Value))
		{
			this.LootGameStage = StringParsers.ParseSInt32(base.Value, 0, -1, NumberStyles.Integer);
		}
		while (i > 0)
		{
			if (array.Length > 1)
			{
				World world = GameManager.Instance.World;
				this.item = LootContainer.GetRewardItem(array[world.GetGameRandom().RandomRange(array.Length)], (float)this.LootGameStage);
			}
			else if (array.Length == 1)
			{
				this.item = LootContainer.GetRewardItem(base.ID, (float)this.LootGameStage);
			}
			bool flag = false;
			for (int j = 0; j < base.OwnerQuest.Rewards.Count; j++)
			{
				RewardLootItem rewardLootItem = base.OwnerQuest.Rewards[j] as RewardLootItem;
				if (rewardLootItem != null)
				{
					if (rewardLootItem == this)
					{
						flag = true;
						break;
					}
					if (rewardLootItem.Item.itemValue.type == this.item.itemValue.type)
					{
						break;
					}
				}
			}
			if (flag)
			{
				break;
			}
			i--;
		}
		this.item.itemValue.UseTimes = 0f;
		this.item.itemValue.MaxDurabilityModifier = 1f;
	}

	public override BaseReward Clone()
	{
		RewardLootItem rewardLootItem = new RewardLootItem();
		base.CopyValues(rewardLootItem);
		rewardLootItem.LootGameStage = this.LootGameStage;
		if (this.item != null)
		{
			rewardLootItem.item = this.item.Clone();
		}
		return rewardLootItem;
	}

	public override string GetRewardText()
	{
		string localizedItemName = this.Item.itemValue.ItemClass.GetLocalizedItemName();
		if (this.Item.itemValue.HasQuality)
		{
			return localizedItemName;
		}
		if (this.Item.count <= 1)
		{
			return localizedItemName;
		}
		return string.Format("{0} ({1})", localizedItemName, this.Item.count);
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(RewardLootItem.PropLootTier))
		{
			base.Value = properties.Values[RewardLootItem.PropLootTier];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int LootGameStage = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack item;

	public static string PropLootTier = "loot_tier";
}
