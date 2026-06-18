using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class TileEntity : ITileEntity, ILockTarget
{
	public bool UseLocalVersioning()
	{
		if (this.readVersion == -1)
		{
			Log.Error("[TileEntity] read must be called before using this.");
			return false;
		}
		return this.readVersion >= 18;
	}

	public int GetLegacyForkVersion()
	{
		if (this.readVersion == -1)
		{
			Log.Error("[TileEntity] read must be called before using this.");
		}
		return this.readVersion;
	}

	public Vector3i localChunkPos
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.chunkPos;
		}
		set
		{
			this.chunkPos = value;
			this.OnSetLocalChunkPosition();
		}
	}

	public BlockValue blockValue
	{
		get
		{
			return this.chunk.GetBlock(this.localChunkPos);
		}
	}

	public Block block
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Block.list[this.chunk.GetBlockId(this.localChunkPos.x, this.localChunkPos.y, this.localChunkPos.z)];
		}
	}

	public bool IsRemoving { get; set; }

	public event XUiEvent_TileEntityDestroyed Destroyed;

	public List<ITileEntityChangedListener> listeners
	{
		get
		{
			return this._listeners;
		}
	}

	public bool bWaitingForServerResponse
	{
		get
		{
			return this.lockHandleWaitingFor != byte.MaxValue;
		}
	}

	public TileEntity(Chunk _chunk)
	{
		this.chunk = _chunk;
	}

	public virtual TileEntity Clone()
	{
		throw new NotImplementedException("Clone() not implemented yet");
	}

	public virtual void CopyFrom(TileEntity _other)
	{
		throw new NotImplementedException("CopyFrom() not implemented yet");
	}

	public virtual void UpdateTick(World world)
	{
	}

	public abstract TileEntityType GetTileEntityType();

	public virtual void OnSetLocalChunkPosition()
	{
	}

	public Vector3i ToWorldPos()
	{
		if (this.chunk != null)
		{
			return new Vector3i(this.chunk.X * 16, this.chunk.Y * 256, this.chunk.Z * 16) + this.localChunkPos;
		}
		return Vector3i.zero;
	}

	public Vector3 ToWorldCenterPos()
	{
		if (this.chunk != null)
		{
			BlockValue blockNoDamage = this.chunk.GetBlockNoDamage(this.chunkPos.x, this.chunkPos.y, this.chunkPos.z);
			Block block = blockNoDamage.Block;
			Vector3 vector;
			vector.x = (float)(this.chunk.X * 16 + this.chunkPos.x);
			vector.y = (float)(this.chunk.Y * 256 + this.chunkPos.y);
			vector.z = (float)(this.chunk.Z * 16 + this.chunkPos.z);
			if (!block.isMultiBlock)
			{
				vector.x += 0.5f;
				vector.y += 0.5f;
				vector.z += 0.5f;
			}
			else
			{
				BlockShapeModelEntity blockShapeModelEntity = block.shape as BlockShapeModelEntity;
				if (blockShapeModelEntity != null)
				{
					Quaternion rotation = blockShapeModelEntity.GetRotation(blockNoDamage);
					vector += blockShapeModelEntity.GetRotatedOffset(block, rotation);
					vector.x += 0.5f;
					vector.z += 0.5f;
				}
			}
			return vector;
		}
		return Vector3.zero;
	}

	public Chunk GetChunk()
	{
		return this.chunk;
	}

	public void SetChunk(Chunk _chunk)
	{
		this.chunk = _chunk;
	}

	public static TileEntity InstantiateFromRead(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, TileEntityType _type, Chunk _chunk, int[] _blockIdMapping, Func<int, int, int, BlockValue> _getBlock)
	{
		TileEntity tileEntity;
		if (TileEntityLegacyUtils.TryReadLegacyType(_br, _eStreamMode, _type, _getBlock, out tileEntity))
		{
			if (tileEntity != null)
			{
				tileEntity.SetChunk(_chunk);
			}
			return tileEntity;
		}
		switch (_type)
		{
		case TileEntityType.Collector:
			tileEntity = new TileEntityCollector(_chunk);
			goto IL_0103;
		case TileEntityType.VendingMachine:
			tileEntity = new TileEntityVendingMachine(_chunk);
			goto IL_0103;
		case TileEntityType.Forge:
			tileEntity = new TileEntityForge(_chunk);
			goto IL_0103;
		case TileEntityType.Workstation:
			tileEntity = new TileEntityWorkstation(_chunk);
			goto IL_0103;
		case TileEntityType.Powered:
			tileEntity = new TileEntityPoweredBlock(_chunk);
			goto IL_0103;
		case TileEntityType.PowerSource:
			tileEntity = new TileEntityPowerSource(_chunk);
			goto IL_0103;
		case TileEntityType.PowerRangeTrap:
			tileEntity = new TileEntityPoweredRangedTrap(_chunk);
			goto IL_0103;
		case TileEntityType.Light:
			tileEntity = new TileEntityLight(_chunk);
			goto IL_0103;
		case TileEntityType.Trigger:
			tileEntity = new TileEntityPoweredTrigger(_chunk);
			goto IL_0103;
		case TileEntityType.Sleeper:
			tileEntity = new TileEntitySleeper(_chunk);
			goto IL_0103;
		case TileEntityType.PowerMeleeTrap:
			tileEntity = new TileEntityPoweredMeleeTrap(_chunk);
			goto IL_0103;
		case TileEntityType.Composite:
			tileEntity = new TileEntityComposite(_chunk);
			goto IL_0103;
		}
		Log.Warning("Dropping TE with unknown/outdated type: " + _type.ToStringCached<TileEntityType>());
		return null;
		IL_0103:
		TileEntityComposite tileEntityComposite = tileEntity as TileEntityComposite;
		if (tileEntityComposite != null)
		{
			tileEntityComposite.read(_br, _eStreamMode, _blockIdMapping);
			return tileEntityComposite;
		}
		tileEntity.read(_br, _eStreamMode);
		return tileEntity;
	}

	public virtual void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			this.readVersion = (int)_br.ReadUInt16();
			this.chunkPos = StreamUtils.ReadVector3i(_br);
			if (this.readVersion <= 18)
			{
				_br.ReadInt32();
			}
			if (this.readVersion > 1)
			{
				this.heapMapUpdateTime = _br.ReadUInt64();
				this.heapMapLastTime = this.heapMapUpdateTime - AIDirector.GetActivityWorldTimeDelay();
				return;
			}
		}
		else
		{
			this.chunkPos = StreamUtils.ReadVector3i(_br);
		}
	}

	public virtual void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(19);
			StreamUtils.Write(_bw, this.chunkPos);
			_bw.Write(this.heapMapUpdateTime);
			return;
		}
		StreamUtils.Write(_bw, this.chunkPos);
	}

	public override string ToString()
	{
		return string.Format("[TE] " + this.GetTileEntityType().ToStringCached<TileEntityType>() + "/" + this.ToWorldPos().ToString(), Array.Empty<object>());
	}

	public virtual void OnRemove(World world)
	{
		this.OnDestroy();
	}

	public virtual void OnLoad()
	{
	}

	public virtual void OnUnload(World world)
	{
	}

	public virtual void OnReadComplete()
	{
	}

	public void SetDisableModifiedCheck(bool _b)
	{
		this.bDisableModifiedCheck = _b;
	}

	public void SetModified()
	{
		this.setModified();
	}

	public void SetChunkModified()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.chunk != null)
		{
			this.chunk.isModified = true;
		}
	}

	public virtual bool IsActive(World world)
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool IsByWater(World _world, Vector3i _blockPos)
	{
		return _world.IsWater(_blockPos.x, _blockPos.y + 1, _blockPos.z) | _world.IsWater(_blockPos.x + 1, _blockPos.y, _blockPos.z) | _world.IsWater(_blockPos.x - 1, _blockPos.y, _blockPos.z) | _world.IsWater(_blockPos.x, _blockPos.y, _blockPos.z + 1) | _world.IsWater(_blockPos.x, _blockPos.y, _blockPos.z - 1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void emitHeatMapEvent(World world, EnumAIDirectorChunkEvent eventType)
	{
		if (world.worldTime < this.heapMapLastTime)
		{
			this.heapMapUpdateTime = 0UL;
		}
		if (world.worldTime >= this.heapMapUpdateTime && world.aiDirector != null)
		{
			Vector3i vector3i = this.ToWorldPos();
			Block block = world.GetBlock(vector3i).Block;
			if (block != null)
			{
				world.aiDirector.NotifyActivity(eventType, vector3i, block.HeatMapStrength, 720f);
				this.heapMapLastTime = world.worldTime;
				this.heapMapUpdateTime = world.worldTime + AIDirector.GetActivityWorldTimeDelay();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void setModified()
	{
		if (this.bDisableModifiedCheck)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.SetChunkModified();
			Vector3? vector = new Vector3?(this.ToWorldCenterPos());
			if (vector.Value == Vector3.zero)
			{
				vector = null;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTileEntity>().Setup(this, TileEntity.StreamModeWrite.ToClient, byte.MaxValue), true, -1, -1, -1, vector, 192, false);
		}
		else
		{
			byte b = this.handleCounter + 1;
			this.handleCounter = b;
			if (b == 255)
			{
				this.handleCounter = 0;
			}
			this.lockHandleWaitingFor = this.handleCounter;
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageTileEntity>().Setup(this, TileEntity.StreamModeWrite.ToServer, this.lockHandleWaitingFor), false);
		}
		this.NotifyListeners();
		if (GameManager.Instance.IsEditMode() && this.chunk != null && this.block.IsTileEntitySavedInPrefab())
		{
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
	}

	public override int GetHashCode()
	{
		return this.ToWorldPos().GetHashCode() & int.MaxValue;
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj) && obj.GetHashCode() == this.GetHashCode();
	}

	public void NotifyListeners()
	{
		for (int i = 0; i < this.listeners.Count; i++)
		{
			this.listeners[i].OnTileEntityChanged(this);
		}
	}

	public virtual void UpgradeDowngradeFrom(TileEntity _other)
	{
		_other.OnDestroy();
	}

	public virtual void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
	}

	public virtual void SetUserAccessing(bool _bUserAccessing)
	{
		this.bUserAccessing = _bUserAccessing;
	}

	public bool IsUserAccessing()
	{
		return this.bUserAccessing;
	}

	public LockTargetType GetLockTargetType()
	{
		return LockTargetType.TileEntity;
	}

	public virtual LockTargetType LockTargetType
	{
		get
		{
			return LockTargetType.TileEntity;
		}
	}

	public virtual bool IsSharedLock(ushort _channel)
	{
		return false;
	}

	public virtual bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return true;
	}

	public virtual bool CanLockOnServer(int _lockingPlayerId, ILockContext _context, ushort _channel)
	{
		return true;
	}

	public virtual void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
	}

	public virtual void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
	}

	public virtual void OnUnlockedServer(int _unlockingPlayerID, ushort _channel)
	{
	}

	public virtual void SetHandle(byte _handle)
	{
		if (this.lockHandleWaitingFor != 255 && this.lockHandleWaitingFor == _handle)
		{
			this.lockHandleWaitingFor = byte.MaxValue;
		}
	}

	public virtual void OnDestroy()
	{
		if (this.Destroyed != null)
		{
			this.Destroyed(this);
		}
	}

	public virtual void Reset(FastTags<TagGroup.Global> questTags)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 19;

	[PublicizedFrom(EAccessModifier.Private)]
	public int readVersion = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i chunkPos;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Chunk chunk;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ulong heapMapLastTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ulong heapMapUpdateTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bDisableModifiedCheck;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bUserAccessing;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ITileEntityChangedListener> _listeners = new List<ITileEntityChangedListener>();

	[PublicizedFrom(EAccessModifier.Private)]
	public byte handleCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte lockHandleWaitingFor = byte.MaxValue;

	public enum StreamModeRead
	{
		Persistency,
		FromServer,
		FromClient
	}

	public enum StreamModeWrite
	{
		Persistency,
		ToServer,
		ToClient
	}
}
