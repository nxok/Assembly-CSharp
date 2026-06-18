using System;
using System.Collections;
using System.Collections.Generic;
using MusicUtils.Enums;
using UniLinq;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public abstract class LayerMixer<ConfigType> : ILayerMixer where ConfigType : IConfiguration
	{
		public SectionType Sect { get; set; }

		public LayerMixer()
		{
			this.clipSetsFor = new EnumDictionary<LayerType, List<LayeredContent>>();
		}

		public abstract float this[int _idx] { get; }

		public virtual IEnumerator Load()
		{
			Log.Out(string.Format("Loading new config for {0}...", this.Sect));
			this.config = AbstractConfiguration.Get<ConfigType>(this.Sect);
			if (this.config == null)
			{
				Log.Warning(string.Format("{0} pulled a null config", this.Sect));
			}
			this.clipSetsFor.Clear();
			yield return null;
			yield break;
		}

		public void Unload()
		{
			this.clipSetsFor.Values.ToList<List<LayeredContent>>().ForEach(delegate(List<LayeredContent> list)
			{
				list.ToList<LayeredContent>().ForEach(delegate(LayeredContent e)
				{
					e.Unload();
				});
			});
			this.clipSetsFor.Clear();
			Log.Out(string.Format("unloaded ClipSets on {0}", this.Sect));
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ConfigType config;

		[PublicizedFrom(EAccessModifier.Protected)]
		public EnumDictionary<LayerType, List<LayeredContent>> clipSetsFor;
	}
}
