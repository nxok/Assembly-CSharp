using System;
using System.Collections;
using System.Collections.Generic;
using MusicUtils.Enums;
using UniLinq;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class CombatLayerMixer : FixedLayerMixer
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void updateHyperbar(int _idx)
		{
			this.hyperbar = _idx / (Content.SamplesFor[base.Sect] * 2) % this.maxHyperbar;
		}

		public override IEnumerator Load()
		{
			yield return this.<>n__0();
			this.maxHyperbar = this.config.Layers.Values.First<FixedConfigurationLayerData>().LayerInstances.First<List<PlacementType>>().Count;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int maxHyperbar;
	}
}
