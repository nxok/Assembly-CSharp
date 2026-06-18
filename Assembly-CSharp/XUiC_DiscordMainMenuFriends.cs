using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordMainMenuFriends : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_DiscordMainMenuFriends.id = base.WindowGroup.Id;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		DiscordManager.Instance.PendingActionsUpdate += this.discordPendingActionsUpdate;
		DiscordManager.Instance.ActivityJoining += this.discordActivityJoining;
		this.discordPendingActionsUpdate(DiscordManager.Instance.GetPendingActionsCount());
	}

	public override void OnClose()
	{
		DiscordManager.Instance.PendingActionsUpdate -= this.discordPendingActionsUpdate;
		DiscordManager.Instance.ActivityJoining -= this.discordActivityJoining;
		base.OnClose();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!DiscordManager.Instance.IsReady)
		{
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordPendingActionsUpdate(int _pendingActionsCount)
	{
		this.hasPendingActions = _pendingActionsCount > 0;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordActivityJoining()
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "has_pending_actions")
		{
			_value = this.hasPendingActions.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public static void ToggleWindow(XUi _xui)
	{
		_xui.playerUI.windowManager.SwitchVisible(XUiC_DiscordMainMenuFriends.id, false);
	}

	public new static bool IsOpen(XUi _xui)
	{
		return _xui.playerUI.windowManager.IsWindowOpen(XUiC_DiscordMainMenuFriends.id);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string id = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasPendingActions;
}
