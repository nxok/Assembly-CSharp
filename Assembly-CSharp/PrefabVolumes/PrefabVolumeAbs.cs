using System;
using System.Runtime.CompilerServices;

namespace PrefabVolumes
{
	public abstract class PrefabVolumeAbs
	{
		public bool Used
		{
			get
			{
				return this.used;
			}
		}

		public virtual Vector3i startPos
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.startPosInternal;
			}
			set
			{
				this.startPosInternal = value;
			}
		}

		public virtual Vector3i size
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.sizeInternal;
			}
			set
			{
				this.sizeInternal = value;
			}
		}

		public abstract PrefabVolumeAbs.EVolumeType VolumeType { get; }

		public abstract int SerializedSize { get; }

		public void Use(Vector3i _startPos, Vector3i _size)
		{
			this.used = true;
			this.startPos = _startPos;
			this.size = _size;
		}

		public virtual void Reset()
		{
			this.used = false;
			this.startPos = default(Vector3i);
			this.size = default(Vector3i);
		}

		public void MarkUnused()
		{
			this.used = false;
		}

		public virtual void Read(PooledBinaryReader _br)
		{
			this.used = _br.ReadBoolean();
			this.startPos = StreamUtils.ReadVector3i(_br);
			this.size = StreamUtils.ReadVector3i(_br);
		}

		public virtual void Write(PooledBinaryWriter _bw)
		{
			_bw.Write(this.Used);
			StreamUtils.Write(_bw, this.startPos);
			StreamUtils.Write(_bw, this.size);
		}

		public virtual void RotateY(bool _bLeft, Vector3i _prefabSize)
		{
			Vector3i size = this.size;
			Vector3i startPos = this.startPos;
			Vector3i vector3i = startPos + size;
			if (_bLeft)
			{
				startPos = new Vector3i(_prefabSize.z - startPos.z, startPos.y, startPos.x);
				vector3i = new Vector3i(_prefabSize.z - vector3i.z, vector3i.y, vector3i.x);
			}
			else
			{
				startPos = new Vector3i(startPos.z, startPos.y, _prefabSize.x - startPos.x);
				vector3i = new Vector3i(vector3i.z, vector3i.y, _prefabSize.x - vector3i.x);
			}
			if (startPos.x > vector3i.x)
			{
				MathUtils.Swap(ref startPos.x, ref vector3i.x);
			}
			if (startPos.z > vector3i.z)
			{
				MathUtils.Swap(ref startPos.z, ref vector3i.z);
			}
			this.startPos = startPos;
			MathUtils.Swap(ref size.x, ref size.z);
			this.size = size;
		}

		public void Move(Vector3i _moveVector)
		{
			this.startPos += _moveVector;
		}

		public void Resize(int _dTop, int _dBottom, int _dNorth, int _dSouth, int _dEast, int _dWest, bool _allowXBelow1 = false, bool _allowYBelow1 = false, bool _allowZBelow1 = false)
		{
			Vector3i vector3i = this.size;
			vector3i += new Vector3i(_dEast + _dWest, _dTop + _dBottom, _dNorth + _dSouth);
			this.startPos += new Vector3i(-_dWest, -_dBottom, -_dSouth);
			if (!_allowXBelow1 && vector3i.x < 2)
			{
				vector3i = new Vector3i(1, vector3i.y, vector3i.z);
			}
			if (!_allowYBelow1 && vector3i.y < 2)
			{
				vector3i = new Vector3i(vector3i.x, 1, vector3i.z);
			}
			if (!_allowZBelow1 && vector3i.z < 2)
			{
				vector3i = new Vector3i(vector3i.x, vector3i.y, 1);
			}
			this.size = vector3i;
		}

		public abstract PrefabVolumeAbs Clone();

		public abstract void CopyValues(PrefabVolumeAbs _target, bool _nonBasicOnly = false);

		public static PrefabVolumeAbs CreateByType(PrefabVolumeAbs.EVolumeType _volumeType)
		{
			PrefabVolumeAbs prefabVolumeAbs;
			switch (_volumeType)
			{
			case PrefabVolumeAbs.EVolumeType.Sleeper:
				prefabVolumeAbs = new PrefabSleeperVolume();
				break;
			case PrefabVolumeAbs.EVolumeType.Teleport:
				prefabVolumeAbs = new PrefabTeleportVolume();
				break;
			case PrefabVolumeAbs.EVolumeType.Info:
				prefabVolumeAbs = new PrefabInfoVolume();
				break;
			case PrefabVolumeAbs.EVolumeType.Wall:
				prefabVolumeAbs = new PrefabWallVolume();
				break;
			case PrefabVolumeAbs.EVolumeType.Trigger:
				prefabVolumeAbs = new PrefabTriggerVolume();
				break;
			case PrefabVolumeAbs.EVolumeType.Marker:
				prefabVolumeAbs = new Marker();
				break;
			default:
				throw new ArgumentOutOfRangeException("_volumeType", string.Format("Invalid volumeType {0}", _volumeType));
			}
			return prefabVolumeAbs;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public PrefabVolumeAbs()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool used;

		[PublicizedFrom(EAccessModifier.Protected)]
		public Vector3i startPosInternal;

		[PublicizedFrom(EAccessModifier.Protected)]
		public Vector3i sizeInternal;

		public enum EVolumeType : byte
		{
			Sleeper,
			Teleport,
			Info,
			Wall,
			Trigger,
			Marker
		}
	}
}
