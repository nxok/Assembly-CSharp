using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldInitInfo : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageWorldInitInfo Setup(List<PrefabInstance.Serializable> _eventPrefabs, List<ValueTuple<int, WallVolume>> _wallVolumes)
	{
		this.eventPrefabs = _eventPrefabs;
		foreach (PrefabInstance.Serializable serializable in this.eventPrefabs)
		{
			this.dataLength += serializable.GetLength();
		}
		this.wallVolumes = _wallVolumes;
		this.dataLength += _wallVolumes.Count * 29;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		if (this.eventPrefabs == null)
		{
			this.eventPrefabs = new List<PrefabInstance.Serializable>();
		}
		this.eventPrefabs.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			this.eventPrefabs.Add(new PrefabInstance.Serializable(_br));
		}
		if (this.wallVolumes == null)
		{
			this.wallVolumes = new List<ValueTuple<int, WallVolume>>();
		}
		this.eventPrefabs.Clear();
		int num2 = _br.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			this.wallVolumes.Add(new ValueTuple<int, WallVolume>(_br.ReadInt32(), WallVolume.Read(_br)));
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.eventPrefabs.Count);
		foreach (PrefabInstance.Serializable serializable in this.eventPrefabs)
		{
			serializable.Write(_bw);
		}
		_bw.Write(this.wallVolumes.Count);
		foreach (ValueTuple<int, WallVolume> valueTuple in this.wallVolumes)
		{
			int item = valueTuple.Item1;
			WallVolume item2 = valueTuple.Item2;
			_bw.Write(item);
			item2.Write(_bw);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			Log.Error("[NetPackageWorldInitInfo] world is null");
			return;
		}
		EventPrefabsClient eventPrefabsClient = _world.m_EventPrefabsClient;
		if (eventPrefabsClient != null)
		{
			using (List<PrefabInstance.Serializable>.Enumerator enumerator = this.eventPrefabs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PrefabInstance.Serializable serializable = enumerator.Current;
					eventPrefabsClient.TryAdd(serializable.id, serializable.prefabName, serializable.rotation, serializable.position);
				}
				goto IL_006F;
			}
		}
		Log.Error("[NetPackageWorldInitInfo] EventPrefabsClient not available");
		IL_006F:
		_world.SetWallVolumesForClient(this.wallVolumes);
		_callbacks.worldInitInfoReceived = true;
	}

	public override int GetLength()
	{
		return this.dataLength;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance.Serializable> eventPrefabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ValueTuple<int, WallVolume>> wallVolumes;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dataLength;
}
