using System;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class POISmoother
	{
		public POISmoother(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public void SmoothStreetTiles()
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			int streetTileMapWidth = this.worldBuilder.StreetTileMapWidth;
			int streetTileMapWidth2 = this.worldBuilder.StreetTileMapWidth;
			int num = 0;
			int num2 = streetTileMapWidth * streetTileMapWidth2;
			for (int i = 0; i < streetTileMapWidth2; i++)
			{
				for (int j = 0; j < streetTileMapWidth; j++)
				{
					num++;
					StreetTile streetTile = this.worldBuilder.StreetTileMap[j + i * streetTileMapWidth];
					streetTile.SmoothTownshipTerrain();
					streetTile.UpdateValidity();
				}
				this.worldBuilder.SetTaskMessage(string.Format(this.worldBuilder.messageSmoothingStreetTiles, Mathf.RoundToInt((float)num / (float)num2 * 100f)));
			}
			Log.Out("POISmoother SmoothStreetTiles in {0}", new object[] { (float)microStopwatch.ElapsedMilliseconds * 0.001f });
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;
	}
}
