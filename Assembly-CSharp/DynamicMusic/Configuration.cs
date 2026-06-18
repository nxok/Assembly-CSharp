using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class Configuration : AbstractConfiguration, IFiniteConfiguration, IConfiguration<IList<PlacementType>>, IConfiguration
	{
		public Dictionary<LayerType, IList<PlacementType>> Layers
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public Configuration()
		{
			this.Layers = new Dictionary<LayerType, IList<PlacementType>>();
		}

		public override int CountFor(LayerType _layer)
		{
			if (!this.Layers.ContainsKey(_layer))
			{
				return 0;
			}
			return 1;
		}

		public override void ParseFromXml(XElement _xmlNode)
		{
			base.ParseFromXml(_xmlNode);
			foreach (XElement xelement in _xmlNode.Elements("layer"))
			{
				this.ParseLayers(xelement);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ParseLayers(XElement e)
		{
			List<PlacementType> list = new List<PlacementType>();
			foreach (string text in e.GetAttribute("value").Split(',', StringSplitOptions.None))
			{
				list.Add((PlacementType)byte.Parse(text));
			}
			LayerType layerType = EnumUtils.Parse<LayerType>(e.GetAttribute("key"), false);
			this.Layers.Add(layerType, list);
		}
	}
}
