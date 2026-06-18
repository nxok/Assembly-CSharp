using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChunkProviderAbstract : IChunkProvider
{
	public virtual IEnumerator Init(World _worldData)
	{
		yield return null;
		yield break;
	}

	public virtual void Update()
	{
	}

	public virtual void StopUpdate()
	{
	}

	public virtual void Cleanup()
	{
	}

	public virtual void RequestChunk(int _x, int _y)
	{
	}

	public virtual HashSetList<long> GetRequestedChunks()
	{
		return null;
	}

	public virtual void SaveAll()
	{
	}

	public virtual void SaveRandomChunks(int count, ulong _curWorldTimeInTicks, ArraySegment<long> _activeChunkSet)
	{
	}

	public virtual void ReloadAllChunks()
	{
	}

	public virtual void ClearCaches()
	{
	}

	public virtual EnumChunkProviderId GetProviderId()
	{
		return EnumChunkProviderId.None;
	}

	public virtual void UnloadChunk(Chunk _chunk)
	{
	}

	public virtual DynamicPrefabDecorator GetDynamicPrefabDecorator()
	{
		return null;
	}

	public virtual EventPrefabs GetEventPrefabs()
	{
		return null;
	}

	public virtual SpawnPointList GetSpawnPointList()
	{
		return null;
	}

	public virtual void SetSpawnPointList(SpawnPointList _spawnPointList)
	{
	}

	public virtual bool GetOverviewMap(Vector2i _startPos, Vector2i _size, Color[] mapColors)
	{
		return false;
	}

	public virtual void SetDecorationsEnabled(bool _bEnable)
	{
		this.bDecorationsEnabled = _bEnable;
	}

	public virtual bool IsDecorationsEnabled()
	{
		return this.bDecorationsEnabled;
	}

	public virtual bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		_minSize = Vector3i.zero;
		_maxSize = Vector3i.zero;
		return false;
	}

	public virtual BoundsInt GetWorldBounds()
	{
		Vector3i vector3i;
		Vector3i vector3i2;
		this.GetWorldExtent(out vector3i, out vector3i2);
		return new BoundsInt(vector3i.x, vector3i.y, vector3i.z, vector3i2.x, vector3i2.y, vector3i2.z);
	}

	public virtual Vector2i GetWorldSize()
	{
		Vector3i vector3i;
		Vector3i vector3i2;
		this.GetWorldExtent(out vector3i, out vector3i2);
		return new Vector2i(vector3i2.x - vector3i.x + 1, vector3i2.y - vector3i.y + 1);
	}

	public virtual IBiomeProvider GetBiomeProvider()
	{
		return null;
	}

	public virtual ITerrainGenerator GetTerrainGenerator()
	{
		return null;
	}

	public virtual int GetPOIBlockIdOverride(int x, int z)
	{
		return 0;
	}

	public virtual float GetPOIHeightOverride(int x, int z)
	{
		return 0f;
	}

	public virtual IEnumerator FillOccupiedMap(int w, int h, DecoOccupiedMap occupiedMap, List<PrefabInstance> overridePOIList = null)
	{
		yield break;
	}

	public virtual void RebuildTerrain(HashSetLong _chunks, Vector3i _areaStart, Vector3i _areaSize, bool _isStopStabilityCalc, bool _isRegenChunk, bool _isFillEmptyBlocks, bool _isReset)
	{
	}

	public virtual ChunkProtectionLevel GetChunkProtectionLevel(Vector3i worldPos)
	{
		return ChunkProtectionLevel.None;
	}

	public GameUtils.WorldInfo WorldInfo
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ChunkProviderAbstract()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bDecorationsEnabled;
}
