using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Platform
{
	public class BlockedPlayerList : IRemotePlayerStorageObject
	{
		public static BlockedPlayerList Instance
		{
			get
			{
				if (BlockedPlayerList.instance == null)
				{
					IPlatform multiPlatform = PlatformManager.MultiPlatform;
					if (((multiPlatform != null) ? multiPlatform.RemotePlayerFileStorage : null) != null)
					{
						BlockedPlayerList.instance = new BlockedPlayerList();
						PlayerInteractions.Instance.OnNewPlayerInteraction += BlockedPlayerList.instance.OnPlayerInteraction;
					}
				}
				return BlockedPlayerList.instance;
			}
		}

		public void Update()
		{
			IPlatform multiPlatform = PlatformManager.MultiPlatform;
			if (((multiPlatform != null) ? multiPlatform.RemotePlayerFileStorage : null) == null)
			{
				return;
			}
			if ((this.writeRequestTime != null && DateTime.UtcNow - this.writeRequestTime >= BlockedPlayerList.WriteRequestDelay) || DateTime.UtcNow - this.lastWriteTime >= BlockedPlayerList.WriteThreshold)
			{
				this.WriteToStorage();
				this.lastWriteTime = DateTime.UtcNow;
				this.writeRequestTime = null;
			}
		}

		public void UpdatePlayersSeenInWorld(World _world)
		{
			if (((_world != null) ? _world.Players : null) == null)
			{
				return;
			}
			foreach (EntityPlayer entityPlayer in _world.Players.list)
			{
				PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(entityPlayer.entityId);
				this.AddOrUpdatePlayer(playerDataFromEntityID.PlayerData, DateTime.UtcNow, null, false);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public BlockedPlayerList.ListEntry AddOrUpdatePlayer(PlayerData _playerData, DateTime _timeStamp, bool? _blocked = null, bool _ignoreLimit = false)
		{
			if (_playerData == null || _playerData.PrimaryId.Equals(PlatformManager.MultiPlatform.User.PlatformUserId))
			{
				return null;
			}
			DateTime dateTime = DateTime.UtcNow.AddHours(-168.0);
			bool? flag = _blocked;
			bool flag2 = false;
			if (((flag.GetValueOrDefault() == flag2) & (flag != null)) && dateTime >= _timeStamp)
			{
				return null;
			}
			object obj = this.bplLock;
			BlockedPlayerList.ListEntry listEntry2;
			lock (obj)
			{
				BlockedPlayerList.ListEntry valueOrDefault = this.playerStates.dict.GetValueOrDefault(_playerData.PrimaryId);
				if (!_ignoreLimit)
				{
					flag = _blocked;
					bool flag3 = true;
					if (((flag.GetValueOrDefault() == flag3) & (flag != null)) && (valueOrDefault == null || !valueOrDefault.Blocked) && this.EntryCount(true, false) >= 500)
					{
						return null;
					}
				}
				BlockedPlayerList.ListEntry listEntry;
				if (_blocked == null && valueOrDefault != null)
				{
					listEntry = new BlockedPlayerList.ListEntry(_playerData, _timeStamp, valueOrDefault.Blocked);
				}
				else
				{
					listEntry = new BlockedPlayerList.ListEntry(_playerData, _timeStamp, _blocked.GetValueOrDefault());
				}
				this.playerStates.Set(_playerData.PrimaryId, listEntry);
				listEntry2 = listEntry;
			}
			return listEntry2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnPlayerInteraction(PlayerInteraction _interaction)
		{
			BlockedPlayerList.ListEntry listEntry = this.AddOrUpdatePlayer(_interaction.PlayerData, DateTime.UtcNow, null, false);
			if (listEntry != null)
			{
				this.MarkForWrite();
				listEntry.SetResolvedOnce();
			}
		}

		public int EntryCount(bool _blocked, bool _resolveRequired)
		{
			return this.playerStates.list.Count((BlockedPlayerList.ListEntry entry) => entry.Blocked == _blocked && (!_resolveRequired || entry.ResolvedOnce));
		}

		public IEnumerable<BlockedPlayerList.ListEntry> GetEntriesOrdered(bool _blocked, bool _resolveRequired)
		{
			object obj = this.bplLock;
			lock (obj)
			{
				this.SortPlayerStates();
				int num;
				for (int i = 0; i < this.playerStates.list.Count; i = num + 1)
				{
					BlockedPlayerList.ListEntry listEntry = this.playerStates.list[i];
					if (listEntry.Blocked == _blocked && (!_resolveRequired || listEntry.ResolvedOnce))
					{
						yield return listEntry;
					}
					num = i;
				}
			}
			obj = null;
			yield break;
			yield break;
		}

		public BlockedPlayerList.ListEntry GetPlayerStateInfo(PlatformUserIdentifierAbs _primaryId)
		{
			object obj = this.bplLock;
			BlockedPlayerList.ListEntry listEntry2;
			lock (obj)
			{
				BlockedPlayerList.ListEntry listEntry;
				if (this.playerStates.dict.TryGetValue(_primaryId, out listEntry))
				{
					listEntry2 = listEntry;
				}
				else
				{
					listEntry2 = null;
				}
			}
			return listEntry2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SortPlayerStates()
		{
			this.playerStates.list.Sort((BlockedPlayerList.ListEntry p1, BlockedPlayerList.ListEntry p2) => p2.LastSeen.CompareTo(p1.LastSeen));
		}

		public IEnumerator ReadStorageAndResolve()
		{
			BlockedPlayerList.<>c__DisplayClass25_0 CS$<>8__locals1 = new BlockedPlayerList.<>c__DisplayClass25_0();
			CS$<>8__locals1.<>4__this = this;
			this.readStorageState = ERoutineState.Running;
			object obj = this.bplLock;
			lock (obj)
			{
				BlockedPlayerList blockedPlayerList = IRemotePlayerFileStorage.ReadCachedObject<BlockedPlayerList>(PlatformManager.MultiPlatform.User, "BlockedPlayerList");
				if (blockedPlayerList != null)
				{
					this.playerStates = blockedPlayerList.playerStates;
				}
			}
			CS$<>8__locals1.callbackComplete = false;
			IRemotePlayerFileStorage remotePlayerFileStorage = PlatformManager.MultiPlatform.RemotePlayerFileStorage;
			if (remotePlayerFileStorage != null)
			{
				remotePlayerFileStorage.ReadRemoteObject<BlockedPlayerList>("BlockedPlayerList", true, new IRemotePlayerFileStorage.FileReadObjectCompleteCallback<BlockedPlayerList>(CS$<>8__locals1.<ReadStorageAndResolve>g__ReadRPFSCallback|0));
				while (!CS$<>8__locals1.callbackComplete)
				{
					yield return null;
				}
			}
			if (this.playerStates.Count > 0)
			{
				yield return this.ResolveUserDetails();
			}
			this.readStorageState = ERoutineState.Succeeded;
			yield break;
		}

		public void ReadInto(BinaryReader _reader)
		{
			object obj = this.bplLock;
			lock (obj)
			{
				this.playerStates.Clear();
				_reader.ReadInt32();
				int num = _reader.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					BlockedPlayerList.ListEntry listEntry = BlockedPlayerList.ListEntry.Read(_reader);
					this.AddOrUpdatePlayer(listEntry.PlayerData, listEntry.LastSeen, new bool?(listEntry.Blocked), false);
				}
			}
		}

		public void MarkForWrite()
		{
			if (this.writeRequestTime == null)
			{
				this.writeRequestTime = new DateTime?(DateTime.UtcNow);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void WriteToStorage()
		{
			if (this.writeToStorageState == ERoutineState.Running)
			{
				Log.Warning("[BlockedPlayerList] Tried to write to storage while another write is already in progress.");
				return;
			}
			if (this.readStorageResult != IRemotePlayerFileStorage.CallbackResult.Success && this.readStorageResult != IRemotePlayerFileStorage.CallbackResult.MalformedData && this.readStorageResult != IRemotePlayerFileStorage.CallbackResult.FileNotFound)
			{
				Log.Out("[BlockedPlayerList] Error when processing remote list. Saving to local cache only.");
				if (!IRemotePlayerFileStorage.WriteCachedObject(PlatformManager.MultiPlatform.User, "BlockedPlayerList", this))
				{
					Log.Warning("[BlockedPlayerList] Failed to write to local cache.");
				}
				return;
			}
			if (this.readStorageResult == IRemotePlayerFileStorage.CallbackResult.MalformedData)
			{
				Log.Out("[BlockedPlayerList] Previous remote list was malformed so it will be overwritten.");
			}
			this.writeToStorageState = ERoutineState.Running;
			PlatformManager.MultiPlatform.RemotePlayerFileStorage.WriteRemoteObject("BlockedPlayerList", this, true, new IRemotePlayerFileStorage.FileWriteCompleteCallback(this.WriteRPFSCallback));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void WriteRPFSCallback(IRemotePlayerFileStorage.CallbackResult _result)
		{
			this.writeToStorageState = ERoutineState.NotStarted;
			if (_result != IRemotePlayerFileStorage.CallbackResult.Success)
			{
				Log.Warning("[BlockedPlayerList] Recent Player List failed to write to remote storage.");
			}
		}

		public void WriteFrom(BinaryWriter _writer)
		{
			object obj = this.bplLock;
			lock (obj)
			{
				_writer.Write(1);
				this.SortPlayerStates();
				int num = this.EntryCount(true, false);
				int num2 = Math.Min(this.EntryCount(false, false), 100);
				_writer.Write(num + num2);
				for (int i = 0; i < num + num2; i++)
				{
					this.playerStates.list[i].Write(_writer);
				}
			}
		}

		public bool PendingResolve()
		{
			return this.readStorageState != ERoutineState.Succeeded || this.resolveState == ERoutineState.Running;
		}

		public IEnumerator ResolveUserDetails()
		{
			while (this.resolveState == ERoutineState.Running)
			{
				yield return null;
			}
			try
			{
				this.resolveState = ERoutineState.Running;
				List<IPlatformUserData> dataList = new List<IPlatformUserData>();
				object obj = this.bplLock;
				lock (obj)
				{
					foreach (BlockedPlayerList.ListEntry listEntry in this.playerStates.list)
					{
						listEntry.PlayerData.PlatformData.RequestUserDetailsUpdate();
						dataList.Add(listEntry.PlayerData.PlatformData);
					}
				}
				yield return PlatformUserManager.ResolveUsersDetailsCoroutine(dataList);
				foreach (IPlatformUserData platformUserData in dataList)
				{
					AuthoredText playerName = this.playerStates.dict[platformUserData.PrimaryId].PlayerData.PlayerName;
					if (platformUserData.Name != null && platformUserData.Name != playerName.Text)
					{
						playerName.Update(platformUserData.Name, playerName.Author);
						GeneratedTextManager.PrefilterText(playerName, GeneratedTextManager.TextFilteringMode.Filter);
					}
					this.playerStates.dict[platformUserData.PrimaryId].SetResolvedOnce();
				}
				this.resolveState = ERoutineState.Succeeded;
				dataList = null;
			}
			finally
			{
				if (this.resolveState != ERoutineState.Succeeded)
				{
					this.resolveState = ERoutineState.Failed;
				}
			}
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static BlockedPlayerList instance;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly TimeSpan WriteThreshold = TimeSpan.FromMinutes(10.0);

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly TimeSpan WriteRequestDelay = TimeSpan.FromSeconds(5.0);

		public const int MaxBlockedPlayerEntries = 500;

		public const int MaxRecentPlayerEntries = 100;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int Version = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public const string FilePath = "BlockedPlayerList";

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TimeoutHours = 168;

		[PublicizedFrom(EAccessModifier.Private)]
		public object bplLock = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public DictionaryList<PlatformUserIdentifierAbs, BlockedPlayerList.ListEntry> playerStates = new DictionaryList<PlatformUserIdentifierAbs, BlockedPlayerList.ListEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime lastWriteTime = DateTime.UtcNow;

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime? writeRequestTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public ERoutineState readStorageState;

		[PublicizedFrom(EAccessModifier.Private)]
		public IRemotePlayerFileStorage.CallbackResult readStorageResult = IRemotePlayerFileStorage.CallbackResult.Other;

		[PublicizedFrom(EAccessModifier.Private)]
		public ERoutineState writeToStorageState;

		[PublicizedFrom(EAccessModifier.Private)]
		public ERoutineState resolveState;

		public class ListEntry
		{
			public bool ResolvedOnce
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			public bool Blocked
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			public ListEntry(PlayerData _playerData, DateTime _lastSeen, bool _blockState)
			{
				this.PlayerData = _playerData;
				this.LastSeen = _lastSeen;
				this.Blocked = _blockState;
			}

			public static BlockedPlayerList.ListEntry Read(BinaryReader _reader)
			{
				PlayerData playerData = PlayerData.Read(_reader);
				DateTime utcDateTime = DateTimeOffset.FromUnixTimeSeconds(_reader.ReadInt64()).UtcDateTime;
				bool flag = _reader.ReadBoolean();
				return new BlockedPlayerList.ListEntry(playerData, utcDateTime, flag);
			}

			public void Write(BinaryWriter _writer)
			{
				this.PlayerData.Write(_writer);
				long num = new DateTimeOffset(this.LastSeen).ToUnixTimeSeconds();
				_writer.Write(num);
				_writer.Write(this.Blocked);
			}

			public void SetResolvedOnce()
			{
				this.ResolvedOnce = true;
			}

			public ValueTuple<bool, string> SetBlockState(bool _blockState)
			{
				if (this.Blocked == _blockState)
				{
					return new ValueTuple<bool, string>(false, null);
				}
				if (PlatformManager.NativePlatform.User.CanShowProfile(this.PlayerData.NativeId))
				{
					this.Blocked = false;
					Log.Warning(string.Format("[BlockedPlayerList] Cannot change block state of native user {0} through the block list", this.PlayerData.NativeId));
					return new ValueTuple<bool, string>(false, null);
				}
				if (_blockState && BlockedPlayerList.Instance.EntryCount(true, false) >= 500)
				{
					return new ValueTuple<bool, string>(false, Localization.Get("xuiBlockedPlayersCantAddMessage", false, null));
				}
				this.PlayerData.PlatformData.MarkBlockedStateChanged();
				BlockedPlayerList.Instance.MarkForWrite();
				this.Blocked = _blockState;
				return new ValueTuple<bool, string>(true, null);
			}

			public readonly PlayerData PlayerData;

			public readonly DateTime LastSeen;
		}
	}
}
