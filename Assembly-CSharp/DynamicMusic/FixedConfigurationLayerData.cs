using System;
using System.Collections.Generic;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class FixedConfigurationLayerData : ICountable
	{
		public FixedConfigurationLayerData()
		{
			this.LayerInstances = new List<List<PlacementType>>();
		}

		public int Count
		{
			get
			{
				return this.LayerInstances.Count;
			}
		}

		public void Add(List<PlacementType> _list)
		{
			this.LayerInstances.Add(_list);
		}

		public List<List<PlacementType>> LayerInstances;
	}
}
