using System;
using System.Xml.Linq;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class BloodmoonClipSet : LayeredContent
	{
		public override float GetSample(PlacementType _placement, int _idx, params float[] _params)
		{
			return this.clips[_placement].GetSample(_idx, _params);
		}

		public override void ParseFromXml(XElement _xmlNode)
		{
			base.ParseFromXml(_xmlNode);
			foreach (XElement xelement in _xmlNode.Elements("clip"))
			{
				PlacementType placementType = EnumUtils.Parse<PlacementType>(xelement.GetAttribute("key"), false);
				IClipAdapter clipAdapter = LayeredContent.CreateClipAdapter(xelement.GetAttribute("type"));
				clipAdapter.ParseXml(xelement);
				this.clips.Add(placementType, clipAdapter);
			}
		}
	}
}
