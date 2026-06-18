using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerSpawnedInWorld : NetPackage
{
	public NetPackagePlayerSpawnedInWorld Setup(RespawnType _respawnReason, Vector3i _position, int _entityId)
	{
		this.respawnReason = _respawnReason;
		this.position = _position;
		this.entityId = _entityId;
		return this;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.Both;
		}
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.respawnReason = (RespawnType)_reader.ReadInt32();
		this.position = StreamUtils.ReadVector3i(_reader);
		this.entityId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((int)this.respawnReason);
		StreamUtils.Write(_writer, this.position);
		_writer.Write(this.entityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!base.ValidEntityIdForSender(this.entityId, false))
		{
			return;
		}
		GameManager.Instance.PlayerSpawnedInWorld(base.Sender, this.respawnReason, this.position, this.entityId);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerSpawnedInWorld>().Setup(this.respawnReason, new Vector3i(this.position), this.entityId), false, -1, base.Sender.entityId, -1, null, 192, false);
		}
	}

	public override int GetLength()
	{
		return 16;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RespawnType respawnReason;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i position;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;
}
