using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Platform
{
	public static class PlatformUserManager
	{
		[Conditional("PLATFORM_USER_MANAGER_DEBUG")]
		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogTrace(string message)
		{
			Log.Out("[PlatformUserManager] " + message);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogInfo(string message)
		{
			Log.Out("[PlatformUserManager] " + message);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogWarning(string message)
		{
			Log.Warning("[PlatformUserManager] " + message);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogError(string message)
		{
			Log.Error("[PlatformUserManager] " + message);
		}

		public static event PlatformUserBlockedStateChangedHandler BlockedStateChanged;

		public static event PlatformUserDetailsUpdatedHandler DetailsUpdated;

		public static void Init()
		{
			PlatformUserManager.s_primaryIdToPlatform = new Dictionary<PlatformUserIdentifierAbs, PlatformUserManager.PlatformUserData>();
			PlatformUserManager.s_primaryIdToPlatformLock = new ReaderWriterLockSlim();
			PlatformUserManager.s_nativeIdToPrimaryIds = new OneToManyDictionary<PlatformUserIdentifierAbs, PlatformUserIdentifierAbs>();
			PlatformUserManager.s_nativeIdToPrimaryIdsLock = new ReaderWriterLockSlim();
			PlatformUserManager.s_nativeUserIdsSeen = new HashSet<PlatformUserIdentifierAbs>();
			PlatformUserManager.s_nativeUserIdsSeenLock = new ReaderWriterLockSlim();
			PlatformUserManager.s_lastPermissions = EUserPerms.All;
			PlatformUserManager.s_persistentPlayerListLast = null;
			PlatformUserManager.s_persistentIdsTemp = new HashSet<PlatformUserIdentifierAbs>();
			PlatformUserManager.s_persistentIdsLast = new HashSet<PlatformUserIdentifierAbs>();
			PlatformUserManager.s_blockedUsersToUpdate = new HashSet<PlatformUserManager.PlatformUserData>();
			PlatformUserManager.s_blockedUsersToUpdateLock = new ReaderWriterLockSlim();
			PlatformUserManager.s_blockedDataCurrentlyUpdating = new List<PlatformUserManager.PlatformUserBlockedResults>();
			PlatformUserManager.s_blockedDataCurrentlyUpdatingReadOnly = new ReadOnlyListWrapper<PlatformUserManager.PlatformUserBlockedResults, IPlatformUserBlockedResults>(PlatformUserManager.s_blockedDataCurrentlyUpdating);
			PlatformManager.MultiPlatform.User.UserBlocksChanged += PlatformUserManager.OnPlatformUserBlocksChanged;
			PlatformUserManager.s_userDetailsToUpdate = new HashSet<PlatformUserManager.PlatformUserData>();
			PlatformUserManager.s_userDetailsCurrentlyUpdating = new List<PlatformUserManager.PlatformUserDetailsResult>();
			PlatformUserManager.s_userDetailsToUpdateLock = new ReaderWriterLockSlim();
			PlatformUserManager.s_enabled = true;
		}

		public static void Destroy()
		{
			PlatformUserManager.s_enabled = false;
			PlatformUserManager.s_userDetailsToUpdateLock = null;
			PlatformUserManager.s_userDetailsCurrentlyUpdating = null;
			PlatformUserManager.s_userDetailsToUpdate = null;
			PlatformManager.MultiPlatform.User.UserBlocksChanged -= PlatformUserManager.OnPlatformUserBlocksChanged;
			PlatformUserManager.s_blockedDataCurrentlyUpdatingReadOnly = null;
			PlatformUserManager.s_blockedDataCurrentlyUpdating = null;
			ReaderWriterLockSlim readerWriterLockSlim = PlatformUserManager.s_blockedUsersToUpdateLock;
			if (readerWriterLockSlim != null)
			{
				readerWriterLockSlim.Dispose();
			}
			PlatformUserManager.s_blockedUsersToUpdateLock = null;
			PlatformUserManager.s_blockedUsersToUpdate = null;
			PlatformUserManager.s_persistentIdsLast = null;
			PlatformUserManager.s_persistentIdsTemp = null;
			PlatformUserManager.s_persistentPlayerListLast = null;
			PlatformUserManager.s_lastPermissions = (EUserPerms)0;
			ReaderWriterLockSlim readerWriterLockSlim2 = PlatformUserManager.s_nativeUserIdsSeenLock;
			if (readerWriterLockSlim2 != null)
			{
				readerWriterLockSlim2.Dispose();
			}
			PlatformUserManager.s_nativeUserIdsSeenLock = null;
			PlatformUserManager.s_nativeUserIdsSeen = null;
			ReaderWriterLockSlim readerWriterLockSlim3 = PlatformUserManager.s_nativeIdToPrimaryIdsLock;
			if (readerWriterLockSlim3 != null)
			{
				readerWriterLockSlim3.Dispose();
			}
			PlatformUserManager.s_nativeIdToPrimaryIdsLock = null;
			PlatformUserManager.s_nativeIdToPrimaryIds = null;
			ReaderWriterLockSlim readerWriterLockSlim4 = PlatformUserManager.s_primaryIdToPlatformLock;
			if (readerWriterLockSlim4 != null)
			{
				readerWriterLockSlim4.Dispose();
			}
			PlatformUserManager.s_primaryIdToPlatformLock = null;
			PlatformUserManager.s_primaryIdToPlatform = null;
		}

		public static void Update()
		{
			if (!PlatformUserManager.s_enabled)
			{
				return;
			}
			try
			{
				PlatformUserManager.UpdatePermissions();
				PlatformUserManager.UpdatePersistentIds();
				PlatformUserManager.UpdateUserDetails();
				PlatformUserManager.UpdateBlockedStates();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		public static IPlatformUserData GetOrCreate(PlatformUserIdentifierAbs primaryId)
		{
			if (primaryId == null)
			{
				return null;
			}
			PlatformUserManager.PlatformUserData platformUserData2;
			using (PlatformUserManager.s_primaryIdToPlatformLock.UpgradableReadLockScope())
			{
				PlatformUserManager.PlatformUserData platformUserData;
				if (PlatformUserManager.s_primaryIdToPlatform.TryGetValue(primaryId, out platformUserData))
				{
					return platformUserData;
				}
				using (PlatformUserManager.s_primaryIdToPlatformLock.WriteLockScope())
				{
					platformUserData2 = new PlatformUserManager.PlatformUserData(primaryId);
					PlatformUserManager.s_primaryIdToPlatform.Add(primaryId, platformUserData2);
				}
			}
			PlatformUserManager.OnUserAdded(primaryId, true);
			return platformUserData2;
		}

		public static bool TryGetNativePlatform(PlatformUserIdentifierAbs primaryId, out EPlatformIdentifier platform)
		{
			if (primaryId == null)
			{
				platform = EPlatformIdentifier.None;
				return false;
			}
			bool flag;
			using (PlatformUserManager.s_primaryIdToPlatformLock.ReadLockScope())
			{
				PlatformUserManager.PlatformUserData platformUserData;
				if (!PlatformUserManager.s_primaryIdToPlatform.TryGetValue(primaryId, out platformUserData))
				{
					platform = EPlatformIdentifier.None;
					flag = false;
				}
				else
				{
					PlatformUserIdentifierAbs nativeId = platformUserData.NativeId;
					if (nativeId == null)
					{
						platform = EPlatformIdentifier.None;
						flag = false;
					}
					else
					{
						platform = nativeId.PlatformIdentifier;
						flag = true;
					}
				}
			}
			return flag;
		}

		public static int TryGetByNative(PlatformUserIdentifierAbs nativeId, Span<PlatformUserIdentifierAbs> primaryIds)
		{
			if (nativeId == null)
			{
				return 0;
			}
			int num;
			using (PlatformUserManager.s_nativeIdToPrimaryIdsLock.ReadLockScope())
			{
				num = PlatformUserManager.s_nativeIdToPrimaryIds.TryGetByKey(nativeId, primaryIds);
			}
			if (num >= 3)
			{
				PlatformUserManager.LogWarning(string.Format("Expected number of values returned {0} to be less than the limit of PrimaryIds per NativeId ({1}).", num, 3));
			}
			return num;
		}

		public static IEnumerator ResolveUserBlockedCoroutine(IPlatformUserData data)
		{
			for (;;)
			{
				using (PlatformUserManager.s_blockedUsersToUpdateLock.ReadLockScope())
				{
					if (!PlatformUserManager.s_blockedUsersToUpdate.Contains((PlatformUserManager.PlatformUserData)data))
					{
						yield break;
					}
				}
				yield return null;
			}
			yield break;
		}

		public static IEnumerator ResolveUserDetailsCoroutine(IPlatformUserData data)
		{
			for (;;)
			{
				using (PlatformUserManager.s_userDetailsToUpdateLock.ReadLockScope())
				{
					if (!PlatformUserManager.s_userDetailsToUpdate.Contains((PlatformUserManager.PlatformUserData)data))
					{
						yield break;
					}
				}
				yield return null;
			}
			yield break;
		}

		public static bool AreUsersPendingResolve(IReadOnlyList<IPlatformUserData> users)
		{
			if (users == null || users.Count == 0)
			{
				return false;
			}
			using (PlatformUserManager.s_blockedUsersToUpdateLock.ReadLockScope())
			{
				for (int i = 0; i < users.Count; i++)
				{
					if (PlatformUserManager.s_blockedUsersToUpdate.Contains((PlatformUserManager.PlatformUserData)users[i]))
					{
						return true;
					}
				}
			}
			using (PlatformUserManager.s_userDetailsToUpdateLock.ReadLockScope())
			{
				for (int j = 0; j < users.Count; j++)
				{
					if (PlatformUserManager.s_userDetailsToUpdate.Contains((PlatformUserManager.PlatformUserData)users[j]))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static IEnumerator ResolveUserBlocksCoroutine(IReadOnlyList<IPlatformUserData> users)
		{
			if (users == null || users.Count == 0)
			{
				yield break;
			}
			foreach (IPlatformUserData platformUserData in users)
			{
				yield return PlatformUserManager.ResolveUserBlockedCoroutine(platformUserData);
			}
			IEnumerator<IPlatformUserData> enumerator = null;
			yield break;
			yield break;
		}

		public static IEnumerator ResolveUsersDetailsCoroutine(IReadOnlyList<IPlatformUserData> users)
		{
			if (users == null || users.Count == 0)
			{
				yield break;
			}
			foreach (IPlatformUserData platformUserData in users)
			{
				yield return PlatformUserManager.ResolveUserDetailsCoroutine(platformUserData);
			}
			IEnumerator<IPlatformUserData> enumerator = null;
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void OnUserAdded(PlatformUserIdentifierAbs userId, bool isPrimary)
		{
			if (!ThreadManager.IsMainThread())
			{
				ThreadManager.AddSingleTaskMainThread("PlatformUserManager.OnUserAdded", delegate(object _)
				{
					PlatformUserManager.OnUserAdded(userId, isPrimary);
				}, null);
				return;
			}
			PlatformManager.MultiPlatform.UserAdded(userId, isPrimary);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void OnBlockedStateChanged(PlatformUserManager.PlatformUserData userData, EBlockType type, EUserBlockState nextBlockState)
		{
			if (!ThreadManager.IsMainThread())
			{
				ThreadManager.AddSingleTaskMainThread("PlatformUserManager.OnBlockedStateChanged", delegate(object _)
				{
					PlatformUserBlockedStateChangedHandler blockedStateChanged2 = PlatformUserManager.BlockedStateChanged;
					if (blockedStateChanged2 == null)
					{
						return;
					}
					blockedStateChanged2(userData, type, nextBlockState);
				}, null);
				return;
			}
			PlatformUserBlockedStateChangedHandler blockedStateChanged = PlatformUserManager.BlockedStateChanged;
			if (blockedStateChanged == null)
			{
				return;
			}
			blockedStateChanged(userData, type, nextBlockState);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void UpdatePermissions()
		{
			if (Time.frameCount % 60 != 0)
			{
				return;
			}
			EUserPerms permissions = PermissionsManager.GetPermissions(PermissionsManager.PermissionSources.All);
			if ((PlatformUserManager.s_lastPermissions ^ permissions).HasCommunication())
			{
				PlatformUserManager.MarkBlockedStateChangedAll();
			}
			PlatformUserManager.s_lastPermissions = permissions;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void UpdatePersistentIds()
		{
			if (Time.frameCount % 300 != 0)
			{
				return;
			}
			PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
			if (persistentPlayers == null)
			{
				return;
			}
			if (PlatformUserManager.s_persistentPlayerListLast != persistentPlayers)
			{
				PlatformUserManager.s_persistentIdsLast.Clear();
				PlatformUserManager.s_persistentPlayerListLast = persistentPlayers;
			}
			ICollection<PlatformUserIdentifierAbs> players = persistentPlayers.Players.Keys;
			PlatformUserManager.s_persistentIdsLast.RemoveWhere((PlatformUserIdentifierAbs last) => !players.Contains(last));
			PlatformUserManager.s_persistentIdsTemp.Clear();
			foreach (PlatformUserIdentifierAbs platformUserIdentifierAbs in players)
			{
				if (!PlatformUserManager.s_persistentIdsLast.Contains(platformUserIdentifierAbs))
				{
					PlatformUserManager.s_persistentIdsLast.Add(platformUserIdentifierAbs);
					PlatformUserManager.s_persistentIdsTemp.Add(platformUserIdentifierAbs);
				}
			}
			foreach (PlatformUserIdentifierAbs platformUserIdentifierAbs2 in PlatformUserManager.s_persistentIdsTemp)
			{
				IPlatformUserData orCreate = PlatformUserManager.GetOrCreate(platformUserIdentifierAbs2);
				foreach (IPlatformUserBlockedData platformUserBlockedData in orCreate.Blocked.Values)
				{
					platformUserBlockedData.Locally = false;
				}
				orCreate.MarkBlockedStateChanged();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void UpdateUserDetails()
		{
			if (PlatformUserManager.s_userDetailsCurrentlyUpdating.Count > 0)
			{
				return;
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			using (PlatformUserManager.s_userDetailsToUpdateLock.UpgradableReadLockScope())
			{
				if (PlatformUserManager.s_userDetailsToUpdate.Count <= 0)
				{
					return;
				}
				foreach (PlatformUserManager.PlatformUserData platformUserData in PlatformUserManager.s_userDetailsToUpdate)
				{
					if (platformUserData.NextDetailRetryTime <= realtimeSinceStartup)
					{
						PlatformUserManager.s_userDetailsCurrentlyUpdating.Add(new PlatformUserManager.PlatformUserDetailsResult(platformUserData));
					}
				}
			}
			if (PlatformUserManager.s_userDetailsCurrentlyUpdating.Count > 0)
			{
				ThreadManager.StartCoroutine(PlatformUserManager.ResolveUserDetailsCoroutine());
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IEnumerator ResolveUserDetailsCoroutine()
		{
			try
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				if (((crossplatformPlatform != null) ? crossplatformPlatform.UserDetailsService : null) != null)
				{
					List<UserDetailsRequest> list = null;
					List<int> list2 = null;
					for (int i = 0; i < PlatformUserManager.s_userDetailsCurrentlyUpdating.Count; i++)
					{
						PlatformUserManager.PlatformUserDetailsResult platformUserDetailsResult = PlatformUserManager.s_userDetailsCurrentlyUpdating[i];
						if (platformUserDetailsResult.UserData.NativeId != null)
						{
							if (list == null)
							{
								list = new List<UserDetailsRequest>();
							}
							if (list2 == null)
							{
								list2 = new List<int>();
							}
							list.Add(new UserDetailsRequest(platformUserDetailsResult.UserData.PrimaryId, platformUserDetailsResult.UserData.NativeId.PlatformIdentifier));
							list2.Add(i);
						}
					}
					if (list != null)
					{
						yield return PlatformUserManager.<ResolveUserDetailsCoroutine>g__ResolveUserDetails|50_0(PlatformManager.CrossplatformPlatform.UserDetailsService, list, list2, PlatformUserManager.s_userDetailsCurrentlyUpdating);
					}
				}
				if (PlatformManager.NativePlatform.UserDetailsService != null)
				{
					List<UserDetailsRequest> list3 = null;
					List<int> list4 = null;
					for (int j = 0; j < PlatformUserManager.s_userDetailsCurrentlyUpdating.Count; j++)
					{
						PlatformUserManager.PlatformUserDetailsResult platformUserDetailsResult2 = PlatformUserManager.s_userDetailsCurrentlyUpdating[j];
						if (platformUserDetailsResult2.UserData.NativeId != null)
						{
							if (list3 == null)
							{
								list3 = new List<UserDetailsRequest>();
							}
							if (list4 == null)
							{
								list4 = new List<int>();
							}
							list3.Add(new UserDetailsRequest(platformUserDetailsResult2.UserData.NativeId));
							list4.Add(j);
						}
					}
					if (list3 != null)
					{
						yield return PlatformUserManager.<ResolveUserDetailsCoroutine>g__ResolveUserDetails|50_0(PlatformManager.NativePlatform.UserDetailsService, list3, list4, PlatformUserManager.s_userDetailsCurrentlyUpdating);
					}
				}
				foreach (PlatformUserManager.PlatformUserDetailsResult platformUserDetailsResult3 in PlatformUserManager.s_userDetailsCurrentlyUpdating)
				{
					if (!string.IsNullOrEmpty(platformUserDetailsResult3.Name))
					{
						platformUserDetailsResult3.UserData.Name = platformUserDetailsResult3.Name;
						PlatformUserDetailsUpdatedHandler detailsUpdated = PlatformUserManager.DetailsUpdated;
						if (detailsUpdated != null)
						{
							detailsUpdated(platformUserDetailsResult3.UserData, platformUserDetailsResult3.Name);
						}
					}
				}
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				using (PlatformUserManager.s_userDetailsToUpdateLock.WriteLockScope())
				{
					foreach (PlatformUserManager.PlatformUserDetailsResult platformUserDetailsResult4 in PlatformUserManager.s_userDetailsCurrentlyUpdating)
					{
						PlatformUserManager.PlatformUserData userData = platformUserDetailsResult4.UserData;
						if (!string.IsNullOrEmpty(platformUserDetailsResult4.Name))
						{
							userData.DetailAttempts = 0;
							userData.NextDetailRetryTime = 0f;
							PlatformUserManager.s_userDetailsToUpdate.Remove(userData);
						}
						else
						{
							userData.DetailAttempts++;
							if (userData.DetailAttempts >= 5)
							{
								PlatformUserManager.LogWarning(string.Format("[PLATFORM] User details lookup gave up after {0} attempts.", userData.DetailAttempts));
								userData.DetailAttempts = 0;
								userData.NextDetailRetryTime = 0f;
								PlatformUserManager.s_userDetailsToUpdate.Remove(userData);
							}
							else
							{
								float num = 0.5f * (float)(1 << userData.DetailAttempts - 1);
								if (num > 30f)
								{
									num = 30f;
								}
								userData.NextDetailRetryTime = realtimeSinceStartup + num;
							}
						}
					}
				}
			}
			finally
			{
				PlatformUserManager.s_userDetailsCurrentlyUpdating.Clear();
			}
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void UpdateBlockedStates()
		{
			if (PlatformUserManager.s_blockedDataCurrentlyUpdating.Count > 0 || PlatformUserManager.s_blockedUsersToUpdate.Count <= 0)
			{
				return;
			}
			PlatformUserManager.PlatformUserData platformUserData;
			bool flag;
			using (PlatformUserManager.s_primaryIdToPlatformLock.ReadLockScope())
			{
				flag = PlatformUserManager.s_primaryIdToPlatform.TryGetValue(PlatformManager.MultiPlatform.User.PlatformUserId, out platformUserData);
			}
			using (PlatformUserManager.s_blockedUsersToUpdateLock.UpgradableReadLockScope())
			{
				if (flag)
				{
					using (PlatformUserManager.s_blockedUsersToUpdateLock.WriteLockScope())
					{
						PlatformUserManager.s_blockedUsersToUpdate.Remove(platformUserData);
					}
				}
				foreach (PlatformUserManager.PlatformUserData platformUserData2 in PlatformUserManager.s_blockedUsersToUpdate)
				{
					PlatformUserManager.s_blockedDataCurrentlyUpdating.Add(new PlatformUserManager.PlatformUserBlockedResults(platformUserData2));
				}
			}
			if (PlatformUserManager.s_blockedDataCurrentlyUpdating.Count > 0)
			{
				ThreadManager.StartCoroutine(PlatformUserManager.UpdateBlockedStatesCoroutine());
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IEnumerator UpdateBlockedStatesCoroutine()
		{
			try
			{
				yield return PlatformManager.MultiPlatform.User.ResolveUserBlocks(PlatformUserManager.s_blockedDataCurrentlyUpdatingReadOnly);
				if (BlockedPlayerList.Instance != null)
				{
					yield return PlatformUserManager.ResolveUserBlocksFromBlockList(PlatformUserManager.s_blockedDataCurrentlyUpdatingReadOnly);
				}
				foreach (PlatformUserManager.PlatformUserBlockedResults platformUserBlockedResults in PlatformUserManager.s_blockedDataCurrentlyUpdating)
				{
					if (!platformUserBlockedResults.HasErrored)
					{
						foreach (EBlockType eblockType in EnumUtils.Values<EBlockType>())
						{
							platformUserBlockedResults.User.Blocked[eblockType].RefreshBlockedState(platformUserBlockedResults.IsBlocked[eblockType]);
						}
					}
				}
				using (PlatformUserManager.s_blockedUsersToUpdateLock.WriteLockScope())
				{
					foreach (PlatformUserManager.PlatformUserBlockedResults platformUserBlockedResults2 in PlatformUserManager.s_blockedDataCurrentlyUpdating)
					{
						PlatformUserManager.s_blockedUsersToUpdate.Remove(platformUserBlockedResults2.User);
					}
				}
			}
			finally
			{
				PlatformUserManager.s_blockedDataCurrentlyUpdating.Clear();
			}
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void MarkBlockedStateChangedAll()
		{
			using (PlatformUserManager.s_primaryIdToPlatformLock.ReadLockScope())
			{
				foreach (PlatformUserManager.PlatformUserData platformUserData in PlatformUserManager.s_primaryIdToPlatform.Values)
				{
					platformUserData.MarkBlockedStateChanged();
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void OnPlatformUserBlocksChanged(IReadOnlyCollection<PlatformUserIdentifierAbs> userIds)
		{
			if (userIds == null)
			{
				PlatformUserManager.MarkBlockedStateChangedAll();
				return;
			}
			using (PlatformUserManager.s_primaryIdToPlatformLock.ReadLockScope())
			{
				foreach (PlatformUserIdentifierAbs platformUserIdentifierAbs in userIds)
				{
					PlatformUserManager.PlatformUserData platformUserData;
					if (PlatformUserManager.s_primaryIdToPlatform.TryGetValue(platformUserIdentifierAbs, out platformUserData))
					{
						platformUserData.MarkBlockedStateChanged();
					}
				}
			}
			using (PlatformUserManager.s_nativeIdToPrimaryIdsLock.ReadLockScope())
			{
				foreach (PlatformUserIdentifierAbs platformUserIdentifierAbs2 in userIds)
				{
					IReadOnlyCollection<PlatformUserIdentifierAbs> readOnlyCollection;
					if (PlatformUserManager.s_nativeIdToPrimaryIds.TryGetByKey(platformUserIdentifierAbs2, out readOnlyCollection))
					{
						using (PlatformUserManager.s_primaryIdToPlatformLock.ReadLockScope())
						{
							foreach (PlatformUserIdentifierAbs platformUserIdentifierAbs3 in readOnlyCollection)
							{
								PlatformUserManager.s_primaryIdToPlatform[platformUserIdentifierAbs3].MarkBlockedStateChanged();
							}
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool CanCheckUserDetails()
		{
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			return ((crossplatformPlatform != null) ? crossplatformPlatform.UserDetailsService : null) != null || PlatformManager.NativePlatform.UserDetailsService != null;
		}

		public static IEnumerator ResolveUserBlocksFromBlockList(IReadOnlyList<IPlatformUserBlockedResults> _results)
		{
			if (BlockedPlayerList.Instance == null)
			{
				yield break;
			}
			while (BlockedPlayerList.Instance.PendingResolve())
			{
				yield return null;
			}
			using (IEnumerator<BlockedPlayerList.ListEntry> enumerator = BlockedPlayerList.Instance.GetEntriesOrdered(true, false).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BlockedPlayerList.ListEntry listEntry = enumerator.Current;
					PlatformUserIdentifierAbs primaryId = listEntry.PlayerData.PrimaryId;
					foreach (IPlatformUserBlockedResults platformUserBlockedResults in _results)
					{
						if (platformUserBlockedResults.User.PrimaryId.Equals(primaryId))
						{
							platformUserBlockedResults.BlockAll();
							break;
						}
					}
				}
				yield break;
			}
			yield break;
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static IEnumerator <ResolveUserDetailsCoroutine>g__ResolveUserDetails|50_0(IUserDetailsService service, IReadOnlyList<UserDetailsRequest> requests, IReadOnlyList<int> resultsIndices, List<PlatformUserManager.PlatformUserDetailsResult> results)
		{
			PlatformUserManager.<>c__DisplayClass50_0 CS$<>8__locals1 = new PlatformUserManager.<>c__DisplayClass50_0();
			CS$<>8__locals1.resultsIndices = resultsIndices;
			CS$<>8__locals1.results = results;
			CS$<>8__locals1.requests = requests;
			CS$<>8__locals1.inProgress = true;
			service.RequestUserDetailsUpdate(CS$<>8__locals1.requests, new UserDetailsRequestCompleteHandler(CS$<>8__locals1.<ResolveUserDetailsCoroutine>g__OnComplete|1));
			while (CS$<>8__locals1.inProgress)
			{
				yield return true;
			}
			yield break;
		}

		public const int PrimaryIdsPerNativeIdLimit = 3;

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool s_enabled;

		[PublicizedFrom(EAccessModifier.Private)]
		public static Dictionary<PlatformUserIdentifierAbs, PlatformUserManager.PlatformUserData> s_primaryIdToPlatform;

		[PublicizedFrom(EAccessModifier.Private)]
		public static ReaderWriterLockSlim s_primaryIdToPlatformLock;

		[PublicizedFrom(EAccessModifier.Private)]
		public static OneToManyDictionary<PlatformUserIdentifierAbs, PlatformUserIdentifierAbs> s_nativeIdToPrimaryIds;

		[PublicizedFrom(EAccessModifier.Private)]
		public static ReaderWriterLockSlim s_nativeIdToPrimaryIdsLock;

		[PublicizedFrom(EAccessModifier.Private)]
		public static HashSet<PlatformUserIdentifierAbs> s_nativeUserIdsSeen;

		[PublicizedFrom(EAccessModifier.Private)]
		public static ReaderWriterLockSlim s_nativeUserIdsSeenLock;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int PermissionFrameFrequency = 60;

		[PublicizedFrom(EAccessModifier.Private)]
		public static EUserPerms s_lastPermissions;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int PersistentFrameFrequency = 300;

		[PublicizedFrom(EAccessModifier.Private)]
		public static PersistentPlayerList s_persistentPlayerListLast;

		[PublicizedFrom(EAccessModifier.Private)]
		public static HashSet<PlatformUserIdentifierAbs> s_persistentIdsTemp;

		[PublicizedFrom(EAccessModifier.Private)]
		public static HashSet<PlatformUserIdentifierAbs> s_persistentIdsLast;

		[PublicizedFrom(EAccessModifier.Private)]
		public static HashSet<PlatformUserManager.PlatformUserData> s_blockedUsersToUpdate;

		[PublicizedFrom(EAccessModifier.Private)]
		public static ReaderWriterLockSlim s_blockedUsersToUpdateLock;

		[PublicizedFrom(EAccessModifier.Private)]
		public static List<PlatformUserManager.PlatformUserBlockedResults> s_blockedDataCurrentlyUpdating;

		[PublicizedFrom(EAccessModifier.Private)]
		public static IReadOnlyList<IPlatformUserBlockedResults> s_blockedDataCurrentlyUpdatingReadOnly;

		[PublicizedFrom(EAccessModifier.Private)]
		public static HashSet<PlatformUserManager.PlatformUserData> s_userDetailsToUpdate;

		[PublicizedFrom(EAccessModifier.Private)]
		public static List<PlatformUserManager.PlatformUserDetailsResult> s_userDetailsCurrentlyUpdating;

		[PublicizedFrom(EAccessModifier.Private)]
		public static ReaderWriterLockSlim s_userDetailsToUpdateLock;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int UserDetailsMaxAttempts = 5;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float UserDetailsRetryBaseDelay = 0.5f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float UserDetailsRetryMaxDelay = 30f;

		[PublicizedFrom(EAccessModifier.Private)]
		public sealed class PlatformUserData : IPlatformUserData, IPlatformUser
		{
			public PlatformUserData(PlatformUserIdentifierAbs primaryId)
			{
				this.PrimaryId = primaryId;
				this.m_userBlockedStates = new EnumDictionary<EBlockType, PlatformUserManager.PlatformUserBlockedData>();
				this.m_userBlockedStatesReadOnly = new ReadOnlyDictionaryWrapper<EBlockType, PlatformUserManager.PlatformUserBlockedData, IPlatformUserBlockedData>(this.m_userBlockedStates);
				foreach (EBlockType eblockType in EnumUtils.Values<EBlockType>())
				{
					this.m_userBlockedStates[eblockType] = new PlatformUserManager.PlatformUserBlockedData(this, eblockType);
				}
				this.RequestUserDetailsUpdate();
			}

			public override string ToString()
			{
				string text = "{0}[PrimaryId={1}, NativeId={2}, Name={3}, {4}]";
				object[] array = new object[5];
				array[0] = "PlatformUserData";
				array[1] = this.PrimaryId;
				array[2] = this.NativeId;
				array[3] = this.Name;
				array[4] = string.Join(", ", this.Blocked.Select((KeyValuePair<EBlockType, PlatformUserManager.PlatformUserBlockedData> kv) => string.Format("Blocked[{0}]={1}", kv.Key, kv.Value)));
				return string.Format(text, array);
			}

			public PlatformUserIdentifierAbs PrimaryId { get; }

			public PlatformUserIdentifierAbs NativeId
			{
				get
				{
					PlatformUserIdentifierAbs platformUserIdentifierAbs;
					using (PlatformUserManager.s_nativeIdToPrimaryIdsLock.ReadLockScope())
					{
						PlatformUserIdentifierAbs platformUserIdentifierAbs2;
						platformUserIdentifierAbs = (PlatformUserManager.s_nativeIdToPrimaryIds.TryGetByValue(this.PrimaryId, out platformUserIdentifierAbs2) ? platformUserIdentifierAbs2 : null);
					}
					return platformUserIdentifierAbs;
				}
				set
				{
					if (value == null)
					{
						return;
					}
					using (PlatformUserManager.s_nativeIdToPrimaryIdsLock.UpgradableReadLockScope())
					{
						PlatformUserIdentifierAbs platformUserIdentifierAbs;
						if (PlatformUserManager.s_nativeIdToPrimaryIds.TryGetByValue(this.PrimaryId, out platformUserIdentifierAbs))
						{
							if (platformUserIdentifierAbs.Equals(value))
							{
								return;
							}
							using (PlatformUserManager.s_nativeIdToPrimaryIdsLock.WriteLockScope())
							{
								PlatformUserManager.s_nativeIdToPrimaryIds.RemoveByValue(this.PrimaryId);
								PlatformUserManager.s_nativeIdToPrimaryIds.Add(value, this.PrimaryId);
								PlatformUserManager.LogError(string.Format("Primary ID '{0}' was be remapped from Native ID '{1}' to Native ID '{2}'.", this.PrimaryId, platformUserIdentifierAbs, value));
								goto IL_00BF;
							}
						}
						using (PlatformUserManager.s_nativeIdToPrimaryIdsLock.WriteLockScope())
						{
							PlatformUserManager.s_nativeIdToPrimaryIds.Add(value, this.PrimaryId);
						}
					}
					IL_00BF:
					bool flag;
					using (PlatformUserManager.s_nativeUserIdsSeenLock.UpgradableReadLockScope())
					{
						if (PlatformUserManager.s_nativeUserIdsSeen.Contains(value))
						{
							flag = false;
						}
						else
						{
							using (PlatformUserManager.s_nativeUserIdsSeenLock.WriteLockScope())
							{
								flag = PlatformUserManager.s_nativeUserIdsSeen.Add(value);
							}
						}
					}
					if (flag)
					{
						PlatformUserManager.OnUserAdded(value, false);
						this.RequestUserDetailsUpdate();
					}
				}
			}

			public string Name { get; set; }

			public void RequestUserDetailsUpdate()
			{
				if (!PlatformUserManager.CanCheckUserDetails())
				{
					return;
				}
				using (PlatformUserManager.s_userDetailsToUpdateLock.WriteLockScope())
				{
					this.DetailAttempts = 0;
					this.NextDetailRetryTime = 0f;
					PlatformUserManager.s_userDetailsToUpdate.Add(this);
				}
			}

			public IReadOnlyDictionary<EBlockType, PlatformUserManager.PlatformUserBlockedData> Blocked
			{
				get
				{
					return this.m_userBlockedStates;
				}
			}

			public IReadOnlyDictionary<EBlockType, IPlatformUserBlockedData> Platform.IPlatformUserData.Blocked
			{
				[PublicizedFrom(EAccessModifier.Private)]
				get
				{
					return this.m_userBlockedStatesReadOnly;
				}
			}

			public void MarkBlockedStateChanged()
			{
				if (GameManager.IsDedicatedServer)
				{
					return;
				}
				using (PlatformUserManager.s_blockedUsersToUpdateLock.UpgradableReadLockScope())
				{
					if (!PlatformUserManager.s_blockedUsersToUpdate.Contains(this))
					{
						using (PlatformUserManager.s_blockedUsersToUpdateLock.WriteLockScope())
						{
							PlatformUserManager.s_blockedUsersToUpdate.Add(this);
						}
					}
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly EnumDictionary<EBlockType, PlatformUserManager.PlatformUserBlockedData> m_userBlockedStates;

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly IReadOnlyDictionary<EBlockType, IPlatformUserBlockedData> m_userBlockedStatesReadOnly;

			public int DetailAttempts;

			public float NextDetailRetryTime;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public sealed class PlatformUserBlockedData : IPlatformUserBlockedData
		{
			public PlatformUserBlockedData(PlatformUserManager.PlatformUserData userData, EBlockType blockType)
			{
				this.m_userData = userData;
				this.Type = blockType;
				this.m_blockedLocally = false;
				this.State = EUserBlockState.NotBlocked;
			}

			public override string ToString()
			{
				return string.Format("{0}[Type={1}, State={2}, Locally={3}]", new object[] { "PlatformUserBlockedData", this.Type, this.State, this.Locally });
			}

			public EBlockType Type { get; }

			public EUserBlockState State
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			public bool Locally
			{
				get
				{
					return this.m_blockedLocally;
				}
				set
				{
					this.m_blockedLocally = value;
					this.RefreshBlockedState(this.State == EUserBlockState.ByPlatform);
				}
			}

			public void RefreshBlockedState(bool isBlockedByPlatform)
			{
				EUserBlockState state = this.State;
				EUserBlockState euserBlockState;
				if (isBlockedByPlatform)
				{
					euserBlockState = EUserBlockState.ByPlatform;
				}
				else if (this.Locally)
				{
					euserBlockState = EUserBlockState.InGame;
				}
				else
				{
					euserBlockState = EUserBlockState.NotBlocked;
				}
				if (euserBlockState == EUserBlockState.ByPlatform)
				{
					this.m_blockedLocally = false;
				}
				this.State = euserBlockState;
				if (state != euserBlockState)
				{
					PlatformUserManager.OnBlockedStateChanged(this.m_userData, this.Type, euserBlockState);
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly PlatformUserManager.PlatformUserData m_userData;

			[PublicizedFrom(EAccessModifier.Private)]
			public bool m_blockedLocally;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public sealed class PlatformUserBlockedResults : IPlatformUserBlockedResults
		{
			public PlatformUserBlockedResults(PlatformUserManager.PlatformUserData userData)
			{
				this.m_userData = userData;
				this.IsBlocked = new EnumDictionary<EBlockType, bool>();
				foreach (EBlockType eblockType in EnumUtils.Values<EBlockType>())
				{
					this.IsBlocked[eblockType] = false;
				}
				this.HasErrored = false;
			}

			public override string ToString()
			{
				string text = "{0}[{1}, HasErrored={2}, {3}.{4}={5}, {6}.{7}={8}]";
				object[] array = new object[9];
				array[0] = "PlatformUserBlockedResults";
				array[1] = string.Join(", ", this.IsBlocked.Select((KeyValuePair<EBlockType, bool> kv) => string.Format("IsBlocked[{0}]={1}", kv.Key, kv.Value)));
				array[2] = this.HasErrored;
				array[3] = "User";
				array[4] = "PrimaryId";
				array[5] = this.User.PrimaryId;
				array[6] = "User";
				array[7] = "NativeId";
				array[8] = this.User.NativeId;
				return string.Format(text, array);
			}

			public PlatformUserManager.PlatformUserData User
			{
				get
				{
					return this.m_userData;
				}
			}

			public EnumDictionary<EBlockType, bool> IsBlocked { get; }

			public bool HasErrored
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			public IPlatformUser Platform.IPlatformUserBlockedResults.User
			{
				[PublicizedFrom(EAccessModifier.Private)]
				get
				{
					return this.m_userData;
				}
			}

			public void Block(EBlockType blockType)
			{
				this.IsBlocked[blockType] = true;
			}

			public void Error()
			{
				this.HasErrored = true;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly PlatformUserManager.PlatformUserData m_userData;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public sealed class PlatformUserDetailsResult
		{
			public PlatformUserDetailsResult(PlatformUserManager.PlatformUserData userData)
			{
				this.UserData = userData;
			}

			public readonly PlatformUserManager.PlatformUserData UserData;

			public string Name;
		}
	}
}
