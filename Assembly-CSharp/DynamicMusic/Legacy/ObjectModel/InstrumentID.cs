using System;
using System.Collections.Generic;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic.Legacy.ObjectModel
{
	public class InstrumentID : Dictionary<PlacementType, string>
	{
		public event InstrumentID.LoadFinishedAction OnLoadFinished;

		public bool IsLoaded
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public InstrumentID()
			: base(3)
		{
			this.IsLoaded = false;
			this.ClipData = new EnumDictionary<PlacementType, float[]>(3);
			this.Clips = new EnumDictionary<PlacementType, AudioClip>(3);
		}

		public void Load()
		{
			if (!this.IsLoaded)
			{
				if (this.thisEnumerator == null)
				{
					this.thisEnumerator = this.LoadClip();
				}
				this.thisEnumerator.MoveNext();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator<bool> LoadClip()
		{
			if (!this.IsLoaded)
			{
				foreach (KeyValuePair<PlacementType, string> kvp in this)
				{
					LoadManager.AssetRequestTask<AudioClip> requestTask = LoadManager.LoadAsset<AudioClip>(kvp.Value, null, null, false, false, false);
					while (!requestTask.IsDone)
					{
						yield return false;
					}
					AudioClip clip = requestTask.Asset;
					if (clip != null)
					{
						if (clip.loadState == null)
						{
							clip.LoadAudioData();
							while (clip.loadState != 2)
							{
								yield return false;
								if (clip.loadState == 3)
								{
									Log.Warning(string.Format("clip load failed in {0}", this.Name));
									break;
								}
							}
						}
						if (!this.hasGrabbedClipProperties)
						{
							this.Frames = clip.samples;
							this.Channels = clip.channels;
							this.Frequency = clip.frequency;
							this.Samples = this.Frames * this.Channels;
							this.hasGrabbedClipProperties = true;
						}
						else if (this.Frames != clip.samples || this.Channels != clip.channels || this.Frequency != clip.frequency)
						{
							Log.Warning(string.Format("Inconsistent clip properties for clips in {0}", this.Name));
						}
						int samplesPerPass = 44100;
						int samplesGrabbed = 0;
						int samplesToGrab = Utils.FastMin(samplesPerPass, this.Samples - samplesGrabbed);
						float[] sampleData = MemoryPools.poolFloat.Alloc(this.Samples);
						float[] buffer = MemoryPools.poolFloat.Alloc(samplesPerPass);
						while (samplesGrabbed < this.Samples)
						{
							if (this.Samples - samplesGrabbed < samplesPerPass)
							{
								buffer = new float[this.Samples - samplesGrabbed];
							}
							clip.GetData(buffer, samplesGrabbed / 2);
							buffer.CopyTo(sampleData, samplesGrabbed);
							samplesGrabbed += samplesToGrab;
							yield return false;
						}
						MemoryPools.poolFloat.Free(buffer);
						this.ClipData.Add(kvp.Key, sampleData);
						yield return false;
						clip.UnloadAudioData();
						this.isClipLoaded = false;
						sampleData = null;
						buffer = null;
					}
					else
					{
						Log.Warning(string.Format("Loaded resource {0} could not be boxed as an AudioClip", kvp.Value));
					}
					requestTask = null;
					clip = null;
					kvp = default(KeyValuePair<PlacementType, string>);
				}
				Dictionary<PlacementType, string>.Enumerator enumerator = default(Dictionary<PlacementType, string>.Enumerator);
			}
			this.IsLoaded = true;
			InstrumentID.LoadFinishedAction onLoadFinished = this.OnLoadFinished;
			if (onLoadFinished != null)
			{
				onLoadFinished();
			}
			yield return true;
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnRequestFinished(AudioClip clip)
		{
			this.isClipLoaded = true;
		}

		public void Unload()
		{
			foreach (float[] array in this.ClipData.Values)
			{
				MemoryPools.poolFloat.Free(array);
			}
			this.ClipData.Clear();
			this.thisEnumerator = null;
			this.IsLoaded = (this.hasGrabbedClipProperties = false);
			this.Frames = (this.Samples = (this.Channels = 0));
			this.Frequency = 44100;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator<bool> thisEnumerator;

		public static string BundlePath;

		public string Name;

		public string SourceName;

		public float Volume = 1f;

		public int Frames;

		public int Samples;

		public int Channels;

		public int Frequency = 44100;

		[PublicizedFrom(EAccessModifier.Private)]
		public EnumDictionary<PlacementType, AudioClip> Clips;

		public EnumDictionary<PlacementType, float[]> ClipData;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasGrabbedClipProperties;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isClipLoaded;

		public delegate void LoadFinishedAction();
	}
}
