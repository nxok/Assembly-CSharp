using System;
using System.Collections.Generic;
using System.Text;
using Audio;
using UnityEngine;
using XMLData.Item;

public abstract class ItemAction : XMLData.Item.ItemActionData
{
	public string GetSoundStart()
	{
		return this.soundStart;
	}

	public virtual ItemClass.EnumCrosshairType GetCrosshairType(global::ItemActionData _actionData)
	{
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal.TPCameraCheckResult == eTPCameraCheckResult.Pass)
		{
			return ItemClass.EnumCrosshairType.Plus;
		}
		if (entityPlayerLocal.TPCameraCheckResult == eTPCameraCheckResult.LineOfSightCheckFailed)
		{
			return ItemClass.EnumCrosshairType.Blocked;
		}
		return ItemClass.EnumCrosshairType.None;
	}

	public virtual bool IsEndDelayed()
	{
		return false;
	}

	public virtual void OnHUD(global::ItemActionData _actionData, int _x, int _y)
	{
	}

	public virtual void OnScreenOverlay(global::ItemActionData _data)
	{
	}

	public virtual bool ConsumeScrollWheel(global::ItemActionData _actionData, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		return false;
	}

	public virtual bool ConsumeCameraFunction(global::ItemActionData _actionData)
	{
		return false;
	}

	public virtual global::ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new global::ItemActionData(_invData, _indexInEntityOfAction);
	}

	public virtual RenderCubeType GetFocusType(global::ItemActionData _actionData)
	{
		return RenderCubeType.None;
	}

	public virtual bool IsFocusBlockInside()
	{
		return true;
	}

	public virtual bool IsHUDDisabled(global::ItemActionData _data)
	{
		return false;
	}

	public virtual void StartHolding(global::ItemActionData _data)
	{
		this.OnModificationsChanged(_data);
	}

	public virtual void StopHolding(global::ItemActionData _data)
	{
		_data.invData.holdingEntity.StopAnimatorAudio(Entity.StopAnimatorAudioType.StopOnStopHolding);
	}

	public virtual void OnHoldingUpdate(global::ItemActionData _actionData)
	{
	}

	public virtual void ShowItem(global::ItemActionData _actionData, bool _show)
	{
	}

	public virtual void OnModificationsChanged(global::ItemActionData _data)
	{
	}

	public virtual bool CanCancel(global::ItemActionData _data)
	{
		return false;
	}

	public virtual void CancelReload(global::ItemActionData _actionData, bool holsterWeapon)
	{
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null && holsterWeapon)
		{
			entityPlayerLocal.HolsterWeapon(true);
		}
	}

	public virtual bool AllowConcurrentActions()
	{
		return false;
	}

	public virtual void Cleanup(global::ItemActionData _data)
	{
	}

	public virtual void ReadFrom(DynamicProperties _props)
	{
		this.Delay = 0f;
		_props.ParseFloat("Delay", ref this.Delay);
		_props.ParseString("Sound_start", ref this.soundStart);
		_props.ParseBool("Sound_in_head", ref this.Sound_in_head);
		if (_props.Values.ContainsKey("Particle_harvesting"))
		{
			this.bUseParticleHarvesting = StringParsers.ParseBool(_props.Values["Particle_harvesting"], 0, -1, true);
			this.particleHarvestingCategory = _props.Params1["Particle_harvesting"];
		}
		this.ActionExp = 2;
		_props.ParseInt("ActionExp", ref this.ActionExp);
		this.ActionExpBonusMultiplier = 10f;
		_props.ParseFloat("ActionExpBonusMultiplier", ref this.ActionExpBonusMultiplier);
		_props.ParseBool("UseAnimation", ref this.UseAnimation);
		this.BuffActions = new List<string>();
		if (_props.Values.ContainsKey("Buff"))
		{
			if (_props.Values["Buff"].Contains(","))
			{
				string[] array = _props.Values["Buff"].Replace(" ", "").Split(',', StringSplitOptions.None);
				this.BuffActions.AddRange(array);
			}
			else
			{
				this.BuffActions.Add(_props.Values["Buff"].Trim());
			}
		}
		else
		{
			this.ActionExpBonusMultiplier = 10f;
		}
		this.Properties = _props;
	}

	public string GetDescription()
	{
		return this.Properties.GetString("Description");
	}

	public virtual string CanInteract(global::ItemActionData _actionData)
	{
		return null;
	}

	public static void ExecuteBuffActions(List<string> actions, int instigatorId, EntityAlive target, bool isCritical, EnumBodyPartHit hitLocation, string context)
	{
		if (target == null)
		{
			return;
		}
		EntityAlive entityAlive = GameManager.Instance.World.GetEntity(instigatorId) as EntityAlive;
		if (entityAlive == null)
		{
			return;
		}
		if (actions != null)
		{
			for (int i = 0; i < actions.Count; i++)
			{
				BuffClass buff = BuffManager.GetBuff(actions[i]);
				if (buff != null)
				{
					float num = 1f;
					num = EffectManager.GetValue(PassiveEffects.BuffProcChance, null, num, entityAlive, null, FastTags<TagGroup.Global>.Parse(buff.Name), true, true, true, true, true, 1, true, false);
					if (target.rand.RandomFloat <= num)
					{
						target.Buffs.AddBuff(actions[i], entityAlive.entityId, true, false, -1f);
					}
				}
			}
		}
	}

	public virtual bool IsActionRunning(global::ItemActionData _actionData)
	{
		return false;
	}

	public virtual bool CanExecute(global::ItemActionData _actionData)
	{
		if (this.ExecutionRequirements == null)
		{
			return true;
		}
		_actionData.invData.holdingEntity.MinEventContext.ItemValue = _actionData.invData.itemValue;
		return this.ExecutionRequirements.IsValid(_actionData.invData.holdingEntity.MinEventContext);
	}

	public abstract void ExecuteAction(global::ItemActionData _actionData, bool _bReleased);

	public virtual bool ExecuteInstantAction(EntityAlive ent, ItemStack stack, bool isHeldItem, XUiC_ItemStack stackController)
	{
		return false;
	}

	public virtual void CancelAction(global::ItemActionData _actionData)
	{
	}

	public virtual WorldRayHitInfo GetExecuteActionTarget(global::ItemActionData _actionData)
	{
		return null;
	}

	public virtual void GetIronSights(global::ItemActionData _actionData, out float _fov)
	{
		_fov = 0f;
	}

	public virtual EnumCameraShake GetCameraShakeType(global::ItemActionData _actionData)
	{
		return EnumCameraShake.None;
	}

	public virtual TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectPull()
	{
		return TriggerEffectManager.NoneEffect;
	}

	public virtual TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectShoot()
	{
		return TriggerEffectManager.NoneEffect;
	}

	public virtual bool AllowItemLoopingSound(global::ItemActionData _actionData)
	{
		return true;
	}

	public virtual bool IsAimingGunPossible(global::ItemActionData _actionData)
	{
		return true;
	}

	public virtual void AimingSet(global::ItemActionData _actionData, bool _isAiming, bool _wasAiming)
	{
	}

	public virtual void ItemActionEffects(GameManager _gameManager, global::ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
	}

	public virtual void UpdateNozzleParticlesPosAndRot(global::ItemActionData _actionData)
	{
	}

	public virtual int GetInitialMeta(ItemValue _itemValue)
	{
		return 0;
	}

	public virtual void SwapAmmoType(EntityAlive _entity, int _ammoItemId = -1)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool canShowOverlay(global::ItemActionData actionData)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool isShowOverlay(global::ItemActionData actionData)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void getOverlayData(global::ItemActionData actionData, out float _perc, out string _text)
	{
		_perc = 0f;
		_text = "";
	}

	public static float GetDismemberChance(global::ItemActionData _actionData, WorldRayHitInfo hitInfo)
	{
		FastTags<TagGroup.Global> fastTags = _actionData.ActionTags;
		if (hitInfo.tag == "E_BP_Head")
		{
			fastTags |= ItemAction.headTag;
		}
		else if (hitInfo.tag.ContainsCaseInsensitive("arm"))
		{
			fastTags |= ItemAction.armTag;
		}
		else if (hitInfo.tag.ContainsCaseInsensitive("leg"))
		{
			fastTags |= ItemAction.legTag;
		}
		return EffectManager.GetValue(PassiveEffects.DismemberChance, _actionData.invData.holdingEntity.inventory.holdingItemItemValue, 0f, _actionData.invData.holdingEntity, null, fastTags | _actionData.invData.item.ItemTags, true, true, true, true, true, 1, true, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual List<string> getBuffActions(global::ItemActionData _actionData)
	{
		if (this.BuffActions == null)
		{
			return new List<string>();
		}
		return this.BuffActions;
	}

	public virtual void GetItemValueActionInfo(ref List<string> _infoList, ItemValue _itemValue, XUi _xui, int _actionIndex = 0)
	{
	}

	public virtual bool IsEditingTool()
	{
		return false;
	}

	public virtual string GetStat(global::ItemActionData _data)
	{
		return string.Empty;
	}

	public virtual bool IsStatChanged()
	{
		return false;
	}

	public virtual bool HasRadial()
	{
		return false;
	}

	public virtual void SetupRadial(XUiC_Radial _xuiRadialWindow, EntityPlayerLocal _epl)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string StringFormatHandler(string title, object value)
	{
		return string.Format("{0}: [REPLACE_COLOR]{1}[-]\n", title, value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string BuffActionStrings(ItemAction itemAction, List<string> stringList)
	{
		if (itemAction.BuffActions == null || itemAction.BuffActions.Count == 0)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < itemAction.BuffActions.Count; i++)
		{
			BuffClass buff = BuffManager.GetBuff(itemAction.BuffActions[i]);
			if (buff != null && !string.IsNullOrEmpty(buff.Name))
			{
				stringList.Add(ItemAction.StringFormatHandler(Localization.Get("lblEffect", false, null), string.Format("{0}", buff.Name)));
			}
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string getColoredItemStat(string _title, float _value)
	{
		if (_value > 0f)
		{
			return string.Format("{0}: [00ff00]+{1}[-]", _title, _value.ToCultureInvariantString());
		}
		if (_value < 0f)
		{
			return string.Format("{0}: [ff0000]{1}[-]", _title, _value.ToCultureInvariantString());
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string getColoredItemStatPercentage(string _title, float _value)
	{
		if (_value > 0f)
		{
			return string.Format("{0}: [00ff00]+{1}%[-]", _title, _value.ToCultureInvariantString("0.0"));
		}
		if (_value < 0f)
		{
			return string.Format("{0}: [ff0000]{1}%[-]", _title, _value.ToCultureInvariantString("0.0"));
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleItemBreak(global::ItemActionData _actionData)
	{
		if (_actionData.invData.itemValue.MaxUseTimes > 0 && _actionData.invData.itemValue.UseTimes >= (float)_actionData.invData.itemValue.MaxUseTimes)
		{
			Manager.BroadcastPlay(_actionData.invData.holdingEntity, "itembreak", false, 1f);
		}
	}

	public static void HandleDegradation(ItemValue _outItem)
	{
		if (ItemAction.ItemMaxDegrationAmount == 0f)
		{
			return;
		}
		float num;
		if (!_outItem.TryGetMetadata("DurabilityModifier", out num))
		{
			num = 1f;
			_outItem.SetMetadata("DurabilityModifier", num);
			return;
		}
		num -= ItemAction.ItemMaxDegrationAmount;
		num = Mathf.Max(num, 0.05f);
		_outItem.SetMetadata("DurabilityModifier", num);
	}

	public void HandleJamSound(ItemValue itemValue, ItemClass itemClass, EntityPlayerLocal localPlayer)
	{
		ItemAction.CanRepairValues canRepairValues = ItemAction.CanRepair(itemValue);
		if (canRepairValues == ItemAction.CanRepairValues.Allowed)
		{
			if (itemClass.Properties.Values.ContainsKey(ItemClass.PropSoundJammed))
			{
				Manager.PlayInsidePlayerHead(itemClass.Properties.Values[ItemClass.PropSoundJammed], -1, 0f, false, false);
			}
			GameManager.ShowTooltip(localPlayer, "ttItemNeedsRepair", false, false, 0f);
			return;
		}
		if (canRepairValues == ItemAction.CanRepairValues.NoLongerRepairable)
		{
			if (itemClass.Properties.Values.ContainsKey(ItemClass.PropSoundJammed))
			{
				Manager.PlayInsidePlayerHead(itemClass.Properties.Values[ItemClass.PropSoundJammed], -1, 0f, false, false);
			}
			GameManager.ShowTooltip(localPlayer, "ttItemNoLongerRepairable", false, false, 0f);
		}
	}

	public static ItemAction.CanRepairValues CanRepair(ItemValue _itemToBeRepaired)
	{
		if (ItemAction.ItemMaxDegrationAmount == 0f)
		{
			return ItemAction.CanRepairValues.Allowed;
		}
		float num;
		if (!_itemToBeRepaired.TryGetMetadata("DurabilityModifier", out num))
		{
			return ItemAction.CanRepairValues.Allowed;
		}
		float num2 = num - ItemAction.ItemMaxDegrationAmount;
		if (num2 < ItemAction.ItemMaxDegrationAmount)
		{
			return ItemAction.CanRepairValues.NoLongerRepairable;
		}
		if (num2 * (float)_itemToBeRepaired.MaxUseTimes < (float)_itemToBeRepaired.MaxUseTimes - _itemToBeRepaired.UseTimes)
		{
			return ItemAction.CanRepairValues.DurabilityWouldBeReduced;
		}
		return ItemAction.CanRepairValues.Allowed;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemAction()
	{
	}

	public static FastTags<TagGroup.Global> headTag = FastTags<TagGroup.Global>.Parse("head");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> armTag = FastTags<TagGroup.Global>.Parse("arm");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> legTag = FastTags<TagGroup.Global>.Parse("leg");

	public static bool ShowDebugDisplayHit;

	public static float DebugDisplayHitSize = 0.005f;

	public static float DebugDisplayHitTime = 10f;

	public static bool ShowDistanceDebugInfo = false;

	public ItemClass item;

	public List<string> BuffActions;

	public new float Delay;

	public new float Range;

	public float SphereRadius;

	public DynamicProperties Properties = new DynamicProperties();

	public RequirementGroup ExecutionRequirements;

	public bool UseAnimation = true;

	public int ActionIndex;

	public static float ItemDegradationModifier = 1f;

	public static float ItemMaxDegrationAmount = 0f;

	public static ItemAction.RepairTypes RepairType = ItemAction.RepairTypes.Both;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string soundStart;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool Sound_in_head;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bUseParticleHarvesting;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string particleHarvestingCategory;

	public int ActionExp;

	public float ActionExpBonusMultiplier;

	public enum RepairTypes
	{
		None,
		RepairOnly,
		CombineOnly,
		Both
	}

	public enum CanRepairValues
	{
		Allowed,
		NoLongerRepairable,
		DurabilityWouldBeReduced
	}
}
