using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSetAttackTarget : NetPackageEntityTargeted
{
	public NetPackageSetAttackTarget Setup(int entityId, int targetId)
	{
		base.Setup(entityId);
		this.m_targetId = targetId;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		this.m_targetId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.m_targetId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(this.entityId) as EntityAlive;
		if (entityAlive == null)
		{
			return;
		}
		EntityAlive entityAlive2 = _world.GetEntity(this.m_targetId) as EntityAlive;
		entityAlive.SetAttackTargetClient(entityAlive2);
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_targetId;
}
