using System;
using UnityEngine.Scripting;

[Preserve]
public abstract class NetPackageEntityTargeted : NetPackage
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Setup(int _entityId)
	{
		this.entityId = _entityId;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
	}

	public override bool ShouldProcess(World _world, GameManager _callbacks)
	{
		return _world == null || (_world.netEntityPackageQueue == null || _world.entityAsyncManager == null) || (!_world.netEntityPackageQueue.HasPackagesForEntity(this.entityId) && !_world.entityAsyncManager.IsEntityPending(this.entityId));
	}

	public override void HandleSkipped(World _world, GameManager _callbacks)
	{
		_world.netEntityPackageQueue.EnqueueNetPackageForEntity(this.entityId, this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public NetPackageEntityTargeted()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int entityId;
}
