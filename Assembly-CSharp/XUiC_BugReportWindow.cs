using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Backtrace.Unity.Model;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BugReportWindow : XUiController
{
	[XuiXmlBinding("intimeout")]
	public bool InTimeout
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return XUiC_BugReportWindow.lastSubmissionTime >= 0f && Time.time - XUiC_BugReportWindow.lastSubmissionTime < 600f;
		}
	}

	[XuiXmlBinding("description")]
	public string Description
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_TextInput xuiC_TextInput = this.txtDescription;
			return ((xuiC_TextInput != null) ? xuiC_TextInput.Text.Trim() : null) ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.txtDescription.Text = value ?? "";
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("attach_saves_enabled")]
	public bool AttachSavesEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return BacktraceUtils.BugReportAttachSaveFeature;
		}
	}

	[XuiXmlBinding("game_running")]
	public bool GameRunningBinding
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return XUiC_BugReportWindow.GameRunning;
		}
	}

	public static bool GameRunning
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance.gameStateManager.IsGameStarted();
		}
	}

	public override void Init()
	{
		base.Init();
		this.windowGroup.isEscClosable = false;
		this.saveSelectWindow.GetChildByType<XUiC_BugReportSavesList>().SelectionChanged += this.List_SelectionChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void List_SelectionChanged(XUiC_List<XUiC_BugReportSavesList.ListEntry> _list, XUiC_BugReportSavesList.ListEntry _previousEntry, XUiC_BugReportSavesList.ListEntry _newEntry)
	{
		this.selectedSaveInfo = ((_newEntry != null) ? _newEntry.SaveEntryInfo : null);
		this.IsDirty = true;
	}

	[XuiBindEvent("OnValueChanged", "comboAttachSave")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboAttachSave_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
	{
		if (!XUiC_BugReportWindow.GameRunning)
		{
			this.saveSelectWindow.ViewComponent.IsVisible = _newValue;
			if (!_newValue)
			{
				this.selectedSaveInfo = null;
				XUiC_BugReportSaveSelect xuiC_BugReportSaveSelect = this.saveSelectWindow;
				if (xuiC_BugReportSaveSelect != null)
				{
					xuiC_BugReportSaveSelect.List.ClearSelection();
				}
			}
		}
		this.IsDirty = true;
	}

	public static void Open()
	{
		LocalPlayerUI primaryUI = LocalPlayerUI.primaryUI;
		XUiC_BugReportWindow childByType = primaryUI.xui.GetChildByType<XUiC_BugReportWindow>();
		primaryUI.windowManager.Open(childByType.windowGroup, XUiC_BugReportWindow.GameRunning);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.Description = "";
		this.selectedSaveInfo = null;
		this.uploading = false;
		this.comboAttachSave.Value = false;
		this.comboAttachScreenshot.Value = false;
		if (XUiC_BugReportWindow.GameRunning)
		{
			GameManager.Instance.Pause(true);
		}
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.uploading = false;
		if (XUiC_BugReportWindow.GameRunning)
		{
			GameManager.Instance.Pause(false);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!XUiC_BugReportWindow.GameRunning && this.saveSelectWindow != null && this.saveSelectWindow.ViewComponent.IsVisible != this.comboAttachSave.Value)
		{
			this.saveSelectWindow.ViewComponent.IsVisible = this.comboAttachSave.Value;
		}
		else if (XUiC_BugReportWindow.GameRunning && this.saveSelectWindow != null && this.saveSelectWindow.ViewComponent.IsVisible)
		{
			this.saveSelectWindow.ViewComponent.IsVisible = false;
		}
		if (!this.uploading && (this.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased || this.xui.playerUI.playerInput.GUIActions.Cancel.WasReleased))
		{
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		}
		base.handleDirtyUpdateDefault();
	}

	[XuiBindEvent("OnChangeHandler", "txtDescription")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtDescriptionOnChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnSubmit")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSubmitOnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.btnSubmit.ViewComponent.Enabled)
		{
			return;
		}
		XUiC_BugReportWindow.lastSubmissionTime = Time.time;
		ThreadManager.StartCoroutine(this.submitRoutine());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator submitRoutine()
	{
		PathAbstractions.AbstractedLocation abstractedLocation = PathAbstractions.AbstractedLocation.None;
		string saveName;
		string worldName;
		string saveDir;
		if (XUiC_BugReportWindow.GameRunning)
		{
			saveName = GamePrefs.GetString(EnumGamePrefs.GameName);
			worldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
			if (GameManager.Instance.World.IsRemote())
			{
				saveDir = GameIO.GetSaveGameLocalDir();
			}
			else
			{
				saveDir = GameIO.GetSaveGameDir();
			}
			abstractedLocation = PathAbstractions.Contextual.FindActiveWorldLocation();
		}
		else
		{
			saveName = this.selectedSaveInfo.Name;
			saveDir = this.selectedSaveInfo.SaveDir;
			worldName = this.selectedSaveInfo.WorldEntry.Name;
			if (this.selectedSaveInfo.WorldEntry.Location.Type == PathAbstractions.EAbstractedLocationType.None && !this.selectedSaveInfo.WorldEntry.Type.Equals(SaveInfoProvider.DeletedWorldsType))
			{
				XUiC_BugReportWindow.<>c__DisplayClass29_0 CS$<>8__locals1 = new XUiC_BugReportWindow.<>c__DisplayClass29_0();
				CS$<>8__locals1.userWorldSelection = null;
				XUiC_WorldSelectionPopup.Open(this.xui, "xuiWorldConflictBugReport", worldName, delegate
				{
					CS$<>8__locals1.userWorldSelection = new PathAbstractions.AbstractedLocation?(PathAbstractions.AbstractedLocation.None);
				}, delegate(PathAbstractions.AbstractedLocation _location)
				{
					CS$<>8__locals1.userWorldSelection = new PathAbstractions.AbstractedLocation?(_location);
				});
				while (CS$<>8__locals1.userWorldSelection == null)
				{
					yield return null;
				}
				if (CS$<>8__locals1.userWorldSelection.Value.Type == PathAbstractions.EAbstractedLocationType.None)
				{
					yield break;
				}
				abstractedLocation = CS$<>8__locals1.userWorldSelection.Value;
				CS$<>8__locals1 = null;
			}
			else
			{
				abstractedLocation = this.selectedSaveInfo.WorldEntry.Location;
			}
		}
		PathAbstractions.EAbstractedLocationType type = abstractedLocation.Type;
		if (type - PathAbstractions.EAbstractedLocationType.LocalSave > 1)
		{
			abstractedLocation = PathAbstractions.AbstractedLocation.None;
		}
		string worldDir = ((abstractedLocation.Type != PathAbstractions.EAbstractedLocationType.None) ? abstractedLocation.FullPath : null);
		this.uploading = true;
		string screenshotPath = null;
		if (this.comboAttachScreenshot.Value)
		{
			base.ViewComponent.IsVisible = false;
			GameManager.Instance.Pause(false);
			int num;
			for (int i = 0; i < 10; i = num + 1)
			{
				yield return null;
				num = i;
			}
			yield return ThreadManager.CoroutineWrapperWithExceptionCallback(GameUtils.TakeScreenshotEnum(GameUtils.EScreenshotMode.File, PlatformApplicationManager.Application.temporaryCachePath + "/" + Application.productName, 0f, false, 0, 0, false), new Action<Exception>(Log.Exception));
			GameManager.Instance.Pause(true);
			yield return null;
			base.ViewComponent.IsVisible = true;
			screenshotPath = GameUtils.lastSavedScreenshotFilename;
		}
		yield return null;
		XUiC_ProgressWindow.Open(this.xui.playerUI, Localization.Get("xuiBugReportUploading", false, null), null, false, true, true);
		yield return new WaitForSecondsRealtime(0.5f);
		BacktraceUtils.SendBugReport(this.Description, worldName, saveName, worldDir, saveDir, this.comboAttachSave.Value, screenshotPath, new Action<BacktraceResult>(this.<submitRoutine>g__BugReportCallBack|29_0));
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handlePostSubmissionClose()
	{
		XUiC_ProgressWindow.Close(this.xui.playerUI);
		this.xui.playerUI.windowManager.Close(base.WindowGroup, false);
		XUiC_MessageBoxWindowGroup.ShowOk(this.xui, Localization.Get("xuiBugReportHeader", false, null), Localization.Get("xuiBugReportSubmitted", false, null), null, false, XUiC_BugReportWindow.GameRunning, false);
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelOnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(base.WindowGroup, false);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <submitRoutine>g__BugReportCallBack|29_0(BacktraceResult _result)
	{
		Log.Out("Bug Report Send callback: {0}", new object[] { (_result == null) ? "null" : _result.message });
		this.handlePostSubmissionClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float lastSubmissionTime = -1f;

	[XuiBindComponent("txtDescription", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtDescription;

	[XuiBindComponent("btnSubmit", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSubmit;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[XuiBindComponent("comboAttachScreenshot", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxBool comboAttachScreenshot;

	[XuiBindComponent("comboAttachSave", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxBool comboAttachSave;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_BugReportSaveSelect saveSelectWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveInfoProvider.SaveEntryInfo selectedSaveInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool uploading;
}
