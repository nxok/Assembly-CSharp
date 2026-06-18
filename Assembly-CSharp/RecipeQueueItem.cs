using System;
using System.Collections.Generic;
using System.IO;

public class RecipeQueueItem
{
	public Recipe Recipe
	{
		get
		{
			if (this.cachedRecipe == null)
			{
				return this.cachedRecipe = CraftingManager.GetRecipe(this.recipeHashCode);
			}
			return this.cachedRecipe;
		}
		set
		{
			this.cachedRecipe = value;
			this.recipeHashCode = ((this.cachedRecipe != null) ? this.cachedRecipe.GetHashCode() : 0);
		}
	}

	public void Write(BinaryWriter _bw, uint version)
	{
		bool flag = this.Recipe != null;
		_bw.Write(flag ? this.Recipe.GetHashCode() : 0);
		_bw.Write(this.Multiplier);
		_bw.Write(this.IsCrafting);
		_bw.Write(this.CraftingTimeLeft);
		bool flag2 = this.RepairItem != null;
		_bw.Write(flag2);
		if (flag2)
		{
			this.RepairItem.Write(_bw);
			_bw.Write(this.AmountToRepair);
		}
		_bw.Write(this.Quality);
		_bw.Write(this.StartingEntityId);
		_bw.Write(this.OneItemCraftTime);
		_bw.Write(this.Recipe != null && this.Recipe.scrapable);
		if (this.Recipe != null && this.Recipe.scrapable)
		{
			_bw.Write(this.Recipe.itemValueType);
			_bw.Write(this.Recipe.count);
			_bw.Write(this.Recipe.ingredients.Count);
			for (int i = 0; i < this.Recipe.ingredients.Count; i++)
			{
				this.Recipe.ingredients[i].Write(_bw);
			}
			_bw.Write(this.Recipe.craftingTime);
			_bw.Write(this.Recipe.craftExpGain);
			_bw.Write(this.Recipe.IsScrap);
		}
		if (flag)
		{
			ItemClass outputItemClass = this.Recipe.GetOutputItemClass();
			NameIdMapping nameIdMapping = (outputItemClass.IsBlock() ? Block.nameIdMapping : ItemClass.nameIdMapping);
			if (nameIdMapping == null)
			{
				return;
			}
			nameIdMapping.AddMapping(outputItemClass.Id, outputItemClass.Name, false);
		}
	}

	public void Read(BinaryReader _br, uint version)
	{
		this.recipeHashCode = _br.ReadInt32();
		this.cachedRecipe = CraftingManager.GetRecipe(this.recipeHashCode);
		this.Multiplier = _br.ReadInt16();
		this.IsCrafting = _br.ReadBoolean();
		this.CraftingTimeLeft = _br.ReadSingle();
		if (_br.ReadBoolean())
		{
			if (version > 39U)
			{
				this.RepairItem = ItemValue.ReadOrNull(_br);
			}
			else
			{
				this.RepairItem = new ItemValue(_br.ReadInt32(), false);
			}
			this.AmountToRepair = _br.ReadUInt16();
		}
		if (version > 0U)
		{
			this.Quality = _br.ReadByte();
			this.StartingEntityId = _br.ReadInt32();
		}
		if (version > 41U)
		{
			this.OneItemCraftTime = _br.ReadSingle();
		}
		if (version > 43U && _br.ReadBoolean())
		{
			this.cachedRecipe = new Recipe();
			this.cachedRecipe.itemValueType = _br.ReadInt32();
			this.cachedRecipe.count = _br.ReadInt32();
			this.cachedRecipe.scrapable = true;
			int num = _br.ReadInt32();
			this.Recipe.ingredients = new List<ItemStack>();
			for (int i = 0; i < num; i++)
			{
				this.Recipe.ingredients.Add(new ItemStack().Read(_br));
			}
			this.cachedRecipe.craftingTime = _br.ReadSingle();
			this.cachedRecipe.craftExpGain = _br.ReadInt32();
			if (version > 46U)
			{
				this.cachedRecipe.IsScrap = _br.ReadBoolean();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void WriteDelta(BinaryWriter _bw, RecipeQueueItem _last)
	{
		_bw.Write((this.Recipe != null) ? this.Recipe.GetHashCode() : 0);
		if (this.Multiplier < 0)
		{
			Log.Error("Multiplier is less than 0!");
			Log.Out(Environment.StackTrace);
			this.Multiplier = 0;
		}
		_bw.Write(this.CraftingTimeLeft - _last.CraftingTimeLeft);
		_last.CraftingTimeLeft += this.CraftingTimeLeft - _last.CraftingTimeLeft;
		_bw.Write(this.Multiplier - _last.Multiplier);
		_last.Multiplier += this.Multiplier - _last.Multiplier;
		_bw.Write(this.IsCrafting);
		bool flag = this.RepairItem != null;
		_bw.Write(flag);
		if (flag)
		{
			this.RepairItem.Write(_bw);
			_bw.Write(this.AmountToRepair);
		}
		_bw.Write(this.Quality);
		_bw.Write(this.StartingEntityId);
		_bw.Write(this.OneItemCraftTime);
		_bw.Write(this.Recipe != null && this.Recipe.scrapable);
		if (this.Recipe != null && this.Recipe.scrapable)
		{
			_bw.Write(this.Recipe.itemValueType);
			_bw.Write(this.Recipe.count);
			_bw.Write(this.Recipe.scrapable);
			_bw.Write(this.Recipe.ingredients.Count);
			for (int i = 0; i < this.Recipe.ingredients.Count; i++)
			{
				this.Recipe.ingredients[i].Write(_bw);
			}
			_bw.Write(this.Recipe.craftingTime);
			_bw.Write(this.Recipe.craftExpGain);
			_bw.Write(this.Recipe.IsScrap);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void ReadDelta(BinaryReader _br, RecipeQueueItem _last)
	{
		this.recipeHashCode = _br.ReadInt32();
		this.cachedRecipe = CraftingManager.GetRecipe(this.recipeHashCode);
		float num = _br.ReadSingle();
		this.CraftingTimeLeft = _last.CraftingTimeLeft + num;
		int num2 = (int)_br.ReadInt16();
		this.Multiplier = (short)((int)_last.Multiplier + num2);
		this.IsCrafting = _br.ReadBoolean();
		if (_br.ReadBoolean())
		{
			this.RepairItem = ItemValue.ReadOrNull(_br);
			this.AmountToRepair = _br.ReadUInt16();
		}
		this.Quality = _br.ReadByte();
		this.StartingEntityId = _br.ReadInt32();
		this.OneItemCraftTime = _br.ReadSingle();
		if (_br.ReadBoolean())
		{
			this.cachedRecipe = new Recipe();
			this.cachedRecipe.itemValueType = _br.ReadInt32();
			this.cachedRecipe.count = _br.ReadInt32();
			this.cachedRecipe.scrapable = true;
			int num3 = _br.ReadInt32();
			this.Recipe.ingredients = new List<ItemStack>();
			for (int i = 0; i < num3; i++)
			{
				this.Recipe.ingredients.Add(new ItemStack().Read(_br));
			}
			this.cachedRecipe.craftingTime = _br.ReadSingle();
			this.cachedRecipe.craftExpGain = _br.ReadInt32();
			this.cachedRecipe.IsScrap = _br.ReadBoolean();
		}
	}

	public short Multiplier;

	public float CraftingTimeLeft;

	public float OneItemCraftTime = -1f;

	public bool IsCrafting;

	public ItemValue RepairItem;

	public ushort AmountToRepair;

	public byte Quality;

	public int StartingEntityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int recipeHashCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe cachedRecipe;
}
