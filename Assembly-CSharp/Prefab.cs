using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using PrefabVolumes;
using UniLinq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Prefab : INeighborBlockCache, IChunkAccess
{
	public string PrefabName
	{
		get
		{
			return this.location.FileNameNoExtension ?? "";
		}
	}

	public string LocalizedName
	{
		get
		{
			return Localization.Get(this.PrefabName, false, null);
		}
	}

	public string LocalizedEnglishName
	{
		get
		{
			return Localization.Get(this.PrefabName, false, "english");
		}
	}

	public float DensityScore
	{
		get
		{
			if (this.renderingCost != null)
			{
				return (float)this.renderingCost.TotalVertices / 100000f;
			}
			return 0f;
		}
	}

	public WorldStats RenderingCostStats
	{
		get
		{
			return this.renderingCost;
		}
		set
		{
			this.renderingCost = value;
		}
	}

	public FastTags<TagGroup.Poi> Tags
	{
		get
		{
			return this.tags;
		}
		set
		{
			this.tags = value;
		}
	}

	public FastTags<TagGroup.Poi> ThemeTags
	{
		get
		{
			return this.themeTags;
		}
		set
		{
			this.themeTags = value;
		}
	}

	public int ThemeRepeatDistance
	{
		get
		{
			return this.themeRepeatDistance;
		}
		set
		{
			this.themeRepeatDistance = value;
		}
	}

	public int DuplicateRepeatDistance
	{
		get
		{
			return this.duplicateRepeatDistance;
		}
		set
		{
			this.duplicateRepeatDistance = value;
		}
	}

	public static PathAbstractions.AbstractedLocation LocationForNewPrefab(string _name, string _prefabsSubfolder = null)
	{
		Mod mod = null;
		PathAbstractions.EAbstractedLocationType eabstractedLocationType = PathAbstractions.EAbstractedLocationType.UserDataPath;
		string launchArgument = GameUtils.GetLaunchArgument("newprefabsmod");
		if (!string.IsNullOrEmpty(launchArgument))
		{
			mod = ModManager.GetMod(launchArgument, true);
			if (mod != null)
			{
				eabstractedLocationType = PathAbstractions.EAbstractedLocationType.Mods;
			}
			else
			{
				Log.Warning("Argument -newprefabsmod given but mod with name '" + launchArgument + "' not found, ignoring!");
			}
		}
		return PathAbstractions.PrefabsSearchPaths.BuildLocation(eabstractedLocationType, _prefabsSubfolder, _name, mod, null).Value;
	}

	public static bool CanSaveIn(PathAbstractions.AbstractedLocation _location)
	{
		return _location.Type != PathAbstractions.EAbstractedLocationType.GameData;
	}

	public Prefab()
	{
		this.SleeperVolumeList = new PrefabSleeperVolumeList(this);
		this.TeleportVolumeList = new PrefabTeleportVolumeList(this);
		this.InfoVolumeList = new PrefabInfoVolumeList(this);
		this.WallVolumeList = new PrefabWallVolumeList(this);
		this.TriggerVolumeList = new PrefabTriggerVolumeList(this);
		this.MarkerVolumeList = new PrefabMarkerVolumeList(this);
		this.AllVolumeLists = new List<PrefabVolumeListAbs> { this.SleeperVolumeList, this.TeleportVolumeList, this.InfoVolumeList, this.WallVolumeList, this.TriggerVolumeList, this.MarkerVolumeList };
		foreach (PrefabVolumeListAbs prefabVolumeListAbs in this.AllVolumeLists)
		{
			this.AllVolumeListsByType[prefabVolumeListAbs.VolumeType] = prefabVolumeListAbs;
		}
	}

	public Prefab(Prefab _other, bool _sharedData = false)
		: this()
	{
		this.size = _other.size;
		if (_sharedData)
		{
			this.isCellsDataOwner = false;
			this.blockCells = _other.blockCells;
			this.damageCells = _other.damageCells;
			this.densityCells = _other.densityCells;
			this.textureCells = _other.textureCells;
			this.waterCells = _other.waterCells;
		}
		else
		{
			this.blockCells = _other.blockCells.Clone();
			this.damageCells = _other.damageCells.Clone();
			this.densityCells = _other.densityCells.Clone();
			this.textureCells = _other.textureCells.Clone();
			this.waterCells = _other.waterCells.Clone();
		}
		if (_sharedData)
		{
			this.multiBlockParentIndices = _other.multiBlockParentIndices;
			this.decoAllowedBlockIndices = _other.decoAllowedBlockIndices;
		}
		else
		{
			this.multiBlockParentIndices = new List<int>(_other.multiBlockParentIndices);
			this.decoAllowedBlockIndices = new List<int>(_other.decoAllowedBlockIndices);
		}
		this.location = _other.location;
		this.bCopyAirBlocks = _other.bCopyAirBlocks;
		this.bExcludeDistantPOIMesh = _other.bExcludeDistantPOIMesh;
		this.bExcludePOICulling = _other.bExcludePOICulling;
		this.distantPOIYOffset = _other.distantPOIYOffset;
		this.distantPOIOverride = _other.distantPOIOverride;
		this.bAllowTopSoilDecorations = _other.bAllowTopSoilDecorations;
		this.bTraderArea = _other.bTraderArea;
		this.TraderAreaProtect = _other.TraderAreaProtect;
		for (int i = 0; i < this.AllVolumeLists.Count; i++)
		{
			this.AllVolumeLists[i].CopyFrom(_other.AllVolumeLists[i]);
		}
		this.yOffset = _other.yOffset;
		this.rotationToFaceNorth = _other.rotationToFaceNorth;
		this.allowedBiomes = new List<string>(_other.allowedBiomes);
		this.allowedTownships = new List<string>(_other.allowedTownships);
		this.allowedZones = new List<string>(_other.allowedZones);
		this.tags = new FastTags<TagGroup.Poi>(_other.tags);
		this.themeTags = new FastTags<TagGroup.Poi>(_other.themeTags);
		this.themeRepeatDistance = _other.themeRepeatDistance;
		this.duplicateRepeatDistance = _other.duplicateRepeatDistance;
		this.questTags = _other.questTags;
		this.DifficultyTier = _other.DifficultyTier;
		this.ShowQuestClearCount = _other.ShowQuestClearCount;
		this.localRotation = _other.localRotation;
		for (int j = 0; j < _other.entities.Count; j++)
		{
			EntityCreationData entityCreationData = _other.entities[j];
			this.entities.Add(entityCreationData.Clone());
		}
		foreach (KeyValuePair<Vector3i, TileEntity> keyValuePair in _other.tileEntities)
		{
			this.tileEntities.Add(keyValuePair.Key, keyValuePair.Value);
		}
		this.insidePos = _other.insidePos.Clone();
		foreach (KeyValuePair<Vector3i, BlockTrigger> keyValuePair2 in _other.triggerData)
		{
			this.triggerData.Add(keyValuePair2.Key, keyValuePair2.Value);
		}
		for (int k = 0; k < _other.TriggerLayers.Count; k++)
		{
			this.TriggerLayers.Add(_other.TriggerLayers[k]);
		}
		this.renderingCost = _other.renderingCost;
	}

	public Prefab(Vector3i _size)
		: this()
	{
		this.size = _size;
		this.localRotation = 0;
		this.InitData();
	}

	public int EstimateOwnedBytes()
	{
		int num = 0;
		if (this.isCellsDataOwner)
		{
			num += IntPtr.Size;
			if (this.blockCells != null)
			{
				int num2;
				int num3;
				int num4;
				int num5;
				int num6;
				this.blockCells.Stats(out num2, out num3, out num4, out num5, out num6);
				num += num5 + num2 * IntPtr.Size;
			}
			num += IntPtr.Size;
			if (this.damageCells != null)
			{
				int num2;
				int num3;
				int num4;
				int num5;
				int num6;
				this.damageCells.Stats(out num2, out num3, out num4, out num5, out num6);
				num += num5 + num2 * IntPtr.Size;
			}
			num += IntPtr.Size;
			if (this.densityCells != null)
			{
				int num2;
				int num3;
				int num4;
				int num5;
				int num6;
				this.densityCells.Stats(out num2, out num3, out num4, out num5, out num6);
				num += num5 + num2 * IntPtr.Size;
			}
			num += IntPtr.Size;
			if (this.textureCells != null)
			{
				int num2;
				int num3;
				int num4;
				int num5;
				int num6;
				this.textureCells.Stats(out num2, out num3, out num4, out num5, out num6);
				num += num5 + num2 * IntPtr.Size;
			}
			num += IntPtr.Size;
			if (this.waterCells != null)
			{
				int num2;
				int num3;
				int num4;
				int num5;
				int num6;
				this.waterCells.Stats(out num2, out num3, out num4, out num5, out num6);
				num += num5 + num2 * IntPtr.Size;
			}
		}
		return num + MemoryTracker.GetSize<byte>(this.TriggerLayers);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitData()
	{
		if (!this.isCellsDataOwner)
		{
			Log.Error("InitData failed: Cannot set block data on non-owning Prefab instance.");
			return;
		}
		this.blockCells = new Prefab.Cells<uint>(this.size.y, 0U);
		this.damageCells = new Prefab.Cells<ushort>(this.size.y, 0);
		this.densityCells = new Prefab.Cells<sbyte>(this.size.y, MarchingCubes.DensityAir);
		this.textureCells = new Prefab.Cells<TextureFullArray>(this.size.y, TextureFullArray.Default);
		this.waterCells = new Prefab.Cells<WaterValue>(this.size.y, WaterValue.Empty);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitTerrainFillers()
	{
		this.terrainFillerType = Block.GetBlockValue(Constants.cTerrainFillerBlockName, false).type;
		this.terrainFiller2Type = Block.GetBlockValue(Constants.cTerrainFiller2BlockName, false).type;
	}

	public Prefab Clone(bool _sharedData = false)
	{
		return new Prefab(this, _sharedData);
	}

	public int GetLocalRotation()
	{
		return this.localRotation;
	}

	public void SetLocalRotation(int _rot)
	{
		this.localRotation = _rot;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CoordToOffset(int _localRotation, int _x, int _y, int _z)
	{
		int num;
		switch (_localRotation)
		{
		default:
			num = _x + _y * this.size.x + _z * this.size.x * this.size.y;
			break;
		case 1:
			num = _z + _y * this.size.z + (this.size.x - _x - 1) * this.size.z * this.size.y;
			break;
		case 2:
			num = this.size.x - _x - 1 + _y * this.size.x + (this.size.z - _z - 1) * this.size.x * this.size.y;
			break;
		case 3:
			num = this.size.z - _z - 1 + _y * this.size.z + _x * this.size.z * this.size.y;
			break;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void offsetToCoord(int _offset, out int _x, out int _y, out int _z)
	{
		int num = this.size.x * this.size.y;
		_z = _offset / num;
		_offset %= num;
		_y = _offset / this.size.x;
		_x = _offset % this.size.x;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void offsetToCoordRotated(int _offset, out int _x, out int _y, out int _z)
	{
		switch (this.localRotation)
		{
		default:
		{
			int num = this.size.x * this.size.y;
			_z = _offset / num;
			_offset %= num;
			_y = _offset / this.size.x;
			_x = _offset % this.size.x;
			return;
		}
		case 1:
			_x = -(_offset / (this.size.z * this.size.y) - this.size.x + 1);
			_offset %= this.size.z * this.size.y;
			_y = _offset / this.size.z;
			_z = _offset % this.size.z;
			return;
		case 2:
			_z = -(_offset / (this.size.x * this.size.y) - this.size.z + 1);
			_offset %= this.size.x * this.size.y;
			_y = _offset / this.size.x;
			_offset %= this.size.x;
			_x = -(_offset - this.size.x + 1);
			return;
		case 3:
			_x = _offset / (this.size.z * this.size.y);
			_offset %= this.size.z * this.size.y;
			_y = _offset / this.size.z;
			_offset %= this.size.z;
			_z = -(_offset - this.size.z + 1);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotateCoords(ref int _x, ref int _z)
	{
		switch (this.localRotation)
		{
		case 1:
		{
			int num = _x;
			_x = _z;
			_z = this.size.x - num - 1;
			return;
		}
		case 2:
			_x = this.size.x - _x - 1;
			_z = this.size.z - _z - 1;
			return;
		case 3:
		{
			int num = _x;
			_x = this.size.z - _z - 1;
			_z = num;
			return;
		}
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotateCoords(int _rot, ref int _x, ref int _z)
	{
		switch (_rot)
		{
		case 1:
		{
			int num = _x;
			_x = _z;
			_z = this.size.x - num - 1;
			return;
		}
		case 2:
			_x = this.size.x - _x - 1;
			_z = this.size.z - _z - 1;
			return;
		case 3:
		{
			int num = _x;
			_x = this.size.z - _z - 1;
			_z = num;
			return;
		}
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InverseRotateRelative(ref int _x, ref int _z)
	{
		switch (this.localRotation)
		{
		case 1:
		{
			int num = _x;
			_x = -_z;
			_z = num;
			return;
		}
		case 2:
			_x = -_x;
			_z = -_z;
			return;
		case 3:
		{
			int num = _x;
			_x = _z;
			_z = -num;
			return;
		}
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotateRelative(ref int _x, ref int _z)
	{
		switch (this.localRotation)
		{
		case 1:
		{
			int num = _x;
			_x = _z;
			_z = -num;
			return;
		}
		case 2:
			_x = -_x;
			_z = -_z;
			return;
		case 3:
		{
			int num = _x;
			_x = -_z;
			_z = num;
			return;
		}
		default:
			return;
		}
	}

	public void SetBlock(int _x, int _y, int _z, BlockValue _bv)
	{
		if (_bv.isWater)
		{
			Log.Warning("Prefabs should no longer store water blocks. Please use SetWater instead");
			this.SetWater(_x, _y, _z, WaterValue.Full);
			return;
		}
		if (!this.isCellsDataOwner)
		{
			Log.Error("SetBlock failed: Cannot set block data on non-owning Prefab instance.");
			return;
		}
		if ((ulong)_x >= (ulong)((long)this.size.x) || (ulong)_y >= (ulong)((long)this.size.y) || (ulong)_z >= (ulong)((long)this.size.z))
		{
			return;
		}
		this.RotateCoords(ref _x, ref _z);
		this.blockCells.SetData(_x, _y, _z, _bv.rawData);
		this.damageCells.SetData(_x, _y, _z, (ushort)_bv.damage);
	}

	public float GetHeight(int _x, int _z, bool _terrainOnly = true)
	{
		for (int i = this.size.y; i >= 0; i--)
		{
			BlockValue block = this.GetBlock(_x, i, _z);
			if (!block.isair && (block.Block.shape.IsTerrain() || !_terrainOnly))
			{
				float num = 1f - (float)((byte)block.Block.Density) / 255f;
				return (float)(i - 1) + num;
			}
		}
		return 0f;
	}

	public BlockValue GetBlock(int _x, int _y, int _z)
	{
		if ((ulong)_x >= (ulong)((long)this.size.x) || (ulong)_y >= (ulong)((long)this.size.y) || (ulong)_z >= (ulong)((long)this.size.z))
		{
			return BlockValue.Air;
		}
		BlockValue air = BlockValue.Air;
		this.RotateCoords(ref _x, ref _z);
		Prefab.Cells<uint>.Cell cell = this.blockCells.GetCell(_x, _y, _z);
		if (cell.a != null)
		{
			air.rawData = cell.Get(_x, _z);
			Prefab.Cells<ushort>.Cell cell2 = this.damageCells.GetCell(_x, _y, _z);
			if (cell2.a != null)
			{
				air.damage = (int)cell2.Get(_x, _z);
			}
			if (!this.isCellsDataOwner && this.localRotation != 0)
			{
				this.ApplyRotation(ref air);
			}
		}
		return air;
	}

	public BlockValue GetBlockNoDamage(int _localRotation, int _x, int _y, int _z)
	{
		BlockValue air = BlockValue.Air;
		this.RotateCoords(_localRotation, ref _x, ref _z);
		Prefab.Cells<uint>.Cell cell = this.blockCells.GetCell(_x, _y, _z);
		if (cell.a != null)
		{
			air.rawData = cell.Get(_x, _z);
			if (!this.isCellsDataOwner && this.localRotation != 0)
			{
				this.ApplyRotation(ref air);
			}
		}
		return air;
	}

	public PropValue GetProp(long chunkKey, int propId)
	{
		return this.GetProp(WorldChunkCache.extractX(chunkKey), WorldChunkCache.extractZ(chunkKey), propId);
	}

	public PropValue GetProp(int chunkX, int chunkZ, int propId)
	{
		Dictionary<int, PropValue> dictionary;
		PropValue propValue;
		if (!this.propsByChunk.TryGetValue(new Vector2i(chunkX, chunkZ), out dictionary) || !dictionary.TryGetValue(propId, out propValue))
		{
			return PropValue.AIR;
		}
		return propValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyRotation(ref BlockValue _bv)
	{
		if (_bv.ischild)
		{
			int parentx = _bv.parentx;
			int parentz = _bv.parentz;
			if (parentx != 0 || parentz != 0)
			{
				this.InverseRotateRelative(ref parentx, ref parentz);
				_bv.parentx = parentx;
				_bv.parentz = parentz;
				return;
			}
		}
		else
		{
			_bv = _bv.Block.shape.RotateY(true, _bv, this.localRotation);
		}
	}

	public BlockValue GetBlockNoDamage(int _offs)
	{
		return BlockValue.Air;
	}

	public int GetBlockCount()
	{
		return this.size.x * this.size.y * this.size.z;
	}

	public WaterValue GetWater(int _x, int _y, int _z)
	{
		this.RotateCoords(ref _x, ref _z);
		return this.waterCells.GetData(_x, _y, _z);
	}

	public void SetWater(int _x, int _y, int _z, WaterValue _wv)
	{
		if ((ulong)_x >= (ulong)((long)this.size.x) || (ulong)_y >= (ulong)((long)this.size.y) || (ulong)_z >= (ulong)((long)this.size.z))
		{
			return;
		}
		this.RotateCoords(ref _x, ref _z);
		this.waterCells.SetData(_x, _y, _z, _wv);
	}

	public byte GetStab(int _relx, int _absy, int _relz)
	{
		return 0;
	}

	public void SetDensity(int _x, int _y, int _z, sbyte _density)
	{
		this.RotateCoords(ref _x, ref _z);
		this.densityCells.SetData(_x, _y, _z, _density);
	}

	public sbyte GetDensity(int _x, int _y, int _z)
	{
		this.RotateCoords(ref _x, ref _z);
		return this.densityCells.GetData(_x, _y, _z);
	}

	public sbyte GetDensity(int _localRotation, int _x, int _y, int _z)
	{
		this.RotateCoords(_localRotation, ref _x, ref _z);
		return this.densityCells.GetData(_x, _y, _z);
	}

	public void SetTexture(int _x, int _y, int _z, TextureFullArray _fullTexture)
	{
		this.RotateCoords(ref _x, ref _z);
		this.textureCells.SetData(_x, _y, _z, _fullTexture);
	}

	public TextureFullArray GetTexture(int _x, int _y, int _z)
	{
		this.RotateCoords(ref _x, ref _z);
		return this.textureCells.GetData(_x, _y, _z);
	}

	public bool IsInsidePrefab(int _x, int _y, int _z)
	{
		int num;
		int num2;
		int num3;
		switch (this.localRotation)
		{
		default:
			num = _x;
			num2 = _y;
			num3 = _z;
			break;
		case 1:
			num = _z;
			num2 = _y;
			num3 = this.size.x - _x - 1;
			break;
		case 2:
			num = this.size.x - _x - 1;
			num2 = _y;
			num3 = this.size.z - _z - 1;
			break;
		case 3:
			num = this.size.z - _z - 1;
			num2 = _y;
			num3 = _x;
			break;
		}
		return this.insidePos.Contains(num, num2, num3);
	}

	public void ToggleQuestTag(FastTags<TagGroup.Global> _questTag)
	{
		if (this.GetQuestTag(_questTag))
		{
			this.questTags = this.questTags.Remove(_questTag);
			return;
		}
		this.questTags |= _questTag;
	}

	public FastTags<TagGroup.Global> GetQuestTags()
	{
		return new FastTags<TagGroup.Global>(this.questTags);
	}

	public bool GetQuestTag(FastTags<TagGroup.Global> _questTag)
	{
		return this.questTags.Test_AllSet(_questTag);
	}

	public bool HasAnyQuestTag(FastTags<TagGroup.Global> _questTag)
	{
		return this.questTags.Test_AnySet(_questTag);
	}

	public bool HasQuestTag()
	{
		return !this.questTags.IsEmpty;
	}

	public TileEntity GetTileEntity(Vector3i _blockPos)
	{
		switch (this.localRotation)
		{
		case 1:
		{
			int num = _blockPos.x;
			_blockPos.x = _blockPos.z;
			_blockPos.z = this.size.x - num - 1;
			break;
		}
		case 2:
			_blockPos.x = this.size.x - _blockPos.x - 1;
			_blockPos.z = this.size.z - _blockPos.z - 1;
			break;
		case 3:
		{
			int num = _blockPos.x;
			_blockPos.x = this.size.z - _blockPos.z - 1;
			_blockPos.z = num;
			break;
		}
		}
		TileEntity tileEntity;
		if (this.tileEntities.TryGetValue(_blockPos, out tileEntity))
		{
			return tileEntity;
		}
		return null;
	}

	public BlockTrigger GetBlockTrigger(Vector3i _blockPos)
	{
		switch (this.localRotation)
		{
		case 1:
		{
			int num = _blockPos.x;
			_blockPos.x = _blockPos.z;
			_blockPos.z = this.size.x - num - 1;
			break;
		}
		case 2:
			_blockPos.x = this.size.x - _blockPos.x - 1;
			_blockPos.z = this.size.z - _blockPos.z - 1;
			break;
		case 3:
		{
			int num = _blockPos.x;
			_blockPos.x = this.size.z - _blockPos.z - 1;
			_blockPos.z = num;
			break;
		}
		}
		BlockTrigger blockTrigger;
		if (this.triggerData.TryGetValue(_blockPos, out blockTrigger))
		{
			return blockTrigger;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadFromProperties()
	{
		this.bCopyAirBlocks = this.properties.GetBool("CopyAirBlocks");
		this.bExcludeDistantPOIMesh = this.properties.GetBool("ExcludeDistantPOIMesh");
		this.bExcludePOICulling = this.properties.GetBool("ExcludePOICulling");
		this.distantPOIYOffset = this.properties.GetFloat("DistantPOIYOffset");
		this.properties.ParseString("DistantPOIOverride", ref this.distantPOIOverride);
		this.bAllowTopSoilDecorations = this.properties.GetBool("AllowTopSoilDecorations");
		this.editorGroups.Clear();
		if (this.properties.Values.ContainsKey("EditorGroups"))
		{
			this.editorGroups.AddRange(this.properties.GetString("EditorGroups").Split(',', StringSplitOptions.None));
			for (int i = 0; i < this.editorGroups.Count; i++)
			{
				this.editorGroups[i] = this.editorGroups[i].Trim();
			}
		}
		if (this.properties.Values.ContainsKey("DifficultyTier"))
		{
			this.DifficultyTier = (byte)this.properties.GetInt("DifficultyTier");
		}
		this.properties.ParseInt("ShowQuestClearCount", ref this.ShowQuestClearCount);
		this.bTraderArea = this.properties.GetBool("TraderArea");
		this.TraderAreaProtect = (this.properties.Values.ContainsKey("TraderAreaProtect") ? StringParsers.ParseVector3i(this.properties.Values["TraderAreaProtect"], 0, -1, false) : Vector3i.zero);
		foreach (PrefabVolumeListAbs prefabVolumeListAbs in this.AllVolumeLists)
		{
			prefabVolumeListAbs.ReadFromProperties(this.properties);
		}
		this.yOffset = this.properties.GetInt("YOffset");
		if (this.size == Vector3i.zero && this.properties.Values.ContainsKey("PrefabSize"))
		{
			this.size = StringParsers.ParseVector3i(this.properties.Values["PrefabSize"], 0, -1, false);
		}
		this.rotationToFaceNorth = this.properties.GetInt("RotationToFaceNorth");
		if (this.properties.Values.ContainsKey("Tags"))
		{
			this.tags = FastTags<TagGroup.Poi>.Parse(this.properties.Values["Tags"].Replace(" ", ""));
		}
		if (this.properties.Values.ContainsKey("ThemeTags"))
		{
			this.themeTags = FastTags<TagGroup.Poi>.Parse(this.properties.Values["ThemeTags"].Replace(" ", ""));
		}
		if (this.properties.Values.ContainsKey("ThemeRepeatDistance"))
		{
			this.themeRepeatDistance = StringParsers.ParseSInt32(this.properties.Values["ThemeRepeatDistance"], 0, -1, NumberStyles.Integer);
		}
		if (this.properties.Values.ContainsKey("DuplicateRepeatDistance"))
		{
			this.duplicateRepeatDistance = StringParsers.ParseSInt32(this.properties.Values["DuplicateRepeatDistance"], 0, -1, NumberStyles.Integer);
		}
		this.indexedBlockOffsets.Clear();
		if (this.properties.Classes.ContainsKey("IndexedBlockOffsets"))
		{
			foreach (KeyValuePair<string, DynamicProperties> keyValuePair in this.properties.Classes["IndexedBlockOffsets"].Classes)
			{
				if (keyValuePair.Value.Values.Count > 0)
				{
					List<Vector3i> list = new List<Vector3i>();
					this.indexedBlockOffsets[keyValuePair.Key] = list;
					foreach (KeyValuePair<string, string> keyValuePair2 in keyValuePair.Value.Values)
					{
						list.Add(StringParsers.ParseVector3i(keyValuePair.Value.Values[keyValuePair2.Key], 0, -1, false));
					}
				}
			}
		}
		if (this.properties.Values.ContainsKey("QuestTags"))
		{
			this.questTags = FastTags<TagGroup.Global>.Parse(this.properties.Values["QuestTags"]);
		}
		if (this.properties.Values.ContainsKey("AllowedTownships"))
		{
			this.allowedTownships.Clear();
			foreach (string text in this.properties.Values["AllowedTownships"].Replace(" ", "").Split(',', StringSplitOptions.None))
			{
				this.allowedTownships.Add(text.ToLower());
			}
		}
		if (this.properties.Values.ContainsKey("AllowedBiomes"))
		{
			this.allowedBiomes.Clear();
			foreach (string text2 in this.properties.Values["AllowedBiomes"].Replace(" ", "").Split(',', StringSplitOptions.None))
			{
				this.allowedBiomes.Add(text2.ToLower());
			}
		}
		if (this.properties.Values.ContainsKey("Zoning"))
		{
			this.allowedZones.Clear();
			string[] array2 = this.properties.Values["Zoning"].Split(',', StringSplitOptions.None);
			for (int k = 0; k < array2.Length; k++)
			{
				this.AddAllowedZone(array2[k].Trim());
			}
		}
		else
		{
			this.allowedZones.Add("none");
		}
		if (this.properties.Classes.ContainsKey("Stats"))
		{
			this.renderingCost = WorldStats.FromProperties(this.properties.Classes["Stats"]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeToProperties()
	{
		this.properties.Values["CopyAirBlocks"] = this.bCopyAirBlocks.ToString();
		this.properties.Values["ExcludeDistantPOIMesh"] = this.bExcludeDistantPOIMesh.ToString();
		this.properties.Values["ExcludePOICulling"] = this.bExcludePOICulling.ToString();
		this.properties.Values["DistantPOIYOffset"] = this.distantPOIYOffset.ToCultureInvariantString();
		if (this.distantPOIOverride != null)
		{
			this.properties.Values["DistantPOIOverride"] = this.distantPOIOverride;
		}
		this.properties.Values.Remove("EditorGroups");
		if (this.editorGroups.Count > 0)
		{
			string text = string.Empty;
			for (int i = 0; i < this.editorGroups.Count; i++)
			{
				text = text + this.editorGroups[i] + ((i < this.editorGroups.Count - 1) ? ", " : string.Empty);
			}
			this.properties.Values["EditorGroups"] = text;
		}
		this.properties.Values["AllowTopSoilDecorations"] = this.bAllowTopSoilDecorations.ToString();
		this.properties.Values["DifficultyTier"] = this.DifficultyTier.ToString();
		this.properties.Values["ShowQuestClearCount"] = this.ShowQuestClearCount.ToString();
		foreach (PrefabVolumeListAbs prefabVolumeListAbs in this.AllVolumeLists)
		{
			prefabVolumeListAbs.WriteToProperties(this.properties);
		}
		if (this.yOffset != 0)
		{
			this.properties.Values["YOffset"] = this.yOffset.ToString();
		}
		else
		{
			this.properties.Values.Remove("YOffset");
		}
		this.properties.Values["PrefabSize"] = this.size.ToString();
		this.properties.Values["RotationToFaceNorth"] = this.rotationToFaceNorth.ToString();
		string text2 = "";
		for (int j = 0; j < this.allowedTownships.Count; j++)
		{
			text2 = text2 + this.allowedTownships[j] + ((j < this.allowedTownships.Count - 1) ? "," : "");
		}
		if (text2.Length > 0)
		{
			this.properties.Values["AllowedTownships"] = text2;
		}
		else
		{
			this.properties.Values.Remove("AllowedTownships");
		}
		text2 = "";
		for (int k = 0; k < this.allowedBiomes.Count; k++)
		{
			text2 = text2 + this.allowedBiomes[k] + ((k < this.allowedBiomes.Count - 1) ? "," : "");
		}
		if (text2.Length > 0)
		{
			this.properties.Values["AllowedBiomes"] = text2;
		}
		else
		{
			this.properties.Values.Remove("AllowedBiomes");
		}
		if (this.tags.ToString() != "")
		{
			this.properties.Values["Tags"] = this.tags.ToString();
		}
		else
		{
			this.properties.Values.Remove("Tags");
		}
		if (this.themeTags.ToString() != "")
		{
			this.properties.Values["ThemeTags"] = this.themeTags.ToString();
		}
		else
		{
			this.properties.Values.Remove("ThemeTags");
		}
		if (this.themeRepeatDistance != 300)
		{
			this.properties.Values["ThemeRepeatDistance"] = this.themeRepeatDistance.ToString(NumberFormatInfo.InvariantInfo);
		}
		else
		{
			this.properties.Values.Remove("ThemeRepeatDistance");
		}
		if (this.duplicateRepeatDistance != 1000)
		{
			this.properties.Values["DuplicateRepeatDistance"] = this.duplicateRepeatDistance.ToString(NumberFormatInfo.InvariantInfo);
		}
		else
		{
			this.properties.Values.Remove("DuplicateRepeatDistance");
		}
		if (this.indexedBlockOffsets.Any((KeyValuePair<string, List<Vector3i>> _pair) => _pair.Value.Count > 0))
		{
			DynamicProperties dynamicProperties = new DynamicProperties();
			this.properties.Classes["IndexedBlockOffsets"] = dynamicProperties;
			using (Dictionary<string, List<Vector3i>>.Enumerator enumerator2 = this.indexedBlockOffsets.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					KeyValuePair<string, List<Vector3i>> keyValuePair = enumerator2.Current;
					if (keyValuePair.Value.Count > 0)
					{
						DynamicProperties dynamicProperties2 = new DynamicProperties();
						dynamicProperties.Classes[keyValuePair.Key] = dynamicProperties2;
						for (int l = 0; l < keyValuePair.Value.Count; l++)
						{
							dynamicProperties2.Values[l.ToString()] = keyValuePair.Value[l].ToString();
						}
					}
				}
				goto IL_05AE;
			}
		}
		this.properties.Classes.Remove("IndexedBlockOffsets");
		IL_05AE:
		if (!this.questTags.IsEmpty)
		{
			text2 = this.questTags.ToString();
		}
		if (text2.Length > 0)
		{
			this.properties.Values["QuestTags"] = text2;
		}
		else
		{
			this.properties.Values.Remove("QuestTags");
		}
		this.properties.Values.Remove("Zoning");
		if (this.allowedZones.Count > 0)
		{
			string text3 = string.Empty;
			for (int m = 0; m < this.allowedZones.Count; m++)
			{
				text3 = text3 + this.allowedZones[m] + ((m < this.allowedZones.Count - 1) ? ", " : string.Empty);
			}
			this.properties.Values["Zoning"] = text3;
		}
		if (this.renderingCost != null)
		{
			this.properties.Classes["Stats"] = this.renderingCost.ToProperties();
		}
	}

	public static bool PrefabExists(string _prefabFileName)
	{
		return PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabFileName, null, null).Type != PathAbstractions.EAbstractedLocationType.None;
	}

	public bool Load(string _prefabName, bool _applyMapping = true, bool _fixChildBlocks = true, bool _allowMissingBlocks = false, bool _skipLoadingBlockData = false)
	{
		return this.Load(PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabName, null, null), _applyMapping, _fixChildBlocks, _allowMissingBlocks, _skipLoadingBlockData);
	}

	public bool Load(PathAbstractions.AbstractedLocation _location, bool _applyMapping = true, bool _fixChildBlocks = true, bool _allowMissingBlocks = false, bool _skipLoadingBlockData = false)
	{
		if (_location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance != null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				Log.Warning("Prefab loading failed. Prefab '{0}' does not exist!", new object[] { _location.Name });
			}
			else
			{
				Log.Error("Prefab loading failed. Prefab '{0}' does not exist!", new object[] { _location.Name });
			}
			return false;
		}
		this.location = _location;
		if (_skipLoadingBlockData)
		{
			if (!this.loadSizeDataOnly(_location, _applyMapping, _fixChildBlocks, _allowMissingBlocks, _skipLoadingBlockData))
			{
				return false;
			}
		}
		else if (!this.loadBlockData(_location, _applyMapping, _fixChildBlocks, _allowMissingBlocks, _skipLoadingBlockData))
		{
			return false;
		}
		this.loadWorldSignData(_location);
		return this.LoadXMLData(_location);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loadSizeDataOnly(PathAbstractions.AbstractedLocation _location, bool _applyMapping, bool _fixChildBlocks, bool _allowMissingBlocks, bool _skipLoadingBlockData = false)
	{
		bool flag;
		using (Stream stream = SdFile.OpenRead(_location.FullPath))
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				pooledBinaryReader.SetBaseStream(stream);
				if (pooledBinaryReader.ReadChar() != 't' || pooledBinaryReader.ReadChar() != 't' || pooledBinaryReader.ReadChar() != 's' || pooledBinaryReader.ReadChar() != '\0')
				{
					flag = false;
				}
				else
				{
					pooledBinaryReader.ReadUInt32();
					this.size = default(Vector3i);
					this.size.x = (int)pooledBinaryReader.ReadInt16();
					this.size.y = (int)pooledBinaryReader.ReadInt16();
					this.size.z = (int)pooledBinaryReader.ReadInt16();
					flag = true;
				}
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loadBlockData(PathAbstractions.AbstractedLocation _location, bool _applyMapping, bool _fixChildBlocks, bool _allowMissingBlocks, bool _skipLoadingBlockData = false)
	{
		bool flag = true;
		ArrayListMP<int> arrayListMP = null;
		if (_applyMapping)
		{
			arrayListMP = this.loadIdMapping(_location.Folder, _location.FileNameNoExtension, _allowMissingBlocks);
			if (arrayListMP == null)
			{
				return false;
			}
		}
		try
		{
			using (Stream stream = SdFile.OpenRead(_location.FullPath))
			{
				using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
				{
					pooledBinaryReader.SetBaseStream(stream);
					if (pooledBinaryReader.ReadChar() != 't' || pooledBinaryReader.ReadChar() != 't' || pooledBinaryReader.ReadChar() != 's' || pooledBinaryReader.ReadChar() != '\0')
					{
						return false;
					}
					uint num = pooledBinaryReader.ReadUInt32();
					int[] array = ((arrayListMP != null) ? arrayListMP.Items : null);
					if (!this.readBlockData(pooledBinaryReader, num, array, true))
					{
						return false;
					}
					if (num > 12U)
					{
						this.readTileEntities(pooledBinaryReader, array);
					}
					if (num > 15U)
					{
						this.readTriggerData(pooledBinaryReader);
					}
					this.insidePos.Load(_location.FullPathNoExtension + ".ins", this.size);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
			flag = false;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loadWorldSignData(PathAbstractions.AbstractedLocation _location)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return false;
		}
		if (PrefabEditModeManager.Instance != null && SdFile.Exists(_location.FullPathNoExtension + "_signs.xml"))
		{
			try
			{
				XmlFile xmlFile = new XmlFile(_location.Folder, _location.FileNameNoExtension + "_signs", false, false);
				SignLibrary signLibrary = new SignLibrary();
				signLibrary.ReadXml(xmlFile);
				SignDataManager.Instance.TryRegisterLibrary(_location.FileNameNoExtension, signLibrary, PrefabEditModeManager.Instance.IsActive());
				return true;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
			return false;
		}
		return false;
	}

	public bool LoadXMLData(PathAbstractions.AbstractedLocation _location)
	{
		this.location = _location;
		if (!SdFile.Exists(_location.FullPathNoExtension + ".xml"))
		{
			return true;
		}
		if (!this.properties.Load(_location.Folder, _location.Name))
		{
			return false;
		}
		this.ReadFromProperties();
		return true;
	}

	public bool Save(string _prefabName, bool _createMapping = true)
	{
		return this.Save(PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabName, null, null), _createMapping);
	}

	public bool Save(PathAbstractions.AbstractedLocation _location, bool _createMapping = true)
	{
		if (this.saveBlockData(_location, _createMapping) && this.SaveXMLData(_location))
		{
			SignLibrary signLibrary;
			if (SignDataManager.Instance.TryGetSignLibrary(_location.FileNameNoExtension, out signLibrary))
			{
				string text = _location.FullPathNoExtension + "_signs.xml";
				signLibrary.WriteXml(text);
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddAllChildBlocks()
	{
		if (!this.isCellsDataOwner)
		{
			return;
		}
		if (this.blockCells == null)
		{
			return;
		}
		if (this.blockCells.a == null)
		{
			return;
		}
		int num = this.blockCells.a.Length;
		for (int i = 0; i < num; i++)
		{
			Prefab.Cells<uint>.CellsAtZ cellsAtZ = this.blockCells.a[i];
			if (cellsAtZ != null)
			{
				int num2 = cellsAtZ.a.Length;
				for (int j = 0; j < num2; j++)
				{
					Prefab.Cells<uint>.CellsAtX cellsAtX = cellsAtZ.a[j];
					if (cellsAtX != null)
					{
						int num3 = cellsAtX.a.Length;
						for (int k = 0; k < num3; k++)
						{
							Prefab.Cells<uint>.Cell cell = cellsAtX.a[k];
							if (cell.a != null)
							{
								for (int l = 0; l < cell.a.Length; l++)
								{
									uint num4 = cell.a[l];
									if ((num4 & 65535U) != 0U)
									{
										BlockValue blockValue = new BlockValue(num4);
										if (blockValue.rawData != 0U && !blockValue.ischild)
										{
											Block block = blockValue.Block;
											if (block != null && block.isMultiBlock)
											{
												int num5 = (k << 2) + (l & 3);
												int num6 = i;
												int num7 = (j << 2) + (l >> 2);
												int rotation = (int)blockValue.rotation;
												for (int m = block.multiBlockPos.Length - 1; m >= 0; m--)
												{
													Vector3i vector3i = block.multiBlockPos.Get(m, blockValue.type, rotation);
													if (!(vector3i == Vector3i.zero))
													{
														int x = vector3i.x;
														int y = vector3i.y;
														int z = vector3i.z;
														blockValue.ischild = true;
														blockValue.parentx = -x;
														blockValue.parenty = -y;
														blockValue.parentz = -z;
														this.RotateRelative(ref x, ref z);
														int num8 = num5 + x;
														int num9 = num6 + y;
														int num10 = num7 + z;
														if ((ulong)num8 < (ulong)((long)this.size.x) && (ulong)num9 < (ulong)((long)this.size.y) && (ulong)num10 < (ulong)((long)this.size.z))
														{
															this.blockCells.SetData(num8, num9, num10, blockValue.rawData);
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveAllChildAndOldBlocks()
	{
		for (int i = this.size.y - 1; i >= 0; i--)
		{
			for (int j = this.size.z - 1; j >= 0; j--)
			{
				for (int k = this.size.x - 1; k >= 0; k--)
				{
					BlockValue block = this.GetBlock(k, i, j);
					Block block2 = block.Block;
					if (block2 == null)
					{
						this.SetBlock(k, i, j, BlockValue.Air);
					}
					else if (block.ischild)
					{
						this.SetBlock(k, i, j, BlockValue.Air);
					}
					else if (block2 is BlockModelTree && (block.meta & 1) != 0)
					{
						this.SetBlock(k, i, j, BlockValue.Air);
					}
				}
			}
		}
	}

	public bool SaveXMLData(PathAbstractions.AbstractedLocation _location)
	{
		this.writeToProperties();
		return this.properties.Save("prefab", _location.Folder, _location.FileNameNoExtension);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool saveBlockData(PathAbstractions.AbstractedLocation _location, bool _createMapping)
	{
		this.RemoveAllChildAndOldBlocks();
		if (_createMapping)
		{
			NameIdMapping nameIdMapping = new NameIdMapping(_location.FullPathNoExtension + ".blocks.nim", Block.MAX_BLOCKS);
			for (int i = this.GetBlockCount() - 1; i >= 0; i--)
			{
				int num;
				int num2;
				int num3;
				this.offsetToCoord(i, out num, out num2, out num3);
				Block block = this.GetBlock(num, num2, num3).Block;
				nameIdMapping.AddMapping(block.blockID, block.GetBlockName(), false);
			}
			nameIdMapping.WriteToFile();
		}
		try
		{
			using (Stream stream = SdFile.Open(_location.FullPath, FileMode.Create))
			{
				using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
				{
					pooledBinaryWriter.SetBaseStream(stream);
					pooledBinaryWriter.Write('t');
					pooledBinaryWriter.Write('t');
					pooledBinaryWriter.Write('s');
					pooledBinaryWriter.Write(0);
					pooledBinaryWriter.Write((uint)Prefab.CurrentSaveVersion);
					this.writeBlockData(pooledBinaryWriter);
					this.writeTileEntities(pooledBinaryWriter);
					this.writeTriggerData(pooledBinaryWriter);
					if (this.IsCullThisPrefab())
					{
						this.insidePos.Save(_location.FullPathNoExtension + ".ins");
					}
					else
					{
						SdFile.Delete(_location.FullPathNoExtension + ".ins");
					}
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
		return false;
	}

	public bool IsCullThisPrefab()
	{
		return !this.bExcludePOICulling;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeBlockData(BinaryWriter _bw)
	{
		_bw.Write((short)this.size.x);
		_bw.Write((short)this.size.y);
		_bw.Write((short)this.size.z);
		Prefab.Data data = this.CellsToArrays();
		uint[] blocks = data.m_Blocks;
		for (int i = 0; i < blocks.Length; i++)
		{
			_bw.Write(blocks[i]);
		}
		_bw.Write(data.m_Density);
		byte[] array = new byte[data.m_Damage.Length * 2];
		for (int j = 0; j < data.m_Damage.Length; j++)
		{
			array[j * 2] = (byte)(data.m_Damage[j] & 255);
			array[j * 2 + 1] = (byte)((data.m_Damage[j] >> 8) & 255);
		}
		_bw.Write(array);
		SimpleBitStream simpleBitStream = new SimpleBitStream(1000);
		for (int k = 0; k < data.m_Textures.Length; k++)
		{
			bool flag = !data.m_Textures[k].IsDefault;
			simpleBitStream.Add(flag);
		}
		simpleBitStream.Write(_bw);
		for (int l = 0; l < data.m_Textures.Length; l++)
		{
			if (!data.m_Textures[l].IsDefault)
			{
				data.m_Textures[l].Write(_bw);
			}
		}
		SimpleBitStream simpleBitStream2 = new SimpleBitStream(1000);
		for (int m = 0; m < data.m_Water.Length; m++)
		{
			simpleBitStream2.Add(data.m_Water[m].HasMass());
		}
		simpleBitStream2.Write(_bw);
		for (int n = 0; n < data.m_Water.Length; n++)
		{
			WaterValue waterValue = data.m_Water[n];
			if (waterValue.HasMass())
			{
				waterValue.Write(_bw);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeTileEntities(BinaryWriter _bw)
	{
		_bw.Write((short)this.tileEntities.Count);
		foreach (KeyValuePair<Vector3i, TileEntity> keyValuePair in this.tileEntities)
		{
			using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
			{
				using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(true))
				{
					pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
					keyValuePair.Value.write(pooledBinaryWriter, TileEntity.StreamModeWrite.Persistency);
				}
				_bw.Write((short)pooledExpandableMemoryStream.Length);
				_bw.Write((byte)keyValuePair.Value.GetTileEntityType());
				pooledExpandableMemoryStream.WriteTo(_bw.BaseStream);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeTriggerData(BinaryWriter _bw)
	{
		_bw.Write((short)this.triggerData.Count);
		foreach (KeyValuePair<Vector3i, BlockTrigger> keyValuePair in this.triggerData)
		{
			using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
			{
				using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(true))
				{
					pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
					keyValuePair.Value.Write(pooledBinaryWriter);
				}
				_bw.Write((short)pooledExpandableMemoryStream.Length);
				StreamUtils.Write(_bw, keyValuePair.Key);
				pooledExpandableMemoryStream.WriteTo(_bw.BaseStream);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool readBlockData(PooledBinaryReader _br, uint _version, int[] _blockIdMapping, bool _fixChildBlocks)
	{
		this.statistics.Clear();
		this.multiBlockParentIndices.Clear();
		this.decoAllowedBlockIndices.Clear();
		this.localRotation = 0;
		this.size.x = (int)_br.ReadInt16();
		this.size.y = (int)_br.ReadInt16();
		this.size.z = (int)_br.ReadInt16();
		int blockCount = this.GetBlockCount();
		this.InitData();
		Prefab.sharedData.Expand(blockCount);
		Prefab.Data data = Prefab.sharedData;
		if (_version >= 2U && _version < 7U)
		{
			this.bCopyAirBlocks = _br.ReadBoolean();
		}
		if (_version >= 3U && _version < 7U)
		{
			this.bAllowTopSoilDecorations = _br.ReadBoolean();
		}
		List<Vector3i> list = null;
		int num = this.blockTypeMissingBlock;
		if (_blockIdMapping != null && num >= 0)
		{
			list = new List<Vector3i>();
		}
		int num2 = blockCount * 4;
		if (Prefab.tempBuf == null || Prefab.tempBuf.Length < num2)
		{
			Prefab.tempBuf = new byte[Utils.FastMax(200000, num2)];
		}
		int num3 = 0;
		_br.Read(Prefab.tempBuf, 0, blockCount * 4);
		if (_version <= 4U)
		{
			for (int i = 0; i < this.size.x; i++)
			{
				for (int j = 0; j < this.size.z; j++)
				{
					for (int k = 0; k < this.size.y; k++)
					{
						BlockValue blockValue = new BlockValue((uint)((int)Prefab.tempBuf[num3] | ((int)Prefab.tempBuf[num3 + 1] << 8) | ((int)Prefab.tempBuf[num3 + 2] << 16) | ((int)Prefab.tempBuf[num3 + 3] << 24)));
						num3 += 4;
						if (_blockIdMapping != null)
						{
							int num4 = _blockIdMapping[blockValue.type];
							if (num4 < 0)
							{
								Log.Error(string.Concat(new string[]
								{
									"Loading prefab \"",
									this.location.ToString(),
									"\" failed: Block ",
									blockValue.type.ToString(),
									" used in prefab has no mapping."
								}));
								return false;
							}
							blockValue.type = num4;
							if (num >= 0 && blockValue.type == this.blockTypeMissingBlock)
							{
								list.Add(new Vector3i(i, k, j));
							}
						}
						if (blockValue.isWater)
						{
							this.SetWater(i, k, j, WaterValue.Full);
						}
						else
						{
							if (_fixChildBlocks)
							{
								if (blockValue.ischild)
								{
									goto IL_0240;
								}
								Block block = blockValue.Block;
								if (block == null || ((blockValue.meta & 1) != 0 && block is BlockModelTree))
								{
									goto IL_0240;
								}
							}
							this.SetBlock(i, k, j, blockValue);
						}
						IL_0240:;
					}
				}
			}
		}
		else
		{
			for (int l = 0; l < blockCount; l++)
			{
				uint num5 = (uint)((int)Prefab.tempBuf[num3] | ((int)Prefab.tempBuf[num3 + 1] << 8) | ((int)Prefab.tempBuf[num3 + 2] << 16) | ((int)Prefab.tempBuf[num3 + 3] << 24));
				num3 += 4;
				data.m_Blocks[l] = 0U;
				if (num5 != 0U)
				{
					if (_version < 18U)
					{
						num5 = BlockValueV3.ConvertOldRawData(num5);
					}
					BlockValue blockValue2 = new BlockValue(num5);
					if (_blockIdMapping != null)
					{
						int type = blockValue2.type;
						if (type != 0)
						{
							int num6 = _blockIdMapping[type];
							if (num6 < 0)
							{
								int num7;
								int num8;
								int num9;
								this.offsetToCoord(l, out num7, out num8, out num9);
								Log.Error(string.Concat(new string[]
								{
									"Loading prefab \"",
									this.location.ToString(),
									"\" failed: Block ",
									type.ToString(),
									" used in prefab at ",
									num7.ToString(),
									" / ",
									num8.ToString(),
									" / ",
									num9.ToString(),
									" has no mapping."
								}));
								return false;
							}
							blockValue2.type = num6;
							if (num >= 0 && num6 == this.blockTypeMissingBlock)
							{
								int num10;
								int num11;
								int num12;
								this.offsetToCoord(l, out num10, out num11, out num12);
								list.Add(new Vector3i(num10, num11, num12));
							}
						}
					}
					if (_version < 17U && blockValue2.isWater)
					{
						data.m_Water[l] = WaterValue.Full;
					}
					else
					{
						Block block2 = blockValue2.Block;
						this.updateBlockStatistics(blockValue2, block2);
						if (!_fixChildBlocks || (!blockValue2.ischild && block2 != null && ((blockValue2.meta & 1) == 0 || !(block2 is BlockModelTree))))
						{
							if (block2.isMultiBlock && !blockValue2.ischild)
							{
								this.multiBlockParentIndices.Add(l);
							}
							if (DecoUtils.HasDecoAllowed(blockValue2))
							{
								this.decoAllowedBlockIndices.Add(l);
							}
							data.m_Blocks[l] = blockValue2.rawData;
						}
					}
				}
			}
			_br.Read(data.m_Density, 0, this.size.x * this.size.y * this.size.z);
		}
		if (_blockIdMapping != null && num >= 0)
		{
			foreach (Vector3i vector3i in list)
			{
				this.SetDensity(vector3i.x, vector3i.y, vector3i.z, MarchingCubes.DensityAir);
			}
		}
		if (_version > 8U)
		{
			_br.Read(Prefab.tempBuf, 0, blockCount * 2);
			for (int m = 0; m < blockCount; m++)
			{
				data.m_Damage[m] = (ushort)((int)Prefab.tempBuf[m * 2] | ((int)Prefab.tempBuf[m * 2 + 1] << 8));
			}
		}
		if (_version >= 10U)
		{
			Prefab.simpleBitStreamReader.Reset();
			Prefab.simpleBitStreamReader.Read(_br);
			if (_version >= 19U)
			{
				while ((num3 = Prefab.simpleBitStreamReader.GetNextOffset()) >= 0)
				{
					data.m_Textures[num3].Read(_br, 1);
				}
			}
			else
			{
				while ((num3 = Prefab.simpleBitStreamReader.GetNextOffset()) >= 0)
				{
					data.m_Textures[num3][0] = _br.ReadInt64();
				}
			}
		}
		this.entities.Clear();
		if (_version >= 4U && _version < 12U)
		{
			int num13 = (int)_br.ReadInt16();
			for (int n = 0; n < num13; n++)
			{
				EntityCreationData entityCreationData = new EntityCreationData();
				entityCreationData.read(_br, false);
				this.entities.Add(entityCreationData);
			}
		}
		if (_version >= 17U)
		{
			Prefab.simpleBitStreamReader.Reset();
			Prefab.simpleBitStreamReader.Read(_br);
			while ((num3 = Prefab.simpleBitStreamReader.GetNextOffset()) >= 0)
			{
				data.m_Water[num3] = WaterValue.FromStream(_br);
			}
		}
		this.CellsFromArrays(ref data);
		if (_fixChildBlocks)
		{
			this.AddAllChildBlocks();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CellsFromArrays(ref Prefab.Data _data)
	{
		BlockValue air = BlockValue.Air;
		for (int i = 0; i < this.size.y; i++)
		{
			for (int j = this.size.z - 1; j >= 0; j--)
			{
				for (int k = this.size.x - 1; k >= 0; k--)
				{
					int num = this.CoordToOffset(0, k, i, j);
					air.rawData = _data.m_Blocks[num];
					if (!air.isair)
					{
						this.blockCells.AllocCell(k, i, j).Set(k, j, air.rawData);
					}
					ushort num2 = _data.m_Damage[num];
					if (num2 != 0)
					{
						this.damageCells.AllocCell(k, i, j).Set(k, j, num2);
					}
					sbyte b = (sbyte)_data.m_Density[num];
					if (b != this.densityCells.defaultValue)
					{
						this.densityCells.AllocCell(k, i, j).Set(k, j, b);
					}
					TextureFullArray textureFullArray = _data.m_Textures[num];
					if (!textureFullArray.IsDefault)
					{
						this.textureCells.AllocCell(k, i, j).Set(k, j, textureFullArray);
					}
					WaterValue waterValue = _data.m_Water[num];
					if (waterValue.HasMass())
					{
						this.waterCells.AllocCell(k, i, j).Set(k, j, waterValue);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.Data CellsToArrays()
	{
		Prefab.Data data;
		data.m_Blocks = this.blockCells.ToArray(this, this.size);
		data.m_Damage = this.damageCells.ToArray(this, this.size);
		data.m_Density = (byte[])this.densityCells.ToArray(this, this.size);
		data.m_Textures = this.textureCells.ToArray(this, this.size);
		data.m_Water = this.waterCells.ToArray(this, this.size);
		return data;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readTileEntities(PooledBinaryReader _br, int[] _blockIdMapping)
	{
		this.tileEntities.Clear();
		int num = (int)_br.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			int num2 = (int)_br.ReadInt16();
			TileEntityType tileEntityType = (TileEntityType)_br.ReadByte();
			try
			{
				TileEntity tileEntity = null;
				using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
				{
					StreamUtils.StreamCopy(_br.BaseStream, pooledExpandableMemoryStream, num2, null, true);
					pooledExpandableMemoryStream.Position = 0L;
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(true))
					{
						pooledBinaryReader.SetBaseStream(pooledExpandableMemoryStream);
						tileEntity = TileEntity.InstantiateFromRead(pooledBinaryReader, TileEntity.StreamModeRead.Persistency, tileEntityType, null, _blockIdMapping, new Func<int, int, int, BlockValue>(this.GetBlock));
					}
				}
				Block block = this.GetBlock(tileEntity.localChunkPos.x, tileEntity.localChunkPos.y, tileEntity.localChunkPos.z).Block;
				if (block == null || block.IsTileEntitySavedInPrefab())
				{
					this.tileEntities.Add(tileEntity.localChunkPos, tileEntity);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Skipping loading of active block data for " + this.PrefabName + " because of the following exception:");
				Log.Exception(ex);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readTriggerData(PooledBinaryReader _br)
	{
		this.triggerData.Clear();
		this.TriggerLayers.Clear();
		int num = (int)_br.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			int num2 = (int)_br.ReadInt16();
			Vector3i vector3i = StreamUtils.ReadVector3i(_br);
			try
			{
				BlockTrigger blockTrigger = new BlockTrigger(null);
				blockTrigger.LocalChunkPos = vector3i;
				using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
				{
					StreamUtils.StreamCopy(_br.BaseStream, pooledExpandableMemoryStream, num2, null, true);
					pooledExpandableMemoryStream.Position = 0L;
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(true))
					{
						pooledBinaryReader.SetBaseStream(pooledExpandableMemoryStream);
						blockTrigger.Read(pooledBinaryReader);
					}
				}
				if (!Block.BlocksLoaded || this.GetBlock(vector3i.x, vector3i.y, vector3i.z).Block.AllowBlockTriggers)
				{
					this.triggerData.Add(blockTrigger.LocalChunkPos, blockTrigger);
					this.HandleAddingTriggerLayers(blockTrigger);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Skipping loading of active block data for " + this.PrefabName + " because of the following exception:");
				Log.Exception(ex);
			}
		}
	}

	public void RotateY(bool _bLeft, int _rotCount)
	{
		if (_rotCount == 0)
		{
			return;
		}
		if (Block.BlocksLoaded && this.isCellsDataOwner)
		{
			int num = this.blockCells.a.Length;
			for (int i = 0; i < num; i++)
			{
				Prefab.Cells<uint>.CellsAtZ cellsAtZ = this.blockCells.a[i];
				if (cellsAtZ != null)
				{
					int num2 = cellsAtZ.a.Length;
					for (int j = 0; j < num2; j++)
					{
						Prefab.Cells<uint>.CellsAtX cellsAtX = cellsAtZ.a[j];
						if (cellsAtX != null)
						{
							int num3 = cellsAtX.a.Length;
							for (int k = 0; k < num3; k++)
							{
								Prefab.Cells<uint>.Cell cell = cellsAtX.a[k];
								if (cell.a != null)
								{
									for (int l = 0; l < cell.a.Length; l++)
									{
										uint num4 = cell.a[l];
										if ((num4 & 65535U) != 0U)
										{
											BlockValue blockValue = new BlockValue(num4);
											if (!blockValue.ischild)
											{
												Block block = blockValue.Block;
												if (block == null || ((blockValue.meta & 1) != 0 && block is BlockModelTree))
												{
													cell.a[l] = 0U;
												}
												else
												{
													blockValue = block.shape.RotateY(_bLeft, blockValue, _rotCount);
													cell.a[l] = blockValue.rawData;
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		for (int m = 0; m < _rotCount; m++)
		{
			this.localRotation += (_bLeft ? 1 : (-1));
			this.localRotation &= 3;
			for (int n = 0; n < this.entities.Count; n++)
			{
				EntityCreationData entityCreationData = this.entities[n];
				if (_bLeft)
				{
					entityCreationData.pos = new Vector3((float)this.size.z - entityCreationData.pos.z, entityCreationData.pos.y, entityCreationData.pos.x);
					entityCreationData.rot = new Vector3(entityCreationData.rot.x, entityCreationData.rot.y - 90f, entityCreationData.rot.z);
				}
				else
				{
					entityCreationData.pos = new Vector3(entityCreationData.pos.z, entityCreationData.pos.y, (float)this.size.x - entityCreationData.pos.x);
					entityCreationData.rot = new Vector3(entityCreationData.rot.x, entityCreationData.rot.y + 90f, entityCreationData.rot.z);
				}
			}
			MathUtils.Swap(ref this.TraderAreaProtect.x, ref this.TraderAreaProtect.z);
			foreach (KeyValuePair<string, List<Vector3i>> keyValuePair in this.indexedBlockOffsets)
			{
				for (int num5 = 0; num5 < keyValuePair.Value.Count; num5++)
				{
					Vector3i vector3i = keyValuePair.Value[num5];
					Prefab.RotatePointOnY(_bLeft, ref vector3i);
					keyValuePair.Value[num5] = vector3i;
				}
			}
			foreach (PrefabVolumeListAbs prefabVolumeListAbs in this.AllVolumeLists)
			{
				prefabVolumeListAbs.RotateY(_bLeft, this.size);
			}
			MathUtils.Swap(ref this.size.x, ref this.size.z);
		}
		if (Block.BlocksLoaded)
		{
			this.AddAllChildBlocks();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RotatePointOnY(bool _bLeft, ref Vector3i _center)
	{
		Vector3 vector;
		if (_bLeft)
		{
			vector = Quaternion.AngleAxis(-90f, Vector3.up) * _center.ToVector3();
		}
		else
		{
			vector = Quaternion.AngleAxis(90f, Vector3.up) * _center.ToVector3();
		}
		_center = new Vector3i(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
	}

	public void Replace(BlockValue _src, BlockValue _dst, bool _bConsiderRotation, int _considerPaintId1 = -1, int _considerPaintId2 = -1)
	{
		for (int i = 0; i < this.size.x; i++)
		{
			for (int j = 0; j < this.size.z; j++)
			{
				for (int k = 0; k < this.size.y; k++)
				{
					BlockValue block = this.GetBlock(i, k, j);
					if (!block.ischild && block.type == _src.type && (!_bConsiderRotation || block.rotation == _src.rotation) && (_considerPaintId1 == -1 || this.hasTexture(this.GetTexture(i, k, j), _considerPaintId1)) && (_considerPaintId2 == -1 || this.hasTexture(this.GetTexture(i, k, j), _considerPaintId2)))
					{
						BlockValue blockValue = _dst;
						if (!_bConsiderRotation)
						{
							blockValue.rotation = block.rotation;
						}
						blockValue.meta = ((_dst.meta != 0) ? _dst.meta : block.meta);
						this.SetBlock(i, k, j, blockValue);
						bool flag = _src.Block.shape.IsTerrain();
						Block block2 = _dst.Block;
						bool flag2 = ((block2 != null) ? block2.shape.IsTerrain() : flag);
						if (flag != flag2)
						{
							sbyte b = this.GetDensity(i, k, j);
							if (flag2)
							{
								b = MarchingCubes.DensityTerrain;
							}
							else if (b != 0)
							{
								b = MarchingCubes.DensityAir;
							}
							this.SetDensity(i, k, j, b);
						}
					}
				}
			}
		}
	}

	public void Replace(int _searchPaintId, int _replacePaintId, int _blockId = -1)
	{
		for (int i = 0; i < this.size.x; i++)
		{
			for (int j = 0; j < this.size.z; j++)
			{
				for (int k = 0; k < this.size.y; k++)
				{
					BlockValue block = this.GetBlock(i, k, j);
					if (!block.ischild && this.hasTexture(this.GetTexture(i, k, j), _searchPaintId) && (_blockId == -1 || _blockId == block.type))
					{
						TextureFullArray texture = this.GetTexture(i, k, j);
						for (int l = 0; l < 1; l++)
						{
							for (int m = 0; m < 6; m++)
							{
								if (((texture[l] >> m * 8) & 255L) == (long)_searchPaintId)
								{
									ref TextureFullArray ptr = ref texture;
									int num = l;
									ptr[num] &= ~(255L << m * 8);
									ptr = ref texture;
									num = l;
									ptr[num] |= (long)_replacePaintId << m * 8;
								}
							}
						}
						this.SetTexture(i, k, j, texture);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasTexture(TextureFullArray _fullTexture, int _textureIdx)
	{
		for (int i = 0; i < 1; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				if (((_fullTexture[i] >> j * 8) & 255L) == (long)_textureIdx)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int Search(BlockValue _src, bool _bConsiderRotation, int _considerPaintId1 = -1, int _considerPaintId2 = -1)
	{
		int num = 0;
		for (int i = 0; i < this.size.x; i++)
		{
			for (int j = 0; j < this.size.z; j++)
			{
				for (int k = 0; k < this.size.y; k++)
				{
					BlockValue block = this.GetBlock(i, k, j);
					if (!block.ischild && block.type == _src.type && (!_bConsiderRotation || block.rotation == _src.rotation) && (_considerPaintId1 == -1 || this.hasTexture(this.GetTexture(i, k, j), _considerPaintId1)) && (_considerPaintId2 == -1 || this.hasTexture(this.GetTexture(i, k, j), _considerPaintId2)))
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	public int Search(int _paintId, int _blockId = -1)
	{
		int num = 0;
		for (int i = 0; i < this.size.x; i++)
		{
			for (int j = 0; j < this.size.z; j++)
			{
				for (int k = 0; k < this.size.y; k++)
				{
					BlockValue block = this.GetBlock(i, k, j);
					if (!block.ischild && this.hasTexture(this.GetTexture(i, k, j), _paintId) && (_blockId == -1 || _blockId == block.type))
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	public void CopyIntoRPC(GameManager _gm, Vector3i _destinationPos, bool _pasteAirBlocks = false)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		NetPackageWaterSet package = NetPackageManager.GetPackage<NetPackageWaterSet>();
		if (_pasteAirBlocks)
		{
			this.AddAllChildBlocks();
		}
		if (this.bCopyAirBlocks)
		{
			NetPackageWaterSet package2 = NetPackageManager.GetPackage<NetPackageWaterSet>();
			for (int i = 0; i < this.size.y; i++)
			{
				for (int j = 0; j < this.size.x; j++)
				{
					for (int k = 0; k < this.size.z; k++)
					{
						int num = j + _destinationPos.x;
						int num2 = i + _destinationPos.y;
						int num3 = k + _destinationPos.z;
						if (!_gm.World.GetBlock(num, num2, num3).isair)
						{
							list.Add(new BlockChangeInfo(new Vector3i(num, num2, num3), BlockValue.Air));
						}
						if (_gm.World.GetWater(num, num2, num3).HasMass())
						{
							package2.AddChange(num, num2, num3, WaterValue.Empty);
						}
					}
				}
			}
			_gm.SetWaterRPC(package2);
		}
		Dictionary<Vector3i, TileEntity> dictionary = new Dictionary<Vector3i, TileEntity>();
		Dictionary<Vector3i, BlockTrigger> dictionary2 = new Dictionary<Vector3i, BlockTrigger>();
		for (int l = 0; l < this.size.y; l++)
		{
			for (int m = 0; m < this.size.x; m++)
			{
				for (int n = 0; n < this.size.z; n++)
				{
					WaterValue water = this.GetWater(m, l, n);
					if (water.HasMass())
					{
						package.AddChange(m + _destinationPos.x, l + _destinationPos.y, n + _destinationPos.z, water);
					}
					BlockValue block = this.GetBlock(m, l, n);
					Block block2 = block.Block;
					if (block2 != null && (!block.isair || _pasteAirBlocks) && (!_pasteAirBlocks || !block.ischild))
					{
						TextureFullArray texture = this.GetTexture(m, l, n);
						sbyte density = this.GetDensity(m, l, n);
						list.Add(new BlockChangeInfo(new Vector3i(m + _destinationPos.x, l + _destinationPos.y, n + _destinationPos.z), block, density, texture));
						Vector3i vector3i;
						if (block2.IsTileEntitySavedInPrefab())
						{
							vector3i = new Vector3i(m, l, n);
							TileEntity tileEntity;
							if ((tileEntity = this.GetTileEntity(vector3i)) != null)
							{
								dictionary.Add(vector3i, tileEntity);
							}
						}
						vector3i = new Vector3i(m, l, n);
						BlockTrigger blockTrigger;
						if ((blockTrigger = this.GetBlockTrigger(vector3i)) != null)
						{
							dictionary2.Add(vector3i, blockTrigger);
						}
					}
				}
			}
		}
		_gm.SetBlocksRPC(list, null);
		_gm.SetWaterRPC(package);
		if (_pasteAirBlocks)
		{
			this.AddAllChildBlocks();
		}
		bool flag = this.PrefabName.StartsWith("part_");
		foreach (KeyValuePair<Vector3i, TileEntity> keyValuePair in dictionary)
		{
			Vector3i vector3i2 = keyValuePair.Key + _destinationPos;
			TileEntity tileEntity2 = _gm.World.GetTileEntity(vector3i2);
			if (tileEntity2 == null || flag)
			{
				Chunk chunk = (Chunk)_gm.World.GetChunkFromWorldPos(vector3i2);
				Vector3i vector3i3 = World.toBlock(vector3i2);
				if (flag)
				{
					chunk.RemoveTileEntityAt<TileEntity>(_gm.World, vector3i3);
				}
				tileEntity2 = keyValuePair.Value.Clone();
				tileEntity2.SetChunk(chunk);
				tileEntity2.localChunkPos = vector3i3;
				chunk.AddTileEntity(tileEntity2);
			}
			Vector3i localChunkPos = tileEntity2.localChunkPos;
			tileEntity2.CopyFrom(keyValuePair.Value);
			tileEntity2.localChunkPos = localChunkPos;
			tileEntity2.SetModified();
		}
		foreach (KeyValuePair<Vector3i, BlockTrigger> keyValuePair2 in dictionary2)
		{
			Vector3i vector3i4 = keyValuePair2.Key + _destinationPos;
			BlockTrigger blockTrigger2 = _gm.World.GetBlockTrigger(vector3i4);
			if (blockTrigger2 == null || flag)
			{
				Chunk chunk2 = (Chunk)_gm.World.GetChunkFromWorldPos(vector3i4);
				Vector3i vector3i5 = World.toBlock(vector3i4);
				if (flag)
				{
					chunk2.RemoveTileEntityAt<TileEntity>(_gm.World, vector3i5);
				}
				blockTrigger2 = keyValuePair2.Value.Clone();
				blockTrigger2.Chunk = chunk2;
				blockTrigger2.LocalChunkPos = vector3i5;
				chunk2.AddBlockTrigger(blockTrigger2);
			}
			Vector3i localChunkPos2 = blockTrigger2.LocalChunkPos;
			blockTrigger2.CopyFrom(keyValuePair2.Value);
			blockTrigger2.LocalChunkPos = localChunkPos2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopyVolumesIntoWorld(World _world, Chunk _chunk, Vector3i _offset)
	{
		foreach (PrefabVolumeListAbs prefabVolumeListAbs in this.AllVolumeLists)
		{
			prefabVolumeListAbs.CopyVolumesIntoWorld(_world, _chunk, _offset);
		}
	}

	public void MoveVolumes(Vector3i moveDistance)
	{
		foreach (PrefabVolumeListAbs prefabVolumeListAbs in this.AllVolumeLists)
		{
			prefabVolumeListAbs.Move(moveDistance);
		}
	}

	public short FindSleeperVolumeFreeGroupId()
	{
		int num = 0;
		for (int i = 0; i < this.SleeperVolumeList.Count; i++)
		{
			PrefabSleeperVolume prefabSleeperVolume = this.SleeperVolumeList[i];
			if ((int)prefabSleeperVolume.groupId > num)
			{
				num = (int)prefabSleeperVolume.groupId;
			}
		}
		return (short)(num + 1);
	}

	public void CopySleeperBlocksContainedInVolume(int _volumeIndex, Vector3i _offset, SleeperVolume _volume, Vector3i _volumeMins, Vector3i _volumeMaxs)
	{
		int num = Mathf.Max(_volumeMins.x, 0);
		int num2 = Mathf.Max(_volumeMins.y, 0);
		int num3 = Mathf.Max(_volumeMins.z, 0);
		int num4 = Mathf.Min(this.size.x, _volumeMaxs.x);
		int num5 = Mathf.Min(this.size.y, _volumeMaxs.y);
		int num6 = Mathf.Min(this.size.z, _volumeMaxs.z);
		for (int i = num; i < num4; i++)
		{
			int num7 = i + _offset.x;
			for (int j = num3; j < num6; j++)
			{
				int num8 = j + _offset.z;
				for (int k = num2; k < num5; k++)
				{
					if (k <= 0 || !this.GetBlockNoDamage(this.localRotation, i, k - 1, j).Block.IsSleeperBlock)
					{
						BlockValue block = this.GetBlock(i, k, j);
						Block block2 = block.Block;
						if (block2.IsSleeperBlock)
						{
							int num9 = k + _offset.y;
							Vector3i vector3i = new Vector3i(i, k, j);
							if (!this.IsPosInSleeperPriorityVolume(vector3i, _volumeIndex))
							{
								_volume.AddSpawnPoint(num7, num9, num8, (BlockSleeper)block2, block);
							}
						}
					}
				}
			}
		}
	}

	public PrefabSleeperVolume FindSleeperVolume(Vector3i _pos)
	{
		PrefabSleeperVolume prefabSleeperVolume = null;
		for (int i = 0; i < this.SleeperVolumeList.Count; i++)
		{
			PrefabSleeperVolume prefabSleeperVolume2 = this.SleeperVolumeList[i];
			if (prefabSleeperVolume2.Used && this.IsPosInSleeperVolume(prefabSleeperVolume2, _pos))
			{
				prefabSleeperVolume = prefabSleeperVolume2;
				if (prefabSleeperVolume2.isPriority)
				{
					break;
				}
			}
		}
		return prefabSleeperVolume;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPosInSleeperPriorityVolume(Vector3i _pos, int _skipIndex)
	{
		for (int i = 0; i < this.SleeperVolumeList.Count; i++)
		{
			if (i != _skipIndex)
			{
				PrefabSleeperVolume prefabSleeperVolume = this.SleeperVolumeList[i];
				if (prefabSleeperVolume.Used && prefabSleeperVolume.isPriority && this.IsPosInSleeperVolume(prefabSleeperVolume, _pos))
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPosInSleeperVolume(PrefabSleeperVolume _volume, Vector3i _pos)
	{
		if (!_volume.Used)
		{
			return false;
		}
		Vector3i startPos = _volume.startPos;
		Vector3i vector3i = startPos + _volume.size;
		return _pos.x >= startPos.x && _pos.x < vector3i.x && _pos.y >= startPos.y && _pos.y < vector3i.y && _pos.z >= startPos.z && _pos.z < vector3i.z;
	}

	public void CountSleeperSpawnsInVolume(World _world, Vector3i _offset, int _index)
	{
		this.Transient_NumSleeperSpawns = 0;
		PrefabSleeperVolume prefabSleeperVolume = this.SleeperVolumeList[_index];
		Vector3i startPos = prefabSleeperVolume.startPos;
		Vector3i vector3i = prefabSleeperVolume.size;
		Vector3i vector3i2 = startPos + vector3i;
		Vector3i vector3i3 = startPos + _offset;
		Vector3i vector3i4 = vector3i2 + _offset;
		int x = vector3i3.x;
		int y = vector3i3.y;
		int z = vector3i3.z;
		int x2 = vector3i4.x;
		int y2 = vector3i4.y;
		int z2 = vector3i4.z;
		for (int i = z; i < z2; i++)
		{
			for (int j = y; j < y2; j++)
			{
				for (int k = x; k < x2; k++)
				{
					if (_world.GetBlock(k, j, i).Block.IsSleeperBlock && !_world.GetBlock(k, j - 1, i).Block.IsSleeperBlock)
					{
						Vector3i vector3i5 = new Vector3i(k - _offset.x, j - _offset.y, i - _offset.z);
						if (!this.IsPosInSleeperPriorityVolume(vector3i5, _index))
						{
							this.Transient_NumSleeperSpawns++;
						}
					}
				}
			}
		}
	}

	public static void TransientSleeperBlockIncrement(Vector3i _point, int _c)
	{
		SelectionBox selection = SelectionBoxManager.Instance.Selection;
		if (selection == null || selection.Category != SelectionBoxManager.Instance.CategorySleeperVolume)
		{
			return;
		}
		if (!selection.WorldPosWithinBox(_point))
		{
			return;
		}
		int num;
		PrefabInstance prefabInstance;
		if (!PrefabVolumeManager.GetPrefabIdAndVolumeId(selection.name, out num, out prefabInstance))
		{
			return;
		}
		Prefab prefab = prefabInstance.prefab;
		if (num >= prefab.SleeperVolumeList.Count)
		{
			return;
		}
		if (!prefab.SleeperVolumeList[num].Used)
		{
			return;
		}
		prefab.Transient_NumSleeperSpawns += _c;
	}

	public string CalcSleeperInfo()
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		for (int i = 0; i < this.SleeperVolumeList.Count; i++)
		{
			PrefabSleeperVolume prefabSleeperVolume = this.SleeperVolumeList[i];
			int spawnCountMin = (int)prefabSleeperVolume.spawnCountMin;
			if (spawnCountMin < 0)
			{
				flag = true;
			}
			else
			{
				num += spawnCountMin;
			}
			int spawnCountMax = (int)prefabSleeperVolume.spawnCountMax;
			if (spawnCountMax < 0)
			{
				flag = true;
			}
			else
			{
				num2 += spawnCountMax;
			}
		}
		string text = string.Format("{0}, {1}-{2}", this.SleeperVolumeList.Count, num, num2);
		if (flag)
		{
			text += "*";
		}
		return text;
	}

	public void CopyIntoLocal(ChunkCluster _cluster, Vector3i _destinationPos, bool _bOverwriteExistingBlocks, bool _bSetChunkToRegenerate, FastTags<TagGroup.Global> _questTags)
	{
		World world = _cluster.GetWorld();
		bool flag = world.IsEditor();
		if (!flag)
		{
			this.CopyVolumesIntoWorld(world, null, _destinationPos);
		}
		Chunk chunk = _cluster.GetChunkSync(World.toChunkXZ(_destinationPos.x), World.toChunkXZ(_destinationPos.z));
		int seed = world.Seed;
		GameRandom gameRandom = ((chunk != null) ? Utils.RandomFromSeedOnPos(chunk.X, chunk.Z, seed) : null);
		GameRandom gameRandom2 = GameRandomManager.Instance.CreateGameRandom((int)world.GetWorldTime());
		if (this.terrainFillerType == 0)
		{
			this.InitTerrainFillers();
		}
		for (int i = this.size.y + _destinationPos.y; i < 255; i++)
		{
			int num = World.toBlockY(i);
			bool flag2 = false;
			for (int j = 0; j < this.size.z; j++)
			{
				int num2 = j + _destinationPos.z;
				int num3 = World.toChunkXZ(num2);
				int num4 = World.toBlockXZ(num2);
				int k = 0;
				while (k < this.size.x)
				{
					int num5 = k + _destinationPos.x;
					int num6 = World.toChunkXZ(num5);
					int num7 = World.toBlockXZ(num5);
					if (chunk != null && chunk.X == num6 && chunk.Z == num3)
					{
						goto IL_0103;
					}
					chunk = _cluster.GetChunkSync(num6, num3);
					if (chunk != null)
					{
						goto IL_0103;
					}
					IL_0151:
					k++;
					continue;
					IL_0103:
					BlockValue block = chunk.GetBlock(num7, num, num4);
					if (block.isair || block.Block.shape.IsTerrain())
					{
						goto IL_0151;
					}
					flag2 = true;
					if (!block.ischild)
					{
						chunk.SetBlock(world, num7, num, num4, BlockValue.Air, true, true, false, true, -1);
						goto IL_0151;
					}
					goto IL_0151;
				}
			}
			if (!flag2)
			{
				break;
			}
		}
		if (_bOverwriteExistingBlocks)
		{
			this.DestroyAllMultiblocks(_cluster, _destinationPos);
		}
		for (int l = 0; l < this.size.z; l++)
		{
			int num8 = l + _destinationPos.z;
			int num9 = World.toChunkXZ(num8);
			int num10 = World.toBlockXZ(num8);
			int m = 0;
			while (m < this.size.x)
			{
				int num11 = m + _destinationPos.x;
				int num12 = World.toChunkXZ(num11);
				int num13 = World.toBlockXZ(num11);
				if (chunk != null && chunk.X == num12 && chunk.Z == num9)
				{
					goto IL_0262;
				}
				chunk = _cluster.GetChunkSync(num12, num9);
				GameRandomManager.Instance.FreeGameRandom(gameRandom);
				gameRandom = null;
				if (chunk != null)
				{
					goto IL_0262;
				}
				Debug.LogError(string.Format("Chunk ({0}, {1}) unavailable during POI reset. Skipping reset for all POI blocks at XZ world position ({2},{3}).", new object[] { num12, num9, num11, num8 }));
				IL_06B8:
				m++;
				continue;
				IL_0262:
				if (gameRandom == null)
				{
					gameRandom = Utils.RandomFromSeedOnPos(num12, num9, seed);
				}
				int num14 = _destinationPos.y - 1;
				bool flag3 = false;
				for (int n = 0; n < this.size.y; n++)
				{
					WaterValue water = this.GetWater(m, n, l);
					BlockValue blockValue = this.GetBlock(m, n, l);
					if (this.bCopyAirBlocks || !blockValue.isair || water.HasMass())
					{
						int num15 = World.toBlockY(n + _destinationPos.y);
						BlockValueRef blockValueRef = new BlockValueRef(num13, num15, num10);
						BlockValue block2 = chunk.GetBlock(num13, num15, num10);
						BlockValue blockValue2 = blockValue;
						sbyte b = this.GetDensity(m, n, l);
						bool flag4 = false;
						if (!blockValue.isair && !flag)
						{
							if (blockValue.Block.IsSleeperBlock)
							{
								flag4 = true;
								blockValue = BlockValue.Air;
							}
							if (blockValue.type == this.terrainFillerType)
							{
								BlockValue blockValue3 = block2;
								Block block3 = blockValue3.Block;
								if (blockValue3.isair || block3 == null || !block3.shape.IsTerrain())
								{
									int terrainHeight = (int)chunk.GetTerrainHeight(num13, num10);
									blockValue3 = chunk.GetBlock(num13, terrainHeight, num10);
									block3 = blockValue3.Block;
									if (blockValue3.isair || block3 == null || !block3.shape.IsTerrain())
									{
										goto IL_066A;
									}
								}
								blockValue = blockValue3;
								flag3 = true;
							}
							if (blockValue.type == this.terrainFiller2Type)
							{
								Block block4 = block2.Block;
								if (!block2.isair && block4 != null && block4.shape.IsTerrain())
								{
									blockValue = block2;
									b = 0;
								}
								else
								{
									blockValue = BlockValue.Air;
									b = MarchingCubes.DensityAir;
								}
							}
							if (blockValue.Block.isMultiBlock && MultiBlockManager.Instance.POIMBTrackingEnabled)
							{
								this.ProcessMultiBlock(ref blockValue, chunk, new Vector3i(m, n, l), new Vector3i(num13, num15, num10), _questTags, _bOverwriteExistingBlocks);
							}
							else if (BlockPlaceholderMap.Instance.IsReplaceableBlockType(blockValue))
							{
								byte meta = blockValue.meta;
								blockValue = BlockPlaceholderMap.Instance.Replace(blockValueRef, blockValue, GameManager.Instance.World.GetGameRandom(), chunk, _questTags, _bOverwriteExistingBlocks, true);
								blockValue.meta = meta;
							}
						}
						if (b == 0)
						{
							b = MarchingCubes.DensityAir;
							if (blockValue.Block.shape.IsTerrain())
							{
								b = MarchingCubes.DensityTerrain;
							}
						}
						if (block2.ischild || (!_bOverwriteExistingBlocks && !block2.isair && !block2.Block.shape.IsTerrain()))
						{
							chunk.SetDensity(num13, num15, num10, b);
						}
						else
						{
							chunk.SetDecoAllowedSizeAt(num13, num10, EnumDecoAllowedSize.OnlySmall);
							if (!flag4)
							{
								TextureFullArray texture = this.GetTexture(m, n, l);
								chunk.GetSetTextureFullArray(num13, num15, num10, texture);
							}
							chunk.SetBlock(world, num13, num15, num10, blockValue, true, true, !_questTags.IsEmpty, true, -1);
							chunk.SetWater(num13, num15, num10, water);
							Vector3i vector3i = new Vector3i(m, n, l);
							TileEntity tileEntity;
							if (blockValue2.Block.IsTileEntitySavedInPrefab() && (tileEntity = this.GetTileEntity(vector3i)) != null)
							{
								Vector3i vector3i2 = new Vector3i(num13, num15, num10);
								TileEntity tileEntity2 = chunk.GetTileEntity(vector3i2);
								if (tileEntity2 == null)
								{
									tileEntity2 = tileEntity.Clone();
									tileEntity2.localChunkPos = vector3i2;
									tileEntity2.SetChunk(chunk);
									chunk.AddTileEntity(tileEntity2);
								}
								tileEntity2.CopyFrom(tileEntity);
								tileEntity2.localChunkPos = vector3i2;
							}
							BlockTrigger blockTrigger = this.GetBlockTrigger(vector3i);
							if (blockTrigger != null)
							{
								BlockTrigger blockTrigger2 = chunk.GetBlockTrigger(new Vector3i(num13, num15, num10));
								if (blockTrigger2 == null)
								{
									blockTrigger2 = blockTrigger.Clone();
									blockTrigger2.LocalChunkPos = new Vector3i(num13, num15, num10);
									blockTrigger2.Chunk = chunk;
									chunk.AddBlockTrigger(blockTrigger2);
								}
								blockTrigger2.CopyFrom(blockTrigger);
								blockTrigger2.LocalChunkPos = new Vector3i(num13, num15, num10);
								blockValue.Block.OnTriggerAddedFromPrefab(blockTrigger2, blockTrigger2.LocalChunkPos, blockValue, FastTags<TagGroup.Global>.Parse(this.questTags.ToString()));
							}
							if (blockValue.Block.shape.IsTerrain())
							{
								num14 = num15;
							}
							chunk.SetDensity(num13, num15, num10, b);
						}
					}
					IL_066A:;
				}
				if (num14 >= 0)
				{
					chunk.SetTerrainHeight(num13, num10, (byte)num14);
				}
				if (!flag3)
				{
					chunk.SetTopSoilBroken(num13, num10);
				}
				chunk.SetDecoAllowedSizeAt(num13, num10, EnumDecoAllowedSize.OnlySmall);
				if (_bSetChunkToRegenerate)
				{
					chunk.NeedsRegeneration = true;
					goto IL_06B8;
				}
				goto IL_06B8;
			}
		}
		this.ApplyDecoAllowed(_cluster, _destinationPos);
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
		GameRandomManager.Instance.FreeGameRandom(gameRandom2);
	}

	public void DestroyAllMultiblocks(ChunkCluster _cluster, Vector3i _destinationPos)
	{
		Chunk chunk = _cluster.GetChunkSync(World.toChunkXZ(_destinationPos.x), World.toChunkXZ(_destinationPos.z));
		for (int i = 0; i < this.size.z; i++)
		{
			int num = i + _destinationPos.z;
			int num2 = World.toChunkXZ(num);
			int num3 = World.toBlockXZ(num);
			int j = 0;
			while (j < this.size.x)
			{
				int num4 = j + _destinationPos.x;
				int num5 = World.toChunkXZ(num4);
				int num6 = World.toBlockXZ(num4);
				if (chunk != null && chunk.X == num5 && chunk.Z == num2)
				{
					goto IL_00C1;
				}
				chunk = _cluster.GetChunkSync(num5, num2);
				if (chunk != null)
				{
					goto IL_00C1;
				}
				Debug.LogError(string.Format("Chunk ({0}, {1}) unavailable during POI reset. Skipping reset for all POI blocks at XZ world position ({2},{3}).", new object[] { num5, num2, num4, num }));
				IL_012D:
				j++;
				continue;
				IL_00C1:
				for (int k = 0; k < this.size.y; k++)
				{
					int num7 = World.toBlockY(k + _destinationPos.y);
					BlockValue block = chunk.GetBlock(num6, num7, num3);
					if (block.Block.isMultiBlock && !block.ischild)
					{
						chunk.SetBlock(_cluster.GetWorld(), num6, num7, num3, BlockValue.Air, true, true, false, false, -1);
					}
				}
				goto IL_012D;
			}
		}
		foreach (int num8 in this.multiBlockParentIndices)
		{
			int num9;
			int num10;
			int num11;
			this.offsetToCoordRotated(num8, out num9, out num10, out num11);
			Vector3i vector3i = new Vector3i(num9, num10, num11) + _destinationPos;
			MultiBlockManager.Instance.DeregisterTrackedBlockData(vector3i);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyDecoAllowed(ChunkCluster _cluster, Vector3i _prefabTargetPos)
	{
		int num = World.toChunkXZ(_prefabTargetPos.x);
		int num2 = World.toChunkXZ(_prefabTargetPos.z);
		int num3 = World.toChunkXZ(_prefabTargetPos.x + this.size.x - 1);
		int num4 = World.toChunkXZ(_prefabTargetPos.z + this.size.z - 1);
		for (int i = num2; i <= num4; i++)
		{
			for (int j = num; j <= num3; j++)
			{
				Chunk chunkSync = _cluster.GetChunkSync(j, i);
				if (chunkSync != null)
				{
					this.ApplyDecoAllowed(chunkSync, _prefabTargetPos);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessMultiBlock(ref BlockValue targetBV, Chunk chunk, Vector3i prefabRelPos, Vector3i chunkRelPos, FastTags<TagGroup.Global> questTags, bool overwriteExistingBlocks)
	{
		if (!targetBV.Block.isMultiBlock)
		{
			Debug.LogError("[MultiBlockManager] BlockValue passed into ProcessMultiBlock is not a MultiBlock.");
			return;
		}
		Vector3i vector3i = chunk.GetWorldPos() + (chunkRelPos - prefabRelPos);
		Vector3i vector3i2 = prefabRelPos;
		if (targetBV.ischild)
		{
			vector3i2 += new Vector3i(targetBV.parentx, targetBV.parenty, targetBV.parentz);
		}
		Vector3i vector3i3 = vector3i + vector3i2;
		MultiBlockManager.TrackedBlockData trackedBlockData;
		BlockValue blockValue;
		if (MultiBlockManager.Instance.TryGetPOIMultiBlock(vector3i3, out trackedBlockData))
		{
			blockValue = new BlockValue(trackedBlockData.rawData);
		}
		else
		{
			BlockValue blockValue2;
			if (targetBV.ischild)
			{
				blockValue2 = this.GetBlock(vector3i2.x, vector3i2.y, vector3i2.z);
			}
			else
			{
				blockValue2 = targetBV;
			}
			if (BlockPlaceholderMap.Instance.IsReplaceableBlockType(blockValue2))
			{
				byte meta = blockValue2.meta;
				blockValue = BlockPlaceholderMap.Instance.Replace(blockValue2, GameManager.Instance.World.GetGameRandom(), chunk, chunkRelPos.x, chunkRelPos.y, chunkRelPos.z, questTags, overwriteExistingBlocks, false);
				if (blockValue.isair)
				{
					blockValue = BlockValue.Air;
				}
				else
				{
					blockValue.meta = meta;
				}
			}
			else
			{
				blockValue = blockValue2;
			}
			MultiBlockManager.Instance.DeregisterTrackedBlockData(vector3i3);
			if (!MultiBlockManager.Instance.TryRegisterPOIMultiBlock(vector3i3, blockValue))
			{
				Debug.LogError("[MultiBlockManager] Failed to register POI MultiBlock.");
			}
		}
		if (blockValue.type == targetBV.type)
		{
			return;
		}
		if (blockValue.isair)
		{
			targetBV = BlockValue.Air;
			return;
		}
		if (blockValue.Block.isMultiBlock)
		{
			Vector3i dim = targetBV.Block.multiBlockPos.dim;
			Vector3i dim2 = blockValue.Block.multiBlockPos.dim;
			if (dim2 != dim)
			{
				if (dim2.x > dim.x || dim2.y > dim.y || dim2.z > dim.z)
				{
					Debug.LogWarning(string.Concat(new string[]
					{
						"[MultiBlockManager] The replacement block \"",
						blockValue.Block.GetBlockName(),
						"\" is larger than the original block \"",
						targetBV.Block.GetBlockName(),
						"\" in dimensions. \n",
						string.Format("Replacement size: \"{0}\", Original size: \"{1}\". ", dim2, dim),
						string.Format("Parent world position: {0}.\n", vector3i3),
						"Child blocks of the replacement will not be placed outside the original block's dimensions. \nNote: We expect to see this warning when single-block helpers are used to place MultiBlocks at 45-degree rotations. Many of these instances will be resolved by converting to the new oversized block format in the near future. \nIn situations where 45-degree rotations aren't needed, helper blocks should be set to the maximum dimensions of any possible replacements. Affected prefabs may need to be re-saved to implement these changes."
					}));
				}
				if (dim2.x < dim.x || dim2.y < dim.y || dim2.z < dim.z)
				{
					Vector3i vector3i4;
					Vector3i vector3i5;
					MultiBlockManager.GetMinMaxWorldPositions(vector3i3, blockValue, out vector3i4, out vector3i5);
					Vector3i vector3i6 = vector3i + prefabRelPos;
					if (vector3i6.x < vector3i4.x || vector3i6.x > vector3i5.x || vector3i6.y < vector3i4.y || vector3i6.y > vector3i5.y || vector3i6.z < vector3i4.z || vector3i6.z > vector3i5.z)
					{
						targetBV = BlockValue.Air;
						return;
					}
				}
			}
			targetBV.type = blockValue.type;
			if (!targetBV.ischild)
			{
				targetBV.rotation = blockValue.rotation;
			}
			return;
		}
		if (targetBV.ischild)
		{
			targetBV = BlockValue.Air;
			return;
		}
		targetBV.type = blockValue.type;
		targetBV.rotation = blockValue.rotation;
	}

	public void SnapTerrainToArea(ChunkCluster _cluster, Vector3i _destinationPos)
	{
		for (int i = -1; i < this.size.x + 1; i++)
		{
			for (int j = -1; j < this.size.z + 1; j++)
			{
				bool flag = i == -1 || j == -1 || i == this.size.x || j == this.size.z;
				_cluster.SnapTerrainToPositionAtLocal(new Vector3i(_destinationPos.x + i, _destinationPos.y - 1, _destinationPos.z + j), true, flag);
			}
		}
	}

	public void CopyEntitiesIntoWorld(World _world, Vector3i _destinationPos, ICollection<int> _entityIds, bool _bSpawnEnemies)
	{
		if (_entityIds != null)
		{
			_entityIds.Clear();
		}
		for (int i = 0; i < this.entities.Count; i++)
		{
			EntityCreationData entityCreationData = this.entities[i];
			entityCreationData.id = -1;
			if (_bSpawnEnemies || !EntityClass.list[entityCreationData.entityClass].bIsEnemyEntity)
			{
				Entity entity = EntityFactory.CreateEntity(entityCreationData);
				entity.SetPosition(entity.position + _destinationPos.ToVector3(), true);
				_world.SpawnEntityInWorld(entity);
				if (_entityIds != null)
				{
					_entityIds.Add(entity.entityId);
				}
			}
		}
	}

	public void CopyEntitiesIntoChunkStub(Chunk _chunk, Vector3i _destinationPos, ICollection<int> _entityIds, bool _bSpawnEnemies)
	{
		for (int i = 0; i < this.entities.Count; i++)
		{
			EntityCreationData entityCreationData = this.entities[i];
			if (EntityClass.list.ContainsKey(entityCreationData.entityClass) && (_bSpawnEnemies || !EntityClass.list[entityCreationData.entityClass].bIsEnemyEntity))
			{
				int num = Utils.Fastfloor(entityCreationData.pos.x) + _destinationPos.x;
				int num2 = Utils.Fastfloor(entityCreationData.pos.z) + _destinationPos.z;
				if (_chunk.X == World.toChunkXZ(num) && _chunk.Z == World.toChunkXZ(num2))
				{
					EntityCreationData entityCreationData2 = entityCreationData.Clone();
					entityCreationData2.pos += _destinationPos.ToVector3() + new Vector3(0f, 0.25f, 0f);
					entityCreationData2.id = EntityFactory.nextEntityID++;
					_chunk.AddEntityStub(entityCreationData2);
					if (_entityIds != null)
					{
						_entityIds.Add(entityCreationData2.id);
					}
				}
			}
		}
	}

	public static Vector3i SizeFromPositions(Vector3i _posStart, Vector3i _posEnd)
	{
		Vector3i vector3i = new Vector3i(Math.Min(_posStart.x, _posEnd.x), Math.Min(_posStart.y, _posEnd.y), Math.Min(_posStart.z, _posEnd.z));
		Vector3i vector3i2 = new Vector3i(Math.Max(_posStart.x, _posEnd.x), Math.Max(_posStart.y, _posEnd.y), Math.Max(_posStart.z, _posEnd.z));
		return new Vector3i(Math.Abs(vector3i2.x - vector3i.x) + 1, Math.Abs(vector3i2.y - vector3i.y) + 1, Math.Abs(vector3i2.z - vector3i.z) + 1);
	}

	public Vector3i copyFromWorld(World _world, Vector3i _posStart, Vector3i _posEnd)
	{
		Vector3i vector3i = Vector3i.Min(_posStart, _posEnd);
		Vector3i vector3i2 = Vector3i.Max(_posStart, _posEnd);
		this.size.x = Math.Abs(vector3i2.x - vector3i.x) + 1;
		this.size.y = Math.Abs(vector3i2.y - vector3i.y) + 1;
		this.size.z = Math.Abs(vector3i2.z - vector3i.z) + 1;
		this.localRotation = 0;
		this.InitData();
		this.tileEntities.Clear();
		int num = 0;
		int i = vector3i.y;
		while (i <= vector3i2.y)
		{
			int num2 = 0;
			int j = vector3i.x;
			while (j <= vector3i2.x)
			{
				int num3 = 0;
				int k = vector3i.z;
				while (k <= vector3i2.z)
				{
					BlockValue blockValue = _world.GetBlock(j, i, k);
					if (blockValue.isWater)
					{
						this.SetWater(j, i, k, WaterValue.Full);
						blockValue = BlockValue.Air;
					}
					this.SetDensity(num2, num, num3, _world.GetDensity(j, i, k));
					if (!blockValue.ischild)
					{
						this.SetBlock(num2, num, num3, blockValue);
						this.SetWater(num2, num, num3, _world.GetWater(j, i, k));
						this.SetTexture(num2, num, num3, _world.GetTextureFullArray(j, i, k));
						if (blockValue.Block.IsTileEntitySavedInPrefab())
						{
							Vector3i vector3i3 = new Vector3i(j, i, k);
							TileEntity tileEntity = _world.GetTileEntity(vector3i3);
							if (tileEntity != null)
							{
								TileEntity tileEntity2 = tileEntity.Clone();
								tileEntity2.localChunkPos = vector3i3 - vector3i;
								this.tileEntities.Add(tileEntity2.localChunkPos, tileEntity2);
							}
						}
					}
					k++;
					num3++;
				}
				j++;
				num2++;
			}
			i++;
			num++;
		}
		return vector3i;
	}

	public Vector3i CopyFromWorldWithEntities(World _world, Vector3i _posStart, Vector3i _posEnd, ICollection<int> _entityIds)
	{
		this.copyFromWorld(_world, _posStart, _posEnd);
		Vector3i vector3i = Vector3i.Min(_posStart, _posEnd);
		Vector3i vector3i2 = Vector3i.Max(_posStart, _posEnd);
		this.entities.Clear();
		int num = World.toChunkXZ(vector3i.x);
		int num2 = World.toChunkXZ(vector3i.z);
		int num3 = World.toChunkXZ(vector3i2.x);
		int num4 = World.toChunkXZ(vector3i2.z);
		Bounds bounds = BoundsUtils.BoundsForMinMax((float)vector3i.x, (float)vector3i.y, (float)vector3i.z, (float)(vector3i2.x + 1), (float)(vector3i2.y + 1), (float)(vector3i2.z + 1));
		List<Entity> list = new List<Entity>();
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				Chunk chunk = (Chunk)_world.GetChunkSync(i, j);
				if (chunk != null)
				{
					chunk.GetEntitiesInBounds(typeof(Entity), bounds, list);
				}
			}
		}
		this.indexedBlockOffsets.Clear();
		this.triggerData.Clear();
		for (int k = num; k <= num3; k++)
		{
			for (int l = num2; l <= num4; l++)
			{
				Chunk chunk2 = (Chunk)_world.GetChunkSync(k, l);
				if (chunk2 != null)
				{
					foreach (KeyValuePair<string, List<Vector3i>> keyValuePair in chunk2.IndexedBlocks.Dict)
					{
						if (keyValuePair.Value != null && keyValuePair.Value.Count > 0)
						{
							List<Vector3i> list2 = new List<Vector3i>();
							this.indexedBlockOffsets[keyValuePair.Key] = list2;
							foreach (Vector3i vector3i3 in keyValuePair.Value)
							{
								Vector3i vector3i4 = chunk2.ToWorldPos(vector3i3);
								Vector3 vector = vector3i4.ToVector3();
								Vector3i vector3i5 = vector3i4 - vector3i;
								if (bounds.Contains(vector))
								{
									list2.Add(vector3i5);
								}
							}
						}
					}
					List<BlockTrigger> list3 = chunk2.GetBlockTriggers().list;
					for (int m = 0; m < list3.Count; m++)
					{
						BlockTrigger blockTrigger = list3[m].Clone();
						blockTrigger.LocalChunkPos = chunk2.ToWorldPos(list3[m].LocalChunkPos) - _posStart;
						this.triggerData.Add(blockTrigger.LocalChunkPos, blockTrigger);
					}
				}
			}
		}
		if (_entityIds != null)
		{
			_entityIds.Clear();
		}
		for (int n = 0; n < list.Count; n++)
		{
			Entity entity = list[n];
			if (!(entity is EntityPlayer))
			{
				EntityCreationData entityCreationData = new EntityCreationData(entity, true);
				entityCreationData.pos -= new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
				this.entities.Add(entityCreationData);
				if (_entityIds != null)
				{
					_entityIds.Add(entity.entityId);
				}
			}
		}
		return vector3i;
	}

	public BlockValue Get(int _absy)
	{
		int num = this.currX;
		int num2 = this.currZ;
		if (num >= 0 && num < this.size.x && _absy >= 0 && _absy < this.size.y && num2 >= 0 && num2 < this.size.z)
		{
			return this.GetBlock(num, _absy, num2);
		}
		return BlockValue.Air;
	}

	public BlockValue Get(int _relx, int _absy, int _relz)
	{
		int num = this.currX + _relx;
		int num2 = this.currZ + _relz;
		if (num >= 0 && num < this.size.x && _absy >= 0 && _absy < this.size.y && num2 >= 0 && num2 < this.size.z)
		{
			return this.GetBlock(num, _absy, num2);
		}
		return BlockValue.Air;
	}

	public IChunk GetChunk(int _x, int _z)
	{
		long num = WorldChunkCache.MakeChunkKey(_x, _z);
		Prefab.PrefabChunk prefabChunk;
		if (!this.dictChunks.TryGetValue(num, out prefabChunk))
		{
			prefabChunk = new Prefab.PrefabChunk(this, _x, _z);
			this.dictChunks.Add(num, prefabChunk);
		}
		return prefabChunk;
	}

	public List<IChunk> GetChunks()
	{
		if (this.dictChunks.Count == 0)
		{
			int i = 0;
			int num = 0;
			while (i < this.size.x + 1)
			{
				int j = 0;
				int num2 = 0;
				while (j < this.size.z + 1)
				{
					this.GetChunk(num, num2);
					j += 16;
					num2++;
				}
				i += 16;
				num++;
			}
		}
		return this.dictChunks.Values.ToList<IChunk>();
	}

	public IChunk GetNeighborChunk(int _x, int _z)
	{
		return this.GetChunk(_x, _z);
	}

	public bool IsWater(int _relx, int _absy, int _relz)
	{
		int num = this.currX + _relx;
		int num2 = this.currZ + _relz;
		return num >= 0 && num < this.size.x && _absy >= 0 && _absy < this.size.y && num2 >= 0 && num2 < this.size.z && this.GetWater(num, _absy, num2).HasMass();
	}

	public bool IsAir(int _relx, int _absy, int _relz)
	{
		int num = this.currX + _relx;
		int num2 = this.currZ + _relz;
		return num >= 0 && num < this.size.x && _absy >= 0 && _absy < this.size.y && num2 >= 0 && num2 < this.size.z && this.GetBlock(num, _absy, num2).isair && !this.GetWater(num, _absy, num2).HasMass();
	}

	public void Init(int _bX, int _bZ)
	{
		this.currX = _bX;
		this.currZ = _bZ;
		this.dictChunks = new Dictionary<long, Prefab.PrefabChunk>();
	}

	public void Clear()
	{
	}

	public void Cache()
	{
	}

	public void ToMesh(VoxelMesh[] _meshes)
	{
		new MeshGenerator(this).GenerateMesh(new Vector3i(-1, -1, -1), this.size + Vector3i.one, _meshes);
	}

	public Transform ToTransform()
	{
		MeshFilter[] array = new MeshFilter[MeshDescription.meshes.Length];
		MeshRenderer[] array2 = new MeshRenderer[MeshDescription.meshes.Length];
		MeshCollider[] array3 = new MeshCollider[MeshDescription.meshes.Length];
		GameObject[] array4 = new GameObject[MeshDescription.meshes.Length];
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = null;
		gameObject.name = "Prefab_" + this.PrefabName;
		GameObject gameObject2 = new GameObject("_BlockEntities");
		gameObject2.transform.parent = gameObject.transform;
		GameObject gameObject3 = new GameObject("Meshes");
		gameObject3.transform.parent = gameObject.transform;
		for (int i = 0; i < MeshDescription.meshes.Length; i++)
		{
			array4[i] = new GameObject(MeshDescription.meshes[i].Name);
			array4[i].transform.parent = gameObject3.transform;
			VoxelMesh.CreateMeshFilter(i, 0, array4[i], MeshDescription.meshes[i].Tag, false, out array[i], out array2[i], out array3[i]);
		}
		VoxelMesh[] array5 = new VoxelMesh[6];
		for (int j = 0; j < array5.Length; j++)
		{
			array5[j] = new VoxelMesh(j, 1024, VoxelMesh.CreateFlags.Default);
		}
		new MeshGenerator(this).GenerateMesh(new Vector3i(-1, -1, -1), this.size + Vector3i.one, array5);
		for (int k = 0; k < array5.Length; k++)
		{
			array5[k].CopyToMesh(array[k], array2[k], 0, null);
		}
		for (int l = 0; l < this.size.x; l++)
		{
			for (int m = 0; m < this.size.z; m++)
			{
				for (int n = 0; n < this.size.y; n++)
				{
					Vector3i vector3i = new Vector3i(l, n, m);
					BlockValue block = this.GetBlock(l, n, m);
					Block block2 = block.Block;
					if (!block2.isMultiBlock || !block.ischild)
					{
						BlockShapeModelEntity blockShapeModelEntity = block2.shape as BlockShapeModelEntity;
						if (blockShapeModelEntity != null)
						{
							Quaternion rotation = blockShapeModelEntity.GetRotation(block);
							Vector3 rotatedOffset = blockShapeModelEntity.GetRotatedOffset(block2, rotation);
							rotatedOffset.x += 0.5f;
							rotatedOffset.z += 0.5f;
							rotatedOffset.y += 0f;
							Vector3 vector = vector3i.ToVector3() + rotatedOffset;
							GameObject objectForType = GameObjectPool.Instance.GetObjectForType(blockShapeModelEntity.modelName);
							if (!(objectForType == null))
							{
								objectForType.SetActive(true);
								Transform transform = objectForType.transform;
								transform.parent = gameObject2.transform;
								transform.localScale = Vector3.one;
								transform.localPosition = vector;
								transform.localRotation = rotation;
							}
						}
					}
				}
			}
		}
		return gameObject.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BiomeDefinition> toBiomeArray(WorldBiomes _biomes, List<string> _biomeStrList)
	{
		List<BiomeDefinition> list = new List<BiomeDefinition>();
		for (int i = 0; i < _biomeStrList.Count; i++)
		{
			string text = _biomeStrList[i];
			BiomeDefinition biome;
			if ((biome = _biomes.GetBiome(text)) != null)
			{
				list.Add(biome);
			}
		}
		return list;
	}

	public string[] GetAllowedBiomes()
	{
		return this.allowedBiomes.ToArray();
	}

	public string[] GetAllowedZones()
	{
		return this.allowedZones.ToArray();
	}

	public bool IsAllowedZone(string _zone)
	{
		return this.allowedZones.ContainsCaseInsensitive(_zone);
	}

	public void AddAllowedZone(string _zone)
	{
		if (!this.IsAllowedZone(_zone))
		{
			this.allowedZones.Add(_zone);
		}
	}

	public void RemoveAllowedZone(string _zone)
	{
		int num = this.allowedZones.FindIndex((string _s) => _s.EqualsCaseInsensitive(_zone));
		if (num >= 0)
		{
			this.allowedZones.RemoveAt(num);
		}
	}

	public string[] GetAllowedTownships()
	{
		return this.allowedTownships.ToArray();
	}

	public void SetAllowedBiomes(string[] _b)
	{
		this.allowedBiomes = new List<string>(_b);
	}

	public List<BiomeDefinition> GetAllowedBiomes(WorldBiomes _biomes)
	{
		return this.toBiomeArray(_biomes, this.allowedBiomes);
	}

	public void CopyBlocksIntoChunkNoEntities(World _world, Chunk _chunk, Vector3i _prefabTargetPos, bool _bForceOverwriteBlocks, FastTags<TagGroup.Global> _questTags)
	{
		bool flag = this.IsCullThisPrefab() && GameStats.GetInt(EnumGameStats.OptionsPOICulling) > 1;
		bool flag2 = _world.IsEditor();
		if (this.terrainFillerType == 0)
		{
			this.InitTerrainFillers();
		}
		GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
		Bounds aabb = _chunk.GetAABB();
		int num = 0;
		int num2 = 0;
		int num3 = _prefabTargetPos.x - (int)aabb.min.x;
		int num4;
		if (num3 >= 0)
		{
			num = num3;
			num4 = Utils.FastMin(16 - num3, this.size.x);
		}
		else
		{
			num2 = -num3;
			num4 = Utils.FastMin(this.size.x + num3, 16);
		}
		int num5 = 0;
		int num6 = 0;
		int num7 = _prefabTargetPos.z - (int)aabb.min.z;
		int num8;
		if (num7 >= 0)
		{
			num5 = num7;
			num8 = Utils.FastMin(16 - num7, this.size.z);
		}
		else
		{
			num6 = -num7;
			num8 = Utils.FastMin(this.size.z + num7, 16);
		}
		for (int i = Utils.FastClamp(_prefabTargetPos.y + this.size.y, 0, 255); i < 256; i++)
		{
			bool flag3 = false;
			for (int j = 0; j < num8; j++)
			{
				int num9 = j + num5;
				for (int k = 0; k < num4; k++)
				{
					int num10 = k + num;
					BlockValue blockNoDamage = _chunk.GetBlockNoDamage(num10, i, num9);
					if (!blockNoDamage.isair && !blockNoDamage.Block.shape.IsTerrain())
					{
						flag3 = true;
						if (!blockNoDamage.ischild)
						{
							_chunk.SetBlock(_world, num10, i, num9, BlockValue.Air, true, true, false, true, -1);
						}
					}
				}
			}
			if (!flag3)
			{
				break;
			}
		}
		for (int l = 0; l < num8; l++)
		{
			int num11 = l + num5;
			int num12 = l + num6;
			for (int m = 0; m < num4; m++)
			{
				int num13 = m + num;
				int num14 = m + num2;
				int num15 = (int)_chunk.GetTerrainHeight(num13, num11);
				BlockValue blockValue = _chunk.GetBlock(num13, num15, num11);
				Block block = blockValue.Block;
				while ((blockValue.isair || block == null || !block.shape.IsTerrain()) && num15 > 0)
				{
					num15--;
					blockValue = _chunk.GetBlock(num13, num15, num11);
					block = blockValue.Block;
				}
				int num16 = _prefabTargetPos.y - 1;
				for (int n = 0; n < this.size.y; n++)
				{
					int num17 = n + _prefabTargetPos.y;
					if (num17 < 255)
					{
						BlockValue blockValue2 = this.GetBlock(num14, n, num12);
						bool flag4 = false;
						if (blockValue2.Block.IsSleeperBlock)
						{
							flag4 = true;
							blockValue2 = BlockValue.Air;
						}
						if (blockValue2.type == this.terrainFillerType && !flag2)
						{
							blockValue2 = blockValue;
						}
						sbyte b = this.GetDensity(num14, n, num12);
						if (blockValue2.type == this.terrainFiller2Type)
						{
							BlockValue block2 = _chunk.GetBlock(num13, num17, num11);
							Block block3 = block2.Block;
							if (!block2.isair && block3 != null && block3.shape.IsTerrain())
							{
								blockValue2 = block2;
								b = _chunk.GetDensity(num13, num17, num11);
							}
							else
							{
								blockValue2 = BlockValue.Air;
								b = MarchingCubes.DensityAir;
								if (num17 > 0 && _chunk.GetBlock(num13, num17 - 1, num11).Block.shape.IsTerrain())
								{
									sbyte density = _chunk.GetDensity(num13, num17 - 1, num11);
									b = MarchingCubes.DensityAir + density;
								}
							}
						}
						if (!flag2)
						{
							if (blockValue2.Block.isMultiBlock && MultiBlockManager.Instance.POIMBTrackingEnabled)
							{
								this.ProcessMultiBlock(ref blockValue2, _chunk, new Vector3i(num14, n, num12), new Vector3i(num13, num17, num11), _questTags, _bForceOverwriteBlocks);
							}
							else if (BlockPlaceholderMap.Instance.IsReplaceableBlockType(blockValue2))
							{
								blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue2, gameRandom, _chunk, num13, num17, num11, _questTags, _bForceOverwriteBlocks, true);
							}
						}
						Block block4 = blockValue2.Block;
						bool flag5 = block4.shape.IsTerrain();
						if (flag5)
						{
							num16 = num17;
						}
						if (b == 0)
						{
							b = MarchingCubes.DensityAir;
							if (flag5)
							{
								b = MarchingCubes.DensityTerrain;
							}
							else if (block4.shape.IsSolidCube && num17 <= num15)
							{
								b = 1;
							}
						}
						if (this.yOffset == 0)
						{
							sbyte density2 = _chunk.GetDensity(num13, num17, num11);
							if ((b >= 0 && density2 >= 0 && (density2 != MarchingCubes.DensityAir / 2 || (block4.IsTerrainDecoration && !this.bCopyAirBlocks))) || (b < 0 && density2 < 0 && density2 != MarchingCubes.DensityTerrain / 2))
							{
								b = density2;
							}
						}
						_chunk.SetDecoAllowedSizeAt(num13, num11, EnumDecoAllowedSize.OnlySmall);
						Vector3i vector3i = new Vector3i(num14, n, num12);
						if (flag && !block4.shape.IsTerrain() && this.IsInsidePrefab(vector3i.x, vector3i.y, vector3i.z))
						{
							_chunk.AddInsideDevicePosition(num13, num17, num11, blockValue2);
						}
						WaterValue water = this.GetWater(num14, n, num12);
						if (this.bCopyAirBlocks || !blockValue2.isair || n < -this.yOffset || water.HasMass())
						{
							BlockValue blockNoDamage2 = _chunk.GetBlockNoDamage(num13, num17, num11);
							if (!_bForceOverwriteBlocks && !blockNoDamage2.Block.shape.IsTerrain() && !blockNoDamage2.isair && (blockNoDamage2.ischild || blockNoDamage2.type == blockValue2.type))
							{
								_chunk.SetDensity(num13, num17, num11, b);
							}
							else
							{
								if (!flag4)
								{
									TextureFullArray texture = this.GetTexture(num14, n, num12);
									_chunk.GetSetTextureFullArray(num13, num17, num11, texture);
								}
								_chunk.SetBlock(_world, num13, num17, num11, blockValue2, true, true, !_questTags.IsEmpty, true, -1);
								_chunk.SetWater(num13, num17, num11, water);
								_chunk.SetDensity(num13, num17, num11, b);
								TileEntity tileEntity;
								if (block4.IsTileEntitySavedInPrefab() && (tileEntity = this.GetTileEntity(vector3i)) != null)
								{
									TileEntity tileEntity2 = _chunk.GetTileEntity(new Vector3i(num13, num17, num11));
									if (tileEntity2 == null)
									{
										tileEntity2 = tileEntity.Clone();
										tileEntity2.localChunkPos = new Vector3i(num13, num17, num11);
										tileEntity2.SetChunk(_chunk);
										_chunk.AddTileEntity(tileEntity2);
									}
									tileEntity2.CopyFrom(tileEntity);
									tileEntity2.localChunkPos = new Vector3i(num13, num17, num11);
								}
								BlockTrigger blockTrigger = this.GetBlockTrigger(vector3i);
								if (blockTrigger != null)
								{
									BlockTrigger blockTrigger2 = _chunk.GetBlockTrigger(new Vector3i(num13, num17, num11));
									if (blockTrigger2 == null)
									{
										blockTrigger2 = blockTrigger.Clone();
										blockTrigger2.LocalChunkPos = new Vector3i(num13, num17, num11);
										blockTrigger2.Chunk = _chunk;
										_chunk.AddBlockTrigger(blockTrigger2);
									}
									blockTrigger2.CopyFrom(blockTrigger);
									blockTrigger2.LocalChunkPos = new Vector3i(num13, num17, num11);
								}
							}
						}
					}
				}
				if (num16 >= 0)
				{
					_chunk.SetTerrainHeight(num13, num11, (byte)num16);
				}
				_chunk.SetTopSoilBroken(num13, num11);
			}
		}
		this.CopyVolumesIntoWorld(_world, _chunk, _prefabTargetPos);
		this.ApplyDecoAllowed(_chunk, _prefabTargetPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyDecoAllowed(Chunk _chunk, Vector3i _prefabTargetPos)
	{
		foreach (int num in this.decoAllowedBlockIndices)
		{
			int num2;
			int num3;
			int num4;
			this.offsetToCoordRotated(num, out num2, out num3, out num4);
			BlockValue block = this.GetBlock(num2, num3, num4);
			DecoUtils.ApplyDecoAllowed(_chunk, _prefabTargetPos + new Vector3i(num2, num3, num4), block);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBlockStatistics(BlockValue _bv, Block _b)
	{
		if (!Block.BlocksLoaded || _b == null)
		{
			return;
		}
		this.statistics.cntWindows = this.statistics.cntWindows + ((_b.BlockTag == BlockTags.Window) ? 1 : 0);
		this.statistics.cntDoors = this.statistics.cntDoors + ((_b.BlockTag == BlockTags.Door) ? 1 : 0);
		this.statistics.cntBlockEntities = this.statistics.cntBlockEntities + ((_b.shape is BlockShapeModelEntity && !_bv.ischild && (!(_b is BlockModelTree) || _bv.meta == 0)) ? 1 : 0);
		this.statistics.cntSolid = this.statistics.cntSolid + ((!_bv.isair) ? 1 : 0);
	}

	public Prefab.BlockStatistics GetBlockStatistics()
	{
		return this.statistics;
	}

	public List<EntityCreationData> GetEntities()
	{
		return this.entities;
	}

	public void Mirror(EnumMirrorAlong _axis)
	{
		Prefab.Data data = this.CellsToArrays();
		Prefab.Data data2 = default(Prefab.Data);
		data2.Init(this.GetBlockCount());
		BlockValue air = BlockValue.Air;
		for (int i = 0; i < this.size.x; i++)
		{
			for (int j = 0; j < this.size.z; j++)
			{
				for (int k = 0; k < this.size.y; k++)
				{
					int num = this.CoordToOffset(this.localRotation, i, k, j);
					WaterValue waterValue = data.m_Water[num];
					air.rawData = data.m_Blocks[num];
					if (!air.ischild && (!air.isair || waterValue.HasMass()))
					{
						Block block = air.Block;
						BlockShape shape = block.shape;
						int num2 = (int)((byte)BlockShapeNew.MirrorStatic(_axis, (int)air.rotation, shape.SymmetryType));
						Vector3i vector3i = new Vector3i(i, k, j);
						Vector3i vector3i2 = GameUtils.Mirror(_axis, vector3i, this.size);
						if (block.isMultiBlock)
						{
							Vector3 vector;
							vector..ctor((block.multiBlockPos.dim.x % 2 == 0) ? (-0.5f) : 0f, (block.multiBlockPos.dim.y % 2 == 0) ? (-0.5f) : 0f, (block.multiBlockPos.dim.z % 2 == 0) ? (-0.5f) : 0f);
							Vector3 vector2 = BlockShapeNew.GetRotationStatic((int)air.rotation) * vector;
							Vector3 vector3 = GameUtils.Mirror(_axis, vector3i2.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f), this.size) + vector2;
							Vector3 vector4 = GameUtils.Mirror(_axis, vector3, this.size);
							Vector3 vector5 = BlockShapeNew.GetRotationStatic(num2) * vector;
							vector3i2 = World.worldToBlockPos(vector4 - vector5);
						}
						int num3 = this.CoordToOffset(this.localRotation, vector3i2.x, vector3i2.y, vector3i2.z);
						if (block.MirrorSibling != 0)
						{
							air.type = block.MirrorSibling;
						}
						air.rotation = (byte)num2;
						data2.m_Blocks[num3] = air.rawData;
						data2.m_Damage[num3] = data.m_Damage[num];
						data2.m_Density[num3] = data.m_Density[num];
						data2.m_Textures[num3] = this.mirrorTexture(_axis, shape, (int)air.rotation, num2, data.m_Textures[num]);
						data2.m_Water[num3] = waterValue;
					}
				}
			}
		}
		this.CellsFromArrays(ref data2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TextureFullArray mirrorTexture(EnumMirrorAlong _axis, BlockShape _shape, int _sourceRot, int _targetRot, TextureFullArray _tex)
	{
		TextureFullArray textureFullArray = new TextureFullArray(0L);
		for (int i = 0; i < 6; i++)
		{
			BlockFace blockFace = (BlockFace)i;
			BlockFace blockFace2;
			BlockFace blockFace3;
			_shape.MirrorFace(_axis, _sourceRot, _targetRot, blockFace, out blockFace2, out blockFace3);
			for (int j = 0; j < 1; j++)
			{
				long num = (_tex[j] >> (int)((BlockFace)8 * blockFace2)) & 255L;
				num <<= (int)((BlockFace)8 * blockFace3);
				ref TextureFullArray ptr = ref textureFullArray;
				int num2 = j;
				ptr[num2] |= num;
			}
		}
		return textureFullArray;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayListMP<int> loadIdMapping(string _directory, string _prefabFileName, bool _allowMissingBlocks)
	{
		if (!Block.BlocksLoaded)
		{
			Log.Error("Block data not loaded");
			return null;
		}
		string text = _directory + "/" + _prefabFileName + ".blocks.nim";
		if (!SdFile.Exists(text))
		{
			Log.Error("Loading prefab \"" + _prefabFileName + "\" failed: Block name to ID mapping file missing.");
			return null;
		}
		ArrayListMP<int> arrayListMP;
		using (NameIdMapping nameIdMapping = MemoryPools.poolNameIdMapping.AllocSync(true))
		{
			nameIdMapping.InitMapping(text, Block.MAX_BLOCKS);
			if (!nameIdMapping.LoadFromFile())
			{
				arrayListMP = null;
			}
			else
			{
				Block missingBlock = null;
				if (_allowMissingBlocks)
				{
					missingBlock = Block.GetBlockByName(Prefab.MISSING_BLOCK_NAME, false);
					this.blockTypeMissingBlock = ((missingBlock != null) ? missingBlock.blockID : (-1));
				}
				arrayListMP = nameIdMapping.createIdTranslationTable(delegate(string _blockName)
				{
					Block blockByName = Block.GetBlockByName(_blockName, false);
					if (blockByName == null)
					{
						return -1;
					}
					return blockByName.blockID;
				}, delegate(string _name, int _id)
				{
					if (!_allowMissingBlocks)
					{
						Log.Error(string.Format("Loading prefab \"{0}\" failed: Block \"{1}\" ({2}) used in prefab is unknown.", _prefabFileName, _name, _id));
						return -1;
					}
					if (missingBlock == null)
					{
						Log.Error(string.Format("Loading prefab \"{0}\" failed: Block \"{1}\" ({2}) used in prefab is unknown and the replacement block \"{3}\" was not found.", new object[]
						{
							_prefabFileName,
							_name,
							_id,
							Prefab.MISSING_BLOCK_NAME
						}));
						return -1;
					}
					Log.Warning(string.Format("Loading prefab \"{0}\": Block \"{1}\" ({2}) used in prefab is unknown and getting replaced by \"{3}\".", new object[]
					{
						_prefabFileName,
						_name,
						_id,
						Prefab.MISSING_BLOCK_NAME
					}));
					return missingBlock.blockID;
				});
			}
		}
		return arrayListMP;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool doRaycast(Ray _ray, out RaycastHit _hitInfo, Vector3i _min)
	{
		bool flag = Physics.Raycast(_ray, ref _hitInfo, 255f, 1073807360);
		if (!flag)
		{
			return false;
		}
		Vector3 vector = _hitInfo.point + _ray.direction * 0.01f;
		Vector3i vector3i = new Vector3i(Utils.Fastfloor(vector.x), Utils.Fastfloor(vector.y), Utils.Fastfloor(vector.z));
		Block block = this.GetBlock(vector3i.x - _min.x, vector3i.y - _min.y, vector3i.z - _min.z).Block;
		if (block.bImposterDontBlock || block.bImposterExclude)
		{
			_ray.origin = _hitInfo.point + _ray.direction * 0.01f;
			flag = Physics.Raycast(_ray, ref _hitInfo, 255f, 1073807360);
		}
		return flag;
	}

	public EnumInsideOutside[] UpdateInsideOutside(Vector3i _min, Vector3i _max)
	{
		EnumInsideOutside[] array = new EnumInsideOutside[this.GetBlockCount()];
		BlockValue air = BlockValue.Air;
		uint[] blocks = this.CellsToArrays().m_Blocks;
		Ray ray;
		for (int i = _min.x; i <= _max.x; i++)
		{
			for (int j = _min.z; j <= _max.z; j++)
			{
				int num = _max.y;
				ray..ctor(Vector3.zero, Vector3.down);
				bool flag = false;
				float num2 = 0f;
				while (!flag && num2 <= 1f)
				{
					float num3 = 0f;
					while (!flag && num3 <= 1f)
					{
						ray.origin = new Vector3((float)i + num2, (float)(_max.y + 3), (float)j + num3);
						RaycastHit raycastHit;
						if (this.doRaycast(ray, out raycastHit, _min))
						{
							num = Utils.FastMin(num, Utils.Fastfloor(raycastHit.point.y + ray.direction.y * 0.1f));
						}
						else
						{
							num = _min.y;
							flag = true;
						}
						num3 += 0.25f;
					}
					num2 += 0.25f;
				}
				int k = i - _min.x + (num - _min.y) * this.size.x + (j - _min.z) * this.size.x * this.size.y;
				if (k >= 0 && k < array.Length)
				{
					while (k > 0)
					{
						air.rawData = blocks[k];
						if (!air.isair)
						{
							break;
						}
						k -= this.size.x;
						num--;
					}
					if (k > 0)
					{
						air.rawData = blocks[k];
						if (air.ischild)
						{
							int type = air.type;
							while (k > 0)
							{
								air.rawData = blocks[k];
								if (air.type != type)
								{
									break;
								}
								k -= this.size.x;
								num--;
							}
						}
					}
				}
				for (int l = _max.y; l >= num; l--)
				{
					k = i - _min.x + (l - _min.y) * this.size.x + (j - _min.z) * this.size.x * this.size.y;
					if (k >= 0 && k < array.Length)
					{
						array[k] = EnumInsideOutside.Outside;
					}
				}
			}
		}
		for (int m = _min.z; m <= _max.z; m++)
		{
			for (int n = _min.y; n <= _max.y; n++)
			{
				int num4 = _min.x;
				ray..ctor(Vector3.zero, Vector3.right);
				bool flag2 = false;
				float num5 = 0f;
				while (!flag2 && num5 <= 1f)
				{
					float num6 = 0f;
					while (!flag2 && num6 <= 1f)
					{
						ray.origin = new Vector3((float)(_min.x - 3), (float)n + num5, (float)m + num6);
						RaycastHit raycastHit;
						if (this.doRaycast(ray, out raycastHit, _min))
						{
							num4 = Utils.FastMax(num4, Utils.Fastfloor(raycastHit.point.x + ray.direction.x * 0.1f));
						}
						else
						{
							num4 = _max.x;
							flag2 = true;
						}
						num6 += 0.25f;
					}
					num5 += 0.25f;
				}
				int num7 = num4 - _min.x + (n - _min.y) * this.size.x + (m - _min.z) * this.size.x * this.size.y;
				if (num7 >= 0 && num7 < array.Length)
				{
					while (num7 < blocks.Length - 1)
					{
						air.rawData = blocks[num7];
						if (!air.isair)
						{
							break;
						}
						num7++;
						num4++;
					}
					if (num7 < array.Length)
					{
						air.rawData = blocks[num7];
						if (air.ischild)
						{
							int type2 = air.type;
							while (num7 > 0)
							{
								air.rawData = blocks[num7];
								if (air.type != type2)
								{
									break;
								}
								num7++;
								num4++;
							}
						}
					}
				}
				for (int num8 = _min.x; num8 <= num4; num8++)
				{
					num7 = num8 - _min.x + (n - _min.y) * this.size.x + (m - _min.z) * this.size.x * this.size.y;
					if (num7 >= 0 && num7 < array.Length)
					{
						array[num7] = EnumInsideOutside.Outside;
					}
				}
				int num9 = _max.x;
				ray..ctor(Vector3.zero, Vector3.left);
				flag2 = false;
				float num10 = 0f;
				while (!flag2 && num10 <= 1f)
				{
					float num11 = 0f;
					while (!flag2 && num11 <= 1f)
					{
						ray.origin = new Vector3((float)(_max.x + 3), (float)n + num10, (float)m + num11);
						RaycastHit raycastHit;
						if (this.doRaycast(ray, out raycastHit, _min))
						{
							num9 = Utils.FastMin(num9, Utils.Fastfloor(raycastHit.point.x + ray.direction.x * 0.1f));
						}
						else
						{
							num9 = _min.x;
							flag2 = true;
						}
						num11 += 0.25f;
					}
					num10 += 0.25f;
				}
				num7 = num9 - _min.x + (n - _min.y) * this.size.x + (m - _min.z) * this.size.x * this.size.y;
				if (num7 >= 0 && num7 < array.Length)
				{
					while (num7 > 0)
					{
						air.rawData = blocks[num7];
						if (!air.isair)
						{
							break;
						}
						num7--;
						num9--;
					}
					if (num7 > 0)
					{
						air.rawData = blocks[num7];
						if (air.ischild)
						{
							int type3 = air.type;
							while (num7 > 0)
							{
								air.rawData = blocks[num7];
								if (air.type != type3)
								{
									break;
								}
								num7--;
								num9--;
							}
						}
					}
				}
				for (int num12 = _max.x; num12 >= num9; num12--)
				{
					num7 = num12 - _min.x + (n - _min.y) * this.size.x + (m - _min.z) * this.size.x * this.size.y;
					if (num7 >= 0 && num7 < array.Length)
					{
						array[num7] = EnumInsideOutside.Outside;
					}
				}
			}
		}
		for (int num13 = _min.x; num13 <= _max.x; num13++)
		{
			for (int num14 = _min.y; num14 <= _max.y; num14++)
			{
				int num15 = _min.z;
				ray..ctor(Vector3.zero, Vector3.forward);
				bool flag3 = false;
				float num16 = 0f;
				while (!flag3 && num16 <= 1f)
				{
					float num17 = 0f;
					while (!flag3 && num17 <= 1f)
					{
						ray.origin = new Vector3((float)num13 + num16, (float)num14 + num17, (float)(_min.z - 3));
						RaycastHit raycastHit;
						if (this.doRaycast(ray, out raycastHit, _min))
						{
							num15 = Utils.FastMax(num15, Utils.Fastfloor(raycastHit.point.z + ray.direction.z * 0.1f));
						}
						else
						{
							num15 = _max.z;
							flag3 = true;
						}
						num17 += 0.25f;
					}
					num16 += 0.25f;
				}
				int num18 = num13 - _min.x + (num14 - _min.y) * this.size.x + (num15 - _min.z) * this.size.x * this.size.y;
				if (num18 >= 0 && num18 < array.Length)
				{
					while (num18 < blocks.Length - 1)
					{
						air.rawData = blocks[num18];
						if (!air.isair)
						{
							break;
						}
						num18 += this.size.x * this.size.y;
						num15++;
					}
					if (num18 < array.Length)
					{
						air.rawData = blocks[num18];
						if (air.ischild)
						{
							int type4 = air.type;
							while (num18 > 0)
							{
								air.rawData = blocks[num18];
								if (air.type != type4)
								{
									break;
								}
								num18 += this.size.x * this.size.y;
								num15++;
							}
						}
					}
				}
				Debug.DrawLine(ray.origin, new Vector3(ray.origin.x, ray.origin.y, (float)num15), Color.blue, 10f);
				for (int num19 = _min.z; num19 <= num15; num19++)
				{
					num18 = num13 - _min.x + (num14 - _min.y) * this.size.x + (num19 - _min.z) * this.size.x * this.size.y;
					if (num18 >= 0 && num18 < array.Length)
					{
						array[num18] = EnumInsideOutside.Outside;
					}
				}
				int num20 = _max.z;
				ray..ctor(Vector3.zero, Vector3.back);
				flag3 = false;
				float num21 = 0f;
				while (!flag3 && num21 <= 1f)
				{
					float num22 = 0f;
					while (!flag3 && num22 <= 1f)
					{
						ray.origin = new Vector3((float)num13 + num21, (float)num14 + num22, (float)(_max.z + 3));
						RaycastHit raycastHit;
						if (this.doRaycast(ray, out raycastHit, _min))
						{
							num20 = Utils.FastMin(num20, Utils.Fastfloor(raycastHit.point.z + ray.direction.z * 0.1f));
						}
						else
						{
							num20 = _min.z;
							flag3 = true;
						}
						num22 += 0.25f;
					}
					num21 += 0.25f;
				}
				num18 = num13 - _min.x + (num14 - _min.y) * this.size.x + (num20 - _min.z) * this.size.x * this.size.y;
				if (num18 >= 0 && num18 < array.Length)
				{
					while (num18 > 0)
					{
						air.rawData = blocks[num18];
						if (!air.isair)
						{
							break;
						}
						num18 -= this.size.x * this.size.y;
						num15--;
					}
					if (num18 > 0)
					{
						air.rawData = blocks[num18];
						if (air.ischild)
						{
							int type5 = air.type;
							while (num18 > 0)
							{
								air.rawData = blocks[num18];
								if (air.type != type5)
								{
									break;
								}
								num18 -= this.size.x * this.size.y;
								num15--;
							}
						}
					}
				}
				for (int num23 = _max.z; num23 >= num20; num23--)
				{
					num18 = num13 - _min.x + (num14 - _min.y) * this.size.x + (num23 - _min.z) * this.size.x * this.size.y;
					if (num18 >= 0 && num18 < array.Length)
					{
						array[num18] = EnumInsideOutside.Outside;
					}
				}
			}
		}
		return array;
	}

	public void RecalcInsideDevices(EnumInsideOutside[] _eInsideOutside)
	{
		this.insidePos.Init(this.size);
		if (!this.IsCullThisPrefab())
		{
			return;
		}
		int blockCount = this.GetBlockCount();
		for (int i = 0; i < blockCount; i++)
		{
			int num;
			int num2;
			int num3;
			this.offsetToCoord(i, out num, out num2, out num3);
			if (!this.GetBlock(num, num2, num3).Block.shape.IsTerrain() && _eInsideOutside[i] == EnumInsideOutside.Inside)
			{
				this.insidePos.Add(i);
			}
		}
	}

	public Vector3i? GetFirstIndexedBlockOffsetOfType(string _indexName)
	{
		List<Vector3i> list;
		if (this.indexedBlockOffsets.TryGetValue(_indexName, out list) && list.Count > 0)
		{
			return new Vector3i?(list[0]);
		}
		return null;
	}

	public IChunk GetChunkSync(int _chunkX, int _chunkZ)
	{
		return this.GetChunk(_chunkX, _chunkZ);
	}

	public IChunk GetChunkSync(long _chunkKey)
	{
		return this.GetChunkSync(WorldChunkCache.extractX(_chunkKey), WorldChunkCache.extractZ(_chunkKey));
	}

	public IChunk GetChunkSync(int chunkX, int chunkY, int chunkZ)
	{
		return IChunkAccess.DefaultGetChunkSync(this, chunkX, chunkY, chunkZ);
	}

	public IChunk GetChunkSync(Vector2i chunkPos)
	{
		return IChunkAccess.DefaultGetChunkSync(this, chunkPos);
	}

	public IChunk GetChunkSync(PropRef propRef)
	{
		return IChunkAccess.DefaultGetChunkSync(this, propRef);
	}

	public IChunk GetChunkSync(BlockValueRef bvRef)
	{
		return IChunkAccess.DefaultGetChunkSync(this, bvRef);
	}

	public IChunk GetChunkFromWorldPos(int x, int z)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, x, z);
	}

	public IChunk GetChunkFromWorldPos(int x, int y, int z)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, x, y, z);
	}

	public IChunk GetChunkFromWorldPos(Vector3i blockPos)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, blockPos);
	}

	public bool GetChunkFromWorldPos(int x, int z, ref IChunk chunk)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, x, z, ref chunk);
	}

	public bool GetChunkFromWorldPos(Vector3i blockPos, ref IChunk chunk)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, blockPos, ref chunk);
	}

	public IEnumerator ToTransform(bool _genBlockModels, bool _genTerrain, bool _genBlockShapes, bool _fillEmptySpace, Transform _parent, string _name, Vector3 _position, int _heightLimit = 0)
	{
		MicroStopwatch ms = new MicroStopwatch(true);
		GameObject _go = new GameObject();
		_go.name = _name;
		_go.transform.SetParent(_parent);
		int ySize = 8;
		if (_heightLimit == 0)
		{
			_heightLimit = this.size.y;
		}
		else if (_heightLimit < 0)
		{
			_heightLimit = -this.yOffset - _heightLimit;
		}
		_heightLimit = Mathf.Clamp(_heightLimit, 0, this.size.y);
		int y = 0;
		int y2 = 0;
		while (y < _heightLimit + 1)
		{
			int x = 0;
			int x2 = 0;
			int num4;
			while (x < this.size.x + 1)
			{
				int z = 0;
				int z2 = 0;
				while (z < this.size.z + 1 && !(_go == null))
				{
					GameObject gameObject = new GameObject();
					gameObject.transform.SetParent(_go.transform, false);
					gameObject.name = string.Format("Chunk[{0},{1}]", x2, z2);
					MeshFilter[] array = new MeshFilter[MeshDescription.meshes.Length];
					MeshRenderer[] array2 = new MeshRenderer[MeshDescription.meshes.Length];
					MeshCollider[] array3 = new MeshCollider[MeshDescription.meshes.Length];
					GameObject[] array4 = new GameObject[MeshDescription.meshes.Length];
					GameObject gameObject2 = new GameObject("_BlockEntities");
					GameObject gameObject3 = new GameObject("Meshes");
					gameObject2.transform.SetParent(gameObject.transform, false);
					gameObject3.transform.SetParent(gameObject.transform, false);
					for (int i = 0; i < MeshDescription.meshes.Length; i++)
					{
						array4[i] = new GameObject(MeshDescription.meshes[i].Name);
						array4[i].transform.parent = gameObject3.transform;
						VoxelMesh.CreateMeshFilter(i, 0, array4[i], MeshDescription.meshes[i].Tag, false, out array[i], out array2[i], out array3[i]);
					}
					VoxelMesh[] array5 = new VoxelMesh[6];
					for (int j = 0; j < array5.Length; j++)
					{
						if (j == 5)
						{
							array5[j] = new VoxelMeshTerrain(j, 500)
							{
								IsPreviewVoxelMesh = true
							};
						}
						else
						{
							array5[j] = new VoxelMesh(j, 1024, VoxelMesh.CreateFlags.Default);
						}
					}
					MeshGeneratorPrefab meshGeneratorPrefab = new MeshGeneratorPrefab(this);
					Vector3i vector3i = new Vector3i(x, y, z);
					Vector3i vector3i2 = new Vector3i(x + 15, y + ySize, z + 16);
					if (_genTerrain && _genBlockShapes)
					{
						meshGeneratorPrefab.GenerateMeshOffset(vector3i, vector3i2, array5);
					}
					else if (!_genTerrain && _genBlockShapes)
					{
						meshGeneratorPrefab.GenerateMeshNoTerrain(vector3i, vector3i2, array5);
					}
					else if (_genTerrain && !_genBlockShapes)
					{
						meshGeneratorPrefab.GenerateMeshTerrainOnly(vector3i, vector3i2, array5);
					}
					for (int k = 0; k < array5.Length; k++)
					{
						array5[k].CopyToMesh(array[k], array2[k], 0, null);
					}
					if (_genBlockModels)
					{
						int num = y;
						while (num < y + ySize && num < this.size.y)
						{
							int num2 = x;
							while (num2 < x + 16 && num2 < this.size.x)
							{
								int num3 = z;
								while (num3 < z + 16 && num3 < this.size.z)
								{
									Vector3i vector3i3 = new Vector3i(num2, num, num3);
									BlockValue block = this.GetBlock(num2, num, num3);
									if (!block.ischild)
									{
										Block block2 = block.Block;
										BlockShapeModelEntity blockShapeModelEntity = block2.shape as BlockShapeModelEntity;
										if (blockShapeModelEntity != null)
										{
											Quaternion rotation = blockShapeModelEntity.GetRotation(block);
											Vector3 rotatedOffset = blockShapeModelEntity.GetRotatedOffset(block2, rotation);
											rotatedOffset.x += 0.5f;
											rotatedOffset.z += 0.5f;
											rotatedOffset.y += 0f;
											Vector3 vector = vector3i3.ToVector3() + rotatedOffset;
											GameObject objectForType = GameObjectPool.Instance.GetObjectForType(blockShapeModelEntity.modelName);
											if (!(objectForType == null))
											{
												Transform transform = objectForType.transform;
												transform.parent = gameObject2.transform;
												transform.localScale = Vector3.one;
												transform.localPosition = vector;
												transform.localRotation = rotation;
											}
										}
									}
									num3++;
								}
								num2++;
							}
							num++;
						}
					}
					yield return null;
					z += 16;
					num4 = z2;
					z2 = num4 + 1;
				}
				x += 16;
				num4 = x2;
				x2 = num4 + 1;
			}
			y += ySize;
			num4 = y2;
			y2 = num4 + 1;
		}
		if (_go != null)
		{
			_go.transform.localPosition = new Vector3(_position.x * _go.transform.localScale.x, _position.y * _go.transform.localScale.y, _position.z * _go.transform.localScale.z);
		}
		Log.Out(string.Format("Prefab preview generation took {0} seconds.", (float)ms.ElapsedMilliseconds / 1000f));
		yield break;
	}

	public void HandleAddingTriggerLayers(BlockTrigger _trigger)
	{
		for (int i = 0; i < _trigger.TriggersIndices.Count; i++)
		{
			if (!this.TriggerLayers.Contains(_trigger.TriggersIndices[i]))
			{
				this.TriggerLayers.Add(_trigger.TriggersIndices[i]);
			}
		}
		for (int j = 0; j < _trigger.TriggeredByIndices.Count; j++)
		{
			if (!this.TriggerLayers.Contains(_trigger.TriggeredByIndices[j]))
			{
				this.TriggerLayers.Add(_trigger.TriggeredByIndices[j]);
			}
		}
	}

	public void HandleAddingTriggerLayers(PrefabTriggerVolume _trigger)
	{
		for (int i = 0; i < _trigger.TriggersIndices.Count; i++)
		{
			if (!this.TriggerLayers.Contains(_trigger.TriggersIndices[i]))
			{
				this.TriggerLayers.Add(_trigger.TriggersIndices[i]);
			}
		}
	}

	public void AddInitialTriggerLayers()
	{
		for (byte b = 1; b < 6; b += 1)
		{
			this.TriggerLayers.Add(b);
		}
	}

	public void AddNewTriggerLayer()
	{
		if (this.TriggerLayers.Count > 0)
		{
			this.TriggerLayers.Sort();
			int num = (int)(this.TriggerLayers[this.TriggerLayers.Count - 1] + 1);
			if (num < 255 && num > 0)
			{
				this.TriggerLayers.Add((byte)num);
				return;
			}
		}
		else
		{
			this.TriggerLayers.Add(1);
		}
	}

	[Conditional("DEBUG_PREFABLOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogPrefab(string _format, params object[] _args)
	{
		Log.Warning(string.Format("{0} Prefab {1}", GameManager.frameCount, _format), _args);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int CurrentSaveVersion = 19;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMinimumSupportedVersion = 13;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_CopyAirBlocks = "CopyAirBlocks";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_AllowTopSoilDecorations = "AllowTopSoilDecorations";

	public const string cProp_YOffset = "YOffset";

	public const string cProp_RotationToFaceNorth = "RotationToFaceNorth";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_ExcludeDistantPOIMesh = "ExcludeDistantPOIMesh";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_ExcludePOICulling = "ExcludePOICulling";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_DistantPOIYOffset = "DistantPOIYOffset";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_DistantPOIOverride = "DistantPOIOverride";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_EditorGroups = "EditorGroups";

	public const string cProp_IsTraderArea = "TraderArea";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_TraderAreaProtect = "TraderAreaProtect";

	public const string cProp_SleeperVolumeStart = "SleeperVolumeStart";

	public const string cProp_SleeperVolumeSize = "SleeperVolumeSize";

	public const string cProp_SleeperVolumeGroup = "SleeperVolumeGroup";

	public const string cProp_SleeperVolumeGroupId = "SleeperVolumeGroupId";

	public const string cProp_SleeperIsPriorityVolume = "SleeperIsLootVolume";

	public const string cProp_SleeperIsQuestExclude = "SleeperIsQuestExclude";

	public const string cProp_SleeperVolumeFlags = "SleeperVolumeFlags";

	public const string cProp_SleeperVolumeTriggeredBy = "SleeperVolumeTriggeredBy";

	public const string cProp_SleeperVolumeScript = "SVS";

	public const string cProp_TeleportVolumeStart = "TeleportVolumeStart";

	public const string cProp_TeleportVolumeSize = "TeleportVolumeSize";

	public const string cProp_InfoVolumeStart = "InfoVolumeStart";

	public const string cProp_InfoVolumeSize = "InfoVolumeSize";

	public const string cProp_WallVolumeStart = "WallVolumeStart";

	public const string cProp_WallVolumeSize = "WallVolumeSize";

	public const string cProp_TriggerVolumeStart = "TriggerVolumeStart";

	public const string cProp_TriggerVolumeSize = "TriggerVolumeSize";

	public const string cProp_TriggerVolumeTriggers = "TriggerVolumeTriggers";

	public const string cProp_POIMarkerStart = "POIMarkerStart";

	public const string cProp_POIMarkerSize = "POIMarkerSize";

	public const string cProp_POIMarkerGroup = "POIMarkerGroup";

	public const string cProp_POIMarkerTags = "POIMarkerTags";

	public const string cProp_POIMarkerType = "POIMarkerType";

	public const string cProp_POIMarkerPartToSpawn = "POIMarkerPartToSpawn";

	public const string cProp_POIMarkerPartRotations = "POIMarkerPartRotations";

	public const string cProp_POIMarkerPartSpawnChance = "POIMarkerPartSpawnChance";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_Zoning = "Zoning";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_AllowedBiomes = "AllowedBiomes";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_AllowedTownships = "AllowedTownships";

	public const string cProp_Tags = "Tags";

	public const string cProp_ThemeTags = "ThemeTags";

	public const string cProp_ThemeRepeatDist = "ThemeRepeatDistance";

	public const string cProp_DuplicateRepeatDist = "DuplicateRepeatDistance";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_IndexedBlockOffsets = "IndexedBlockOffsets";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_QuestTags = "QuestTags";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_ShowQuestClearCount = "ShowQuestClearCount";

	public const string cProp_DifficultyTier = "DifficultyTier";

	public const string cProp_PrefabSize = "PrefabSize";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string MISSING_BLOCK_NAME = "missingBlock";

	public Vector3i size;

	public PathAbstractions.AbstractedLocation location;

	public bool bCopyAirBlocks = true;

	public bool bExcludeDistantPOIMesh;

	public bool bExcludePOICulling;

	public float distantPOIYOffset;

	public string distantPOIOverride;

	public bool bAllowTopSoilDecorations;

	public bool bTraderArea;

	public Vector3i TraderAreaProtect;

	public readonly List<PrefabVolumeListAbs> AllVolumeLists;

	public readonly Dictionary<PrefabVolumeAbs.EVolumeType, PrefabVolumeListAbs> AllVolumeListsByType = new Dictionary<PrefabVolumeAbs.EVolumeType, PrefabVolumeListAbs>();

	public readonly PrefabSleeperVolumeList SleeperVolumeList;

	public readonly PrefabTeleportVolumeList TeleportVolumeList;

	public readonly PrefabInfoVolumeList InfoVolumeList;

	public readonly PrefabWallVolumeList WallVolumeList;

	public readonly PrefabTriggerVolumeList TriggerVolumeList;

	public readonly PrefabMarkerVolumeList MarkerVolumeList;

	public int yOffset;

	public int Transient_NumSleeperSpawns;

	public List<string> editorGroups = new List<string>();

	public int rotationToFaceNorth = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> allowedZones = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> allowedBiomes = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> allowedTownships = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> tags;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> themeTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public int themeRepeatDistance = 300;

	[PublicizedFrom(EAccessModifier.Private)]
	public int duplicateRepeatDistance = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> multiBlockParentIndices = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> decoAllowedBlockIndices = new List<int>();

	public readonly Dictionary<string, List<Vector3i>> indexedBlockOffsets = new CaseInsensitiveStringDictionary<List<Vector3i>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> questTags = FastTags<TagGroup.Global>.none;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.BlockStatistics statistics;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldStats renderingCost;

	public int ShowQuestClearCount = 1;

	public byte DifficultyTier;

	[PublicizedFrom(EAccessModifier.Private)]
	public int localRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool isCellsDataOwner = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.Cells<uint> blockCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.Cells<ushort> damageCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.Cells<sbyte> densityCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.Cells<TextureFullArray> textureCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.Cells<WaterValue> waterCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Prefab.Data sharedData = default(Prefab.Data);

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityCreationData> entities = new List<EntityCreationData>();

	public DynamicProperties properties = new DynamicProperties();

	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainFillerType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainFiller2Type;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blockTypeMissingBlock = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, TileEntity> tileEntities = new Dictionary<Vector3i, TileEntity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector2i, Dictionary<int, PropValue>> propsByChunk = new Dictionary<Vector2i, Dictionary<int, PropValue>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly PrefabInsideDataFile insidePos = new PrefabInsideDataFile();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, BlockTrigger> triggerData = new Dictionary<Vector3i, BlockTrigger>();

	public readonly List<byte> TriggerLayers = new List<byte>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] tempBuf;

	[PublicizedFrom(EAccessModifier.Private)]
	public static SimpleBitStream simpleBitStreamReader = new SimpleBitStream(1000);

	[PublicizedFrom(EAccessModifier.Private)]
	public int currX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, Prefab.PrefabChunk> dictChunks;

	public struct BlockStatistics
	{
		public void Clear()
		{
			this.cntWindows = 0;
			this.cntDoors = 0;
			this.cntBlockEntities = 0;
			this.cntSolid = 0;
			this.cntBlockModels = 0;
		}

		public override string ToString()
		{
			return string.Format("Blocks: {0} BEnts: {1} BMods: {2} Wdws: {3}", new object[] { this.cntSolid, this.cntBlockEntities, this.cntBlockModels, this.cntWindows });
		}

		public int cntWindows;

		public int cntDoors;

		public int cntBlockEntities;

		public int cntBlockModels;

		public int cntSolid;
	}

	public struct Data
	{
		public void Init(int _count)
		{
			this.m_Blocks = new uint[_count];
			this.m_Damage = new ushort[_count];
			this.m_Density = new byte[_count];
			this.m_Textures = new TextureFullArray[_count];
			this.m_Water = new WaterValue[_count];
		}

		public void Expand(int _count)
		{
			int num = ((this.m_Blocks != null) ? this.m_Blocks.Length : 0);
			if (_count > num)
			{
				this.m_Blocks = new uint[_count];
				this.m_Damage = new ushort[_count];
				this.m_Density = new byte[_count];
				this.m_Textures = new TextureFullArray[_count];
				this.m_Water = new WaterValue[_count];
			}
			for (int i = 0; i < _count; i++)
			{
				this.m_Textures[i].Fill(0L);
				this.m_Water[i] = WaterValue.Empty;
			}
		}

		public uint[] m_Blocks;

		public ushort[] m_Damage;

		public byte[] m_Density;

		public TextureFullArray[] m_Textures;

		public WaterValue[] m_Water;
	}

	public class Cells<[IsUnmanaged] T> where T : struct, ValueType
	{
		public Cells(int _sizeY, T _defaultValue)
		{
			this.sizeY = _sizeY;
			this.defaultValue = _defaultValue;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Cells(Prefab.Cells<T> _template)
		{
			this.sizeY = _template.sizeY;
			this.defaultValue = _template.defaultValue;
		}

		public Prefab.Cells<T>.Cell AllocCell(int _x, int _y, int _z)
		{
			if (this.a == null)
			{
				this.a = new Prefab.Cells<T>.CellsAtZ[this.sizeY];
			}
			Prefab.Cells<T>.CellsAtZ cellsAtZ = this.a[_y];
			if (cellsAtZ == null)
			{
				cellsAtZ = new Prefab.Cells<T>.CellsAtZ();
				this.a[_y] = cellsAtZ;
			}
			int num = _z >> 2;
			if (cellsAtZ.a == null || num >= cellsAtZ.a.Length)
			{
				Array.Resize<Prefab.Cells<T>.CellsAtX>(ref cellsAtZ.a, num + 1);
			}
			Prefab.Cells<T>.CellsAtX cellsAtX = cellsAtZ.a[num];
			if (cellsAtX == null)
			{
				cellsAtX = new Prefab.Cells<T>.CellsAtX();
				cellsAtZ.a[num] = cellsAtX;
			}
			int num2 = _x >> 2;
			if (cellsAtX.a == null || num2 >= cellsAtX.a.Length)
			{
				Array.Resize<Prefab.Cells<T>.Cell>(ref cellsAtX.a, num2 + 1);
			}
			Prefab.Cells<T>.Cell cell = cellsAtX.a[num2];
			if (cell.a == null)
			{
				cell = new Prefab.Cells<T>.Cell(this.defaultValue);
				cellsAtX.a[num2] = cell;
			}
			return cell;
		}

		public Prefab.Cells<T>.Cell GetCell(int _x, int _y, int _z)
		{
			if (this.a == null)
			{
				return Prefab.Cells<T>.Cell.empty;
			}
			Prefab.Cells<T>.CellsAtZ cellsAtZ = this.a[_y];
			if (cellsAtZ == null)
			{
				return Prefab.Cells<T>.Cell.empty;
			}
			int num = _z >> 2;
			if (cellsAtZ.a == null || num >= cellsAtZ.a.Length)
			{
				return Prefab.Cells<T>.Cell.empty;
			}
			Prefab.Cells<T>.CellsAtX cellsAtX = cellsAtZ.a[num];
			if (cellsAtX == null)
			{
				return Prefab.Cells<T>.Cell.empty;
			}
			int num2 = _x >> 2;
			if (cellsAtX.a == null || num2 >= cellsAtX.a.Length)
			{
				return Prefab.Cells<T>.Cell.empty;
			}
			return cellsAtX.a[num2];
		}

		public T GetData(int _x, int _y, int _z)
		{
			Prefab.Cells<T>.Cell cell = this.GetCell(_x, _y, _z);
			if (cell.a == null)
			{
				return this.defaultValue;
			}
			return cell.Get(_x, _z);
		}

		public void SetData(int _x, int _y, int _z, T _data)
		{
			this.AllocCell(_x, _y, _z).Set(_x, _z, _data);
		}

		public Prefab.Cells<T> Clone()
		{
			Prefab.Cells<T> cells = new Prefab.Cells<T>(this.sizeY, this.defaultValue);
			if (this.a == null)
			{
				return cells;
			}
			cells.a = new Prefab.Cells<T>.CellsAtZ[this.sizeY];
			for (int i = 0; i < this.sizeY; i++)
			{
				Prefab.Cells<T>.CellsAtZ cellsAtZ = this.a[i];
				if (cellsAtZ != null)
				{
					Prefab.Cells<T>.CellsAtZ cellsAtZ2 = new Prefab.Cells<T>.CellsAtZ();
					cellsAtZ2.a = new Prefab.Cells<T>.CellsAtX[cellsAtZ.a.Length];
					cells.a[i] = cellsAtZ2;
					for (int j = 0; j < cellsAtZ.a.Length; j++)
					{
						Prefab.Cells<T>.CellsAtX cellsAtX = cellsAtZ.a[j];
						if (cellsAtX != null)
						{
							Prefab.Cells<T>.CellsAtX cellsAtX2 = new Prefab.Cells<T>.CellsAtX();
							cellsAtX2.a = new Prefab.Cells<T>.Cell[cellsAtX.a.Length];
							cellsAtZ2.a[j] = cellsAtX2;
							for (int k = 0; k < cellsAtX.a.Length; k++)
							{
								Prefab.Cells<T>.Cell cell = cellsAtX.a[k];
								if (cell.a != null)
								{
									cellsAtX2.a[k] = cell.Clone();
								}
							}
						}
					}
				}
			}
			return cells;
		}

		public void Stats(out int _arrayCount, out int _arraySize, out int _cellsCount, out int _cellsSize, out int _usedCount)
		{
			_arrayCount = 0;
			_arraySize = 0;
			_cellsCount = 0;
			_cellsSize = 0;
			_usedCount = 0;
			int num = ((this.a != null) ? this.a.Length : 0);
			_arrayCount += num;
			_arraySize += num * 8 + 8;
			for (int i = 0; i < num; i++)
			{
				Prefab.Cells<T>.CellsAtZ cellsAtZ = this.a[i];
				if (cellsAtZ != null)
				{
					_arrayCount += cellsAtZ.a.Length;
					_arraySize += cellsAtZ.a.Length * 8 + 8;
					for (int j = 0; j < cellsAtZ.a.Length; j++)
					{
						Prefab.Cells<T>.CellsAtX cellsAtX = cellsAtZ.a[j];
						if (cellsAtX != null)
						{
							_arrayCount += cellsAtX.a.Length;
							_arraySize += cellsAtX.a.Length * 8 + 8;
							for (int k = 0; k < cellsAtX.a.Length; k++)
							{
								Prefab.Cells<T>.Cell cell = cellsAtX.a[k];
								if (cell.a != null)
								{
									_cellsCount += 16;
									_cellsSize += cell.Size();
									_usedCount += cell.UsedCount(this.defaultValue);
								}
							}
						}
					}
				}
			}
		}

		public void Load(PooledBinaryReader _br)
		{
			Array.Clear(this.a, 0, this.a.Length);
			if (_br.ReadByte() == 1 && _br.ReadUInt16() > 0)
			{
				this.LoadData(_br);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LoadData(PooledBinaryReader _br)
		{
			for (;;)
			{
				_br.Read(Prefab.Cells<T>.cellBytes, 0, 3);
				int num = (int)Prefab.Cells<T>.cellBytes[0];
				if (num == 255)
				{
					break;
				}
				int num2 = (int)Prefab.Cells<T>.cellBytes[1] << 2;
				int num3 = (int)Prefab.Cells<T>.cellBytes[2] << 2;
				this.AllocCell(num2, num, num3).Load(_br);
			}
		}

		public void Save(BinaryWriter _bw)
		{
			ushort num = (ushort)((this.a != null) ? this.a.Length : 0);
			if (num == 0)
			{
				_bw.Write(0);
				return;
			}
			_bw.Write(1);
			_bw.Write(num);
			for (int i = 0; i < (int)num; i++)
			{
				Prefab.Cells<T>.CellsAtZ cellsAtZ = this.a[i];
				if (cellsAtZ != null)
				{
					for (int j = 0; j < cellsAtZ.a.Length; j++)
					{
						Prefab.Cells<T>.CellsAtX cellsAtX = cellsAtZ.a[j];
						if (cellsAtX != null)
						{
							for (int k = 0; k < cellsAtX.a.Length; k++)
							{
								Prefab.Cells<T>.Cell cell = cellsAtX.a[k];
								if (cell.a != null)
								{
									Prefab.Cells<T>.cellBytes[0] = (byte)i;
									Prefab.Cells<T>.cellBytes[1] = (byte)k;
									Prefab.Cells<T>.cellBytes[2] = (byte)j;
									_bw.Write(Prefab.Cells<T>.cellBytes, 0, 3);
									cell.Save(_bw);
								}
							}
						}
					}
				}
			}
			Prefab.Cells<T>.cellBytes[0] = byte.MaxValue;
			_bw.Write(Prefab.Cells<T>.cellBytes, 0, 3);
		}

		public T[] ToArray(Prefab prefab, Vector3i _size)
		{
			T[] array = new T[_size.x * _size.y * _size.z];
			int num = ((this.a != null) ? this.a.Length : 0);
			for (int i = 0; i < num; i++)
			{
				Prefab.Cells<T>.CellsAtZ cellsAtZ = this.a[i];
				if (cellsAtZ != null)
				{
					for (int j = 0; j < cellsAtZ.a.Length; j++)
					{
						Prefab.Cells<T>.CellsAtX cellsAtX = cellsAtZ.a[j];
						if (cellsAtX != null)
						{
							for (int k = 0; k < cellsAtX.a.Length; k++)
							{
								Prefab.Cells<T>.Cell cell = cellsAtX.a[k];
								if (cell.a != null)
								{
									int num2 = k << 2;
									int num3 = j << 2;
									int num4 = Utils.FastMin(_size.x - num2, 4);
									int num5 = Utils.FastMin(_size.z - num3, 4);
									for (int l = 0; l < num5; l++)
									{
										for (int m = 0; m < num4; m++)
										{
											T t = cell.Get(m, l);
											int num6 = prefab.CoordToOffset(0, num2 + m, i, num3 + l);
											array[num6] = t;
										}
									}
								}
							}
						}
					}
				}
			}
			return array;
		}

		public unsafe void CompareTest(Vector3i size, PooledBinaryReader _br)
		{
			Prefab.Cells<T> cells = new Prefab.Cells<T>(this);
			cells.Load(_br);
			if (this.a.Length != cells.a.Length)
			{
				Log.Error("Cells size");
			}
			for (int i = 0; i < size.y; i++)
			{
				for (int j = 0; j < size.z; j++)
				{
					for (int k = 0; k < size.x; k++)
					{
						Prefab.Cells<T>.Cell cell = this.GetCell(k, i, j);
						Prefab.Cells<T>.Cell cell2 = cells.GetCell(k, i, j);
						if (cell.a == null)
						{
							if (cell2.a != null)
							{
								Log.Error("Cells one is null {0} {1} {2}", new object[] { k, i, j });
							}
						}
						else if (cell2.a != null)
						{
							for (int l = 0; l < 4; l++)
							{
								for (int m = 0; m < 4; m++)
								{
									T t = cell.Get(m, l);
									T t2 = cell2.Get(m, l);
									byte* ptr = (byte*)UnsafeUtility.AddressOf<T>(ref t);
									byte* ptr2 = (byte*)UnsafeUtility.AddressOf<T>(ref t2);
									for (int n = 0; n < sizeof(T); n++)
									{
										if (ptr[n] != ptr2[n])
										{
											Log.Error("Cells data {0} {1} {2}, {3} != {4}", new object[]
											{
												k,
												i,
												j,
												ptr[n],
												ptr2[n]
											});
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public T defaultValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public int sizeY;

		public Prefab.Cells<T>.CellsAtZ[] a;

		[PublicizedFrom(EAccessModifier.Private)]
		public static byte[] cellBytes = new byte[256];

		public class CellsAtX
		{
			public Prefab.Cells<T>.Cell[] a;
		}

		public class CellsAtZ
		{
			public Prefab.Cells<T>.CellsAtX[] a;
		}

		public struct Cell
		{
			public Cell(T _defaultValue)
			{
				this.a = new T[16];
				for (int i = 0; i < 16; i++)
				{
					this.a[i] = _defaultValue;
				}
			}

			public Prefab.Cells<T>.Cell Clone()
			{
				Prefab.Cells<T>.Cell cell = default(Prefab.Cells<T>.Cell);
				if (this.a != null)
				{
					cell.a = new T[16];
					for (int i = 0; i < 16; i++)
					{
						cell.a[i] = this.a[i];
					}
				}
				return cell;
			}

			public override string ToString()
			{
				return string.Format("{0}", (this.a != null) ? this.a.Length : (-1));
			}

			public int Size()
			{
				return 16 * sizeof(T);
			}

			public unsafe int UsedCount(T _defaultValue)
			{
				int num = 0;
				byte* ptr = (byte*)UnsafeUtility.AddressOf<T>(ref _defaultValue);
				for (int i = 0; i < 16; i++)
				{
					byte* ptr2 = (byte*)UnsafeUtility.AddressOf<T>(ref this.a[i]);
					for (int j = 0; j < sizeof(T); j++)
					{
						if (ptr[j] != ptr2[j])
						{
							num++;
							break;
						}
					}
				}
				return num;
			}

			public void Set(int _x, int _z, T _value)
			{
				int num = (_x & 3) + ((_z & 3) << 2);
				this.a[num] = _value;
			}

			public T Get(int _x, int _z)
			{
				int num = (_x & 3) + ((_z & 3) << 2);
				return this.a[num];
			}

			public unsafe void Load(PooledBinaryReader _br)
			{
				int num = (int)_br.BaseStream.Position;
				int num2 = (int)_br.ReadUInt16();
				_br.Read(Prefab.Cells<T>.cellBytes, 0, num2);
				Log.Warning("Cell Load at {0}, count{1}", new object[] { num, num2 });
				int num3 = 0;
				byte* ptr = (byte*)UnsafeUtility.AddressOf<T>(ref this.a[0]);
				int i = 0;
				while (i < num2)
				{
					int num4 = (int)((sbyte)Prefab.Cells<T>.cellBytes[i++]);
					if (num4 >= 0)
					{
						for (int j = 0; j < num4; j++)
						{
							byte b = Prefab.Cells<T>.cellBytes[i++];
							ptr[num3++] = b;
						}
					}
					else
					{
						num4 = -num4;
						byte b2 = Prefab.Cells<T>.cellBytes[i++];
						for (int k = 0; k < num4; k++)
						{
							ptr[num3++] = b2;
						}
					}
				}
			}

			public unsafe void Save(BinaryWriter _bw)
			{
				byte* ptr = (byte*)UnsafeUtility.AddressOf<T>(ref this.a[0]);
				int num = 0;
				int num2 = this.a.Length * sizeof(T);
				int i = 0;
				while (i < num2)
				{
					int num3 = 1;
					byte b = ptr[i];
					if (i + 1 < num2)
					{
						byte b2 = ptr[i + 1];
						if (b == b2)
						{
							int num4 = 2;
							int num5 = i + 2;
							while (num5 < num2 && ptr[num5] == b)
							{
								num4++;
								if (num4 >= 128)
								{
									break;
								}
								num5++;
							}
							if (num4 >= 3)
							{
								num3 = -num4;
								i += num4;
							}
							else
							{
								num3 = num4;
							}
						}
						if (num3 >= 0)
						{
							for (int j = i + num3; j < num2; j++)
							{
								b2 = ptr[j];
								if (j + 2 < num2 && b2 == ptr[j + 1] && b2 == ptr[j + 2])
								{
									break;
								}
								num3++;
								if (num3 >= 127)
								{
									break;
								}
							}
						}
					}
					Prefab.Cells<T>.cellBytes[num++] = (byte)num3;
					if (num3 >= 0)
					{
						for (int k = 0; k < num3; k++)
						{
							byte b3 = ptr[i++];
							Prefab.Cells<T>.cellBytes[num++] = b3;
						}
					}
					else
					{
						Prefab.Cells<T>.cellBytes[num++] = b;
					}
				}
				_bw.Write((ushort)num);
				_bw.Write(Prefab.Cells<T>.cellBytes, 0, num);
			}

			public const int cSizeXZ = 4;

			public const int cSizeArray = 16;

			public const int cSizeXZMask = 3;

			public const int cSizeXZShift = 2;

			public static Prefab.Cells<T>.Cell empty;

			public T[] a;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public enum DataFormat
		{
			Empty,
			RLE
		}
	}

	public class PrefabChunk : IChunk, IBlockAccess
	{
		public PrefabChunk(Prefab _prefab, int _x, int _z)
		{
			this.prefab = _prefab;
			this.X = _x;
			this.Z = _z;
			this.Y = 0;
		}

		public int X { get; set; }

		public int Y { get; set; }

		public int Z { get; set; }

		public Vector3i ChunkPos { get; set; }

		public bool GetAvailable()
		{
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool checkCoordinates(ref int _x, ref int _y, ref int _z)
		{
			_x = this.X * 16 + _x;
			_y = this.Y * 256 + _y;
			_z = this.Z * 16 + _z;
			return _x >= 0 && _x < this.prefab.size.x && _y >= 0 && _y < this.prefab.size.y && _z >= 0 && _z < this.prefab.size.z;
		}

		public BlockValue GetBlock(int _x, int _y, int _z)
		{
			if (!this.checkCoordinates(ref _x, ref _y, ref _z))
			{
				return BlockValue.Air;
			}
			return this.prefab.GetBlock(_x, _y, _z);
		}

		public BlockValue GetBlock(Vector3i pos)
		{
			return IBlockAccess.DefaultGetBlock(this, pos);
		}

		public BlockValue GetBlock(BlockValueRef bvRef)
		{
			return IBlockAccess.DefaultGetBlock(this, bvRef);
		}

		public BlockValue GetBlockNoDamage(int _x, int _y, int _z)
		{
			return this.GetBlock(_x, _y, _z);
		}

		public void GetBlockColumn(int _x, int _y, int _z, BlockValue[] _blocks)
		{
			int num = _blocks.Length;
			for (int i = 0; i < num; i++)
			{
				_blocks[i] = this.GetBlock(_x, _y + i, _z);
			}
		}

		public BlockValue GetBlock(int _bos, int _y)
		{
			return this.GetBlock(ChunkBlockLayerLegacy.OffsetX(_bos), _y, ChunkBlockLayerLegacy.OffsetX(_bos));
		}

		public bool IsAir(int _x, int _y, int _z)
		{
			return this.checkCoordinates(ref _x, ref _y, ref _z) && this.prefab.GetBlock(_x, _y, _z).isair && !this.prefab.GetWater(_x, _y, _z).HasMass();
		}

		public WaterValue GetWater(int _x, int _y, int _z)
		{
			if (!this.checkCoordinates(ref _x, ref _y, ref _z))
			{
				return WaterValue.Empty;
			}
			return this.prefab.GetWater(_x, _y, _z);
		}

		public bool IsWater(int _x, int _y, int _z)
		{
			return this.GetWater(_x, _y, _z).HasMass();
		}

		public int GetBlockFaceTexture(int _x, int _y, int _z, BlockFace _blockFace, int _channel)
		{
			if (!this.checkCoordinates(ref _x, ref _y, ref _z))
			{
				return 0;
			}
			return (int)((this.prefab.GetTexture(_x, _y, _z)[_channel] >> (int)(_blockFace * BlockFace.Middle)) & 63L);
		}

		public long GetTextureFull(int _x, int _y, int _z, int _channel = 0)
		{
			if (!this.checkCoordinates(ref _x, ref _y, ref _z))
			{
				return 0L;
			}
			return this.prefab.GetTexture(_x, _y, _z)[_channel];
		}

		public PropValue GetProp(int chunkX, int chunkZ, int propId)
		{
			return this.prefab.GetProp(chunkX, chunkZ, propId);
		}

		public PropValue GetProp(long chunkKey, int propId)
		{
			return this.prefab.GetProp(chunkKey, propId);
		}

		public PropValue GetProp(Vector2i chunkPos, int propId)
		{
			return IBlockAccess.DefaultGetProp(this, chunkPos, propId);
		}

		public PropValue GetProp(PropRef propRef)
		{
			return IBlockAccess.DefaultGetProp(this, propRef);
		}

		public bool IsOnlyTerrain(int _y)
		{
			return false;
		}

		public bool IsOnlyTerrainLayer(int _idx)
		{
			return false;
		}

		public bool IsEmpty()
		{
			return false;
		}

		public bool IsEmpty(int _y)
		{
			return false;
		}

		public bool IsEmptyLayer(int _y)
		{
			return false;
		}

		public byte GetStability(int _x, int _y, int _z)
		{
			return 15;
		}

		public byte GetStability(int _offs, int _y)
		{
			return 15;
		}

		public void SetStability(int _offs, int _y, byte _v)
		{
		}

		public void SetStability(int _x, int _y, int _z, byte _v)
		{
		}

		public byte GetLight(int _x, int _y, int _z, Chunk.LIGHT_TYPE _type)
		{
			return 15;
		}

		public int GetLightValue(int _x, int _y, int _z, int _darknessV)
		{
			return 15;
		}

		public float GetLightBrightness(int _x, int _y, int _z, int _darknessV)
		{
			return 1f;
		}

		public Vector3i GetWorldPos()
		{
			return new Vector3i(this.X, this.Y, this.Z);
		}

		public void SetVertexOffset(int _x, int _y, int _z, Vector3 _vertexOffset)
		{
		}

		public bool GetVertexOffset(int _x, int _y, int _z, out Vector3 _vertexOffset)
		{
			_vertexOffset = Vector3.zero;
			return false;
		}

		public void SetVertexYOffset(int _x, int _y, int _z, float _addYPos)
		{
		}

		public byte GetHeight(int _blockOffset)
		{
			return (byte)this.prefab.size.y;
		}

		public byte GetHeight(int _x, int _z)
		{
			return (byte)this.prefab.size.y;
		}

		public sbyte GetDensity(int _xzOffs, int _y)
		{
			return this.GetDensity(ChunkBlockLayerLegacy.OffsetX(_xzOffs), _y, ChunkBlockLayerLegacy.OffsetX(_xzOffs));
		}

		public sbyte GetDensity(int _x, int _y, int _z)
		{
			if (!this.checkCoordinates(ref _x, ref _y, ref _z))
			{
				return sbyte.MaxValue;
			}
			return this.prefab.GetDensity(_x, _y, _z);
		}

		public sbyte SetDensity(int _xzOffs, int _y, sbyte _density)
		{
			return 0;
		}

		public bool HasSameDensityValue(int _y)
		{
			return false;
		}

		public sbyte GetSameDensityValue(int _y)
		{
			return 0;
		}

		public BlockEntityData GetBlockEntity(Vector3i _blockPos)
		{
			return null;
		}

		public BlockEntityData GetBlockEntity(Transform _transform)
		{
			return null;
		}

		public void SetTopSoilBroken(int _x, int _z)
		{
		}

		public bool IsTopSoil(int _x, int _z)
		{
			return false;
		}

		public byte GetTerrainHeight(int _x, int _z)
		{
			for (int i = this.prefab.size.y - 1; i >= 0; i--)
			{
				if (this.GetBlock(_x, i, _z).Block.shape.IsTerrain())
				{
					return (byte)i;
				}
			}
			return 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Prefab prefab;
	}
}
