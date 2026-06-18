using System;
using System.Collections.Generic;
using Platform;

public class PartyVoice
{
	public static PartyVoice Instance
	{
		get
		{
			PartyVoice partyVoice;
			if ((partyVoice = PartyVoice.instance) == null)
			{
				partyVoice = (PartyVoice.instance = new PartyVoice());
			}
			return partyVoice;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PartyVoice()
	{
		this.platformPartyVoice = PlatformManager.MultiPlatform.PartyVoice;
		if (this.platformPartyVoice != null)
		{
			this.platformPartyVoice.Initialized += this.OnPlatformPartyVoiceInitialized;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlatformPartyVoiceInitialized()
	{
		this.platformPartyVoiceInitialized = true;
		this.platformPartyVoice.OnRemotePlayerStateChanged += this.PlatformPartyVoice_OnRemotePlayerStateChanged;
		this.platformPartyVoice.OnRemotePlayerVoiceStateChanged += this.PlatformPartyVoice_OnRemotePlayerVoiceStateChanged;
		GameManager.Instance.OnLocalPlayerChanged += this.localPlayerChangedEvent;
		World world = GameManager.Instance.World;
		EntityPlayerLocal entityPlayerLocal = ((world != null) ? world.GetPrimaryPlayer() : null);
		if (entityPlayerLocal != null)
		{
			this.gameStarted(entityPlayerLocal);
		}
		PlatformUserManager.BlockedStateChanged += this.playerBlockStateChanged;
		this.gamePrefChanged(EnumGamePrefs.OptionsVoiceVolumeLevel);
		this.gamePrefChanged(EnumGamePrefs.OptionsVoiceInputDevice);
		this.gamePrefChanged(EnumGamePrefs.OptionsVoiceOutputDevice);
		GamePrefs.OnGamePrefChanged += this.gamePrefChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void localPlayerChangedEvent(EntityPlayerLocal _newLocalPlayer)
	{
		if (_newLocalPlayer == null)
		{
			this.gameEnded();
			return;
		}
		this.gameStarted(_newLocalPlayer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameStarted(EntityPlayerLocal _newLocalPlayer)
	{
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.OfflineMode)
		{
			return;
		}
		this.localPlayer = _newLocalPlayer;
		this.localPlayer.PartyJoined += this.playerJoinedParty;
		this.localPlayer.PartyChanged += this.playerJoinedParty;
		this.localPlayer.PartyLeave += this.playerLeftParty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameEnded()
	{
		if (this.localPlayer != null)
		{
			this.localPlayer.PartyJoined -= this.playerJoinedParty;
			this.localPlayer.PartyChanged -= this.playerJoinedParty;
			this.localPlayer.PartyLeave -= this.playerLeftParty;
			this.localPlayer = null;
		}
		this.platformPartyVoice.LeaveLobby();
		this.playerVoiceStates.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerJoinedParty(Party _affectedParty, EntityPlayer _player)
	{
		bool flag = _affectedParty.Leader == this.localPlayer;
		if (!this.platformPartyVoice.InLobbyOrProgress)
		{
			if (flag)
			{
				this.platformPartyVoice.CreateLobby(delegate(string _lobbyId)
				{
					if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SetVoiceLobby, _player.entityId, _player.entityId, null, _lobbyId), false);
						return;
					}
					Party.ServerHandleSetVoiceLoby(_player, _lobbyId);
				});
				return;
			}
			if (!string.IsNullOrEmpty(_affectedParty.VoiceLobbyId))
			{
				this.platformPartyVoice.JoinLobby(_affectedParty.VoiceLobbyId);
				return;
			}
		}
		else if (this.platformPartyVoice.InLobby && this.platformPartyVoice.IsLobbyOwner() && !flag)
		{
			this.promoteLeader(_affectedParty);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerLeftParty(Party _affectedParty, EntityPlayer _player)
	{
		int num = ((_affectedParty == null || _affectedParty.LeaderIndex < 0 || _affectedParty.LeaderIndex > 8 || _affectedParty.MemberList.Count == 0) ? 1 : 0);
		bool flag = this.platformPartyVoice.IsLobbyOwner();
		if (num == 0 && flag)
		{
			this.promoteLeader(_affectedParty);
		}
		this.platformPartyVoice.LeaveLobby();
		this.playerVoiceStates.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void promoteLeader(Party _affectedParty)
	{
		int entityId = _affectedParty.Leader.entityId;
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(entityId);
		if (playerDataFromEntityID == null)
		{
			Log.Error(string.Format("[Voice] Can not promote lobby owner, no persistent data for party leader {0}", entityId));
			return;
		}
		this.platformPartyVoice.PromoteLeader(playerDataFromEntityID.PrimaryId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlatformPartyVoice_OnRemotePlayerStateChanged(PlatformUserIdentifierAbs _userIdentifier, IPartyVoice.EVoiceChannelAction _memberChannelAction)
	{
		IPlatformUserData orCreate = PlatformUserManager.GetOrCreate(_userIdentifier);
		if (_memberChannelAction == IPartyVoice.EVoiceChannelAction.Joined)
		{
			this.playerVoiceStates[_userIdentifier] = IPartyVoice.EVoiceMemberState.Normal;
			this.platformPartyVoice.BlockUser(orCreate.PrimaryId, orCreate.Blocked[EBlockType.VoiceChat].IsBlocked());
			return;
		}
		if (_memberChannelAction != IPartyVoice.EVoiceChannelAction.Left)
		{
			return;
		}
		this.playerVoiceStates.Remove(_userIdentifier);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlatformPartyVoice_OnRemotePlayerVoiceStateChanged(PlatformUserIdentifierAbs _userIdentifier, IPartyVoice.EVoiceMemberState _voiceState)
	{
		this.playerVoiceStates[_userIdentifier] = _voiceState;
	}

	public IPartyVoice.EVoiceMemberState GetVoiceMemberState(PlatformUserIdentifierAbs _userIdentifier)
	{
		IPartyVoice.EVoiceMemberState evoiceMemberState;
		if (!this.playerVoiceStates.TryGetValue(_userIdentifier, out evoiceMemberState))
		{
			return IPartyVoice.EVoiceMemberState.Disabled;
		}
		return evoiceMemberState;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerBlockStateChanged(IPlatformUserData _ppd, EBlockType _blockType, EUserBlockState _blockState)
	{
		if (_blockType != EBlockType.VoiceChat || !this.playerVoiceStates.ContainsKey(_ppd.PrimaryId))
		{
			return;
		}
		this.platformPartyVoice.BlockUser(_ppd.PrimaryId, _blockState.IsBlocked());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref == EnumGamePrefs.OptionsVoiceVolumeLevel)
		{
			this.platformPartyVoice.OutputVolume = GamePrefs.GetFloat(EnumGamePrefs.OptionsVoiceVolumeLevel);
			return;
		}
		if (_pref == EnumGamePrefs.OptionsVoiceInputDevice)
		{
			this.platformPartyVoice.SetInputDevice(GamePrefs.GetString(EnumGamePrefs.OptionsVoiceInputDevice));
			return;
		}
		if (_pref != EnumGamePrefs.OptionsVoiceOutputDevice)
		{
			return;
		}
		this.platformPartyVoice.SetOutputDevice(GamePrefs.GetString(EnumGamePrefs.OptionsVoiceOutputDevice));
	}

	public void Update()
	{
		if (!this.platformPartyVoiceInitialized)
		{
			return;
		}
		if (this.localPlayer == null)
		{
			return;
		}
		if (!this.platformPartyVoice.InLobby)
		{
			return;
		}
		this.platformPartyVoice.MuteSelf = !VoiceHelpers.PlatformVoiceEnabled || !VoiceHelpers.PushToTalkPressed();
		this.platformPartyVoice.MuteOthers = !VoiceHelpers.PlatformVoiceEnabled;
	}

	public bool InVoice
	{
		get
		{
			if (!this.platformPartyVoiceInitialized)
			{
				return false;
			}
			if (this.localPlayer == null)
			{
				return false;
			}
			LocalPlayerUI playerUI = this.localPlayer.PlayerUI;
			return !(playerUI == null) && playerUI.playerInput != null && this.platformPartyVoice.InLobby;
		}
	}

	public bool SendingVoice
	{
		get
		{
			return this.InVoice && !this.platformPartyVoice.MuteSelf;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PartyVoice instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IPartyVoice platformPartyVoice;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool platformPartyVoiceInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlatformUserIdentifierAbs, IPartyVoice.EVoiceMemberState> playerVoiceStates = new Dictionary<PlatformUserIdentifierAbs, IPartyVoice.EVoiceMemberState>();
}
