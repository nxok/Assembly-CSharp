using System;
using System.Collections;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_InGameMenuWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_InGameMenuWindow.ID = base.WindowGroup.Id;
		this.btnInvite = base.GetChildById("btnInvite").GetChildByType<XUiC_SimpleButton>();
		this.btnInvite.OnPressed += this.BtnInvite_OnPressed;
		this.btnOptions = base.GetChildById("btnOptions").GetChildByType<XUiC_SimpleButton>();
		this.btnOptions.OnPressed += this.BtnOptions_OnPressed;
		this.btnSandboxSettings = base.GetChildById("btnSandboxSettings").GetChildByType<XUiC_SimpleButton>();
		this.btnSandboxSettings.OnPressed += this.StnSandboxSettings_OnPressed;
		this.btnHelp = base.GetChildById("btnHelp").GetChildByType<XUiC_SimpleButton>();
		this.btnHelp.OnPressed += this.BtnHelp_OnPressed;
		this.btnSave = base.GetChildById("btnSave").GetChildByType<XUiC_SimpleButton>();
		this.btnSave.OnPressed += this.BtnSave_OnPressed;
		this.btnExit = base.GetChildById("btnExit").GetChildByType<XUiC_SimpleButton>();
		this.btnExit.OnPressed += this.BtnExit_OnPressed;
		this.btnExportPrefab = base.GetChildById("btnExportPrefab").GetChildByType<XUiC_SimpleButton>();
		this.btnExportPrefab.OnPressed += this.BtnExportPrefab_OnPressed;
		this.btnTpPoi = base.GetChildById("btnTpPoi").GetChildByType<XUiC_SimpleButton>();
		this.btnTpPoi.OnPressed += this.BtnTpPoi_OnPressed;
		this.btnOpenConsole = base.GetChildById("btnOpenConsole").GetChildByType<XUiC_SimpleButton>();
		this.btnOpenConsole.OnPressed += this.BtnOpenConsole_OnPressed;
		this.btnBugReport = base.GetChildById("btnBugReport").GetChildByType<XUiC_SimpleButton>();
		this.btnBugReport.OnPressed += this.BtnBugReport_OnPressed;
		XUiController xuiController = this.xui.FindWindowGroupByName(XUiC_InGameMenuWindow.ServerInfoWindowGroupName);
		if (xuiController != null)
		{
			this.serverInfoWindowGroup = xuiController.WindowGroup.Id;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnInvite_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		IMultiplayerInvitationDialog multiplayerInvitationDialog = PlatformManager.NativePlatform.MultiplayerInvitationDialog;
		if (multiplayerInvitationDialog == null)
		{
			return;
		}
		multiplayerInvitationDialog.ShowInviteDialog();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOptions_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.continueGamePause = true;
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		LocalPlayerUI.primaryUI.windowManager.Open(XUiC_OptionsMenuNew.ParentSelector.WindowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StnSandboxSettings_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.xui.playerUI.windowManager.Open("sandboxOptions", true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnHelp_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.xui.playerUI.windowManager.Open(XUiC_PrefabEditorHelp.ID, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (PrefabEditModeManager.Instance.IsActive())
		{
			XUiC_SaveDirtyPrefab.Show(this.xui, new Action<XUiC_SaveDirtyPrefab.ESelectedAction>(this.savePrefab), XUiC_SaveDirtyPrefab.EMode.ForceSave);
			return;
		}
		GameManager.Instance.SaveLocalPlayerData();
		GameManager.Instance.SaveWorld();
		GameManager.ShowTooltip(GameManager.Instance.World.GetLocalPlayers()[0], Localization.Get("xuiWorldEditorSaved", false, null), false, false, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void savePrefab(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		this.xui.playerUI.windowManager.Open(this.windowGroup, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnExit_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (PrefabEditModeManager.Instance.IsActive() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			XUiC_SaveDirtyPrefab.Show(this.xui, new Action<XUiC_SaveDirtyPrefab.ESelectedAction>(this.exitGame), XUiC_SaveDirtyPrefab.EMode.AskSaveIfDirty);
			return;
		}
		this.exitGame(XUiC_SaveDirtyPrefab.ESelectedAction.DontSave);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void exitGame(XUiC_SaveDirtyPrefab.ESelectedAction _action)
	{
		if (_action == XUiC_SaveDirtyPrefab.ESelectedAction.Cancel)
		{
			this.xui.playerUI.windowManager.Open(this.windowGroup, true);
			return;
		}
		GameManager.Instance.SetActiveBlockTool(null);
		if (PlatformApplicationManager.IsRestartRequired)
		{
			ThreadManager.StartCoroutine(this.DisconnectAfterDisplayingExitingGameMessage());
			return;
		}
		GameManager.Instance.Disconnect();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DisconnectAfterDisplayingExitingGameMessage()
	{
		yield return GameManager.Instance.ShowExitingGameUICoroutine();
		GameManager.Instance.Disconnect();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnExportPrefab_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		XUiC_ExportPrefab.Open(this.xui);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnTpPoi_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.xui.playerUI.windowManager.Open(XUiC_PoiTeleportMenu.ID, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOpenConsole_OnPressed(XUiController _sender, int _mouseButton)
	{
		GUIWindowConsole.Open();
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBugReport_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		XUiC_BugReportWindow.Open();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (PlatformManager.NativePlatform.MultiplayerInvitationDialog != null)
		{
			int num = SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() + (GameManager.IsDedicatedServer ? 0 : 1);
			int @int = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
			this.btnInvite.Enabled = PlatformManager.NativePlatform.MultiplayerInvitationDialog.CanShow && (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || num < @int);
			this.btnInvite.ViewComponent.IsVisible = !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || SingletonMonoBehaviour<ConnectionManager>.Instance.HasRunningServers;
		}
		else
		{
			this.btnInvite.ViewComponent.IsVisible = false;
		}
		this.btnSave.ViewComponent.IsVisible = GameManager.Instance.IsEditMode() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		this.btnHelp.ViewComponent.IsVisible = GameManager.Instance.IsEditMode();
		this.btnExportPrefab.ViewComponent.IsVisible = GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
		this.btnTpPoi.ViewComponent.IsVisible = GameManager.Instance.GetDynamicPrefabDecorator() != null && (GameManager.Instance.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled));
		this.continueGamePause = false;
		GameManager.Instance.Pause(true);
		XUiC_FocusedBlockHealth.SetData(this.xui.playerUI, null, 0f);
		this.xui.playerUI.windowManager.Close("toolbelt");
		XUi.InGameMenuOpen = true;
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
		{
			this.xui.playerUI.windowManager.Open(XUiC_EditorPanelSelector.ID, false);
		}
		if (this.serverInfoWindowGroup != null)
		{
			this.xui.playerUI.windowManager.Open(this.serverInfoWindowGroup, false);
		}
		if (this.btnInvite.ViewComponent.IsVisible)
		{
			this.btnInvite.SelectCursorElement(true, false);
		}
		else
		{
			this.btnOptions.SelectCursorElement(true, false);
		}
		base.RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.windowManager.Close(XUiC_EditorPanelSelector.ID);
		if (!this.continueGamePause)
		{
			GameManager.Instance.Pause(false);
			XUi.InGameMenuOpen = false;
		}
		if (this.serverInfoWindowGroup != null)
		{
			this.xui.playerUI.windowManager.Close(this.serverInfoWindowGroup);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (PrefabEditModeManager.Instance.IsActive())
		{
			bool flag = PrefabEditModeManager.Instance.VoxelPrefab != null;
			this.btnSave.Enabled = flag;
		}
		this.btnExportPrefab.Enabled = BlockToolSelection.Instance.SelectionActive;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "creativeenabled")
		{
			_value = AchievementUtils.IsCreativeModeActive().ToString();
			return true;
		}
		if (_bindingName == "bug_reporting")
		{
			_value = BacktraceUtils.BugReportFeature.ToString();
			return true;
		}
		if (!(_bindingName == "console_button"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = GamePrefs.GetBool(EnumGamePrefs.OptionsShowConsoleButton).ToString();
		return true;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnInvite;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOptions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSandboxSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnHelp;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnExit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnExportPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnTpPoi;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnOpenConsole;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBugReport;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool continueGamePause;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string ServerInfoWindowGroupName = "serverinfowindow";

	[PublicizedFrom(EAccessModifier.Private)]
	public string serverInfoWindowGroup;
}
