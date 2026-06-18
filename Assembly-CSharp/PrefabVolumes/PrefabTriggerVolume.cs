using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace PrefabVolumes
{
	[Preserve]
	public class PrefabTriggerVolume : PrefabVolumeAbs<PrefabTriggerVolume>
	{
		public override PrefabVolumeAbs.EVolumeType VolumeType
		{
			get
			{
				return PrefabVolumeAbs.EVolumeType.Trigger;
			}
		}

		public override int SerializedSize
		{
			get
			{
				return 26 + this.TriggersIndices.Count;
			}
		}

		public override void Reset()
		{
			base.Reset();
			this.TriggersIndices.Clear();
		}

		public override void Read(PooledBinaryReader _br)
		{
			base.Read(_br);
			this.TriggersIndices.Clear();
			int num = (int)_br.ReadByte();
			for (int i = 0; i < num; i++)
			{
				this.TriggersIndices.Add(_br.ReadByte());
			}
		}

		public override void Write(PooledBinaryWriter _bw)
		{
			base.Write(_bw);
			_bw.Write((byte)this.TriggersIndices.Count);
			for (int i = 0; i < this.TriggersIndices.Count; i++)
			{
				_bw.Write(this.TriggersIndices[i]);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void CopyValues(PrefabVolumeAbs<PrefabTriggerVolume> _target, bool _nonBasicOnly = false)
		{
			if (!_nonBasicOnly)
			{
				base.CopyValues(_target, false);
			}
			((PrefabTriggerVolume)_target).TriggersIndices.AddRange(this.TriggersIndices);
		}

		public void SetTriggersFlag(byte _index)
		{
			if (!this.TriggersIndices.Contains(_index))
			{
				this.TriggersIndices.Add(_index);
			}
		}

		public void RemoveTriggersFlag(byte _index)
		{
			this.TriggersIndices.Remove(_index);
		}

		public void RemoveAllTriggersFlags()
		{
			this.TriggersIndices.Clear();
		}

		public bool HasTriggers(byte _index)
		{
			return this.TriggersIndices.Contains(_index);
		}

		public void ToggleTriggersFlag(byte layer)
		{
			int num = this.TriggersIndices.IndexOf(layer);
			if (num >= 0)
			{
				this.TriggersIndices.RemoveAt(num);
				return;
			}
			this.TriggersIndices.Add(layer);
		}

		public bool HasAnyTriggers()
		{
			return this.TriggersIndices.Count > 0;
		}

		public readonly List<byte> TriggersIndices = new List<byte>();
	}
}
