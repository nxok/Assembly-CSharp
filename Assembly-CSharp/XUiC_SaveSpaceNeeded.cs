using System;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SaveSpaceNeeded : XUiController
{
	[XuiXmlBinding("canCancel")]
	public bool CanCancel
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = true;

	[XuiXmlBinding("canDiscard")]
	public bool CanDiscard
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = true;

	[XuiXmlBinding("langKeyTitle")]
	public string LangKeyTitle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "xuiSave";

	[XuiXmlBinding("langKeyBody")]
	public string LangKeyBody
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "xuiDmSavingBody";

	[XuiXmlBinding("langKeyCancel")]
	public string LangKeyCancel
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "xuiCancel";

	[XuiXmlBinding("langKeyDiscard")]
	public string LangKeyDiscard
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "xuiDiscard";

	[XuiXmlBinding("langKeyConfirm")]
	public string LangKeyConfirm
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "xuiConfirm";

	[XuiXmlBinding("langKeyManage")]
	public string LangKeyManage
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "xuiDmManageSaves";

	[XuiXmlBinding("shouldShowDataBar")]
	public bool ShouldShowDataBar
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SaveInfoProvider.DataLimitEnabled && this.mStorageType.UsesDataLimit();
		}
	}

	[XuiXmlBinding("hasSufficientSpace")]
	public bool HasSufficientSpace
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !SaveInfoProvider.DataLimitEnabled || !this.mStorageType.UsesDataLimit() || this.mPendingBytes <= this.mTotalAvailableBytes;
		}
	}

	public XUiC_SaveSpaceNeeded.ConfirmationResult Result
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("prompt_sound", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attribBrowseSound(string _value)
	{
		this.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
		{
			this.mPromptReadyClip = _o;
		});
	}

	public override void Init()
	{
		base.Init();
		XUiC_SaveSpaceNeeded.ID = base.WindowGroup.Id;
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.CanCancel)
		{
			Log.Error("[SaveSpaceNeeded] Cancel button was pressed even though cancel is hidden?");
			return;
		}
		this.Result = XUiC_SaveSpaceNeeded.ConfirmationResult.Cancelled;
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[XuiBindEvent("OnPress", "btnDiscard")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDiscard_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.CanDiscard)
		{
			Log.Error("[SaveSpaceNeeded] Discard button was pressed even though discard is hidden?");
			return;
		}
		this.Result = XUiC_SaveSpaceNeeded.ConfirmationResult.Discarded;
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[XuiBindEvent("OnPress", "btnManage")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnManage_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiWindowGroup xuiWindowGroup = (XUiWindowGroup)this.xui.playerUI.windowManager.GetWindow(XUiC_LoadingScreen.ID);
		object obj;
		if (xuiWindowGroup == null)
		{
			obj = null;
		}
		else
		{
			XUiController controller = xuiWindowGroup.Controller;
			obj = ((controller != null) ? controller.GetChildByType<XUiC_LoadingScreen>() : null);
		}
		object obj2 = obj;
		if (obj2 != null)
		{
			obj2.SetTipsVisible(false);
		}
		XUiC_DataManagement.OpenDataManagementWindow(this, new Action(this.OnDataManagementWindowClosed));
	}

	[XuiBindEvent("OnPress", "btnConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirm_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.HasSufficientSpace)
		{
			Log.Error("[SaveSpaceNeeded] Confirm button was pressed even though there isn't enough free space?");
			return;
		}
		this.Result = XUiC_SaveSpaceNeeded.ConfirmationResult.Confirmed;
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDataManagementWindowClosed()
	{
		XUiWindowGroup xuiWindowGroup = (XUiWindowGroup)this.xui.playerUI.windowManager.GetWindow(XUiC_LoadingScreen.ID);
		object obj;
		if (xuiWindowGroup == null)
		{
			obj = null;
		}
		else
		{
			XUiController controller = xuiWindowGroup.Controller;
			obj = ((controller != null) ? controller.GetChildByType<XUiC_LoadingScreen>() : null);
		}
		object obj2 = obj;
		if (obj2 != null)
		{
			obj2.SetTipsVisible(true);
		}
		this.updateBarValues();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!XUiC_SaveSpaceNeeded.mOpenedProperly)
		{
			Log.Error("[SaveSpaceNeeded] XUiC_SaveSpaceNeeded should be opened with the static Open method so that initInternal is executed.");
		}
		XUiC_SaveSpaceNeeded.mOpenedProperly = false;
		if (this.mProtectedPaths != null)
		{
			foreach (string text in this.mProtectedPaths)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					SaveInfoProvider.Instance.SetDirectoryProtected(text, true);
				}
			}
		}
		if (this.ShouldShowDataBar)
		{
			this.updateBarValues();
			if (this.mPendingBytes != 0L)
			{
				Log.Out("[SaveSpaceNeeded] Pending Bytes: " + this.mPendingBytes.FormatSize(true) + ", Total Available Bytes: " + this.mTotalAvailableBytes.FormatSize(true));
			}
		}
		Manager.PlayXUiSound(this.mPromptReadyClip, 1f);
		this.mWasCursorHidden = this.xui.playerUI.CursorController.GetCursorHidden();
		this.xui.playerUI.CursorController.SetCursorHidden(false);
		this.mWasCursorLocked = this.xui.playerUI.CursorController.Locked;
		this.xui.playerUI.CursorController.Locked = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.CursorController.SetCursorHidden(this.mWasCursorHidden);
		this.xui.playerUI.CursorController.Locked = this.mWasCursorLocked;
		if (this.mProtectedPaths != null)
		{
			foreach (string text in this.mProtectedPaths)
			{
				if (!string.IsNullOrWhiteSpace(text))
				{
					SaveInfoProvider.Instance.SetDirectoryProtected(text, false);
				}
			}
		}
		this.mPendingBytes = 0L;
		this.mProtectedPaths = null;
		this.CanCancel = true;
		this.CanDiscard = true;
		this.LangKeyTitle = "xuiSave";
		this.LangKeyBody = "xuiDmSavingBody";
		this.LangKeyCancel = "xuiCancel";
		this.LangKeyDiscard = "xuiDiscard";
		this.LangKeyConfirm = "xuiConfirm";
		this.LangKeyManage = "xuiDmManageSaves";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initInternal(long _pendingBytes, string[] _protectedPaths, UserDataStorageType _storageType, bool _canCancel, bool _canDiscard, string _langKeyTitle, string _langKeyBody, string _langKeyCancel, string _langKeyDiscard, string _langKeyConfirm, string _langKeyManage)
	{
		this.mPendingBytes = _pendingBytes;
		this.mProtectedPaths = _protectedPaths;
		this.mStorageType = _storageType;
		this.CanCancel = _canCancel;
		this.CanDiscard = _canDiscard;
		this.LangKeyTitle = (string.IsNullOrWhiteSpace(_langKeyTitle) ? "xuiSave" : _langKeyTitle);
		this.LangKeyBody = (string.IsNullOrWhiteSpace(_langKeyBody) ? "xuiDmSavingBody" : _langKeyBody);
		this.LangKeyCancel = (string.IsNullOrWhiteSpace(_langKeyCancel) ? "xuiCancel" : _langKeyCancel);
		this.LangKeyDiscard = (string.IsNullOrWhiteSpace(_langKeyDiscard) ? "xuiDiscard" : _langKeyDiscard);
		this.LangKeyConfirm = (string.IsNullOrWhiteSpace(_langKeyConfirm) ? "xuiConfirm" : _langKeyConfirm);
		this.LangKeyManage = (string.IsNullOrWhiteSpace(_langKeyManage) ? "xuiDmManageSaves" : _langKeyManage);
		this.mTotalAvailableBytes = SaveInfoProvider.Instance.TotalAvailableBytes;
		this.Result = XUiC_SaveSpaceNeeded.ConfirmationResult.Pending;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBarValues()
	{
		SaveInfoProvider instance = SaveInfoProvider.Instance;
		this.dataManagementBar.ViewComponent.IsVisible = SaveInfoProvider.DataLimitEnabled;
		this.dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
		this.dataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
		this.dataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
		this.dataManagementBar.SetPendingBytes(this.mPendingBytes);
		this.mTotalAvailableBytes = instance.TotalAvailableBytes;
		base.RefreshBindings();
		if (this.HasSufficientSpace)
		{
			this.btnConfirm.SelectCursorElement(true, false);
			return;
		}
		this.btnManage.SelectCursorElement(true, false);
	}

	public static XUiC_SaveSpaceNeeded Open(long _pendingBytes, string _protectedPath, UserDataStorageType _storageType, bool _onlyShowOnInsufficientSpace = false, bool _canCancel = true, bool _canDiscard = true, string _title = null, string _body = null, string _cancel = null, string _discard = null, string _confirm = null, string _manage = null)
	{
		return XUiC_SaveSpaceNeeded.Open(_pendingBytes, new string[] { _protectedPath }, _storageType, _onlyShowOnInsufficientSpace, _canCancel, _canDiscard, _title, _body, _cancel, _discard, _confirm, _manage);
	}

	public static XUiC_SaveSpaceNeeded Open(long _pendingBytes, string[] _protectedPaths, UserDataStorageType _storageType, bool _onlyShowOnInsufficientSpace = false, bool _canCancel = true, bool _canDiscard = true, string _title = null, string _body = null, string _cancel = null, string _discard = null, string _confirm = null, string _manage = null)
	{
		if (_onlyShowOnInsufficientSpace)
		{
			if (!SaveInfoProvider.DataLimitEnabled || !_storageType.UsesDataLimit())
			{
				return null;
			}
			if (_pendingBytes <= SaveInfoProvider.Instance.TotalAvailableBytes)
			{
				return null;
			}
		}
		GUIWindowManager windowManager = LocalPlayerUI.primaryUI.xui.playerUI.windowManager;
		XUiWindowGroup xuiWindowGroup = (XUiWindowGroup)windowManager.GetWindow(XUiC_SaveSpaceNeeded.ID);
		XUiC_SaveSpaceNeeded xuiC_SaveSpaceNeeded;
		if (xuiWindowGroup == null)
		{
			xuiC_SaveSpaceNeeded = null;
		}
		else
		{
			XUiController controller = xuiWindowGroup.Controller;
			xuiC_SaveSpaceNeeded = ((controller != null) ? controller.GetChildByType<XUiC_SaveSpaceNeeded>() : null);
		}
		XUiC_SaveSpaceNeeded xuiC_SaveSpaceNeeded2 = xuiC_SaveSpaceNeeded;
		if (xuiC_SaveSpaceNeeded2 == null)
		{
			Log.Error("[SaveSpaceNeeded] Failed to retrieve reference to XUiC_SaveSpaceNeeded instance.");
			return null;
		}
		xuiC_SaveSpaceNeeded2.initInternal(_pendingBytes, _protectedPaths, _storageType, _canCancel, _canDiscard, _title, _body, _cancel, _discard, _confirm, _manage);
		XUiC_SaveSpaceNeeded.mOpenedProperly = true;
		windowManager.Open(XUiC_SaveSpaceNeeded.ID, false);
		XUiC_SaveSpaceNeeded.mOpenedProperly = false;
		return xuiC_SaveSpaceNeeded2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultTitle = "xuiSave";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultBody = "xuiDmSavingBody";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultCancel = "xuiCancel";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultDiscard = "xuiDiscard";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultConfirm = "xuiConfirm";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string LangKeyDefaultManage = "xuiDmManageSaves";

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool mOpenedProperly;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[XuiBindComponent("btnDiscard", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscard;

	[XuiBindComponent("btnManage", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnManage;

	[XuiBindComponent("btnConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConfirm;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DataManagementBar dataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public long mPendingBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] mProtectedPaths;

	[PublicizedFrom(EAccessModifier.Private)]
	public UserDataStorageType mStorageType;

	[PublicizedFrom(EAccessModifier.Private)]
	public long mTotalAvailableBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool mWasCursorHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool mWasCursorLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip mPromptReadyClip;

	public enum ConfirmationResult
	{
		Pending,
		Cancelled,
		Discarded,
		Confirmed
	}
}
