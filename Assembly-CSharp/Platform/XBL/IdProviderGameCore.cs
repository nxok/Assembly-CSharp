using System;
using Platform.EOS;
using Platform.Shared;

namespace Platform.XBL
{
	public class IdProviderGameCore
	{
		public UserIdentifierXbl Id
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public IdProviderGameCore(User _nativeUser, IUserClient _crossplatformUser)
		{
			this.m_nativeUser = _nativeUser;
			if (_crossplatformUser != null)
			{
				_crossplatformUser.UserLoggedIn += this.CrossplatformLogin;
				return;
			}
			this.m_nativeUser.UserLoggedIn += this.NativeLogin;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void NativeLogin(IPlatform _platform)
		{
			this.Id = new UserIdentifierXbl(this.m_nativeUser.LocalID.Value.ToString());
			Log.Out("[XBL] Initializing user id with Local ID");
			XblXuidMapper.SetXuid(this.Id, this.m_nativeUser.Xuid);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CrossplatformLogin(IPlatform _platform)
		{
			PlatformUserIdentifierAbs nativePlatformUserId = ((User)_platform.User).NativePlatformUserId;
			if (nativePlatformUserId != null)
			{
				UserIdentifierXbl userIdentifierXbl = nativePlatformUserId as UserIdentifierXbl;
				if (userIdentifierXbl == null)
				{
					Log.Error(string.Format("[XBL] Got different native platform id from EOS: {0}", nativePlatformUserId.PlatformIdentifier));
					return;
				}
				this.Id = userIdentifierXbl;
				Log.Out("[XBL] Initializing user id with PXUID " + userIdentifierXbl.CombinedString);
				PlatformIdCache.SetCachedId(this.Id);
				XblXuidMapper.SetXuid(this.Id, this.m_nativeUser.Xuid);
			}
		}

		public bool LoadOfflineId()
		{
			UserIdentifierXbl userIdentifierXbl;
			if (PlatformIdCache.TryGetCachedId<UserIdentifierXbl>(out userIdentifierXbl))
			{
				this.Id = userIdentifierXbl;
				Log.Out("[XBL] Retrieved offline user id: " + userIdentifierXbl.CombinedString);
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly User m_nativeUser;
	}
}
