using System;
using System.Collections.Generic;
using Audio;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEat : ItemAction
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionEat.MyInventoryData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		this.Consume = true;
		_props.ParseBool("Consume", ref this.Consume);
		if (_props.Values.ContainsKey("Create_item"))
		{
			this.CreateItem = _props.Values["Create_item"];
			if (_props.Values.ContainsKey("Create_item_count"))
			{
				this.CreateItemCount = int.Parse(_props.Values["Create_item_count"]);
			}
			else
			{
				this.CreateItemCount = 1;
			}
			if (_props.Values.ContainsKey("Use_jar_refund"))
			{
				this.UseJarRefund = StringParsers.ParseBool(_props.Values["Use_jar_refund"], 0, -1, true);
			}
		}
		else
		{
			this.CreateItem = null;
			this.CreateItemCount = 0;
			this.UseJarRefund = false;
		}
		string @string = _props.GetString("BlocksAllowed");
		if (@string.Length > 0)
		{
			string[] array = @string.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].Trim();
				Block blockByName = Block.GetBlockByName(text, true);
				if (blockByName == null)
				{
					Log.Error("ItemActionEat BlocksAllowed invalid {0}", new object[] { text });
				}
				else
				{
					if (this.ConditionBlockTypes == null)
					{
						this.ConditionBlockTypes = new HashSet<int>();
					}
					this.ConditionBlockTypes.Add(blockByName.blockID);
				}
			}
			if (this.ConditionBlockTypes != null && this.ConditionBlockTypes.Count == 0)
			{
				this.ConditionBlockTypes = null;
			}
		}
		_props.ParseString("PromptDescription", ref this.PromptDescription);
		_props.ParseString("PromptTitle", ref this.PromptTitle);
		_props.ParseFloat("SmellUse", ref this.smellUse);
		if (this.PromptDescription != null)
		{
			this.UsePrompt = true;
		}
	}

	public override string CanInteract(ItemActionData _actionData)
	{
		if (!_actionData.invData.holdingEntity.isHeadUnderwater && this.IsValidConditions(_actionData))
		{
			return "lblContextActionDrink";
		}
		return null;
	}

	public bool NeedPrompt(ItemActionData _actionData)
	{
		ItemActionEat.MyInventoryData myInventoryData = (ItemActionEat.MyInventoryData)_actionData;
		return this.UsePrompt && !myInventoryData.bPromptChecked;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsValidConditions(ItemActionData _actionData)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (this.ConditionBlockTypes != null)
		{
			Ray lookRay = holdingEntity.GetLookRay();
			int modelLayer = holdingEntity.GetModelLayer();
			holdingEntity.SetModelLayer(2, false, null);
			Voxel.Raycast(_actionData.invData.world, lookRay, 2.5f, 131, (holdingEntity is EntityPlayer) ? 0.2f : 0.4f);
			holdingEntity.SetModelLayer(modelLayer, false, null);
			WorldRayHitInfo voxelRayHitInfo = Voxel.voxelRayHitInfo;
			if (!GameUtils.IsBlockOrTerrain(voxelRayHitInfo.tag))
			{
				return false;
			}
			BlockValue blockValue = voxelRayHitInfo.hit.blockValue;
			bool flag = false;
			using (HashSet<int>.Enumerator enumerator = this.ConditionBlockTypes.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == 240)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag ? (!voxelRayHitInfo.hit.waterValue.HasMass()) : (!this.ConditionBlockTypes.Contains(blockValue.type)))
			{
				return false;
			}
		}
		return true;
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
		if (this.IsActionRunning(_actionData))
		{
			return;
		}
		if (!this.IsValidConditions(_actionData))
		{
			return;
		}
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, _actionData.invData.item.ItemTags, true, true, true, true, true, 1, true, false) > 0f)
		{
			_actionData.lastUseTime = Time.time + 1f;
			Manager.PlayInsidePlayerHead("twitch_no_attack", -1, 0f, false, false);
			return;
		}
		_actionData.lastUseTime = Time.time;
		if (this.UseAnimation)
		{
			if (holdingEntity.emodel != null && holdingEntity.emodel.avatarController != null)
			{
				holdingEntity.emodel.avatarController.SetMeleeAttackSpeed(1f);
			}
			holdingEntity.RightArmAnimationUse = true;
			if (this.soundStart != null)
			{
				holdingEntity.PlayOneShot(this.soundStart, false, false, false, null, 1f);
			}
			((ItemActionEat.MyInventoryData)_actionData).bEatingStarted = true;
			((ItemActionEat.MyInventoryData)_actionData).bEatingFinished = false;
			return;
		}
		this.ExecuteInstantAction(holdingEntity, _actionData.invData.itemStack, true, null);
	}

	public override bool ExecuteInstantAction(EntityAlive ent, ItemStack stack, bool isHeldItem, XUiC_ItemStack stackController)
	{
		ent.MinEventContext.ItemValue = stack.itemValue;
		ent.MinEventContext.ItemValue.FireEvent(MinEventTypes.onSelfPrimaryActionStart, ent.MinEventContext);
		ent.FireEvent(MinEventTypes.onSelfPrimaryActionStart, false);
		if (this.soundStart != null)
		{
			ent.PlayOneShot(this.soundStart, this.Sound_in_head, false, false, null, 1f);
		}
		EntityPlayer entityPlayer = ent as EntityPlayer;
		if (this.Consume)
		{
			if (stack.itemValue.MaxUseTimes > 0 && stack.itemValue.UseTimes + 1f < (float)stack.itemValue.MaxUseTimes)
			{
				stack.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, stack.itemValue, 1f, ent, null, stack.itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false) * ItemAction.ItemDegradationModifier;
				return true;
			}
			if (isHeldItem)
			{
				ent.inventory.DecHoldingItem(1);
			}
			else
			{
				stack.count--;
			}
			if (stackController != null)
			{
				stackController.ItemStack.count--;
				if (stackController.ItemStack.count == 0)
				{
					stackController.ItemStack = ItemStack.Empty;
				}
				stackController.ForceRefreshItemStack();
			}
		}
		if (stackController != null)
		{
			ent.MinEventContext.ItemValue = stack.itemValue;
			ent.MinEventContext.ItemValue.FireEvent(MinEventTypes.onSelfPrimaryActionEnd, ent.MinEventContext);
			ent.FireEvent(MinEventTypes.onSelfPrimaryActionEnd, false);
		}
		QuestEventManager.Current.UsedItem(stack.itemValue);
		if (this.CreateItem != null && this.CreateItemCount > 0 && (!this.UseJarRefund || SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.JarRefund) > entityPlayer.rand.RandomRange(1f)))
		{
			ItemStack itemStack = new ItemStack(ItemClass.GetItem(this.CreateItem, false), this.CreateItemCount);
			if (!LocalPlayerUI.GetUIForPlayer(entityPlayer as EntityPlayerLocal).xui.PlayerInventory.AddItem(itemStack))
			{
				ent.world.gameManager.ItemDropServer(itemStack, ent.GetPosition(), Vector3.zero, -1, 60f, false);
			}
		}
		return true;
	}

	public float PercentDone(ItemActionData _actionData)
	{
		float num = 0f;
		ItemActionEat.MyInventoryData myInventoryData = (ItemActionEat.MyInventoryData)_actionData;
		if (myInventoryData.bEatingStarted)
		{
			num = (Time.time - myInventoryData.lastUseTime) / AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast;
		}
		return num;
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		ItemActionEat.MyInventoryData myInventoryData = (ItemActionEat.MyInventoryData)_actionData;
		return (myInventoryData.bEatingStarted && (!myInventoryData.bEatingFinished || Time.time - myInventoryData.lastUseTime < 2f * AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast)) || (!this.UseAnimation && Time.time - myInventoryData.lastUseTime < this.Delay);
	}

	public override bool IsEndDelayed()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void consume(ItemActionData _actionData)
	{
		ItemActionEat.MyInventoryData myInventoryData = _actionData as ItemActionEat.MyInventoryData;
		if (myInventoryData == null || !myInventoryData.bEatingStarted)
		{
			return;
		}
		myInventoryData.bEatingStarted = false;
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		MinEventTypes minEventTypes = MinEvent.End[_actionData.indexInEntityOfAction];
		holdingEntity.MinEventContext.ItemValue = _actionData.invData.itemStack.itemValue;
		QuestEventManager.Current.UsedItem(holdingEntity.MinEventContext.ItemValue);
		holdingEntity.FireEvent(minEventTypes, true);
		if (this.Consume)
		{
			if (_actionData.invData.itemValue.MaxUseTimes > 0 && _actionData.invData.itemValue.UseTimes + 1f < (float)_actionData.invData.itemValue.MaxUseTimes)
			{
				_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, myInventoryData.invData.itemValue, 1f, holdingEntity, null, myInventoryData.invData.itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false) * ItemAction.ItemDegradationModifier;
				return;
			}
			holdingEntity.inventory.DecHoldingItem(1);
		}
		EntityPlayerLocal entityPlayerLocal = holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal && this.smellUse > 0f)
		{
			entityPlayerLocal.Stealth.SetSmellEat(this.smellUse);
		}
		if (this.CreateItem != null && this.CreateItemCount > 0 && (!this.UseJarRefund || SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.JarRefund) > holdingEntity.rand.RandomRange(1f)))
		{
			ItemStack itemStack = new ItemStack(ItemClass.GetItem(this.CreateItem, false), this.CreateItemCount);
			if (!LocalPlayerUI.GetUIForPlayer(holdingEntity as EntityPlayerLocal).xui.PlayerInventory.AddItem(itemStack))
			{
				holdingEntity.world.gameManager.ItemDropServer(itemStack, holdingEntity.GetPosition(), Vector3.zero, -1, 60f, false);
			}
		}
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionEat.MyInventoryData myInventoryData = (ItemActionEat.MyInventoryData)_actionData;
		if ((double)this.PercentDone(_actionData) >= 1.0)
		{
			this.consume(myInventoryData);
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		((ItemActionEat.MyInventoryData)_data).bEatingStarted = false;
	}

	public void Completed(ItemActionData _actionData)
	{
		this.consume(_actionData);
	}

	public new string CreateItem;

	public int CreateItemCount;

	public new bool Consume;

	public HashSet<int> ConditionBlockTypes;

	public bool UsePrompt;

	public bool UseJarRefund;

	public string PromptDescription;

	public string PromptTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public float smellUse;

	[PublicizedFrom(EAccessModifier.Private)]
	public class MyInventoryData : ItemActionAttackData
	{
		public MyInventoryData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}

		public bool bEatingStarted;

		public bool bPromptChecked;

		public bool bEatingFinished;
	}
}
