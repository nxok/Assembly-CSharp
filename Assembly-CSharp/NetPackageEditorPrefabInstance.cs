using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorPrefabInstance : NetPackage
{
	public NetPackageEditorPrefabInstance Setup(NetPackageEditorPrefabInstance.EChangeType _changeType, PrefabInstance _prefabInstance)
	{
		this.changeType = _changeType;
		this.id = _prefabInstance.id;
		this.boundingBoxPosition = _prefabInstance.boundingBoxPosition;
		this.boundingBoxSize = _prefabInstance.boundingBoxSize;
		this.name = _prefabInstance.name;
		this.size = _prefabInstance.prefab.size;
		this.filename = _prefabInstance.prefab.PrefabName;
		this.localRotation = _prefabInstance.prefab.GetLocalRotation();
		this.yOffset = _prefabInstance.prefab.yOffset;
		return this;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override void read(PooledBinaryReader _br)
	{
		this.changeType = (NetPackageEditorPrefabInstance.EChangeType)_br.ReadByte();
		this.id = _br.ReadInt32();
		this.boundingBoxPosition = StreamUtils.ReadVector3i(_br);
		this.boundingBoxSize = StreamUtils.ReadVector3i(_br);
		this.name = _br.ReadString();
		this.size = StreamUtils.ReadVector3i(_br);
		this.filename = _br.ReadString();
		this.localRotation = _br.ReadInt32();
		this.yOffset = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.changeType);
		_bw.Write(this.id);
		StreamUtils.Write(_bw, this.boundingBoxPosition);
		StreamUtils.Write(_bw, this.boundingBoxSize);
		_bw.Write(this.name);
		StreamUtils.Write(_bw, this.size);
		_bw.Write(this.filename);
		_bw.Write(this.localRotation);
		_bw.Write(this.yOffset);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
			return;
		}
		switch (this.changeType)
		{
		case NetPackageEditorPrefabInstance.EChangeType.Added:
			PrefabInstanceClientManager.Instance.PrefabLoadedClient(this.id, this.boundingBoxPosition, this.boundingBoxSize, this.name, this.size, this.filename, this.localRotation, this.yOffset);
			return;
		case NetPackageEditorPrefabInstance.EChangeType.Changed:
			PrefabInstanceClientManager.Instance.PrefabChangedClient(this.id, this.boundingBoxPosition, this.boundingBoxSize, this.name, this.size, this.filename, this.localRotation, this.yOffset);
			return;
		case NetPackageEditorPrefabInstance.EChangeType.Removed:
			PrefabInstanceClientManager.Instance.PrefabRemovedClient(this.id);
			return;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override int GetLength()
	{
		return 33 + this.name.Length + 12 + this.filename.Length + 4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageEditorPrefabInstance.EChangeType changeType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int id;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i boundingBoxPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i boundingBoxSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i size;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filename;

	[PublicizedFrom(EAccessModifier.Private)]
	public int localRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public int yOffset;

	public enum EChangeType
	{
		Added,
		Changed,
		Removed
	}
}
