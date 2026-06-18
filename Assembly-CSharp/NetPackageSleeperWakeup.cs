using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSleeperWakeup : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageSleeperWakeup Setup(int targetId)
	{
		this.m_targetId = targetId;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.m_targetId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.m_targetId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || !_world.IsRemote())
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(this.m_targetId) as EntityAlive;
		if (entityAlive == null)
		{
			return;
		}
		entityAlive.ConditionalTriggerSleeperWakeUp();
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_targetId;
}
