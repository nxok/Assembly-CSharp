using System;
using System.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public abstract class LayeredSection<T> : Section, ISection, IPlayable, IFadeable, ICleanable where T : ILayerMixer, new()
	{
		public LayeredSection()
		{
			this.Mixer = new T();
		}

		public override void Init()
		{
			base.Init();
			this.Reset();
			this.IsReady = (base.IsInitialized = true);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void Reset()
		{
			this.cursor = 0;
			this.IsReady = (this.IsDone = false);
			if (this.src)
			{
				this.src.loop = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator PlayCoroutine()
		{
			yield return this.LoadContentCoroutine();
			AudioSource src = this.src;
			if (src != null)
			{
				src.Play();
			}
			yield break;
		}

		public override IEnumerator PreloadRoutine()
		{
			Log.Out("Preloading LayeredSection {0} : Type: {1}", new object[]
			{
				base.Sect.ToString(),
				base.GetType().ToString()
			});
			yield return this.LoadContentCoroutine();
			this.preloaded = true;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public IEnumerator LoadContentCoroutine()
		{
			this.IsReady = false;
			this.Mixer.Sect = base.Sect;
			if (!this.preloaded)
			{
				yield return this.Mixer.Load();
			}
			else
			{
				this.preloaded = false;
				yield return null;
			}
			if (!this.src)
			{
				using (LayeredSection<T>.s_LoadContentMarker.Auto())
				{
					AudioSource component = DataLoader.LoadAsset<GameObject>(Content.SourcePathFor[base.Sect], false).GetComponent<AudioSource>();
					this.src = Object.Instantiate<AudioSource>(component);
					this.src.transform.SetParent(Section.parent.transform);
					this.src.name = base.Sect.ToString();
					this.src.loop = true;
					this.src.priority = 0;
					this.src.clip = AudioClip.Create(base.Sect.ToString(), Content.SamplesFor[base.Sect], 2, 44100, true, new AudioClip.PCMReaderCallback(this.FillStream));
				}
			}
			this.IsReady = (base.IsInitialized = true);
			this.LoadRoutine = null;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void FillStream(float[] data)
		{
			using (LayeredSection<T>.s_FillStreamMarker.Auto())
			{
				for (int i = 0; i < data.Length; i++)
				{
					int num = i;
					int num2 = this.cursor;
					this.cursor = num2 + 1;
					data[num] = this.Mixer[num2];
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ProfilerMarker s_FillStreamMarker = new ProfilerMarker("DynamicMusic.LayeredSection.FillStream");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ProfilerMarker s_LoadContentMarker = new ProfilerMarker("DynamicMusic.LayeredSection.LoadContentCoroutine");

		[PublicizedFrom(EAccessModifier.Protected)]
		public T Mixer;

		public int cursor;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool preloaded;
	}
}
