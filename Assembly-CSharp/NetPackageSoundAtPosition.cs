using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSoundAtPosition : NetPackage
{
	public NetPackageSoundAtPosition Setup(Vector3 _pos, string _audioClipName, AudioRolloffMode _mode, int _distance, int _entityId, float _volumeScale)
	{
		this.pos = _pos;
		this.audioClipName = _audioClipName;
		this.mode = _mode;
		this.distance = _distance;
		this.entityId = _entityId;
		this.volumeScale = _volumeScale;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.pos = StreamUtils.ReadVector3(_br);
		this.audioClipName = _br.ReadString();
		this.mode = _br.ReadByte();
		this.distance = _br.ReadInt32();
		this.entityId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, this.pos);
		_bw.Write(this.audioClipName);
		_bw.Write(this.mode);
		_bw.Write(this.distance);
		_bw.Write(this.entityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
			_world.gameManager.PlaySoundAtPositionServer(this.pos, this.audioClipName, this.mode, this.distance, this.entityId, this.volumeScale);
			return;
		}
		_world.gameManager.PlaySoundAtPositionClient(this.pos, this.audioClipName, this.mode, this.distance, this.volumeScale);
	}

	public override int GetLength()
	{
		return 40;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 pos;

	[PublicizedFrom(EAccessModifier.Private)]
	public string audioClipName;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioRolloffMode mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public int distance;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public float volumeScale;
}
