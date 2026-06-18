using System;

namespace Platform
{
	public sealed class ClientAuthenticateServerContext
	{
		public ClientAuthenticateServerContext(GameServerInfo _gameServerInfo, PlatformUserIdentifierAbs _platformUserId, PlatformUserIdentifierAbs _crossplatformUserId, ClientAuthenticateServerSuccessDelegate _success, ClientAuthenticateServerDisconnectDelegate _disconnect)
		{
			this.GameServerInfo = _gameServerInfo;
			this.PlatformUserId = _platformUserId;
			this.CrossplatformUserId = _crossplatformUserId;
			this.success = _success;
			this.disconnect = _disconnect;
		}

		public GameServerInfo GameServerInfo { get; }

		public PlatformUserIdentifierAbs PlatformUserId { get; }

		public PlatformUserIdentifierAbs CrossplatformUserId { get; }

		public void Success()
		{
			ClientAuthenticateServerSuccessDelegate clientAuthenticateServerSuccessDelegate = this.success;
			if (clientAuthenticateServerSuccessDelegate == null)
			{
				return;
			}
			clientAuthenticateServerSuccessDelegate();
		}

		public void DisconnectNoCrossplay()
		{
			string text = PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay, PermissionsManager.PermissionSources.All) ?? Localization.Get("auth_noCrossplay", false, null);
			ClientAuthenticateServerDisconnectDelegate clientAuthenticateServerDisconnectDelegate = this.disconnect;
			if (clientAuthenticateServerDisconnectDelegate == null)
			{
				return;
			}
			clientAuthenticateServerDisconnectDelegate(text);
		}

		public void DisconnectNoCrossplay(EPlayGroup otherPlayGroup)
		{
			ClientAuthenticateServerDisconnectDelegate clientAuthenticateServerDisconnectDelegate = this.disconnect;
			if (clientAuthenticateServerDisconnectDelegate == null)
			{
				return;
			}
			clientAuthenticateServerDisconnectDelegate(string.Format(Localization.Get("auth_noCrossplayBetween", false, null), EPlayGroupExtensions.Current, otherPlayGroup));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ClientAuthenticateServerSuccessDelegate success;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ClientAuthenticateServerDisconnectDelegate disconnect;
	}
}
