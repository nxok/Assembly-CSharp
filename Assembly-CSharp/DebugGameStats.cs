using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Platform;
using UnityEngine;

public static class DebugGameStats
{
	public static void StartStatisticsUpdate(DebugGameStats.StatisticsUpdatedCallback callback)
	{
		if (DebugGameStats.m_memorySampler == null)
		{
			IPlatformMemory memory = PlatformManager.MultiPlatform.Memory;
			DebugGameStats.m_memorySampler = ((memory != null) ? memory.CreateSampler() : null);
		}
		IPlatformMemorySampler memorySampler = DebugGameStats.m_memorySampler;
		object obj;
		if (memorySampler == null)
		{
			obj = null;
		}
		else
		{
			IReadOnlyList<IPlatformMemoryStat> statistics = memorySampler.Statistics;
			if (statistics == null)
			{
				obj = null;
			}
			else
			{
				obj = statistics.FirstOrDefault((IPlatformMemoryStat s) => s.Name == "GameUsed");
			}
		}
		DebugGameStats.m_memoryGameUsedStat = (IPlatformMemoryStat<long>)obj;
		DebugGameStats.TryInitializeStatisticsDictionary();
		if (DebugGameStats.updateStatsCoroutine != null)
		{
			ThreadManager.StopCoroutine(DebugGameStats.updateStatsCoroutine);
		}
		if (DebugGameStats.updateDeltasCoroutine != null)
		{
			ThreadManager.StopCoroutine(DebugGameStats.updateDeltasCoroutine);
		}
		DebugGameStats.doStatisticsUpdate = true;
		DebugGameStats.updateStatsCoroutine = ThreadManager.StartCoroutine(DebugGameStats.UpdateStatisticsCo(callback));
		DebugGameStats.updateDeltasCoroutine = ThreadManager.StartCoroutine(DebugGameStats.UpdateDeltas());
	}

