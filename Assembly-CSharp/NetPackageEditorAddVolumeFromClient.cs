using System;
using PrefabVolumes;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorAddVolumeFromClient : NetPackage
{
	public NetPackageEditorAddVolumeFromClient Setup(PrefabVolumeAbs.EVolumeType _volumeType, Vector3i _startPos, Vector3i _size)
	{
		this.addType = NetPackageEditorAddVolumeFromClient.EAddType.New;
		this.volumeType = _volumeType;
		this.startPos = _startPos;
		this.size = _size;
		return this;
	}

	public NetPackageEditorAddVolumeFromClient Setup(PrefabVolumeAbs.EVolumeType _volumeType, int _prefabInstanceId, int _existingIndex, Vector3i _offset)
	{
		this.addType = NetPackageEditorAddVolumeFromClient.EAddType.Clone;
		this.volumeType = _volumeType;
		this.startPos = _offset;
		this.prefabInstanceId = _prefabInstanceId;
		this.existingIndex = _existingIndex;
		return this;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public override void read(PooledBinaryReader _br)
	{
		this.addType = (NetPackageEditorAddVolumeFromClient.EAddType)_br.ReadByte();
		this.volumeType = (PrefabVolumeAbs.EVolumeType)_br.ReadByte();
		this.startPos = StreamUtils.ReadVector3i(_br);
		this.size = StreamUtils.ReadVector3i(_br);
		this.prefabInstanceId = (int)_br.ReadInt16();
		this.existingIndex = (int)_br.ReadInt16();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.addType);
		_bw.Write((byte)this.volumeType);
		StreamUtils.Write(_bw, this.startPos);
		StreamUtils.Write(_bw, this.size);
		_bw.Write((short)this.prefabInstanceId);
		_bw.Write((short)this.existingIndex);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (this.addType == NetPackageEditorAddVolumeFromClient.EAddType.New)
		{
			PrefabVolumeManager.Instance.AddVolumeServer(this.volumeType, this.startPos, this.size);
			return;
		}
		PrefabVolumeManager.Instance.CloneVolumeServer(this.volumeType, this.prefabInstanceId, this.existingIndex, this.startPos);
	}

	public override int GetLength()
	{
		return 29;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageEditorAddVolumeFromClient.EAddType addType;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabVolumeAbs.EVolumeType volumeType;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i startPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i size;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int existingIndex;

	public enum EAddType
	{
		New,
		Clone
	}
}
