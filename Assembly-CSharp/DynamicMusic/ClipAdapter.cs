using System;
using System.Collections;
using System.Xml.Linq;
using MusicUtils;
using MusicUtils.Enums;
using Unity.Profiling;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class ClipAdapter : IClipAdapter
	{
		public float GetSample(int idx, params float[] _params)
		{
			if (idx % 4096 == 0)
			{
				this.reader.Position = 2 * idx;
				this.reader.Read(this.sampleData, 4096);
			}
			return this.sampleData[idx % 4096];
		}

		public bool IsLoaded
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public IEnumerator Load()
		{
			using (ClipAdapter.s_LoadMarker.Auto())
			{
				this.reader = new WaveReader(this.path);
				this.sampleData = MemoryPools.poolFloat.Alloc(4096);
				this.IsLoaded = true;
			}
			yield return null;
			yield break;
		}

		public void LoadImmediate()
		{
			using (ClipAdapter.s_LoadImmediateMarker.Auto())
			{
				this.reader = new WaveReader(this.path);
				this.sampleData = MemoryPools.poolFloat.Alloc(4096);
				this.IsLoaded = true;
			}
		}

		public void Unload()
		{
			MemoryPools.poolFloat.Free(this.sampleData);
			this.sampleData = null;
			this.reader.Cleanup();
			this.reader = null;
			this.IsLoaded = false;
		}

		public void ParseXml(XElement _xmlNode)
		{
			this.path = _xmlNode.GetAttribute("value");
		}

		public void SetPaths(int _num, PlacementType _placement, SectionType _section, LayerType _layer, string stress = "")
		{
			this.path = string.Concat(new string[]
			{
				GameIO.GetApplicationPath(),
				"/Data/Music/",
				_num.ToString("000"),
				DMSConstants.PlacementAbbrv[_placement],
				DMSConstants.SectionAbbrvs[_section],
				DMSConstants.LayerAbbrvs[_layer],
				stress,
				".wav"
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ProfilerMarker s_LoadMarker = new ProfilerMarker("DynamicMusic.ClipAdapter.Load");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ProfilerMarker s_LoadImmediateMarker = new ProfilerMarker("DynamicMusic.ClipAdapter.LoadImmediate");

		[PublicizedFrom(EAccessModifier.Private)]
		public const int bufferSize = 4096;

		[PublicizedFrom(EAccessModifier.Private)]
		public string path;

		[PublicizedFrom(EAccessModifier.Private)]
		public float[] sampleData;

		[PublicizedFrom(EAccessModifier.Private)]
		public WaveReader reader;
	}
}
