using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Audio
{
	[Preserve]
	public class NetPackageAudio : NetPackageEntityTargeted
	{
		public NetPackageAudio Setup(int _playOnEntityId, string _soundGroupName, float _occlusion, bool _play, bool _signalOnly = false, float _volumeScale = 1f)
		{
			base.Setup(_playOnEntityId);
			this.playOnEntity = true;
			this.soundGroupName = _soundGroupName;
			this.play = _play;
			this.occlusion = _occlusion;
			this.signalOnly = _signalOnly;
			this.volumeScale = _volumeScale;
			return this;
		}

		public NetPackageAudio Setup(Vector3 _position, string _soundGroupName, float _occlusion, bool _play, int _entityId = -1, float _volumeScale = 1f)
		{
			base.Setup(_entityId);
			this.playOnEntity = false;
			this.position = _position;
			this.soundGroupName = _soundGroupName;
			this.play = _play;
			this.occlusion = _occlusion;
			this.volumeScale = _volumeScale;
			return this;
		}

		public override void read(PooledBinaryReader _reader)
		{
			base.read(_reader);
			this.soundGroupName = _reader.ReadString();
			this.play = _reader.ReadBoolean();
			float num = _reader.ReadSingle();
			float num2 = _reader.ReadSingle();
			float num3 = _reader.ReadSingle();
			this.position.x = num;
			this.position.y = num2;
			this.position.z = num3;
			this.playOnEntity = _reader.ReadBoolean();
			this.occlusion = _reader.ReadSingle();
			this.volumeScale = _reader.ReadSingle();
			this.signalOnly = _reader.ReadBoolean();
		}

		public override void write(PooledBinaryWriter _writer)
		{
			base.write(_writer);
			_writer.Write((this.soundGroupName != null) ? this.soundGroupName : "");
			_writer.Write(this.play);
			_writer.Write(this.position.x);
			_writer.Write(this.position.y);
			_writer.Write(this.position.z);
			_writer.Write(this.playOnEntity);
			_writer.Write(this.occlusion);
			_writer.Write(this.volumeScale);
			_writer.Write(this.signalOnly);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (_world == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(this.soundGroupName))
			{
				return;
			}
			if (this.playOnEntity && this.entityId >= 0)
			{
				Entity entity = _world.GetEntity(this.entityId);
				if (entity == null)
				{
					return;
				}
				if (GameManager.IsDedicatedServer && Manager.ServerAudio != null)
				{
					if (this.play)
					{
						Manager.ServerAudio.Play(entity, this.soundGroupName, this.occlusion, this.signalOnly, this.volumeScale);
						return;
					}
					Manager.ServerAudio.Stop(this.entityId, this.soundGroupName);
					return;
				}
				else if (!GameManager.IsDedicatedServer && Manager.ServerAudio != null)
				{
					if (this.play)
					{
						Manager.Play(entity, this.soundGroupName, this.volumeScale, false);
						Manager.ServerAudio.Play(entity, this.soundGroupName, this.occlusion, this.signalOnly, this.volumeScale);
						return;
					}
					Manager.Stop(this.entityId, this.soundGroupName);
					Manager.ServerAudio.Stop(this.entityId, this.soundGroupName);
					return;
				}
				else if (Manager.ServerAudio == null)
				{
					if (!this.play)
					{
						Manager.Stop(this.entityId, this.soundGroupName);
						return;
					}
					if (!this.signalOnly)
					{
						Manager.Play(entity, this.soundGroupName, this.volumeScale, false);
						return;
					}
				}
			}
			else if (GameManager.IsDedicatedServer && Manager.ServerAudio != null)
			{
				if (this.play)
				{
					Manager.ServerAudio.Play(this.position, this.soundGroupName, this.occlusion, this.entityId, this.volumeScale);
					return;
				}
				Manager.ServerAudio.Stop(this.position, this.soundGroupName);
				return;
			}
			else if (!GameManager.IsDedicatedServer && Manager.ServerAudio != null)
			{
				if (this.play)
				{
					Manager.Play(this.position, this.soundGroupName, this.entityId, false, this.volumeScale);
					Manager.ServerAudio.Play(this.position, this.soundGroupName, this.occlusion, this.entityId, this.volumeScale);
					return;
				}
				Manager.Stop(this.position, this.soundGroupName);
				Manager.ServerAudio.Stop(this.position, this.soundGroupName);
				return;
			}
			else if (Manager.ServerAudio == null)
			{
				if (this.play)
				{
					Manager.Play(this.position, this.soundGroupName, this.entityId, false, this.volumeScale);
					return;
				}
				Manager.Stop(this.position, this.soundGroupName);
			}
		}

		public override int GetLength()
		{
			return 10;
		}

		public string soundGroupName;

		public bool play;

		public Vector3 position;

		public bool playOnEntity;

		public float occlusion;

		public bool signalOnly;

		public float volumeScale;
	}
}
