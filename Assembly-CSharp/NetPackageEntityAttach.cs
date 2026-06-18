using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAttach : NetPackage
{
	public NetPackageEntityAttach Setup(NetPackageEntityAttach.AttachType _attachType, int _riderId, int _vehicleId, int _slot)
	{
		this.attachType = _attachType;
		this.riderId = _riderId;
		this.vehicleId = _vehicleId;
		this.slot = _slot;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.attachType = (NetPackageEntityAttach.AttachType)_br.ReadByte();
		this.riderId = _br.ReadInt32();
		this.vehicleId = _br.ReadInt32();
		this.slot = (int)_br.ReadInt16();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.attachType);
		_bw.Write(this.riderId);
		_bw.Write(this.vehicleId);
		_bw.Write((short)this.slot);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Entity entity = GameManager.Instance.World.GetEntity(this.riderId);
		if (entity == null)
		{
			return;
		}
		Entity entity2 = GameManager.Instance.World.GetEntity(this.vehicleId);
		switch (this.attachType)
		{
		case NetPackageEntityAttach.AttachType.AttachServer:
		{
			if (entity2 == null)
			{
				return;
			}
			int num = entity2.FindAttachSlot(entity);
			if (num < 0)
			{
				num = entity.AttachToEntity(entity2, this.slot);
			}
			if (num >= 0)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.AttachClient, this.riderId, this.vehicleId, num), false, -1, -1, -1, null, 192, false);
				return;
			}
			break;
		}
		case NetPackageEntityAttach.AttachType.AttachClient:
			if (entity2 == null)
			{
				return;
			}
			entity.AttachToEntity(entity2, this.slot);
			return;
		case NetPackageEntityAttach.AttachType.DetachServer:
			entity.Detach();
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAttach>().Setup(NetPackageEntityAttach.AttachType.DetachClient, this.riderId, -1, -1), false, -1, this.riderId, -1, null, 192, false);
			return;
		case NetPackageEntityAttach.AttachType.DetachClient:
			entity.Detach();
			break;
		default:
			return;
		}
	}

	public override int GetLength()
	{
		return 20;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int riderId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int vehicleId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int slot;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageEntityAttach.AttachType attachType;

	public enum AttachType : byte
	{
		AttachServer,
		AttachClient,
		DetachServer,
		DetachClient
	}
}
