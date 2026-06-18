using System;
using Webserver.UrlHandlers;

namespace Webserver
{
	public static class WebServer
	{
		public static void Init()
		{
			ModEvents.GameStartDone.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SGameStartDoneData>(WebServer.GameStartDone));
			ModEvents.WorldShuttingDown.RegisterHandler(new ModEvents.ModEventHandlerDelegate<ModEvents.SWorldShuttingDownData>(WebServer.WorldShuttingDown));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void GameStartDone(ref ModEvents.SGameStartDoneData _data)
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return;
			}
			WebServer.webInstance = new Web();
			LogBuffer.Init();
			if (ItemIconHandler.Instance != null)
			{
				ThreadManager.StartCoroutine(ItemIconHandler.Instance.LoadIcons());
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void WorldShuttingDown(ref ModEvents.SWorldShuttingDownData _data)
		{
			Web web = WebServer.webInstance;
			if (web == null)
			{
				return;
			}
			web.Disconnect();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static Web webInstance;
	}
}
