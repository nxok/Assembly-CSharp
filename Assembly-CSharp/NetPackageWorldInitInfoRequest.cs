using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldInitInfoRequest : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackageWorldInitInfoRequest Setup()
	{
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			Log.Error("[NetPackageWorldInitInfoRequest] world is null");
			return;
		}
		IChunkProvider chunkProvider = _world.ChunkCache.ChunkProvider;
		EventPrefabs eventPrefabs = ((chunkProvider != null) ? chunkProvider.GetEventPrefabs() : null);
		List<PrefabInstance.Serializable> list;
		if (eventPrefabs != null)
		{
			list = eventPrefabs.GetPrefabsSerialized();
		}
		else
		{
			list = new List<PrefabInstance.Serializable>();
			Log.Error("[NetPackageWorldInitInfoRequest] EventPrefabs not available");
		}
		List<ValueTuple<int, WallVolume>> allWallVolumes = _world.GetAllWallVolumes();
		base.Sender.SendPackage(NetPackageManager.GetPackage<NetPackageWorldInitInfo>().Setup(list, allWallVolumes));
	}

	public override int GetLength()
	{
		return 1;
	}
}
