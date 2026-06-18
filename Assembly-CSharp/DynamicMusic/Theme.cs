using System;
using System.Collections;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class Theme : SingleClipPlayer, ISection, IPlayable, IFadeable, ICleanable
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator InitializationCoroutine()
		{
			yield return this.<>n__0();
			if (this.src != null)
			{
				this.src.loop = true;
			}
			this.IsReady = true;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator PlayCoroutine()
		{
			yield return new WaitUntil(() => this.IsReady);
			AudioSource src = this.src;
			if (src != null)
			{
				src.Play();
			}
			this.coroutines.Remove(MusicActionType.Play);
			yield break;
		}
	}
}
