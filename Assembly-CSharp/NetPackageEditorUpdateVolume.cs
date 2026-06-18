using System;
using PrefabVolumes;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorUpdateVolume : NetPackage
{
	public NetPackageEditorUpdateVolume Setup(NetPackageEditorUpdateVolume.EChangeType _changeType, int _prefabInstanceId, int _volumeId, PrefabVolumeAbs _volume)
	{
		this.changeType = _changeType;
		this.prefabInstanceId = _prefabInstanceId;
		this.volumeId = _volumeId;
		this.volume = _volume.Clone();
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.changeType = (NetPackageEditorUpdateVolume.EChangeType)_br.ReadByte();
		this.prefabInstanceId = _br.ReadInt32();
		this.volumeId = _br.ReadInt32();
		PrefabVolumeAbs.EVolumeType evolumeType = (PrefabVolumeAbs.EVolumeType)_br.ReadByte();
		this.volume = PrefabVolumeAbs.CreateByType(evolumeType);
		this.volume.Read(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.changeType);
		_bw.Write(this.prefabInstanceId);
		_bw.Write(this.volumeId);
		_bw.Write((byte)this.volume.VolumeType);
		this.volume.Write(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
			NetPackageEditorUpdateVolume.EChangeType echangeType = this.changeType;
			if (echangeType != NetPackageEditorUpdateVolume.EChangeType.Added && echangeType == NetPackageEditorUpdateVolume.EChangeType.Changed)
			{
				PrefabVolumeManager.Instance.UpdatePropertiesServer(this.prefabInstanceId, this.volumeId, this.volume);
				return;
			}
			throw new ArgumentOutOfRangeException();
		}
		else
		{
			NetPackageEditorUpdateVolume.EChangeType echangeType = this.changeType;
			if (echangeType <= NetPackageEditorUpdateVolume.EChangeType.Changed)
			{
				PrefabVolumeManager.Instance.AddUpdatePropertiesClient(this.prefabInstanceId, this.volumeId, this.volume);
				return;
			}
			throw new ArgumentOutOfRangeException();
		}
	}

	public override int GetLength()
	{
		return 14 + this.volume.SerializedSize;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageEditorUpdateVolume.EChangeType changeType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabInstanceId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int volumeId;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabVolumeAbs volume;

	public enum EChangeType
	{
		Added,
		Changed
	}
}
