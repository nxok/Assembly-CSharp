using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignGalleryWindow : XUiController
{
	public SignCanvas targetCanvas
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public TEFeatureCanvas targetEntity
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
			if (this.page != value)
			{
				this.page = value;
				this.signGrid.Page = this.page;
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
		XUiC_SignGalleryWindow.ID = base.WindowGroup.Id;
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
		this.signGrid = base.Parent.GetChildByType<XUiC_SignStackGrid>();
		XUiC_SignStack[] childrenByType = this.signGrid.GetChildrenByType<XUiC_SignStack>(null);
		for (int j = 0; j < childrenByType.Length; j++)
		{
			childrenByType[j].OnScroll += this.HandleOnScroll;
			XUiC_SignStack xuiC_SignStack = childrenByType[j];
			xuiC_SignStack.OnBecameSelected = (Action<XUiC_SignStack>)Delegate.Combine(xuiC_SignStack.OnBecameSelected, new Action<XUiC_SignStack>(this.HandleOnSelect));
		}
		this.length = childrenByType.Length;
		this.txtInput = (XUiC_TextInput)this.windowGroup.Controller.GetChildById("searchInput");
		if (this.txtInput != null)
		{
			this.txtInput.OnChangeHandler += this.HandleOnChangedHandler;
			this.txtInput.Text = "";
		}
		this.btnNew = (XUiC_SimpleButton)base.GetChildById("btnNew");
		this.btnNew.OnPressed += this.BtnNew_OnPressed;
		this.infoWindow = this.windowGroup.Controller.GetChildByType<XUiC_SignInfoWindow>();
		this.instanceWindow = this.windowGroup.Controller.GetChildByType<XUiC_SignInstanceWindow>();
		this.lblTotal = Localization.Get("lblTotalItems", false, null);
		XUiController childById = base.GetChildById("filterDefault");
		this.filterDefaultButton = childById.ViewComponent as XUiV_Button;
		childById.OnPress += this.BtnFilterDefault_OnPressed;
		XUiController childById2 = base.GetChildById("filterPrefab");
		this.filterPrefabButton = childById2.ViewComponent as XUiV_Button;
		childById2.OnPress += this.BtnFilterPrefab_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnNew_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!PrefabEditModeManager.Instance.IsActive())
		{
			Log.Error("New signs are saved to prefabs and can only be created in the Prefab Editor.");
			return;
		}
		string fileNameNoExtension = PrefabEditModeManager.Instance.LoadedPrefab.FileNameNoExtension;
		SignData newSignData = SignData.GetNewSignData(Localization.Get("lblSignNewSign", false, null));
		GlobalSignId globalSignId = SignDataManager.Instance.AddSignToLibrary(fileNameNoExtension, newSignData);
		PrefabEditModeManager.Instance.NeedsSaving = true;
		this.RefreshAndSelect(globalSignId);
		if (this.targetEntity != null)
		{
			XUiC_SignEditorWindow.Open(this.xui.playerUI, this.targetEntity);
			return;
		}
		XUiC_SignEditorWindow.Open(this.xui.playerUI, newSignData, globalSignId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnSelect(XUiC_SignStack _sender)
	{
		if (this.currentSignId != _sender.SignId)
		{
			this.currentSignId = _sender.SignId;
			if (this.targetCanvas != null)
			{
				this.targetCanvas.SignId = this.currentSignId;
			}
			this.instanceWindow.RefreshPreviewRenderers();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
		this.infoWindow.SetSignInfo(_sender.SignId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.Page = 0;
		this.RefreshItems(this.txtInput.Text);
		this.UpdateGrid(false);
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
	public void RefreshItems(string _filter)
	{
		this.currentItems.Clear();
		this.length = this.signGrid.Length;
		this.Page = 0;
		if (this.includeDefaultLibrary)
		{
			SignDataManager.Instance.AddFilteredSignIdsToList("[D]", _filter, this.currentItems);
		}
		if (PrefabEditModeManager.Instance.IsActive() && this.includePrefabLibrary)
		{
			string fileNameNoExtension = PrefabEditModeManager.Instance.LoadedPrefab.FileNameNoExtension;
			if (fileNameNoExtension != null && SignDataManager.Instance.HasSignLibrary(fileNameNoExtension))
			{
				SignDataManager.Instance.AddFilteredSignIdsToList(PrefabEditModeManager.Instance.LoadedPrefab.FileNameNoExtension, _filter, this.currentItems);
			}
		}
		this.signNamesById.Clear();
		foreach (GlobalSignId globalSignId in this.currentItems)
		{
			this.signNamesById[globalSignId] = XUiC_SignGalleryWindow.<RefreshItems>g__GetNiceName|35_1(globalSignId);
		}
		this.currentItems.Sort(delegate(GlobalSignId a, GlobalSignId b)
		{
			int num = a.libraryId.CompareTo(b.libraryId);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(this.signNamesById[a], this.signNamesById[b]);
		});
		XUiC_Paging xuiC_Paging = this.pager;
		if (xuiC_Paging != null)
		{
			xuiC_Paging.SetLastPageByElementsAndPageLength(this.currentItems.Count, this.length);
		}
		this.resultCount.Text = string.Format(this.lblTotal, this.currentItems.Count.ToString());
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.RefreshItems(this.txtInput.Text);
		this.IsDirty = true;
		if (this.targetCanvas != null)
		{
			this.currentSignId = this.targetCanvas.SignId;
		}
		else
		{
			this.currentSignId = GlobalSignId.InvalidId;
		}
		this.AutoPage();
		this.UpdateGrid(true);
		this.instanceWindow.InitialiseTo(this.targetEntity);
		this.RefreshFilterButtons();
		this.windowGroup.Controller.GetChildByType<XUiC_WindowNonPagingHeader>().SetHeader(Localization.Get("xuiSigns", false, null).ToUpper());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AutoPage()
	{
		int num = this.currentItems.IndexOf(this.currentSignId);
		if (num >= 0)
		{
			this.Page = num / this.signGrid.Length;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.ToolTipWindow.ToolTip = "";
		if (this.xui.playerUI.windowManager.IsWindowOpen("windowpaging"))
		{
			this.xui.playerUI.windowManager.Close("windowpaging");
		}
		if (this.targetCanvas != null)
		{
			this.targetCanvas.SignId = this.currentSignId;
			this.targetCanvas = null;
		}
		this.targetEntity = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.viewComponent.IsVisible && this.IsDirty)
		{
			if (this.updateFilters)
			{
				this.RefreshFilterButtons();
				this.Page = 0;
				this.RefreshItems(this.txtInput.Text);
				this.UpdateGrid(false);
				this.updateFilters = false;
			}
			base.RefreshBindings();
			this.signGrid.IsDirty = true;
			this.IsDirty = false;
		}
	}

	public static void Open(LocalPlayerUI _playerUi, TEFeatureCanvas _feature)
	{
		XUiC_SignGalleryWindow childByType = _playerUi.xui.FindWindowGroupByName(XUiC_SignGalleryWindow.ID).GetChildByType<XUiC_SignGalleryWindow>();
		childByType.targetCanvas = ((_feature != null) ? _feature.Canvas : null);
		childByType.targetEntity = _feature;
		_playerUi.windowManager.Open(XUiC_SignGalleryWindow.ID, true);
	}

	public void RefreshAndSelect(GlobalSignId newSignId)
	{
		this.RefreshItems(this.txtInput.Text);
		this.IsDirty = true;
		this.currentSignId = newSignId;
		if (this.targetCanvas != null)
		{
			this.targetCanvas.SignId = this.currentSignId;
		}
		this.AutoPage();
		this.UpdateGrid(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateGrid(bool forceRefreshInfoPanel)
	{
		this.signGrid.SetSignIds(this.currentItems, this.currentSignId);
		if (forceRefreshInfoPanel && !this.currentItems.Contains(this.currentSignId))
		{
			this.infoWindow.SetSignInfo(this.currentSignId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshFilterButtons()
	{
		this.filterDefaultButton.Selected = this.includeDefaultLibrary;
		this.filterPrefabButton.Selected = this.includePrefabLibrary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnFilterDefault_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.includeDefaultLibrary = !this.includeDefaultLibrary;
		this.updateFilters = true;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnFilterPrefab_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.includePrefabLibrary = !this.includePrefabLibrary;
		this.updateFilters = true;
		this.IsDirty = true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static string <RefreshItems>g__GetNiceName|35_1(GlobalSignId signId)
	{
		SignData signData;
		if (SignDataManager.Instance.TryGetSignData(signId, out signData))
		{
			return signData.name;
		}
		Log.Error("Failed to retrieve sign data for global id: '" + signId.ToString() + "'.");
		return string.Empty;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label resultCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignStackGrid signGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateFilters;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool includeDefaultLibrary = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool includePrefabLibrary = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnNew;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotal;

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId currentSignId = GlobalSignId.InvalidId;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignInfoWindow infoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignInstanceWindow instanceWindow;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Button filterDefaultButton;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Button filterPrefabButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<GlobalSignId> currentItems = new List<GlobalSignId>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<GlobalSignId, string> signNamesById = new Dictionary<GlobalSignId, string>();
}
