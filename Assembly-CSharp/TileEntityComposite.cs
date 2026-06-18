using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TileEntityComposite : TileEntity
{
	public bool HasBlockTriggered
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool OverridesPhysicalChecks
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public TileEntityCompositeData TeData
	{
		get
		{
			return this.teData;
		}
	}

	public bool PlayerPlaced
	{
		get
		{
			return this.Owner != null;
		}
	}

	public bool LocalPlayerIsOwner
	{
		get
		{
			return this.Owner != null && this.Owner.Equals(PlatformManager.InternalLocalUserIdentifier);
		}
	}

	public PlatformUserIdentifierAbs Owner
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Composite;
	}

	public TileEntityComposite(Chunk _chunk)
		: base(_chunk)
	{
	}

	public TileEntityComposite(Chunk _chunk, BlockValue _blockValue)
		: base(_chunk)
	{
		this.InitModulesFromBlock(_blockValue.Block as BlockCompositeTileEntity);
		TileEntityLegacyUtils.MigrateLegacyMetadata(this, _blockValue);
	}

	public TileEntityComposite(BlockCompositeTileEntity _block, TileEntityLegacyUtils.LegacyState _state)
		: base(null)
	{
		this.chunkPos = _state.chunkPos;
		this.heapMapLastTime = _state.heapMapLastTime;
		this.heapMapUpdateTime = _state.heapMapUpdateTime;
		this.InitModulesFromBlock(_block);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityComposite(TileEntityComposite _original)
		: base(null)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitModulesFromBlock(BlockCompositeTileEntity _block)
	{
		if (!TileEntityCompositeData.FeaturesByBlock.TryGetValue(_block, out this.teData))
		{
			throw new Exception("Trying to initialize TileEntityComposite for block " + _block.GetBlockName() + " failed, no feature definitions found");
		}
		this.modulesCustomOrder = new ITileEntityFeature[this.teData.Features.Count];
		this.modulesInternalOrder = new ITileEntityFeature[this.teData.Features.Count];
		for (int i = 0; i < this.teData.Features.Count; i++)
		{
			TileEntityFeatureData tileEntityFeatureData = this.teData.Features[i];
			ITileEntityFeature tileEntityFeature = tileEntityFeatureData.InstantiateModule();
			this.modulesCustomOrder[tileEntityFeatureData.CustomOrder] = tileEntityFeature;
			this.modulesInternalOrder[i] = tileEntityFeature;
		}
		for (int j = 0; j < this.teData.Features.Count; j++)
		{
			TileEntityFeatureData tileEntityFeatureData2 = this.teData.Features[j];
			this.modulesInternalOrder[j].Init(this, tileEntityFeatureData2);
		}
		this.PrecomputeCapabilities();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrecomputeCapabilities()
	{
		for (int i = 0; i < this.modulesCustomOrder.Length; i++)
		{
			ITileEntityFeature tileEntityFeature = this.modulesCustomOrder[i];
			this.HasBlockTriggered |= tileEntityFeature is IFeatureTriggerCapability;
			this.OverridesPhysicalChecks |= tileEntityFeature is IFeaturePhysicalCapabilities;
		}
	}

	public override TileEntity Clone()
	{
		TileEntityComposite tileEntityComposite = new TileEntityComposite(this);
		tileEntityComposite.CopyFrom(this);
		return tileEntityComposite;
	}

	public override void CopyFrom(TileEntity _other)
	{
		TileEntityComposite tileEntityComposite = _other as TileEntityComposite;
		if (tileEntityComposite == null)
		{
			Log.Warning(string.Format("TEC.CopyFrom with non TEC ({0})", _other));
			return;
		}
		this.InitModulesFromBlock(tileEntityComposite.TeData.Block);
		this.bUserAccessing = _other.IsUserAccessing();
		this.Owner = tileEntityComposite.Owner;
		ITileEntityFeature[] array = this.modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].CopyFrom(tileEntityComposite);
		}
	}

	public override void UpdateTick(World _world)
	{
		base.UpdateTick(_world);
		ITileEntityFeature[] array = this.modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateTick(_world);
		}
	}

	public void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		ITileEntityFeature[] array = this.modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetBlockEntityData(_blockEntityData);
		}
	}

	public override void OnRemove(World _world)
	{
		ITileEntityFeature[] array = this.modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnRemove(_world);
		}
		base.OnRemove(_world);
	}

	public override void OnLoad()
	{
		ITileEntityFeature[] array = this.modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnLoad();
		}
		base.OnLoad();
	}

	public override void OnUnload(World _world)
	{
		ITileEntityFeature[] array = this.modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnUnload(_world);
		}
		base.OnUnload(_world);
	}

	public override void OnDestroy()
	{
		ITileEntityFeature[] array = this.modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnDestroy();
		}
		base.OnDestroy();
	}

	public void OnBlockTriggered(EntityPlayer _player, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		if (!this.HasBlockTriggered)
		{
			return;
		}
		for (int i = 0; i < this.modulesCustomOrder.Length; i++)
		{
			IFeatureTriggerCapability featureTriggerCapability = this.modulesCustomOrder[i] as IFeatureTriggerCapability;
			if (featureTriggerCapability != null)
			{
				featureTriggerCapability.OnBlockTriggered(_player, _blockPos, _blockValue, _blockChanges, _triggeredBy);
			}
		}
	}

	public IEnumerable<IFeaturePhysicalCapabilities> GetOverridesPhysicalChecksModules()
	{
		if (!this.OverridesPhysicalChecks)
		{
			yield break;
		}
		int num;
		for (int i = 0; i < this.modulesCustomOrder.Length; i = num + 1)
		{
			IFeaturePhysicalCapabilities featurePhysicalCapabilities = this.modulesCustomOrder[i] as IFeaturePhysicalCapabilities;
			if (featurePhysicalCapabilities != null)
			{
				yield return featurePhysicalCapabilities;
			}
			num = i;
		}
		yield break;
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		TileEntityComposite tileEntityComposite = _other as TileEntityComposite;
		if (tileEntityComposite == null)
		{
			return;
		}
		this.Owner = tileEntityComposite.Owner;
		ITileEntityFeature[] array = this.modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpgradeDowngradeFrom(tileEntityComposite);
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		ITileEntityFeature[] array = this.modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ReplacedBy(_bvOld, _bvNew, _teNew);
		}
	}

	public override void Reset(FastTags<TagGroup.Global> _questTags)
	{
		base.Reset(_questTags);
		ITileEntityFeature[] array = this.modulesInternalOrder;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset(_questTags);
		}
	}

	public override bool IsActive(World world)
	{
		return base.IsActive(world);
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		this.Owner = _userIdentifier;
		base.SetModified();
	}

	public string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		ITileEntityFeature[] array = this.modulesCustomOrder;
		for (int i = 0; i < array.Length; i++)
		{
			string activationText = array[i].GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
			if (activationText != null)
			{
				return activationText;
			}
		}
		return null;
	}

	public BlockActivationCommand[] InitBlockActivationCommands()
	{
		List<ValueTuple<BlockActivationCommand, int>> commands = new List<ValueTuple<BlockActivationCommand, int>>();
		int firsts = 100;
		int regular = 200;
		int last = 300;
		Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> <>9__1;
		foreach (ITileEntityFeature tileEntityFeature in this.modulesCustomOrder)
		{
			Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> action;
			if ((action = <>9__1) == null)
			{
				action = (<>9__1 = delegate(BlockActivationCommand _bac, TileEntityComposite.EBlockCommandOrder _order, TileEntityFeatureData _featureData)
				{
					BlockActivationCommand blockActivationCommand = new BlockActivationCommand(_featureData.Name + ":" + _bac.text, _bac.icon, _bac.enabled, _bac.highlighted, null);
					int num2;
					switch (_order)
					{
					case TileEntityComposite.EBlockCommandOrder.First:
					{
						int num = firsts;
						firsts = num + 1;
						num2 = num;
						break;
					}
					case TileEntityComposite.EBlockCommandOrder.Normal:
					{
						int num = regular;
						regular = num + 1;
						num2 = num;
						break;
					}
					case TileEntityComposite.EBlockCommandOrder.Last:
					{
						int num = last;
						last = num + 1;
						num2 = num;
						break;
					}
					default:
						throw new ArgumentOutOfRangeException("_order", _order, null);
					}
					int num3 = num2;
					commands.Add(new ValueTuple<BlockActivationCommand, int>(blockActivationCommand, num3));
				});
			}
			tileEntityFeature.InitBlockActivationCommands(action);
		}
		commands.Sort((ValueTuple<BlockActivationCommand, int> _a, ValueTuple<BlockActivationCommand, int> _b) => _a.Item2.CompareTo(_b.Item2));
		BlockActivationCommand[] array2 = new BlockActivationCommand[commands.Count];
		for (int j = 0; j < commands.Count; j++)
		{
			array2[j] = commands[j].Item1;
		}
		return array2;
	}

	public bool UpdateBlockActivationCommands(BlockActivationCommand[] _commands, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		int frameCount = Time.frameCount;
		if (frameCount <= this.lastUpdateFrameOfBlockActivationCommands)
		{
			return this.lastUpdateHadEnabledCommands;
		}
		this.lastUpdateHadEnabledCommands = false;
		for (int i = 0; i < _commands.Length; i++)
		{
			_commands[i].enabled = true;
		}
		for (int j = 0; j < _commands.Length; j++)
		{
			ReadOnlyMemory<char> readOnlyMemory;
			ReadOnlyMemory<char> readOnlyMemory2;
			if (!TileEntityComposite.SplitFullCommandName(_commands[j].text, out readOnlyMemory, out readOnlyMemory2))
			{
				_commands[j].enabled = false;
			}
			else
			{
				ITileEntityFeature feature = this.GetFeature(readOnlyMemory);
				if (feature == null)
				{
					_commands[j].enabled = false;
				}
				else
				{
					foreach (ITileEntityFeature tileEntityFeature in this.modulesInternalOrder)
					{
						_commands[j].enabled = tileEntityFeature.AllowBlockActivationCommand(feature, readOnlyMemory2.Span, _world, _blockPos, _blockValue, _entityFocusing);
						if (!_commands[j].enabled)
						{
							break;
						}
					}
					this.lastUpdateHadEnabledCommands |= _commands[j].enabled;
				}
			}
		}
		this.lastUpdateFrameOfBlockActivationCommands = frameCount;
		return this.lastUpdateHadEnabledCommands;
	}

	public void OnBlockAdded(Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _owner)
	{
		this.Owner = _owner;
		for (int i = 0; i < this.modulesInternalOrder.Length; i++)
		{
			this.modulesInternalOrder[i].OnAdded(_blockPos, _blockValue);
		}
	}

	public void OnBlockValueChanged(Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		for (int i = 0; i < this.modulesInternalOrder.Length; i++)
		{
			this.modulesInternalOrder[i].OnBlockValueChanged(_blockPos, _oldBlockValue, _newBlockValue);
		}
	}

	public void OnBlockReset(Vector3i _blockPos, BlockValue _blockValue)
	{
		for (int i = 0; i < this.modulesInternalOrder.Length; i++)
		{
			this.modulesInternalOrder[i].OnBlockReset(_blockPos, _blockValue);
		}
	}

	public void OnBlockStartsToFall(Vector3i _blockPos, BlockValue _blockValue)
	{
		for (int i = 0; i < this.modulesInternalOrder.Length; i++)
		{
			this.modulesInternalOrder[i].OnBlockStartsToFall(_blockPos, _blockValue);
		}
	}

	public Block.DestroyedResult OnBlockDestroyedBy(Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		Block.DestroyedResult destroyedResult = Block.DestroyedResult.None;
		for (int i = 0; i < this.modulesCustomOrder.Length; i++)
		{
			Block.DestroyedResult destroyedResult2 = this.modulesCustomOrder[i].OnBlockDestroyedBy(_blockPos, _blockValue, _entityId, _bUseHarvestTool);
			if (destroyedResult == Block.DestroyedResult.None)
			{
				destroyedResult = destroyedResult2;
			}
		}
		return destroyedResult;
	}

	public Block.DestroyedResult OnBlockDestroyedByExplosion(Vector3i _blockPos, BlockValue _blockValue, int _playerThatStartedExpl)
	{
		Block.DestroyedResult destroyedResult = Block.DestroyedResult.None;
		for (int i = 0; i < this.modulesCustomOrder.Length; i++)
		{
			Block.DestroyedResult destroyedResult2 = this.modulesCustomOrder[i].OnBlockDestroyedByExplosion(_blockPos, _blockValue, _playerThatStartedExpl);
			if (destroyedResult == Block.DestroyedResult.None)
			{
				destroyedResult = destroyedResult2;
			}
		}
		return destroyedResult;
	}

	public bool OnBlockActivated(BlockActivationCommand[] _commands, string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		ReadOnlyMemory<char> readOnlyMemory;
		ReadOnlyMemory<char> readOnlyMemory2;
		if (!TileEntityComposite.SplitFullCommandName(_commandName, out readOnlyMemory, out readOnlyMemory2))
		{
			return false;
		}
		ITileEntityFeature feature = this.GetFeature(readOnlyMemory);
		return feature != null && feature.OnBlockActivated(readOnlyMemory2.Span, _world, _blockPos, _blockValue, _player);
	}

	public ITileEntityFeature GetFeature(ReadOnlyMemory<char> _featureName)
	{
		int featureIndex = this.teData.GetFeatureIndex(_featureName);
		if (featureIndex < 0)
		{
			return null;
		}
		return this.modulesInternalOrder[featureIndex];
	}

	public T GetFeature<T>() where T : class
	{
		int featureIndex = this.teData.GetFeatureIndex<T>();
		if (featureIndex < 0)
		{
			return default(T);
		}
		return (T)((object)this.modulesInternalOrder[featureIndex]);
	}

	public static bool SplitFullCommandName(string _fullCommandName, out ReadOnlyMemory<char> _moduleName, out ReadOnlyMemory<char> _commandName)
	{
		int num = _fullCommandName.IndexOf(':');
		if (num < 0)
		{
			_moduleName = default(ReadOnlyMemory<char>);
			_commandName = default(ReadOnlyMemory<char>);
			return false;
		}
		_moduleName = _fullCommandName.AsMemory(0, num);
		_commandName = _fullCommandName.AsMemory(num + 1);
		return true;
	}

	public override string ToString()
	{
		return string.Format(string.Concat(new string[]
		{
			"[TEC] ",
			this.GetTileEntityType().ToStringCached<TileEntityType>(),
			"/",
			base.ToWorldPos().ToString(),
			"/",
			this.teData.Block.GetBlockName()
		}), Array.Empty<object>());
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		this.read(_br, _eStreamMode, null);
	}

	public void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, int[] _blockIdMapping)
	{
		base.read(_br, _eStreamMode);
		int num;
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency)
		{
			num = 18;
		}
		else if (base.UseLocalVersioning())
		{
			num = (int)_br.ReadUInt16();
		}
		else
		{
			num = base.GetLegacyForkVersion();
		}
		PooledBinaryReader.StreamReadSizeMarker streamReadSizeMarker = _br.ReadSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
		int num2 = _br.ReadInt32();
		int num3 = ((_blockIdMapping == null) ? num2 : _blockIdMapping[num2]);
		BlockCompositeTileEntity blockCompositeTileEntity = Block.list[num3] as BlockCompositeTileEntity;
		if (blockCompositeTileEntity == null)
		{
			string text = "TileEntityComposite.read: Failed reading data, block ";
			Block block = Block.list[num3];
			Log.Error(text + ((block != null) ? block.GetBlockName() : null) + " is not a BlockCompositeTileEntity");
			uint num4;
			_br.ValidateSizeMarker(ref streamReadSizeMarker, out num4, true);
			return;
		}
		try
		{
			this.Owner = PlatformUserIdentifierAbs.FromStream(_br, false, false);
			if (this.modulesInternalOrder == null || this.modulesCustomOrder == null || this.teData == null)
			{
				this.InitModulesFromBlock(blockCompositeTileEntity);
			}
			byte b = _br.ReadByte();
			if (num < 17)
			{
				if ((int)b != this.modulesInternalOrder.Length)
				{
					Log.Warning(string.Format("{0}.{1}: Legacy composite TE (ver {2}) for block \"{3}\" ", new object[]
					{
						"TileEntityComposite",
						"read",
						num,
						blockCompositeTileEntity.GetBlockName()
					}) + string.Format("has {0} features in stream but current definition has {1}. Skipping TE payload.", b, this.modulesInternalOrder.Length));
					uint num4;
					_br.ValidateSizeMarker(ref streamReadSizeMarker, out num4, true);
					return;
				}
				for (int i = 0; i < (int)b; i++)
				{
					ITileEntityFeature tileEntityFeature = this.modulesInternalOrder[i];
					int num5 = _br.ReadInt32();
					if (tileEntityFeature.FeatureData.NameHash != num5)
					{
						Log.Warning(string.Format("{0}.{1}: Legacy composite TE (ver {2}) for block \"{3}\" ", new object[]
						{
							"TileEntityComposite",
							"read",
							num,
							blockCompositeTileEntity.GetBlockName()
						}) + string.Format("hash mismatch at index {0}: stream 0x{1:X8} != expected 0x{2:X8} ({3}). Skipping TE payload.", new object[]
						{
							i,
							num5,
							tileEntityFeature.FeatureData.NameHash,
							tileEntityFeature.FeatureData.Name
						}));
						uint num4;
						_br.ValidateSizeMarker(ref streamReadSizeMarker, out num4, true);
						return;
					}
					tileEntityFeature.Read(_br, _eStreamMode);
				}
				uint num6;
				if (!_br.ValidateSizeMarker(ref streamReadSizeMarker, out num6, true))
				{
					Log.Error("TileEntityComposite.read: Legacy composite TE for block \"" + blockCompositeTileEntity.GetBlockName() + "\" failed size validation: " + string.Format("expected {0} B, read {1} B", streamReadSizeMarker.ExpectedSize, num6));
				}
				return;
			}
			else
			{
				HashSet<int> hashSet = new HashSet<int>((int)b);
				for (int j = 0; j < (int)b; j++)
				{
					int num7 = _br.ReadInt32();
					hashSet.Add(num7);
					PooledBinaryReader.StreamReadSizeMarker streamReadSizeMarker2 = _br.ReadSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
					int featureIndex = this.teData.GetFeatureIndex(num7);
					if (featureIndex < 0)
					{
						Log.Warning(string.Format("{0}.{1}: Block \"{2}\" no longer defines feature hash 0x{3:X8}. Skipping payload.", new object[]
						{
							"TileEntityComposite",
							"read",
							blockCompositeTileEntity.GetBlockName(),
							num7
						}));
						uint num4;
						_br.ValidateSizeMarker(ref streamReadSizeMarker2, out num4, true);
					}
					else
					{
						ITileEntityFeature tileEntityFeature2 = this.modulesInternalOrder[featureIndex];
						try
						{
							tileEntityFeature2.Read(_br, _eStreamMode);
						}
						catch (Exception ex)
						{
							Log.Error(string.Format("{0}.{1}: Module read failed for feature \"{2}\" (hash 0x{3:X8}) ", new object[]
							{
								"TileEntityComposite",
								"read",
								tileEntityFeature2.FeatureData.Name,
								num7
							}) + "on block \"" + blockCompositeTileEntity.GetBlockName() + "\": Caught exception:");
							Log.Exception(ex);
						}
						uint num8;
						if (!_br.ValidateSizeMarker(ref streamReadSizeMarker2, out num8, true))
						{
							Log.Error(string.Format("{0}.{1}: Module \"{2}\" failed size validation (hash 0x{3:X8}) ", new object[]
							{
								"TileEntityComposite",
								"read",
								tileEntityFeature2.FeatureData.Name,
								num7
							}) + string.Format("on block \"{0}\": expected {1} B, read {2} B", blockCompositeTileEntity.GetBlockName(), streamReadSizeMarker2.ExpectedSize, num8));
						}
					}
				}
				int num9 = 0;
				for (int k = 0; k < this.teData.Features.Count; k++)
				{
					int nameHash = this.teData.Features[k].NameHash;
					if (!hashSet.Contains(nameHash))
					{
						num9++;
					}
				}
				if (num9 > 0)
				{
					Log.Warning(string.Format("{0}.{1}: Block \"{2}\" has {3} feature(s) in the current definition ", new object[]
					{
						"TileEntityComposite",
						"read",
						blockCompositeTileEntity.GetBlockName(),
						num9
					}) + "that were not present in the saved data. They will use default values.");
				}
			}
		}
		catch (Exception ex2)
		{
			Log.Error("TileEntityComposite.read: Failed reading data for block " + Block.list[num3].GetBlockName() + ": Caught exception:");
			Log.Exception(ex2);
			uint num4;
			_br.ValidateSizeMarker(ref streamReadSizeMarker, out num4, true);
			return;
		}
		uint num10;
		if (!_br.ValidateSizeMarker(ref streamReadSizeMarker, out num10, true))
		{
			Log.Error("TileEntityComposite.read: Failed reading data for block \"" + Block.list[num3].GetBlockName() + "\": " + string.Format("Data received {0} B, read {1} B", streamReadSizeMarker.ExpectedSize, num10));
		}
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(18);
		}
		PooledBinaryWriter.StreamWriteSizeMarker streamWriteSizeMarker = _bw.ReserveSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
		_bw.Write(this.teData.Block.blockID);
		(GameManager.Instance.IsEditMode() ? null : this.Owner).ToStream(_bw, false);
		_bw.Write((byte)this.modulesInternalOrder.Length);
		for (int i = 0; i < this.modulesInternalOrder.Length; i++)
		{
			ITileEntityFeature tileEntityFeature = this.modulesInternalOrder[i];
			_bw.Write(tileEntityFeature.FeatureData.NameHash);
			PooledBinaryWriter.StreamWriteSizeMarker streamWriteSizeMarker2 = _bw.ReserveSizeMarker(PooledBinaryWriter.EMarkerSize.UInt32);
			tileEntityFeature.Write(_bw, _eStreamMode);
			_bw.FinalizeSizeMarker(ref streamWriteSizeMarker2);
		}
		_bw.FinalizeSizeMarker(ref streamWriteSizeMarker);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCompositeData teData;

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityFeature[] modulesCustomOrder;

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityFeature[] modulesInternalOrder;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastUpdateFrameOfBlockActivationCommands;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastUpdateHadEnabledCommands;

	[PublicizedFrom(EAccessModifier.Private)]
	public const char ModuleCommandSeparator = ':';

	public enum EBlockCommandOrder
	{
		First,
		Normal,
		Last
	}
}
