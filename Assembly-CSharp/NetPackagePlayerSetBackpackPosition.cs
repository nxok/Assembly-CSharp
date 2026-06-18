using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerSetBackpackPosition : NetPackage
{
	public NetPackagePlayerSetBackpackPosition Setup(int _playerId, List<Vector3i> _positions)
	{
		this.playerId = _playerId;
		this.positions = _positions;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.playerId = _br.ReadInt32();
		int num = (int)_br.ReadByte();
		this.positions = new List<Vector3i>();
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				this.positions.Add(StreamUtils.ReadVector3i(_br));
			}
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.playerId);
		if (this.positions == null)
		{
			_bw.Write(0);
			return;
		}
		_bw.Write((byte)this.positions.Count);
		for (int i = 0; i < this.positions.Count; i++)
		{
			StreamUtils.Write(_bw, this.positions[i]);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (GameManager.Instance.World == null)
		{
			return;
		}
		EntityPlayerLocal entityPlayerLocal = GameManager.Instance.World.GetEntity(this.playerId) as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			entityPlayerLocal.SetDroppedBackpackPositions(this.positions);
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
		return 16;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> positions;
}
