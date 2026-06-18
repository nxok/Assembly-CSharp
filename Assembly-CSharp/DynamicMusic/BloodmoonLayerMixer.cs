using System;
using System.Collections;
using System.Collections.Generic;
using MusicUtils.Enums;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class BloodmoonLayerMixer : LayerMixer<BloodmoonConfiguration>
	{
		public BloodmoonLayerMixer()
		{
			BloodmoonLayerMixer.player = GameManager.Instance.World.GetPrimaryPlayer();
			this.paramsFor = new EnumDictionary<LayerType, LayerParams>();
			Enum.GetValues(typeof(LayerType)).Cast<LayerType>().ToList<LayerType>()
				.ForEach(delegate(LayerType lyr)
				{
					this.paramsFor.Add(lyr, new LayerParams(0f, 1f));
				});
		}

		public override float this[int _idx]
		{
			get
			{
				if (BloodmoonLayerMixer.player == null)
				{
					BloodmoonLayerMixer.player = GameManager.Instance.World.GetPrimaryPlayer();
				}
				float num = ((BloodmoonLayerMixer.player == null) ? 0f : BloodmoonLayerMixer.player.ThreatLevel.Numeric);
				float num2 = 0f;
				foreach (KeyValuePair<LayerType, LayerState> keyValuePair in this.config.Layers)
				{
					LayerParams layerParams = this.paramsFor[keyValuePair.Key];
					layerParams.Volume = Mathf.Clamp01(layerParams.Volume + ((keyValuePair.Value.Get(num) == LayerStateType.disabled) ? (-3.7792895E-06f) : 3.7792895E-06f));
					layerParams.Mix = Mathf.Clamp01(layerParams.Mix + ((keyValuePair.Value.Get(num) != LayerStateType.hi) ? (-3.7792895E-06f) : 3.7792895E-06f));
					foreach (LayeredContent layeredContent in this.clipSetsFor[keyValuePair.Key])
					{
						num2 += layeredContent.GetSample(PlacementType.Loop, _idx, new float[] { layerParams.Volume, layerParams.Mix });
					}
				}
				return (float)Math.Tanh((double)num2);
			}
		}

		public override IEnumerator Load()
		{
			yield return this.<>n__0();
			foreach (LayerParams layerParams in this.paramsFor.Values)
			{
				layerParams.Mix = 1f;
				layerParams.Volume = 0f;
			}
			foreach (LayerType layer in this.config.Layers.Keys)
			{
				LayeredContent content = LayeredContent.Get<BloodmoonClipSet>(SectionType.Bloodmoon, layer);
				yield return content.Load();
				this.clipSetsFor.Add(layer, new List<LayeredContent> { content });
				content = null;
			}
			Dictionary<LayerType, LayerState>.KeyCollection.Enumerator enumerator2 = default(Dictionary<LayerType, LayerState>.KeyCollection.Enumerator);
			BloodmoonLayerMixer.player = GameManager.Instance.World.GetPrimaryPlayer();
			yield break;
			yield break;
		}

		public static float ThreatLevel = 0.75f;

		[PublicizedFrom(EAccessModifier.Private)]
		public static EntityPlayerLocal player;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cIncrement = 3.7792895E-06f;

		[PublicizedFrom(EAccessModifier.Private)]
		public EnumDictionary<LayerType, LayerParams> paramsFor;
	}
}
