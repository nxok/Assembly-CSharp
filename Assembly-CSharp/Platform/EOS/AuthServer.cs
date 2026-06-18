using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Sanctions;

namespace Platform.EOS
{
	public class AuthServer : IAuthenticationServer
	{
		public ConnectInterface connectInterface
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return ((Api)this.owner.Api).ConnectInterface;
			}
		}

		public SanctionsInterface sanctionsInterface
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return ((Api)this.owner.Api).SanctionsInterface;
			}
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
		}

		public EBeginUserAuthenticationResult AuthenticateUser(ClientInfo _cInfo)
		{
			UserIdentifierEos identifierEos = (UserIdentifierEos)_cInfo.CrossplatformId;
			Log.Out("[EOS] Verifying token for " + identifierEos.ProductUserIdString);
			EosHelpers.AssertMainThread("ASe.Auth");
			IdToken idToken = default(IdToken);
			idToken.JsonWebToken = identifierEos.Ticket;
			idToken.ProductUserId = identifierEos.ProductUserId;
			IdToken idToken2 = idToken;
			VerifyIdTokenOptions verifyIdTokenOptions = default(VerifyIdTokenOptions);
			verifyIdTokenOptions.IdToken = new IdToken?(idToken2);
			VerifyIdTokenOptions verifyIdTokenOptions2 = verifyIdTokenOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				Action<SanctionsCheckResult> <>9__1;
				this.connectInterface.VerifyIdToken(ref verifyIdTokenOptions2, null, delegate(ref VerifyIdTokenCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode != null)
					{
						Log.Error("[EOS] VerifyIdToken failed: " + _callbackData.ResultCode.ToStringCached<Result>());
						KickPlayerDelegate kickPlayerDelegate = this.kickPlayerDelegate;
						ClientInfo cInfo = _cInfo;
						GameUtils.EKickReason ekickReason = GameUtils.EKickReason.CrossPlatformAuthenticationFailed;
						int num = 50;
						string text = _callbackData.ResultCode.ToStringCached<Result>();
						kickPlayerDelegate(cInfo, new GameUtils.KickPlayerData(ekickReason, num, default(DateTime), text));
						return;
					}
					if (!_callbackData.IsAccountInfoPresent)
					{
						Log.Error("[EOS] VerifyIdToken failed: No account info");
						KickPlayerDelegate kickPlayerDelegate2 = this.kickPlayerDelegate;
						ClientInfo cInfo2 = _cInfo;
						GameUtils.EKickReason ekickReason2 = GameUtils.EKickReason.CrossPlatformAuthenticationFailed;
						int num2 = 50;
						string text = _callbackData.ResultCode.ToStringCached<Result>();
						kickPlayerDelegate2(cInfo2, new GameUtils.KickPlayerData(ekickReason2, num2, default(DateTime), text));
						return;
					}
					string text2 = _callbackData.Platform;
					string text3 = _callbackData.AccountId;
					string text4 = _callbackData.DeviceType;
					ExternalAccountType accountIdType = _callbackData.AccountIdType;
					ProductUserId productUserId = _callbackData.ProductUserId;
					EPlatformIdentifier eplatformIdentifier;
					if (!EosHelpers.AccountTypeMappings.TryGetValue(accountIdType, out eplatformIdentifier))
					{
						KickPlayerDelegate kickPlayerDelegate3 = this.kickPlayerDelegate;
						ClientInfo cInfo3 = _cInfo;
						GameUtils.EKickReason ekickReason3 = GameUtils.EKickReason.CrossPlatformAuthenticationFailed;
						int num3 = 50;
						string text = "UnsupportedAccountType " + accountIdType.ToStringCached<ExternalAccountType>();
						kickPlayerDelegate3(cInfo3, new GameUtils.KickPlayerData(ekickReason3, num3, default(DateTime), text));
						return;
					}
					if (eplatformIdentifier != _cInfo.PlatformId.PlatformIdentifier)
					{
						KickPlayerDelegate kickPlayerDelegate4 = this.kickPlayerDelegate;
						ClientInfo cInfo4 = _cInfo;
						GameUtils.EKickReason ekickReason4 = GameUtils.EKickReason.CrossPlatformAuthenticationFailed;
						int num4 = 50;
						string text = string.Concat(new string[]
						{
							"PlatformIdentifierMismatch (",
							eplatformIdentifier.ToStringCached<EPlatformIdentifier>(),
							" vs ",
							_cInfo.PlatformId.PlatformIdentifier.ToStringCached<EPlatformIdentifier>(),
							")"
						});
						kickPlayerDelegate4(cInfo4, new GameUtils.KickPlayerData(ekickReason4, num4, default(DateTime), text));
						return;
					}
					if (text3 != _cInfo.PlatformId.ReadablePlatformUserIdentifier)
					{
						KickPlayerDelegate kickPlayerDelegate5 = this.kickPlayerDelegate;
						ClientInfo cInfo5 = _cInfo;
						GameUtils.EKickReason ekickReason5 = GameUtils.EKickReason.CrossPlatformAuthenticationFailed;
						int num5 = 50;
						string text = "AccountIdMismatch (" + text3 + ")";
						kickPlayerDelegate5(cInfo5, new GameUtils.KickPlayerData(ekickReason5, num5, default(DateTime), text));
						return;
					}
					string text5 = productUserId.ToString();
					if (text5 != identifierEos.ProductUserIdString)
					{
						KickPlayerDelegate kickPlayerDelegate6 = this.kickPlayerDelegate;
						ClientInfo cInfo6 = _cInfo;
						GameUtils.EKickReason ekickReason6 = GameUtils.EKickReason.CrossPlatformAuthenticationFailed;
						int num6 = 50;
						string text = "PuidMismatch (" + text5 + ")";
						kickPlayerDelegate6(cInfo6, new GameUtils.KickPlayerData(ekickReason6, num6, default(DateTime), text));
						return;
					}
					Log.Out(string.Format("[EOS] Device={0}, Platform={1}, AccType={2}, AccId={3}, PUID={4}", new object[] { text4, text2, accountIdType, text3, productUserId }));
					_cInfo.device = EosHelpers.GetDeviceTypeFromPlatform(text2);
					_cInfo.requiresAntiCheat = _cInfo.device.RequiresAntiCheat();
					EPlayGroup eplayGroup = _cInfo.device.ToPlayGroup();
					if (eplayGroup != EPlayGroupExtensions.Current && !SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.AllowsCrossplay)
					{
						KickPlayerDelegate kickPlayerDelegate7 = this.kickPlayerDelegate;
						ClientInfo cInfo7 = _cInfo;
						GameUtils.EKickReason ekickReason7 = GameUtils.EKickReason.CrossPlatformAuthenticationFailed;
						int num7 = 50;
						string text = string.Format("NoCrossplay {0} <-> {1}", EPlayGroupExtensions.Current, eplayGroup);
						kickPlayerDelegate7(cInfo7, new GameUtils.KickPlayerData(ekickReason7, num7, default(DateTime), text));
						return;
					}
					IUserClient user = this.owner.User;
					PlatformUserIdentifierAbs platformUserIdentifierAbs;
					if ((platformUserIdentifierAbs = ((user != null) ? user.PlatformUserId : null)) == null)
					{
						IUserClient userServer = this.owner.UserServer;
						platformUserIdentifierAbs = ((userServer != null) ? userServer.PlatformUserId : null);
					}
					UserIdentifierEos userIdentifierEos = platformUserIdentifierAbs as UserIdentifierEos;
					ProductUserId productUserId2 = ((userIdentifierEos != null) ? userIdentifierEos.ProductUserId : null);
					if (GameManager.IsDedicatedServer && GamePrefs.GetBool(EnumGamePrefs.IgnoreEOSSanctions))
					{
						this.authSuccessfulDelegate(_cInfo);
						((SessionsHost)this.owner.ServerListAnnouncer).RegisterUser(_cInfo);
						return;
					}
					SanctionsCheck eosSanctionsCheck = ((Api)this.owner.Api).eosSanctionsCheck;
					SanctionsInterface sanctionsInterface = this.sanctionsInterface;
					ProductUserId productUserId3 = identifierEos.ProductUserId;
					ProductUserId productUserId4 = productUserId2;
					Action<SanctionsCheckResult> action;
					if ((action = <>9__1) == null)
					{
						action = (<>9__1 = delegate(SanctionsCheckResult result)
						{
							if (!result.Success || result.HasActiveSanctions)
							{
								this.kickPlayerDelegate(_cInfo, result.KickReason);
								return;
							}
							this.authSuccessfulDelegate(_cInfo);
							((SessionsHost)this.owner.ServerListAnnouncer).RegisterUser(_cInfo);
						});
					}
					eosSanctionsCheck.CheckSanctions(sanctionsInterface, productUserId3, productUserId4, action);
				});
			}
			return EBeginUserAuthenticationResult.Ok;
		}

		public void RemoveUser(ClientInfo _cInfo)
		{
			((SessionsHost)this.owner.ServerListAnnouncer).UnregisterUser(_cInfo);
		}

		public void StartServer(AuthenticationSuccessfulCallbackDelegate _authSuccessfulDelegate, KickPlayerDelegate _kickPlayerDelegate)
		{
			this.authSuccessfulDelegate = _authSuccessfulDelegate;
			this.kickPlayerDelegate = _kickPlayerDelegate;
		}

		public void StartServerSteamGroups(SteamGroupStatusResponse _groupStatusResponseDelegate)
		{
			throw new NotImplementedException();
		}

		public void StopServer()
		{
			this.authSuccessfulDelegate = null;
			this.kickPlayerDelegate = null;
		}

		public bool RequestUserInGroupStatus(ClientInfo _cInfo, string _steamIdGroup)
		{
			throw new NotImplementedException();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public AuthenticationSuccessfulCallbackDelegate authSuccessfulDelegate;

		[PublicizedFrom(EAccessModifier.Private)]
		public KickPlayerDelegate kickPlayerDelegate;
	}
}
