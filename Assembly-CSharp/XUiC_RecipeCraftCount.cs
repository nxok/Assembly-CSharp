using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeCraftCount : XUiC_Counter
{
	public override void Init()
	{
		base.Init();
		XUiC_RecipeList childByType = this.windowGroup.Controller.GetChildByType<XUiC_RecipeList>();
		if (childByType != null)
		{
			childByType.CraftCount = this;
			childByType.RecipeChanged += this.HandleRecipeChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleRecipeChanged(Recipe _recipe, XUiC_RecipeEntry recipeEntry)
	{
		if (this.recipe != _recipe)
		{
			this.recipe = _recipe;
			base.Count = 1;
			this.CalculateMaxCount();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleMaxCountOnPress(XUiController _sender, int _mouseButton)
	{
		base.Count = this.calcMaxCraftable();
		base.HandleCountChangedEvent();
	}

	public void CalculateMaxCount()
	{
		this.MaxCount = this.calcMaxCraftable();
		if (base.Count > this.MaxCount)
		{
			base.Count = this.MaxCount;
		}
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int calcMaxCraftable()
	{
		if (this.recipe == null)
		{
			return 1;
		}
		XUiC_WorkstationInputGrid childByType = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
		ItemStack[] array;
		if (childByType != null)
		{
			array = childByType.GetSlots();
		}
		else
		{
			array = this.xui.PlayerInventory.GetAllItemStacks().ToArray();
		}
		for (int i = 0; i < this.recipe.ingredients.Count; i++)
		{
			ItemStack itemStack = this.recipe.ingredients[i];
			if (itemStack != null && itemStack.itemValue.HasQuality)
			{
				return 1;
			}
		}
		int num = int.MaxValue;
		int num2 = ((this.recipe.craftingTier == -1) ? this.recipe.GetCraftingTier(this.xui.playerUI.entityPlayer) : this.recipe.craftingTier);
		for (int j = 0; j < this.recipe.ingredients.Count; j++)
		{
			ItemStack itemStack2 = this.recipe.ingredients[j];
			if (itemStack2 != null && itemStack2.itemValue.type != 0)
			{
				int count = itemStack2.count;
				float num3;
				if (this.recipe.UseIngredientModifier)
				{
					num3 = (float)((int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)count, this.xui.playerUI.entityPlayer, this.recipe, FastTags<TagGroup.Global>.Parse(itemStack2.itemValue.ItemClass.GetItemName()), true, true, true, true, true, num2, true, false));
					if (num3 > 0f)
					{
						num3 = (float)((int)(num3 * XUiM_Recipes.GetCraftingInputModifier(this.recipe)));
					}
				}
				else
				{
					num3 = (float)count;
				}
				if (num3 >= 1f)
				{
					int num4 = 0;
					for (int k = 0; k < array.Length; k++)
					{
						if (array[k] != null && array[k].itemValue.type != 0 && itemStack2.itemValue.type == array[k].itemValue.type)
						{
							num4 += array[k].count;
						}
					}
					int num5 = Mathf.CeilToInt((float)num4 / num3);
					if (Mathf.FloorToInt(num3 * (float)num5) > num4)
					{
						num5--;
					}
					num = Mathf.Min(num5, num);
					if (num == 0)
					{
						break;
					}
				}
			}
		}
		return Mathf.Clamp(num, 1, 10000);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.CalculateMaxCount();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.xui.PlayerInventory != null)
		{
			this.xui.PlayerInventory.OnBackpackItemsChanged += this.PlayerInventory_OnBackpackItemsChanged;
			this.xui.PlayerInventory.OnToolbeltItemsChanged += this.PlayerInventory_OnToolbeltItemsChanged;
			this.CalculateMaxCount();
		}
		((XUiV_Label)this.counter.ViewComponent).Text = (this.textInput.Text = base.Count.ToString());
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.PlayerInventory.OnBackpackItemsChanged -= this.PlayerInventory_OnBackpackItemsChanged;
		this.xui.PlayerInventory.OnToolbeltItemsChanged -= this.PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		this.CalculateMaxCount();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		this.CalculateMaxCount();
	}

	public void RefreshCounts()
	{
		this.CalculateMaxCount();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "enablecountdown")
		{
			value = (base.Count > 1).ToString();
			return true;
		}
		if (!(bindingName == "enablecountup"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = (base.Count < this.MaxCount).ToString();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxAllowed = 10000;

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;
}
