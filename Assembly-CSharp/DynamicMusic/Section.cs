using System;
using System.Collections;
using System.Text;
using MusicUtils.Enums;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public abstract class Section : ContentPlayer, ISection, IPlayable, IFadeable, ICleanable
	{
		public SectionType Sect { get; set; }

		public bool IsInitialized
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public override float Volume
		{
			get
			{
				if (!this.src)
				{
					return 0f;
				}
				return this.src.volume;
			}
			set
			{
				if (this.src)
				{
					this.src.volume = value;
				}
			}
		}

		public override void Init()
		{
			this.coroutines = new EnumDictionary<MusicActionType, Coroutine>();
		}

		public override void Play()
		{
			if (!this.coroutines.ContainsKey(MusicActionType.Play))
			{
				base.Play();
				this.coroutines.Add(MusicActionType.Play, GameManager.Instance.StartCoroutine(this.PlayCoroutine()));
				Log.Out(string.Format("Played {0}", this.Sect));
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Format("Attempted to play {0}, while play was running", this.Sect));
			stringBuilder.AppendLine("Currently running coroutines: ");
			foreach (MusicActionType musicActionType in this.coroutines.Keys)
			{
				stringBuilder.AppendLine(musicActionType.ToString());
			}
			Log.Warning(stringBuilder.ToString());
		}

		public override void Pause()
		{
			base.Pause();
			AudioSource audioSource = this.src;
			if (audioSource != null)
			{
				audioSource.Pause();
			}
			Log.Out(string.Format("Paused {0}", this.Sect));
		}

		public override void UnPause()
		{
			base.UnPause();
			AudioSource audioSource = this.src;
			if (audioSource != null)
			{
				audioSource.UnPause();
			}
			Log.Out(string.Format("Unpaused {0}", this.Sect));
		}

		public override void Stop()
		{
			base.Stop();
			AudioSource audioSource = this.src;
			if (audioSource != null)
			{
				audioSource.Stop();
			}
			Log.Out(string.Format("Stopped {0}", this.Sect));
		}

		public virtual void FadeIn()
		{
			Coroutine coroutine;
			if (this.coroutines.TryGetValue(MusicActionType.FadeOut, out coroutine))
			{
				GameManager.Instance.StopCoroutine(coroutine);
				this.coroutines.Remove(MusicActionType.FadeOut);
			}
			if (this.IsPaused || this.coroutines.ContainsKey(MusicActionType.Play))
			{
				this.UnPause();
			}
			else
			{
				this.Play();
			}
			Log.Out(string.Format("Fading in {0}", this.Sect));
			Coroutine coroutine2;
			if (this.coroutines.TryGetValue(MusicActionType.FadeIn, out coroutine2))
			{
				GameManager.Instance.StopCoroutine(coroutine2);
				this.coroutines.Remove(MusicActionType.FadeIn);
			}
			this.coroutines.Add(MusicActionType.FadeIn, GameManager.Instance.StartCoroutine(this.FadeInCoroutine()));
		}

		public virtual void FadeOut()
		{
			Coroutine coroutine;
			if (this.coroutines.TryGetValue(MusicActionType.FadeIn, out coroutine))
			{
				GameManager.Instance.StopCoroutine(coroutine);
				this.coroutines.Remove(MusicActionType.FadeIn);
			}
			Log.Out(string.Format("Fading out {0}", this.Sect));
			Coroutine coroutine2;
			if (this.coroutines.TryGetValue(MusicActionType.FadeOut, out coroutine2))
			{
				GameManager.Instance.StopCoroutine(coroutine2);
				this.coroutines.Remove(MusicActionType.FadeOut);
			}
			this.coroutines.Add(MusicActionType.FadeOut, GameManager.Instance.StartCoroutine(this.FadeOutCoroutine()));
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual IEnumerator FadeInCoroutine()
		{
			double dspTime = AudioSettings.dspTime;
			double endTime = AudioSettings.dspTime + 3.0;
			double num = endTime - AudioSettings.dspTime;
			double perc = 1.0 - num / 3.0;
			float startVol = this.Volume;
			while (perc <= 1.0)
			{
				this.Volume = Mathf.Lerp(startVol, 1f, (float)perc);
				num = endTime - AudioSettings.dspTime;
				perc = 1.0 - num / 3.0;
				yield return null;
			}
			this.Volume = 1f;
			this.coroutines.Remove(MusicActionType.FadeIn);
			Log.Out(string.Format("fadeInCo complete on {0}", this.Sect));
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual IEnumerator FadeOutCoroutine()
		{
			double dspTime = AudioSettings.dspTime;
			double endTime = AudioSettings.dspTime + 3.0;
			double num = endTime - AudioSettings.dspTime;
			double perc = 1.0 - num / 3.0;
			float startVol = this.Volume;
			while (perc <= 1.0)
			{
				this.Volume = Mathf.Lerp(startVol, 0f, (float)perc);
				num = endTime - AudioSettings.dspTime;
				perc = 1.0 - num / 3.0;
				yield return null;
			}
			this.Volume = 0f;
			this.Pause();
			double timerStart = AudioSettings.dspTime;
			yield return new WaitUntil(() => AudioSettings.dspTime - timerStart >= 60.0 || !this.IsPaused);
			if (this.IsPaused)
			{
				this.Stop();
			}
			else
			{
				Log.Out(string.Format("{0} was resumed. FadeOut coroutine has been exited.", this.Sect));
			}
			this.coroutines.Remove(MusicActionType.FadeOut);
			Log.Out(string.Format("fadeOutCo complete on {0}", this.Sect));
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract IEnumerator PlayCoroutine();

		public virtual IEnumerator PreloadRoutine()
		{
			yield return null;
			yield break;
		}

		public virtual void CleanUp()
		{
			AudioSource audioSource = this.src;
			if (audioSource != null)
			{
				audioSource.Stop();
			}
			foreach (Coroutine coroutine in this.coroutines.Values)
			{
				GameManager.Instance.StopCoroutine(coroutine);
			}
			if (this.LoadRoutine != null)
			{
				GameManager.Instance.StopCoroutine(this.LoadRoutine);
				this.LoadRoutine = null;
			}
			this.coroutines.Clear();
			this.coroutines = null;
			if (this.src)
			{
				Object.Destroy(this.src.gameObject);
				this.src = null;
			}
			if (Section.parent != null)
			{
				Section.parent.transform.DetachChildren();
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public Section()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public static GameObject parent = new GameObject("Music");

		[PublicizedFrom(EAccessModifier.Protected)]
		public static GameRandom rng = GameRandomManager.Instance.CreateGameRandom();

		[PublicizedFrom(EAccessModifier.Protected)]
		public AudioSource src;

		[PublicizedFrom(EAccessModifier.Protected)]
		public EnumDictionary<MusicActionType, Coroutine> coroutines;

		[PublicizedFrom(EAccessModifier.Protected)]
		public Coroutine LoadRoutine;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float fadeTime = 3f;
	}
}
