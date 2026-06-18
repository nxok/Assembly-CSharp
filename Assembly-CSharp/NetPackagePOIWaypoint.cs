using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePOIWaypoint : NetPackage
{
	public NetPackagePOIWaypoint Setup(NetPackagePOIWaypoint.OperationType _operation, int _entityId, int _prefabInstanceId = -1, bool _hiddenOnCompass = true)
	{
		this.operation = _operation;
		this.entityId = _entityId;
		this.prefabInstanceId = _prefabInstanceId;
		this.hiddenOnCompass = _hiddenOnCompass;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.operation = (NetPackagePOIWaypoint.OperationType)_br.ReadByte();
		this.entityId = _br.ReadInt32();
		switch (this.operation)
		{
		case NetPackagePOIWaypoint.OperationType.TrySet:
			this.prefabInstanceId = _br.ReadInt32();
			this.hiddenOnCompass = _br.ReadBoolean();
			return;
		case NetPackagePOIWaypoint.OperationType.Remove:
			this.prefabInstanceId = _br.ReadInt32();
			break;
		case NetPackagePOIWaypoint.OperationType.ClearAll:
			break;
		default:
			return;
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.operation);
		_bw.Write(this.entityId);
		switch (this.operation)
		{
		case NetPackagePOIWaypoint.OperationType.TrySet:
			_bw.Write(this.prefabInstanceId);
			_bw.Write(this.hiddenOnCompass);
			return;
		case NetPackagePOIWaypoint.OperationType.Remove:
			_bw.Write(this.prefabInstanceId);
			break;
		case NetPackagePOIWaypoint.OperationType.ClearAll:
			break;
		default:
			return;
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		EntityPlayer entityPlayer = _world.GetEntity(this.entityId) as EntityPlayer;
		if (entityPlayer == null)
		{
			Log.Warning(string.Format("NetPackagePOIWaypoint: Could not find player with entityId {0}", this.entityId));
			return;
		}
		switch (this.operation)
		{
		case NetPackagePOIWaypoint.OperationType.TrySet:
			POIWaypoint.TrySet(entityPlayer, this.prefabInstanceId, this.hiddenOnCompass);
			return;
		case NetPackagePOIWaypoint.OperationType.Remove:
			POIWaypoint.Remove(entityPlayer, this.prefabInstanceId);
			return;
		case NetPackagePOIWaypoint.OperationType.ClearAll:
			POIWaypoint.ClearAll(entityPlayer);
			return;
		default:
			return;
		}
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override int GetLength()
	{
		return 10;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackagePOIWaypoint.OperationType operation;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hiddenOnCompass;

	public enum OperationType : byte
	{
		TrySet,
		Remove,
		ClearAll
	}
}
