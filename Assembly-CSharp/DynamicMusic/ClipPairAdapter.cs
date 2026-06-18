using System;
using System.Collections;
using System.Xml.Linq;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class ClipPairAdapter : IClipAdapter
	{
		public bool IsLoaded
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public ClipPairAdapter()
		{
			this.clipAdapterLo = new ClipAdapter();
			this.clipAdapterHi = new ClipAdapter();
		}

		public float GetSample(int idx, params float[] _params)
		{
			return _params[0] * (this.clipAdapterLo.GetSample(idx, null) * (1f - _params[1]) + _params[1] * this.clipAdapterHi.GetSample(idx, null));
		}

		public IEnumerator Load()
		{
			yield return this.clipAdapterLo.Load();
			yield return this.clipAdapterHi.Load();
			yield break;
		}

		public void LoadImmediate()
		{
			this.clipAdapterLo.LoadImmediate();
			this.clipAdapterHi.LoadImmediate();
		}

		public void Unload()
		{
			this.clipAdapterLo.Unload();
			this.clipAdapterHi.Unload();
			this.IsLoaded = false;
		}

		public void ParseXml(XElement _xmlNode)
		{
		}

		public void SetPaths(int _num, PlacementType _placement, SectionType _section, LayerType _layer, string stress = "")
		{
			this.clipAdapterLo.SetPaths(_num, _placement, _section, _layer, "Lo");
			this.clipAdapterHi.SetPaths(_num, _placement, _section, _layer, "Hi");
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ClipAdapter clipAdapterLo;

		[PublicizedFrom(EAccessModifier.Private)]
		public ClipAdapter clipAdapterHi;
	}
}
