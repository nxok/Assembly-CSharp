using System;
using System.Collections.Generic;

public class Recipe
{
	public bool IsLearnable
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public List<ItemStack> GetIngredientsSummedUp()
	{
		return this.ingredients;
	}

	public void Init()
	{
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < this.ingredients.Count; i++)
		{
			ItemStack itemStack = this.ingredients[i];
			if (itemStack.itemValue.ItemClass != null)
			{
				num += itemStack.itemValue.ItemClass.CraftComponentExp * (float)itemStack.count;
				num2 += itemStack.itemValue.ItemClass.CraftComponentTime * (float)itemStack.count;
			}
		}
		if (this.unlockExpGain < 0)
		{
			this.unlockExpGain = (int)(num * 2f);
		}
		if (this.craftExpGain < 0)
		{
			this.craftExpGain = (int)num;
		}
		if (this.craftingTime < 0f)
		{
			this.craftingTime = num2;
		}
		this.IsLearnable = this.tags.Test_AnySet(Recipe.LearnableRecipe);
	}

	public void AddIngredient(ItemValue _itemValue, int _count)
	{
		for (int i = 0; i < this.ingredients.Count - 1; i++)
		{
			if (this.ingredients[i].itemValue.type == _itemValue.type)
			{
				this.ingredients[i].count += _count;
				return;
			}
		}
		this.ingredients.Add(new ItemStack(_itemValue, _count));
	}

	public void AddIngredients(List<ItemStack> _items)
	{
		this.ingredients.AddRange(_items);
	}

	public string GetName()
	{
		ItemClass forId = ItemClass.GetForId(this.itemValueType);
		if (forId == null)
		{
			return string.Empty;
		}
		return forId.GetItemName();
	}

	public string GetIcon()
	{
		ItemClass forId = ItemClass.GetForId(this.itemValueType);
		if (forId == null)
		{
			return string.Empty;
		}
		return forId.GetIconName();
	}

	public bool CanCraft(IList<ItemStack> _itemStack, EntityAlive _ea = null, int _craftingTier = -1)
	{
		this.craftingTier = this.GetCraftingTier((EntityPlayer)_ea);
		if (_craftingTier != -1 && _craftingTier < this.craftingTier)
		{
			this.craftingTier = _craftingTier;
		}
		for (int i = 0; i < this.ingredients.Count; i++)
		{
			ItemStack itemStack = this.ingredients[i];
			int num = itemStack.count;
			if (this.UseIngredientModifier)
			{
				num = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)itemStack.count, _ea, this, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), true, true, true, true, true, this.craftingTier, true, false);
				if (num > 0)
				{
					num = (int)((float)num * XUiM_Recipes.GetCraftingInputModifier(this));
				}
			}
			if (num != 0)
			{
				int num2 = 0;
				while (num > 0 && num2 < _itemStack.Count)
				{
					if ((!_itemStack[num2].itemValue.HasModSlots || !_itemStack[num2].itemValue.HasMods()) && _itemStack[num2].itemValue.type == itemStack.itemValue.type)
					{
						num -= _itemStack[num2].count;
					}
					num2++;
				}
				if (num > 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool CanCraftAny(IList<ItemStack> _itemStack, EntityAlive _ea = null)
	{
		for (int i = this.GetCraftingTier((EntityPlayer)_ea); i >= 0; i--)
		{
			bool flag = true;
			for (int j = 0; j < this.ingredients.Count; j++)
			{
				ItemStack itemStack = this.ingredients[j];
				int num = itemStack.count;
				if (this.UseIngredientModifier)
				{
					num = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)itemStack.count, _ea, this, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), true, true, true, true, true, i, true, false);
					if (num > 0)
					{
						num = (int)((float)num * XUiM_Recipes.GetCraftingInputModifier(this));
					}
				}
				if (num != 0)
				{
					int num2 = 0;
					while (num > 0 && num2 < _itemStack.Count)
					{
						if ((!_itemStack[num2].itemValue.HasModSlots || !_itemStack[num2].itemValue.HasMods()) && _itemStack[num2].itemValue.type == itemStack.itemValue.type)
						{
							num -= _itemStack[num2].count;
						}
						num2++;
					}
					if (num > 0)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsUnlocked(EntityPlayer _ep)
	{
		return !XUiM_Recipes.CraftingProgression || !this.IsLearnable || EffectManager.GetValue(PassiveEffects.RecipeTagUnlocked, null, _ep.GetCVar(this.GetName()), _ep, null, this.tags, true, true, true, true, true, 1, true, false) > 0f;
	}

	public int GetCraftingTier(EntityPlayer _ep)
	{
		if (!XUiM_Recipes.CraftingProgression)
		{
			return 6;
		}
		return (int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, _ep, this, this.tags, true, true, true, true, true, 1, true, false);
	}

	public ItemClass GetOutputItemClass()
	{
		return ItemClass.GetForId(this.itemValueType);
	}

	public bool ContainsIngredients(ItemValue[] _items)
	{
		for (int i = 0; i < this.ingredients.Count; i++)
		{
			for (int j = 0; j < _items.Length; j++)
			{
				if (this.ingredients[i].itemValue.type == _items[j].type)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool CanBeCraftedWith(Dictionary<ItemValue, int> _items)
	{
		return false;
	}

	public override bool Equals(object _other)
	{
		return _other.GetHashCode() == this.GetHashCode();
	}

	public override int GetHashCode()
	{
		if (!this.hashCodeSetup)
		{
			int num = 0;
			for (int i = 0; i < this.ingredients.Count; i++)
			{
				num += this.ingredients[i].count;
			}
			this.hashcode = string.Concat(new string[]
			{
				this.itemValueType.ToString(),
				"_",
				this.craftingArea,
				"_",
				num.ToString()
			}).GetHashCode();
			this.hashCodeSetup = true;
		}
		return this.hashcode;
	}

	public override string ToString()
	{
		return string.Format("[Recipe: " + this.GetName() + "]", Array.Empty<object>());
	}

	public void ModifyValue(PassiveEffects _passiveEffect, ref float _base_val, ref float _perc_val, FastTags<TagGroup.Global> tags, int _craftingTier = 1)
	{
		if (this.Effects != null)
		{
			this.Effects.ModifyValue(null, _passiveEffect, ref _base_val, ref _perc_val, (float)_craftingTier, tags, 1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte Version = 3;

	public static FastTags<TagGroup.Global> MaterialBased = FastTags<TagGroup.Global>.Parse("materialbased");

	public static FastTags<TagGroup.Global> LearnableRecipe = FastTags<TagGroup.Global>.Parse("learnable");

	public int itemValueType;

	public int count;

	public bool scrapable;

	public List<ItemStack> ingredients = new List<ItemStack>();

	public bool wildcardForgeCategory;

	public bool wildcardCampfireCategory;

	public bool materialBasedRecipe;

	public int craftingToolType;

	public float craftingTime;

	public string craftingArea;

	public string tooltip;

	public int unlockExpGain;

	public int craftExpGain;

	public bool UseIngredientModifier = true;

	public FastTags<TagGroup.Global> tags;

	public bool IsTrackable = true;

	public bool isQuest;

	public bool isChallenge;

	public bool IsTracked;

	public bool IsScrap;

	public int craftingTier = -1;

	public MinEffectController Effects;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hashcode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hashCodeSetup;
}
