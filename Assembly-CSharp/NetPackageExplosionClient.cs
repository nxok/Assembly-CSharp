using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageExplosionClient : NetPackage
{
	public NetPackageExplosionClient Setup(Vector3 _center, Quaternion _rotation, int _expType, int _blastPower, float _blastRadius, float _blockDamage, int _entityId, List<BlockChangeInfo> _explosionChanges)
	{
		this.center = _center;
		this.rotation = _rotation;
		this.expType = _expType;
		this.blastPower = _blastPower;
		this.blastRadius = (int)_blastRadius;
		this.blockDamage = (int)_blockDamage;
		this.entityId = _entityId;
		this.explosionChanges.Clear();
		this.explosionChanges.AddRange(_explosionChanges);
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.center = StreamUtils.ReadVector3(_br);
		this.rotation = StreamUtils.ReadQuaterion(_br);
		this.expType = (int)_br.ReadInt16();
		this.blastPower = (int)_br.ReadInt16();
		this.blastRadius = (int)_br.ReadInt16();
		this.blockDamage = (int)_br.ReadInt16();
		this.entityId = _br.ReadInt32();
		int num = (int)_br.ReadUInt16();
		this.explosionChanges.Clear();
		for (int i = 0; i < num; i++)
		{
			BlockChangeInfo blockChangeInfo = new BlockChangeInfo();
			blockChangeInfo.Read(_br);
			this.explosionChanges.Add(blockChangeInfo);
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, this.center);
		StreamUtils.Write(_bw, this.rotation);
		_bw.Write((short)this.expType);
		_bw.Write((ushort)this.blastPower);
		_bw.Write((ushort)this.blastRadius);
		_bw.Write((ushort)this.blockDamage);
		_bw.Write(this.entityId);
		_bw.Write((ushort)this.explosionChanges.Count);
		for (int i = 0; i < this.explosionChanges.Count; i++)
		{
			this.explosionChanges[i].Write(_bw);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		_world.GetGameManager().ExplosionClient(this.center, this.rotation, this.expType, this.blastPower, (float)this.blastRadius, (float)this.blockDamage, this.entityId, this.explosionChanges);
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
		return 24 + this.explosionChanges.Count * 30;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 center;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion rotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockChangeInfo> explosionChanges = new List<BlockChangeInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int expType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blastPower;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blastRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blockDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;
}
