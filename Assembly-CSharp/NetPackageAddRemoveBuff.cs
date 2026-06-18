using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageAddRemoveBuff : NetPackage
{
	public NetPackageAddRemoveBuff Setup(int _entityId, string _buffName, float _duration, bool _adding, int _instigatorId, Vector3i _instigatorPos)
	{
		this.entityId = _entityId;
		this.buffName = _buffName;
		this.adding = _adding;
		this.instigatorId = _instigatorId;
		this.duration = _duration;
		this.instigatorPos = _instigatorPos;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		this.buffName = _reader.ReadString();
		this.duration = _reader.ReadSingle();
		this.adding = _reader.ReadBoolean();
		this.instigatorId = _reader.ReadInt32();
		this.instigatorPos = StreamUtils.ReadVector3i(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		_writer.Write(this.buffName);
		_writer.Write(this.duration);
		_writer.Write(this.adding);
		_writer.Write(this.instigatorId);
		StreamUtils.Write(_writer, this.instigatorPos);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			ConnectionManager instance = SingletonMonoBehaviour<ConnectionManager>.Instance;
			NetPackage netPackage = NetPackageManager.GetPackage<NetPackageAddRemoveBuff>().Setup(this.entityId, this.buffName, this.duration, this.adding, this.instigatorId, this.instigatorPos);
			bool flag = false;
			int num = -1;
			int num2 = this.entityId;
			instance.SendPackage(netPackage, flag, num, this.instigatorId, num2, null, 192, false);
		}
		EntityAlive entityAlive = _world.GetEntity(this.entityId) as EntityAlive;
		if (entityAlive != null)
		{
			if (this.adding)
			{
				entityAlive.Buffs.AddBuff(this.buffName, this.instigatorPos, this.instigatorId, false, false, this.duration);
				return;
			}
			entityAlive.Buffs.RemoveBuff(this.buffName, this.instigatorId, false);
		}
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int instigatorId;

	[PublicizedFrom(EAccessModifier.Private)]
	public string buffName;

	[PublicizedFrom(EAccessModifier.Private)]
	public float duration;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool adding;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i instigatorPos;
}
