using System;
using System.Collections.Generic;
using DynamicMusic.Legacy.ObjectModel;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic.Legacy
{
	public class LayerStreamer
	{
		public bool HasReachedLastHyperbar
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public bool IsPlaying
		{
			get
			{
				return this.Src && this.Src.isPlaying;
			}
		}

		public static LayerStreamer Create(Layer _layer, LayerConfig _layerConfig, ThreatLevelStreamer _parent = null)
		{
			return new LayerStreamer(_layer, _layerConfig, _parent);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public LayerStreamer(Layer _layer, LayerConfig _layerConfig, ThreatLevelStreamer _parent)
		{
			this.parentId = _parent.id;
			this.HasReachedLastHyperbar = false;
			this.LayerConfig = _layerConfig;
			this.instrumentID = _layer.GetInstrumentID();
			if (this.instrumentID.IsLoaded)
			{
				this.OnClipSetLoad();
				return;
			}
			this.instrumentID.OnLoadFinished += this.OnClipSetLoad;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnClipSetLoad()
		{
			this.Src = Object.Instantiate<AudioSource>(Resources.Load<AudioSource>(this.instrumentID.SourceName));
			this.Src.volume = this.instrumentID.Volume;
			this.Src.clip = AudioClip.Create(this.instrumentID.Name, this.instrumentID.Frames, this.instrumentID.Channels, this.instrumentID.Frequency, true, new AudioClip.PCMReaderCallback(this.FillStream));
			this.InitFinished = true;
		}

		public void Cleanup()
		{
			if (this.Src)
			{
				Object.Destroy(this.Src.gameObject);
			}
			this.instrumentID.OnLoadFinished -= this.OnClipSetLoad;
			this.instrumentID.Unload();
		}

		public void Play(double _time)
		{
			this.Src.PlayScheduled(_time);
			this.Src.loop = true;
		}

		public void Pause()
		{
			if (this.Src && this.Src.isPlaying)
			{
				this.Src.Pause();
			}
		}

		public void UnPause()
		{
			if (this.Src)
			{
				this.Src.UnPause();
			}
		}

		public void Stop()
		{
			if (this.Src && this.Src.isPlaying)
			{
				this.Src.Stop();
			}
		}

		public void Tick()
		{
			if (this.HasReachedLastHyperbar && this.Src && this.Src.loop)
			{
				this.Src.loop = false;
				Log.Out(string.Format("Set loop to false on {0}", this.instrumentID.Name));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void FillStream(float[] data)
		{
			if (!this.InFillStream)
			{
				this.InFillStream = true;
				int num = data.Length;
				for (int i = 0; i < num; i++)
				{
					if (this.cursor == 0)
					{
						Dictionary<byte, PlacementType> layerConfig = this.LayerConfig;
						int num2 = this.hyperbar;
						this.hyperbar = num2 + 1;
						PlacementType placementType;
						if (layerConfig.TryGetValue((byte)num2, out placementType))
						{
							this.currentClipData = this.instrumentID.ClipData[placementType];
							this.HasReachedLastHyperbar = placementType == PlacementType.End;
						}
						else
						{
							this.currentClipData = null;
						}
					}
					data[i] = ((this.currentClipData != null) ? this.currentClipData[this.cursor] : 0f);
					this.cursor++;
					this.cursor %= this.instrumentID.Samples;
				}
				this.InFillStream = false;
				return;
			}
			Log.Warning("FillStream was called while it was still running.");
		}

		public bool InitFinished;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int parentId;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly InstrumentID instrumentID;

		[PublicizedFrom(EAccessModifier.Private)]
		public LayerConfig LayerConfig;

		[PublicizedFrom(EAccessModifier.Private)]
		public AudioSource Src;

		[PublicizedFrom(EAccessModifier.Private)]
		public int hyperbar;

		[PublicizedFrom(EAccessModifier.Private)]
		public int cursor;

		[PublicizedFrom(EAccessModifier.Private)]
		public float[] currentClipData;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool InFillStream;
	}
}
