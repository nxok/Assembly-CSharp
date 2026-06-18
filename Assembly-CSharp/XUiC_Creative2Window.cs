using System;
using System.Collections.Generic;
using System.Text;
using GUI_2;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Creative2Window : XUiController
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
				this.creativeGrid.Page = this.page;
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
		XUiController childById = base.GetChildById("simplepickup");
		if (childById != null)
		{
			this.simpleClickButton = childById.ViewComponent as XUiV_Button;
			if (this.simpleClickButton != null)
			{
				childById.OnPress += this.SimpleClickButton_OnPress;
			}
		}
		this.allowDevBlocks = !(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() || !Submission.Enabled;
		if (this.allowDevBlocks)
		{
			XUiController childById2 = base.GetChildById("devblocks");
			if (childById2 != null)
			{
				this.devBlockButton = childById2.ViewComponent as XUiV_Button;
				if (this.devBlockButton != null)
				{
					childById2.OnPress += this.DevBlockButton_OnPress;
				}
			}
		}
		XUiController childById3 = base.GetChildById("favorites");
		if (childById3 != null)
		{
			this.favorites = childById3.ViewComponent as XUiV_Button;
			if (this.favorites != null)
			{
				childById3.OnPress += this.HandleFavoritesChanged;
			}
		}
		XUiController childById4 = base.GetChildById("hideshapes");
		if (childById4 != null)
		{
			this.hideshapes = childById4.ViewComponent as XUiV_Button;
			if (this.hideshapes != null)
			{
				childById4.OnPress += this.Hideshapes_Changed;
			}
		}
		this.creativeGrid = base.Parent.GetChildByType<XUiC_Creative2StackGrid>();
		XUiC_ItemStack[] childrenByType = this.creativeGrid.GetChildrenByType<XUiC_ItemStack>(null);
		for (int j = 0; j < childrenByType.Length; j++)
		{
			childrenByType[j].OnScroll += this.HandleOnScroll;
		}
		this.txtInput = this.windowGroup.Controller.GetChildById("searchInput") as XUiC_TextInput;
		if (this.txtInput != null)
		{
			this.txtInput.OnChangeHandler += this.TxtInput_OnChange;
			this.txtInput.Text = "";
		}
		this.setupMainCategories((XUiC_CategoryList)base.GetChildById("categories"));
		List<XUiC_CategoryList> list = new List<XUiC_CategoryList>();
		base.GetChildrenByType<XUiC_CategoryList>(list);
		foreach (XUiC_CategoryList xuiC_CategoryList in list)
		{
			if (xuiC_CategoryList.ViewComponent.ID.StartsWith("sub", StringComparison.OrdinalIgnoreCase))
			{
				xuiC_CategoryList.CategoryChanged += this.SubCategory_CategoryChanged;
				xuiC_CategoryList.OnVisiblity += delegate(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
				{
					if (this.isOpening)
					{
						return;
					}
					if (!_visibleInScene)
					{
						XUiC_CategoryEntry xuiC_CategoryEntry = this.currentSubCategory;
						if (((xuiC_CategoryEntry != null) ? xuiC_CategoryEntry.CategoryList : null) != _sender)
						{
							return;
						}
					}
					XUiC_CategoryList xuiC_CategoryList2 = (XUiC_CategoryList)_sender;
					if (xuiC_CategoryList2 == null)
					{
						return;
					}
					xuiC_CategoryList2.HandleCategoryChanged();
				};
			}
		}
		this.subCategoryListShapes = (XUiC_CategoryList)base.GetChildById("subCategoriesShapes");
		this.currentCreativeMode = (GameManager.Instance.IsEditMode() ? EnumCreativeMode.Dev : EnumCreativeMode.Player);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Hideshapes_Changed(XUiController _sender, int _mouseButton)
	{
		this.hideShapes = !this.hideShapes;
		this.hideshapes.Selected = this.hideShapes;
		this.RefreshList = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleFavoritesChanged(XUiController _sender, int _mouseButton)
	{
		this.showFavorites = !this.showFavorites;
		this.favorites.Selected = this.showFavorites;
		this.RefreshList = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		this.currentFilterExcludeFuncs[0] = null;
		this.currentFilterExcludeFuncs[1] = null;
		this.currentMainCategory = _categoryEntry;
		this.RefreshList = true;
		this.IsDirty = true;
		if (_categoryEntry == null)
		{
			return;
		}
		this.currentFilterExcludeFuncs[0] = this.buildFilter(_categoryEntry);
		Block block = this.categoryIsBlockShapes(_categoryEntry);
		if (block != null)
		{
			this.setupSubcategoriesForShapes(block);
			return;
		}
		if (this.categoryIsOtherShapes(_categoryEntry))
		{
			this.setupSubcategoriesForShapes(this.otherShapeBlocks[0]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SubCategory_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		this.currentFilterExcludeFuncs[1] = null;
		this.currentSubCategory = _categoryEntry;
		this.IsDirty = true;
		this.RefreshList = true;
		if (this.currentMainCategory == null)
		{
			return;
		}
		this.currentFilterExcludeFuncs[1] = this.buildFilter(this.currentSubCategory);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtInput_OnChange(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.RefreshList = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DevBlockButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.showDevBlocks = !this.showDevBlocks;
		this.devBlockButton.Selected = this.showDevBlocks;
		this.RefreshList = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SimpleClickButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.simpleClick = !this.simpleClick;
		this.simpleClickButton.Selected = this.simpleClick;
		XUiC_ItemStack[] childrenByType = this.creativeGrid.GetChildrenByType<XUiC_ItemStack>(null);
		for (int i = 0; i < childrenByType.Length; i++)
		{
			childrenByType[i].SimpleClick = this.simpleClick;
		}
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
	public void updateHeader()
	{
		if (this.currentMainCategory == null)
		{
			this.filterCrumbs = Localization.Get("lblAll", false, null);
			return;
		}
		this.headerNameBuilder.Clear();
		this.headerNameBuilder.Append(this.currentMainCategory.CategoryDisplayName);
		if (this.currentSubCategory == null)
		{
			this.filterCrumbs = this.headerNameBuilder.ToString();
			return;
		}
		this.headerNameBuilder.Append(" | ");
		this.headerNameBuilder.Append(this.currentSubCategory.CategoryDisplayName);
		this.filterCrumbs = this.headerNameBuilder.ToString();
	}

	public void Refresh()
	{
		this.updateHeader();
		XUiC_TextInput xuiC_TextInput = this.txtInput;
		string text = ((xuiC_TextInput != null) ? xuiC_TextInput.Text : null);
		this.currentFilterExcludeFuncs[2] = null;
		if (this.hideShapes && this.currentMainCategory == null)
		{
			this.currentFilterExcludeFuncs[2] = (ItemClass _class, Block _block) => _block != null && _block.GetAutoShapeType() > EAutoShapeType.None;
		}
		this.filteredItems.Clear();
		ItemClass.GetItemsAndBlocks(this.filteredItems, -1, -1, this.currentFilterExcludeFuncs, text, this.showDevBlocks, this.currentCreativeMode, this.showFavorites, true, this.xui);
		this.length = this.creativeGrid.Length;
		this.Page = 0;
		this.IsDirty = true;
		ItemClass.CreateItemStacks(this.filteredItems, this.itemStacks, text);
		this.creativeGrid.SetSlots(this.itemStacks.ToArray());
		XUiC_Paging xuiC_Paging = this.pager;
		if (xuiC_Paging == null)
		{
			return;
		}
		xuiC_Paging.SetLastPageByElementsAndPageLength(this.itemStacks.Count, this.length);
	}

	public void RefreshView()
	{
		this.IsDirty = true;
		this.creativeGrid.IsDirty = true;
	}

	public override void OnOpen()
	{
		this.isOpening = true;
		base.OnOpen();
		this.isOpening = false;
		this.Refresh();
		if (!GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) && !GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled))
		{
			this.xui.playerUI.windowManager.CloseAllOpenModalWindows(null, false);
		}
		this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoCategoryLeft", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoCategoryRight", XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
		this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory, 0f);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.ToolTipWindow.ToolTip = "";
		this.xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings();
		}
		if (this.RefreshList)
		{
			this.RefreshList = false;
			this.Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "filter_crumb")
		{
			_value = this.filterCrumbs ?? "";
			return true;
		}
		if (_bindingName == "result_count")
		{
			_value = this.itemStacks.Count.ToString();
			return true;
		}
		if (_bindingName == "main_category_is_shapes")
		{
			_value = "autoshapes".EqualsCaseInsensitive(this.categoryFilterType(this.currentMainCategory)).ToString();
			return true;
		}
		if (_bindingName == "main_category_name")
		{
			XUiC_CategoryEntry xuiC_CategoryEntry = this.currentMainCategory;
			_value = ((xuiC_CategoryEntry != null) ? xuiC_CategoryEntry.CategoryName : null) ?? "";
			return true;
		}
		if (!(_bindingName == "allow_dev"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = this.allowDevBlocks.ToString();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass.FilterItem buildFilter(XUiC_CategoryEntry _categoryEntry)
	{
		string text = this.categoryFilterType(_categoryEntry);
		if (text == null)
		{
			if (_categoryEntry != null)
			{
				Log.Warning("[XUi] CreativeMenu: No filtertype on sub category button '" + _categoryEntry.CategoryName + "'");
			}
			return null;
		}
		string filtertext = this.categoryFilterText(_categoryEntry);
		if (text.EqualsCaseInsensitive("autoshapes"))
		{
			if (filtertext == null)
			{
				return (ItemClass _class, Block _block) => _block == null || _block.GetAutoShapeType() == EAutoShapeType.None;
			}
			if (filtertext.EqualsCaseInsensitive("$other"))
			{
				return delegate(ItemClass _class, Block _block)
				{
					if (_block == null || _block.GetAutoShapeType() == EAutoShapeType.None)
					{
						return true;
					}
					for (int i = 0; i < this.otherShapeBlocks.Count; i++)
					{
						Block block = this.otherShapeBlocks[i];
						if (_block == block || _block.GetAutoShapeHelperBlock() == block)
						{
							return false;
						}
					}
					return true;
				};
			}
			string text2 = filtertext + ":" + ShapesFromXml.VariantHelperName;
			Block shapesHelper = Block.GetBlockByName(text2, false);
			if (shapesHelper == null)
			{
				Log.Warning("[XUi] CreativeMenu: Filtering on autoshapes with baseblock '" + text2 + "' failed: Block not found!");
				return null;
			}
			return (ItemClass _class, Block _block) => _block == null || _block.GetAutoShapeType() == EAutoShapeType.None || (_block != shapesHelper && _block.GetAutoShapeHelperBlock() != shapesHelper);
		}
		else if (text.EqualsCaseInsensitive("shapecategory"))
		{
			if (filtertext == null)
			{
				return null;
			}
			ShapesFromXml.ShapeCategory shapeCategory;
			if (!ShapesFromXml.shapeCategories.TryGetValue(filtertext, out shapeCategory))
			{
				Log.Warning("[XUi] CreativeMenu: Filtering on autoshapes with category '" + filtertext + "' failed: Unknown shape category!");
				return null;
			}
			return (ItemClass _class, Block _block) => ((_block != null) ? _block.ShapeCategories : null) == null || !_block.ShapeCategories.Contains(shapeCategory);
		}
		else if (text.EqualsCaseInsensitive("itemgroups"))
		{
			if (string.IsNullOrEmpty(filtertext))
			{
				return (ItemClass _class, Block _block) => _block != null;
			}
			return delegate(ItemClass _class, Block _block)
			{
				if (_block != null)
				{
					return true;
				}
				for (int j = 0; j < _class.Groups.Length; j++)
				{
					if (_class.Groups[j] == filtertext)
					{
						return false;
					}
				}
				return true;
			};
		}
		else
		{
			if (!text.EqualsCaseInsensitive("blockfiltertags"))
			{
				Log.Warning(string.Concat(new string[] { "[XUi] CreativeMenu: Unknown filtertype '", text, "' on sub category button '", _categoryEntry.CategoryName, "'" }));
				return null;
			}
			if (string.IsNullOrEmpty(filtertext))
			{
				return (ItemClass _class, Block _block) => _block == null;
			}
			return delegate(ItemClass _class, Block _block)
			{
				if (((_block != null) ? _block.FilterTags : null) != null)
				{
					for (int k = 0; k < _block.FilterTags.Length; k++)
					{
						if (_block.FilterTags[k] == filtertext)
						{
							return false;
						}
					}
				}
				return true;
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupMainCategories(XUiC_CategoryList _categoryList)
	{
		if (_categoryList == null)
		{
			return;
		}
		List<Block> individualButtonShapes = new List<Block>();
		for (int i = 0; i < _categoryList.MaxCategories; i++)
		{
			XUiC_CategoryEntry categoryByIndex = _categoryList.GetCategoryByIndex(i);
			Block block = this.categoryIsBlockShapes(categoryByIndex);
			if (block != null)
			{
				categoryByIndex.CategoryName = block.GetBlockName();
				categoryByIndex.CategoryDisplayName = block.blockMaterial.GetLocalizedMaterialName();
				individualButtonShapes.Add(block);
			}
		}
		List<ItemClass> list = new List<ItemClass>();
		ItemClass.GetItemsAndBlocks(list, -1, Block.ItemsStartHere, new ItemClass.FilterItem[]
		{
			(ItemClass _class, Block _block) => _block == null || _block.GetAutoShapeType() != EAutoShapeType.Helper || individualButtonShapes.Contains(_block)
		}, null, true, EnumCreativeMode.Player, false, false, null);
		foreach (ItemClass itemClass in list)
		{
			this.otherShapeBlocks.Add(itemClass.GetBlock());
		}
		_categoryList.CategoryChanged += this.CategoryList_CategoryChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Block categoryIsBlockShapes(XUiC_CategoryEntry _categoryEntry)
	{
		if (!"autoshapes".EqualsCaseInsensitive(this.categoryFilterType(_categoryEntry)))
		{
			return null;
		}
		string text = this.categoryFilterText(_categoryEntry);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return Block.GetBlockByName(text + ":" + ShapesFromXml.VariantHelperName, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool categoryIsOtherShapes(XUiC_CategoryEntry _categoryEntry)
	{
		return "autoshapes".EqualsCaseInsensitive(this.categoryFilterType(_categoryEntry)) && "$other".EqualsCaseInsensitive(this.categoryFilterText(_categoryEntry));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryFilterType(XUiC_CategoryEntry _categoryEntry)
	{
		if (_categoryEntry == null)
		{
			return null;
		}
		object obj;
		_categoryEntry.CustomAttributes.TryGetValue("filtertype", out obj);
		return obj.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryFilterText(XUiC_CategoryEntry _categoryEntry)
	{
		if (_categoryEntry == null)
		{
			return null;
		}
		object obj;
		_categoryEntry.CustomAttributes.TryGetValue("filtertext", out obj);
		return obj.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupSubcategoriesForShapes(Block _shapesHelper)
	{
		if (this.subCategoryListShapes == null)
		{
			return;
		}
		this.subCategoryListShapes.CategoryChanged -= this.SubCategory_CategoryChanged;
		XUiC_CategoryEntry currentCategory = this.subCategoryListShapes.CurrentCategory;
		string text = ((currentCategory != null) ? currentCategory.CategoryName : null);
		IEnumerable<Block> altBlocks = _shapesHelper.GetAltBlocks();
		List<ShapesFromXml.ShapeCategory> list = new List<ShapesFromXml.ShapeCategory>();
		Block.GetShapeCategories(altBlocks, list);
		for (int i = 0; i < this.subCategoryListShapes.Children.Count; i++)
		{
			if (i < list.Count)
			{
				this.subCategoryListShapes.SetCategoryEntry(i, list[i].Name, list[i].Icon, list[i].LocalizedName);
				this.subCategoryListShapes.GetCategoryByIndex(i).CustomAttributes["filtertype"] = "shapecategory";
				this.subCategoryListShapes.GetCategoryByIndex(i).CustomAttributes["filtertext"] = list[i].Name;
			}
			else
			{
				this.subCategoryListShapes.SetCategoryEmpty(i);
			}
		}
		this.subCategoryListShapes.CategoryChanged += this.SubCategory_CategoryChanged;
		this.subCategoryListShapes.SetCategory(text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilterTypeAutoshapes = "autoshapes";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilterTextAutoshapesOther = "$other";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilterTypeItemgroups = "itemgroups";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilterTypeBlocktags = "blockfiltertags";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string FilterTypeShapecategory = "shapecategory";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button simpleClickButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button devBlockButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button favorites;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button hideshapes;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Creative2StackGrid creativeGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CategoryList subCategoryListShapes;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CategoryEntry currentMainCategory;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CategoryEntry currentSubCategory;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpening;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool simpleClick;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowDevBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showDevBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showFavorites;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hideShapes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool RefreshList;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder headerNameBuilder = new StringBuilder();

	[PublicizedFrom(EAccessModifier.Private)]
	public string filterCrumbs;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ItemClass.FilterItem[] currentFilterExcludeFuncs = new ItemClass.FilterItem[3];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ItemClass> filteredItems = new List<ItemClass>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ItemStack> itemStacks = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumCreativeMode currentCreativeMode = EnumCreativeMode.Player;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Block> otherShapeBlocks = new List<Block>();
}
