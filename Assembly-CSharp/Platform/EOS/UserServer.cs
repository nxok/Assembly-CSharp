using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

namespace Platform.EOS
{
	public class UserServer : UserBase
	{
		public override EUserPerms Permissions
		{
			get
			{
				return EUserPerms.All;
			}
		}

		public override void Login(LoginUserCallback _delegate)
		{
			if (base.UserStatus == EUserStatus.LoggedIn)
			{
				Log.Out("[EOS] Login already done.");
				this.eosLoginDone(_delegate);
				return;
			}
			Log.Out("[EOS] Login");
			EosHelpers.TestEosConnection(delegate(bool _success)
			{
				if (_success)
				{
					this.startLogin(_delegate, false);
					return;
				}
				this.UserStatus = EUserStatus.OfflineMode;
				_delegate(this.Owner, EApiStatusReason.Other, "No connection to EOS backend");
			});
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void startLogin(LoginUserCallback _delegate, bool _refreshing = false)
		{
			this.fetchDeviceId(_delegate, _refreshing);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void fetchDeviceId(LoginUserCallback _delegate, bool _refreshing)
		{
			Log.Warning(string.Format("[EOS] [DEVICEID] Fetching device id from native platform, with delegate={0}, refreshing={1}", _delegate != null, _refreshing));
			CreateDeviceIdOptions createDeviceIdOptions = default(CreateDeviceIdOptions);
			createDeviceIdOptions.DeviceModel = "DedicatedServer";
			CreateDeviceIdOptions createDeviceIdOptions2 = createDeviceIdOptions;
			base.ConnectInterface.CreateDeviceId(ref createDeviceIdOptions2, null, delegate(ref CreateDeviceIdCallbackInfo _result)
			{
				if (_result.ResultCode == null || _result.ResultCode == 24)
				{
					this.connectLogin(null, null, 10, _delegate, _refreshing, "DedicatedServer");
					return;
				}
				Log.Error("[EOS] [DEVICEID] Failed creating DeviceId: " + _result.ResultCode.ToStringCached<Result>());
				this.UserStatus = EUserStatus.TemporaryError;
				LoginUserCallback @delegate = _delegate;
				if (@delegate == null)
				{
					return;
				}
				@delegate(this.Owner, EApiStatusReason.NoLoginTicket, null);
			});
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void connectCreateUser(ContinuanceToken _continuanceToken, LoginUserCallback _callback)
		{
			throw new NotImplementedException();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void eosLoggedIn(ProductUserId _puid, LoginUserCallback _callback)
		{
			base.eosLoggedIn(_puid, _callback);
			this.eosLoginDone(_callback);
		}
	}
}
