using System;
using System.Collections.Generic;
using Unity.Mathematics;

public class WaterSimulationApplyChanges
{
	public WaterSimulationApplyChanges(ChunkCluster _cc)
	{
		this.chunks = _cc;
		this.isServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		this.applyThread = ThreadManager.StartThread("WaterSimulationApplyChanges", null, new ThreadManager.ThreadFunctionLoopDelegate(this.ThreadLoop), null, null, null, true, false);
	}

	public WaterSimulationApplyChanges.ChangesForChunk.Writer GetChangeWriter(long _chunkKey)
	{
		Dictionary<long, WaterSimulationApplyChanges.ChangesForChunk> dictionary = this.changeCache;
		WaterSimulationApplyChanges.ChangesForChunk.Writer writer;
		lock (dictionary)
		{
			WaterSimulationApplyChanges.ChangesForChunk changesForChunk;
			if (!this.changeCache.TryGetValue(_chunkKey, out changesForChunk))
			{
				changesForChunk = this.changesPool.AllocSync(true);
				this.changeCache.Add(_chunkKey, changesForChunk);
				this.changedChunkList.AddLast(_chunkKey);
			}
			writer = new WaterSimulationApplyChanges.ChangesForChunk.Writer(changesForChunk);
		}
		return writer;
	}

