using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Paging : XUiController
{
	public int CurrentPageNumber
	{
		get
		{
			return this.currentPageNumber;
		}
		set
		{
			value = Mathf.Clamp(value, 0, this.LastPageNumber);
			if (value == this.currentPageNumber)
			{
				return;
			}
			this.currentPageNumber = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("pagenumber")]
	public int CurrentPageNumberNatural
	{
		get
		{
			return this.CurrentPageNumber + 1;
		}
	}

	public int LastPageNumber
	{
		get
		{
			return this.lastPageNumber;
		}
		set
		{
			if (value == this.lastPageNumber)
			{
				return;
			}
			this.lastPageNumber = value;
			this.IsDirty = true;
			if (this.CurrentPageNumber > this.lastPageNumber)
			{
				this.CurrentPageNumber = this.lastPageNumber;
				XUiEvent_PageChangedEventHandler onPageChanged = this.OnPageChanged;
				if (onPageChanged == null)
				{
					return;
				}
				onPageChanged();
			}
		}
	}

	[XuiXmlBinding("maxpagenumber")]
	public int LastPageNumberNatural
	{
		get
		{
			return this.LastPageNumber + 1;
		}
	}

	[XuiXmlBinding("over_content_area")]
	public bool ControllerOverContentArea
	{
		get
		{
			return this.controllerOverContentArea;
		}
		set
		{
			if (this.controllerOverContentArea == value)
			{
				return;
			}
			this.controllerOverContentArea = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("show_max_page", false)]
	[XuiXmlBinding("showmaxpage")]
	public bool ShowMaxPage { get; set; }

	[XuiXmlAttribute("separator", false)]
	[XuiXmlBinding("separator")]
	public string Separator { get; set; } = "/";

	[XuiXmlAttribute("contents_parent", false)]
	public string ContentParentName { get; set; }

	[XuiXmlAttribute("hotkeys_enabled", false)]
	public bool HotkeysEnabled { get; set; } = true;

	public event XUiEvent_PageChangedEventHandler OnPageChanged;

	public override void Init()
	{
		base.Init();
		if (this.btnPageDown != null)
		{
			this.btnPageDown.OnPress += delegate(XUiController _, int _)
			{
				this.PageDown();
			};
		}
		if (this.btnPageUp != null)
		{
			this.btnPageUp.OnPress += delegate(XUiController _, int _)
			{
				this.PageUp();
			};
		}
		if (!string.IsNullOrEmpty(this.ContentParentName))
		{
			XUiController xuiController = this;
			while (!xuiController.TryGetChildController<XUiController>(this.ContentParentName, out this.contentsParent))
			{
				xuiController = xuiController.Parent;
				if (xuiController == null)
				{
					break;
				}
			}
			if (this.contentsParent == null)
			{
				Log.Warning("[XUi] Could not find contents parent '" + this.ContentParentName + "' for pager. Hierarchy: " + base.GetXuiHierarchy());
			}
		}
		else
		{
			Log.Warning("[XUi] No contents parent set for pager. Hierarchy: " + base.GetXuiHierarchy());
		}
		this.handlePageDownAction = new Func<bool>(this.PageDown);
		this.handlePageUpAction = new Func<bool>(this.PageUp);
		this.CurrentPageNumber = 0;
	}

	public bool PageUp()
	{
		if (this.CurrentPageNumber >= this.LastPageNumber)
		{
			return false;
		}
		int num = this.CurrentPageNumber;
		this.CurrentPageNumber = num + 1;
		XUiEvent_PageChangedEventHandler onPageChanged = this.OnPageChanged;
		if (onPageChanged != null)
		{
			onPageChanged();
		}
		return true;
	}

	public bool PageDown()
	{
		if (this.CurrentPageNumber <= 0)
		{
			return false;
		}
		int num = this.CurrentPageNumber;
		this.CurrentPageNumber = num - 1;
		XUiEvent_PageChangedEventHandler onPageChanged = this.OnPageChanged;
		if (onPageChanged != null)
		{
			onPageChanged();
		}
		return true;
	}

	public int GetPage()
	{
		return this.CurrentPageNumber;
	}

	public void SetPage(int _page)
	{
		this.CurrentPageNumber = _page;
	}

	public int GetLastPage()
	{
		return this.LastPageNumber;
	}

	public void SetLastPageByElementsAndPageLength(int _elementCount, int _pageLength)
	{
		this.LastPageNumber = Math.Max(0, Mathf.CeilToInt((float)_elementCount / (float)_pageLength) - 1);
	}

	public void Reset()
	{
		this.CurrentPageNumber = 0;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.ControllerOverContentArea = this.updateControllerState();
		if (this.ControllerOverContentArea)
		{
			XUi.HandlePaging(this.xui, this.handlePageUpAction, this.handlePageDownAction, false);
		}
		base.handleDirtyUpdateDefault();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateControllerState()
	{
		IPlatform nativePlatform = PlatformManager.NativePlatform;
		bool flag;
		if (nativePlatform == null)
		{
			flag = false;
		}
		else
		{
			PlayerInputManager input = nativePlatform.Input;
			PlayerInputManager.InputStyle? inputStyle = ((input != null) ? new PlayerInputManager.InputStyle?(input.CurrentInputStyle) : null);
			PlayerInputManager.InputStyle inputStyle2 = PlayerInputManager.InputStyle.Keyboard;
			flag = (inputStyle.GetValueOrDefault() == inputStyle2) & (inputStyle != null);
		}
		return !flag && this.contentsParent != null && this.HotkeysEnabled && !XUiC_Paging.IsOverPagingOverrideElement && this.xui.playerUI.CursorController.CurrentTarget != null && this.xui.playerUI.CursorController.CurrentTarget.Controller.IsSelfOrChildOf(this.contentsParent);
	}

	public static bool IsOverPagingOverrideElement;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentPageNumber;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool controllerOverContentArea;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> handlePageDownAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> handlePageUpAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastPageNumber;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController contentsParent;

	[XuiBindComponent("pageUp", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController btnPageUp;

	[XuiBindComponent("pageDown", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController btnPageDown;
}
