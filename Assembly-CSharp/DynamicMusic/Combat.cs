using System;
using System.Collections;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class Combat : LayeredSection<CombatLayerMixer>
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
	}
}
