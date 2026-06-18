using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldSpawnPoints : NetPackage
{
	public NetPackageWorldSpawnPoints Setup(SpawnPointList _spawnPoints)
	{
		this.spawnPoints = _spawnPoints;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.spawnPoints = new SpawnPointList();
		this.spawnPoints.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		this.spawnPoints.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.SetSpawnPointList(this.spawnPoints);
	}

	public override int GetLength()
	{
		if (this.spawnPoints == null)
		{
			return 0;
		}
		return this.spawnPoints.Count * 20;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public SpawnPointList spawnPoints;
}
