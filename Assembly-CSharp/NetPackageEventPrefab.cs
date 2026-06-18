using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEventPrefab : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageEventPrefab Setup(NetPackageEventPrefab.Operation _operation, PrefabInstance _pi)
	{
		this.operation = _operation;
		this.serializablePi = new PrefabInstance.Serializable(_pi);
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.operation = (NetPackageEventPrefab.Operation)_br.ReadByte();
		this.serializablePi = new PrefabInstance.Serializable(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.operation);
		this.serializablePi.Write(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			if (!_callbacks.worldInitInfoReceived)
			{
				return;
			}
			int id = this.serializablePi.id;
			string prefabName = this.serializablePi.prefabName;
			Vector3i position = this.serializablePi.position;
			byte rotation = this.serializablePi.rotation;
			NetPackageEventPrefab.Operation operation = this.operation;
			if (operation == NetPackageEventPrefab.Operation.Add)
			{
				_world.m_EventPrefabsClient.TryAdd(id, prefabName, rotation, position);
				return;
			}
			if (operation != NetPackageEventPrefab.Operation.Remove)
			{
				return;
			}
			_world.m_EventPrefabsClient.Remove(id, prefabName, rotation, position);
		}
	}

	public override int GetLength()
	{
		return 1 + this.serializablePi.GetLength();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageEventPrefab.Operation operation;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance.Serializable serializablePi;

	public enum Operation
	{
		Add,
		Remove
	}
}
