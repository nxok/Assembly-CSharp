using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Steamworks;

namespace Platform.Steam
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class JoinSessionGameInviteListener : IJoinSessionGameInviteListener
	{
		public void Init(IPlatform _owner)
		{
			if (this.m_friends_serverchange == null)
			{
				this.m_friends_serverchange = Callback<GameServerChangeRequested_t>.Create(new Callback<GameServerChangeRequested_t>.DispatchDelegate(this.Friends_GameServerChangeRequested));
			}
			_owner.Api.ClientApiInitialized += delegate
			{
				string[] commandLineArgs = Environment.GetCommandLineArgs();
				for (int i = 0; i < commandLineArgs.Length - 1; i++)
				{
					ulong num;
					if (commandLineArgs[i] == "+connect_lobby" && ulong.TryParse(commandLineArgs[i + 1], out num))
					{
						this.SetLobby(new CSteamID(num));
					}
				}
			};
		}

		[return: TupleElementNames(new string[] { "invite", "password" })]
		public ValueTuple<string, string> TakePendingInvite()
		{
			string text = this.pendingInvite;
			this.pendingInvite = null;
			string text2 = this.pendingPassword;
			this.pendingPassword = null;
			return new ValueTuple<string, string>(text, text2);
		}

		public IEnumerator ConnectToInvite(string _invite, string _password = null, Action<bool> _onFinished = null)
		{
			if (string.IsNullOrEmpty(_invite))
			{
				if (_onFinished != null)
				{
					_onFinished(false);
				}
				yield break;
			}
			if (_invite.StartsWith("Lobby:"))
			{
				ILobbyHost lobbyHost = PlatformManager.NativePlatform.LobbyHost;
				if (lobbyHost != null)
				{
					lobbyHost.JoinLobby(_invite.Substring("Lobby:".Length), null);
				}
				if (_onFinished != null)
				{
					_onFinished(true);
				}
				yield break;
			}
			string[] array = _invite.Split(':', StringSplitOptions.None);
			string text = "";
			int num = 0;
			if (array.Length == 2)
			{
				text = array[0];
				num = Convert.ToInt32(array[1]);
			}
			yield return InviteManager.HandleIpPortInvite(text, num, _password, _onFinished);
			yield break;
		}

		public string GetListenerIdentifier()
		{
			return "STM";
		}

		public void SetLobby(CSteamID _lobbyId)
		{
			this.pendingInvite = "Lobby:" + _lobbyId.m_SteamID.ToString();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Friends_GameServerChangeRequested(GameServerChangeRequested_t _value)
		{
			Log.Out("[Steamworks.NET] Friends_GameServerChangeRequested");
			this.pendingInvite = _value.m_rgchServer;
			this.pendingPassword = _value.m_rgchPassword;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string LobbyMarker = "Lobby:";

		[PublicizedFrom(EAccessModifier.Private)]
		public string pendingInvite;

		[PublicizedFrom(EAccessModifier.Private)]
		public string pendingPassword;

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<GameServerChangeRequested_t> m_friends_serverchange;
	}
}
