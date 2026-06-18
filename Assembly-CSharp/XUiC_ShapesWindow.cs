using System;
using System.Collections.Generic;
using GUI_2;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ShapesWindow : XUiController
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
				this.shapeGrid.Page = this.page;
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
		this.shapeGrid = base.Parent.GetChildByType<XUiC_ShapeStackGrid>();
		XUiController[] childrenByType = this.shapeGrid.GetChildrenByType<XUiC_ShapeStack>(null);
		XUiController[] array = childrenByType;
		this.shapeGrid.Owner = this;
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
		XUiController childById = base.GetChildById("favorites");
		if (childById != null)
		{
			this.favoritesBtn = childById.ViewComponent as XUiV_Button;
			if (this.favoritesBtn != null)
			{
				childById.OnPress += this.HandleFavoritesChanged;
			}
		}
		this.lblTotal = Localization.Get("lblTotalItems", false, null);
		this.categoryList = (XUiC_CategoryList)base.GetChildById("categories");
		if (this.categoryList != null)
		{
			this.categoryList.AllowUnselect = true;
			this.categoryList.CategoryChanged += this.CategoryList_CategoryChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleFavoritesChanged(XUiController _sender, int _mouseButton)
	{
		this.showFavorites = !this.showFavorites;
		this.favoritesBtn.Selected = this.showFavorites;
		this.UpdateAll();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.Page = 0;
		this.UpdateShapesList();
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

	public void UpgradeDowngradeShapes(BlockValue _targetBv)
	{
		string blockName = _targetBv.Block.GetBlockName();
		Block autoShapeHelperBlock = _targetBv.Block.GetAutoShapeHelperBlock();
		ItemValue itemValue = new BlockValue((uint)autoShapeHelperBlock.blockID).ToItemValue();
		itemValue.Meta = autoShapeHelperBlock.GetAlternateBlockIndex(blockName);
		this.ItemValue = itemValue;
		this.UpdateAll();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAltList()
	{
		this.altBlocks = this.ItemValue.ToBlockValue(false).Block.GetAltBlocks();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCategories()
	{
		XUiC_CategoryEntry currentCategory = this.categoryList.CurrentCategory;
		string text = ((currentCategory != null) ? currentCategory.CategoryName : null);
		Block.GetShapeCategories(this.altBlocks, this.shapeCategories);
		for (int i = 0; i < this.categoryList.Children.Count; i++)
		{
			if (i < this.shapeCategories.Count)
			{
				this.categoryList.SetCategoryEntry(i, this.shapeCategories[i].Name, this.shapeCategories[i].Icon, this.shapeCategories[i].LocalizedName);
			}
			else
			{
				this.categoryList.SetCategoryEmpty(i);
			}
		}
		if (text != null)
		{
			this.categoryList.SetCategory(text);
			return;
		}
		if (!this.openedBefore)
		{
			this.categoryList.SetCategoryToFirst();
			this.openedBefore = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateShapesList()
	{
		List<string> favoriteShapes = this.xui.playerUI.entityPlayer.favoriteShapes;
		this.currentItems.Clear();
		this.length = this.shapeGrid.Length;
		string text = this.txtInput.Text;
		XUiC_CategoryEntry currentCategory = this.categoryList.CurrentCategory;
		string text2 = ((currentCategory != null) ? currentCategory.CategoryName : null);
		ShapesFromXml.ShapeCategory shapeCategory = null;
		if (!string.IsNullOrEmpty(text2))
		{
			shapeCategory = ShapesFromXml.shapeCategories[text2];
		}
		for (int i = 0; i < this.altBlocks.Length; i++)
		{
			Block block = this.altBlocks[i];
			string blockName = block.GetBlockName();
			string localizedBlockName = block.GetLocalizedBlockName();
			if ((!this.showFavorites || favoriteShapes.Contains(XUiC_ShapeStack.GetFavoritesEntryName(block))) && (string.IsNullOrEmpty(text) || blockName.ContainsCaseInsensitive(text) || localizedBlockName.ContainsCaseInsensitive(text)) && (shapeCategory == null || block.ShapeCategories.Contains(shapeCategory)) && !block.Properties.GetString("ShapeMenu").EqualsCaseInsensitive("false"))
			{
				this.currentItems.Add(new XUiC_ShapeStackGrid.ShapeData
				{
					Block = block,
					Index = i
				});
			}
		}
		this.currentItems.Sort((XUiC_ShapeStackGrid.ShapeData _shapeA, XUiC_ShapeStackGrid.ShapeData _shapeB) => StringComparer.Ordinal.Compare(_shapeA.Block.SortOrder, _shapeB.Block.SortOrder));
		XUiC_Paging xuiC_Paging = this.pager;
		if (xuiC_Paging != null)
		{
			xuiC_Paging.SetLastPageByElementsAndPageLength(this.currentItems.Count, this.length);
		}
		this.shapeGrid.SetShapes(this.currentItems, this.ItemValue.Meta);
		int num = this.currentItems.FindIndex((XUiC_ShapeStackGrid.ShapeData _data) => _data.Index == this.ItemValue.Meta);
		if (num < 0)
		{
			this.Page = 0;
		}
		else
		{
			this.Page = num / this.length;
		}
		this.resultCount.Text = string.Format(this.lblTotal, this.currentItems.Count.ToString());
	}

	public void UpdateAll()
	{
		this.updateAltList();
		this.updateCategories();
		this.UpdateShapesList();
		this.IsDirty = true;
	}

	public void RefreshItemStack()
	{
		XUiC_ItemStack stackController = this.StackController;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoCategoryLeft", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoCategoryRight", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory, 0f);
		this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoToggleFavorite", XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts, 0f);
		int holdingItemIdx = this.xui.playerUI.entityPlayer.inventory.holdingItemIdx;
		XUiC_Toolbelt childByType = ((XUiWindowGroup)this.xui.playerUI.windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>();
		this.xui.DragAndDropWindow.InMenu = true;
		if (childByType != null)
		{
			this.StackController = childByType.GetSlotControl(holdingItemIdx);
			this.StackController.AssembleLock = true;
		}
		this.windowGroup.Controller.GetChildByType<XUiC_WindowNonPagingHeader>().SetHeader(Localization.Get("xuiShapes", false, null).ToUpper());
		this.UpdateAll();
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.ToolTipWindow.ToolTip = "";
		this.xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		this.xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		bool childByType = ((XUiWindowGroup)this.xui.playerUI.windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>() != null;
		this.xui.DragAndDropWindow.InMenu = false;
		if (childByType)
		{
			this.StackController.AssembleLock = false;
			this.StackController.ItemStack = new ItemStack(this.ItemValue, this.StackController.ItemStack.count);
			this.ItemValue = ItemValue.None;
			this.StackController.ForceRefreshItemStack();
		}
		if (this.xui.playerUI.windowManager.IsWindowOpen("windowpaging"))
		{
			this.xui.playerUI.windowManager.Close("windowpaging");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		this.UpdateShapesList();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.viewComponent.IsVisible && this.IsDirty)
		{
			base.RefreshBindings();
			this.shapeGrid.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "keyboardonly")
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				_value = "true";
			}
			else
			{
				_value = "false";
			}
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label resultCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ShapeStackGrid shapeGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showFavorites;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button favoritesBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController paintbrushButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController paintrollerButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblPaintEyeDropper;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblCopyBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotal;

	public ItemValue ItemValue = ItemValue.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public Block[] altBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ShapesFromXml.ShapeCategory> shapeCategories = new List<ShapesFromXml.ShapeCategory>();

	public XUiC_ItemStack StackController;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_ShapeStackGrid.ShapeData> currentItems = new List<XUiC_ShapeStackGrid.ShapeData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openedBefore;
}
