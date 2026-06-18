using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartyWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "partyvisible")
		{
			_value = ((this.xui.playerUI.entityPlayer != null) ? (this.xui.playerUI.entityPlayer.Party != null && !this.playerDead).ToString() : "false");
			return true;
		}
		if (_bindingName == "isleader")
		{
			_value = ((this.xui.playerUI.entityPlayer != null) ? (this.xui.playerUI.entityPlayer.Party != null && this.xui.playerUI.entityPlayer.Party.Leader == this.xui.playerUI.entityPlayer).ToString() : "false");
			return true;
		}
		if (_bindingName == "voicevisible")
		{
			_value = VoiceHelpers.InAnyVoiceChat.ToString();
			return true;
		}
		if (!(_bindingName == "voiceactive"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = this.voiceActive.ToString();
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.playerUI.entityPlayer.PartyJoined += this.Player_PartyChanged;
		this.xui.playerUI.entityPlayer.PartyChanged += this.Player_PartyChanged;
		this.xui.playerUI.entityPlayer.PartyLeave += this.Player_PartyChanged;
		this.playerDead = this.xui.playerUI.entityPlayer.IsDead();
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.entityPlayer.PartyJoined -= this.Player_PartyChanged;
		this.xui.playerUI.entityPlayer.PartyChanged -= this.Player_PartyChanged;
		this.xui.playerUI.entityPlayer.PartyLeave -= this.Player_PartyChanged;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.xui.playerUI.entityPlayer == null)
		{
			return;
		}
		float time = Time.time;
		bool flag = time > this.updateTime;
		bool flag2 = this.xui.playerUI.entityPlayer.IsDead();
		bool flag3 = flag2 != this.playerDead;
		this.playerDead = flag2;
		if (flag || flag3 || this.updateVoiceState())
		{
			this.updateTime = time + 1f;
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Player_PartyChanged(Party _affectedParty, EntityPlayer _player)
	{
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateVoiceState()
	{
		if (GameManager.IsDedicatedServer)
		{
			return false;
		}
		if (GameStats.GetInt(EnumGameStats.GameState) != 1)
		{
			return false;
		}
		if (this.xui.playerUI.entityPlayer == null)
		{
			return false;
		}
		bool flag = VoiceHelpers.LocalPlayerTalking();
		if (flag == this.voiceActive)
		{
			return false;
		}
		this.voiceActive = flag;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ForcedRefreshTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool voiceActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool playerDead;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;
}
