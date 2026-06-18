using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityCollect : NetPackage
{
	public NetPackageEntityCollect Setup(int _entityId, int _playerId)
	{
		this.entityId = _entityId;
		this.playerId = _playerId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.entityId = _br.ReadInt32();
		this.playerId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.entityId);
		_bw.Write(this.playerId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!base.ValidEntityIdForSender(this.playerId, false))
		{
			return;
		}
		Entity entity = _world.GetEntity(this.entityId);
		if (entity == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(this.entityId, this.playerId), false, this.playerId, -1, -1, null, 192, false);
			entity.OnCollectServer(this.playerId);
			return;
		}
		entity.OnCollectLocal(this.playerId);
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId;
}
