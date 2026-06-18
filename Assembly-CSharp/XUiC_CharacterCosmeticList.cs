using System;
using System.Collections.Generic;
using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterCosmeticList : XUiController
{
	public XUiC_CharacterCosmeticEntry SelectedEntry
	{
		get
		{
			return this.selectedEntry;
		}
		set
		{
			if (this.selectedEntry != null)
			{
				this.selectedEntry.Selected = false;
			}
			this.selectedEntry = value;
			if (this.selectedEntry != null)
			{
				this.selectedEntry.Selected = true;
				this.SetTempCosmeticSlot(this.selectedEntry);
			}
			this.Owner.RefreshBindings();
		}
	}

	public override void Init()
	{
		base.Init();
		this.entryList = base.GetChildrenByType<XUiC_CharacterCosmeticEntry>(null);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.player = this.xui.playerUI.entityPlayer;
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.PlayerEquipment.Equipment.ClearTempCosmeticSlot();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.xui.playerUI.entityPlayer.AimingGun = false;
		bool isVisible = base.ViewComponent.IsVisible;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCallouts(bool _holdingItem)
	{
		string text = string.Format(Localization.Get("igcoItemActions", false, null), InControlExtensions.GetBlankDPadSourceString());
		XUiC_GamepadCalloutWindow calloutWindow = this.xui.CalloutWindow;
		if (calloutWindow != null)
		{
			calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
			if (!_holdingItem)
			{
				calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoInspect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
				calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, text, XUiC_GamepadCalloutWindow.CalloutType.Menu);
				calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			}
			calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		}
	}

	public void SetCosmeticList(List<ItemClass> _currentItems, EquipmentSlots slot)
	{
		this.currentItems = _currentItems;
		if (this.currentItems == null)
		{
			return;
		}
		ItemClass cosmeticSlot = this.xui.PlayerEquipment.Equipment.GetCosmeticSlot((int)slot, false);
		this.SelectedEntry = null;
		int i = 0;
		for (int j = 0; j < this.entryList.Length; j++)
		{
			XUiC_CharacterCosmeticEntry xuiC_CharacterCosmeticEntry = this.entryList[j];
			xuiC_CharacterCosmeticEntry.OnPress -= this.OnPressCosmetic;
			xuiC_CharacterCosmeticEntry.Selected = false;
			xuiC_CharacterCosmeticEntry.Owner = this;
		}
		XUiC_CharacterCosmeticEntry xuiC_CharacterCosmeticEntry2 = this.entryList[0];
		xuiC_CharacterCosmeticEntry2.EntryType = XUiC_CharacterCosmeticEntry.EntryTypes.Empty;
		xuiC_CharacterCosmeticEntry2.ItemClass = null;
		xuiC_CharacterCosmeticEntry2.Index = (int)slot;
		xuiC_CharacterCosmeticEntry2.OnPress += this.OnPressCosmetic;
		if (xuiC_CharacterCosmeticEntry2.ItemClass == cosmeticSlot)
		{
			this.SelectedEntry = xuiC_CharacterCosmeticEntry2;
		}
		i++;
		XUiC_CharacterCosmeticEntry xuiC_CharacterCosmeticEntry3 = this.entryList[1];
		xuiC_CharacterCosmeticEntry3.EntryType = XUiC_CharacterCosmeticEntry.EntryTypes.Hide;
		xuiC_CharacterCosmeticEntry3.ItemClass = ItemClass.MissingItem;
		xuiC_CharacterCosmeticEntry3.Index = (int)slot;
		xuiC_CharacterCosmeticEntry3.OnPress += this.OnPressCosmetic;
		if (xuiC_CharacterCosmeticEntry3.ItemClass == cosmeticSlot)
		{
			this.SelectedEntry = xuiC_CharacterCosmeticEntry3;
		}
		i++;
		int num = 0;
		while (i < this.entryList.Length)
		{
			if (num >= this.currentItems.Count)
			{
				break;
			}
			XUiC_CharacterCosmeticEntry xuiC_CharacterCosmeticEntry4 = this.entryList[i];
			xuiC_CharacterCosmeticEntry4.EntryType = XUiC_CharacterCosmeticEntry.EntryTypes.Item;
			xuiC_CharacterCosmeticEntry4.ItemClass = this.currentItems[num];
			xuiC_CharacterCosmeticEntry4.Index = (int)slot;
			xuiC_CharacterCosmeticEntry4.OnPress += this.OnPressCosmetic;
			if (xuiC_CharacterCosmeticEntry4.ItemClass == cosmeticSlot)
			{
				this.SelectedEntry = xuiC_CharacterCosmeticEntry4;
			}
			i++;
			num++;
		}
		while (i < this.entryList.Length)
		{
			XUiC_CharacterCosmeticEntry xuiC_CharacterCosmeticEntry5 = this.entryList[i];
			xuiC_CharacterCosmeticEntry5.EntryType = XUiC_CharacterCosmeticEntry.EntryTypes.Item;
			xuiC_CharacterCosmeticEntry5.ItemClass = null;
			xuiC_CharacterCosmeticEntry5.Index = (int)slot;
			i++;
		}
		this.entryList[0].SelectCursorElement(true, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressCosmetic(XUiController _sender, int _mouseButton)
	{
		XUiC_CharacterCosmeticEntry xuiC_CharacterCosmeticEntry = _sender as XUiC_CharacterCosmeticEntry;
		if (xuiC_CharacterCosmeticEntry != null)
		{
			if (this.SelectedEntry == xuiC_CharacterCosmeticEntry)
			{
				return;
			}
			this.SelectedEntry = xuiC_CharacterCosmeticEntry;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetTempCosmeticSlot(XUiC_CharacterCosmeticEntry _entry)
	{
		if (_entry == null)
		{
			return;
		}
		switch (_entry.EntryType)
		{
		case XUiC_CharacterCosmeticEntry.EntryTypes.Item:
			this.xui.PlayerEquipment.Equipment.SetTempCosmeticSlot(_entry.Index, _entry.ItemClass);
			break;
		case XUiC_CharacterCosmeticEntry.EntryTypes.Empty:
			this.xui.PlayerEquipment.Equipment.SetTempCosmeticSlot(_entry.Index, null);
			break;
		case XUiC_CharacterCosmeticEntry.EntryTypes.Hide:
			this.xui.PlayerEquipment.Equipment.SetTempCosmeticSlot(_entry.Index, ItemClass.MissingItem);
			break;
		}
		((XUiC_CharacterCosmeticWindowGroup)base.WindowGroup.Controller).ResetPreview();
	}

	public void SetEmptyCosmetic()
	{
		this.SelectedEntry = this.entryList[0];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterCosmeticEntry[] entryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterCosmeticEntry selectedEntry;

	public XUiC_CharacterCosmeticsListWindow Owner;

	public List<ItemClass> currentItems;
}
