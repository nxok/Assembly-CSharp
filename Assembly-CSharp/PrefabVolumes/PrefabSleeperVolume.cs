using System;
using System.Collections.Generic;

namespace PrefabVolumes
{
	public class PrefabSleeperVolume : PrefabVolumeAbs<PrefabSleeperVolume>
	{
		public override void Reset()
		{
			base.Reset();
			this.groupName = "GroupGenericZombie";
			this.isPriority = false;
			this.isQuestExclude = false;
			this.spawnCountMin = 5;
			this.spawnCountMax = 6;
			this.groupId = 0;
			this.flags = 0;
			this.minScript = null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void CopyValues(PrefabVolumeAbs<PrefabSleeperVolume> _target, bool _nonBasicOnly = false)
		{
			if (!_nonBasicOnly)
			{
				base.CopyValues(_target, false);
			}
			PrefabSleeperVolume prefabSleeperVolume = (PrefabSleeperVolume)_target;
			prefabSleeperVolume.groupId = this.groupId;
			prefabSleeperVolume.groupName = this.groupName;
			prefabSleeperVolume.isPriority = this.isPriority;
			prefabSleeperVolume.isQuestExclude = this.isQuestExclude;
			prefabSleeperVolume.spawnCountMin = this.spawnCountMin;
			prefabSleeperVolume.spawnCountMax = this.spawnCountMax;
			prefabSleeperVolume.triggeredByIndices.AddRange(this.triggeredByIndices);
			prefabSleeperVolume.flags = this.flags;
			prefabSleeperVolume.minScript = this.minScript;
		}

		public override PrefabVolumeAbs.EVolumeType VolumeType
		{
			get
			{
				return PrefabVolumeAbs.EVolumeType.Sleeper;
			}
		}

		public override int SerializedSize
		{
			get
			{
				int num = 25;
				string text = this.groupName;
				int num2 = num + (1 + ((text != null) ? new int?(text.Length) : null)).GetValueOrDefault() + 1 + 1 + 2 + 2 + 2 + 4;
				string text2 = this.minScript;
				return num2 + (1 + ((text2 != null) ? new int?(text2.Length) : null)).GetValueOrDefault();
			}
		}

		public override void Read(PooledBinaryReader _br)
		{
			base.Read(_br);
			this.groupName = _br.ReadString();
			this.isPriority = _br.ReadBoolean();
			this.isQuestExclude = _br.ReadBoolean();
			this.spawnCountMin = _br.ReadInt16();
			this.spawnCountMax = _br.ReadInt16();
			this.groupId = _br.ReadInt16();
			this.flags = _br.ReadInt32();
			this.minScript = _br.ReadString();
		}

		public override void Write(PooledBinaryWriter _bw)
		{
			base.Write(_bw);
			_bw.Write(this.groupName);
			_bw.Write(this.isPriority);
			_bw.Write(this.isQuestExclude);
			_bw.Write(this.spawnCountMin);
			_bw.Write(this.spawnCountMax);
			_bw.Write(this.groupId);
			_bw.Write(this.flags);
			_bw.Write(this.minScript ?? "");
		}

		public void SetTrigger(SleeperVolume.ETriggerType _type)
		{
			this.flags = (this.flags & -8) | (int)_type;
		}

		public void SetTriggeredByFlag(byte _index)
		{
			if (!this.triggeredByIndices.Contains(_index))
			{
				this.triggeredByIndices.Add(_index);
			}
		}

		public void ClearTriggeredBy()
		{
			this.triggeredByIndices.Clear();
		}

		public void RemoveTriggeredByFlag(byte _index)
		{
			this.triggeredByIndices.Remove(_index);
		}

		public bool HasTriggeredBy(byte _index)
		{
			return this.triggeredByIndices.Contains(_index);
		}

		public void ToggleTriggeredByFlag(byte layer)
		{
			int num = this.triggeredByIndices.IndexOf(layer);
			if (num >= 0)
			{
				this.triggeredByIndices.RemoveAt(num);
				return;
			}
			this.triggeredByIndices.Add(layer);
		}

		public bool HasAnyTriggeredBy()
		{
			return this.triggeredByIndices.Count > 0;
		}

		public string groupName;

		public bool isPriority;

		public bool isQuestExclude;

		public short spawnCountMin;

		public short spawnCountMax;

		public short groupId;

		public int flags;

		public string minScript;

		public readonly List<byte> triggeredByIndices = new List<byte>();
	}
}
