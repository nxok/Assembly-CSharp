using System;
using System.Collections;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class Bloodmoon : LayeredSection<BloodmoonLayerMixer>
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator PlayCoroutine()
		{
			yield return this.<>n__0();
			yield return new WaitUntil(() => !this.src.isPlaying && !this.IsPaused);
			this.Reset();
			this.Mixer.Unload();
			this.coroutines.Remove(MusicActionType.Play);
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void FillStream(float[] data)
		{
			for (int i = 0; i < data.Length; i++)
			{
				int num = i;
				LayerMixer<BloodmoonConfiguration> mixer = this.Mixer;
				int cursor = this.cursor;
				this.cursor = cursor + 1;
				data[num] = mixer[cursor];
				this.cursor %= Content.SamplesFor[base.Sect] * 2;
			}
		}
	}
}
