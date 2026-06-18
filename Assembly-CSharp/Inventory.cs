using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio;
using UnityEngine;

public class Inventory
{
	public int PUBLIC_SLOTS_PLAYMODE
	{
		get
		{
			return 10;
		}
	}

	public int PUBLIC_SLOTS_PREFABEDITOR
	{
		get
		{
			return 2 * this.PUBLIC_SLOTS_PLAYMODE;
		}
	}

	public int SHIFT_KEY_SLOT_OFFSET
	{
		get
		{
			return 10;
		}
	}

	public int PUBLIC_SLOTS
	{
		get
		{
			if (PrefabEditModeManager.Instance == null || !PrefabEditModeManager.Instance.IsActive())
			{
				return this.PUBLIC_SLOTS_PLAYMODE;
			}
			return this.PUBLIC_SLOTS_PREFABEDITOR;
		}
	}

	public int INVENTORY_SLOTS
	{
		get
		{
			return this.PUBLIC_SLOTS + 1;
		}
	}

	public int DUMMY_SLOT_IDX
	{
		get
		{
			return this.INVENTORY_SLOTS - 1;
		}
	}

	public event XUiEvent_ToolbeltItemsChangedInternal OnToolbeltItemsChangedInternal;

	public Inventory(IGameManager _gameManager, EntityAlive _entity)
	{
		this.m_LastDrawnHoldingItemIndex = this.DUMMY_SLOT_IDX;
		this.m_HoldingItemIdx = this.DUMMY_SLOT_IDX;
		this.preferredItemSlots = new int[this.PUBLIC_SLOTS];
		this.models = new Transform[this.INVENTORY_SLOTS];
		this.slots = new ItemInventoryData[this.INVENTORY_SLOTS];
		this.entity = _entity;
		this.gameManager = _gameManager;
		this.emptyItem = new ItemInventoryData(null, ItemStack.Empty, _gameManager, _entity, 0);
		this.Clear();
		this.m_HoldingItemIdx = 0;
		this.m_LastDrawnHoldingItemIndex = -1;
		this.previousHeldItemValue = null;
		this.previousHeldItemSlotIdx = -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitInactiveItemsObject()
	{
		if (!this.inactiveItems)
		{
			GameObject gameObject = new GameObject("InactiveItems");
			gameObject.SetActive(false);
			this.inactiveItems = gameObject.transform;
			this.inactiveItems.SetParent(this.entity.transform, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onInventoryChanged()
	{
		if (this.OnToolbeltItemsChangedInternal != null)
		{
			this.OnToolbeltItemsChangedInternal();
		}
	}

	public void CallOnToolbeltChangedInternal()
	{
		this.onInventoryChanged();
	}

	public bool UsingBareHand()
	{
		return this.bareHandItem == this.holdingItem;
	}

	public virtual ItemClass GetBareHandItem()
	{
		return this.bareHandItem;
	}

	public virtual ItemValue GetBareHandItemValue()
	{
		return this.bareHandItemValue;
	}

	public virtual void SetBareHandItem(ItemValue _bareHandItemValue)
	{
		this.bareHandItemValue = _bareHandItemValue;
		this.bareHandItem = ItemClass.GetForId(this.bareHandItemValue.type);
		this.bareHandItemInventoryData = this.bareHandItem.CreateInventoryData(new ItemStack(_bareHandItemValue, 1), this.gameManager, this.entity, 0);
	}

	public virtual void SetSlots(ItemStack[] _slots, bool _allowSettingDummySlot = true)
	{
		int num = 0;
		while (num < _slots.Length && num < (_allowSettingDummySlot ? this.INVENTORY_SLOTS : this.PUBLIC_SLOTS))
		{
			this.SetItem(num, _slots[num].itemValue, _slots[num].count, false);
			num++;
		}
		this.notifyListeners();
	}

	public virtual ItemActionData GetItemActionDataInSlot(int _slotIdx, int _actionIdx)
	{
		if (_slotIdx == this.holdingItemIdx)
		{
			return this.holdingItemData.actionData[_actionIdx];
		}
		return this.slots[_slotIdx].actionData[_actionIdx];
	}

	public virtual ItemAction GetItemActionInSlot(int _slotIdx, int _actionIdx)
	{
		if (_slotIdx == this.holdingItemIdx)
		{
			return this.holdingItem.Actions[_actionIdx];
		}
		if (this.slots[_slotIdx] == null || this.slots[_slotIdx].item == null)
		{
			return null;
		}
		return this.slots[_slotIdx].item.Actions[_actionIdx];
	}

	public virtual ItemClass GetItemInSlot(int _idx)
	{
		if (this.slots[_idx].item == null)
		{
			return this.bareHandItem;
		}
		return this.slots[_idx].item;
	}

	public ItemInventoryData GetItemDataInSlot(int _idx)
	{
		if (this.slots[_idx].item == null)
		{
			return this.bareHandItemInventoryData;
		}
		return this.slots[_idx];
	}

	public virtual ItemValue this[int _idx]
	{
		get
		{
			return this.slots[_idx].itemStack.itemValue;
		}
		set
		{
			this.slots[_idx].itemStack.itemValue = value;
			this.notifyListeners();
		}
	}

	public ItemStack GetItemStack(int _idx)
	{
		return this.slots[_idx].itemStack;
	}

	public void ModifyValue(ItemValue _originalItemValue, PassiveEffects _passiveEffect, ref float _base_val, ref float _perc_val, FastTags<TagGroup.Global> tags)
	{
		if (this.holdingItemItemValue != null && !this.holdingItemItemValue.Equals(_originalItemValue) && !this.holdingItemItemValue.ItemClass.ItemTags.Test_AnySet(this.ignoreWhenHeld))
		{
			this.holdingItemItemValue.ModifyValue(this.entity, _originalItemValue, _passiveEffect, ref _base_val, ref _perc_val, tags, true, false);
		}
	}

	public void OnUpdate()
	{
		if (!this.isSwitchingHeldItem && !this.entity.IsDead() && this.entity.IsSpawned())
		{
			this.holdingItem.OnHoldingUpdate(this.holdingItemData);
			ItemValue itemValue = this.holdingItemData.itemValue;
			this.entity.MinEventContext.ItemValue = itemValue;
			itemValue.FireEvent(MinEventTypes.onSelfEquipUpdate, this.entity.MinEventContext);
		}
		if (this.entity is EntityPlayer)
		{
			if (this.holdingCount <= 0)
			{
				this.clearSlotByIndex(this.m_HoldingItemIdx);
			}
			if (this.entity.emodel && this.entity.emodel.avatarController)
			{
				this.entity.emodel.avatarController.CancelEvent(AvatarController.itemHasChangedTriggerHash);
			}
		}
	}

	public void ShowHeldItemOld(bool show, float waitTime = 0.015f)
	{
		if (show)
		{
			this.entity.MinEventContext.ItemValue = this.holdingItemItemValue;
			EntityPlayerLocal entityPlayerLocal = this.entity as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				entityPlayerLocal.HolsterWeapon(false);
			}
		}
		this.HoldingItemHasChanged();
		GameManager.Instance.StartCoroutine(this.delayedShowHideHeldItem(show, waitTime));
	}

	public void ShowHeldItem(float waitTime = 0.015f, bool hideFirst = false)
	{
		if (this.delayedShowHideHeldItemRoutine != null)
		{
			GameManager.Instance.StopCoroutine(this.delayedShowHideHeldItemRoutine);
			this.delayedShowHideHeldItemRoutine = null;
		}
		this.delayedShowHideHeldItemRoutine = GameManager.Instance.StartCoroutine(this.delayedShowHideHeldItem(hideFirst, waitTime));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator delayedShowHideHeldItem(bool hideFirst, float waitTime)
	{
		EntityPlayerLocal epl = this.entity as EntityPlayerLocal;
		if (epl != null)
		{
			while (epl.emodel.avatarController.IsAnimationPlayerFPRevivePlaying())
			{
				yield return new WaitForSeconds(0.1f);
			}
			if (hideFirst)
			{
				epl.HolsterWeapon(true);
				this.entity.StopOneShot(this.holdingItem.SoundUnholster);
				this.entity.PlayOneShot(this.holdingItem.SoundHolster, false, false, false, null, 1f);
			}
			if (waitTime < 0.15f)
			{
				waitTime = 0.15f;
			}
			yield return new WaitForSeconds(waitTime);
			this.entity.MinEventContext.ItemValue = this.holdingItemItemValue;
			this.HoldingItemHasChanged();
			this.entity.StopOneShot(this.holdingItem.SoundHolster);
			this.entity.PlayOneShot(this.holdingItem.SoundUnholster, false, false, false, null, 1f);
			if (this.entity.emodel && this.entity.emodel.avatarController)
			{
				this.entity.emodel.avatarController.TriggerEvent(AvatarController.itemHasChangedTriggerHash);
			}
			this.updateHoldingItem();
			epl.HolsterWeapon(false);
			if (epl.bFirstPersonView)
			{
				epl.ShowHoldingItemLayer(true);
			}
			this.ShowRightHand(true);
		}
		else
		{
			this.updateHoldingItem();
		}
		this.SetIsFinishedSwitchingHeldItem();
		yield return new WaitForSeconds(Mathf.Max(waitTime, 0.3f));
		if (this.previousHeldItemSlotIdx != this.holdingItemIdx && this.holdingItemIdx != this.DUMMY_SLOT_IDX && !this.holdingItem.Equals(this.bareHandItem) && !this.slots[this.holdingItemIdx].itemStack.IsEmpty())
		{
			this.quickSwapItemValue = this.previousHeldItemValue;
			this.quickSwapSlotIdx = this.previousHeldItemSlotIdx;
			this.previousHeldItemValue = this.holdingItemItemValue;
			this.previousHeldItemSlotIdx = this.holdingItemIdx;
		}
		yield break;
	}

	public void ShowRightHand(bool _show)
	{
		if (this.entity.emodel && this.entity.emodel.avatarController)
		{
			Transform rightHandTransform = this.entity.emodel.avatarController.GetRightHandTransform();
			if (rightHandTransform)
			{
				rightHandTransform.gameObject.SetActive(_show);
			}
		}
	}

	public void SetRightHandAsModel()
	{
		if (this.entity.emodel && this.entity.emodel.avatarController)
		{
			this.models[this.m_HoldingItemIdx] = this.entity.emodel.avatarController.GetRightHandTransform();
		}
	}

	public virtual ItemValue holdingItemItemValue
	{
		get
		{
			ItemValue itemValue = this.slots[this.m_HoldingItemIdx].itemStack.itemValue;
			if (!itemValue.IsEmpty())
			{
				return itemValue;
			}
			return this.bareHandItemValue;
		}
	}

	public virtual ItemStack holdingItemStack
	{
		get
		{
			ItemStack itemStack = this.slots[this.m_HoldingItemIdx].itemStack;
			if (!itemStack.IsEmpty())
			{
				return itemStack;
			}
			return new ItemStack(this.bareHandItemValue, 0);
		}
	}

	public virtual ItemClass holdingItem
	{
		get
		{
			ItemValue itemValue = this.slots[this.m_HoldingItemIdx].itemStack.itemValue;
			if (!itemValue.IsEmpty() && ItemClass.list != null)
			{
				return ItemClass.GetForId(itemValue.type);
			}
			return this.bareHandItem;
		}
	}

	public virtual int holdingCount
	{
		get
		{
			ItemStack itemStack = this.slots[this.m_HoldingItemIdx].itemStack;
			if (!itemStack.itemValue.IsEmpty())
			{
				return itemStack.count;
			}
			return 0;
		}
	}

	public virtual ItemInventoryData holdingItemData
	{
		get
		{
			if (!this.slots[this.m_HoldingItemIdx].itemStack.itemValue.IsEmpty())
			{
				return this.slots[this.m_HoldingItemIdx];
			}
			this.bareHandItemInventoryData.slotIdx = this.holdingItemIdx;
			return this.bareHandItemInventoryData;
		}
	}

	public virtual int holdingItemIdx
	{
		get
		{
			return this.m_HoldingItemIdx;
		}
	}

	public virtual bool IsHolsterDelayActive()
	{
		return this.isSwitchingHeldItem;
	}

	public virtual bool IsUnholsterDelayActive()
	{
		return this.isSwitchingHeldItem;
	}

	public void SetIsFinishedSwitchingHeldItem()
	{
		this.isSwitchingHeldItem = false;
		this.entity.MinEventContext.Self = this.entity;
		this.entity.MinEventContext.ItemValue = this.holdingItemItemValue;
		this.HoldingItemHasChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void syncHeldItem()
	{
		this.entity.IsEquipping = true;
		if (this.holdingItemItemValue == null || this.holdingItemItemValue.IsEmpty())
		{
			return;
		}
		if (this.holdingItemItemValue.ItemClass == null)
		{
			if (ItemClass.list == null)
			{
				Log.Out("[Inventory:syncHeldItem] Cannot find item class for held item id '{0}'. Item list is null!", new object[] { this.holdingItemItemValue.type });
			}
			else
			{
				Log.Out("[Inventory:syncHeldItem] Cannot find item class for held item id '{0}'. Item id is out of range! ItemClass list length '{1}'", new object[]
				{
					this.holdingItemItemValue.type,
					ItemClass.list.Length
				});
			}
		}
		else if (!this.holdingItemItemValue.ItemClass.ItemTags.Test_AnySet(this.ignoreWhenHeld))
		{
			this.entity.MinEventContext.ItemValue = this.holdingItemItemValue;
			if (this.holdingItemItemValue.ItemClass != null && this.holdingItemItemValue.ItemClass.HasTrigger(MinEventTypes.onSelfItemActivate))
			{
				if (this.holdingItemItemValue.Activated == 1)
				{
					this.holdingItemItemValue.FireEvent(MinEventTypes.onSelfItemActivate, this.entity.MinEventContext);
				}
				else
				{
					this.holdingItemItemValue.FireEvent(MinEventTypes.onSelfItemDeactivate, this.entity.MinEventContext);
				}
			}
			if (this.holdingItemItemValue.Modifications.Length != 0)
			{
				ItemValue itemValue = this.entity.MinEventContext.ItemValue;
				for (int i = 0; i < this.holdingItemItemValue.Modifications.Length; i++)
				{
					ItemValue itemValue2 = this.holdingItemItemValue.Modifications[i];
					if (itemValue2 != null && itemValue2.ItemClass != null)
					{
						ItemClass itemClass = itemValue2.ItemClass;
						this.entity.MinEventContext.ItemValue = itemValue2;
						if (itemClass.HasTrigger(MinEventTypes.onSelfItemActivate))
						{
							if (itemValue2.Activated == 1)
							{
								itemValue2.FireEvent(MinEventTypes.onSelfItemActivate, this.entity.MinEventContext);
							}
							else
							{
								itemValue2.FireEvent(MinEventTypes.onSelfItemDeactivate, this.entity.MinEventContext);
							}
						}
					}
				}
				this.entity.MinEventContext.ItemValue = itemValue;
			}
		}
		this.CallOnToolbeltChangedInternal();
		this.entity.IsEquipping = false;
	}

	public bool GetIsFinishedSwitchingHeldItem()
	{
		return !this.isSwitchingHeldItem;
	}

	public virtual int GetFocusedItemIdx()
	{
		return this.m_FocusedItemIdx;
	}

	public virtual int SetFocusedItemIdx(int _idx)
	{
		while (_idx < 0)
		{
			_idx += this.PUBLIC_SLOTS;
		}
		while (_idx >= this.PUBLIC_SLOTS)
		{
			_idx -= this.PUBLIC_SLOTS;
		}
		this.m_FocusedItemIdx = _idx;
		return _idx;
	}

	public virtual bool IsHoldingItemActionRunning()
	{
		return this.holdingItem.IsActionRunning(this.holdingItemData);
	}

	public virtual void SetHoldingItemIdxNoHolsterTime(int _inventoryIdx)
	{
		this.setHeldItemByIndex(_inventoryIdx, false);
	}

	public virtual void SetHoldingItemIdx(int _inventoryIdx)
	{
		this.setHeldItemByIndex(_inventoryIdx, true);
	}

	public void BeginSwapHoldingItem()
	{
		this.isSwitchingHeldItem = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void setHeldItemByIndex(int _inventoryIdx, bool _applyHolsterTime)
	{
		this.BeginSwapHoldingItem();
		while (_inventoryIdx < 0)
		{
			_inventoryIdx += this.slots.Length;
		}
		while (_inventoryIdx >= this.slots.Length)
		{
			_inventoryIdx -= this.slots.Length;
		}
		bool flag = this.flashlightOn && this.IsHoldingFlashlight;
		this.HoldingItemHasChanged();
		if (this.entity != null && this.entity.emodel != null && this.entity.emodel.avatarController != null)
		{
			this.entity.emodel.avatarController.TriggerEvent(AvatarController.itemHasChangedTriggerHash);
		}
		for (int i = 0; i < this.holdingItem.Actions.Length; i++)
		{
			if (this.holdingItem.Actions[i] is ItemActionAttack)
			{
				Manager.BroadcastStop(this.entity.entityId, this.holdingItem.Actions[i].GetSoundStart());
			}
		}
		this.m_HoldingItemIdx = _inventoryIdx;
		this.m_FocusedItemIdx = _inventoryIdx;
		if (this.entity.isEntityRemote)
		{
			this.updateHoldingItem();
			return;
		}
		this.ShowHeldItem((!_applyHolsterTime) ? 0f : 0.2f, true);
		if (flag)
		{
			bool flag2 = this.SetFlashlight(false);
			this.currActiveItemIndex = -1;
			if (flag2)
			{
				this.entity.PlayOneShot("flashlight_toggle", false, false, false, null, 1f);
			}
		}
	}

	public void HoldingItemHasChanged()
	{
		if (this.entity != null && this.entity.emodel != null && this.entity.emodel.avatarController != null)
		{
			this.entity.emodel.avatarController.CancelEvent("WeaponFire");
			this.entity.emodel.avatarController.CancelEvent("PowerAttack");
			this.entity.emodel.avatarController.CancelEvent("UseItem");
			this.entity.emodel.avatarController.CancelEvent("ItemUse");
			this.entity.emodel.avatarController.UpdateBool("Reload", false, true);
		}
	}

	public virtual bool IsHoldingGun()
	{
		return this.holdingItem != null && this.holdingItem.IsGun();
	}

	public virtual bool IsHoldingDynamicMelee()
	{
		return this.holdingItem != null && this.holdingItem.IsDynamicMelee();
	}

	public virtual bool IsHoldingBlock()
	{
		return this.holdingItem != null && this.holdingItem.IsBlock();
	}

	public virtual ItemAction GetHoldingPrimary()
	{
		return this.holdingItem.Actions[0];
	}

	public virtual ItemAction GetHoldingSecondary()
	{
		return this.holdingItem.Actions[1];
	}

	public virtual ItemActionAttack GetHoldingGun()
	{
		return this.holdingItem.Actions[0] as ItemActionAttack;
	}

	public virtual ItemActionDynamic GetHoldingDynamicMelee()
	{
		return this.holdingItem.Actions[0] as ItemActionDynamic;
	}

	public virtual ItemClassBlock GetHoldingBlock()
	{
		return this.holdingItem as ItemClassBlock;
	}

	public Transform GetHoldingItemTransform()
	{
		return this.models[this.m_HoldingItemIdx];
	}

	public virtual int GetItemCount(ItemValue _itemValue, bool _bConsiderTexture = false, int _seed = -1, int _meta = -1, bool _ignoreModdedItems = true)
	{
		if (this.slots == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < this.slots.Length; i++)
		{
			if ((!_ignoreModdedItems || !this.slots[i].itemValue.HasModSlots || !this.slots[i].itemValue.HasMods()) && this.slots[i].itemStack.itemValue.type == _itemValue.type && (!_bConsiderTexture || this.slots[i].itemStack.itemValue.TextureFullArray == _itemValue.TextureFullArray) && (_seed == -1 || _seed == (int)this.slots[i].itemValue.Seed) && (_meta == -1 || _meta == this.slots[i].itemValue.Meta))
			{
				num += this.slots[i].itemStack.count;
			}
		}
		return num;
	}

	public virtual int GetItemCount(FastTags<TagGroup.Global> itemTags, int _seed = -1, int _meta = -1, bool _ignoreModdedItems = true)
	{
		if (this.slots == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < this.slots.Length; i++)
		{
			if ((!_ignoreModdedItems || !this.slots[i].itemValue.HasModSlots || !this.slots[i].itemValue.HasMods()) && !this.slots[i].itemValue.IsEmpty() && this.slots[i].itemValue.ItemClass.ItemTags.Test_AnySet(itemTags) && (_seed == -1 || _seed == (int)this.slots[i].itemValue.Seed) && (_meta == -1 || _meta == this.slots[i].itemValue.Meta))
			{
				num += this.slots[i].itemStack.count;
			}
		}
		return num;
	}

	public virtual bool AddItem(ItemStack _itemStack)
	{
		int num;
		return this.AddItem(_itemStack, out num);
	}

	public bool AddItem(ItemStack _itemStack, out int _slot)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.ToolBelt))
		{
			_slot = -1;
			return false;
		}
		for (int i = 0; i < this.slots.Length - 1; i++)
		{
			if (this.slots[i].itemStack.itemValue.type == _itemStack.itemValue.type && this.slots[i].itemStack.CanStackWith(_itemStack, false))
			{
				this.slots[i].itemStack.count += _itemStack.count;
				this.notifyListeners();
				this.entity.bPlayerStatsChanged = !this.entity.isEntityRemote;
				_slot = i;
				return true;
			}
		}
		for (int j = 0; j < this.slots.Length - 1; j++)
		{
			if (this.slots[j].itemStack.IsEmpty())
			{
				this.SetItem(j, _itemStack.itemValue, _itemStack.count, true);
				this.notifyListeners();
				this.entity.bPlayerStatsChanged = !this.entity.isEntityRemote;
				_slot = j;
				return true;
			}
		}
		_slot = -1;
		return false;
	}

	public virtual bool ReturnItem(ItemStack _itemStack)
	{
		for (int i = 0; i < this.PUBLIC_SLOTS; i++)
		{
			i = this.PreferredItemSlot(_itemStack.itemValue.type, i);
			if (i < 0 || i >= this.PUBLIC_SLOTS)
			{
				return false;
			}
			if (this.AddItemAtSlot(_itemStack, i))
			{
				return true;
			}
		}
		return false;
	}

	public bool AddItemAtSlot(ItemStack _itemStack, int _slot)
	{
		bool flag = false;
		if (_slot >= 0 && _slot < this.PUBLIC_SLOTS)
		{
			if (this.slots[_slot].itemStack.itemValue.type == _itemStack.itemValue.type && this.slots[_slot].itemStack.CanStackWith(_itemStack, false))
			{
				this.slots[_slot].itemStack.count += _itemStack.count;
				flag = true;
			}
			if (this.slots[_slot].itemStack.IsEmpty())
			{
				this.SetItem(_slot, _itemStack.itemValue, _itemStack.count, true);
				flag = true;
			}
		}
		if (flag)
		{
			this.notifyListeners();
			this.entity.bPlayerStatsChanged = !this.entity.isEntityRemote;
			if (_slot == this.m_HoldingItemIdx)
			{
				this.HoldingItemHasChanged();
			}
		}
		return flag;
	}

	public virtual int DecItem(ItemValue _itemValue, int _count, bool _ignoreModdedItems = false, IList<ItemStack> _removedItems = null)
	{
		int num = _count;
		int num2 = 0;
		while (_count > 0 && num2 < this.slots.Length - 1)
		{
			if (this.slots[num2].itemStack.itemValue.type == _itemValue.type && (!_ignoreModdedItems || !this.slots[num2].itemValue.HasModSlots || !this.slots[num2].itemValue.HasMods()))
			{
				if (ItemClass.GetForId(this.slots[num2].itemStack.itemValue.type).CanStack())
				{
					int count = this.slots[num2].itemStack.count;
					int num3 = ((count >= _count) ? _count : count);
					if (_removedItems != null)
					{
						_removedItems.Add(new ItemStack(this.slots[num2].itemStack.itemValue.Clone(), num3));
					}
					this.slots[num2].itemStack.count -= num3;
					_count -= num3;
					if (this.slots[num2].itemStack.count <= 0)
					{
						this.clearSlotByIndex(num2);
					}
				}
				else
				{
					if (_removedItems != null)
					{
						_removedItems.Add(this.slots[num2].itemStack.Clone());
					}
					this.clearSlotByIndex(num2);
					_count--;
				}
			}
			num2++;
		}
		this.notifyListeners();
		return num - _count;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearSlotByIndex(int _idx)
	{
		if (!this.slots[_idx].itemStack.itemValue.IsEmpty())
		{
			this.slots[_idx].itemStack = ItemStack.Empty;
		}
		Transform transform = this.models[_idx];
		if (transform)
		{
			this.HoldingItemHasChanged();
			transform.SetParent(null, false);
			transform.gameObject.SetActive(false);
			Object.Destroy(transform.gameObject);
			this.models[_idx] = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual Transform createHeldItem(int _idx, ItemValue _itemValue)
	{
		this.InitInactiveItemsObject();
		Transform transform = _itemValue.ItemClass.CloneModel(this.entity.world, _itemValue, this.entity.GetPosition(), this.inactiveItems, BlockShape.MeshPurpose.Hold, default(TextureFullArray));
		if (transform != null)
		{
			transform.gameObject.SetActive(false);
		}
		return transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual ItemInventoryData createInventoryData(int _idx, ItemValue _itemValue)
	{
		return ItemClass.GetForId(_itemValue.type).CreateInventoryData(ItemStack.Empty, this.gameManager, this.entity, _idx);
	}

	public virtual void SetItem(int _idx, ItemStack _itemStack)
	{
		this.SetItem(_idx, _itemStack.itemValue, _itemStack.count, true);
	}

	public virtual void SetItem(int _idx, ItemValue _itemValue, int _count, bool _notifyListeners = true)
	{
		if (_idx == this.holdingItemIdx && !_itemValue.EqualsExceptUseTimesAndAmmo(this.slots[_idx].itemStack.itemValue))
		{
			this.ShowHeldItem(0.2f, true);
		}
		if ((ulong)_idx >= (ulong)((long)this.slots.Length))
		{
			return;
		}
		if (_itemValue.type != 0 && _itemValue.ItemClass == null)
		{
			Log.Warning("Inventory slot {0} {1} missing item class", new object[] { _idx, _itemValue.type });
			_itemValue.Clear();
		}
		bool flag = false;
		if (_idx < this.preferredItemSlots.Length)
		{
			if (_itemValue.type != 0 && _count != 0)
			{
				this.preferredItemSlots[_idx] = _itemValue.type;
			}
			else if (this.slots[_idx].itemStack.itemValue.type != 0)
			{
				this.preferredItemSlots[_idx] = this.slots[_idx].itemStack.itemValue.type;
			}
		}
		if (_count == 0)
		{
			_itemValue.Clear();
		}
		ItemClass itemClass = this.slots[_idx].itemStack.itemValue.ItemClass;
		ItemClass itemClass2 = _itemValue.ItemClass;
		if (itemClass == null || itemClass != itemClass2)
		{
			this.clearSlotByIndex(_idx);
			if (_itemValue.ItemClass != null)
			{
				this.models[_idx] = (_itemValue.ItemClass.CanHold() ? this.createHeldItem(_idx, _itemValue) : null);
				this.slots[_idx] = this.createInventoryData(_idx, _itemValue);
			}
			flag = true;
		}
		this.slots[_idx].itemStack.itemValue = _itemValue.Clone();
		this.slots[_idx].itemStack.count = _count;
		if (flag && _idx == this.holdingItemIdx)
		{
			this.updateHoldingItem();
		}
		if (_notifyListeners)
		{
			this.notifyListeners();
		}
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		ItemValue itemValue = _eventParms.ItemValue;
		if (itemValue == null)
		{
			itemValue = this.holdingItemItemValue;
		}
		itemValue.FireEvent(_eventType, _eventParms);
	}

	public virtual ItemStack GetItem(int _idx)
	{
		return this.slots[_idx].itemStack;
	}

	public virtual int GetItemCount()
	{
		return this.slots.Length;
	}

	public virtual bool DecHoldingItem(int _count)
	{
		bool flag = true;
		if (ItemClass.GetForId(this.holdingItemItemValue.type).CanStack())
		{
			this.slots[this.m_HoldingItemIdx].itemStack.count -= _count;
			flag = this.slots[this.m_HoldingItemIdx].itemStack.count <= 0;
		}
		if (flag)
		{
			this.HandleTurningOffHoldingFlashlight();
			this.clearSlotByIndex(this.m_HoldingItemIdx);
		}
		this.updateHoldingItem();
		this.notifyListeners();
		return true;
	}

	public bool IsFlashlightOn
	{
		get
		{
			return this.flashlightOn;
		}
	}

	public bool IsAnItemActive()
	{
		return this.currActiveItemIndex != -1;
	}

	public void SetActiveItemIndexOff()
	{
		this.currActiveItemIndex = -1;
	}

	public void ResetActiveIndex()
	{
		this.currActiveItemIndex = -2;
	}

	public bool CycleActivatableItems()
	{
		return true;
	}

	public void HandleTurningOffHoldingFlashlight()
	{
	}

	public void TurnOffLightFlares()
	{
	}

	public virtual bool SetFlashlight(bool on)
	{
		return false;
	}

	public virtual bool IsHoldingFlashlight
	{
		get
		{
			if (this.holdingItem.IsLightSource())
			{
				ItemClass forId = ItemClass.GetForId(this.holdingItemItemValue.type);
				Transform transform = this.models[this.m_HoldingItemIdx].gameObject.transform.Find(forId.ActivateObject.Value);
				if (transform == null && this.models[this.m_HoldingItemIdx].gameObject.name.Equals(forId.ActivateObject.Value))
				{
					transform = this.models[this.m_HoldingItemIdx].gameObject.transform;
				}
				return transform != null && transform.parent.gameObject.activeInHierarchy;
			}
			if (this.holdingItemItemValue.HasQuality)
			{
				for (int i = 0; i < this.holdingItemItemValue.Modifications.Length; i++)
				{
					if (this.holdingItemItemValue.Modifications[i] != null)
					{
						ItemClass itemClass = this.holdingItemItemValue.Modifications[i].ItemClass;
						if (itemClass != null && itemClass.ActivateObject != null)
						{
							Transform transform2 = this.models[this.m_HoldingItemIdx].gameObject.transform.Find(itemClass.ActivateObject.Value);
							if (transform2 == null && this.models[this.m_HoldingItemIdx].gameObject.name.Equals(itemClass.ActivateObject.Value))
							{
								transform2 = this.models[this.m_HoldingItemIdx].gameObject.transform;
							}
							if (transform2 != null && transform2.parent.gameObject.activeInHierarchy)
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}
	}

	public float GetLightLevel()
	{
		float num = 0f;
		this.activatables.Clear();
		this.entity.CollectActivatableItems(this.activatables);
		for (int i = 0; i < this.activatables.Count; i++)
		{
			ItemValue itemValue = this.activatables[i];
			if (itemValue != null && itemValue.Activated > 0)
			{
				ItemClass itemClass = itemValue.ItemClass;
				if (itemClass != null)
				{
					num += itemClass.lightValue;
				}
			}
		}
		ItemClass holdingItem = this.holdingItem;
		if (holdingItem.AlwaysActive != null && holdingItem.AlwaysActive.Value)
		{
			num += holdingItem.lightValue;
		}
		string propertyOverride = this.holdingItemItemValue.GetPropertyOverride("LightValue", string.Empty);
		if (propertyOverride.Length > 0)
		{
			num += float.Parse(propertyOverride);
		}
		return Mathf.Clamp01(num);
	}

	public IEnumerator SimulateActionExecution(int _actionIdx, ItemStack _itemStack, Action<ItemStack> onComplete)
	{
		Inventory.<>c__DisplayClass138_0 CS$<>8__locals1;
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1._itemStack = _itemStack;
		CS$<>8__locals1.onComplete = onComplete;
		while (!this.GetItem(this.DUMMY_SLOT_IDX).IsEmpty())
		{
			yield return null;
		}
		this.SetItem(this.DUMMY_SLOT_IDX, CS$<>8__locals1._itemStack.Clone());
		yield return new WaitForSeconds(0.1f);
		CS$<>8__locals1.previousHoldingIdx = this.m_HoldingItemIdx;
		CS$<>8__locals1.previousFocusedIdx = this.m_FocusedItemIdx;
		this.SetHoldingItemIdx(this.DUMMY_SLOT_IDX);
		yield return new WaitForSeconds(0.1f);
		this.CallOnToolbeltChangedInternal();
		yield return new WaitForSeconds(0.1f);
		while (this.IsHolsterDelayActive())
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (!this.<SimulateActionExecution>g__IsDummySlotActive|138_0(ref CS$<>8__locals1))
		{
			this.<SimulateActionExecution>g__HandleComplete|138_1(ref CS$<>8__locals1);
			yield break;
		}
		this.Execute(_actionIdx, false, null);
		yield return new WaitForSeconds(0.1f);
		if (!this.<SimulateActionExecution>g__IsDummySlotActive|138_0(ref CS$<>8__locals1))
		{
			this.<SimulateActionExecution>g__HandleComplete|138_1(ref CS$<>8__locals1);
			yield break;
		}
		this.Execute(_actionIdx, true, null);
		if (!this.<SimulateActionExecution>g__IsDummySlotActive|138_0(ref CS$<>8__locals1))
		{
			this.<SimulateActionExecution>g__HandleComplete|138_1(ref CS$<>8__locals1);
			yield break;
		}
		CS$<>8__locals1.dummyItem = this.GetItem(this.DUMMY_SLOT_IDX);
		if (CS$<>8__locals1.dummyItem.itemValue.ItemClass != null && CS$<>8__locals1.dummyItem.itemValue.ItemClass.Actions.Length > _actionIdx && CS$<>8__locals1.dummyItem.itemValue.ItemClass.Actions[_actionIdx] != null)
		{
			CS$<>8__locals1.dummyItem.itemValue.ItemClass.Actions[_actionIdx].OnHoldingUpdate(this.GetItemActionDataInSlot(this.DUMMY_SLOT_IDX, _actionIdx));
			while (this.IsHoldingItemActionRunning())
			{
				yield return new WaitForSeconds(0.1f);
			}
			if (!this.<SimulateActionExecution>g__IsDummySlotActive|138_0(ref CS$<>8__locals1))
			{
				this.<SimulateActionExecution>g__HandleComplete|138_1(ref CS$<>8__locals1);
				yield break;
			}
		}
		while (this.IsHolsterDelayActive())
		{
			yield return new WaitForSeconds(0.1f);
		}
		this.<SimulateActionExecution>g__HandleComplete|138_1(ref CS$<>8__locals1);
		yield break;
	}

	public void ForceHoldingItemUpdate()
	{
		if (this.models[this.holdingItemIdx] != null)
		{
			Object.Destroy(this.models[this.holdingItemIdx].gameObject);
		}
		ItemStack holdingItemStack = this.holdingItemStack;
		ItemValue itemValue = holdingItemStack.itemValue.Clone();
		int count = holdingItemStack.count;
		if (itemValue.ItemClass != null)
		{
			this.models[this.holdingItemIdx] = (itemValue.ItemClass.CanHold() ? this.createHeldItem(this.holdingItemIdx, itemValue) : null);
			if (this.slots[this.holdingItemIdx] == null || !(this.slots[this.holdingItemIdx] is ItemClassBlock.ItemBlockInventoryData) || !(itemValue.ItemClass is ItemClassBlock))
			{
				this.slots[this.holdingItemIdx] = this.createInventoryData(this.holdingItemIdx, itemValue);
			}
		}
		this.slots[this.holdingItemIdx].itemStack.itemValue = itemValue;
		this.slots[this.holdingItemIdx].itemStack.count = count;
		this.m_LastDrawnHoldingItemIndex = -1;
		this.updateHoldingItem();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateHoldingItem()
	{
		ItemValue holdingItemItemValue = this.holdingItemItemValue;
		if (this.lastDrawnHoldingItemValue == holdingItemItemValue && this.m_LastDrawnHoldingItemIndex == this.holdingItemIdx)
		{
			this.holdingItem.OnHoldingReset(this.holdingItemData);
			return;
		}
		this.entity.bPlayerStatsChanged = !this.entity.isEntityRemote;
		if (this.lastdrawnHoldingItem != null)
		{
			this.entity.MinEventContext.ItemValue = this.lastDrawnHoldingItemValue;
			this.entity.MinEventContext.Transform = this.lastdrawnHoldingItemTransform;
			this.lastdrawnHoldingItem.StopHolding(this.lastdrawnHoldingItemData, this.lastdrawnHoldingItemTransform);
			if (!this.lastDrawnHoldingItemValue.ItemClass.ItemTags.Test_AnySet(this.ignoreWhenHeld))
			{
				this.lastDrawnHoldingItemValue.FireEvent(MinEventTypes.onSelfEquipStop, this.entity.MinEventContext);
			}
			if (this.lastdrawnHoldingItemTransform != null)
			{
				this.InitInactiveItemsObject();
				this.lastdrawnHoldingItemTransform.SetParent(this.inactiveItems, false);
				this.lastdrawnHoldingItemTransform.gameObject.SetActive(false);
			}
		}
		QuestEventManager.Current.HeldItem(this.holdingItemData.itemValue);
		this.holdingItem.StartHolding(this.holdingItemData, this.models[this.holdingItemIdx]);
		this.entity.MinEventContext.ItemValue = holdingItemItemValue;
		this.entity.MinEventContext.ItemValue.Seed = holdingItemItemValue.Seed;
		this.entity.MinEventContext.Transform = this.models[this.holdingItemIdx];
		this.setHoldingItemTransform(this.models[this.holdingItemIdx]);
		this.ShowRightHand(true);
		holdingItemItemValue.FireEvent(MinEventTypes.onSelfHoldingItemCreated, this.entity.MinEventContext);
		if (!holdingItemItemValue.ItemClass.ItemTags.Test_AnySet(this.ignoreWhenHeld))
		{
			holdingItemItemValue.FireEvent(MinEventTypes.onSelfEquipStart, this.entity.MinEventContext);
		}
		this.entity.OnHoldingItemChanged();
		this.m_LastDrawnHoldingItemIndex = this.m_HoldingItemIdx;
		this.lastdrawnHoldingItem = this.holdingItem;
		this.lastDrawnHoldingItemValue = holdingItemItemValue;
		this.lastdrawnHoldingItemTransform = this.models[this.holdingItemIdx];
		this.lastdrawnHoldingItemData = this.holdingItemData;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setHoldingItemTransform(Transform _t)
	{
		this.entity.SetHoldingItemTransform(_t);
		if (_t != null)
		{
			_t.position = Vector3.zero;
			_t.localPosition = (this.entity.emodel.IsFPV ? Vector3.zero : AnimationGunjointOffsetData.AnimationGunjointOffset[this.entity.inventory.holdingItem.HoldType.Value].position);
			_t.eulerAngles = Vector3.zero;
			_t.localEulerAngles = (this.entity.emodel.IsFPV ? Vector3.zero : AnimationGunjointOffsetData.AnimationGunjointOffset[this.entity.inventory.holdingItem.HoldType.Value].rotation);
			_t.localEulerAngles = _t.localRotation.eulerAngles;
			if (!this.holdingItem.GetCorrectionScale().Equals(Vector3.zero))
			{
				_t.localScale = this.holdingItem.GetCorrectionScale();
			}
			_t.gameObject.SetActive(!this.holdingItem.HoldingItemHidden);
		}
		this.syncHeldItem();
		this.lastdrawnHoldingItemTransform = _t;
	}

	public int PreferredItemSlot(int _itemType, int _startSlotIdx)
	{
		for (int i = _startSlotIdx; i < this.preferredItemSlots.Length; i++)
		{
			if (this.preferredItemSlots[i] == _itemType)
			{
				return i;
			}
		}
		return -1;
	}

	public void ClearPreferredItemInSlot(int _slotIdx)
	{
		if (_slotIdx < this.preferredItemSlots.Length)
		{
			this.preferredItemSlots[_slotIdx] = 0;
		}
	}

	public bool CanStack(ItemStack _itemStack)
	{
		for (int i = 0; i < this.PUBLIC_SLOTS; i++)
		{
			if (this.slots[i].itemStack.IsEmpty() || this.slots[i].itemStack.CanStackWith(_itemStack, false))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanStackNoEmpty(ItemStack _itemStack)
	{
		for (int i = 0; i < this.PUBLIC_SLOTS; i++)
		{
			int num;
			if (this.slots[i].itemStack.CanStackPartlyWith(_itemStack, out num))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryStackItem(int startIndex, ItemStack _itemStack)
	{
		int num = 0;
		for (int i = startIndex; i < this.PUBLIC_SLOTS; i++)
		{
			num = _itemStack.count;
			ItemStack itemStack = this.slots[i].itemStack;
			if (_itemStack.itemValue.type == itemStack.itemValue.type && !itemStack.IsEmpty() && itemStack.CanStackPartly(ref num))
			{
				itemStack.count += num;
				_itemStack.count -= num;
				this.notifyListeners();
				this.entity.bPlayerStatsChanged = !this.entity.isEntityRemote;
				if (_itemStack.count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool TryTakeItem(ItemStack _itemStack)
	{
		int num = 0;
		for (int i = 0; i < this.PUBLIC_SLOTS; i++)
		{
			num = _itemStack.count;
			ItemStack itemStack = this.slots[i].itemStack;
			if (itemStack.IsEmpty())
			{
				itemStack = _itemStack.Clone();
				this.notifyListeners();
				this.entity.bPlayerStatsChanged = !this.entity.isEntityRemote;
				return true;
			}
			if (_itemStack.itemValue.type == itemStack.itemValue.type && !itemStack.IsEmpty() && itemStack.CanStackPartly(ref num))
			{
				itemStack.count += num;
				_itemStack.count -= num;
				this.notifyListeners();
				this.entity.bPlayerStatsChanged = !this.entity.isEntityRemote;
				if (_itemStack.count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool CanTakeItem(ItemStack _itemStack)
	{
		for (int i = 0; i < this.slots.Length - 1; i++)
		{
			int num;
			if (this.slots[i].itemStack.CanStackPartlyWith(_itemStack, out num))
			{
				return true;
			}
			if (this.slots[i].itemStack.IsEmpty())
			{
				return true;
			}
		}
		return false;
	}

	public virtual void AddChangeListener(IInventoryChangedListener _listener)
	{
		if (this.listeners == null)
		{
			this.listeners = new HashSet<IInventoryChangedListener>();
		}
		this.listeners.Add(_listener);
		_listener.OnInventoryChanged(this);
	}

	public virtual void RemoveChangeListener(IInventoryChangedListener _listener)
	{
		if (this.listeners != null)
		{
			this.listeners.Remove(_listener);
		}
	}

	public void Changed()
	{
		this.notifyListeners();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void notifyListeners()
	{
		this.onInventoryChanged();
		if (this.listeners == null)
		{
			return;
		}
		foreach (IInventoryChangedListener inventoryChangedListener in this.listeners)
		{
			inventoryChangedListener.OnInventoryChanged(this);
		}
	}

	public virtual bool IsHUDDisabled()
	{
		return this.holdingItem.IsHUDDisabled(this.holdingItemData);
	}

	public string CanInteract()
	{
		return this.holdingItem.CanInteract(this.holdingItemData);
	}

	public void Interact()
	{
		this.holdingItem.Interact(this.holdingItemData);
	}

	public virtual void Execute(int _actionIdx, bool _bReleased, PlayerActionsLocal _playerActions = null)
	{
		if (this.IsHolsterDelayActive() || this.IsUnholsterDelayActive())
		{
			return;
		}
		if (this.WaitForSecondaryRelease && _actionIdx == 1)
		{
			if (!_bReleased)
			{
				return;
			}
			this.WaitForSecondaryRelease = false;
		}
		this.holdingItem.ExecuteAction(_actionIdx, this.holdingItemData, _bReleased, _playerActions);
	}

	public void ReleaseAll(PlayerActionsLocal _playerActions = null)
	{
		ItemClass holdingItem = this.holdingItem;
		for (int i = 0; i < holdingItem.Actions.Length; i++)
		{
			this.Execute(i, true, _playerActions);
		}
	}

	public void Clear()
	{
		for (int i = 0; i < this.slots.Length; i++)
		{
			this.slots[i] = this.emptyItem;
		}
	}

	public void Cleanup()
	{
		if (this.delayedShowHideHeldItemRoutine != null)
		{
			GameManager.Instance.StopCoroutine(this.delayedShowHideHeldItemRoutine);
			this.delayedShowHideHeldItemRoutine = null;
		}
		this.CleanupHoldingActions();
		this.ResetActiveIndex();
		this.entity = null;
		this.listeners = null;
		this.slots = null;
		this.models = null;
	}

	public void CleanupHoldingActions()
	{
		if (this.holdingItem != null && this.holdingItemData != null)
		{
			this.holdingItem.CleanupHoldingActions(this.holdingItemData);
		}
	}

	public ItemStack[] GetSlots()
	{
		ItemStack[] array = new ItemStack[this.slots.Length];
		for (int i = 0; i < this.slots.Length; i++)
		{
			array[i] = this.slots[i].itemStack;
		}
		return array;
	}

	public int GetSlotCount()
	{
		return this.slots.Length;
	}

	public void PerformActionOnSlots(Action<ItemStack> _action)
	{
		for (int i = 0; i < this.slots.Length; i++)
		{
			_action(this.slots[i].itemStack);
		}
	}

	public int GetSlotWithItemValue(ItemValue _itemValue)
	{
		for (int i = 0; i < this.slots.Length; i++)
		{
			if (this.slots[i].itemValue.Equals(_itemValue))
			{
				return i;
			}
		}
		return -1;
	}

	public List<int> GetSlotsWithBlock(Block _block)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < this.slots.Length; i++)
		{
			if (this.slots[i].item != null && this.slots[i].item.IsBlock())
			{
				if (_block.CanPickup)
				{
					if (this.slots[i].item.Name.Equals(_block.PickedUpItemValue))
					{
						list.Add(i);
					}
				}
				else if ((this.slots[i].item as ItemClassBlock).GetBlock().Equals(_block))
				{
					list.Add(i);
				}
			}
		}
		return list;
	}

	public int GetBestQuickSwapSlot()
	{
		if (this.quickSwapSlotIdx == -1 || this.quickSwapItemValue == null)
		{
			return -1;
		}
		if (this.slots[this.quickSwapSlotIdx].itemValue.GetItemId() == this.quickSwapItemValue.GetItemId())
		{
			return this.quickSwapSlotIdx;
		}
		for (int i = 0; i < this.slots.Length; i++)
		{
			if (this.slots[i].itemValue.GetItemId() == this.quickSwapItemValue.GetItemId())
			{
				return i;
			}
		}
		return -1;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <SimulateActionExecution>g__IsDummySlotActive|138_0(ref Inventory.<>c__DisplayClass138_0 A_1)
	{
		return this.m_HoldingItemIdx == this.DUMMY_SLOT_IDX && !this.GetItem(this.DUMMY_SLOT_IDX).IsEmpty();
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <SimulateActionExecution>g__HandleComplete|138_1(ref Inventory.<>c__DisplayClass138_0 A_1)
	{
		A_1.dummyItem = this.GetItem(this.DUMMY_SLOT_IDX);
		ItemStack itemStack;
		if (object.Equals(A_1._itemStack, A_1.dummyItem))
		{
			itemStack = A_1._itemStack;
		}
		else
		{
			itemStack = A_1.dummyItem;
		}
		if (this.m_FocusedItemIdx == this.DUMMY_SLOT_IDX)
		{
			this.SetHoldingItemIdx(A_1.previousHoldingIdx);
			this.SetFocusedItemIdx(A_1.previousFocusedIdx);
		}
		this.SetItem(this.DUMMY_SLOT_IDX, ItemStack.Empty);
		A_1.onComplete(itemStack);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly FastTags<TagGroup.Global> ignoreWhenHeld = FastTags<TagGroup.Global>.Parse("clothing,armor");

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemInventoryData[] slots;

	public Transform[] models;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] preferredItemSlots;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass wearingActiveItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue wearingActiveItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass lastdrawnHoldingItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue lastDrawnHoldingItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform lastdrawnHoldingItemTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemInventoryData lastdrawnHoldingItemData;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool flashlightOn;

	public ItemStack itemArmor;

	public ItemStack itemOnBack;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_HoldingItemIdx;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_LastDrawnHoldingItemIndex;

	public Transform inactiveItems;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive entity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IGameManager gameManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue bareHandItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass bareHandItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemInventoryData bareHandItemInventoryData;

	[PublicizedFrom(EAccessModifier.Protected)]
	public HashSet<IInventoryChangedListener> listeners;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_FocusedItemIdx;

	public Inventory.HeldItemState HoldState;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemInventoryData emptyItem;

	public bool WaitForSecondaryRelease;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue previousHeldItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int previousHeldItemSlotIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue quickSwapItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int quickSwapSlotIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine delayedShowHideHeldItemRoutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSwitchingHeldItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currActiveItemIndex = -2;

	public bool bResetLightLevelWhenChanged = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemValue> activatables = new List<ItemValue>();

	public enum HeldItemState
	{
		None,
		Unholstering,
		Holding,
		Holstering
	}

	public enum ActiveIndex
	{
		NOT_INITIALIZED = -2,
		ALL_OFF
	}
}
