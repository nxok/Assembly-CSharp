using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace Platform.Steam
{
	public class LobbyHost : ILobbyHost
	{
		public string LobbyId
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = string.Empty;

		public bool IsInLobby
		{
			get
			{
				return this.CurrentLobby.m_SteamID > 0UL;
			}
		}

		public bool AllowClientLobby
		{
			get
			{
				return true;
			}
		}

		public CSteamID CurrentLobby
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.currentLobby;
			}
			[PublicizedFrom(EAccessModifier.Private)]
			set
			{
				this.currentLobby = value;
				this.LobbyId = this.currentLobby.m_SteamID.ToString();
			}
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			_owner.Api.ClientApiInitialized += delegate
			{
				if (this.m_LobbyCreated == null)
				{
					this.m_gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(new Callback<GameLobbyJoinRequested_t>.DispatchDelegate(this.Lobby_JoinRequested));
					this.m_lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(new Callback<LobbyDataUpdate_t>.DispatchDelegate(this.Lobby_DataUpdate));
					this.m_LobbyCreated = Callback<LobbyCreated_t>.Create(new Callback<LobbyCreated_t>.DispatchDelegate(this.LobbyCreated_Callback));
					this.m_LobbyEnter = Callback<LobbyEnter_t>.Create(new Callback<LobbyEnter_t>.DispatchDelegate(this.LobbyEnter_Callback));
				}
			};
		}

		public void JoinLobby(string _lobbyId, Action<LobbyHostJoinResult> _onComplete)
		{
			if (this.CurrentLobby != CSteamID.Nil)
			{
				this.ExitLobby();
			}
			this.gameServerInfo = null;
			ulong num;
			if (StringParsers.TryParseUInt64(_lobbyId, out num))
			{
				this.JoinLobby(num);
			}
			if (_onComplete != null)
			{
				LobbyHostJoinResult lobbyHostJoinResult = new LobbyHostJoinResult
				{
					success = true
				};
				_onComplete(lobbyHostJoinResult);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void JoinLobby(ulong _steamLobbyId)
		{
			if (_steamLobbyId != CSteamID.Nil.m_SteamID)
			{
				Log.Out("[Steamworks.NET] Joining Lobby");
				this.CurrentLobby = new CSteamID(_steamLobbyId);
				SteamMatchmaking.JoinLobby(this.CurrentLobby);
			}
		}

		public void UpdateLobby(GameServerInfo _gameServerInfo)
		{
			if (this.CurrentLobby != CSteamID.Nil)
			{
				this.ExitLobby();
			}
			this.gameServerInfo = null;
			if (!GameManager.IsDedicatedServer && _gameServerInfo != null)
			{
				this.gameServerInfo = _gameServerInfo;
				this.lobbyCreationAttempts = 0;
				this.createLobby();
			}
		}

		public void ExitLobby()
		{
			Log.Out("[Steamworks.NET] Exiting Lobby");
			if (this.CurrentLobby != CSteamID.Nil)
			{
				SteamMatchmaking.LeaveLobby(this.CurrentLobby);
			}
			this.CurrentLobby = CSteamID.Nil;
			this.gameServerInfo = null;
		}

		public void UpdateGameTimePlayers(ulong _time, int _players)
		{
			if (this.owner.User.UserStatus != EUserStatus.LoggedIn || this.gameServerInfo == null || this.CurrentLobby == CSteamID.Nil)
			{
				return;
			}
			if (Time.unscaledTime - this.timeLastWorldTimeUpdate < 30f)
			{
				return;
			}
			this.timeLastWorldTimeUpdate = Time.unscaledTime;
			SteamMatchmaking.SetLobbyData(this.CurrentLobby, GameInfoString.LevelName.ToStringCached<GameInfoString>(), this.gameServerInfo.GetValue(GameInfoString.LevelName));
			SteamMatchmaking.SetLobbyData(this.CurrentLobby, GameInfoInt.CurrentServerTime.ToStringCached<GameInfoInt>(), _time.ToString());
			SteamMatchmaking.SetLobbyData(this.CurrentLobby, GameInfoInt.CurrentPlayers.ToStringCached<GameInfoInt>(), _players.ToString());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void PassLobbyToInviteListener(CSteamID _lobbyId)
		{
			foreach (IJoinSessionGameInviteListener joinSessionGameInviteListener in PlatformManager.MultiPlatform.InviteListeners)
			{
				JoinSessionGameInviteListener joinSessionGameInviteListener2 = joinSessionGameInviteListener as JoinSessionGameInviteListener;
				if (joinSessionGameInviteListener2 != null)
				{
					joinSessionGameInviteListener2.SetLobby(_lobbyId);
					break;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LobbyCreated_Callback(LobbyCreated_t _val)
		{
			this.lobbyCreationAttempts++;
			if (_val.m_eResult == 1 && this.gameServerInfo != null)
			{
				Log.Out("[Steamworks.NET] Lobby creation succeeded, LobbyID={0}, server SteamID={1}, server public IP={2}, server port={3}", new object[]
				{
					_val.m_ulSteamIDLobby,
					this.gameServerInfo.GetValue(GameInfoString.SteamID),
					Utils.MaskIp(this.gameServerInfo.GetValue(GameInfoString.IP)),
					this.gameServerInfo.GetValue(GameInfoInt.Port)
				});
				this.CurrentLobby = new CSteamID(_val.m_ulSteamIDLobby);
				foreach (GameInfoString gameInfoString in EnumUtils.Values<GameInfoString>())
				{
					SteamMatchmaking.SetLobbyData(this.CurrentLobby, gameInfoString.ToStringCached<GameInfoString>(), this.gameServerInfo.GetValue(gameInfoString));
				}
				foreach (GameInfoInt gameInfoInt in EnumUtils.Values<GameInfoInt>())
				{
					SteamMatchmaking.SetLobbyData(this.CurrentLobby, gameInfoInt.ToStringCached<GameInfoInt>(), this.gameServerInfo.GetValue(gameInfoInt).ToString());
				}
				using (IEnumerator<GameInfoBool> enumerator3 = EnumUtils.Values<GameInfoBool>().GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						GameInfoBool gameInfoBool = enumerator3.Current;
						SteamMatchmaking.SetLobbyData(this.CurrentLobby, gameInfoBool.ToStringCached<GameInfoBool>(), this.gameServerInfo.GetValue(gameInfoBool).ToString());
					}
					return;
				}
			}
			if (this.lobbyCreationAttempts < 3 && this.gameServerInfo != null)
			{
				this.createLobby();
			}
			Log.Out("[Steamworks.NET] Lobby creation failed: " + _val.m_eResult.ToString());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void createLobby()
		{
			int value = this.gameServerInfo.GetValue(GameInfoInt.ServerVisibility);
			ELobbyType elobbyType;
			if (value != 1)
			{
				if (value == 2)
				{
					elobbyType = 2;
				}
				else
				{
					elobbyType = 0;
				}
			}
			else
			{
				elobbyType = 1;
			}
			ELobbyType elobbyType2 = elobbyType;
			Log.Out("[Steamworks.NET] Trying to create Lobby (visibility: " + elobbyType2.ToStringCached<ELobbyType>() + ")");
			SteamMatchmaking.CreateLobby(elobbyType2, this.gameServerInfo.GetValue(GameInfoInt.MaxPlayers) + 4);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LobbyEnter_Callback(LobbyEnter_t _val)
		{
			Log.Out("[Steamworks.NET] Lobby entered: " + _val.m_ulSteamIDLobby.ToString());
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected && this.CurrentLobby != CSteamID.Nil)
			{
				this.StartGameWithLobby(new CSteamID(_val.m_ulSteamIDLobby));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Lobby_JoinRequested(GameLobbyJoinRequested_t _val)
		{
			Log.Out("[Steamworks.NET] LobbyJoinRequested");
			this.PassLobbyToInviteListener(_val.m_steamIDLobby);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Lobby_DataUpdate(LobbyDataUpdate_t _val)
		{
			if (_val.m_ulSteamIDLobby != this.lobbyJoinRequestForId)
			{
				return;
			}
			this.lobbyJoinRequestForId = 0UL;
			Log.Out("[Steamworks.NET] JoinLobby LobbyDataUpdate: " + _val.m_bSuccess.ToString());
			CSteamID csteamID;
			csteamID..ctor(_val.m_ulSteamIDLobby);
			if (_val.m_bSuccess != 0)
			{
				this.StartGameWithLobby(csteamID);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void StartGameWithLobby(CSteamID _lobbyId)
		{
			if (_lobbyId != CSteamID.Nil)
			{
				Log.Out("[Steamworks.NET] Connecting to server from lobby");
				GameServerInfo gameServerInfo = new GameServerInfo();
				int lobbyDataCount = SteamMatchmaking.GetLobbyDataCount(_lobbyId);
				for (int i = 0; i < lobbyDataCount; i++)
				{
					string text;
					string text2;
					if (SteamMatchmaking.GetLobbyDataByIndex(_lobbyId, i, ref text, 100, ref text2, 200))
					{
						gameServerInfo.ParseAny(text, text2);
					}
				}
				SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(gameServerInfo);
				return;
			}
			Log.Warning("[Steamworks.NET] Tried starting a game with an invalid lobby");
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public CSteamID currentLobby = CSteamID.Nil;

		[PublicizedFrom(EAccessModifier.Private)]
		public int lobbyCreationAttempts;

		[PublicizedFrom(EAccessModifier.Private)]
		public float timeLastWorldTimeUpdate;

		[PublicizedFrom(EAccessModifier.Private)]
		public GameServerInfo gameServerInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<LobbyCreated_t> m_LobbyCreated;

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<LobbyEnter_t> m_LobbyEnter;

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<GameLobbyJoinRequested_t> m_gameLobbyJoinRequested;

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<LobbyDataUpdate_t> m_lobbyDataUpdate;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong lobbyJoinRequestForId;
	}
}
