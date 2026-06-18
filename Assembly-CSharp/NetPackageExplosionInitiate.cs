using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageExplosionInitiate : NetPackage
{
	public NetPackageExplosionInitiate Setup(Vector3 _worldPos, Vector3i _blockPos, Quaternion _rotation, ExplosionData _explosionData, int _entityId, float _delay, bool _bRemoveBlockAtExplPosition, ItemValue _itemValueExplosive)
	{
		this.worldPos = _worldPos;
		this.blockPos = _blockPos;
		this.rotation = _rotation;
		this.explosionData = _explosionData;
		this.entityId = _entityId;
		this.delay = _delay;
		this.bRemoveBlockAtExplPosition = _bRemoveBlockAtExplPosition;
		this.itemValueExplosive = _itemValueExplosive;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.worldPos = StreamUtils.ReadVector3(_br);
		this.blockPos = StreamUtils.ReadVector3i(_br);
		this.rotation = StreamUtils.ReadQuaterion(_br);
		int num = (int)_br.ReadUInt16();
		this.explosionData = new ExplosionData(_br.ReadBytes(num));
		this.entityId = _br.ReadInt32();
		this.delay = _br.ReadSingle();
		this.bRemoveBlockAtExplPosition = _br.ReadBoolean();
		if (_br.ReadBoolean())
		{
			this.itemValueExplosive = new ItemValue();
			this.itemValueExplosive.Read(_br);
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, this.worldPos);
		StreamUtils.Write(_bw, this.blockPos);
		StreamUtils.Write(_bw, this.rotation);
		byte[] array = this.explosionData.ToByteArray();
		_bw.Write((ushort)array.Length);
		_bw.Write(array);
		_bw.Write(this.entityId);
		_bw.Write(this.delay);
		_bw.Write(this.bRemoveBlockAtExplPosition);
		_bw.Write(this.itemValueExplosive != null);
		if (this.itemValueExplosive != null)
		{
			this.itemValueExplosive.Write(_bw);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		_world.GetGameManager().ExplosionServer(this.worldPos, this.blockPos, this.rotation, this.explosionData, this.entityId, this.delay, this.bRemoveBlockAtExplPosition, this.itemValueExplosive);
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public override int GetLength()
	{
		return 70;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 worldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion rotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public ExplosionData explosionData;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delay;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bRemoveBlockAtExplPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValueExplosive;
}
