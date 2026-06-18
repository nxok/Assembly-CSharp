using System;
using UnityEngine;

namespace Audio
{
	public class Handle
	{
		public Handle(string soundGroupName, AudioSource near, AudioSource far)
		{
			this.name = soundGroupName;
			this.nearSource = near;
			this.farSource = far;
			if (this.nearSource)
			{
				this.basePitch = this.nearSource.pitch;
				this.baseVolume = this.nearSource.volume;
			}
		}

		public void SetPitch(float pitch)
		{
			if (this.nearSource)
			{
				this.nearSource.pitch = pitch + this.basePitch;
			}
			if (this.farSource)
			{
				this.farSource.pitch = pitch + this.basePitch;
			}
		}

		public void SetVolume(float volume)
		{
			if (this.nearSource)
			{
				this.nearSource.volume = volume * this.baseVolume;
			}
			if (this.farSource)
			{
				this.farSource.volume = volume * this.baseVolume;
			}
		}

		public void Stop(int entityId)
		{
			Manager.Stop(entityId, this.name);
		}

		public float ClipLength()
		{
			if (this.nearSource)
			{
				return this.nearSource.clip.length;
			}
			if (this.farSource)
			{
				return this.farSource.clip.length;
			}
			return 0f;
		}

		public bool IsPlaying()
		{
			bool flag = false;
			if (this.nearSource != null)
			{
				flag = this.nearSource.isPlaying;
			}
			if (this.farSource != null)
			{
				flag |= this.farSource.isPlaying;
			}
			return flag;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string name;

		[PublicizedFrom(EAccessModifier.Private)]
		public AudioSource nearSource;

		[PublicizedFrom(EAccessModifier.Private)]
		public AudioSource farSource;

		[PublicizedFrom(EAccessModifier.Private)]
		public float basePitch;

		[PublicizedFrom(EAccessModifier.Private)]
		public float baseVolume;
	}
}
