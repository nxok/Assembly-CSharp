using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionGainSkill : ItemAction
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionGainSkill.MyInventoryData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (!_props.Values.ContainsKey("Skills_to_gain"))
		{
			this.SkillsToGain = new string[0];
		}
		else
		{
			this.SkillsToGain = _props.Values["Skills_to_gain"].Split(',', StringSplitOptions.None);
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
			return;
		}
		this.Description = _props.Values["Description"];
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		((ItemActionGainSkill.MyInventoryData)_data).bReadingStarted = false;
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
		((ItemActionGainSkill.MyInventoryData)_actionData).bReadingStarted = true;
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		ItemActionGainSkill.MyInventoryData myInventoryData = (ItemActionGainSkill.MyInventoryData)_actionData;
		return myInventoryData.bReadingStarted && Time.time - myInventoryData.lastUseTime < 2f * AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionGainSkill.MyInventoryData myInventoryData = (ItemActionGainSkill.MyInventoryData)_actionData;
		if (!myInventoryData.bReadingStarted || Time.time - myInventoryData.lastUseTime < AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast)
		{
			return;
		}
		myInventoryData.bReadingStarted = false;
		bool flag = false;
		for (int i = 0; i < this.SkillsToGain.Length; i++)
		{
			if (myInventoryData.invData.holdingEntity is EntityPlayer)
			{
				EntityPlayerLocal entityPlayerLocal = myInventoryData.invData.holdingEntity as EntityPlayerLocal;
				ProgressionValue progressionValue = myInventoryData.invData.holdingEntity.Progression.GetProgressionValue(this.SkillsToGain[i]);
				if (progressionValue != null)
				{
					if (progressionValue.Level + 1 <= progressionValue.ProgressionClass.MaxLevel)
					{
						ProgressionValue progressionValue2 = progressionValue;
						int level = progressionValue2.Level;
						progressionValue2.Level = level + 1;
						entityPlayerLocal.MinEventContext.ProgressionValue = progressionValue;
						entityPlayerLocal.FireEvent(MinEventTypes.onPerkLevelChanged, true);
						string text = Localization.Get(progressionValue.ProgressionClass.NameKey, false, null);
						GameManager.ShowTooltip(entityPlayerLocal, string.Format(Localization.Get("ttSkillLevelUp", false, null), text, progressionValue.Level), false, false, 0f);
						(myInventoryData.invData.holdingEntity as EntityPlayer).bPlayerStatsChanged = true;
						flag = true;
					}
					else
					{
						GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttSkillMaxLevel", false, null), false, false, 0f);
					}
				}
			}
		}
		if (flag)
		{
			_actionData.invData.holdingEntity.inventory.DecHoldingItem(1);
			if (this.soundStart != null)
			{
				Manager.PlayInsidePlayerHead(this.soundStart, -1, 0f, false, false);
			}
		}
	}

	public override void GetItemValueActionInfo(ref List<string> _infoList, ItemValue _itemValue, XUi _xui, int _actionIndex = 0)
	{
		for (int i = 0; i < this.SkillsToGain.Length; i++)
		{
			_infoList.Add(ItemAction.StringFormatHandler(Localization.Get(_xui.playerUI.entityPlayer.Progression.GetProgressionValue(this.SkillsToGain[i]).ProgressionClass.NameKey, false, null), "+1"));
		}
	}

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
