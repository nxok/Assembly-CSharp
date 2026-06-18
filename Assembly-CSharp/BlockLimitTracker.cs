using System;
using System.Collections.Generic;
using System.IO;
using Platform;

public class BlockLimitTracker
{
	public static void Init()
	{
		BlockLimitTracker.instance = new BlockLimitTracker();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			BlockLimitTracker.instance.Load();
		}
	}

	public BlockLimitTracker()
	{
		this.poweredBlockTracker = new BlockTracker(10000);
		this.playerStorageTracker = new BlockTracker(10000);
		this.clientAmounts = new List<int>();
	}

	public bool CanAddBlock(BlockValue _blockValue, Vector3i _blockPosition, out eSetBlockResponse _response)
	{
		_response = eSetBlockResponse.Success;
		if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent() || GameManager.Instance.IsEditMode())
		{
			return true;
		}
		if (_blockValue.Block.isMultiBlock && _blockValue.ischild)
		{
			return true;
		}
		if (_blockValue.Block is BlockPowered || _blockValue.Block is BlockPowerSource)
		{
			if (!this.poweredBlockTracker.CanAdd(_blockPosition))
			{
				_response = eSetBlockResponse.PowerBlockLimitExceeded;
				return false;
			}
		}
		else
		{
			BlockCompositeTileEntity blockCompositeTileEntity = _blockValue.Block as BlockCompositeTileEntity;
			if (blockCompositeTileEntity != null && blockCompositeTileEntity.CompositeData.HasFeature<ITileEntityLootable>() && !this.playerStorageTracker.CanAdd(_blockPosition))
			{
				_response = eSetBlockResponse.StorageBlockLimitExceeded;
				return false;
			}
		}
		return true;
	}

	public void TryAddTrackedBlock(BlockValue _blockValue, Vector3i _blockPosition, int _entityId)
	{
		if (_entityId == -1)
		{
			return;
		}
		if (_blockValue.isair)
		{
			return;
		}
		if (_blockValue.Block.isMultiBlock && _blockValue.ischild)
		{
			return;
		}
		Entity entity = GameManager.Instance.World.GetEntity(_entityId);
		if (entity == null || !(entity is EntityPlayer))
		{
			return;
		}
		Log.Out("TryAddTrackedBlock {0} from entity {1}", new object[]
		{
			_blockValue.Block.GetBlockName(),
			_entityId
		});
		if (_blockValue.Block is BlockPowered || _blockValue.Block is BlockPowerSource)
		{
			if (this.poweredBlockTracker.TryAddBlock(_blockPosition))
			{
				this.TriggerSave();
				Log.Out("{0}/{1} Powered Blocks", new object[]
				{
					this.poweredBlockTracker.blockLocations.Count,
					this.poweredBlockTracker.limit
				});
				return;
			}
		}
		else
		{
			BlockCompositeTileEntity blockCompositeTileEntity = _blockValue.Block as BlockCompositeTileEntity;
			if (blockCompositeTileEntity != null && blockCompositeTileEntity.CompositeData.HasFeature<ITileEntityLootable>() && this.playerStorageTracker.TryAddBlock(_blockPosition))
			{
				this.TriggerSave();
				Log.Out("{0}/{1} Storage Blocks", new object[]
				{
					this.playerStorageTracker.blockLocations.Count,
					this.playerStorageTracker.limit
				});
			}
		}
	}

	public void TryRemoveOrReplaceBlock(BlockValue _oldBlockValue, BlockValue _newBlockValue, Vector3i _blockPosition)
	{
		if (_oldBlockValue.Block.isMultiBlock && _oldBlockValue.ischild)
		{
			return;
		}
		if (_oldBlockValue.Block is BlockPowered || _oldBlockValue.Block is BlockPowerSource)
		{
			if (_newBlockValue.Block is BlockPowered || _newBlockValue.Block is BlockPowerSource)
			{
				return;
			}
			if (this.poweredBlockTracker.RemoveBlock(_blockPosition))
			{
				this.TriggerSave();
				Log.Out("{0}/{1} powered Blocks", new object[]
				{
					this.poweredBlockTracker.blockLocations.Count,
					this.poweredBlockTracker.limit
				});
				return;
			}
		}
		else
		{
			BlockCompositeTileEntity blockCompositeTileEntity = _oldBlockValue.Block as BlockCompositeTileEntity;
			if (blockCompositeTileEntity != null && blockCompositeTileEntity.CompositeData.HasFeature<ITileEntityLootable>())
			{
				BlockCompositeTileEntity blockCompositeTileEntity2 = _newBlockValue.Block as BlockCompositeTileEntity;
				if (blockCompositeTileEntity2 != null && blockCompositeTileEntity2.CompositeData.HasFeature<ITileEntityLootable>())
				{
					return;
				}
				if (this.playerStorageTracker.RemoveBlock(_blockPosition))
				{
					this.TriggerSave();
					Log.Out("{0}/{1} Storage Blocks", new object[]
					{
						this.playerStorageTracker.blockLocations.Count,
						this.playerStorageTracker.limit
					});
				}
			}
		}
	}

	public void ServerUpdateClients()
	{
		if (this.clientAmounts.Count == 0 || this.clientAmounts[0] != this.poweredBlockTracker.blockLocations.Count || this.clientAmounts[1] != this.playerStorageTracker.blockLocations.Count)
		{
			this.clientAmounts.Clear();
			this.clientAmounts.Add(this.poweredBlockTracker.blockLocations.Count);
			this.clientAmounts.Add(this.playerStorageTracker.blockLocations.Count);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageBlockLimitTracking>().Setup(this.clientAmounts), false, -1, -1, -1, null, 192, false);
		}
	}

	public void UpdateClientAmounts(List<int> _amounts)
	{
		if (_amounts.Count != 2)
		{
			Log.Error("Client block limit count not exepcted amount");
			return;
		}
		this.poweredBlockTracker.clientAmount = _amounts[0];
		this.playerStorageTracker.clientAmount = _amounts[1];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearAll()
	{
		this.poweredBlockTracker.Clear();
		this.playerStorageTracker.Clear();
	}

	public static void Cleanup()
	{
		if (BlockLimitTracker.instance != null)
		{
			BlockLimitTracker.instance.Save();
			BlockLimitTracker.instance.ClearAll();
			BlockLimitTracker.instance = null;
		}
	}

	public void Load()
	{
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "blockLimits.dat");
		if (SdFile.Exists(text))
		{
			try
			{
				using (Stream stream = SdFile.OpenRead(text))
				{
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
					{
						pooledBinaryReader.SetBaseStream(stream);
						this.read(pooledBinaryReader);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("BlockLimitTracker Load Exception: " + ex.Message);
				text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "blockLimits.dat.bak");
				if (SdFile.Exists(text))
				{
					using (Stream stream2 = SdFile.OpenRead(text))
					{
						using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
						{
							pooledBinaryReader2.SetBaseStream(stream2);
							this.read(pooledBinaryReader2);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void read(PooledBinaryReader _reader)
	{
		if (_reader.ReadByte() != 1)
		{
			Log.Error("BlockLimitTracker Read bad version");
			return;
		}
		this.ClearAll();
		this.poweredBlockTracker.Read(_reader);
		this.playerStorageTracker.Read(_reader);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TriggerSave()
	{
		if (this.saveThread == null || this.saveThread.HasTerminated())
		{
			this.Save();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Save()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && (this.saveThread == null || !ThreadManager.ActiveThreads.ContainsKey("blockLimitSaveData")))
		{
			PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				this.write(pooledBinaryWriter);
			}
			this.saveThread = ThreadManager.StartThread("blockLimitSaveData", null, new ThreadManager.ThreadFunctionLoopDelegate(this.SaveThread), null, pooledExpandableMemoryStream, null, false, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int SaveThread(ThreadManager.ThreadInfo _threadInfo)
	{
		PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "blockLimits.dat");
		if (SdFile.Exists(text))
		{
			SdFile.Copy(text, string.Format("{0}/{1}.dat.bak", GameIO.GetSaveGameDir(), "blockLimits"), true);
		}
		pooledExpandableMemoryStream.Position = 0L;
		StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void write(PooledBinaryWriter _writer)
	{
		_writer.Write(1);
		this.poweredBlockTracker.Write(_writer);
		this.playerStorageTracker.Write(_writer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxPowerBlocks = 10000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxPlayerStorageBlocks = 10000;

	public static BlockLimitTracker instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTracker poweredBlockTracker;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTracker playerStorageTracker;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo saveThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> clientAmounts;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cNameKey = "blockLimits";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cThreadKey = "blockLimitSaveData";
}
