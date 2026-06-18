using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWorldTime : NetPackage
{
	public NetPackageWorldTime Setup(ulong _worldTime)
	{
		this.worldTime = _worldTime;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.worldTime = _br.ReadUInt64();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.worldTime);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		_world.gameManager.SetWorldTime(this.worldTime);
	}

	public override int GetLength()
	{
		return 8;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong worldTime;
}
