using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageTurretSync : NetPackage
{
	public NetPackageTurretSync Setup(int _entityId, int _targetEntityId, bool _isOn, ItemValue _originalItemValue)
	{
		this.entityId = _entityId;
		this.targetEntityId = _targetEntityId;
		this.isOn = _isOn;
		this.itemValue = _originalItemValue;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		this.targetEntityId = _reader.ReadInt32();
		this.isOn = _reader.ReadBoolean();
		this.itemValue = ItemValue.None;
		this.itemValue.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		_writer.Write(this.targetEntityId);
		_writer.Write(this.isOn);
		this.itemValue.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityTurret entityTurret = GameManager.Instance.World.GetEntity(this.entityId) as EntityTurret;
		if (entityTurret != null)
		{
			entityTurret.TargetEntityId = this.targetEntityId;
			entityTurret.OriginalItemValue = this.itemValue;
			entityTurret.IsOn = this.isOn;
		}
	}

	public override int GetLength()
	{
		return 20;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public int entityId;

	public int targetEntityId;

	public bool isOn;

	public ItemValue itemValue;
}
