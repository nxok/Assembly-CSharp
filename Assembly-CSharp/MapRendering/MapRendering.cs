using System;
using Webserver;
using Webserver.UrlHandlers;

namespace MapRendering
{
	public static class MapRendering
	{
		public static void Init()
		{
			ModEvents.GameStartDone.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SGameStartDoneData>(MapRendering.GameStartDone));
			ModEvents.GameShutdown.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SGameShutdownData>(MapRendering.GameShutdown));
			ModEvents.CalcChunkColorsDone.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SCalcChunkColorsDoneData>(MapRendering.CalcChunkColorsDone));
			Web.ServerInitialized += delegate(Web _web)
			{
				if (!MapRenderer.Enabled)
				{
					return;
				}
				_web.RegisterPathHandler("/map/", new StaticHandler(GameIO.GetSaveGameDir() + "/map", MapRenderer.GetTileCache(), false, "web.map"));
				_web.OpenApiHelpers.RegisterCustomSpec(typeof(MapRendering).Assembly, "MapTileHandler", "/map/");
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void GameStartDone(ref ModEvents.SGameStartDoneData _data)
		{
			if (!MapRenderer.Enabled)
			{
				return;
			}
			MapRenderer instance = MapRenderer.Instance;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void GameShutdown(ref ModEvents.SGameShutdownData _data)
		{
			MapRenderer.Shutdown();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void CalcChunkColorsDone(ref ModEvents.SCalcChunkColorsDoneData _data)
		{
			MapRenderer.RenderSingleChunk(_data.Chunk);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string mapTilesBaseUrl = "/map/";
	}
}
