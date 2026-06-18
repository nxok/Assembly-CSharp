using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageBiomeIntensity : NetPackage
{
	public NetPackageBiomeIntensity Setup(BiomeIntensity _bi)
	{
		this.bi = _bi;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.bi = BiomeIntensity.Default;
		this.bi.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		this.bi.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		_world.LocalPlayerBiomeIntensityStandingOn = this.bi;
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeIntensity bi;
}
