using System;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerTwitchStats : NetPackageEntityTargeted
{
	public NetPackagePlayerTwitchStats Setup(EntityAlive _entity)
	{
		base.Setup(_entity.entityId);
		EntityPlayer entityPlayer = _entity as EntityPlayer;
		if (entityPlayer)
		{
			this.twitchEnabled = entityPlayer.TwitchEnabled;
			this.twitchSafe = entityPlayer.TwitchSafe;
			this.twitchVoteLock = entityPlayer.TwitchVoteLock;
			this.twitchVisionDisabled = entityPlayer.TwitchVisionDisabled;
			this.twitchActionsEnabled = entityPlayer.TwitchActionsEnabled;
		}
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		this.twitchEnabled = _reader.ReadBoolean();
		this.twitchSafe = _reader.ReadBoolean();
		this.twitchVoteLock = (TwitchVoteLockTypes)_reader.ReadByte();
		this.twitchVisionDisabled = _reader.ReadBoolean();
		this.twitchActionsEnabled = (EntityPlayer.TwitchActionsStates)_reader.ReadByte();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.twitchEnabled);
		_writer.Write(this.twitchSafe);
		_writer.Write((byte)this.twitchVoteLock);
		_writer.Write(this.twitchVisionDisabled);
		_writer.Write((byte)this.twitchActionsEnabled);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityPlayer entityPlayer = _world.GetEntity(this.entityId) as EntityPlayer;
		if (entityPlayer)
		{
			entityPlayer.TwitchEnabled = this.twitchEnabled;
			entityPlayer.TwitchSafe = this.twitchSafe;
			entityPlayer.TwitchVoteLock = this.twitchVoteLock;
			entityPlayer.TwitchVisionDisabled = this.twitchVisionDisabled;
			entityPlayer.TwitchActionsEnabled = this.twitchActionsEnabled;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerTwitchStats>().Setup(entityPlayer), false, -1, base.Sender.entityId, -1, null, 192, false);
		}
	}

	public override int GetLength()
	{
		return 60;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool twitchEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool twitchSafe;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchVoteLockTypes twitchVoteLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool twitchVisionDisabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer.TwitchActionsStates twitchActionsEnabled;
}
