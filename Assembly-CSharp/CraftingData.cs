using System;
using System.IO;

public class CraftingData
{
	public CraftingData()
	{
		this.items = new ItemStack[0];
		this.outputItems = new ItemStack[0];
		this.breakDownType = CraftingData.BreakdownType.None;
		this.RecipeQueueItems = new RecipeQueueItem[0];
	}

	public void Write(BinaryWriter _bw)
	{
		int num = this.RecipeQueueItems.Length;
		_bw.Write((byte)num);
		for (int i = 0; i < num; i++)
		{
			if (this.RecipeQueueItems[i] == null)
			{
				this.RecipeQueueItems[i] = new RecipeQueueItem();
			}
			this.RecipeQueueItems[i].Write(_bw, 0U);
		}
	}

	public void Read(BinaryReader _br, uint _version = 100U)
	{
		int num = (int)_br.ReadByte();
		this.RecipeQueueItems = new RecipeQueueItem[num];
		for (int i = 0; i < num; i++)
		{
			this.RecipeQueueItems[i] = new RecipeQueueItem();
			this.RecipeQueueItems[i].Read(_br, _version);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 100;

	public ItemStack[] items;

	public ItemStack[] outputItems;

	public bool isCrafting;

	public ulong lastWorldTick;

	public int totalLeftToCraft;

	public float currentRecipeTimer;

	public Recipe currentRecipeToCraft;

	public Recipe lastRecipeToCraft;

	public ItemValue repairedItem;

	public CraftingData.BreakdownType breakDownType;

	public bool isItemPlacedByUser;

	public ulong savedWorldTick;

	public RecipeQueueItem[] RecipeQueueItems;

	public enum BreakdownType
	{
		None,
		Part,
		Recipe
	}
}
