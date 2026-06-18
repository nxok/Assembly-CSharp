using System;
using System.Collections.Generic;

namespace Webserver
{
	public static class UserRegistrationTokens
	{
		public static bool TryValidate(string _token, out UserRegistrationTokens.RegistrationData _data)
		{
			return UserRegistrationTokens.activeTokens.TryGetValue(_token, out _data) && _data.ExpiryTime > DateTime.Now;
		}

		public static string CreateToken(string _playerName, PlatformUserIdentifierAbs _platformUserId, PlatformUserIdentifierAbs _crossPlatformUserId)
		{
			string text = Utils.GenerateGuid();
			DateTime currentTime = DateTime.Now;
			UserRegistrationTokens.activeTokens.RemoveAll(delegate(UserRegistrationTokens.RegistrationData _data)
			{
				if (!(_data.ExpiryTime < currentTime) && !_platformUserId.Equals(_data.PlatformUserId))
				{
					PlatformUserIdentifierAbs crossPlatformUserId = _crossPlatformUserId;
					return crossPlatformUserId != null && crossPlatformUserId.Equals(_data.CrossPlatformUserId);
				}
				return true;
			});
			UserRegistrationTokens.RegistrationData registrationData = new UserRegistrationTokens.RegistrationData(_playerName, _platformUserId, _crossPlatformUserId);
			UserRegistrationTokens.activeTokens[text] = registrationData;
			return text;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const float tokenExpirationMinutes = 3f;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Dictionary<string, UserRegistrationTokens.RegistrationData> activeTokens = new Dictionary<string, UserRegistrationTokens.RegistrationData>();

		public class RegistrationData
		{
			public RegistrationData(string _playerName, PlatformUserIdentifierAbs _platformUserId, PlatformUserIdentifierAbs _crossPlatformUserId)
			{
				this.PlayerName = _playerName;
				this.ExpiryTime = DateTime.Now + TimeSpan.FromMinutes(3.0);
				this.PlatformUserId = _platformUserId;
				this.CrossPlatformUserId = _crossPlatformUserId;
			}

			public readonly string PlayerName;

			public readonly DateTime ExpiryTime;

			public readonly PlatformUserIdentifierAbs PlatformUserId;

			public readonly PlatformUserIdentifierAbs CrossPlatformUserId;
		}
	}
}
