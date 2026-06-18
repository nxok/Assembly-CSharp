using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MultiplayerWindows : XUiController
{
	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings();
		}
		this.UpdateInput();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "is_multiplayer")
		{
			_value = (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsSinglePlayer).ToString();
			return true;
		}
		if (_bindingName == "blocked_players_available")
		{
			_value = (BlockedPlayerList.Instance != null).ToString();
			return true;
		}
		if (!(_bindingName == "discord_ready"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = DiscordManager.Instance.IsReady.ToString();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInput()
	{
		if ((this.xui.playerUI.playerInput.GUIActions.Cancel.WasPressed || this.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed) && !this.xui.PopupMenuWindow.ViewComponent.IsVisible)
		{
			this.xui.playerUI.windowManager.CloseAllOpenModalWindows(null, true);
		}
	}
}
