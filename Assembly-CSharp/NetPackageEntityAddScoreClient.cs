using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAddScoreClient : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageEntityAddScoreClient Setup(int _entityId, int _zombieKills, int _playerKills, int _otherTeamNumber, int _conditions)
	{
		this.entityId = _entityId;
		this.zombieKills = _zombieKills;
		this.playerKills = _playerKills;
		this.otherTeamNumber = _otherTeamNumber;
		this.conditions = _conditions;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		this.zombieKills = (int)_reader.ReadInt16();
		this.playerKills = (int)_reader.ReadInt16();
		this.otherTeamNumber = (int)_reader.ReadInt16();
		this.conditions = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		_writer.Write((short)this.zombieKills);
		_writer.Write((short)this.playerKills);
		_writer.Write((short)this.otherTeamNumber);
		_writer.Write(this.conditions);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = (EntityAlive)_world.GetEntity(this.entityId);
		if (entityAlive != null)
		{
			entityAlive.AddScore(0, this.zombieKills, this.playerKills, this.otherTeamNumber, this.conditions);
		}
	}

	public override int GetLength()
	{
		return 20;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int zombieKills;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int playerKills;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int otherTeamNumber;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int conditions;
}
