using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MusicUtils.Enums;
using Unity.Profiling;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class FixedLayerMixer : LayerMixer<FixedConfiguration>
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void updateHyperbar(int _idx)
		{
			this.hyperbar = _idx / (this.sectSamplesFor * 2);
		}

		public override float this[int _idx]
		{
			get
			{
				float num = 0f;
				this.updateHyperbar(_idx);
				int num2 = _idx % (this.sectSamplesFor * 2);
				foreach (KeyValuePair<LayerType, FixedConfigurationLayerData> keyValuePair in this.config.Layers)
				{
					int num3 = 0;
					foreach (List<PlacementType> list in keyValuePair.Value.LayerInstances)
					{
						if (this.hyperbar < list.Count)
						{
							PlacementType placementType = list[this.hyperbar];
							if (placementType != PlacementType.None)
							{
								List<LayeredContent> list2;
								if (this.clipSetsFor.TryGetValue(keyValuePair.Key, out list2) && num3 < list2.Count)
								{
									float sample = list2[num3].GetSample(placementType, num2, Array.Empty<float>());
									num = num + sample - num * sample;
								}
								else if (!this.warningLogHash.Contains(keyValuePair.Key))
								{
									StringBuilder stringBuilder = new StringBuilder();
									stringBuilder.AppendLine(string.Format("could not get clipsets for {0} on {1}", keyValuePair.Key, base.Sect));
									stringBuilder.AppendLine(string.Format("Summary of state for LayerMixer on {0}:", base.Sect));
									stringBuilder.AppendLine("selected config:");
									foreach (KeyValuePair<LayerType, FixedConfigurationLayerData> keyValuePair2 in this.config.Layers)
									{
										stringBuilder.AppendLine(string.Format("{0}: {1}", keyValuePair2.Key, keyValuePair2.Value.Count));
									}
									stringBuilder.AppendLine("clipset data:");
									foreach (KeyValuePair<LayerType, List<LayeredContent>> keyValuePair3 in this.clipSetsFor)
									{
										stringBuilder.AppendLine(string.Format("{0}: {1}", keyValuePair3.Key, keyValuePair3.Value.Count));
									}
									Log.Warning(stringBuilder.ToString());
									this.warningLogHash.Add(keyValuePair.Key);
								}
							}
							num3++;
						}
					}
				}
				return num;
			}
		}

		public override IEnumerator Load()
		{
			yield return this.<>n__0();
			Log.Out(string.Format("Loading new ClipSets for {0}...", base.Sect));
			foreach (KeyValuePair<LayerType, FixedConfigurationLayerData> kvp in this.config.Layers)
			{
				List<LayeredContent> list = new List<LayeredContent>();
				int num;
				for (int i = 0; i < kvp.Value.Count; i = num + 1)
				{
					LayeredContent content = LayeredContent.Get<ClipSet>(base.Sect, kvp.Key);
					yield return content.Load();
					list.Add(content);
					content = null;
					num = i;
				}
				this.clipSetsFor.Add(kvp.Key, list);
				list = null;
				kvp = default(KeyValuePair<LayerType, FixedConfigurationLayerData>);
			}
			Dictionary<LayerType, FixedConfigurationLayerData>.Enumerator enumerator = default(Dictionary<LayerType, FixedConfigurationLayerData>.Enumerator);
			this.sectSamplesFor = Content.SamplesFor[base.Sect];
			Log.Out(string.Format("{0} loaded new config and clipsets", base.Sect));
			this.warningLogHash.Clear();
			yield break;
			yield break;
		}

		public bool IsFinished
		{
			get
			{
				foreach (FixedConfigurationLayerData fixedConfigurationLayerData in this.config.Layers.Values)
				{
					foreach (List<PlacementType> list in fixedConfigurationLayerData.LayerInstances)
					{
						if (this.hyperbar >= list.Count)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ProfilerMarker s_LoadMarker = new ProfilerMarker("DynamicMusic.FixedLayerMixer.Load");

		[PublicizedFrom(EAccessModifier.Protected)]
		public int hyperbar;

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSet<LayerType> warningLogHash = new HashSet<LayerType>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public int sectSamplesFor;
	}
}
