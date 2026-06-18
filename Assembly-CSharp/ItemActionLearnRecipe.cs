using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionLearnRecipe : ItemAction
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionLearnRecipe.MyInventoryData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (!_props.Values.ContainsKey("Recipes_to_learn"))
		{
			this.RecipesToLearn = new string[0];
		}
		else
		{
			this.RecipesToLearn = _props.Values["Recipes_to_learn"].Replace(" ", "").Split(',', StringSplitOptions.None);
		}
		if (!_props.Values.ContainsKey("Title"))
		{
			this.Title = "The title is impossible to read.";
		}
		else
		{
			this.Title = _props.Values["Title"];
		}
		if (!_props.Values.ContainsKey("Description"))
		{
			this.Description = "The description is impossible to read.";
		}
		else
		{
			this.Description = _props.Values["Description"];
		}
		for (int i = 0; i < this.RecipesToLearn.Length; i++)
		{
			CraftingManager.LockRecipe(this.RecipesToLearn[i], CraftingManager.RecipeLockTypes.Item);
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		((ItemActionLearnRecipe.MyInventoryData)_data).bReadingStarted = false;
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (!_bReleased)
		{
			return;
		}
		if (Time.time - _actionData.lastUseTime < this.Delay)
		{
			return;
		}
		_actionData.lastUseTime = Time.time;
		_actionData.invData.holdingEntity.RightArmAnimationUse = true;
		((ItemActionLearnRecipe.MyInventoryData)_actionData).bReadingStarted = true;
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		ItemActionLearnRecipe.MyInventoryData myInventoryData = (ItemActionLearnRecipe.MyInventoryData)_actionData;
		return myInventoryData.bReadingStarted && Time.time - myInventoryData.lastUseTime < 2f * AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		ItemActionLearnRecipe.MyInventoryData myInventoryData = (ItemActionLearnRecipe.MyInventoryData)_actionData;
		if (!myInventoryData.bReadingStarted || Time.time - myInventoryData.lastUseTime < AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast)
		{
			return;
		}
		myInventoryData.bReadingStarted = false;
		bool flag = false;
		for (int i = 0; i < this.RecipesToLearn.Length; i++)
		{
			if (CraftingManager.GetRecipe(this.RecipesToLearn[i]).tags.Equals(FastTags<TagGroup.Global>.Parse("learnable")) && myInventoryData.invData.holdingEntity.GetCVar(this.RecipesToLearn[i]) == 0f)
			{
				flag = true;
				myInventoryData.invData.holdingEntity.SetCVar(this.RecipesToLearn[i], 1f);
				GameManager.ShowTooltip(entityPlayerLocal, string.Format(Localization.Get("ttRecipeUnlocked", false, null), Localization.Get(this.RecipesToLearn[i], false, null)), false, false, 0f);
			}
		}
		if (flag)
		{
			_actionData.invData.holdingEntity.inventory.DecHoldingItem(1);
			if (this.soundStart != null)
			{
				Manager.PlayInsidePlayerHead(this.soundStart, -1, 0f, false, false);
				return;
			}
		}
		else
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("alreadyKnown", false, null), false, false, 0f);
		}
	}

	public override void GetItemValueActionInfo(ref List<string> _infoList, ItemValue _itemValue, XUi _xui, int _actionIndex = 0)
	{
		for (int i = 0; i < this.RecipesToLearn.Length; i++)
		{
			if (!XUiM_Recipes.GetRecipeIsUnlocked(_xui, this.RecipesToLearn[i]))
			{
				_infoList.Add(ItemAction.StringFormatHandler(Localization.Get("lblAttributeRecipe", false, null), Localization.Get(this.RecipesToLearn[i], false, null)));
			}
			else
			{
				_infoList.Add(ItemAction.StringFormatHandler(Localization.Get("lblAttributeRecipe", false, null), Localization.Get("lblKnown", false, null)));
			}
		}
	}

	public new string[] RecipesToLearn;

	public string[] SkillsToGain;

	public new string Title;

	public new string Description;

	[PublicizedFrom(EAccessModifier.Private)]
	public class MyInventoryData : ItemActionAttackData
	{
		public MyInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}

		public bool bReadingStarted;
	}
}
