using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

namespace Platform.EOS
{
	public class EosUserIdMapper : IUserIdentifierMappingService
	{
		public EosUserIdMapper(Api _eosApi, User _eosUser)
		{
			this.api = _eosApi;
			this.user = _eosUser;
		}

		public bool CanQuery(PlatformUserIdentifierAbs _id)
		{
			return _id is UserIdentifierEos;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool TryValidateUser(out ProductUserId loggedInUser, out MappedAccountQueryResult failureResult)
		{
			loggedInUser = null;
			failureResult = MappedAccountQueryResult.QueryFailed;
			PlatformUserIdentifierAbs platformUserId = this.user.PlatformUserId;
			if (platformUserId == null)
			{
				Log.Out("[EOS] Cannot query mapped account details yet - EOS does not have info yet");
				failureResult = MappedAccountQueryResult.NotReady;
				return false;
			}
			UserIdentifierEos userIdentifierEos = platformUserId as UserIdentifierEos;
			if (userIdentifierEos == null)
			{
				Log.Error(string.Format("[EOS] Cannot query mapped account details. EosUserIdMapper got non-EOS id type: {0}", platformUserId));
				return false;
			}
			loggedInUser = userIdentifierEos.ProductUserId;
			if (loggedInUser == null)
			{
				Log.Out(string.Format("[EOS] Cannot query mapped account details - EOS does not have info yet for {0}", userIdentifierEos));
				failureResult = MappedAccountQueryResult.NotReady;
				return false;
			}
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool TryValidateRequest(PlatformUserIdentifierAbs _id, EPlatformIdentifier _platform, out ProductUserId _puid)
		{
			UserIdentifierEos userIdentifierEos = _id as UserIdentifierEos;
			if (userIdentifierEos == null)
			{
				Log.Error(string.Format("[EOS] Cannot retrieve mapped account details, {0} is not an eos product user id", _id));
				_puid = null;
				return false;
			}
			_puid = userIdentifierEos.ProductUserId;
			if (!EosHelpers.PlatformIdentifierMappings.ContainsKey(_platform))
			{
				Log.Error(string.Format("[EOS] Cannot retrieve mapped acount details, target platform {0} does not map to a known external account type", _platform));
				return false;
			}
			return true;
		}

		public void QueryMappedAccountDetails(PlatformUserIdentifierAbs _id, EPlatformIdentifier _platform, MappedAccountQueryCallback _callback)
		{
			ProductUserId productUserId;
			MappedAccountQueryResult mappedAccountQueryResult;
			if (!this.TryValidateUser(out productUserId, out mappedAccountQueryResult))
			{
				_callback(mappedAccountQueryResult, _platform, null, null);
				return;
			}
			ProductUserId productUserId2;
			if (!EosUserIdMapper.TryValidateRequest(_id, _platform, out productUserId2))
			{
				_callback(MappedAccountQueryResult.QueryFailed, _platform, null, null);
				return;
			}
			this.QueryMappedExternalAccount(productUserId, productUserId2, _platform, _callback);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void QueryMappedExternalAccount(ProductUserId _loggedInUser, ProductUserId _puid, EPlatformIdentifier _platform, MappedAccountQueryCallback _callback)
		{
			if (!ThreadManager.IsMainThread())
			{
				ThreadManager.AddSingleTaskMainThread("QueryEosMappedAccount", delegate(object _)
				{
					this.QueryMappedExternalAccount(_loggedInUser, _puid, _platform, _callback);
				}, null);
				return;
			}
			if (this.api.ConnectInterface == null)
			{
				Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
				_callback(MappedAccountQueryResult.QueryFailed, _platform, null, null);
				return;
			}
			ExternalAccountType externalAccountType;
			if (!EosHelpers.PlatformIdentifierMappings.TryGetValue(_platform, out externalAccountType))
			{
				Log.Out(string.Format("[EOS] Unknown external account type for {0}", _platform));
				_callback(MappedAccountQueryResult.QueryFailed, _platform, null, null);
				return;
			}
			QueryProductUserIdMappingsOptions queryProductUserIdMappingsOptions = default(QueryProductUserIdMappingsOptions);
			queryProductUserIdMappingsOptions.LocalUserId = _loggedInUser;
			queryProductUserIdMappingsOptions.ProductUserIds = new ProductUserId[] { _puid };
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.api.ConnectInterface.QueryProductUserIdMappings(ref queryProductUserIdMappingsOptions, queryProductUserIdMappingsOptions.ProductUserIds, delegate(ref QueryProductUserIdMappingsCallbackInfo _response)
				{
					if (_response.ResultCode != null)
					{
						Log.Out(string.Format("[EOS] QueryProductUserIdMappings failed {0}", _response.ResultCode));
						_callback(MappedAccountQueryResult.QueryFailed, _platform, null, null);
						return;
					}
					if (this.api.ConnectInterface == null)
					{
						Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
						_callback(MappedAccountQueryResult.QueryFailed, _platform, null, null);
						return;
					}
					ProductUserId productUserId = ((ProductUserId[])_response.ClientData)[0];
					CopyProductUserExternalAccountByAccountTypeOptions copyProductUserExternalAccountByAccountTypeOptions = default(CopyProductUserExternalAccountByAccountTypeOptions);
					copyProductUserExternalAccountByAccountTypeOptions.TargetUserId = productUserId;
					copyProductUserExternalAccountByAccountTypeOptions.AccountIdType = externalAccountType;
					ExternalAccountInfo externalAccountInfo;
					if (!this.TryCopyResult(copyProductUserExternalAccountByAccountTypeOptions, out externalAccountInfo))
					{
						_callback(MappedAccountQueryResult.MappingNotFound, _platform, null, null);
						return;
					}
					if (Api.DebugLevel == Api.EDebugLevel.Verbose)
					{
						Log.Out(string.Format("[EOS] found external account for {0}: Type: {1}, Id: {2}", _puid, externalAccountType, externalAccountInfo.AccountId));
					}
					_callback(MappedAccountQueryResult.Success, _platform, externalAccountInfo.AccountId, externalAccountInfo.DisplayName);
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool TryCopyResult(CopyProductUserExternalAccountByAccountTypeOptions _copyOptions, out ExternalAccountInfo _externalAccountInfo)
		{
			object lockObject = AntiCheatCommon.LockObject;
			ExternalAccountInfo? externalAccountInfo;
			Result result;
			lock (lockObject)
			{
				result = this.api.ConnectInterface.CopyProductUserExternalAccountByAccountType(ref _copyOptions, ref externalAccountInfo);
			}
			if (result != null)
			{
				Log.Out(string.Format("[EOS] {0} copy failed. Result: {1}", _copyOptions.TargetUserId, result));
				_externalAccountInfo = default(ExternalAccountInfo);
				return false;
			}
			if (externalAccountInfo == null)
			{
				Log.Out(string.Format("[EOS] {0} copy failed, null info", _copyOptions.TargetUserId));
				_externalAccountInfo = default(ExternalAccountInfo);
				return false;
			}
			_externalAccountInfo = externalAccountInfo.Value;
			return true;
		}

		public void QueryMappedAccountsDetails(IReadOnlyList<MappedAccountRequest> _requests, MappedAccountsQueryCallback _callback)
		{
			if (_requests.Count == 0)
			{
				_callback(_requests);
				return;
			}
			ProductUserId productUserId;
			MappedAccountQueryResult mappedAccountQueryResult;
			if (!this.TryValidateUser(out productUserId, out mappedAccountQueryResult))
			{
				foreach (MappedAccountRequest mappedAccountRequest in _requests)
				{
					mappedAccountRequest.Result = mappedAccountQueryResult;
				}
				_callback(_requests);
				return;
			}
			this.QueryMappedExternalAccounts(productUserId, _requests, _callback);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void QueryMappedExternalAccounts(ProductUserId _loggedInUser, IReadOnlyList<MappedAccountRequest> _requests, MappedAccountsQueryCallback _callback)
		{
			if (!ThreadManager.IsMainThread())
			{
				ThreadManager.AddSingleTaskMainThread("QueryEosMappedAccounts", delegate(object _)
				{
					this.QueryMappedExternalAccounts(_loggedInUser, _requests, _callback);
				}, null);
				return;
			}
			if (this.api.ConnectInterface == null)
			{
				Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
				foreach (MappedAccountRequest mappedAccountRequest in _requests)
				{
					mappedAccountRequest.Result = MappedAccountQueryResult.QueryFailed;
				}
				_callback(_requests);
				return;
			}
			List<ProductUserId> puids = null;
			List<int> requestIndices = null;
			for (int i = 0; i < _requests.Count; i++)
			{
				MappedAccountRequest mappedAccountRequest2 = _requests[i];
				ProductUserId productUserId;
				if (!EosUserIdMapper.TryValidateRequest(mappedAccountRequest2.Id, mappedAccountRequest2.Platform, out productUserId))
				{
					mappedAccountRequest2.Result = MappedAccountQueryResult.QueryFailed;
				}
				else
				{
					if (puids == null)
					{
						puids = new List<ProductUserId>();
					}
					if (requestIndices == null)
					{
						requestIndices = new List<int>();
					}
					puids.Add(productUserId);
					requestIndices.Add(i);
				}
			}
			if (puids == null)
			{
				_callback(_requests);
				return;
			}
			QueryProductUserIdMappingsOptions queryProductUserIdMappingsOptions = default(QueryProductUserIdMappingsOptions);
			queryProductUserIdMappingsOptions.LocalUserId = _loggedInUser;
			queryProductUserIdMappingsOptions.ProductUserIds = puids.ToArray();
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.api.ConnectInterface.QueryProductUserIdMappings(ref queryProductUserIdMappingsOptions, null, delegate(ref QueryProductUserIdMappingsCallbackInfo _response)
				{
					if (_response.ResultCode != null)
					{
						Log.Out(string.Format("[EOS] QueryProductUserIdMappings failed {0}", _response.ResultCode));
						foreach (MappedAccountRequest mappedAccountRequest3 in _requests)
						{
							mappedAccountRequest3.Result = MappedAccountQueryResult.QueryFailed;
						}
						_callback(_requests);
						return;
					}
					if (this.api.ConnectInterface == null)
					{
						Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
						foreach (MappedAccountRequest mappedAccountRequest4 in _requests)
						{
							mappedAccountRequest4.Result = MappedAccountQueryResult.QueryFailed;
						}
						_callback(_requests);
						return;
					}
					CopyProductUserExternalAccountByAccountTypeOptions copyProductUserExternalAccountByAccountTypeOptions = default(CopyProductUserExternalAccountByAccountTypeOptions);
					for (int j = 0; j < puids.Count; j++)
					{
						int num = requestIndices[j];
						MappedAccountRequest mappedAccountRequest5 = _requests[num];
						ProductUserId productUserId2 = puids[j];
						copyProductUserExternalAccountByAccountTypeOptions.TargetUserId = productUserId2;
						copyProductUserExternalAccountByAccountTypeOptions.AccountIdType = EosHelpers.PlatformIdentifierMappings[mappedAccountRequest5.Platform];
						ExternalAccountInfo externalAccountInfo;
						if (!this.TryCopyResult(copyProductUserExternalAccountByAccountTypeOptions, out externalAccountInfo))
						{
							mappedAccountRequest5.Result = MappedAccountQueryResult.MappingNotFound;
						}
						else
						{
							mappedAccountRequest5.MappedAccountId = externalAccountInfo.AccountId;
							mappedAccountRequest5.DisplayName = externalAccountInfo.DisplayName;
							mappedAccountRequest5.Result = MappedAccountQueryResult.Success;
							if (Api.DebugLevel == Api.EDebugLevel.Verbose)
							{
								Log.Out(string.Format("[EOS] found external account for {0}: Type: {1}, Id: {2}", productUserId2, externalAccountInfo.AccountIdType, externalAccountInfo.AccountId));
							}
						}
					}
					_callback(_requests);
				});
			}
		}

		public bool CanReverseQuery(EPlatformIdentifier _platform, string _platformId)
		{
			return EosHelpers.PlatformIdentifierMappings.ContainsKey(_platform);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool TryValidateReverseRequest(EPlatformIdentifier _platform, out ExternalAccountType _externalAccountType)
		{
			if (!EosHelpers.PlatformIdentifierMappings.TryGetValue(_platform, out _externalAccountType))
			{
				Log.Error(string.Format("[EOS] Cannot retrieve reverse mapped account details, target platform {0} does not map to a known external account type", _platform));
				return false;
			}
			return true;
		}

		public void ReverseQueryMappedAccountDetails(EPlatformIdentifier _platform, string _platformId, MappedAccountReverseQueryCallback _callback)
		{
			EosUserIdMapper.<>c__DisplayClass13_0 CS$<>8__locals1 = new EosUserIdMapper.<>c__DisplayClass13_0();
			CS$<>8__locals1._callback = _callback;
			this.ReverseQueryMappedAccountsDetails(new MappedAccountReverseRequest[]
			{
				new MappedAccountReverseRequest(_platform, _platformId)
			}, new MappedAccountsReverseQueryCallback(CS$<>8__locals1.<ReverseQueryMappedAccountDetails>g__Callback|0));
		}

		public void ReverseQueryMappedAccountsDetails(IReadOnlyList<MappedAccountReverseRequest> _requests, MappedAccountsReverseQueryCallback _callback)
		{
			if (_requests.Count == 0)
			{
				_callback(_requests);
				return;
			}
			ProductUserId productUserId;
			MappedAccountQueryResult mappedAccountQueryResult;
			if (!this.TryValidateUser(out productUserId, out mappedAccountQueryResult))
			{
				foreach (MappedAccountReverseRequest mappedAccountReverseRequest in _requests)
				{
					mappedAccountReverseRequest.Result = mappedAccountQueryResult;
				}
				_callback(_requests);
				return;
			}
			this.QueryExternalAccountMappings(productUserId, _requests, _callback);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void QueryExternalAccountMappings(ProductUserId _loggedInUser, IReadOnlyList<MappedAccountReverseRequest> _requests, MappedAccountsReverseQueryCallback _callback)
		{
			EosUserIdMapper.<>c__DisplayClass15_0 CS$<>8__locals1 = new EosUserIdMapper.<>c__DisplayClass15_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1._loggedInUser = _loggedInUser;
			CS$<>8__locals1._requests = _requests;
			CS$<>8__locals1._callback = _callback;
			if (!ThreadManager.IsMainThread())
			{
				ThreadManager.AddSingleTaskMainThread("QueryEosExternalAccountMappings", delegate(object _)
				{
					CS$<>8__locals1.<>4__this.QueryExternalAccountMappings(CS$<>8__locals1._loggedInUser, CS$<>8__locals1._requests, CS$<>8__locals1._callback);
				}, null);
				return;
			}
			if (this.api.ConnectInterface == null)
			{
				Log.Out("[EOS] QueryExternalAccountMappings failed, connect interface null");
				foreach (MappedAccountReverseRequest mappedAccountReverseRequest in CS$<>8__locals1._requests)
				{
					mappedAccountReverseRequest.Result = MappedAccountQueryResult.QueryFailed;
				}
				CS$<>8__locals1._callback(CS$<>8__locals1._requests);
				return;
			}
			CS$<>8__locals1.requestsByType = null;
			foreach (MappedAccountReverseRequest mappedAccountReverseRequest2 in CS$<>8__locals1._requests)
			{
				ExternalAccountType externalAccountType;
				if (!EosUserIdMapper.TryValidateReverseRequest(mappedAccountReverseRequest2.Platform, out externalAccountType))
				{
					mappedAccountReverseRequest2.Result = MappedAccountQueryResult.QueryFailed;
				}
				else
				{
					if (CS$<>8__locals1.requestsByType == null)
					{
						CS$<>8__locals1.requestsByType = new Dictionary<ExternalAccountType, List<MappedAccountReverseRequest>>();
					}
					List<MappedAccountReverseRequest> list;
					if (!CS$<>8__locals1.requestsByType.TryGetValue(externalAccountType, out list))
					{
						list = new List<MappedAccountReverseRequest>();
						CS$<>8__locals1.requestsByType[externalAccountType] = list;
					}
					list.Add(mappedAccountReverseRequest2);
				}
			}
			if (CS$<>8__locals1.requestsByType == null)
			{
				CS$<>8__locals1._callback(CS$<>8__locals1._requests);
				return;
			}
			CS$<>8__locals1.requestsFinished = 0;
			foreach (KeyValuePair<ExternalAccountType, List<MappedAccountReverseRequest>> keyValuePair in CS$<>8__locals1.requestsByType)
			{
				ExternalAccountType externalAccountType2;
				List<MappedAccountReverseRequest> list2;
				keyValuePair.Deconstruct(out externalAccountType2, out list2);
				ExternalAccountType externalAccountType3 = externalAccountType2;
				List<MappedAccountReverseRequest> list3 = list2;
				this.QueryExternalAccountMappings(CS$<>8__locals1._loggedInUser, externalAccountType3, list3, new Action(CS$<>8__locals1.<QueryExternalAccountMappings>g__Callback|1));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void QueryExternalAccountMappings(ProductUserId _loggedInUser, ExternalAccountType _externalAccountType, IReadOnlyList<MappedAccountReverseRequest> _requests, Action _callback)
		{
			Utf8String[] externalAccountIds = new Utf8String[_requests.Count];
			for (int i = 0; i < _requests.Count; i++)
			{
				externalAccountIds[i] = _requests[i].Id;
			}
			QueryExternalAccountMappingsOptions queryExternalAccountMappingsOptions = default(QueryExternalAccountMappingsOptions);
			queryExternalAccountMappingsOptions.LocalUserId = _loggedInUser;
			queryExternalAccountMappingsOptions.AccountIdType = _externalAccountType;
			queryExternalAccountMappingsOptions.ExternalAccountIds = externalAccountIds;
			QueryExternalAccountMappingsOptions queryExternalAccountMappingsOptions2 = queryExternalAccountMappingsOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.api.ConnectInterface.QueryExternalAccountMappings(ref queryExternalAccountMappingsOptions2, null, delegate(ref QueryExternalAccountMappingsCallbackInfo _response)
				{
					if (_response.ResultCode != null)
					{
						Log.Out(string.Format("[EOS] QueryProductUserIdMappings failed {0}", _response.ResultCode));
						foreach (MappedAccountReverseRequest mappedAccountReverseRequest in _requests)
						{
							mappedAccountReverseRequest.Result = MappedAccountQueryResult.QueryFailed;
						}
						_callback();
						return;
					}
					if (this.api.ConnectInterface == null)
					{
						Log.Out("[EOS] QueryProductUserIdMappings failed, connect interface null");
						foreach (MappedAccountReverseRequest mappedAccountReverseRequest2 in _requests)
						{
							mappedAccountReverseRequest2.Result = MappedAccountQueryResult.QueryFailed;
						}
						_callback();
						return;
					}
					GetExternalAccountMappingsOptions getExternalAccountMappingsOptions = default(GetExternalAccountMappingsOptions);
					getExternalAccountMappingsOptions.LocalUserId = _loggedInUser;
					getExternalAccountMappingsOptions.AccountIdType = _externalAccountType;
					GetExternalAccountMappingsOptions getExternalAccountMappingsOptions2 = getExternalAccountMappingsOptions;
					for (int j = 0; j < externalAccountIds.Length; j++)
					{
						getExternalAccountMappingsOptions2.TargetExternalUserId = externalAccountIds[j];
						MappedAccountReverseRequest mappedAccountReverseRequest3 = _requests[j];
						object lockObject2 = AntiCheatCommon.LockObject;
						ProductUserId externalAccountMapping;
						lock (lockObject2)
						{
							externalAccountMapping = this.api.ConnectInterface.GetExternalAccountMapping(ref getExternalAccountMappingsOptions2);
						}
						if (externalAccountMapping == null || !externalAccountMapping.IsValid())
						{
							mappedAccountReverseRequest3.Result = MappedAccountQueryResult.MappingNotFound;
						}
						else
						{
							mappedAccountReverseRequest3.Result = MappedAccountQueryResult.Success;
							mappedAccountReverseRequest3.PlatformId = new UserIdentifierEos(externalAccountMapping);
							if (Api.DebugLevel == Api.EDebugLevel.Verbose)
							{
								Log.Out(string.Format("[EOS] found EOS account {0} from external account: Type: {1}, Id: {2}", externalAccountMapping, _externalAccountType, mappedAccountReverseRequest3.Id));
							}
						}
					}
					_callback();
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Api api;

		[PublicizedFrom(EAccessModifier.Private)]
		public User user;
	}
}
