using System;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class PathShared
	{
		public PathShared(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
			this.IdToColor = new Color32[]
			{
				default(Color32),
				this.CountryColor,
				this.HighwayColor,
				this.CountryColor,
				this.WaterColor
			};
		}

		public void ConvertIdsToColors(byte[] ids, Color32[] dest)
		{
			for (int i = 0; i < ids.Length; i++)
			{
				int num = (int)ids[i];
				dest[i] = this.IdToColor[num & 15];
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Color32 CountryColor = new Color32(0, byte.MaxValue, 0, byte.MaxValue);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Color32 HighwayColor = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Color32 WaterColor = new Color32(0, 0, byte.MaxValue, byte.MaxValue);

		public readonly Color32[] IdToColor;
	}
}
