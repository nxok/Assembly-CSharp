using System;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageAudioPlayInHead : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageAudioPlayInHead Setup(string _soundName, bool _isUnique)
	{
		this.soundName = _soundName;
		this.isUnique = _isUnique;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.soundName = _br.ReadString();
		this.isUnique = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.soundName);
		_bw.Write(this.isUnique);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Manager.PlayInsidePlayerHead(this.soundName, -1, 0f, false, this.isUnique);
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUnique;
}
