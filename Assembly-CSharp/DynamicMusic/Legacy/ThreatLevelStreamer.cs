using System;
using System.Collections.Generic;
using DynamicMusic.Legacy.ObjectModel;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic.Legacy
{
	public class ThreatLevelStreamer
	{
		public bool InitFinished
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				using (Dictionary<LayerType, LayerStreamer>.ValueCollection.Enumerator enumerator = this.LayerStreamers.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (!enumerator.Current.InitFinished)
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public bool HasReachedLastHyperbar
		{
			get
			{
				using (Dictionary<LayerType, LayerStreamer>.ValueCollection.Enumerator enumerator = this.LayerStreamers.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (!enumerator.Current.HasReachedLastHyperbar)
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public static ThreatLevelStreamer Create(ThreatLevelLegacyType _tl)
		{
			return new ThreatLevelStreamer(_tl);
		}

		public static ThreatLevelStreamer Create(ThreatLevelLegacyType _tl, ThreatLevel _groupTL, ThreatLevelConfig _config)
		{
			return new ThreatLevelStreamer(_tl, _groupTL, _config);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ThreatLevelStreamer(ThreatLevelLegacyType _tl)
		{
			this.id = ThreatLevelStreamer.numCreated++;
			this.threatLevel = _tl;
			MusicGroup musicGroup = MusicGroup.AllGroups[0];
			Dictionary<LayerType, LayerConfig> dictionary = ConfigSet.AllConfigSets[musicGroup.ConfigIDs[0]][_tl];
			ThreatLevel threatLevel = musicGroup[_tl];
			this.LayerStreamers = new EnumDictionary<LayerType, LayerStreamer>();
			foreach (KeyValuePair<LayerType, LayerConfig> keyValuePair in dictionary)
			{
				this.LayerStreamers.Add(keyValuePair.Key, LayerStreamer.Create(threatLevel[keyValuePair.Key], keyValuePair.Value, this));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ThreatLevelStreamer(ThreatLevelLegacyType _tl, ThreatLevel _groupTL, ThreatLevelConfig _config)
		{
			this.id = ThreatLevelStreamer.numCreated++;
			this.LayerStreamers = new EnumDictionary<LayerType, LayerStreamer>(_config.Count);
			foreach (KeyValuePair<LayerType, LayerConfig> keyValuePair in _config)
			{
				this.LayerStreamers.Add(keyValuePair.Key, LayerStreamer.Create(_groupTL[keyValuePair.Key], keyValuePair.Value, this));
			}
		}

		public void Cleanup()
		{
			foreach (LayerStreamer layerStreamer in this.LayerStreamers.Values)
			{
				layerStreamer.Cleanup();
			}
		}

		public void Play()
		{
			if (this.InitFinished)
			{
				double num = AudioSettings.dspTime + 0.25;
				Log.Out(string.Format("Calling Play on {0}", this.id));
				foreach (LayerStreamer layerStreamer in this.LayerStreamers.Values)
				{
					layerStreamer.Play(num);
				}
			}
		}

		public void Pause()
		{
			this.IsPaused = true;
			foreach (LayerStreamer layerStreamer in this.LayerStreamers.Values)
			{
				layerStreamer.Pause();
			}
		}

		public void UnPause()
		{
			this.IsPaused = false;
			foreach (LayerStreamer layerStreamer in this.LayerStreamers.Values)
			{
				layerStreamer.UnPause();
			}
		}

		public void Stop()
		{
			foreach (LayerStreamer layerStreamer in this.LayerStreamers.Values)
			{
				layerStreamer.Stop();
			}
		}

		public void Tick()
		{
			if (!this.IsPlaying)
			{
				return;
			}
			foreach (LayerStreamer layerStreamer in this.LayerStreamers.Values)
			{
				layerStreamer.Tick();
			}
		}

		public bool IsPlaying
		{
			get
			{
				using (Dictionary<LayerType, LayerStreamer>.ValueCollection.Enumerator enumerator = this.LayerStreamers.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.IsPlaying)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public ThreatLevelLegacyType threatLevel;

		public static int numCreated;

		public readonly int id;

		public bool IsPaused;

		public Dictionary<LayerType, LayerStreamer> LayerStreamers;
	}
}
