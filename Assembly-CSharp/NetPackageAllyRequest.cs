using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageAllyRequest : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackageAllyRequest Setup(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, bool _addAlly)
	{
		this.source = _source;
		this.target = _target;
		this.addAlly = _addAlly;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.source = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.target = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.addAlly = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		this.source.ToStream(_bw, false);
		this.target.ToStream(_bw, false);
		_bw.Write(this.addAlly);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		GameManager.Instance.persistentPlayers.Allies.ProcessAllyRequest(this.source, this.target, this.addAlly);
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs source;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs target;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool addAlly;
}