	public void DiscardChangesForChunks(List<long> _chunkKeys)
	{
		Dictionary<long, WaterSimulationApplyChanges.ChangesForChunk> dictionary = this.changeCache;
		lock (dictionary)
		{
			foreach (long num in _chunkKeys)
			{
				WaterSimulationApplyChanges.ChangesForChunk changesForChunk;
				if (this.changeCache.TryGetValue(num, out changesForChunk))
				{
					this.changesPool.FreeSync(changesForChunk);
					this.changeCache.Remove(num);
					this.changedChunkList.Remove(num);
					Log.Out(string.Format("[DiscardChangesForChunks] Discarding pending water changes for chunk: {0}", num));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ThreadLoop(ThreadManager.ThreadInfo _threadInfo)
	{
		if (_threadInfo.TerminationRequested())
		{
			return -1;
		}
		if (this.isServer && this.networkMaxBytesPerSecond > 0L)
		{
			NetPackageMeasure netPackageMeasure = this.networkMeasure;
			lock (netPackageMeasure)
			{
				this.networkMeasure.RecalculateTotals();
			}
		}
		Chunk chunk;
		WaterSimulationApplyChanges.ChangesForChunk changesForChunk;
		if (!this.TryFindChangeToApply(out chunk, out changesForChunk))
		{
			return 15;
		}
		this.ApplyChanges(chunk, changesForChunk.changedVoxels);
		chunk.EnterWriteLock();
		chunk.InProgressWaterSim = false;
		chunk.ExitWriteLock();
		this.changesPool.FreeSync(changesForChunk);
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryFindChangeToApply(out Chunk _chunk, out WaterSimulationApplyChanges.ChangesForChunk _changes)
	{
		Dictionary<long, WaterSimulationApplyChanges.ChangesForChunk> dictionary = this.changeCache;
		bool flag2;
		lock (dictionary)
		{
			if (this.changedChunkList.Count == 0)
			{
				_chunk = null;
				_changes = null;
				flag2 = false;
			}
			else
			{
				LinkedListNode<long> linkedListNode = this.changedChunkList.First;
				while (linkedListNode != null)
				{
					long value = linkedListNode.Value;
					if (!this.changeCache.TryGetValue(value, out _changes))
					{
						LinkedListNode<long> linkedListNode2 = linkedListNode;
						linkedListNode = linkedListNode.Next;
						this.changedChunkList.Remove(linkedListNode2);
					}
					else if (_changes.IsRecordingChanges)
					{
						linkedListNode = linkedListNode.Next;
					}
					else
					{
						if (WaterUtils.TryOpenChunkForUpdate(this.chunks, value, out _chunk))
						{
							LinkedListNode<long> linkedListNode3 = linkedListNode;
							linkedListNode = linkedListNode.Next;
							this.changedChunkList.Remove(linkedListNode3);
							this.changeCache.Remove(value);
							return true;
						}
						linkedListNode = linkedListNode.Next;
					}
				}
				_chunk = null;
				_changes = null;
				flag2 = false;
			}
		}
		return flag2;
	}

	public void ApplyChanges(Chunk _chunk, Dictionary<int, WaterValue> changedVoxels)
	{
		NetPackageWaterSimChunkUpdate netPackageWaterSimChunkUpdate = null;
		if (this.isServer && SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0)
		{
			netPackageWaterSimChunkUpdate = this.SetupForSend(_chunk);
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		foreach (KeyValuePair<int, WaterValue> keyValuePair in changedVoxels)
		{
			int key = keyValuePair.Key;
			int3 voxelCoords = WaterDataHandle.GetVoxelCoords(key);
			WaterValue value = keyValuePair.Value;
			WaterValue waterValue;
			_chunk.SetWaterSimUpdate(voxelCoords.x, voxelCoords.y, voxelCoords.z, value, out waterValue);
			if (value.GetMass() != waterValue.GetMass())
			{
				if (WaterUtils.GetWaterLevel(waterValue) != WaterUtils.GetWaterLevel(value))
				{
					int num7 = voxelCoords.y >> 4;
					int num8 = 1 << num7;
					if ((voxelCoords.y & 3) == 3 && voxelCoords.y < 255)
					{
						num8 |= 2 << num7;
					}
					num2 |= num8;
					if (voxelCoords.x == 0)
					{
						num3 |= num8;
					}
					else if (voxelCoords.x == 15)
					{
						num4 |= num8;
					}
					if (voxelCoords.z == 0)
					{
						num5 |= num8;
					}
					else if (voxelCoords.z == 15)
					{
						num6 |= num8;
					}
				}
				if (netPackageWaterSimChunkUpdate != null)
				{
					netPackageWaterSimChunkUpdate.AddChange((ushort)key, value);
				}
			}
		}
		if (netPackageWaterSimChunkUpdate != null)
		{
			netPackageWaterSimChunkUpdate.FinalizeSend();
			num += this.SendUpdateToClients(netPackageWaterSimChunkUpdate);
		}
		if (num2 != 0)
		{
			_chunk.NeedsRegenerationOrBits(num2);
		}
		if (num3 != 0)
		{
			Chunk chunkSync = this.chunks.GetChunkSync(_chunk.X - 1, _chunk.Z);
			if (chunkSync != null)
			{
				chunkSync.NeedsRegenerationOrBits(num3);
			}
		}
		if (num4 != 0)
		{
			Chunk chunkSync2 = this.chunks.GetChunkSync(_chunk.X + 1, _chunk.Z);
			if (chunkSync2 != null)
			{
				chunkSync2.NeedsRegenerationOrBits(num4);
			}
		}
		if (num5 != 0)
		{
			Chunk chunkSync3 = this.chunks.GetChunkSync(_chunk.X, _chunk.Z - 1);
			if (chunkSync3 != null)
			{
				chunkSync3.NeedsRegenerationOrBits(num5);
			}
		}
		if (num6 != 0)
		{
			Chunk chunkSync4 = this.chunks.GetChunkSync(_chunk.X, _chunk.Z + 1);
			if (chunkSync4 != null)
			{
				chunkSync4.NeedsRegenerationOrBits(num6);
			}
		}
		if (num > 0)
		{
			if (this.networkMaxBytesPerSecond > 0L)
			{
				NetPackageMeasure netPackageMeasure = this.networkMeasure;
				lock (netPackageMeasure)
				{
					this.networkMeasure.AddSample((long)num);
				}
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.FlushClientSendQueues();
		}
	}

	public bool HasNetWorkLimitBeenReached()
	{
		if (this.isServer && this.networkMaxBytesPerSecond > 0L && SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0)
		{
			NetPackageMeasure netPackageMeasure = this.networkMeasure;
			lock (netPackageMeasure)
			{
				return this.networkMeasure.totalSent > this.networkMaxBytesPerSecond;
			}
			return false;
		}
		return false;
	}

	public NetPackageWaterSimChunkUpdate SetupForSend(Chunk _chunk)
	{
		this.clientsNearChunkBuffer.Clear();
		long key = _chunk.Key;
		foreach (ClientInfo clientInfo in SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List)
		{
			EntityPlayer entityPlayer;
			if (GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out entityPlayer) && entityPlayer.ChunkObserver.chunksAround.Contains(key))
			{
				this.clientsNearChunkBuffer.Add(clientInfo);
			}
		}
		NetPackageWaterSimChunkUpdate package = NetPackageManager.GetPackage<NetPackageWaterSimChunkUpdate>();
		package.SetupForSend(_chunk);
		return package;
	}

	public int SendUpdateToClients(NetPackageWaterSimChunkUpdate _package)
	{
		int num = 0;
		_package.RegisterSendQueue();
		foreach (ClientInfo clientInfo in this.clientsNearChunkBuffer)
		{
			clientInfo.SendPackage(_package);
			num += _package.GetLength();
		}
		_package.SendQueueHandled();
		return num;
	}

	public void Cleanup()
	{
		this.applyThread.WaitForEnd(30);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryPooledObject<WaterSimulationApplyChanges.ChangesForChunk> changesPool = new MemoryPooledObject<WaterSimulationApplyChanges.ChangesForChunk>(300);

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCluster chunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo applyThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int noWorkPauseDurationMs = 15;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, WaterSimulationApplyChanges.ChangesForChunk> changeCache = new Dictionary<long, WaterSimulationApplyChanges.ChangesForChunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedList<long> changedChunkList = new LinkedList<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageMeasure networkMeasure = new NetPackageMeasure(1.0);

	public long networkMaxBytesPerSecond = 524288L;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ClientInfo> clientsNearChunkBuffer = new List<ClientInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isServer;

	public class ChangesForChunk : IMemoryPoolableObject
	{
		public bool IsRecordingChanges
		{
			get
			{
				bool flag2;
				lock (this)
				{
					flag2 = this.isRecordingChanges;
				}
				return flag2;
			}
		}

		public void Cleanup()
		{
			this.Reset();
		}

		public void Reset()
		{
			this.isRecordingChanges = false;
			this.changedVoxels.Clear();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isRecordingChanges;

		public Dictionary<int, WaterValue> changedVoxels = new Dictionary<int, WaterValue>();

		public struct Writer : IDisposable
		{
			public Writer(WaterSimulationApplyChanges.ChangesForChunk _changes)
			{
				lock (_changes)
				{
					_changes.isRecordingChanges = true;
				}
				this.changes = _changes;
			}

			public void RecordChange(int _voxelIndex, WaterValue _waterValue)
			{
				this.changes.changedVoxels[_voxelIndex] = _waterValue;
			}

			public void Dispose()
			{
				WaterSimulationApplyChanges.ChangesForChunk changesForChunk = this.changes;
				lock (changesForChunk)
				{
					this.changes.isRecordingChanges = false;
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public WaterSimulationApplyChanges.ChangesForChunk changes;
		}
	}
}
