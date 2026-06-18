using System;
using System.Collections.Generic;

namespace WorldGenerationEngineFinal
{
	public class TownshipData
	{
		public TownshipData(string _name, int _id)
		{
			this.Name = _name;
			this.Id = _id;
			if (_name.EndsWith("roadside"))
			{
				this.Category = TownshipData.eCategory.Roadside;
			}
			else if (_name.EndsWith("rural"))
			{
				this.Category = TownshipData.eCategory.Rural;
			}
			else if (_name.EndsWith("wilderness"))
			{
				this.Category = TownshipData.eCategory.Wilderness;
			}
			WorldBuilderStatic.idToTownshipData[this.Id] = this;
		}

		public string Name;

		public int Id;

		public List<string> SpawnableTerrain = new List<string>();

		public bool SpawnCustomSizes;

		public bool SpawnTrader = true;

		public bool SpawnGateway = true;

		public string OutskirtDistrict;

		public float OutskirtDistrictPercent;

		public FastTags<TagGroup.Poi> Biomes;

		public readonly TownshipData.eCategory Category;

		public enum eCategory
		{
			Normal,
			Roadside,
			Rural,
			Wilderness
		}
	}
}
