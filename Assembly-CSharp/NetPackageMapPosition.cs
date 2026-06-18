using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageMapPosition : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackageMapPosition Setup(int _entityId, Vector2i _mapMiddlePosition)
	{
		this.entityId = _entityId;
		this.mapMiddlePosition = _mapMiddlePosition;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.entityId = _br.ReadInt32();
		this.mapMiddlePosition = StreamUtils.ReadVector2i(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.entityId);
		StreamUtils.Write(_bw, this.mapMiddlePosition);
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
		EntityPlayer entityPlayer = _world.GetEntity(this.entityId) as EntityPlayer;
		if (entityPlayer != null && entityPlayer.ChunkObserver.mapDatabase != null)
		{
			entityPlayer.ChunkObserver.mapDatabase.SetClientMapMiddlePosition(this.mapMiddlePosition);
		}
	}

	public override int GetLength()
	{
		return 16;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i mapMiddlePosition;
}
