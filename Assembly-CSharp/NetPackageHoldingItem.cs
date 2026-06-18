using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageHoldingItem : NetPackage
{
	public NetPackageHoldingItem Setup(EntityAlive _entity)
	{
		this.entityId = _entity.entityId;
		this.holdingItemStack = _entity.inventory.holdingItemStack;
		this.holdingItemIndex = (byte)_entity.inventory.holdingItemIdx;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		this.holdingItemStack = new ItemStack();
		this.holdingItemStack.Read(_reader);
		this.holdingItemIndex = _reader.ReadByte();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		this.holdingItemStack.Write(_writer);
		_writer.Write(this.holdingItemIndex);
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
			entityAlive.EnqueueNetworkHoldingData(this.holdingItemStack, this.holdingItemIndex);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageHoldingItem>().Setup(entityAlive), false, -1, base.Sender.entityId, -1, null, 192, false);
		}
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack holdingItemStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte holdingItemIndex;
}
