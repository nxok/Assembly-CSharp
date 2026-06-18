using System;
using Steamworks;

namespace Platform.Steam
{
	public class MultiplayerInvitationDialogSteam : IMultiplayerInvitationDialog
	{
		public bool CanShow
		{
			get
			{
				return this.lobbyHost != null && this.lobbyHost.IsInLobby;
			}
		}

		public void Init(IPlatform owner)
		{
			this.lobbyHost = (LobbyHost)owner.LobbyHost;
		}

		public void ShowInviteDialog()
		{
			if (this.lobbyHost == null)
			{
				Log.Error("[Steam] Cannot open invite dialog, lobby host is null");
				return;
			}
			string lobbyId = this.lobbyHost.LobbyId;
			if (string.IsNullOrEmpty(lobbyId))
			{
				Log.Error("[Steam] Cannot open invite dialog, no lobby id set");
				return;
			}
			ulong num;
			if (StringParsers.TryParseUInt64(lobbyId, out num))
			{
				Log.Out(string.Format("[Steam] Opening invite dialog for lobby: {0}", num));
				SteamFriends.ActivateGameOverlayInviteDialog(new CSteamID(num));
				return;
			}
			Log.Error("[Steam] Cannot open invite dialog, could not parse Steam lobby id: " + lobbyId);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public LobbyHost lobbyHost;
	}
}
