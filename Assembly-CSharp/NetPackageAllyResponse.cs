using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageAllyResponse : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageAllyResponse Setup(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, AllyStore.AllyStatus _newStatus, AllyStore.AllyEvent _allyEventSource, AllyStore.AllyEvent _allyEventTarget)
	{
		this.source = _source;
		this.target = _target;
		this.newStatus = _newStatus;
		this.allyEventSource = _allyEventSource;
		this.allyEventTarget = _allyEventTarget;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.source = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.target = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.newStatus = (AllyStore.AllyStatus)_br.ReadByte();
		this.allyEventSource = (AllyStore.AllyEvent)_br.ReadByte();
		this.allyEventTarget = (AllyStore.AllyEvent)_br.ReadByte();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		this.source.ToStream(_bw, false);
		this.target.ToStream(_bw, false);
		_bw.Write((byte)this.newStatus);
		_bw.Write((byte)this.allyEventSource);
		_bw.Write((byte)this.allyEventTarget);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		GameManager.Instance.persistentPlayers.Allies.AllyUpdateResponse(this.source, this.target, this.newStatus, this.allyEventSource, this.allyEventTarget);
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
	public AllyStore.AllyStatus newStatus;

	[PublicizedFrom(EAccessModifier.Private)]
	public AllyStore.AllyEvent allyEventSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public AllyStore.AllyEvent allyEventTarget;
}
