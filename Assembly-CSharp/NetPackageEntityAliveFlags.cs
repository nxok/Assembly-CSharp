using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAliveFlags : NetPackageEntityTargeted
{
	public NetPackageEntityAliveFlags Setup(EntityAlive _entity)
	{
		base.Setup(_entity.entityId);
		this.flags = 0;
		if (_entity.AimingGun)
		{
			this.flags |= 4;
		}
		if (_entity.Spawned)
		{
			this.flags |= 8;
		}
		if (_entity.Jumping)
		{
			this.flags |= 16;
		}
		if (_entity.IsBreakingBlocks)
		{
			this.flags |= 32;
		}
		if (_entity.IsAlert)
		{
			this.flags |= 64;
		}
		if (_entity.inventory.IsFlashlightOn)
		{
			this.flags |= 128;
		}
		if (_entity.IsGodMode.Value)
		{
			this.flags |= 256;
		}
		if (_entity.IsCrouching)
		{
			this.flags |= 512;
		}
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		this.flags = _reader.ReadUInt16();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.flags);
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
		EntityAlive entityAlive = _world.GetEntity(this.entityId) as EntityAlive;
		if (entityAlive)
		{
			entityAlive.AimingGun = (this.flags & 4) > 0;
			entityAlive.Spawned = (this.flags & 8) > 0;
			entityAlive.Jumping = (this.flags & 16) > 0;
			entityAlive.IsBreakingBlocks = (this.flags & 32) > 0;
			entityAlive.IsGodMode.Value = (this.flags & 256) > 0;
			entityAlive.Crouching = (this.flags & 512) > 0;
			if (entityAlive.isEntityRemote)
			{
				entityAlive.bReplicatedAlertFlag = (this.flags & 64) > 0;
			}
			entityAlive.inventory.SetFlashlight((this.flags & 128) > 0);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAliveFlags>().Setup(entityAlive), false, -1, base.Sender.entityId, -1, null, 192, false);
			}
		}
	}

	public override int GetLength()
	{
		return 60;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFApproachingEnemy = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFApproachingPlayer = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFAimingGun = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFSpawned = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFJumping = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsBreakingBlocks = 32;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsAlert = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsFlashlightOn = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFIsGodMode = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFCrouching = 512;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort flags;
}
