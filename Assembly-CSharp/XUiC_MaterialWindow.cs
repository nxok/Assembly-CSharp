using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MaterialWindow : XUiController
{
	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			if (this.page != value)
			{
				this.page = value;
				this.materialGrid.Page = this.page;
				XUiC_Paging xuiC_Paging = this.pager;
				if (xuiC_Paging == null)
				{
					return;
				}
				xuiC_Paging.SetPage(this.page);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		this.resultCount = (XUiV_Label)base.GetChildById("resultCount").ViewComponent;
		this.pager = base.GetChildByType<XUiC_Paging>();
		if (this.pager != null)
		{
			this.pager.OnPageChanged += delegate
			{
				this.Page = this.pager.CurrentPageNumber;
			};
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnScroll += this.HandleOnScroll;
		}
		base.OnScroll += this.HandleOnScroll;
		this.materialGrid = base.Parent.GetChildByType<XUiC_MaterialStackGrid>();
		XUiController[] childrenByType = this.materialGrid.GetChildrenByType<XUiC_MaterialStack>(null);
		XUiController[] array = childrenByType;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].OnScroll += this.HandleOnScroll;
		}
		this.length = array.Length;
		this.txtInput = (XUiC_TextInput)this.windowGroup.Controller.GetChildById("searchInput");
		if (this.txtInput != null)
		{
			this.txtInput.OnChangeHandler += this.HandleOnChangedHandler;
			this.txtInput.Text = "";
		}
		this.lblTotal = Localization.Get("lblTotalItems", false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.Page = 0;
		this.GetMaterialData(this.txtInput.Text);
		this.materialGrid.SetMaterials(this.currentItems, this.CurrentPaintId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			XUiC_Paging xuiC_Paging = this.pager;
			if (xuiC_Paging == null)
			{
				return;
			}
			xuiC_Paging.PageDown();
			return;
		}
		else
		{
			XUiC_Paging xuiC_Paging2 = this.pager;
			if (xuiC_Paging2 == null)
			{
				return;
			}
			xuiC_Paging2.PageUp();
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetMaterialData(string _name)
	{
		if (_name == null)
		{
			_name = "";
		}
		this.currentItems.Clear();
		this.length = this.materialGrid.Length;
		this.Page = 0;
		this.FilterByName(_name);
	}

	public void FilterByName(string _name)
	{
		bool flag = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) && GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
		this.currentItems.Clear();
		for (int i = 0; i < BlockTextureData.list.Length; i++)
		{
			BlockTextureData blockTextureData = BlockTextureData.list[i];
			if (blockTextureData != null && (!blockTextureData.Hidden || flag))
			{
				if (_name != "")
				{
					string name = blockTextureData.Name;
					if (_name == "" || name.ContainsCaseInsensitive(_name))
					{
						this.currentItems.Add(blockTextureData);
					}
				}
				else
				{
					this.currentItems.Add(blockTextureData);
				}
			}
		}
		XUiC_Paging xuiC_Paging = this.pager;
		if (xuiC_Paging != null)
		{
			xuiC_Paging.SetLastPageByElementsAndPageLength(this.currentItems.Count, this.length);
		}
		this.resultCount.Text = string.Format(this.lblTotal, this.currentItems.Count.ToString());
	}

	public int CurrentPaintId
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.xui.playerUI.entityPlayer.inventory.holdingItem is ItemClassBlock)
			{
				return (int)(this.xui.playerUI.entityPlayer.inventory.holdingItemItemValue.TextureFullArray[0] & 255L);
			}
			return ((ItemActionTextureBlock.ItemActionTextureBlockData)this.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1]).idx;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.GetMaterialData(this.txtInput.Text);
		this.IsDirty = true;
		int currentPaintId = this.CurrentPaintId;
		this.materialGrid.SetMaterials(this.currentItems, currentPaintId);
		int holdingItemIdx = this.xui.playerUI.entityPlayer.inventory.holdingItemIdx;
		XUiC_Toolbelt childByType = ((XUiWindowGroup)this.xui.playerUI.windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>();
		this.xui.DragAndDropWindow.InMenu = true;
		if (childByType != null)
		{
			childByType.GetSlotControl(holdingItemIdx).AssembleLock = true;
		}
		this.windowGroup.Controller.GetChildByType<XUiC_WindowNonPagingHeader>().SetHeader(Localization.Get("xuiMaterials", false, null).ToUpper());
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.ToolTipWindow.ToolTip = "";
		int holdingItemIdx = this.xui.playerUI.entityPlayer.inventory.holdingItemIdx;
		XUiController childByType = ((XUiWindowGroup)this.xui.playerUI.windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>();
		this.xui.DragAndDropWindow.InMenu = false;
		if (childByType != null)
		{
			(childByType as XUiC_Toolbelt).GetSlotControl(holdingItemIdx).AssembleLock = false;
		}
		if (this.xui.playerUI.windowManager.IsWindowOpen("windowpaging"))
		{
			this.xui.playerUI.windowManager.Close("windowpaging");
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.viewComponent.IsVisible)
		{
			if (null != this.xui.playerUI && this.xui.playerUI.playerInput != null && this.xui.playerUI.playerInput.GUIActions != null)
			{
				PlayerActionsGUI guiactions = this.xui.playerUI.playerInput.GUIActions;
			}
			if (this.IsDirty)
			{
				base.RefreshBindings();
				this.materialGrid.IsDirty = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label resultCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_MaterialStackGrid materialGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPaintEyeDropper;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblCopyBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotal;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<BlockTextureData> currentItems = new List<BlockTextureData>();
}
