using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSharedPartyKill : NetPackage
{
	public NetPackageSharedPartyKill Setup(int _entityID, int _killerID)
	{
		this.entityID = _entityID;
		this.killerID = _killerID;
		return this;
	}

	public NetPackageSharedPartyKill Setup(int _entityTypeID, int _xp, int _killerID, int _killedEntityID)
	{
		this.entityTypeID = _entityTypeID;
		this.xp = _xp;
		this.killerID = _killerID;
		this.entityID = _killedEntityID;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.entityTypeID = _br.ReadInt32();
		this.xp = _br.ReadInt32();
		this.entityID = _br.ReadInt32();
		this.killerID = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.entityTypeID);
		_bw.Write(this.xp);
		_bw.Write(this.entityID);
		_bw.Write(this.killerID);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GameManager.Instance.SharedKillServer(this.entityID, this.killerID, 1f);
			return;
		}
		GameManager.Instance.SharedKillClient(this.entityTypeID, this.xp, null, this.entityID, -1);
	}

	public override int GetLength()
	{
		return 25;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityTypeID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int xp;

	[PublicizedFrom(EAccessModifier.Private)]
	public int killerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityID;
}
