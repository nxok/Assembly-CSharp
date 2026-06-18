using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class District
	{
		public District()
		{
		}

		public District(District _other)
		{
			this.name = _other.name;
			this.prefabName = _other.prefabName;
			this.tag = _other.tag;
			this.townships = _other.townships;
			this.weight = _other.weight;
			this.preview_color = _other.preview_color;
			this.avoidedNeighborDistricts = _other.avoidedNeighborDistricts;
			this.Init();
		}

		public void Init()
		{
			this.type = District.Type.None;
			if (this.name.EndsWith("commercial"))
			{
				this.type = District.Type.Commercial;
				this.spawnOrder = 1f;
				return;
			}
			if (this.name.EndsWith("downtown"))
			{
				this.type = District.Type.Downtown;
				this.spawnOrder = 99f;
				return;
			}
			if (this.name.EndsWith("gateway"))
			{
				this.type = District.Type.Gateway;
				return;
			}
			if (this.name.EndsWith("industrial"))
			{
				this.type = District.Type.Industrial;
				return;
			}
			if (this.name.EndsWith("residential"))
			{
				this.type = District.Type.Residential;
				this.spawnOrder = -1f;
				return;
			}
			if (this.name.EndsWith("rural"))
			{
				this.type = District.Type.Rural;
			}
		}

		public string name;

		public string prefabName;

		public District.Type type;

		public FastTags<TagGroup.Poi> tag;

		public FastTags<TagGroup.Poi> townships;

		public float weight = 0.5f;

		public float spawnOrder;

		public Color preview_color;

		public bool spawnCustomSizePrefabs;

		public List<string> avoidedNeighborDistricts = new List<string>();

		public enum Type
		{
			None,
			Commercial,
			Downtown,
			Gateway,
			Industrial,
			Residential,
			Rural
		}
	}
}
