using System;
using System.Collections;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class Adventure : LayeredSection<FixedLayerMixer>
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator PlayCoroutine()
		{
			yield return this.<>n__0();
			yield return new WaitUntil(() => this.Mixer.IsFinished || (this.src && !this.src.isPlaying && !this.IsPaused));
			Log.Out(string.Format("Mixer IsFinished: {0}\n AudioSource is not playing: {1}\n IsPaused: {2}\n IsPlaying: {3}", new object[]
			{
				this.Mixer.IsFinished,
				this.src && !this.src.isPlaying,
				this.IsPaused,
				this.IsPlaying
			}));
			if (this.src)
			{
				this.src.loop = false;
				if (this.IsPlaying)
				{
					this.Stop();
				}
			}
			this.IsDone = true;
			this.Reset();
			this.Mixer.Unload();
			this.coroutines.Remove(MusicActionType.Play);
			yield break;
		}
	}
}
