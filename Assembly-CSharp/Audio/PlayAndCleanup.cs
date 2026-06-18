using System;
using System.Collections;
using UnityEngine;

namespace Audio
{
	public class PlayAndCleanup
	{
		public PlayAndCleanup(LoopingPair _lp, string _soundGroupName)
		{
			this.lp = _lp;
			double num;
			this.lp.sgoBegin.src.PlayScheduled(num = AudioSettings.dspTime + 0.05);
			this.lp.sgoLoop.src.PlayScheduled(num + (double)this.lp.sgoBegin.src.clip.samples / 44100.0);
			Manager.AddPlayingAudioSource(this.lp.sgoBegin.src);
			Manager.AddPlayingAudioSource(this.lp.sgoLoop.src);
			if (!string.IsNullOrEmpty(_soundGroupName))
			{
				Manager.AddPlayAndCleanupAudioSource(_soundGroupName, this.lp.sgoBegin.src);
				Manager.AddPlayAndCleanupAudioSource(_soundGroupName, this.lp.sgoLoop.src);
			}
			GameManager.Instance.StartCoroutine(this.StopBeginWhenDone(this.lp.sgoBegin.src.clip.length, _soundGroupName));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator StopBeginWhenDone(float waitTime, string soundGroupName)
		{
			yield return new WaitForSeconds(waitTime + 0.1f);
			if (GameManager.Instance.IsPaused())
			{
				yield return new WaitForSeconds(0.1f);
			}
			if (this.lp.sgoBegin.src == null)
			{
				yield break;
			}
			if (this.lp.sgoBegin.src.isPlaying)
			{
				yield return new WaitForSeconds(0.1f);
			}
			if (this.lp.sgoBegin.src == null)
			{
				yield break;
			}
			Manager.RemovePlayingAudioSource(this.lp.sgoBegin.src);
			if (!string.IsNullOrEmpty(soundGroupName))
			{
				Manager.RemovePlayAndCleanupAudioSource(soundGroupName, this.lp.sgoBegin.src);
			}
			if (this.lp.sgoBegin.go)
			{
				Object.Destroy(this.lp.sgoBegin.go);
			}
			yield break;
		}

		public PlayAndCleanup(GameObject _go, AudioSource _source, float _occlusion = 0f, float delay = 0f, bool isLooping = false, bool hasLoopingAnalog = false, string soundGroupName = "")
		{
			this.go = _go;
			this.src = _source;
			float num = 1f - _occlusion;
			float num2 = Utils.FastAbs(Manager.currentListenerPosition.y - this.go.transform.position.y);
			num2 = Utils.FastClamp01(num2 / 30f);
			this.src.volume *= (1f - num2) * num;
			if (num < 0.95f)
			{
				this.go.AddComponent<AudioLowPassFilter>().cutoffFrequency = Utils.FastLerp(10f, 5000f, Mathf.Pow(num, 2f));
			}
			if (delay > 0f)
			{
				this.src.PlayDelayed(delay);
			}
			else
			{
				Manager.PlaySource(this.src);
			}
			Manager.AddPlayingAudioSource(this.src);
			if (!string.IsNullOrEmpty(soundGroupName))
			{
				Manager.AddPlayAndCleanupAudioSource(soundGroupName, this.src);
			}
			if (!isLooping)
			{
				float num3 = _source.clip.length * (1f + Utils.FastClamp01(1f - _source.pitch)) + delay;
				GameManager.Instance.StartCoroutine(this.StopWhenDone(num3, soundGroupName));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator StopWhenDone(float waitTime, string soundGroupName)
		{
			yield return new WaitForSeconds(waitTime + 0.001f);
			if (GameManager.Instance.IsPaused())
			{
				yield return new WaitForSeconds(0.1f);
			}
			Manager.RemovePlayingAudioSource(this.src);
			if (!string.IsNullOrEmpty(soundGroupName))
			{
				Manager.RemovePlayAndCleanupAudioSource(soundGroupName, this.src);
			}
			if (this.go)
			{
				Object.Destroy(this.go);
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public GameObject go;

		[PublicizedFrom(EAccessModifier.Private)]
		public AudioSource src;

		[PublicizedFrom(EAccessModifier.Private)]
		public LoopingPair lp;
	}
}
