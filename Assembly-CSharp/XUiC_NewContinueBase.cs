using System;
using System.Collections;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_NewContinueBase : XUiC_PlayGameDialogBase
{
	[XuiXmlBinding("startable")]
	public bool ValidStartableState
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.validStartableState;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (value == this.validStartableState)
			{
				return;
			}
			this.validStartableState = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("isroamingoptional")]
	public bool IsRoamingOptional
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional;
		}
	}

	[XuiXmlBinding("showbar")]
	public bool DataManagementBarEnabled
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.dataManagementBarEnabled;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (value == this.dataManagementBarEnabled)
			{
				return;
			}
			this.dataManagementBarEnabled = value;
			this.IsDirty = true;
		}
	}

	public abstract bool PlayIntroMovie
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public abstract bool AllowChangingCreativeMode
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public override void Init()
	{
		base.Init();
		((XUiC_Button)base.GetChildById("btnDataManagement")).OnPress += this.BtnDataManagement_OnPressed;
		this.DataManagementBarEnabled = this.DataManagementBar != null && SaveInfoProvider.DataLimitEnabled;
		this.IsDirty = true;
	}

	public override void OnOpen()
	{
		this.windowGroup.openWindowOnEsc = XUiC_MainMenu.ID;
		this.Settings.ApplyCreativeModeChangeAllowed(this.AllowChangingCreativeMode);
		base.OnOpen();
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_MultiplayerPrivilegeNotification.Close();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings();
		}
	}

	[XuiBindEvent("SettingsChanged", "Settings")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSettingsChanged()
	{
		this.validateStartable();
	}

	[XuiBindEvent("OpenSandboxSettingsRequested", "Settings")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnOpenSandboxSettingsRequested()
	{
		XUiC_SandboxOptions.Open(this.xui, delegate
		{
			this.Settings.UpdateSandboxPresetGroups();
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDataManagement_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_DataManagement.OpenDataManagementWindow(this, new Action(this.OnDataManagementWindowClosed));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void OnDataManagementWindowClosed();

	[PublicizedFrom(EAccessModifier.Protected)]
	public void updateBarUsageAndAllowanceValues()
	{
		if (!this.DataManagementBarEnabled)
		{
			return;
		}
		SaveInfoProvider instance = SaveInfoProvider.Instance;
		this.DataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
		this.DataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
	}

	[XuiBindEvent("OnPress", "BtnStart")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void BtnStart_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.BtnStart.ViewComponent.Enabled)
		{
			return;
		}
		GameManager.Instance.showOpenerMovieOnLoad = this.PlayIntroMovie;
		if (GamePrefs.GetBool(EnumGamePrefs.ServerEnabled))
		{
			EUserPerms perms = EUserPerms.HostMultiplayer;
			if (GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay))
			{
				perms |= EUserPerms.Crossplay;
			}
			XUiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(perms, delegate(bool _result)
			{
				bool flag = PermissionsManager.CanHostMultiplayer();
				GamePrefs.Set(EnumGamePrefs.ServerEnabled, flag);
				this.Settings.UpdateServerEnabledState();
				bool flag2 = perms.HasCrossplay() && PermissionsManager.IsCrossplayAllowed();
				GamePrefs.Set(EnumGamePrefs.ServerAllowCrossplay, flag2);
				this.Settings.UpdateCrossplayEnabledState();
				if (!_result)
				{
					return;
				}
				ThreadManager.StartCoroutine(this.startGameCo());
			}, -1f, false, null);
			return;
		}
		ThreadManager.StartCoroutine(this.startGameCo());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract IEnumerator startGameCo();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void validateStartable();

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_NewContinueBase()
	{
	}

	[XuiBindComponent("btnStart", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_Button BtnStart;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_NewContinueGameSettings Settings;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validStartableState;

	[XuiBindComponent("data_bar_controller", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_DataManagementBar DataManagementBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dataManagementBarEnabled;
}
