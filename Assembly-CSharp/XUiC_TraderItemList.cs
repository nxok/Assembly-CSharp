using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TraderItemList : XUiController
{
	public ItemStack CurrentItem
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			this.page = value;
		}
	}

	public XUiC_ItemInfoWindow InfoWindow { get; set; }

	public XUiC_TraderItemEntry SelectedEntry
	{
		get
		{
			return this.selectedEntry;
		}
		set
		{
			if (this.selectedEntry != null)
			{
				this.selectedEntry.IsSelected = false;
			}
			this.selectedEntry = value;
			if (this.selectedEntry != null)
			{
				this.selectedEntry.IsSelected = true;
				for (int i = 0; i < this.indexList.Count; i++)
				{
					if (this.selectedEntry.SlotIndex == this.indexList[i])
					{
						this.selectedIndex = i % this.entryList.Count;
						break;
					}
				}
				this.InfoWindow.ViewComponent.IsVisible = true;
				this.InfoWindow.SetItemStack(this.selectedEntry, false);
				this.CurrentItem = this.selectedEntry.Item;
				return;
			}
			if (this.selectedIndex >= 0)
			{
				if (this.entryList[this.selectedIndex].Item != null)
				{
					this.SelectedEntry = this.entryList[this.selectedIndex];
				}
				else if (this.entryList[0].Item != null)
				{
					this.SelectedEntry = this.entryList[0];
				}
			}
			if (this.SelectedEntry == null)
			{
				this.selectedIndex = -1;
				this.InfoWindow.SetItemStack(null, false);
				this.CurrentItem = null;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < this.children.Count; i++)
		{
			XUiController xuiController = this.children[i];
			if (xuiController is XUiC_TraderItemEntry)
			{
				this.entryList.Add((XUiC_TraderItemEntry)xuiController);
			}
		}
		XUiV_Grid xuiV_Grid = (XUiV_Grid)base.ViewComponent;
		if (xuiV_Grid != null)
		{
			this.Length = xuiV_Grid.Columns * xuiV_Grid.Rows;
		}
		this.InfoWindow = this.xui.GetChildByType<XUiC_ItemInfoWindow>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressEntry(XUiController _sender, int _mouseButton)
	{
		XUiC_TraderItemEntry xuiC_TraderItemEntry = _sender as XUiC_TraderItemEntry;
		if (xuiC_TraderItemEntry != null)
		{
			this.SelectedEntry = xuiC_TraderItemEntry;
			if (InputUtils.ShiftKeyPressed)
			{
				xuiC_TraderItemEntry.InfoWindow.BuySellCounter.SetToMaxCount();
				return;
			}
			xuiC_TraderItemEntry.InfoWindow.BuySellCounter.Count = xuiC_TraderItemEntry.Item.itemValue.ItemClass.EconomicBundleSize;
		}
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
		this.ClearSelection();
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
		this.ClearSelection();
	}

	public void ClearSelection()
	{
		this.CurrentItem = null;
		this.selectedIndex = -1;
		this.SelectedEntry = null;
		this.InfoWindow.SetItemStack(null, false);
	}

	public void SetItems(ItemStack[] _stackList, List<int> _indexList)
	{
		if (_stackList == null)
		{
			return;
		}
		this.items.Clear();
		this.items.AddRange(_stackList);
		this.indexList.Clear();
		this.indexList.AddRange(_indexList);
		XUiC_ItemInfoWindow childByType = this.xui.GetChildByType<XUiC_ItemInfoWindow>();
		for (int i = 0; i < this.Length; i++)
		{
			int num = i + this.Length * this.page;
			this.entryList[i].OnPress -= this.OnPressEntry;
			this.entryList[i].InfoWindow = childByType;
			if (num < this.items.Count)
			{
				this.entryList[i].SlotIndex = _indexList[num];
				this.entryList[i].Item = _stackList[num];
				this.entryList[i].OnPress += this.OnPressEntry;
				this.entryList[i].ViewComponent.SoundPlayOnClick = true;
			}
			else
			{
				this.entryList[i].Item = null;
				this.entryList[i].ViewComponent.SoundPlayOnClick = false;
			}
		}
		if (this.CurrentItem != null)
		{
			ItemValue itemValue = this.CurrentItem.itemValue;
			XUiC_TraderItemEntry xuiC_TraderItemEntry = this.SelectedEntry;
			ItemValue itemValue2;
			if (xuiC_TraderItemEntry == null)
			{
				itemValue2 = null;
			}
			else
			{
				ItemStack item = xuiC_TraderItemEntry.Item;
				itemValue2 = ((item != null) ? item.itemValue : null);
			}
			if (itemValue.Equals(itemValue2))
			{
				return;
			}
		}
		this.ClearSelection();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TraderItemEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> items = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> indexList = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	public int Length;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showFavorites;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectedColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public string category;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_TraderItemEntry> entryList = new List<XUiC_TraderItemEntry>();
}
