using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSleeperPose : NetPackage
{
	public NetPackageSleeperPose Setup(int targetId, byte pose)
	{
		this.m_targetId = targetId;
		this.m_pose = pose;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.m_targetId = _reader.ReadInt32();
		this.m_pose = _reader.ReadByte();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.m_targetId);
		_writer.Write(this.m_pose);
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
		entityAlive.TriggerSleeperPose((int)this.m_pose, false);
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_targetId;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte m_pose;
}
