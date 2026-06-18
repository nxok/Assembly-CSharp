using System;
using InControl;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordMainMenuButton : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("button");
		XUiV_Button xuiV_Button = ((childById != null) ? childById.ViewComponent : null) as XUiV_Button;
		if (xuiV_Button != null)
		{
			xuiV_Button.Controller.OnPress += delegate(XUiController _, int _)
			{
				this.toggleWindow();
			};
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		DiscordManager.Instance.StatusChanged += this.discordStatusChanged;
		DiscordManager.Instance.PendingActionsUpdate += this.discordPendingActionsUpdate;
		this.discordPendingActionsUpdate(DiscordManager.Instance.GetPendingActionsCount());
	}

	public override void OnClose()
	{
		this.clearEvents();
		base.OnClose();
	}

	public override void Cleanup()
	{
		this.clearEvents();
		base.Cleanup();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		XUi xui = this.xui;
		bool? flag;
		if (xui == null)
		{
			flag = null;
		}
		else
		{
			LocalPlayerUI playerUI = xui.playerUI;
			if (playerUI == null)
			{
				flag = null;
			}
			else
			{
				PlayerActionsLocal playerInput = playerUI.playerInput;
				if (playerInput == null)
				{
					flag = null;
				}
				else
				{
					PlayerActionsGUI guiactions = playerInput.GUIActions;
					if (guiactions == null)
					{
						flag = null;
					}
					else
					{
						PlayerAction inspect = guiactions.Inspect;
						flag = ((inspect != null) ? new bool?(inspect.WasReleased) : null);
					}
				}
			}
		}
		bool? flag2 = flag;
		if (flag2.GetValueOrDefault() && XUiUtils.HotkeysAllowedFor(this.viewComponent, true))
		{
			this.toggleWindow();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleWindow()
	{
		if (!DiscordManager.Instance.IsReady)
		{
			return;
		}
		XUiC_DiscordMainMenuFriends.ToggleWindow(this.xui);
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearEvents()
	{
		DiscordManager.Instance.StatusChanged -= this.discordStatusChanged;
		DiscordManager.Instance.PendingActionsUpdate -= this.discordPendingActionsUpdate;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordStatusChanged(DiscordManager.EDiscordStatus _status)
	{
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordPendingActionsUpdate(int _pendingActionsCount)
	{
		this.pendingActions = _pendingActionsCount;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "discord_ready")
		{
			_value = DiscordManager.Instance.IsReady.ToString();
			return true;
		}
		if (_bindingName == "discord_open")
		{
			_value = XUiC_DiscordMainMenuFriends.IsOpen(this.xui).ToString();
			return true;
		}
		if (_bindingName == "pending_actions")
		{
			_value = this.pendingActions.ToString();
			return true;
		}
		if (!(_bindingName == "discord_name"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		DiscordManager.DiscordUser localUser = DiscordManager.Instance.LocalUser;
		_value = ((localUser != null) ? localUser.DiscordDisplayName : null) ?? "";
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int pendingActions;
}
