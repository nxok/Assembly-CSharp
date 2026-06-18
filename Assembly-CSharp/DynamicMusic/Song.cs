using System;
using System.Collections;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class Song : SingleClipPlayer, ISection, IPlayable, IFadeable, ICleanable
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator PlayCoroutine()
		{
			yield return new WaitUntil(() => this.IsReady);
			AudioSource src = this.src;
			if (src != null)
			{
				src.Play();
			}
			yield return new WaitUntil(() => !this.src.isPlaying && !this.IsPaused);
			if (this.IsPlaying)
			{
				this.Stop();
			}
			this.IsDone = true;
			this.coroutines.Remove(MusicActionType.Play);
			yield break;
		}
	}
}
