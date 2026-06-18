using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Platform.XBL
{
	public static class XblXuidMapper
	{
		public static event XblXuidMapper.XuidMappedHandler XuidMapped;

		public static event XblXuidMapper.UserIdentifierMappedHandler UserIdentifierMapped;

		public static void Enable()
		{
			if (XblXuidMapper.s_enabled)
			{
				return;
			}
			XblXuidMapper.s_enabled = true;
			Log.Out("[XBL-XuidMapper] Enabled.");
		}

		public static ulong GetXuid(PlatformUserIdentifierAbs userId)
		{
			if (!XblXuidMapper.s_enabled)
			{
				return 0UL;
			}
			XblXuidMapper.XuidState xuidState = XblXuidMapper.GetXuidState(userId);
			using (XblXuidMapper.s_xuidStateDictionaryLock.ReadLockScope())
			{
				ulong num;
				if (XblXuidMapper.s_xuidStateToXuid.TryGetByKey(xuidState, out num))
				{
					return num;
				}
			}
			if (XblXuidMapper.s_mappingService == null)
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				XblXuidMapper.s_mappingService = ((crossplatformPlatform != null) ? crossplatformPlatform.IdMappingService : null);
			}
			if (XblXuidMapper.s_mappingService == null)
			{
				Log.Error("[XBL-XuidMapper] ID mapping service required to identify Xbl users");
				return 0UL;
			}
			XblXuidMapper.ResolveXuid(xuidState);
			return 0UL;
		}

		public static void SetXuid(PlatformUserIdentifierAbs userId, ulong xuid)
		{
			if (xuid == 0UL)
			{
				return;
			}
			XblXuidMapper.XuidState xuidState = XblXuidMapper.GetXuidState(userId);
			ulong num;
			using (XblXuidMapper.s_xuidStateDictionaryLock.UpgradableReadLockScope())
			{
				if (!XblXuidMapper.s_xuidStateToXuid.TryGetByKey(xuidState, out num))
				{
					num = 0UL;
				}
				if (num == xuid)
				{
					return;
				}
				using (XblXuidMapper.s_xuidStateDictionaryLock.WriteLockScope())
				{
					XblXuidMapper.s_xuidStateToXuid.RemoveByKey(xuidState);
					XblXuidMapper.s_xuidStateToXuid.RemoveByValue(xuid);
					XblXuidMapper.s_xuidStateToXuid.Add(xuidState, xuid);
				}
				XblXuidMapper.XuidState xuidState2 = xuidState;
				lock (xuidState2)
				{
					xuidState.InProgress = false;
					xuidState.AttemptsCompleted++;
				}
			}
			if (num != 0UL)
			{
				using (XblXuidMapper.s_xuidStateDictionaryLock.ReadLockScope())
				{
					IReadOnlyCollection<PlatformUserIdentifierAbs> readOnlyCollection;
					XblXuidMapper.s_xuidStateToUserId.TryGetByKey(xuidState, out readOnlyCollection);
					Log.Warning(string.Format("[XBL-XuidMapper] Unexpected mapping change Xuid changed from '{0}' to '{1}' for UserIds: {2}", num, xuid, string.Join<PlatformUserIdentifierAbs>(", ", readOnlyCollection)));
				}
			}
			object obj = XblXuidMapper.s_xuidMappedResultTempLock;
			lock (obj)
			{
				XblXuidMapper.s_xuidMappedResultTemp.Clear();
				using (XblXuidMapper.s_xuidStateDictionaryLock.ReadLockScope())
				{
					XblXuidMapper.s_xuidStateToUserId.TryGetByKey(xuidState, XblXuidMapper.s_xuidMappedResultTemp);
				}
				XblXuidMapper.XuidMappedHandler xuidMapped = XblXuidMapper.XuidMapped;
				if (xuidMapped != null)
				{
					xuidMapped(XblXuidMapper.s_xuidMappedResultTemp, xuid);
				}
				XblXuidMapper.s_xuidMappedResultTemp.Clear();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void ResolveXuid(XblXuidMapper.XuidState xuidState)
		{
			XblXuidMapper.<>c__DisplayClass24_0 CS$<>8__locals1 = new XblXuidMapper.<>c__DisplayClass24_0();
			CS$<>8__locals1.xuidState = xuidState;
			CS$<>8__locals1.userIds = null;
			XblXuidMapper.XuidState xuidState2 = CS$<>8__locals1.xuidState;
			lock (xuidState2)
			{
				if (CS$<>8__locals1.xuidState.InProgress)
				{
					return;
				}
				using (XblXuidMapper.s_xuidStateDictionaryLock.ReadLockScope())
				{
					IReadOnlyCollection<PlatformUserIdentifierAbs> readOnlyCollection;
					if (XblXuidMapper.s_xuidStateToUserId.TryGetByKey(CS$<>8__locals1.xuidState, out readOnlyCollection))
					{
						foreach (PlatformUserIdentifierAbs platformUserIdentifierAbs in readOnlyCollection)
						{
							if (XblXuidMapper.s_mappingService.CanQuery(platformUserIdentifierAbs))
							{
								using (XblXuidMapper.s_userIdsWithNoXblMappingLock.ReadLockScope())
								{
									if (XblXuidMapper.s_userIdsWithNoXblMapping.Contains(platformUserIdentifierAbs))
									{
										continue;
									}
								}
								if (CS$<>8__locals1.userIds == null)
								{
									CS$<>8__locals1.userIds = new List<PlatformUserIdentifierAbs>();
								}
								CS$<>8__locals1.userIds.Add(platformUserIdentifierAbs);
							}
						}
					}
				}
				if (CS$<>8__locals1.userIds == null || CS$<>8__locals1.userIds.Count <= 0)
				{
					return;
				}
				CS$<>8__locals1.attempt = CS$<>8__locals1.xuidState.AttemptsCompleted + 1;
				CS$<>8__locals1.xuidState.InProgress = true;
			}
			CS$<>8__locals1.userIdsIndex = -1;
			CS$<>8__locals1.<ResolveXuid>g__ProcessNextId|1();
		}

		public static void ResolveUserIdentifiers(IReadOnlyCollection<ulong> xuids)
		{
			XblXuidMapper.<>c__DisplayClass25_0 CS$<>8__locals1 = new XblXuidMapper.<>c__DisplayClass25_0();
			if (XblXuidMapper.s_mappingService == null)
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				XblXuidMapper.s_mappingService = ((crossplatformPlatform != null) ? crossplatformPlatform.IdMappingService : null);
			}
			if (XblXuidMapper.s_mappingService == null)
			{
				return;
			}
			using (XblXuidMapper.s_xuidsCheckedForUserIdentifiersLock.ReadLockScope())
			{
				CS$<>8__locals1.xuidsToRequest = (from xuid in xuids
					where !XblXuidMapper.s_xuidsCheckedForUserIdentifiers.Contains(xuid)
					where XblXuidMapper.s_mappingService.CanReverseQuery(EPlatformIdentifier.XBL, xuid.ToString())
					select xuid).ToArray<ulong>();
			}
			if (CS$<>8__locals1.xuidsToRequest.Length == 0)
			{
				return;
			}
			CS$<>8__locals1.requests = CS$<>8__locals1.xuidsToRequest.Select((ulong xuid) => new MappedAccountReverseRequest(EPlatformIdentifier.XBL, xuid.ToString())).ToArray<MappedAccountReverseRequest>();
			Log.Out("[XBL-XuidMapper] Reverse mapping XUID(s) '" + string.Join<ulong>("','", xuids) + "'...");
			XblXuidMapper.s_mappingService.ReverseQueryMappedAccountsDetails(CS$<>8__locals1.requests, new MappedAccountsReverseQueryCallback(CS$<>8__locals1.<ResolveUserIdentifiers>g__ResolveUserIdentifiersCallback|1));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static XblXuidMapper.XuidState GetXuidState(PlatformUserIdentifierAbs userId)
		{
			XblXuidMapper.XuidState xuidState2;
			using (XblXuidMapper.s_xuidStateDictionaryLock.UpgradableReadLockScope())
			{
				XblXuidMapper.XuidState xuidState;
				if (XblXuidMapper.s_xuidStateToUserId.TryGetByValue(userId, out xuidState))
				{
					xuidState2 = xuidState;
				}
				else
				{
					using (XblXuidMapper.s_xuidStateDictionaryLock.WriteLockScope())
					{
						PlatformUserIdentifierAbs[] array = new PlatformUserIdentifierAbs[4];
						int num = PlatformUserManager.TryGetByNative(userId, array.AsSpan(0, array.Length - 1));
						array[num] = userId;
						int num2 = num + 1;
						int num3 = 0;
						while (num3 < num2 && !XblXuidMapper.s_xuidStateToUserId.TryGetByValue(array[num3], out xuidState))
						{
							num3++;
						}
						if (xuidState == null)
						{
							xuidState = new XblXuidMapper.XuidState();
						}
						for (int i = 0; i < num2; i++)
						{
							PlatformUserIdentifierAbs platformUserIdentifierAbs = array[i];
							XblXuidMapper.XuidState xuidState3;
							if (!XblXuidMapper.s_xuidStateToUserId.TryGetByValue(platformUserIdentifierAbs, out xuidState3))
							{
								XblXuidMapper.s_xuidStateToUserId.Add(xuidState, platformUserIdentifierAbs);
							}
							else if (xuidState3 != xuidState)
							{
								XblXuidMapper.s_xuidStateToUserId.RemoveByValue(platformUserIdentifierAbs);
								XblXuidMapper.s_xuidStateToUserId.Add(xuidState, platformUserIdentifierAbs);
								Log.Error(string.Format("[XBL-XuidMapper] Unexpected state merge. UserId '{0}' already had state but has been merged with UserIds: '{1}'.", array[i], string.Join<PlatformUserIdentifierAbs>("', '", array.Take(num2))));
							}
						}
					}
					xuidState2 = xuidState;
				}
			}
			return xuidState2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static XblXuidMapper.XuidState GetXuidState(ulong xuid)
		{
			if (xuid == 0UL)
			{
				return XblXuidMapper.EmptyXuidState;
			}
			XblXuidMapper.XuidState xuidState2;
			using (XblXuidMapper.s_xuidStateDictionaryLock.UpgradableReadLockScope())
			{
				XblXuidMapper.XuidState xuidState;
				if (XblXuidMapper.s_xuidStateToXuid.TryGetByValue(xuid, out xuidState))
				{
					xuidState2 = xuidState;
				}
				else
				{
					using (XblXuidMapper.s_xuidStateDictionaryLock.WriteLockScope())
					{
						xuidState = new XblXuidMapper.XuidState();
						XblXuidMapper.s_xuidStateToXuid.Add(xuidState, xuid);
					}
					xuidState2 = xuidState;
				}
			}
			return xuidState2;
		}

		public static void ResolveXuids(IReadOnlyList<XuidResolveRequest> _requests, Action<IReadOnlyList<XuidResolveRequest>> _onComplete)
		{
			XblXuidMapper.<>c__DisplayClass29_0 CS$<>8__locals1 = new XblXuidMapper.<>c__DisplayClass29_0();
			CS$<>8__locals1._requests = _requests;
			CS$<>8__locals1._onComplete = _onComplete;
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			IUserIdentifierMappingService userIdentifierMappingService = ((crossplatformPlatform != null) ? crossplatformPlatform.IdMappingService : null);
			if (userIdentifierMappingService == null)
			{
				Log.Error("[XBL-XuidMapper] Cannot resolve xuids, no mapping service available");
				CS$<>8__locals1._onComplete(CS$<>8__locals1._requests);
				return;
			}
			List<MappedAccountRequest> list = null;
			CS$<>8__locals1.requestIndices = null;
			int i = 0;
			while (i < CS$<>8__locals1._requests.Count)
			{
				XuidResolveRequest xuidResolveRequest = CS$<>8__locals1._requests[i];
				XblXuidMapper.XuidState xuidState = XblXuidMapper.GetXuidState(xuidResolveRequest.Id);
				IReadOnlyCollection<PlatformUserIdentifierAbs> readOnlyCollection;
				using (XblXuidMapper.s_xuidStateDictionaryLock.ReadLockScope())
				{
					ulong num;
					if (!XblXuidMapper.s_xuidStateToXuid.TryGetByKey(xuidState, out num))
					{
						num = 0UL;
					}
					if (num != 0UL)
					{
						xuidResolveRequest.Xuid = num;
						xuidResolveRequest.IsSuccess = true;
						goto IL_0169;
					}
					if (!XblXuidMapper.s_xuidStateToUserId.TryGetByKey(xuidState, out readOnlyCollection))
					{
						goto IL_0169;
					}
				}
				goto IL_00D0;
				IL_0169:
				i++;
				continue;
				IL_00D0:
				foreach (PlatformUserIdentifierAbs platformUserIdentifierAbs in readOnlyCollection)
				{
					if (userIdentifierMappingService.CanQuery(platformUserIdentifierAbs))
					{
						using (XblXuidMapper.s_userIdsWithNoXblMappingLock.ReadLockScope())
						{
							if (XblXuidMapper.s_userIdsWithNoXblMapping.Contains(platformUserIdentifierAbs))
							{
								continue;
							}
						}
						if (list == null)
						{
							list = new List<MappedAccountRequest>();
						}
						if (CS$<>8__locals1.requestIndices == null)
						{
							CS$<>8__locals1.requestIndices = new List<int>();
						}
						list.Add(new MappedAccountRequest(platformUserIdentifierAbs, EPlatformIdentifier.XBL));
						CS$<>8__locals1.requestIndices.Add(i);
						break;
					}
				}
				goto IL_0169;
			}
			if (list == null)
			{
				CS$<>8__locals1._onComplete(CS$<>8__locals1._requests);
				return;
			}
			userIdentifierMappingService.QueryMappedAccountsDetails(list, new MappedAccountsQueryCallback(CS$<>8__locals1.<ResolveXuids>g__Callback|0));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool s_enabled;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IUserIdentifierMappingService s_mappingService;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly XblXuidMapper.XuidState EmptyXuidState = new XblXuidMapper.XuidState();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly OneToManyDictionary<XblXuidMapper.XuidState, PlatformUserIdentifierAbs> s_xuidStateToUserId = new OneToManyDictionary<XblXuidMapper.XuidState, PlatformUserIdentifierAbs>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly BiDictionary<XblXuidMapper.XuidState, ulong> s_xuidStateToXuid = new BiDictionary<XblXuidMapper.XuidState, ulong>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ReaderWriterLockSlim s_xuidStateDictionaryLock = new ReaderWriterLockSlim();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly HashSet<PlatformUserIdentifierAbs> s_userIdsWithNoXblMapping = new HashSet<PlatformUserIdentifierAbs>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ReaderWriterLockSlim s_userIdsWithNoXblMappingLock = new ReaderWriterLockSlim();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly HashSet<ulong> s_xuidsCheckedForUserIdentifiers = new HashSet<ulong>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ReaderWriterLockSlim s_xuidsCheckedForUserIdentifiersLock = new ReaderWriterLockSlim();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly List<PlatformUserIdentifierAbs> s_xuidMappedResultTemp = new List<PlatformUserIdentifierAbs>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly object s_xuidMappedResultTempLock = new object();

		public delegate void XuidMappedHandler(IReadOnlyCollection<PlatformUserIdentifierAbs> userIds, ulong xuid);

		public delegate void UserIdentifierMappedHandler(ulong xuid, PlatformUserIdentifierAbs userId);

		[PublicizedFrom(EAccessModifier.Private)]
		public sealed class XuidState
		{
			public int AttemptsCompleted;

			public bool InProgress;
		}
	}
}