	public static void TryInitializeStatisticsDictionary()
	{
		if (DebugGameStats.statisticsDictionary.Keys.Count > 0)
		{
			return;
		}
		foreach (FieldInfo fieldInfo in typeof(DebugGameStats.Statistics).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy))
		{
			if (fieldInfo.IsLiteral && !fieldInfo.IsInitOnly && fieldInfo.FieldType == typeof(string))
			{
				string text = (string)fieldInfo.GetValue(null);
				if (!DebugGameStats.statisticsDictionary.ContainsKey(text))
				{
					DebugGameStats.statisticsDictionary[text] = string.Empty;
				}
			}
		}
	}

	public static void StopStatisticsUpdate()
	{
		DebugGameStats.doStatisticsUpdate = false;
		DebugGameStats.updateStatsCoroutine = null;
		DebugGameStats.updateDeltasCoroutine = null;
	}

	public static string GetHeader(char separator)
	{
		DebugGameStats.TryInitializeStatisticsDictionary();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> keyValuePair in DebugGameStats.statisticsDictionary)
		{
			stringBuilder.Append(keyValuePair.Key);
			stringBuilder.Append(separator);
		}
		return stringBuilder.ToString();
	}

	public static string GetCurrentStatsString(char separator = ',')
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> keyValuePair in DebugGameStats.statisticsDictionary)
		{
			stringBuilder.Append(keyValuePair.Value);
			stringBuilder.Append(separator);
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator UpdateDeltas()
	{
		while (DebugGameStats.doStatisticsUpdate)
		{
			DebugGameStats.deltaTextureMemory += (long)(Texture.currentTextureMemory - (ulong)DebugGameStats.m_textureMemoryPrevFrame);
			DebugGameStats.m_textureMemoryPrevFrame = (long)Texture.currentTextureMemory;
			yield return null;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator UpdateStatisticsCo(DebugGameStats.StatisticsUpdatedCallback callback)
	{
		long num = 0L;
		while (DebugGameStats.doStatisticsUpdate)
		{
			DebugGameStats.statisticsDictionary["gamestats.ingametime"] = (GameTimer.Instance.ticksSincePlayfieldLoaded / 20UL).ToString();
			if (DebugGameStats.m_memorySampler != null)
			{
				DebugGameStats.m_memorySampler.Sample();
				if (DebugGameStats.m_memoryGameUsedStat != null && DebugGameStats.m_memoryGameUsedStat.TryGet(MemoryStatColumn.Current, out num))
				{
					DebugGameStats.statisticsDictionary["gamestats.nativegameused"] = (num / 1024L).ToString();
				}
			}
			DebugGameStats.statisticsDictionary["gamestats.entityinstances"] = Entity.InstanceCount.ToString();
			DebugGameStats.statisticsDictionary["gamestats.maxusedchunks"] = Chunk.InstanceCount.ToString();
			DebugGameStats.statisticsDictionary["gamestats.displayedprefabs"] = GameManager.Instance.prefabLODManager.displayedPrefabs.Count.ToString();
			long currentTextureMemory = (long)Texture.currentTextureMemory;
			DebugGameStats.statisticsDictionary["gamestats.texturememorydelta60"] = (DebugGameStats.deltaTextureMemory / 1024L).ToString();
			DebugGameStats.deltaTextureMemory = 0L;
			DebugGameStats.statisticsDictionary["gamestats.texturememorycurrent"] = (currentTextureMemory / 1024L).ToString();
			DebugGameStats.statisticsDictionary["gamestats.textureMemorydesired"] = (Texture.desiredTextureMemory / 1024UL).ToString();
			Log.Out(string.Concat(new string[]
			{
				"60sec delta: ",
				DebugGameStats.statisticsDictionary["gamestats.texturememorydelta60"],
				",current: ",
				DebugGameStats.statisticsDictionary["gamestats.texturememorycurrent"],
				",desired: ",
				DebugGameStats.statisticsDictionary["gamestats.textureMemorydesired"]
			}));
			int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode);
			if (@int - 1 <= 1)
			{
				DebugGameStats.statisticsDictionary["gamestats.VideoScalingSetting"] = string.Format("{0}, Quality preset {1}, FSR preset {2}", GameOptionsPlatforms.UpscalerMode.ToString(@int), GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset), GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFSRPreset));
			}
			else
			{
				DebugGameStats.statisticsDictionary["gamestats.VideoScalingSetting"] = string.Format("{0}, Quality preset {1}", GameOptionsPlatforms.UpscalerMode.ToString(@int), GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset));
			}
			if (GameManager.Instance.World != null)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
				{
					DebugGameStats.statisticsDictionary["gamestats.ConnectionStatus"] = "Connected";
				}
				else
				{
					DebugGameStats.statisticsDictionary["gamestats.ConnectionStatus"] = "Disconnected";
				}
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsSinglePlayer)
				{
					DebugGameStats.statisticsDictionary["gamestats.HostStatus"] = "SinglePlayer";
				}
				else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					DebugGameStats.statisticsDictionary["gamestats.HostStatus"] = "MultiplayerHostOrServer";
				}
				else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
				{
					DebugGameStats.statisticsDictionary["gamestats.HostStatus"] = "Client";
				}
				else
				{
					DebugGameStats.statisticsDictionary["gamestats.HostStatus"] = "Unknown";
				}
				Dictionary<string, string> dictionary = DebugGameStats.statisticsDictionary;
				string text = "gamestats.gameMode";
				GameMode gameMode = GameManager.Instance.GetGameStateManager().GetGameMode();
				dictionary[text] = ((gameMode != null) ? gameMode.GetName() : null);
				DebugGameStats.statisticsDictionary["gamestats.PlayerCount"] = string.Format("Clients: {0}", SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount());
				DebugGameStats.statisticsDictionary["gamestats.worldentities"] = GameManager.Instance.World.Entities.Count.ToString();
				DebugGameStats.statisticsDictionary["gamestats.chunkobservers"] = GameManager.Instance.World.m_ChunkManager.m_ObservedEntities.Count.ToString();
				DebugGameStats.statisticsDictionary["gamestats.syncedchunks"] = GameManager.Instance.World.ChunkCache.chunks.list.Count.ToString();
				DebugGameStats.statisticsDictionary["gamestats.chunkgameobjects"] = GameManager.Instance.World.m_ChunkManager.GetDisplayedChunkGameObjectsCount().ToString();
				DebugGameStats.statisticsDictionary["gamestats.worldtime"] = ValueDisplayFormatters.WorldTime(GameManager.Instance.World.worldTime, "Day {0}, {1:00}:{2:00}");
				DebugGameStats.statisticsDictionary["gamestats.isbloodmoon"] = GameManager.Instance.World.isEventBloodMoon.ToString();
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				if (primaryPlayer)
				{
					DebugGameStats.statisticsDictionary["gamestats.currentPlayerBiome"] = string.Format("Player biome {0}", primaryPlayer.biomeStandingOn);
				}
				if (WeatherManager.Instance)
				{
					Color fogColor = SkyManager.GetFogColor();
					WeatherManager.BiomeWeather currentWeather = WeatherManager.currentWeather;
					bool flag = currentWeather != null;
					DebugGameStats.statisticsDictionary["gamestats.weathersystemstatus"] = string.Concat(new string[]
					{
						"WM status: ",
						WeatherManager.Instance.ToString(),
						"\n PlayerCurrent: <Temp: ",
						WeatherManager.GetTemperature().ToCultureInvariantString(),
						" Clouds ",
						(WeatherManager.GetCloudThickness() * 0.01f).ToCultureInvariantString(),
						" Fog density ",
						SkyManager.GetFogDensity().ToCultureInvariantString(),
						", start ",
						SkyManager.GetFogStart().ToCultureInvariantString(),
						", end ",
						SkyManager.GetFogEnd().ToCultureInvariantString(),
						" FogColor ",
						fogColor.r.ToCultureInvariantString(),
						" ",
						fogColor.g.ToCultureInvariantString(),
						" ",
						fogColor.b.ToCultureInvariantString(),
						" Rain  ",
						((WeatherManager.forceRain >= 0f) ? WeatherManager.forceRain : (flag ? currentWeather.rainParam.value : 0f)).ToCultureInvariantString(),
						" Snowfall  ",
						((WeatherManager.forceSnowfall >= 0f) ? WeatherManager.forceSnowfall : (flag ? currentWeather.snowFallParam.value : 0f)).ToCultureInvariantString(),
						" Temperature ",
						WeatherManager.GetTemperature().ToCultureInvariantString(),
						" Wind ",
						WeatherManager.GetWindSpeed().ToCultureInvariantString(),
						">"
					});
				}
				if (GameManager.Instance.World.GetLocalPlayers().Count > 0)
				{
					if (!DiscordManager.Instance.IsInitialized)
					{
						DebugGameStats.statisticsDictionary["gamestats.discordstatus"] = (DiscordManager.Instance.Settings.DiscordDisabled ? "Disabled and not initialized" : "Not initialized");
					}
					else if (!DiscordManager.Instance.IsReady)
					{
						DebugGameStats.statisticsDictionary["gamestats.discordstatus"] = (DiscordManager.Instance.Settings.DiscordDisabled ? "Disabled and not ready" : "Not ready");
					}
					else
					{
						DebugGameStats.statisticsDictionary["gamestats.discordstatus"] = "Discord enabled initialized and ready";
						DebugGameStats.statisticsDictionary["gamestats.discordsettings"] = SdPlayerPrefs.GetString("DiscordSettings");
					}
					DebugGameStats.statisticsDictionary["gamestats.localplayerpos"] = GameManager.Instance.World.GetLocalPlayers()[0].position.ToString();
				}
			}
			Log.Out("[Backtrace] Updated Statistics");
			callback(DebugGameStats.statisticsDictionary);
			yield return new WaitForSeconds(60f);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string> statisticsDictionary = new Dictionary<string, string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static IPlatformMemorySampler m_memorySampler;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IPlatformMemoryStat<long> m_memoryGameUsedStat;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool doStatisticsUpdate = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Coroutine updateStatsCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Coroutine updateDeltasCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public static long deltaTextureMemory = 0L;

	[PublicizedFrom(EAccessModifier.Private)]
	public static long m_textureMemoryPrevFrame = 0L;

	[PublicizedFrom(EAccessModifier.Private)]
	public static class Statistics
	{
		public const string InGameTimeKey = "gamestats.ingametime";

		public const string ManagedHeapSizeKey = "gamestats.managedheap";

		public const string NativeGameUsedKey = "gamestats.nativegameused";

		public const string TextureMemoryDesiredKey = "gamestats.textureMemorydesired";

		public const string TextureMemoryCurrentKey = "gamestats.texturememorycurrent";

		public const string TextureMemoryDelta60Key = "gamestats.texturememorydelta60";

		public const string WorldEntitiesCountKey = "gamestats.worldentities";

		public const string EntityInstanceCountKey = "gamestats.entityinstances";

		public const string ChunkObserverCountKey = "gamestats.chunkobservers";

		public const string MaxUsedChunkCountKey = "gamestats.maxusedchunks";

		public const string SyncedChunkCountKey = "gamestats.syncedchunks";

		public const string ChunkGameObjectCountKey = "gamestats.chunkgameobjects";

		public const string DisplayedPrefabCountKey = "gamestats.displayedprefabs";

		public const string LocalPlayerPositionKey = "gamestats.localplayerpos";

		public const string WorldTimeKey = "gamestats.worldtime";

		public const string BloodMoonKey = "gamestats.isbloodmoon";

		public const string GameModeKey = "gamestats.gameMode";

		public const string PlayerCountKey = "gamestats.PlayerCount";

		public const string ConnectStatusKey = "gamestats.ConnectionStatus";

		public const string HostStatusKey = "gamestats.HostStatus";

		public const string CurrentBiomeKey = "gamestats.currentPlayerBiome";

		public const string WeatherStatusKey = "gamestats.weathersystemstatus";

		public const string VideoScalingSetting = "gamestats.VideoScalingSetting";

		public const string DiscordPluginStatusKey = "gamestats.discordstatus";

		public const string DiscordPluginSettingsKey = "gamestats.discordsettings";
	}

	public delegate void StatisticsUpdatedCallback(Dictionary<string, string> statisticsDictionary);
}
