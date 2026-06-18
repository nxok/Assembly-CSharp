using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDeleteChunkData : NetPackage
{
	public NetPackageDeleteChunkData Setup(ICollection<long> _chunkKeys)
	{
		this.chunkKeys.Clear();
		this.chunkKeys.AddRange(_chunkKeys);
		this.length = 4 + 8 * this.chunkKeys.Count;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		int num = _br.ReadInt32();
		this.chunkKeys = new List<long>();
		for (int i = 0; i < num; i++)
		{
			this.chunkKeys.Add(_br.ReadInt64());
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.chunkKeys.Count);
		for (int i = 0; i < this.chunkKeys.Count; i++)
		{
			_bw.Write(this.chunkKeys[i]);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		DynamicMeshUnity.DeleteDynamicMeshData(this.chunkKeys);
		WaterSimulationNative instance = WaterSimulationNative.Instance;
		if (instance != null)
		{
			WaterSimulationApplyChanges changeApplier = instance.changeApplier;
			if (changeApplier != null)
			{
				changeApplier.DiscardChangesForChunks(this.chunkKeys);
			}
		}
		MultiBlockManager.Instance.CullChunklessDataOnClient(this.chunkKeys);
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override int GetLength()
	{
		return this.length;
	}

	public List<long> chunkKeys = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;
}
