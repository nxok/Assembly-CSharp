using System;
using System.Collections.Generic;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EquipmentStackGrid : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_EquipmentStack[] childrenByType = base.GetChildrenByType<XUiC_EquipmentStack>(null);
		this.RegisterExtraSlots(childrenByType);
		this.IsDirty = false;
		XUiM_PlayerEquipment.HandleRefreshEquipment += this.XUiM_PlayerEquipment_HandleRefreshEquipment;
		this.xui.OnShutdown += this.handleShutdown;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment(XUiM_PlayerEquipment _playerEquipment)
	{
		if (this.xui.PlayerEquipment == _playerEquipment)
		{
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleShutdown()
	{
		XUiM_PlayerEquipment.HandleRefreshEquipment -= this.XUiM_PlayerEquipment_HandleRefreshEquipment;
		this.xui.OnShutdown -= this.handleShutdown;
	}

	public void RegisterExtraSlots(IEnumerable<XUiC_EquipmentStack> _slots)
	{
		foreach (XUiC_EquipmentStack xuiC_EquipmentStack in _slots)
		{
			if (xuiC_EquipmentStack.EquipSlot == EquipmentSlots.Count)
			{
				Log.Error("[XUi] EquipmentStack slot does not have a proper 'slot' value");
			}
			this.itemControllers[xuiC_EquipmentStack.EquipSlot] = xuiC_EquipmentStack;
			xuiC_EquipmentStack.SlotChangedEvent += this.HandleSlotChangedEvent;
		}
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (this.IsDirty)
		{
			this.items = this.GetSlots();
			this.SetStacks(this.items);
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	public virtual ItemValue[] GetSlots()
	{
		Equipment equipment = this.xui.PlayerEquipment.Equipment;
		ItemValue[] array = new ItemValue[12];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = equipment.GetSlotItem(i) ?? ItemValue.None;
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetStacks(ItemValue[] _stackList)
	{
		if (_stackList == null)
		{
			return;
		}
		XUiC_ItemInfoWindow childByType = this.xui.GetChildByType<XUiC_ItemInfoWindow>();
		XUiC_CharacterFrameWindow childByType2 = this.xui.GetChildByType<XUiC_CharacterFrameWindow>();
		for (int i = 0; i < _stackList.Length; i++)
		{
			XUiC_EquipmentStack slot = this.GetSlot((EquipmentSlots)i);
			if (slot != null)
			{
				slot.SlotChangedEvent -= this.HandleSlotChangedEvent;
				slot.ItemValue = _stackList[i];
				slot.SlotChangedEvent += this.HandleSlotChangedEvent;
				slot.InfoWindow = childByType;
				slot.FrameWindow = childByType2;
			}
		}
	}

	public void HandleSlotChangedEvent(int _slotNumber, ItemStack _stack)
	{
		if (this.items == null)
		{
			this.items = this.GetSlots();
		}
		if (!_stack.IsEmpty())
		{
			string soundPlace = _stack.itemValue.ItemClass.SoundPlace;
			if (!string.IsNullOrEmpty(soundPlace))
			{
				Manager.PlayInsidePlayerHead(soundPlace, -1, 0f, false, false);
			}
		}
		if (_stack.IsEmpty())
		{
			this.xui.PlayerEquipment.Equipment.SetSlotItem(_slotNumber, null, true);
			this.xui.PlayerEquipment.RefreshEquipment();
			return;
		}
		this.items[_slotNumber] = _stack.itemValue.Clone();
		this.xui.PlayerEquipment.Equipment.SetSlotItem(_slotNumber, _stack.itemValue, true);
		this.xui.PlayerEquipment.RefreshEquipment();
		QuestEventManager.Current.WoreItem(_stack.itemValue);
	}

	public XUiC_EquipmentStack GetSlot(EquipmentSlots _slotType)
	{
		XUiC_EquipmentStack xuiC_EquipmentStack;
		this.itemControllers.TryGetValue(_slotType, out xuiC_EquipmentStack);
		return xuiC_EquipmentStack;
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		foreach (KeyValuePair<EquipmentSlots, XUiC_EquipmentStack> keyValuePair in this.itemControllers)
		{
			EquipmentSlots equipmentSlots;
			XUiC_EquipmentStack xuiC_EquipmentStack;
			keyValuePair.Deconstruct(out equipmentSlots, out xuiC_EquipmentStack);
			xuiC_EquipmentStack.Hovered(false);
		}
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly EnumDictionary<EquipmentSlots, XUiC_EquipmentStack> itemControllers = new EnumDictionary<EquipmentSlots, XUiC_EquipmentStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue[] items;
}
