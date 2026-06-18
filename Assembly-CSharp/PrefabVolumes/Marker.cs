using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PrefabVolumes
{
	public class Marker : PrefabVolumeAbs<Marker>
	{
		public bool PartDirty
		{
			get
			{
				return this.partDirty;
			}
		}

		public string GroupName
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.groupName;
			}
			set
			{
				if (this.groupName != value)
				{
					this.color = default(Color);
					this.groupId = -1;
					this.groupName = value;
				}
			}
		}

		public Color GroupColor
		{
			get
			{
				if (this.color == default(Color))
				{
					GameRandom tempGameRandom = GameRandomManager.Instance.GetTempGameRandom(this.GroupId);
					this.color = new Color32((byte)tempGameRandom.RandomRange(0, 256), (byte)tempGameRandom.RandomRange(0, 256), (byte)tempGameRandom.RandomRange(0, 256), (this.MarkerType == Marker.MarkerTypes.PartSpawn) ? 32 : 128);
				}
				return this.color;
			}
		}

		public int GroupId
		{
			get
			{
				if (this.groupId == -1)
				{
					this.groupId = this.GroupName.GetHashCode();
				}
				return this.groupId;
			}
		}

		public override Vector3i startPos
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

		public override Vector3i size
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.sizeInternal;
			}
			set
			{
				if (this.sizeInternal != value)
				{
					this.sizeInternal = value;
					this.partDirty = true;
				}
			}
		}

		public override PrefabVolumeAbs.EVolumeType VolumeType
		{
			get
			{
				return PrefabVolumeAbs.EVolumeType.Marker;
			}
		}

		public override int SerializedSize
		{
			get
			{
				int num = 26;
				string text = this.GroupName;
				int num2 = num + (1 + ((text != null) ? new int?(text.Length) : null)).GetValueOrDefault() + 100;
				string text2 = this.PartToSpawn;
				return num2 + (1 + ((text2 != null) ? new int?(text2.Length) : null)).GetValueOrDefault() + 1 + 4;
			}
		}

		public Marker.MarkerTypes MarkerType
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.markerType;
			}
			set
			{
				if (this.markerType != value)
				{
					this.markerType = value;
					this.partDirty = true;
				}
			}
		}

		public FastTags<TagGroup.Poi> Tags
		{
			get
			{
				return this.tags;
			}
			set
			{
				this.tags = value;
			}
		}

		public string PartToSpawn
		{
			get
			{
				return this.partToSpawn;
			}
			set
			{
				if (this.partToSpawn != value)
				{
					this.partToSpawn = value;
					this.partDirty = true;
				}
			}
		}

		public byte Rotations
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.rotations;
			}
			set
			{
				if (this.rotations != value)
				{
					this.rotations = value;
					this.partDirty = true;
				}
			}
		}

		public float PartChanceToSpawn
		{
			get
			{
				return this.partChanceToSpawn;
			}
			set
			{
				if (!Mathf.Approximately((float)this.rotations, value))
				{
					this.partChanceToSpawn = value;
					this.partDirty = true;
				}
			}
		}

		public override void Reset()
		{
			base.Reset();
			this.markerType = Marker.MarkerTypes.None;
			this.tags = FastTags<TagGroup.Poi>.none;
			this.groupId = -1;
			this.groupName = "new";
			this.color = default(Color);
			this.partToSpawn = null;
			this.rotations = 0;
			this.partChanceToSpawn = 0f;
			this.partDirty = false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void CopyValues(PrefabVolumeAbs<Marker> _target, bool _nonBasicOnly = false)
		{
			if (!_nonBasicOnly)
			{
				base.CopyValues(_target, false);
			}
			Marker marker = (Marker)_target;
			marker.MarkerType = this.MarkerType;
			marker.GroupName = this.GroupName;
			marker.Tags = this.Tags;
			marker.PartToSpawn = this.PartToSpawn;
			marker.Rotations = this.Rotations;
			marker.PartChanceToSpawn = this.PartChanceToSpawn;
		}

		public override void Read(PooledBinaryReader _br)
		{
			base.Read(_br);
			this.MarkerType = (Marker.MarkerTypes)_br.ReadByte();
			this.GroupName = StreamUtils.ReadString(_br);
			this.Tags = FastTags<TagGroup.Poi>.Parse(_br.ReadString());
			this.PartToSpawn = StreamUtils.ReadString(_br);
			this.Rotations = _br.ReadByte();
			this.PartChanceToSpawn = _br.ReadSingle();
		}

		public override void Write(PooledBinaryWriter _bw)
		{
			base.Write(_bw);
			_bw.Write((byte)this.MarkerType);
			StreamUtils.Write(_bw, this.GroupName);
			_bw.Write(this.Tags.ToString());
			StreamUtils.Write(_bw, this.PartToSpawn);
			_bw.Write(this.Rotations);
			_bw.Write(this.PartChanceToSpawn);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Marker.MarkerTypes markerType;

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Poi> tags;

		[PublicizedFrom(EAccessModifier.Private)]
		public int groupId = -1;

		[PublicizedFrom(EAccessModifier.Private)]
		public string groupName;

		[PublicizedFrom(EAccessModifier.Private)]
		public Color color;

		[PublicizedFrom(EAccessModifier.Private)]
		public string partToSpawn;

		[PublicizedFrom(EAccessModifier.Private)]
		public byte rotations;

		[PublicizedFrom(EAccessModifier.Private)]
		public float partChanceToSpawn = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool partDirty = true;

		public static readonly List<Vector3i> MarkerSizes = new List<Vector3i>
		{
			Vector3i.one,
			new Vector3i(25, 0, 25),
			new Vector3i(42, 0, 42),
			new Vector3i(60, 0, 60),
			new Vector3i(100, 0, 100)
		};

		public enum MarkerTypes : byte
		{
			None,
			POISpawn,
			RoadExit,
			PartSpawn
		}

		public enum MarkerSize : byte
		{
			One,
			ExtraSmall,
			Small,
			Medium,
			Large,
			Custom
		}
	}
}
