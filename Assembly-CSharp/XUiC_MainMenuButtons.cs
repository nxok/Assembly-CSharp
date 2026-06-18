using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MainMenuButtons : XUiController
{
	[XuiXmlBinding("online_mode")]
	public bool OnlineMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			IPlatform multiPlatform = PlatformManager.MultiPlatform;
			EUserStatus? euserStatus;
			if (multiPlatform == null)
			{
				euserStatus = null;
			}
			else
			{
				IUserClient user = multiPlatform.User;
				euserStatus = ((user != null) ? new EUserStatus?(user.UserStatus) : null);
			}
			EUserStatus? euserStatus2 = euserStatus;
			return euserStatus2.GetValueOrDefault() != EUserStatus.OfflineMode;
		}
	}

	public override void Init()
	{
		base.Init();
		if (base.TryGetChildByIdAndType<XUiC_Button>("btnPlayGame", out this.btnPlayGame))
		{
			this.btnPlayGame.OnPress += this.btnPlayGame_OnPressed;
		}
		if (base.TryGetChildByIdAndType<XUiC_Button>("btnNewGame", out this.btnNewGame))
		{
			this.btnNewGame.OnPress += this.btnNewGame_OnPressed;
		}
		if (base.TryGetChildByIdAndType<XUiC_Button>("btnContinueGame", out this.btnContinueGame))
		{
			this.btnContinueGame.OnPress += this.btnContinueGame_OnPressed;
		}
		XUiC_Button xuiC_Button;
		if (base.TryGetChildByIdAndType<XUiC_Button>("btnConnectToServer", out xuiC_Button))
		{
			xuiC_Button.OnPress += this.btnConnectToServer_OnPressed;
		}
		XUiC_Button xuiC_Button2;
		if (base.TryGetChildByIdAndType<XUiC_Button>("btnOptions", out xuiC_Button2))
		{
			xuiC_Button2.OnPress += this.btnOptions_OnPressed;
		}
		XUiC_Button xuiC_Button3;
		if (base.TryGetChildByIdAndType<XUiC_Button>("btnCredits", out xuiC_Button3))
		{
			xuiC_Button3.OnPress += this.btnCredits_OnPressed;
		}
		XUiC_Button xuiC_Button4;
		if (base.TryGetChildByIdAndType<XUiC_Button>("btnNews", out xuiC_Button4))
		{
			xuiC_Button4.OnPress += this.btnNews_OnPressed;
		}
		XUiC_Button xuiC_Button5;
		if (base.TryGetChildByIdAndType<XUiC_Button>("btnQuit", out xuiC_Button5))
		{
			xuiC_Button5.OnPress += this.btnQuit_OnPressed;
		}
		XUiC_Button xuiC_Button6;
		if (base.TryGetChildByIdAndType<XUiC_Button>("btnEditingTools", out xuiC_Button6))
		{
			xuiC_Button6.OnPress += this.btnEditingTools_OnPressed;
		}
		XUiC_Button xuiC_Button7;
		if (base.TryGetChildByIdAndType<XUiC_Button>("btnRWG", out xuiC_Button7))
		{
			xuiC_Button7.OnPress += this.btnRWG_OnPressed;
		}
		XUiC_Button xuiC_Button8;
		if (base.TryGetChildByIdAndType<XUiC_Button>("btnDlc", out xuiC_Button8))
		{
			xuiC_Button8.OnPress += this.btnDlc_OnPressed;
		}
		XUiC_NewsWindow[] childControllers = base.GetChildControllers<XUiC_NewsWindow>("", null);
		for (int i = 0; i < childControllers.Length; i++)
		{
			childControllers[i].NewsEntryClicked += this.NewsBoxOnNewsEntryClicked;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NewsBoxOnNewsEntryClicked(NewsManager.NewsEntry _newsEntry)
	{
		this.btnNews_OnPressed(this, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnPlayGame_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.HasAcceptedLatestEula())
		{
			this.xui.playerUI.windowManager.Open(XUiC_PlayGameMenu.ID, true);
			return;
		}
		XUiC_EulaWindow.Open(this.xui, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnNewGame_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.HasAcceptedLatestEula())
		{
			this.CheckProfile(XUiC_NewGame.ID);
			return;
		}
		XUiC_EulaWindow.Open(this.xui, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnContinueGame_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.HasAcceptedLatestEula())
		{
			this.CheckProfile(XUiC_ContinueGame.ID);
			return;
		}
		XUiC_EulaWindow.Open(this.xui, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnConnectToServer_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!GameManager.HasAcceptedLatestEula())
		{
			XUiC_EulaWindow.Open(this.xui, false);
			return;
		}
		XUiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(EUserPerms.Multiplayer, delegate(bool _)
		{
			if (PermissionsManager.IsMultiplayerAllowed())
			{
				this.CheckProfile(XUiC_ServerBrowser.ID);
			}
		}, -1f, false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnEditingTools_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (GameManager.HasAcceptedLatestEula())
		{
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
			this.xui.playerUI.windowManager.Open(XUiC_EditingTools.ID, true);
			return;
		}
		XUiC_EulaWindow.Open(this.xui, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnRWG_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!GameManager.HasAcceptedLatestEula())
		{
			XUiC_EulaWindow.Open(this.xui, false);
			return;
		}
		XUiC_WorldGenerationWindow.Open(this.xui, XUiC_MainMenu.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOptions_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.xui.playerUI.windowManager.Open(XUiC_OptionsMenuNew.ParentSelector.WindowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnCredits_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.xui.playerUI.windowManager.Open(XUiC_Credits.ID, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnNews_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		XUiC_NewsScreen.Open(this.xui);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDlc_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.xui.playerUI.windowManager.Open(XUiC_DlcWindowNew.ID, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnQuit_OnPressed(XUiController _sender, int _mouseButton)
	{
		Application.Quit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckProfile(string _windowToOpen)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		if (ProfileSDF.CurrentProfileName().Length == 0)
		{
			XUiC_PlayerProfile.Open(this.xui, delegate
			{
				this.xui.playerUI.windowManager.Open(_windowToOpen, true);
			});
			return;
		}
		this.xui.playerUI.windowManager.Open(_windowToOpen, true);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.RefreshBindings();
		if (this.btnNewGame != null || this.btnContinueGame != null)
		{
			this.DoLoadSaveGameAutomation();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoLoadSaveGameAutomation()
	{
		EPlatformLoadSaveGameState loadSaveGameState = PlatformApplicationManager.GetLoadSaveGameState();
		if (loadSaveGameState != EPlatformLoadSaveGameState.NewGameOpen)
		{
			if (loadSaveGameState != EPlatformLoadSaveGameState.ContinueGameOpen)
			{
				return;
			}
			if (!this.btnContinueGame.ViewComponent.Enabled)
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				return;
			}
			this.btnContinueGame_OnPressed(this.btnContinueGame, -1);
			if (!this.xui.playerUI.windowManager.IsWindowOpen(XUiC_ContinueGame.ID))
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				return;
			}
			PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			return;
		}
		else
		{
			if (!this.btnNewGame.ViewComponent.Enabled)
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				return;
			}
			this.btnNewGame_OnPressed(this.btnNewGame, -1);
			if (!this.xui.playerUI.windowManager.IsWindowOpen(XUiC_NewGame.ID))
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				return;
			}
			PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			return;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		XUiC_Button xuiC_Button = this.btnNewGame;
		if (xuiC_Button == null)
		{
			return;
		}
		xuiC_Button.SelectCursorElement(true, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Button btnPlayGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Button btnNewGame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Button btnContinueGame;
}
