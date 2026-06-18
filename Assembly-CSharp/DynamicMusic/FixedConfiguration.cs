using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class FixedConfiguration : AbstractConfiguration, IConfiguration<FixedConfigurationLayerData>, IConfiguration
	{
		public Dictionary<LayerType, FixedConfigurationLayerData> Layers
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public FixedConfiguration()
		{
			this.Layers = new Dictionary<LayerType, FixedConfigurationLayerData>();
		}

		public override int CountFor(LayerType _layer)
		{
			FixedConfigurationLayerData fixedConfigurationLayerData;
			if (this.Layers.TryGetValue(_layer, out fixedConfigurationLayerData))
			{
				return fixedConfigurationLayerData.Count;
			}
			return 0;
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
			FixedConfigurationLayerData fixedConfigurationLayerData;
			if (!this.Layers.TryGetValue(layerType, out fixedConfigurationLayerData))
			{
				this.Layers.Add(layerType, fixedConfigurationLayerData = new FixedConfigurationLayerData());
			}
			fixedConfigurationLayerData.Add(list);
		}
	}
}
