using System;
using Discord.Sdk;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordLobbyControl : XUiController
{
	public override void Init()
	{
		base.Init();
		DiscordManager.Instance.LobbyStateChanged += this.onLobbyStateChanged;
		DiscordManager.Instance.CallStatusChanged += this.onCallStatusChanged;
		XUiC_SimpleButton xuiC_SimpleButton = base.GetChildById("btnJoinVoice") as XUiC_SimpleButton;
		if (xuiC_SimpleButton != null)
		{
			xuiC_SimpleButton.OnPressed += this.btnJoinPressed;
		}
		XUiC_SimpleButton xuiC_SimpleButton2 = base.GetChildById("btnLeaveVoice") as XUiC_SimpleButton;
		if (xuiC_SimpleButton2 != null)
		{
			xuiC_SimpleButton2.OnPressed += this.btnLeavePressed;
		}
		XUiController childById = base.GetChildById("btnJoinVoice");
		XUiV_Button xuiV_Button = ((childById != null) ? childById.ViewComponent : null) as XUiV_Button;
		if (xuiV_Button != null)
		{
			xuiV_Button.Controller.OnPress += this.btnJoinPressed;
		}
		XUiController childById2 = base.GetChildById("btnLeaveVoice");
		XUiV_Button xuiV_Button2 = ((childById2 != null) ? childById2.ViewComponent : null) as XUiV_Button;
		if (xuiV_Button2 != null)
		{
			xuiV_Button2.Controller.OnPress += this.btnLeavePressed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onLobbyStateChanged(DiscordManager.LobbyInfo _lobby, bool _isReady, bool _isJoined)
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onCallStatusChanged(DiscordManager.CallInfo _call, Call.Status _callStatus)
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnJoinPressed(XUiController _sender, int _mouseButton)
	{
		DiscordManager.Instance.JoinLobbyVoice(this.lobbyType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnLeavePressed(XUiController _sender, int _mouseButton)
	{
		DiscordManager.Instance.LeaveLobbyVoice(true);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "lobby_ready")
		{
			_value = this.lobby.IsReady.ToString();
			return true;
		}
		if (_bindingName == "lobby_joined")
		{
			_value = this.lobby.IsJoined.ToString();
			return true;
		}
		if (_bindingName == "voice_status")
		{
			_value = this.lobby.VoiceCall.Status.ToString();
			return true;
		}
		if (_bindingName == "in_other_voice")
		{
			DiscordManager.LobbyInfo activeVoiceLobby = DiscordManager.Instance.ActiveVoiceLobby;
			_value = (activeVoiceLobby != null && activeVoiceLobby != this.lobby).ToString();
			return true;
		}
		if (!(_bindingName == "any_lobby_unstable_state"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = DiscordManager.Instance.AnyLobbyInUnstableVoiceState.ToString();
		return true;
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "lobby_type")
		{
			this.lobbyType = EnumUtils.Parse<DiscordManager.ELobbyType>(_value, true);
			this.lobby = DiscordManager.Instance.GetLobby(this.lobbyType);
			return true;
		}
		return base.ParseAttribute(_name, _value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DiscordManager.ELobbyType lobbyType;

	[PublicizedFrom(EAccessModifier.Private)]
	public DiscordManager.LobbyInfo lobby;
}
