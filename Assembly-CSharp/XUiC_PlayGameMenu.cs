using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayGameMenu : XUiController
{
	[XuiXmlBinding("has_profile")]
	public bool HasProfile
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !string.IsNullOrEmpty(ProfileSDF.CurrentProfileName());
		}
	}

	[XuiXmlBinding("has_saved_game")]
	public bool HasSavedGame
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

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

	[XuiBindEvent("WindowSelected", "parentSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void onWindowSelected(XUiC_WindowSelector _sender, string _windowId)
	{
		GUIWindowManager windowManager = this.xui.playerUI.windowManager;
		if (_windowId == "playerProfiles")
		{
			XUiC_PlayerProfile.Open(this.xui, null);
			return;
		}
		if (_windowId == "newGame")
		{
			windowManager.Open(XUiC_NewGame.ID, true);
			return;
		}
		if (_windowId == "continueGame")
		{
			windowManager.Open(XUiC_ContinueGame.ID, true);
			return;
		}
		if (!(_windowId == "serverBrowser"))
		{
			return;
		}
		if (windowManager.IsWindowOpen(XUiC_ServerBrowser.ID))
		{
			return;
		}
		XUiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(EUserPerms.Multiplayer, delegate(bool _)
		{
			if (PermissionsManager.IsMultiplayerAllowed())
			{
				windowManager.Open(XUiC_ServerBrowser.ID, true);
			}
		}, -1f, false, null);
	}

	public override void Init()
	{
		base.Init();
		XUiC_PlayGameMenu.ID = base.WindowGroup.Id;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.RefreshBindings();
	}

	public void SetWindow(string _windowName)
	{
		this.parentSelector.SetSelected(_windowName);
		this.explicitlyRequestedWindow = true;
	}

	public override void OnOpen()
	{
		this.HasSavedGame = GameIO.GetPlayerSaves(null, false) > 0;
		base.RefreshBindings();
		if (!this.openedBefore)
		{
			this.openedBefore = true;
			if (!this.HasProfile)
			{
				this.parentSelector.SetSelected("playerProfiles");
			}
			else if (!this.explicitlyRequestedWindow)
			{
				if (this.HasSavedGame)
				{
					this.parentSelector.SetSelected("continueGame");
				}
				else
				{
					this.parentSelector.SetSelected("newGame");
				}
			}
		}
		this.explicitlyRequestedWindow = false;
		base.OnOpen();
	}

	public static string ID;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openedBefore;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool explicitlyRequestedWindow;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WindowSelector parentSelector;
}
