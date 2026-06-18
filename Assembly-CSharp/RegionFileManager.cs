using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Profiling;
using UnityEngine;

public class RegionFileManager : WorldChunkCache
{
	public long MaxBytes
	{
		get
		{
			return this.maxBytes;
		}
	}

	public SaveDataSlot SaveDataSlot { get; }

	public IReadOnlyCollection<LongSetGroups.Group> ChunkGroups
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public RegionFileManager(string _loadDirectory, string _saveDirectory, int _maxChunksInCache, bool _bSaveOnChunkDrop)
	{
		RegionFileManager.platformFactory = RegionFilePlatform.CreateFactory();
		SaveDataUtils.SaveDataManager.RegisterRegionFileManager(this);
		Vector2i worldSize = GameManager.Instance.World.ChunkCache.ChunkProvider.GetWorldSize();
		int num = worldSize.x / 16 * (worldSize.y / 16);
		this.chunksInSaveDir = new Dictionary<long, uint>(num);
		this.bSaveOnChunkDrop = _bSaveOnChunkDrop;
		this.RebuildChunkGroupsFromPOIs();
		this.ChunkGroups = this.chunkGroups.Groups;
		this.loadDirectory = ((_loadDirectory != null) ? Path.GetFullPath(_loadDirectory) : null);
		this.saveDirectory = ((_saveDirectory != null) ? Path.GetFullPath(_saveDirectory) : null);
		SaveDataManagedPath saveDataManagedPath;
		if (SaveDataUtils.TryGetManagedPath(this.saveDirectory, out saveDataManagedPath))
		{
			this.SaveDataSlot = saveDataManagedPath.Slot;
			Debug.Log(string.Format("[RegionFileManager] SaveDataSlot set to: {0}", saveDataManagedPath.Slot));
		}
		this.regionFileAccess = RegionFileManager.platformFactory.CreateRegionFileAccess();
		this.regionFileAccess.ReadDirectory(this.loadDirectory, delegate(long chunkKey, string ext, uint timeStamp)
		{
			this.chunksInLoadDir.Add(chunkKey);
		});
		this.regionFileAccess.ReadDirectory(this.saveDirectory, delegate(long chunkKey, string ext, uint timeStamp)
		{
			this.SetChunkTimestamp(chunkKey, timeStamp);
		});
		this.maxChunksInCache = _maxChunksInCache;
		this.snapshotUtil = RegionFileManager.platformFactory.CreateSnapshotUtil(this.regionFileAccess);
		this.OnGamePrefChanged(EnumGamePrefs.MaxChunkAge);
		this.OnGamePrefChanged(EnumGamePrefs.SaveDataLimit);
		GamePrefs.OnGamePrefChanged += this.OnGamePrefChanged;
		this.LoadResetRequests();
		string text;
		if (!string.IsNullOrEmpty(this.saveDirectory))
		{
			text = this.saveDirectory;
		}
		else
		{
			text = this.loadDirectory;
		}
		this.taskInfoThreadSaveChunks = ThreadManager.StartThread("SaveChunks " + text, null, new ThreadManager.ThreadFunctionLoopDelegate(this.thread_SaveChunks), null, null, null, true, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadResetRequests()
	{
		if (string.IsNullOrEmpty(this.saveDirectory))
		{
			return;
		}
		this.resetRequestedChunks.Clear();
		string text = Path.Combine(this.saveDirectory, "PendingResets.7pr");
		if (SdFile.Exists(text))
		{
			using (Stream stream = SdFile.OpenRead(text))
			{
				int num = StreamUtils.ReadInt32(stream);
				for (int i = 0; i < num; i++)
				{
					this.resetRequestedChunks.Add(StreamUtils.ReadInt64(stream));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveResetRequests()
	{
		if (string.IsNullOrEmpty(this.saveDirectory))
		{
			return;
		}
		string text = Path.Combine(this.saveDirectory, "PendingResets.7pr");
		if (this.resetRequestedChunks.Count == 0)
		{
			if (SdFile.Exists(text))
			{
				SdFile.Delete(text);
			}
			return;
		}
		using (Stream stream = SdFile.OpenWrite(text))
		{
			StreamUtils.Write(stream, this.resetRequestedChunks.Count);
			foreach (long num in this.resetRequestedChunks)
			{
				StreamUtils.Write(stream, num);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs pref)
	{
		Dictionary<long, uint> dictionary = this.chunksInSaveDir;
		lock (dictionary)
		{
			if (pref == EnumGamePrefs.MaxChunkAge)
			{
				int @int = GamePrefs.GetInt(EnumGamePrefs.MaxChunkAge);
				this.maxChunkAge = (long)(@int * 24 * 60);
				if (this.maxChunkAge >= 0L)
				{
					ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(GameUtils.TotalMinutesToWorldTime((uint)this.maxChunkAge));
					int num = valueTuple.Item1;
					int item = valueTuple.Item2;
					int item2 = valueTuple.Item3;
					num--;
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Chunk Reset Time (MaxChunkAge) has been set to {0} days, {1} hours, and {2} minutes.", num, item, item2));
				}
				else
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Chunk Reset Time (MaxChunkAge) has been disabled.");
				}
			}
			else if (pref == EnumGamePrefs.SaveDataLimit)
			{
				long limitFromPref = SaveDataLimit.GetLimitFromPref();
				if (limitFromPref >= 0L)
				{
					this.maxBytes = limitFromPref;
					if (this.maxBytes < 20971520L)
					{
						Log.Warning(string.Format("Cannot set RegionFileManager storage limit to {0} bytes as it is below the minimum value of {1} bytes. The miniumum value will be used instead.", this.maxBytes, 20971520));
						this.maxBytes = 20971520L;
					}
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Save data limit has been set to {0} bytes.", this.maxBytes));
				}
				else
				{
					this.maxBytes = -1L;
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Save data limit has been disabled.");
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetChunkTimestamp(long chunkKey, uint timeStamp)
	{
		this.chunksInSaveDir[chunkKey] = timeStamp;
		if (this.groupTimestampsDirty)
		{
			return;
		}
		LongSetGroups.Group group;
		uint num;
		if (this.chunkGroups.TryGetGroup(chunkKey, out group) && (!this.groupTimestamps.TryGetValue(group, out num) || num < timeStamp))
		{
			this.groupTimestamps[group] = timeStamp;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public uint GetChunkTimestamp(long chunkKey)
	{
		LongSetGroups.Group group;
		uint num;
		if (this.chunkGroups.TryGetGroup(chunkKey, out group) && this.groupTimestamps.TryGetValue(group, out num))
		{
			return num;
		}
		uint num2;
		if (this.chunksInSaveDir.TryGetValue(chunkKey, out num2))
		{
			return num2;
		}
		Log.Error(string.Format("No timestamp available for chunk with key: {0}", chunkKey));
		return 0U;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateGroupTimestamps()
	{
		using (RegionFileManager.s_UpdateGroupTimestamps.Auto())
		{
			this.groupTimestamps.Clear();
			foreach (LongSetGroups.Group group in this.chunkGroups.Groups)
			{
				uint? num = null;
				foreach (long num2 in group.Keys)
				{
					uint num3;
					if (this.chunksInSaveDir.TryGetValue(num2, out num3) && (num == null || num.Value < num3))
					{
						num = new uint?(num3);
					}
				}
				if (num != null)
				{
					this.groupTimestamps.Add(group, num.Value);
				}
			}
			this.groupTimestampsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkProtectionLevel GetChunkProtectionLevelWithGrouping(long chunkKey)
	{
		LongSetGroups.Group group;
		ChunkProtectionLevel chunkProtectionLevel;
		if (this.chunkGroups.TryGetGroup(chunkKey, out group) && this.groupProtectionLevels.TryGetValue(group, out chunkProtectionLevel))
		{
			return chunkProtectionLevel;
		}
		ChunkProtectionLevel chunkProtectionLevel2;
		if (this.chunkProtectionLevels.TryGetValue(chunkKey, out chunkProtectionLevel2))
		{
			return chunkProtectionLevel2;
		}
		return ChunkProtectionLevel.None;
	}

	public ChunkProtectionLevel GetChunkProtectionLevelForWorldPos(Vector3i worldPos)
	{
		return this.GetChunkProtectionLevel(World.toChunkXZ(worldPos.x), World.toChunkXZ(worldPos.z));
	}

	public ChunkProtectionLevel GetChunkProtectionLevel(int chunkX, int chunkZ)
	{
		return this.GetChunkProtectionLevel(WorldChunkCache.MakeChunkKey(chunkX, chunkZ));
	}

	public ChunkProtectionLevel GetChunkProtectionLevel(long chunkKey)
	{
		object obj = this.saveLock;
		ChunkProtectionLevel chunkProtectionLevel2;
		lock (obj)
		{
			Dictionary<long, uint> dictionary = this.chunksInSaveDir;
			lock (dictionary)
			{
				if (this.protectionLevelsDirty)
				{
					this.UpdateChunkProtectionLevels(EnumResetUnprotectedChunksGroupingMode.GroupedPOIs);
				}
				ChunkProtectionLevel chunkProtectionLevel;
				if (this.chunkProtectionLevels.TryGetValue(chunkKey, out chunkProtectionLevel))
				{
					chunkProtectionLevel2 = chunkProtectionLevel;
				}
				else
				{
					chunkProtectionLevel2 = ChunkProtectionLevel.None;
				}
			}
		}
		return chunkProtectionLevel2;
	}

	public void RebuildChunkGroupsFromPOIs()
	{
		using (RegionFileManager.s_ComputeChunkGroups.Auto())
		{
			Dictionary<long, uint> dictionary = this.chunksInSaveDir;
			lock (dictionary)
			{
				DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
				this.dynamicPrefabsBuffer.Clear();
				dynamicPrefabDecorator.GetAllPrefabs(this.dynamicPrefabsBuffer);
				this.chunkGroups.Clear();
				this.chunksByTraderID.Clear();
				LongSetGroups.Group group = null;
				foreach (PrefabInstance prefabInstance in this.dynamicPrefabsBuffer)
				{
					if (!prefabInstance.name.Contains("rwg_tile"))
					{
						HashSetLong occupiedChunks = prefabInstance.GetOccupiedChunks();
						if (occupiedChunks.Count > 1)
						{
							if (prefabInstance.prefab.bTraderArea)
							{
								this.chunksByTraderID[prefabInstance.id] = new HashSetLong(occupiedChunks);
							}
							LongSetGroups.Group group2 = this.chunkGroups.MergeOrCreateGroup(occupiedChunks);
							if (group == null || group2.Count > group.Count)
							{
								group = group2;
							}
						}
					}
				}
				this.UpdateGroupTimestamps();
				Log.Out(string.Format("Computed {0} chunk groups containing a total of {1} chunks. Largest group contains {2} chunks.", this.chunkGroups.Count, this.chunkGroups.GroupedLongsCount, (group != null) ? new int?(group.Count) : null));
			}
		}
	}

	public void AddGroupedChunks(ICollection<long> chunksToGroup)
	{
		using (RegionFileManager.s_AddGroupedChunks.Auto())
		{
			Dictionary<long, uint> dictionary = this.chunksInSaveDir;
			lock (dictionary)
			{
				HashSetLong hashSetLong = new HashSetLong(chunksToGroup);
				this.chunkGroups.MergeOrCreateGroup(hashSetLong);
				this.protectionLevelsDirty = true;
				this.groupTimestampsDirty = true;
			}
		}
	}

	public void RemoveGroupedChunks(HashSetLong chunks)
	{
		using (RegionFileManager.s_RemoveGroupedChunks.Auto())
		{
			Dictionary<long, uint> dictionary = this.chunksInSaveDir;
			lock (dictionary)
			{
				new List<LongSetGroups.Group>();
				this.chunkGroups.RemoveGroupedKeys(chunks);
				this.protectionLevelsDirty = true;
				this.groupTimestampsDirty = true;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		this.ProcessPendingPersistentDataRemovals();
	}

	public void Cleanup()
	{
		this.startSavingTask();
		this.bSaveRunning = false;
		if (this.taskInfoThreadSaveChunks != null)
		{
			this.taskInfoThreadSaveChunks.WaitForEnd(30);
		}
		this.ProcessChunkDeletionPackages();
		this.SaveResetRequests();
		SaveDataUtils.SaveDataManager.DeregisterRegionFileManager(this);
		this.regionFileAccess.Close();
		GamePrefs.OnGamePrefChanged -= this.OnGamePrefChanged;
		this.expiredChunks.Clear();
		this.resetRequestedChunks.Clear();
		this.persistentDataRemovalsRequestedChunks.Clear();
		this.chunkProtectionLevels.Clear();
		this.sortedCullingCandidates.Clear();
		this.chunksByTraderID.Clear();
		this.chunkGroups.Clear();
		this.groupProtectionLevels.Clear();
		this.groupTimestamps.Clear();
		this.processedChunkGroups.Clear();
		this.pendingChunkDeletionPackages.Clear();
		RegionFileManager.ProtectedPositionCache protectedPositionCache = this.ppdPositionCache;
		lock (protectedPositionCache)
		{
			this.ppdPositionCache.ClearAll();
		}
		this.snapshotUtil.Cleanup();
	}

	public void SetCacheSize(int _cacheSize)
	{
		this.maxChunksInCache = _cacheSize;
	}

	public void RequestChunkReset(long _chunkKey)
	{
		Dictionary<long, uint> dictionary = this.chunksInSaveDir;
		lock (dictionary)
		{
			LongSetGroups.Group group;
			if (this.chunkGroups.TryGetGroup(_chunkKey, out group))
			{
				using (IEnumerator<long> enumerator = group.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						long num = enumerator.Current;
						if (!this.resetRequestedChunks.Contains(num))
						{
							this.resetRequestedChunks.Add(num);
						}
					}
					return;
				}
			}
			if (!this.resetRequestedChunks.Contains(_chunkKey))
			{
				this.resetRequestedChunks.Add(_chunkKey);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CullExpiredChunks()
	{
		if (this.maxChunkAge < 0L && this.resetRequestedChunks.Count == 0)
		{
			return;
		}
		using (RegionFileManager.s_CullExpiredChunks.Auto())
		{
			object obj = this.saveLock;
			lock (obj)
			{
				Dictionary<long, uint> dictionary = this.chunksInSaveDir;
				lock (dictionary)
				{
					if (this.protectionLevelsDirty)
					{
						this.UpdateChunkProtectionLevels(EnumResetUnprotectedChunksGroupingMode.GroupedPOIs);
					}
					if (this.groupTimestampsDirty)
					{
						this.UpdateGroupTimestamps();
					}
					if (this.maxChunkAge >= 0L)
					{
						uint num = GameUtils.WorldTimeToTotalMinutes(GameManager.Instance.World.worldTime);
						using (Dictionary<long, uint>.KeyCollection.Enumerator enumerator = this.chunksInSaveDir.Keys.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								long num2 = enumerator.Current;
								if (!this.resetRequestedChunks.Contains(num2))
								{
									uint chunkTimestamp = this.GetChunkTimestamp(num2);
									if ((ulong)(num - chunkTimestamp) <= (ulong)this.maxChunkAge)
									{
										continue;
									}
								}
								ChunkProtectionLevel chunkProtectionLevel;
								if (this.chunkProtectionLevels.TryGetValue(num2, out chunkProtectionLevel))
								{
									this.<CullExpiredChunks>g__UpdateResetRequest|87_0(num2, chunkProtectionLevel);
								}
								else
								{
									this.expiredChunks.Add(num2);
									this.resetRequestedChunks.Remove(num2);
									if (this.expiredChunks.Count >= 10000)
									{
										break;
									}
								}
							}
							goto IL_01B1;
						}
					}
					for (int i = this.resetRequestedChunks.Count - 1; i >= 0; i--)
					{
						long num3 = this.resetRequestedChunks[i];
						ChunkProtectionLevel chunkProtectionLevel2;
						if (this.chunkProtectionLevels.TryGetValue(num3, out chunkProtectionLevel2))
						{
							this.<CullExpiredChunks>g__UpdateResetRequest|87_0(num3, chunkProtectionLevel2);
						}
						else
						{
							this.expiredChunks.Add(num3);
							this.resetRequestedChunks.RemoveAt(i);
							if (this.expiredChunks.Count >= 10000)
							{
								break;
							}
						}
					}
					IL_01B1:
					this.RemoveChunks(this.expiredChunks, true, false);
					this.expiredChunks.Clear();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MakeRoomForChunk(long chunkSizeInBytes, long chunkKey)
	{
		object obj = this.saveLock;
		lock (obj)
		{
			Dictionary<long, uint> dictionary = this.chunksInSaveDir;
			lock (dictionary)
			{
				if (this.maxBytes > 0L)
				{
					long num = (long)this.GetChunkByteCount(chunkKey);
					long num2 = chunkSizeInBytes - num;
					if (num2 > 0L)
					{
						long totalByteCount = this.regionFileAccess.GetTotalByteCount(this.saveDirectory);
						long num3 = totalByteCount + num2 - this.maxBytes;
						if (num3 > 0L)
						{
							if (chunkSizeInBytes > this.maxBytes)
							{
								throw new Exception(string.Format("Requested space ({0} bytes) exceeds maximum available space ({1} bytes).", chunkSizeInBytes, this.maxBytes));
							}
							LongSetGroups.Group group;
							if (!this.chunkGroups.TryGetGroup(chunkKey, out group))
							{
								group = null;
							}
							if (this.protectionLevelsDirty)
							{
								this.UpdateChunkProtectionLevels(EnumResetUnprotectedChunksGroupingMode.GroupedPOIs);
							}
							if (this.groupTimestampsDirty)
							{
								this.UpdateGroupTimestamps();
							}
							using (RegionFileManager.s_SortCullingCandidates.Auto())
							{
								this.sortedCullingCandidates.Clear();
								foreach (long num4 in this.chunksInSaveDir.Keys)
								{
									if (num4 != chunkKey && (group == null || !group.Contains(chunkKey)))
									{
										ulong num5 = (ulong)this.GetChunkTimestamp(num4);
										ChunkProtectionLevel chunkProtectionLevel;
										if (this.chunkProtectionLevels.TryGetValue(num4, out chunkProtectionLevel))
										{
											num5 += (ulong)((ulong)((long)chunkProtectionLevel) << 32);
										}
										this.sortedCullingCandidates.Add(new KeyValuePair<long, ulong>(num4, num5));
									}
								}
								this.sortedCullingCandidates.Sort((KeyValuePair<long, ulong> a, KeyValuePair<long, ulong> b) => a.Value.CompareTo(b.Value));
							}
							long num6 = 0L;
							using (RegionFileManager.s_CollectBestCandidates.Auto())
							{
								long num7 = num3 + 5242880L;
								this.expiredChunks.Clear();
								this.processedChunkGroups.Clear();
								foreach (KeyValuePair<long, ulong> keyValuePair in this.sortedCullingCandidates)
								{
									if (num6 >= num7)
									{
										break;
									}
									if (num6 >= num3 && keyValuePair.Value >= 549755813888UL)
									{
										break;
									}
									LongSetGroups.Group group2;
									if (this.chunkGroups.TryGetGroup(keyValuePair.Key, out group2))
									{
										if (!this.processedChunkGroups.Contains(group2))
										{
											foreach (long num8 in group2.Keys)
											{
												if (this.chunksInSaveDir.ContainsKey(num8))
												{
													this.expiredChunks.Add(num8);
													int chunkByteCount = this.GetChunkByteCount(num8);
													num6 += (long)chunkByteCount;
												}
											}
											this.processedChunkGroups.Add(group2);
										}
									}
									else
									{
										this.expiredChunks.Add(keyValuePair.Key);
										int chunkByteCount2 = this.GetChunkByteCount(keyValuePair.Key);
										num6 += (long)chunkByteCount2;
									}
								}
							}
							using (RegionFileManager.s_ForceCullChunks.Auto())
							{
								this.RemoveChunks(this.expiredChunks, true, true);
								this.expiredChunks.Clear();
							}
							if (totalByteCount - this.regionFileAccess.GetTotalByteCount(this.saveDirectory) < num3)
							{
								throw new Exception("Failed to clear as much space as requested.");
							}
						}
					}
				}
			}
		}
	}

	public bool MakeRoom(long requiredBytesToClear)
	{
		if (requiredBytesToClear <= 0L)
		{
			return true;
		}
		object obj = this.saveLock;
		lock (obj)
		{
			Dictionary<long, uint> dictionary = this.chunksInSaveDir;
			lock (dictionary)
			{
				if (this.protectionLevelsDirty)
				{
					this.UpdateChunkProtectionLevels(EnumResetUnprotectedChunksGroupingMode.GroupedPOIs);
				}
				if (this.groupTimestampsDirty)
				{
					this.UpdateGroupTimestamps();
				}
				long totalByteCount = this.regionFileAccess.GetTotalByteCount(this.saveDirectory);
				if (requiredBytesToClear > totalByteCount)
				{
					Debug.LogError("RegionFileManager has been requested to clear more space than is currently occupied by region data. This may imply the save data limit has been set too low, or another system has exceeded its expected save data budget. Region data will be cleared, but the target will not be met.");
				}
				using (RegionFileManager.s_SortCullingCandidates.Auto())
				{
					this.sortedCullingCandidates.Clear();
					foreach (long num in this.chunksInSaveDir.Keys)
					{
						ulong num2 = (ulong)this.GetChunkTimestamp(num);
						ChunkProtectionLevel chunkProtectionLevel;
						if (this.chunkProtectionLevels.TryGetValue(num, out chunkProtectionLevel))
						{
							num2 += (ulong)((ulong)((long)chunkProtectionLevel) << 32);
						}
						this.sortedCullingCandidates.Add(new KeyValuePair<long, ulong>(num, num2));
					}
					this.sortedCullingCandidates.Sort((KeyValuePair<long, ulong> a, KeyValuePair<long, ulong> b) => a.Value.CompareTo(b.Value));
				}
				long num3 = 0L;
				using (RegionFileManager.s_CollectBestCandidates.Auto())
				{
					long num4 = requiredBytesToClear + 5242880L;
					this.expiredChunks.Clear();
					this.processedChunkGroups.Clear();
					foreach (KeyValuePair<long, ulong> keyValuePair in this.sortedCullingCandidates)
					{
						if (num3 >= num4)
						{
							break;
						}
						if (num3 >= requiredBytesToClear && keyValuePair.Value >= 549755813888UL)
						{
							break;
						}
						LongSetGroups.Group group;
						if (this.chunkGroups.TryGetGroup(keyValuePair.Key, out group))
						{
							if (!this.processedChunkGroups.Contains(group))
							{
								foreach (long num5 in group.Keys)
								{
									if (this.chunksInSaveDir.ContainsKey(num5))
									{
										this.expiredChunks.Add(num5);
										int chunkByteCount = this.GetChunkByteCount(num5);
										num3 += (long)chunkByteCount;
									}
								}
								this.processedChunkGroups.Add(group);
							}
						}
						else
						{
							this.expiredChunks.Add(keyValuePair.Key);
							int chunkByteCount2 = this.GetChunkByteCount(keyValuePair.Key);
							num3 += (long)chunkByteCount2;
						}
					}
				}
				using (RegionFileManager.s_ForceCullChunks.Auto())
				{
					this.RemoveChunks(this.expiredChunks, true, true);
					this.expiredChunks.Clear();
				}
				long num6 = totalByteCount - this.regionFileAccess.GetTotalByteCount(this.saveDirectory);
				if (num6 < requiredBytesToClear)
				{
					Debug.LogError("Failed to clear as much space as requested." + string.Format("\nCleared: {0} ({1:0.00} MB).", num6, (float)num6 / 1048576f) + string.Format("\nRequested: {0} ({1:0.00} MB).", requiredBytesToClear, (float)requiredBytesToClear / 1048576f));
					return false;
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetChunkByteCount(long cullingCandidate)
	{
		int num = WorldChunkCache.extractX(cullingCandidate);
		int num2 = WorldChunkCache.extractZ(cullingCandidate);
		return this.GetChunkByteCount(num, num2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetChunkByteCount(int chunkX, int chunkZ)
	{
		return this.regionFileAccess.GetChunkByteCount(this.saveDirectory, chunkX, chunkZ);
	}

	public void MainThreadCacheProtectedPositions()
	{
		if (!ThreadManager.IsMainThread())
		{
			Debug.LogError("RegionFileManager.MainThreadCacheProtectedPositions called from a secondary thread.");
			return;
		}
		using (RegionFileManager.s_UpdateProtectionLevels.Auto())
		{
			RegionFileManager.ProtectedPositionCache protectedPositionCache = this.ppdPositionCache;
			lock (protectedPositionCache)
			{
				this.ppdPositionCache.ClearAll();
				double num = (double)GameStats.GetInt(EnumGameStats.LandClaimExpiryTime) * 24.0;
				double num2 = (double)GameStats.GetInt(EnumGameStats.BedrollExpiryTime) * 24.0;
				foreach (PersistentPlayerData persistentPlayerData in ((IDictionary<PlatformUserIdentifierAbs, PersistentPlayerData>)GameManager.Instance.GetPersistentPlayerList().Players).Values)
				{
					if (persistentPlayerData.HasBedrollPos && persistentPlayerData.OfflineHours < num2)
					{
						this.ppdPositionCache.bedrolls.Add(persistentPlayerData.BedrollPos);
					}
					if (persistentPlayerData.OfflineHours < num)
					{
						foreach (Vector3i vector3i in persistentPlayerData.GetLandProtectionBlocks())
						{
							this.ppdPositionCache.lpBlocks.Add(vector3i);
						}
					}
					if (persistentPlayerData.EntityId == -1)
					{
						this.ppdPositionCache.offlinePlayers.Add(persistentPlayerData.Position);
					}
					if (persistentPlayerData.OfflineHours < num)
					{
						persistentPlayerData.ProcessBackpacks(delegate(PersistentPlayerData.ProtectedBackpack backpack)
						{
							this.ppdPositionCache.backpacks.Add(backpack.Position);
						});
					}
					foreach (QuestPositionData questPositionData in persistentPlayerData.QuestPositions)
					{
						this.ppdPositionCache.quests.Add(questPositionData.blockPosition);
					}
					foreach (Vector3i vector3i2 in persistentPlayerData.OwnedVendingMachinePositions)
					{
						this.ppdPositionCache.vendingMachines.Add(vector3i2);
					}
				}
				foreach (ValueTuple<int, Vector3> valueTuple in VehicleManager.Instance.GetVehiclePositionsList())
				{
					this.ppdPositionCache.vehicles.Add(World.worldToBlockPos(valueTuple.Item2));
				}
				foreach (ValueTuple<int, Vector3> valueTuple2 in DroneManager.Instance.GetDronePositionsList())
				{
					this.ppdPositionCache.drones.Add(World.worldToBlockPos(valueTuple2.Item2));
				}
				foreach (AIDirectorAirDropComponent.SupplyCrateCache supplyCrateCache in GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>().supplyCrates)
				{
					this.ppdPositionCache.supplyCrates.Add(supplyCrateCache.blockPos);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateChunkProtectionLevels(EnumResetUnprotectedChunksGroupingMode _groupingMode = EnumResetUnprotectedChunksGroupingMode.GroupedPOIs)
	{
		using (RegionFileManager.s_UpdateProtectionLevels.Auto())
		{
			this.chunkProtectionLevels.Clear();
			this.groupProtectionLevels.Clear();
			this.<UpdateChunkProtectionLevels>g__EvaluateSingleChunkProtectionLevels|95_0();
			switch (_groupingMode)
			{
			case EnumResetUnprotectedChunksGroupingMode.NoGrouping:
				goto IL_0208;
			case EnumResetUnprotectedChunksGroupingMode.SeparatePOIs:
				break;
			case EnumResetUnprotectedChunksGroupingMode.GroupedPOIs:
			{
				using (Dictionary<LongSetGroups.Group, ChunkProtectionLevel>.Enumerator enumerator = this.groupProtectionLevels.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<LongSetGroups.Group, ChunkProtectionLevel> keyValuePair = enumerator.Current;
						LongSetGroups.Group group;
						ChunkProtectionLevel chunkProtectionLevel;
						keyValuePair.Deconstruct(out group, out chunkProtectionLevel);
						LongSetGroups.Group group2 = group;
						ChunkProtectionLevel chunkProtectionLevel2 = chunkProtectionLevel;
						foreach (long num in group2.Keys)
						{
							ChunkProtectionLevel chunkProtectionLevel3;
							if (this.chunkProtectionLevels.TryGetValue(num, out chunkProtectionLevel3) && chunkProtectionLevel3 != (chunkProtectionLevel3 & chunkProtectionLevel2))
							{
								Log.Error(string.Format("Error in chunk group protection: member has protections not accounted for by the group. chunkKey: {0}, protectionLevel: {1}, groupProtectionLevel: {2})", num, chunkProtectionLevel3, chunkProtectionLevel2));
							}
							this.chunkProtectionLevels[num] = chunkProtectionLevel2;
						}
					}
					goto IL_0208;
				}
				break;
			}
			default:
				goto IL_0208;
			}
			List<ChunkProtectionLevel> list = new List<ChunkProtectionLevel>(this.dynamicPrefabsBuffer.Count);
			for (int i = 0; i < this.dynamicPrefabsBuffer.Count; i++)
			{
				ChunkProtectionLevel chunkProtectionLevel4 = ChunkProtectionLevel.None;
				foreach (long num2 in this.dynamicPrefabsBuffer[i].GetOccupiedChunks())
				{
					ChunkProtectionLevel chunkProtectionLevel5;
					if (this.chunkProtectionLevels.TryGetValue(num2, out chunkProtectionLevel5))
					{
						chunkProtectionLevel4 |= chunkProtectionLevel5;
					}
				}
				list.Add(chunkProtectionLevel4);
			}
			for (int j = 0; j < this.dynamicPrefabsBuffer.Count; j++)
			{
				HashSetLong occupiedChunks = this.dynamicPrefabsBuffer[j].GetOccupiedChunks();
				if (list[j] != ChunkProtectionLevel.None)
				{
					foreach (long num3 in occupiedChunks)
					{
						this.<UpdateChunkProtectionLevels>g__AddProtectionLevel|95_3(num3, list[j]);
					}
				}
			}
			IL_0208:
			this.protectionLevelsDirty = _groupingMode != EnumResetUnprotectedChunksGroupingMode.GroupedPOIs;
			RegionFileManager.s_ProtectedChunkCount.Value = this.chunkProtectionLevels.Count;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool DoSaveChunks()
	{
		IRegionFileChunkSnapshot regionFileChunkSnapshot = null;
		try
		{
			List<Chunk> list = this.chunksToUnloadLater;
			lock (list)
			{
				for (int i = this.chunksToUnloadLater.Count - 1; i >= 0; i--)
				{
					if (!this.chunksToUnloadLater[i].IsLockedExceptUnloading)
					{
						MemoryPools.PoolChunks.FreeSync(this.chunksToUnloadLater[i]);
						this.chunksToUnloadLater.RemoveAt(i);
					}
				}
			}
			this.protectionLevelsDirty = true;
			this.CullExpiredChunks();
			this.regionFileAccess.OptimizeLayouts();
			if (this.chunkMemoryStreamsToSave.Count == 0 && this.chunksToSave.Count == 0)
			{
				return false;
			}
			long num = 0L;
			Dictionary<long, IRegionFileChunkSnapshot> dictionary = this.chunkMemoryStreamsToSave;
			lock (dictionary)
			{
				if (this.chunkMemoryStreamsToSave.Count > 0)
				{
					using (Dictionary<long, IRegionFileChunkSnapshot>.Enumerator enumerator = this.chunkMemoryStreamsToSave.GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							KeyValuePair<long, IRegionFileChunkSnapshot> keyValuePair = enumerator.Current;
							num = keyValuePair.Key;
							regionFileChunkSnapshot = this.chunkMemoryStreamsToSave[num];
							this.chunkMemoryStreamsToSave.Remove(num);
						}
					}
				}
			}
			if (regionFileChunkSnapshot == null)
			{
				Chunk chunk = null;
				Dictionary<long, Chunk> dictionary2 = this.chunksToSave;
				lock (dictionary2)
				{
					using (Dictionary<long, Chunk>.Enumerator enumerator2 = this.chunksToSave.GetEnumerator())
					{
						if (enumerator2.MoveNext())
						{
							KeyValuePair<long, Chunk> keyValuePair2 = enumerator2.Current;
							chunk = this.chunksToSave[keyValuePair2.Key];
							this.chunksToSave.Remove(keyValuePair2.Key);
							this.chunkKeyCurrentlySaved = keyValuePair2.Key;
						}
					}
				}
				if (chunk != null)
				{
					num = chunk.Key;
					regionFileChunkSnapshot = this.snapshotUtil.TakeSnapshot(chunk, false);
					chunk.InProgressSaving = false;
					if (chunk.IsLockedExceptUnloading)
					{
						list = this.chunksToUnloadLater;
						lock (list)
						{
							this.chunksToUnloadLater.Add(chunk);
							goto IL_022C;
						}
					}
					MemoryPools.PoolChunks.FreeSync(chunk);
				}
			}
			IL_022C:
			if (regionFileChunkSnapshot != null)
			{
				int num2 = WorldChunkCache.extractX(num);
				int num3 = WorldChunkCache.extractZ(num);
				if (!SaveDataUtils.SaveDataManager.AppliesSaveSizeLimit)
				{
					this.MakeRoomForChunk(regionFileChunkSnapshot.Size, num);
				}
				this.snapshotUtil.WriteSnapshot(regionFileChunkSnapshot, this.saveDirectory, num2, num3);
				Dictionary<long, uint> dictionary3 = this.chunksInSaveDir;
				lock (dictionary3)
				{
					uint num4 = GameUtils.WorldTimeToTotalMinutes(GameManager.Instance.World.worldTime);
					this.SetChunkTimestamp(num, num4);
					this.chunkKeyCurrentlySaved = long.MaxValue;
				}
				RegionFileManager.s_RegionDataSize.Value = this.regionFileAccess.GetTotalByteCount(this.saveDirectory);
			}
			MultiBlockManager.Instance.SaveIfDirty();
		}
		catch (Exception ex)
		{
			string text = "ERROR: ";
			Exception ex2 = ex;
			Log.Error(text + ((ex2 != null) ? ex2.ToString() : null));
			return false;
		}
		finally
		{
			if (regionFileChunkSnapshot != null)
			{
				this.snapshotUtil.Free(regionFileChunkSnapshot);
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int thread_SaveChunks(ThreadManager.ThreadInfo _taskInfo)
	{
		if (this.bSaveRunning)
		{
			if (this.thread_SaveChunks_SleepCount >= 40 || this.saveThreadWaitHandle.WaitOne(0))
			{
				for (;;)
				{
					object obj = this.saveLock;
					lock (obj)
					{
						if (this.DoSaveChunks())
						{
							continue;
						}
					}
					break;
				}
				this.thread_SaveChunks_SleepCount = 0;
			}
			return 5;
		}
		return -1;
	}

	public override Chunk GetChunkSync(long _key)
	{
		Chunk chunk = null;
		bool flag = false;
		if (base.ContainsChunkSync(_key))
		{
			chunk = base.GetChunkSync(_key);
			flag = true;
		}
		IRegionFileChunkSnapshot regionFileChunkSnapshot = null;
		if (chunk == null)
		{
			Dictionary<long, IRegionFileChunkSnapshot> dictionary = this.chunkMemoryStreamsToSave;
			lock (dictionary)
			{
				if (this.chunkMemoryStreamsToSave.ContainsKey(_key))
				{
					regionFileChunkSnapshot = this.chunkMemoryStreamsToSave[_key];
					this.chunkMemoryStreamsToSave.Remove(_key);
				}
			}
			this.snapshotUtil.Free(regionFileChunkSnapshot);
		}
		if (chunk == null)
		{
			Dictionary<long, Chunk> dictionary2 = this.chunksToSave;
			lock (dictionary2)
			{
				if (this.chunksToSave.ContainsKey(_key))
				{
					chunk = this.chunksToSave[_key];
					this.chunksToSave.Remove(_key);
					chunk.OnLoadedFromCache();
				}
			}
		}
		if (chunk == null)
		{
			bool flag3 = false;
			do
			{
				Dictionary<long, Chunk> dictionary2 = this.chunksToSave;
				lock (dictionary2)
				{
					flag3 = _key == this.chunkKeyCurrentlySaved;
				}
				if (flag3)
				{
					Thread.Sleep(5);
				}
			}
			while (flag3);
		}
		if (chunk == null)
		{
			if (this.isChunkInSaveDir(_key))
			{
				chunk = this.snapshotUtil.LoadChunk(this.saveDirectory, _key);
				if (chunk != null)
				{
					goto IL_0180;
				}
				Dictionary<long, uint> dictionary3 = this.chunksInSaveDir;
				lock (dictionary3)
				{
					this.chunksInSaveDir.Remove(_key);
					goto IL_0180;
				}
			}
			if (this.isChunkInLoadDir(_key))
			{
				chunk = this.snapshotUtil.LoadChunk(this.loadDirectory, _key);
				if (chunk == null)
				{
					this.chunksInLoadDir.Remove(_key);
				}
			}
		}
		IL_0180:
		if (chunk != null && !flag && this.maxChunksInCache > 0)
		{
			this.cacheChunk(chunk);
		}
		return chunk;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cacheChunk(Chunk _chunk)
	{
		if (this.maxChunksInCache > 0)
		{
			base.AddChunkSync(_chunk, false);
			List<long> list = this.chunksInLocalCache;
			long num;
			lock (list)
			{
				this.chunksInLocalCache.Add(_chunk.Key);
				if (this.chunksInLocalCache.Count <= this.maxChunksInCache)
				{
					return;
				}
				num = this.chunksInLocalCache[0];
			}
			_chunk = base.GetChunkSync(num);
			this.RemoveChunkSync(num);
		}
		if (this.bSaveOnChunkDrop && _chunk.NeedsSaving && !_chunk.InProgressSaving)
		{
			_chunk.InProgressSaving = true;
			Dictionary<long, Chunk> dictionary = this.chunksToSave;
			lock (dictionary)
			{
				if (!this.chunksToSave.ContainsKey(_chunk.Key))
				{
					this.chunksToSave.Add(_chunk.Key, _chunk);
					this.startSavingTask();
				}
				return;
			}
		}
		if (_chunk.IsLockedExceptUnloading)
		{
			List<Chunk> list2 = this.chunksToUnloadLater;
			lock (list2)
			{
				this.chunksToUnloadLater.Add(_chunk);
				return;
			}
		}
		MemoryPools.PoolChunks.FreeSync(_chunk);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startSavingTask()
	{
		this.saveThreadWaitHandle.Set();
	}

	public override bool AddChunkSync(Chunk _chunk, bool _bOmitCallbacks = false)
	{
		if (!GameManager.bSavingActive || _chunk.NeedsDecoration)
		{
			MemoryPools.PoolChunks.FreeSync(_chunk);
			return false;
		}
		if (!base.ContainsChunkSync(_chunk.Key))
		{
			this.cacheChunk(_chunk);
		}
		return true;
	}

	public override bool ContainsChunkSync(long key)
	{
		if (base.ContainsChunkSync(key))
		{
			return true;
		}
		Dictionary<long, Chunk> dictionary = this.chunksToSave;
		lock (dictionary)
		{
			if (this.chunksToSave.ContainsKey(key))
			{
				return true;
			}
			if (this.chunkKeyCurrentlySaved == key)
			{
				return true;
			}
		}
		return this.isChunkInSaveDir(key) || this.isChunkInLoadDir(key);
	}

	public bool IsChunkSavedAndDormant(long key)
	{
		Dictionary<long, uint> dictionary = this.chunksInSaveDir;
		bool flag2;
		lock (dictionary)
		{
			if (!this.chunksInSaveDir.ContainsKey(key))
			{
				flag2 = false;
			}
			else if (base.ContainsChunkSync(key))
			{
				flag2 = false;
			}
			else
			{
				Dictionary<long, Chunk> dictionary2 = this.chunksToSave;
				lock (dictionary2)
				{
					if (this.chunksToSave.ContainsKey(key))
					{
						return false;
					}
					if (this.chunkKeyCurrentlySaved == key)
					{
						return false;
					}
				}
				flag2 = true;
			}
		}
		return flag2;
	}

	public override void RemoveChunkSync(long key)
	{
		if (base.ContainsChunkSync(key))
		{
			base.RemoveChunkSync(key);
		}
		List<long> list = this.chunksInLocalCache;
		lock (list)
		{
			this.chunksInLocalCache.Remove(key);
		}
	}

	public int MakePersistent(ChunkCluster _mainCache, bool _bSaveEvenIfUnchanged)
	{
		List<Chunk> list = new List<Chunk>();
		List<Chunk> chunkArrayCopySync = base.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			if (!chunk.NeedsDecoration && (chunk.NeedsSaving || _bSaveEvenIfUnchanged))
			{
				Dictionary<long, Chunk> dictionary = this.chunksToSave;
				lock (dictionary)
				{
					if (this.chunksToSave.ContainsKey(chunk.Key))
					{
						goto IL_0075;
					}
				}
				chunk.InProgressSaving = true;
				list.Add(chunk);
			}
			IL_0075:;
		}
		if (_mainCache != null)
		{
			List<Chunk> chunkArrayCopySync2 = _mainCache.GetChunkArrayCopySync();
			int j = 0;
			while (j < chunkArrayCopySync2.Count)
			{
				Chunk chunk2 = chunkArrayCopySync2[j];
				Chunk chunk3 = chunk2;
				lock (chunk3)
				{
					if (chunk2.InProgressUnloading)
					{
						goto IL_0107;
					}
					chunk2.InProgressSaving = true;
				}
				goto IL_00D5;
				IL_0107:
				j++;
				continue;
				IL_00D5:
				_mainCache.NotifyOnChunkBeforeSave(chunk2);
				if (!chunk2.NeedsDecoration && (chunk2.NeedsSaving || _bSaveEvenIfUnchanged))
				{
					list.Add(chunk2);
					goto IL_0107;
				}
				chunk2.InProgressSaving = false;
				goto IL_0107;
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			this.SaveChunkSnapshot(list[k], _bSaveEvenIfUnchanged);
			list[k].InProgressSaving = false;
		}
		this.startSavingTask();
		return list.Count;
	}

	public void WaitSaveDone()
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		int count = this.chunkMemoryStreamsToSave.Count;
		int num = 0;
		do
		{
			Dictionary<long, IRegionFileChunkSnapshot> dictionary = this.chunkMemoryStreamsToSave;
			lock (dictionary)
			{
				num = this.chunkMemoryStreamsToSave.Count;
			}
			Dictionary<long, Chunk> dictionary2 = this.chunksToSave;
			lock (dictionary2)
			{
				num += this.chunksToSave.Count;
			}
			Thread.Sleep(20);
		}
		while (num > 0);
		this.ProcessPendingPersistentDataRemovals();
		Log.Out(string.Concat(new string[]
		{
			"Saving ",
			count.ToString(),
			" of chunks took ",
			microStopwatch.ElapsedMilliseconds.ToString(),
			"ms"
		}));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isChunkInLoadDir(long _key)
	{
		return this.chunksInLoadDir.Contains(_key);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isChunkInSaveDir(long _key)
	{
		Dictionary<long, uint> dictionary = this.chunksInSaveDir;
		bool flag2;
		lock (dictionary)
		{
			flag2 = this.chunksInSaveDir.ContainsKey(_key);
		}
		return flag2;
	}

	public virtual void SaveChunkSnapshot(Chunk _chunk, bool _saveIfUnchanged)
	{
		if (this.saveDirectory == null)
		{
			return;
		}
		Dictionary<long, IRegionFileChunkSnapshot> dictionary = this.chunkMemoryStreamsToSave;
		lock (dictionary)
		{
			IRegionFileChunkSnapshot regionFileChunkSnapshot;
			if (this.chunkMemoryStreamsToSave.TryGetValue(_chunk.Key, out regionFileChunkSnapshot))
			{
				regionFileChunkSnapshot.Update(_chunk, _saveIfUnchanged);
			}
			else
			{
				this.chunkMemoryStreamsToSave.Add(_chunk.Key, this.snapshotUtil.TakeSnapshot(_chunk, _saveIfUnchanged));
			}
		}
		this.startSavingTask();
	}

	public long[] GetAllChunkKeys()
	{
		HashSetLong hashSetLong = new HashSetLong();
		foreach (long num in this.chunksInLoadDir)
		{
			hashSetLong.Add(num);
		}
		Dictionary<long, uint> dictionary = this.chunksInSaveDir;
		lock (dictionary)
		{
			foreach (long num2 in this.chunksInSaveDir.Keys)
			{
				hashSetLong.Add(num2);
			}
		}
		long[] array = new long[hashSetLong.Count];
		hashSetLong.CopyTo(array);
		return array;
	}

	public HashSetLong GetUniqueChunkKeys(ChunkProtectionLevel excludedProtectionLevels)
	{
		HashSetLong hashSetLong = new HashSetLong();
		foreach (long num in this.chunksInLoadDir)
		{
			ChunkProtectionLevel chunkProtectionLevel;
			if (!this.chunkProtectionLevels.TryGetValue(num, out chunkProtectionLevel) || (chunkProtectionLevel & excludedProtectionLevels) == ChunkProtectionLevel.None)
			{
				hashSetLong.Add(num);
			}
		}
		Dictionary<long, uint> dictionary = this.chunksInSaveDir;
		lock (dictionary)
		{
			foreach (long num2 in this.chunksInSaveDir.Keys)
			{
				ChunkProtectionLevel chunkProtectionLevel2;
				if (!this.chunkProtectionLevels.TryGetValue(num2, out chunkProtectionLevel2) || (chunkProtectionLevel2 & excludedProtectionLevels) == ChunkProtectionLevel.None)
				{
					hashSetLong.Add(num2);
				}
			}
		}
		return hashSetLong;
	}

	public override void Clear()
	{
		base.Clear();
	}

	public void ClearCaches()
	{
		this.regionFileAccess.ClearCache();
	}

	public void RemoveChunks(ICollection<long> _chunks, bool _resetDecos = true, bool _optimizeFileLayouts = true)
	{
		if (_chunks.Count == 0)
		{
			return;
		}
		if (Monitor.IsEntered(this.chunksInSaveDir) && !Monitor.IsEntered(this.saveLock))
		{
			Debug.LogError("RemoveChunks failed. Thread safety violation: the lock on \"saveLock\" must be taken before the lock on \"chunksInSaveDir\" to avoid a possible deadlock.");
			return;
		}
		object obj = this.saveLock;
		lock (obj)
		{
			Dictionary<long, uint> dictionary = this.chunksInSaveDir;
			lock (dictionary)
			{
				this.RemovePersistentDataForChunks(_chunks);
				RegionFileManager.resetVolumeDataForChunks(_chunks);
				foreach (long num in _chunks)
				{
					this.<RemoveChunks>g__RemoveChunk|116_0(num, _resetDecos);
				}
				MultiBlockManager.Instance.CullChunklessData();
				List<NetPackageDeleteChunkData> list = this.pendingChunkDeletionPackages;
				lock (list)
				{
					NetPackageDeleteChunkData netPackageDeleteChunkData = NetPackageManager.GetPackage<NetPackageDeleteChunkData>().Setup(_chunks);
					this.pendingChunkDeletionPackages.Add(netPackageDeleteChunkData);
				}
				ThreadManager.AddSingleTaskMainThread("RemoveChunks.ProcessChunkDeletionPackages", delegate
				{
					this.ProcessChunkDeletionPackages();
				});
				if (_optimizeFileLayouts)
				{
					this.regionFileAccess.OptimizeLayouts();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessChunkDeletionPackages()
	{
		List<NetPackageDeleteChunkData> list = this.pendingChunkDeletionPackages;
		lock (list)
		{
			foreach (NetPackageDeleteChunkData netPackageDeleteChunkData in this.pendingChunkDeletionPackages)
			{
				DynamicMeshUnity.DeleteDynamicMeshData(netPackageDeleteChunkData.chunkKeys);
				WaterSimulationNative.Instance.changeApplier.DiscardChangesForChunks(netPackageDeleteChunkData.chunkKeys);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackageDeleteChunkData, false, -1, -1, -1, null, 192, false);
			}
			this.pendingChunkDeletionPackages.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemovePersistentDataForChunks(ICollection<long> _chunks)
	{
		if (_chunks.Count == 0)
		{
			return;
		}
		if (!ThreadManager.IsMainThread())
		{
			HashSet<long> hashSet = this.persistentDataRemovalsRequestedChunks;
			lock (hashSet)
			{
				foreach (long num in _chunks)
				{
					this.persistentDataRemovalsRequestedChunks.Add(num);
				}
			}
			return;
		}
		if (GameManager.Instance.World == null)
		{
			Log.Error("RemovePersistentDataForChunks world is null");
			return;
		}
		foreach (PersistentPlayerData persistentPlayerData in GameManager.Instance.persistentPlayers.Players.Values)
		{
			bool backpacksChanged = false;
			persistentPlayerData.RemoveBackpacks(delegate(PersistentPlayerData.ProtectedBackpack backpack)
			{
				long num5 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(backpack.Position.x), World.toChunkXZ(backpack.Position.z));
				using (IEnumerator<long> enumerator7 = _chunks.GetEnumerator())
				{
					while (enumerator7.MoveNext())
					{
						if (enumerator7.Current == num5)
						{
							backpacksChanged = true;
							return true;
						}
					}
				}
				return false;
			});
			if (backpacksChanged && persistentPlayerData.EntityId != -1)
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(persistentPlayerData.EntityId) as EntityPlayer;
				if (entityPlayer != null)
				{
					if (!entityPlayer.isEntityRemote)
					{
						entityPlayer.SetDroppedBackpackPositions(persistentPlayerData.GetDroppedBackpackPositions());
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerSetBackpackPosition>().Setup(persistentPlayerData.EntityId, persistentPlayerData.GetDroppedBackpackPositions()), false, persistentPlayerData.EntityId, -1, -1, null, 192, false);
					}
				}
			}
		}
		foreach (ValueTuple<int, Vector3> valueTuple in VehicleManager.Instance.GetVehiclePositionsList())
		{
			Vector3i vector3i = World.worldToBlockPos(valueTuple.Item2);
			long num2 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(vector3i.x), World.toChunkXZ(vector3i.z));
			using (IEnumerator<long> enumerator = _chunks.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == num2)
					{
						VehicleManager.Instance.RemoveUnloadedVehicle(valueTuple.Item1);
					}
				}
			}
		}
		AIDirectorAirDropComponent component = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>();
		List<int> list = new List<int>();
		foreach (AIDirectorAirDropComponent.SupplyCrateCache supplyCrateCache in component.supplyCrates)
		{
			long num3 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(supplyCrateCache.blockPos.x), World.toChunkXZ(supplyCrateCache.blockPos.z));
			using (IEnumerator<long> enumerator = _chunks.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == num3)
					{
						list.Add(supplyCrateCache.entityId);
					}
				}
			}
		}
		foreach (int num4 in list)
		{
			component.RemoveSupplyCrate(num4);
		}
		List<PowerItem> list2 = new List<PowerItem>();
		PowerManager.Instance.FindPowerItems(delegate(PowerItem item)
		{
			long num6 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(item.Position.x), World.toChunkXZ(item.Position.z));
			return _chunks.Contains(num6);
		}, list2);
		foreach (PowerItem powerItem in list2)
		{
			PowerManager.Instance.RemovePowerNode(powerItem);
		}
	}

	public void CheckPersistentData()
	{
		using (RegionFileManager.s_CheckPersistentData.Auto())
		{
			foreach (PersistentPlayerData persistentPlayerData in GameManager.Instance.persistentPlayers.Players.Values)
			{
				persistentPlayerData.RemoveBackpacks(delegate(PersistentPlayerData.ProtectedBackpack backpack)
				{
					long num4 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(backpack.Position.x), World.toChunkXZ(backpack.Position.z));
					if (!this.ContainsChunkSync(num4))
					{
						Log.Out(string.Format("Removing backpack at ({0}) as the chunk data no longer exists", backpack.Position));
						return true;
					}
					return false;
				});
			}
			foreach (ValueTuple<int, Vector3> valueTuple in VehicleManager.Instance.GetVehiclePositionsList())
			{
				Vector3i vector3i = World.worldToBlockPos(valueTuple.Item2);
				long num = WorldChunkCache.MakeChunkKey(World.toChunkXZ(vector3i.x), World.toChunkXZ(vector3i.z));
				if (!this.ContainsChunkSync(num))
				{
					Log.Out(string.Format("Removing vehicle at ({0}) as the chunk data no longer exists", vector3i));
					VehicleManager.Instance.RemoveUnloadedVehicle(valueTuple.Item1);
				}
			}
			AIDirectorAirDropComponent component = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>();
			List<int> list = new List<int>();
			foreach (AIDirectorAirDropComponent.SupplyCrateCache supplyCrateCache in component.supplyCrates)
			{
				long num2 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(supplyCrateCache.blockPos.x), World.toChunkXZ(supplyCrateCache.blockPos.z));
				if (!this.ContainsChunkSync(num2))
				{
					Log.Out(string.Format("Removing air drop at ({0}) as the chunk data no longer exists", supplyCrateCache.blockPos));
					list.Add(supplyCrateCache.entityId);
				}
			}
			foreach (int num3 in list)
			{
				component.RemoveSupplyCrate(num3);
			}
			List<PowerItem> list2 = new List<PowerItem>();
			PowerManager.Instance.FindPowerItems(delegate(PowerItem item)
			{
				long num5 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(item.Position.x), World.toChunkXZ(item.Position.z));
				return !this.ContainsChunkSync(num5);
			}, list2);
			foreach (PowerItem powerItem in list2)
			{
				Log.Out(string.Format("Removing power item at ({0}) as the chunk data no longer exists", powerItem.Position));
				PowerManager.Instance.RemovePowerNode(powerItem);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessPendingPersistentDataRemovals()
	{
		using (RegionFileManager.s_ProcessPersistentDataRemovals.Auto())
		{
			HashSet<long> hashSet = this.persistentDataRemovalsRequestedChunks;
			lock (hashSet)
			{
				if (this.persistentDataRemovalsRequestedChunks.Count > 0)
				{
					this.RemovePersistentDataForChunks(this.persistentDataRemovalsRequestedChunks);
					this.persistentDataRemovalsRequestedChunks.Clear();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void resetVolumeDataForChunks(ICollection<long> _chunks)
	{
		World world = GameManager.Instance.World;
		foreach (long num in _chunks)
		{
			world.ResetTriggerVolumes(num);
			world.ResetSleeperVolumes(num);
		}
	}

	public HashSetLong ResetAllChunks(ChunkProtectionLevel excludedProtectionLevels, EnumResetUnprotectedChunksGroupingMode _groupingMode = EnumResetUnprotectedChunksGroupingMode.GroupedPOIs)
	{
		object obj = this.saveLock;
		HashSetLong hashSetLong;
		lock (obj)
		{
			Dictionary<long, uint> dictionary = this.chunksInSaveDir;
			lock (dictionary)
			{
				this.UpdateChunkProtectionLevels(_groupingMode);
				HashSetLong uniqueChunkKeys = this.GetUniqueChunkKeys(excludedProtectionLevels);
				this.RemoveChunks(uniqueChunkKeys, true, true);
				hashSetLong = uniqueChunkKeys;
			}
		}
		return hashSetLong;
	}

	public HashSetLong ResetRegion(int _regionX, int _regionZ, ChunkProtectionLevel excludedProtectionLevels, EnumResetUnprotectedChunksGroupingMode _groupingMode = EnumResetUnprotectedChunksGroupingMode.GroupedPOIs)
	{
		object obj = this.saveLock;
		HashSetLong hashSetLong2;
		lock (obj)
		{
			Dictionary<long, uint> dictionary = this.chunksInSaveDir;
			lock (dictionary)
			{
				this.UpdateChunkProtectionLevels(_groupingMode);
				int chunksPerRegionPerDimension = this.regionFileAccess.ChunksPerRegionPerDimension;
				Vector2i vector2i = new Vector2i(_regionX * chunksPerRegionPerDimension, _regionZ * chunksPerRegionPerDimension);
				Vector2i vector2i2 = vector2i + new Vector2i(chunksPerRegionPerDimension - 1, chunksPerRegionPerDimension - 1);
				HashSetLong hashSetLong = new HashSetLong();
				for (int i = vector2i.x; i <= vector2i2.x; i++)
				{
					for (int j = vector2i.y; j <= vector2i2.y; j++)
					{
						long num = WorldChunkCache.MakeChunkKey(i, j);
						ChunkProtectionLevel chunkProtectionLevel;
						if (this.ContainsChunkSync(num) && (!this.chunkProtectionLevels.TryGetValue(num, out chunkProtectionLevel) || (chunkProtectionLevel & excludedProtectionLevels) == ChunkProtectionLevel.None))
						{
							hashSetLong.Add(num);
						}
					}
				}
				this.RemoveChunks(hashSetLong, true, true);
				hashSetLong2 = hashSetLong;
			}
		}
		return hashSetLong2;
	}

	public bool TryResetChunks(ICollection<long> _chunks, ChunkProtectionLevel _excludedProtectionLevels, out ChunkProtectionLevel _protectionLevels)
	{
		if (_chunks.Count == 0)
		{
			_protectionLevels = ChunkProtectionLevel.None;
			return true;
		}
		object obj = this.saveLock;
		bool flag3;
		lock (obj)
		{
			Dictionary<long, uint> dictionary = this.chunksInSaveDir;
			lock (dictionary)
			{
				foreach (long num in _chunks)
				{
					ChunkProtectionLevel chunkProtectionLevel = this.GetChunkProtectionLevel(num);
					_protectionLevels = chunkProtectionLevel & _excludedProtectionLevels;
					if (_protectionLevels != ChunkProtectionLevel.None)
					{
						return false;
					}
				}
				this.RemoveChunks(_chunks, true, false);
				_protectionLevels = ChunkProtectionLevel.None;
				flag3 = true;
			}
		}
		return flag3;
	}

	public void IterateChunkExpiryTimes(Action<long, ulong> action)
	{
		if (this.maxChunkAge < 0L)
		{
			return;
		}
		Dictionary<long, uint> dictionary = this.chunksInSaveDir;
		lock (dictionary)
		{
			this.UpdateChunkProtectionLevels(EnumResetUnprotectedChunksGroupingMode.GroupedPOIs);
			foreach (KeyValuePair<long, uint> keyValuePair in this.chunksInSaveDir)
			{
				ChunkProtectionLevel chunkProtectionLevel;
				if (!this.chunkProtectionLevels.TryGetValue(keyValuePair.Key, out chunkProtectionLevel))
				{
					action(keyValuePair.Key, GameUtils.TotalMinutesToWorldTime(keyValuePair.Value + (uint)this.maxChunkAge));
				}
			}
		}
	}

	public void SaveChunkAgeDebugTexture(float rangeInDays)
	{
		float num = rangeInDays * 24f * 60f;
		Vector2i worldSize = GameManager.Instance.World.ChunkCache.ChunkProvider.GetWorldSize();
		Vector2i vector2i = new Vector2i(worldSize.x / 16, worldSize.y / 16);
		int num2 = vector2i.x / 2;
		int num3 = vector2i.y / 2;
		uint num4 = GameUtils.WorldTimeToTotalMinutes(GameManager.Instance.World.worldTime);
		Color32[] array = new Color32[vector2i.x * vector2i.y];
		Dictionary<long, uint> dictionary = this.chunksInSaveDir;
		lock (dictionary)
		{
			this.UpdateChunkProtectionLevels(EnumResetUnprotectedChunksGroupingMode.GroupedPOIs);
			this.UpdateGroupTimestamps();
			float num5 = 1f / (float)RegionFileManager.<SaveChunkAgeDebugTexture>g__OrdinalProtectionLevel|126_0(ChunkProtectionLevel.CurrentlySynced);
			for (int i = 0; i < array.Length; i++)
			{
				int num6 = i % vector2i.x - num2;
				int num7 = i / vector2i.x - num3;
				long num8 = WorldChunkCache.MakeChunkKey(num6, num7);
				ValueTuple<float, float, float, float> valueTuple = new ValueTuple<float, float, float, float>(0f, 0f, 0f, 0f);
				uint num9;
				if (this.chunksInSaveDir.TryGetValue(num8, out num9))
				{
					valueTuple.Item4 = 1f;
					long num10 = (long)((ulong)(num4 - num9));
					valueTuple.Item2 = (float)num10 / num;
				}
				LongSetGroups.Group group;
				uint num11;
				if (this.chunkGroups.TryGetGroup(num8, out group) && this.groupTimestamps.TryGetValue(group, out num11))
				{
					long num12 = (long)((ulong)(num4 - num11));
					valueTuple.Item1 = (float)num12 / num;
				}
				else
				{
					valueTuple.Item1 = valueTuple.Item2;
				}
				ChunkProtectionLevel chunkProtectionLevel;
				if (this.chunkProtectionLevels.TryGetValue(num8, out chunkProtectionLevel))
				{
					valueTuple.Item3 = (float)RegionFileManager.<SaveChunkAgeDebugTexture>g__OrdinalProtectionLevel|126_0(chunkProtectionLevel) * num5;
				}
				array[i] = new Color(valueTuple.Item1, valueTuple.Item2, valueTuple.Item3, valueTuple.Item4);
			}
		}
		Texture2D texture2D = new Texture2D(vector2i.x, vector2i.y);
		texture2D.SetPixels32(array);
		texture2D.Apply();
		byte[] array2 = ImageConversion.EncodeToTGA(texture2D);
		SdFile.WriteAllBytes("ChunkAgeMap.tga", array2);
		Object.Destroy(texture2D);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <CullExpiredChunks>g__UpdateResetRequest|87_0(long chunkKey, ChunkProtectionLevel protectionLevel)
	{
		if ((protectionLevel & ~(ChunkProtectionLevel.NearOfflinePlayer | ChunkProtectionLevel.NearQuestObjective | ChunkProtectionLevel.OfflinePlayer | ChunkProtectionLevel.QuestObjective | ChunkProtectionLevel.CurrentlySynced)) != ChunkProtectionLevel.None)
		{
			this.resetRequestedChunks.Remove(chunkKey);
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateChunkProtectionLevels>g__EvaluateSingleChunkProtectionLevels|95_0()
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.LandClaimSize) / 2;
		int @int = GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize);
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		if (chunkCache != null)
		{
			chunkCache.GetSyncRoot().EnterReadLock();
			foreach (long num2 in chunkCache.chunkKeys)
			{
				this.<UpdateChunkProtectionLevels>g__AddProtectionLevel|95_3(num2, ChunkProtectionLevel.CurrentlySynced);
			}
			chunkCache.GetSyncRoot().ExitReadLock();
		}
		GameManager.Instance.World.m_ChunkManager.ProcessChunksPendingUnload(delegate(Chunk chunk)
		{
			this.<UpdateChunkProtectionLevels>g__AddProtectionLevel|95_3(chunk.Key, ChunkProtectionLevel.CurrentlySynced);
		});
		Dictionary<long, Chunk> dictionary = this.chunksToSave;
		lock (dictionary)
		{
			foreach (long num3 in this.chunksToSave.Keys)
			{
				this.<UpdateChunkProtectionLevels>g__AddProtectionLevel|95_3(num3, ChunkProtectionLevel.CurrentlySynced);
			}
		}
		Dictionary<long, IRegionFileChunkSnapshot> dictionary2 = this.chunkMemoryStreamsToSave;
		lock (dictionary2)
		{
			foreach (long num4 in this.chunkMemoryStreamsToSave.Keys)
			{
				this.<UpdateChunkProtectionLevels>g__AddProtectionLevel|95_3(num4, ChunkProtectionLevel.CurrentlySynced);
			}
		}
		foreach (HashSetLong hashSetLong in this.chunksByTraderID.Values)
		{
			using (HashSetLong.Enumerator enumerator = hashSetLong.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					long num5 = enumerator.Current;
					this.<UpdateChunkProtectionLevels>g__AddProtectionLevel|95_3(num5, ChunkProtectionLevel.Trader);
				}
			}
		}
		RegionFileManager.ProtectedPositionCache protectedPositionCache = this.ppdPositionCache;
		lock (protectedPositionCache)
		{
			this.<UpdateChunkProtectionLevels>g__ProcessProtectedPositionList|95_1(this.ppdPositionCache.bedrolls, @int, ChunkProtectionLevel.Bedroll, 1, ChunkProtectionLevel.NearBedroll);
			this.<UpdateChunkProtectionLevels>g__ProcessProtectedPositionList|95_1(this.ppdPositionCache.lpBlocks, num, ChunkProtectionLevel.LandClaim, 1, ChunkProtectionLevel.NearLandClaim);
			this.<UpdateChunkProtectionLevels>g__ProcessProtectedPositionList|95_1(this.ppdPositionCache.offlinePlayers, 0, ChunkProtectionLevel.OfflinePlayer, 1, ChunkProtectionLevel.NearOfflinePlayer);
			this.<UpdateChunkProtectionLevels>g__ProcessProtectedPositionList|95_1(this.ppdPositionCache.backpacks, 0, ChunkProtectionLevel.DroppedBackpack, 1, ChunkProtectionLevel.NearDroppedBackpack);
			this.<UpdateChunkProtectionLevels>g__ProcessProtectedPositionList|95_1(this.ppdPositionCache.quests, num, ChunkProtectionLevel.QuestObjective, 1, ChunkProtectionLevel.NearQuestObjective);
			this.<UpdateChunkProtectionLevels>g__ProcessProtectedPositionList|95_1(this.ppdPositionCache.vendingMachines, num, ChunkProtectionLevel.Trader, 1, ChunkProtectionLevel.Trader);
			this.<UpdateChunkProtectionLevels>g__ProcessProtectedPositionList|95_1(this.ppdPositionCache.drones, num, ChunkProtectionLevel.Drone, 1, ChunkProtectionLevel.Drone);
			this.<UpdateChunkProtectionLevels>g__ProcessProtectedPositionList|95_1(this.ppdPositionCache.vehicles, num, ChunkProtectionLevel.Vehicle, 1, ChunkProtectionLevel.NearVehicle);
			this.<UpdateChunkProtectionLevels>g__ProcessProtectedPositionList|95_1(this.ppdPositionCache.supplyCrates, num, ChunkProtectionLevel.SupplyCrate, 1, ChunkProtectionLevel.NearSupplyCrate);
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateChunkProtectionLevels>g__ProcessProtectedPositionList|95_1(List<Vector3i> centerPositions, int halfSize, ChunkProtectionLevel innerProtectionLevel, int margin, ChunkProtectionLevel marginProtectionLevel)
	{
		foreach (Vector3i vector3i in centerPositions)
		{
			this.<UpdateChunkProtectionLevels>g__AddChunksInWorldArea|95_2(vector3i, halfSize, innerProtectionLevel, margin, marginProtectionLevel);
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateChunkProtectionLevels>g__AddChunksInWorldArea|95_2(Vector3i centerPos, int halfSize, ChunkProtectionLevel innerProtectionLevel, int margin, ChunkProtectionLevel marginProtectionLevel)
	{
		int num = World.toChunkXZ(centerPos.x - halfSize);
		int num2 = World.toChunkXZ(centerPos.x + halfSize);
		int num3 = World.toChunkXZ(centerPos.z - halfSize);
		int num4 = World.toChunkXZ(centerPos.z + halfSize);
		for (int i = num - margin; i <= num2 + margin; i++)
		{
			for (int j = num3 - margin; j <= num4 + margin; j++)
			{
				ChunkProtectionLevel chunkProtectionLevel = ((i < num || i > num2 || j < num3 || j > num4) ? marginProtectionLevel : innerProtectionLevel);
				long num5 = WorldChunkCache.MakeChunkKey(i, j);
				this.<UpdateChunkProtectionLevels>g__AddProtectionLevel|95_3(num5, chunkProtectionLevel);
			}
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <UpdateChunkProtectionLevels>g__AddProtectionLevel|95_3(long chunkKey, ChunkProtectionLevel protectionLevel)
	{
		ChunkProtectionLevel chunkProtectionLevel;
		if (this.chunkProtectionLevels.TryGetValue(chunkKey, out chunkProtectionLevel))
		{
			if (protectionLevel == (protectionLevel & chunkProtectionLevel))
			{
				return;
			}
			this.chunkProtectionLevels[chunkKey] = chunkProtectionLevel | protectionLevel;
		}
		else
		{
			this.chunkProtectionLevels[chunkKey] = protectionLevel;
		}
		LongSetGroups.Group group;
		if (this.chunkGroups.TryGetGroup(chunkKey, out group))
		{
			ChunkProtectionLevel chunkProtectionLevel2;
			if (this.groupProtectionLevels.TryGetValue(group, out chunkProtectionLevel2))
			{
				this.groupProtectionLevels[group] = chunkProtectionLevel2 | protectionLevel;
				return;
			}
			this.groupProtectionLevels[group] = protectionLevel;
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <RemoveChunks>g__RemoveChunk|116_0(long _key, bool _resetDecos)
	{
		using (RegionFileManager.s_RemoveChunk.Auto())
		{
			Dictionary<long, IRegionFileChunkSnapshot> dictionary = this.chunkMemoryStreamsToSave;
			lock (dictionary)
			{
				this.chunkMemoryStreamsToSave.Remove(_key);
			}
			Dictionary<long, Chunk> dictionary2 = this.chunksToSave;
			lock (dictionary2)
			{
				this.chunksToSave.Remove(_key);
			}
			int num = WorldChunkCache.extractX(_key);
			int num2 = WorldChunkCache.extractZ(_key);
			this.regionFileAccess.Remove(this.saveDirectory, num, num2);
			this.RemoveChunkSync(_key);
			this.chunksInLoadDir.Remove(_key);
			this.chunksInSaveDir.Remove(_key);
			if (_resetDecos)
			{
				DecoManager.Instance.ResetDecosForWorldChunk(_key);
			}
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static int <SaveChunkAgeDebugTexture>g__OrdinalProtectionLevel|126_0(ChunkProtectionLevel level)
	{
		if (level != ChunkProtectionLevel.None)
		{
			return 1 + (int)Math.Log((double)Convert.ToInt32(level), 2.0);
		}
		return 0;
	}

	public const int cProtectedLandClaimChunkMargin = 1;

	public const int cProtectedBedrollChunkMargin = 1;

	public const int cProtectedOfflinePlayerChunkMargin = 1;

	public const int cProtectedDroppedBackpackChunkMargin = 1;

	public const int cProtectedVehicleChunkMargin = 1;

	public const int cProtectedQuestObjectiveMargin = 1;

	public const int cProtectedSupplyCrateMargin = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_CullExpiredChunks = new ProfilerMarker("RegionFileManager.CullExpiredChunks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_RemoveChunk = new ProfilerMarker("RegionFileManager.RemoveChunk");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_UpdateProtectionLevels = new ProfilerMarker("RegionFileManager.UpdateProtectionLevels");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_SortCullingCandidates = new ProfilerMarker("RegionFileManager.SortCullingCandidates");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_CollectBestCandidates = new ProfilerMarker("RegionFileManager.CollectBestCandidates");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ForceCullChunks = new ProfilerMarker("RegionFileManager.ForceCullChunks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ComputeChunkGroups = new ProfilerMarker("RegionFileManager.ComputeChunkGroups");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_AddGroupedChunks = new ProfilerMarker("RegionFileManager.AddGroupedChunks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_RemoveGroupedChunks = new ProfilerMarker("RegionFileManager.RemoveGroupedChunks");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_UpdateGroupTimestamps = new ProfilerMarker("RegionFileManager.UpdateGrouptimestamps");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_ProcessPersistentDataRemovals = new ProfilerMarker("RegionFileManager.ProcessPendingPersistentDataRemovals");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_CheckPersistentData = new ProfilerMarker("RegionFileManager.CheckPersistentData");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<long> s_RegionDataSize = new ProfilerCounterValue<long>(ProfilerCategory.Scripts, "Saved Region Data Size", 2, 2);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_ProtectedChunkCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Protected Chunks", 3, 2);

	public const string pendingResetsFileName = "PendingResets.7pr";

	public const string cChunkFileExt = ".ttc";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxChunksToCull = 10000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMinimumByteAllowance = 20971520;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long cHeadroomBytes = 5242880L;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IRegionFilePlatformFactory platformFactory = RegionFilePlatform.CreateFactory();

	public static readonly IRegionFileDebugUtil DebugUtil = RegionFileManager.platformFactory.CreateDebugUtil();

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileAccessAbstract regionFileAccess;

	[PublicizedFrom(EAccessModifier.Private)]
	public string saveDirectory;

	[PublicizedFrom(EAccessModifier.Private)]
	public string loadDirectory;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxChunksInCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public long maxChunkAge = -1L;

	[PublicizedFrom(EAccessModifier.Private)]
	public long maxBytes = -1L;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> chunksInLocalCache = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong chunksInLoadDir = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, uint> chunksInSaveDir;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> expiredChunks = new List<long>(10000);

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> resetRequestedChunks = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<long> persistentDataRemovalsRequestedChunks = new HashSet<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool protectionLevelsDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, ChunkProtectionLevel> chunkProtectionLevels = new Dictionary<long, ChunkProtectionLevel>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<KeyValuePair<long, ulong>> sortedCullingCandidates = new List<KeyValuePair<long, ulong>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, HashSetLong> chunksByTraderID = new Dictionary<int, HashSetLong>();

	[PublicizedFrom(EAccessModifier.Private)]
	public LongSetGroups chunkGroups = new LongSetGroups();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<LongSetGroups.Group, uint> groupTimestamps = new Dictionary<LongSetGroups.Group, uint>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool groupTimestampsDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<LongSetGroups.Group, ChunkProtectionLevel> groupProtectionLevels = new Dictionary<LongSetGroups.Group, ChunkProtectionLevel>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<LongSetGroups.Group> processedChunkGroups = new HashSet<LongSetGroups.Group>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<NetPackageDeleteChunkData> pendingChunkDeletionPackages = new List<NetPackageDeleteChunkData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, Chunk> chunksToSave = new Dictionary<long, Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, IRegionFileChunkSnapshot> chunkMemoryStreamsToSave = new Dictionary<long, IRegionFileChunkSnapshot>();

	[PublicizedFrom(EAccessModifier.Private)]
	public IRegionFileChunkSnapshotUtil snapshotUtil;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk> chunksToUnloadLater = new List<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public AutoResetEvent saveThreadWaitHandle = new AutoResetEvent(false);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bSaveOnChunkDrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public long chunkKeyCurrentlySaved = long.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo taskInfoThreadSaveChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance> dynamicPrefabsBuffer = new List<PrefabInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool bSaveRunning = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly RegionFileManager.ProtectedPositionCache ppdPositionCache = new RegionFileManager.ProtectedPositionCache();

	[PublicizedFrom(EAccessModifier.Private)]
	public int thread_SaveChunks_SleepCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object saveLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public class ProtectedPositionCache
	{
		public void ClearAll()
		{
			this.bedrolls.Clear();
			this.lpBlocks.Clear();
			this.offlinePlayers.Clear();
			this.backpacks.Clear();
			this.quests.Clear();
			this.vendingMachines.Clear();
			this.vehicles.Clear();
			this.drones.Clear();
			this.supplyCrates.Clear();
		}

		public readonly List<Vector3i> bedrolls = new List<Vector3i>();

		public readonly List<Vector3i> lpBlocks = new List<Vector3i>();

		public readonly List<Vector3i> offlinePlayers = new List<Vector3i>();

		public readonly List<Vector3i> backpacks = new List<Vector3i>();

		public readonly List<Vector3i> quests = new List<Vector3i>();

		public readonly List<Vector3i> vendingMachines = new List<Vector3i>();

		public readonly List<Vector3i> vehicles = new List<Vector3i>();

		public readonly List<Vector3i> drones = new List<Vector3i>();

		public readonly List<Vector3i> supplyCrates = new List<Vector3i>();
	}
}
