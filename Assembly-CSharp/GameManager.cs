using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Audio;
using BhvrAnalyticsServices.Interfaces;
using GUI_2;
using InControl;
using MapRendering;
using Platform;
using SandboxOptions;
using Services;
using Services.Analytics.Events;
using Twitch;
using Unity.Collections;
using UnityEngine;
using Webserver;

public class GameManager : MonoBehaviour, IGameManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator waitForGameStart()
	{
		if (!GameManager.IsDedicatedServer && !this.IsEditMode())
		{
			EntityPlayerLocal epl = null;
			while (this.World != null)
			{
				epl = this.World.GetPrimaryPlayer();
				if (epl != null)
				{
					while (!epl.IsSpawned())
					{
						yield return null;
					}
					epl.HasUpdated = false;
					while (!epl.HasUpdated)
					{
						yield return false;
					}
					yield return null;
					this.GameHasStarted = true;
					epl = null;
					goto IL_010F;
				}
				yield return null;
			}
			yield break;
		}
		IL_010F:
		yield break;
	}

	public void ShowBackground(bool show)
	{
		if (this.bShowBackground == show)
		{
			return;
		}
		this.bShowBackground = show;
		Camera main = Camera.main;
		if (main != null)
		{
			if (!this.bShowBackground)
			{
				this.cameraCullMask = main.cullingMask;
				main.cullingMask = LayerMask.GetMask(new string[] { "LocalPlayer" });
				main.backgroundColor = this.backgroundColor;
				return;
			}
			main.cullingMask = this.cameraCullMask;
		}
	}

	public bool ShowBackground()
	{
		return this.bShowBackground;
	}

	public void IncreaseBackgroundColor()
	{
		switch (this.currentBackgroundColorChannel)
		{
		case 0:
			this.backgroundColor.r = this.backgroundColor.r + 0.003921569f;
			break;
		case 1:
			this.backgroundColor.g = this.backgroundColor.g + 0.003921569f;
			break;
		case 2:
			this.backgroundColor.b = this.backgroundColor.b + 0.003921569f;
			break;
		}
		this.backgroundColor.r = Mathf.Clamp01(this.backgroundColor.r);
		this.backgroundColor.g = Mathf.Clamp01(this.backgroundColor.g);
		this.backgroundColor.b = Mathf.Clamp01(this.backgroundColor.b);
		Camera main = Camera.main;
		if (main != null)
		{
			main.backgroundColor = this.backgroundColor;
		}
	}

	public void DecreaseBackgroundColor()
	{
		switch (this.currentBackgroundColorChannel)
		{
		case 0:
			this.backgroundColor.r = this.backgroundColor.r - 0.003921569f;
			break;
		case 1:
			this.backgroundColor.g = this.backgroundColor.g - 0.003921569f;
			break;
		case 2:
			this.backgroundColor.b = this.backgroundColor.b - 0.003921569f;
			break;
		}
		this.backgroundColor.r = Mathf.Clamp01(this.backgroundColor.r);
		this.backgroundColor.g = Mathf.Clamp01(this.backgroundColor.g);
		this.backgroundColor.b = Mathf.Clamp01(this.backgroundColor.b);
		Camera main = Camera.main;
		if (main != null)
		{
			main.backgroundColor = this.backgroundColor;
		}
	}

	public void BackgroundColorNext()
	{
		this.currentBackgroundColorChannel++;
		if (this.currentBackgroundColorChannel > 2)
		{
			this.currentBackgroundColorChannel = 0;
		}
	}

	public void BackgroundColorPrev()
	{
		this.currentBackgroundColorChannel--;
		if (this.currentBackgroundColorChannel < 0)
		{
			this.currentBackgroundColorChannel = 2;
		}
	}

	public static bool IsDedicatedServer
	{
		get
		{
			if (!GameManager.isDedicatedChecked)
			{
				string[] commandLineArgs = GameStartupHelper.GetCommandLineArgs();
				for (int i = 0; i < commandLineArgs.Length; i++)
				{
					if (commandLineArgs[i].Equals(global::Constants.cArgDedicatedServer))
					{
						GameManager.isDedicated = true;
					}
				}
				GameManager.isDedicatedChecked = true;
			}
			return GameManager.isDedicated;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnUserDetailsUpdated(IPlatformUserData userData, string name)
	{
		if (this.persistentPlayers != null)
		{
			this.persistentPlayers.HandlePlayerDetailsUpdate(userData, name);
		}
	}

	public bool GameIsFocused
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsMouseCursorVisible
	{
		get
		{
			return PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && Cursor.visible;
		}
	}

	public event GameManager.OnWorldChangedEvent OnWorldChanged;

	public event GameManager.OnLocalPlayerChangedEvent OnLocalPlayerChanged;

	public event Action<ClientInfo> OnClientSpawned;

	public void ApplyAllOptions()
	{
		if (this.windowManager != null)
		{
			GameOptionsManager.ApplyAllOptions(this.windowManager.playerUI);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (!GameEntrypoint.EntrypointSuccess)
		{
			return;
		}
		MicroStopwatch microStopwatch = new MicroStopwatch(true);
		GameManager.Instance = this;
		this.GameIsFocused = !GameManager.IsDedicatedServer && Application.isFocused;
		Log.Out("Awake IsFocused: " + Application.isFocused.ToString());
		Log.Out("Awake");
		ThreadManager.SetMonoBehaviour(this);
		Utils.InitStatic();
		LoadManager.Init();
		if (Application.isEditor)
		{
			Application.runInBackground = true;
			this.bCursorVisibleOverride = true;
			this.bCursorVisibleOverrideState = true;
		}
		if (!GameManager.IsDedicatedServer)
		{
			GameOptionsManager.ResolutionChanged += this.OnResolutionChanged;
			this.RefreshRefreshRate();
			this.UpdateFPSCap();
		}
		else
		{
			Application.targetFrameRate = 500;
		}
		Application.wantsToQuit += this.OnApplicationQuit;
		if (GameManager.IsDedicatedServer && GamePrefs.GetBool(EnumGamePrefs.TerminalWindowEnabled))
		{
			try
			{
				WinFormInstance winFormInstance = new WinFormInstance();
				SingletonMonoBehaviour<SdtdConsole>.Instance.RegisterServer(winFormInstance);
			}
			catch (Exception ex)
			{
				Log.Error("Could not start Terminal Window:");
				Log.Exception(ex);
			}
		}
		this.windowManager = Object.FindAnyObjectByType<GUIWindowManager>();
		this.nguiWindowManager = Object.FindAnyObjectByType<NGUIWindowManager>();
		TaskManager.Init();
		LocalPlayerManager.Init();
		GameStatsBridge.Init();
		if (!GameManager.IsDedicatedServer)
		{
			GameOptionsControls.Load();
		}
		MeshDataManager.Init();
		OcclusionManager.Load();
		this.waitForTargetFPS = new GameObject("WaitForTargetFPS").AddComponent<WaitForTargetFPS>();
		if (GameManager.IsDedicatedServer)
		{
			GameOptionsManager.ApplyTextureQuality(3);
			QualitySettings.vSyncCount = 0;
			this.waitForTargetFPS.TargetFPS = 0;
		}
		else
		{
			QualitySettings.vSyncCount = GamePrefs.GetInt(PlatformApplicationManager.Application.VSyncCountPref);
			this.waitForTargetFPS.TargetFPS = 0;
		}
		ServerDateTimeRequest.GetNtpTimeAsync(delegate(ServerDateTimeResult result)
		{
			GameManager.ServerClockSync = result;
		}, "pool.ntp.org", 5000);
		GameObjectPool.Instance.Init();
		MemoryPools.InitStatic(!GameManager.IsDedicatedServer);
		this.gameRandomManager = GameRandomManager.Instance;
		this.gameStateManager = new GameStateManager(this);
		this.prefabLODManager = new PrefabLODManager();
		new PrefabEditModeManager();
		Object.Instantiate<GameObject>(DataLoader.LoadAsset<GameObject>("@:Sound_Mixers/AudioMixerManager.prefab", false));
		this.m_SoundsGameObject = GameObject.Find("Sounds");
		this.PhysicsInit();
		ParticleEffect.Init();
		SelectionBoxManager.Instance.SetupCategories();
		if (!GameManager.IsDedicatedServer)
		{
			if (GameOptionsReset.NeedsResetGame())
			{
				GameOptionsReset.ResetGame(RoamingPrefs.Store);
				GamePrefs.Instance.Save();
				Log.Out("Game Reset");
			}
			else
			{
				if (GameOptionsReset.Graphics.NeedsReset())
				{
					GameOptionsReset.Graphics.Reset(RoamingPrefs.Store);
					GamePrefs.Instance.Save();
					Log.Out("Graphics Reset");
				}
				if (GameOptionsReset.Controls.NeedsReset())
				{
					GameOptionsReset.Controls.Reset(RoamingPrefs.Store);
					GamePrefs.Instance.Save();
					Log.Out("Controls Reset");
				}
				if (GameOptionsReset.Bindings.NeedsReset())
				{
					GameOptionsReset.Bindings.Reset(RoamingPrefs.Store);
					GamePrefs.Instance.Save();
					Log.Out("Bindings Reset");
				}
			}
		}
		DeviceGamePrefs.Apply();
		GameOptionsManager.ValidateGamePrefs();
		if (!GameManager.IsDedicatedServer)
		{
			GameOptionsManager.ApplyAllOptions(this.windowManager.playerUI);
			UIUtils.LoadAtlas();
		}
		Manager.Init();
		UIOptions.Init();
		UIRoot uiroot = Object.FindAnyObjectByType<UIRoot>();
		if (!GameManager.IsDedicatedServer)
		{
			this.InitMultiSourceUiAtlases(uiroot.gameObject);
		}
		this.windowManager.gameObject.AddComponent<LocalPlayerUI>();
		this.blockSelectionTool = new BlockToolSelection();
		this.nguiWindowManager.ParseWindows();
		float activeUiScale = GameOptionsManager.GetActiveUiScale();
		this.nguiWindowManager.SetBackgroundScale(activeUiScale);
		this.AddWindows(this.windowManager);
		PrefabVolumeManager.Instance.Init();
		ModManager.LoadMods();
		ThreadManager.RunCoroutineSync(ModManager.LoadPatchStuff(false));
		this.adminTools = new AdminTools();
		SingletonMonoBehaviour<SdtdConsole>.Instance.RegisterCommands();
		IEnumerator enumerator = this.loadStaticData();
		if (GameManager.IsDedicatedServer)
		{
			this.bStaticDataLoadSync = true;
			ThreadManager.RunCoroutineSync(enumerator);
		}
		else
		{
			this.bStaticDataLoadSync = false;
			ThreadManager.StartCoroutine(enumerator);
		}
		if (!GameManager.IsDedicatedServer)
		{
			CursorControllerAbs.LoadStaticData(LoadManager.CreateGroup());
		}
		else
		{
			InputManager.Enabled = false;
		}
		if (GameManager.IsDedicatedServer && GamePrefs.GetBool(EnumGamePrefs.TelnetEnabled))
		{
			try
			{
				TelnetConsole telnetConsole = new TelnetConsole();
				SingletonMonoBehaviour<SdtdConsole>.Instance.RegisterServer(telnetConsole);
			}
			catch (Exception ex2)
			{
				Log.Error("Could not start network console:");
				Log.Exception(ex2);
			}
		}
		WebServer.Init();
		global::MapRendering.MapRendering.Init();
		AuthorizationManager.Instance.Init();
		ModEvents.SGameAwakeData sgameAwakeData;
		ModEvents.GameAwake.Invoke(ref sgameAwakeData);
		this.nguiWindowManager.Show(EnumNGUIWindow.InGameHUD, false);
		ConsoleCmdShow.Init();
		if (!GameManager.IsDedicatedServer)
		{
			GameSenseManager instance = GameSenseManager.Instance;
			if (instance != null)
			{
				instance.Init();
			}
			if (GamePrefs.GetBool(EnumGamePrefs.OptionsMumblePositionalAudioSupport))
			{
				MumblePositionalAudio.Init();
			}
		}
		DiscordManager instance2 = DiscordManager.Instance;
		if (this.BackgroundMusicClip || this.CreditsSongClip)
		{
			if (!GameManager.IsDedicatedServer)
			{
				base.gameObject.AddComponent<BackgroundMusicMono>();
			}
			else
			{
				Resources.UnloadAsset(this.BackgroundMusicClip);
				Resources.UnloadAsset(this.CreditsSongClip);
			}
		}
		PartyQuests.EnforeInstance();
		Input.simulateMouseWithTouches = false;
		IPlatform nativePlatform = PlatformManager.NativePlatform;
		IApplicationStateController applicationStateController = ((nativePlatform != null) ? nativePlatform.ApplicationState : null);
		if (applicationStateController != null)
		{
			ApplicationState lastState = ApplicationState.Foreground;
			applicationStateController.OnApplicationStateChanged += delegate(ApplicationState state)
			{
				if (state != ApplicationState.Suspended && lastState == ApplicationState.Suspended)
				{
					this.OnApplicationResume();
				}
				lastState = state;
			};
			applicationStateController.OnNetworkStateChanged += this.OnNetworkStateChanged;
		}
		PlatformUserManager.DetailsUpdated += this.OnUserDetailsUpdated;
		if (!GameManager.IsDedicatedServer)
		{
			this.triggerEffectManager = new TriggerEffectManager();
			TriggerEffectManager.SetMainMenuLightbarColor();
		}
		Log.Out("Awake done in " + microStopwatch.ElapsedMilliseconds.ToString() + " ms");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitMultiSourceUiAtlases(GameObject _parent)
	{
		GameManager.<>c__DisplayClass127_0 CS$<>8__locals1 = new GameManager.<>c__DisplayClass127_0();
		CS$<>8__locals1.atlasesGo = new GameObject("UIAtlases");
		CS$<>8__locals1.atlasesGo.transform.parent = _parent.transform;
		Shader shader = Shader.Find("Unlit/Transparent Colored");
		Shader shader2 = Shader.Find("Unlit/Transparent Greyscale");
		MultiSourceAtlasManager multiSourceAtlasManager = MultiSourceAtlasManager.Create(CS$<>8__locals1.atlasesGo, "ItemIconAtlas");
		MultiSourceAtlasManager multiSourceAtlasManager2 = MultiSourceAtlasManager.Create(CS$<>8__locals1.atlasesGo, "ItemIconAtlasGreyscale");
		ModManager.ModAtlasesDefaults(CS$<>8__locals1.atlasesGo, shader);
		ModManager.RegisterAtlasManager(multiSourceAtlasManager, false, shader, new Action<INGUIAtlas, bool>(this.AddGreyscaleItemIconAtlas));
		ModManager.RegisterAtlasManager(multiSourceAtlasManager2, false, shader2, null);
		Resources.Load<UIAtlas>("GUI/Prefabs/SymbolAtlas");
		Resources.Load<UIAtlas>("GUI/Prefabs/ControllerArtAtlas");
		GameManager.<>c__DisplayClass127_0 CS$<>8__locals2 = CS$<>8__locals1;
		INGUIAtlas[] array = Resources.FindObjectsOfTypeAll<UIAtlas>();
		CS$<>8__locals2.<InitMultiSourceUiAtlases>g__addLoadedAtlases|1(array);
		GameManager.<>c__DisplayClass127_0 CS$<>8__locals3 = CS$<>8__locals1;
		array = Resources.FindObjectsOfTypeAll<NGUIAtlas>();
		CS$<>8__locals3.<InitMultiSourceUiAtlases>g__addLoadedAtlases|1(array);
		CS$<>8__locals1.mipFilter = GameOptionsPlatforms.GetItemIconFilterString();
		LoadManager.AssetsRequestTask<GameObject> assetsRequestTask = LoadManager.LoadAssetsFromAddressables<GameObject>("iconatlas", (string address) => address.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) && address.Contains(CS$<>8__locals1.mipFilter), null, false, true);
		List<GameObject> list = new List<GameObject>();
		assetsRequestTask.CollectResults(list);
		foreach (GameObject gameObject in list)
		{
			GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
			gameObject2.transform.parent = multiSourceAtlasManager.transform;
			UIAtlas component = gameObject2.GetComponent<UIAtlas>();
			multiSourceAtlasManager.AddAtlas(component, gameObject2, false);
			this.AddGreyscaleItemIconAtlas(component, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddGreyscaleItemIconAtlas(INGUIAtlas _atlas, bool _isLoadingInGame)
	{
		MultiSourceAtlasManager atlasManager = ModManager.GetAtlasManager("ItemIconAtlasGreyscale");
		Shader shader = Shader.Find("Unlit/Transparent Greyscale");
		INGUIAtlas inguiatlas = _atlas.Clone();
		GameObject gameObject = null;
		UIAtlas uiatlas = inguiatlas as UIAtlas;
		if (uiatlas != null)
		{
			uiatlas.gameObject.transform.parent = atlasManager.transform;
			gameObject = uiatlas.gameObject;
		}
		inguiatlas.spriteMaterial = new Material(shader)
		{
			mainTexture = inguiatlas.texture
		};
		atlasManager.AddAtlas(inguiatlas, gameObject, _isLoadingInGame);
	}

	public void AddWindows(GUIWindowManager _guiWindowManager)
	{
		if (_guiWindowManager == this.windowManager)
		{
			_guiWindowManager.Add(GUIWindowConsole.GetNewInstance());
			_guiWindowManager.Add(new GUIWindowScreenshotText());
		}
		_guiWindowManager.Add(new GUIWindowNGUI(EnumNGUIWindow.InGameHUD));
		_guiWindowManager.CloseAllOpenModalWindows(null, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator loadStaticData()
	{
		this.CurrentLoadAction = Localization.Get("loadActionCharacterModels", false, null);
		yield return null;
		this.CurrentLoadAction = Localization.Get("loadActionTerrainTextures", false, null);
		yield return null;
		yield return null;
		yield return WorldStaticData.Init(false, GameManager.IsDedicatedServer, delegate(string _progressText, float _percentage)
		{
			this.CurrentLoadAction = _progressText;
		}, null);
		this.CurrentLoadAction = Localization.Get("loadActionDone", false, null);
		this.bStaticDataLoaded = true;
		yield break;
	}

	public bool IsStartingGame
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public void StartGame(bool _offline)
	{
		Time.timeScale = 1f;
		GamePrefs.Set(EnumGamePrefs.GameGuidClient, "");
		PlatformManager.MultiPlatform.UserDataRoaming.ValidateRoamingMode();
		if (GameSparksManager.Instance() != null)
		{
			GameSparksManager.Instance().PrepareNewSession();
		}
		base.StartCoroutine(this.startGameCo(_offline));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startGameCo(bool _offline)
	{
		this.IsStartingGame = true;
		Log.Out("StartGame");
		ModEvents.SGameStartingData sgameStartingData = new ModEvents.SGameStartingData(SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer);
		ModEvents.GameStarting.Invoke(ref sgameStartingData);
		this.allowQuit = false;
		this.backgroundColor = Color.white;
		EntityStats.WeatherSurvivalEnabled = true;
		yield return null;
		yield return ModManager.LoadPatchStuff(true);
		yield return null;
		SaveInfoProvider.Instance.ClearResources();
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			yield break;
		}
		if (!GameManager.IsDedicatedServer)
		{
			XUiC_MainMenu.CloseGlobalMenuWindows(this.windowManager.playerUI.xui);
			this.windowManager.CloseAllOpenModalWindows(null, false);
			XUiFromXml.ClearData();
			LocalPlayerUI.QueueUIForNewPlayerEntity(LocalPlayerUI.CreateUIForNewLocalPlayer());
			this.windowManager.Open(XUiC_LoadingScreen.ID, false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.EACEnabled)
				{
					this.windowManager.Open("eacWarning", false);
				}
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.AllowsCrossplay)
				{
					this.windowManager.Open("crossplayWarning", false);
				}
			}
			XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, Localization.Get("uiLoadStartingGame", false, null), null, true, true, false);
		}
		yield return null;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GamePrefs.Set(EnumGamePrefs.GameWorld, string.Empty);
		}
		this.isEditMode = GameModeEditWorld.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode));
		GamePrefs.Set(EnumGamePrefs.DebugStopEnemiesMoving, this.IsEditMode());
		GamePrefs.Set(EnumGamePrefs.DebugMenuEnabled, this.isEditMode || GameUtils.IsPlaytesting());
		GamePrefs.Set(EnumGamePrefs.CreativeMenuEnabled, this.isEditMode || GameUtils.IsPlaytesting());
		GamePrefs.Instance.Save();
		if (!Application.isEditor)
		{
			GameUtils.DebugOutputGamePrefs(delegate(string _text)
			{
				Log.WriteLine("GamePref." + _text);
			});
			GameUtils.DebugOutputGameStats(delegate(string _text)
			{
				Log.WriteLine("GameStat." + _text);
			});
		}
		yield return null;
		CraftingManager.InitForNewGame();
		yield return null;
		GameManager.bSavingActive = true;
		GameManager.bPhysicsActive = !this.IsEditMode();
		GameManager.bTickingActive = !this.IsEditMode();
		GameManager.bShowDecorBlocks = true;
		GameManager.bShowLootBlocks = true;
		GameManager.bShowPaintables = true;
		GameManager.bShowUnpaintables = true;
		GameManager.bShowTerrain = true;
		GameManager.bVolumeBlocksEditing = true;
		Block.nameIdMapping = null;
		ItemClass.nameIdMapping = null;
		PlatformApplicationManager.SetRestartRequired();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			yield return this.StartAsServer(_offline);
		}
		else
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
			{
				yield break;
			}
			XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, Localization.Get("uiLoadWaitingForServer", false, null), null, true, true, false);
			this.StartAsClient();
		}
		DismembermentManager.Init();
		yield return null;
		if (GameSparksManager.Instance() != null)
		{
			GameSparksManager.Instance().SessionStarted(GamePrefs.GetString(EnumGamePrefs.GameWorld), GamePrefs.GetString(EnumGamePrefs.GameMode), SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer);
		}
		if (!GameManager.IsDedicatedServer && SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode != ProtocolManager.NetworkType.OfflineServer)
		{
			PlatformManager.MultiPlatform.User.StartAdvertisePlaying(SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		}
		Log.Out("Loading dymesh settings");
		DynamicMeshManager.CONTENT_ENABLED = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled);
		DynamicMeshSettings.OnlyPlayerAreas = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshLandClaimOnly);
		DynamicMeshSettings.UseImposterValues = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshUseImposters);
		DynamicMeshSettings.MaxViewDistance = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshDistance);
		DynamicMeshSettings.PlayerAreaChunkBuffer = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshLandClaimBuffer);
		DynamicMeshSettings.MaxRegionMeshData = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshMaxRegionCache);
		DynamicMeshSettings.MaxDyMeshData = GamePrefs.GetInt(EnumGamePrefs.DynamicMeshMaxItemCache);
		DynamicMeshSettings.LogSettings();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicMeshManager.Init();
		}
		ModEvents.SGameStartDoneData eventData;
		ModEvents.GameStartDone.Invoke(ref eventData);
		EnumResetUnprotectedChunksGroupingMode @int = (EnumResetUnprotectedChunksGroupingMode)GamePrefs.GetInt(EnumGamePrefs.ResetUnprotectedChunks);
		this.ResetUnprotectedChunksOnLoad(@int);
		if (GameManager.IsDedicatedServer)
		{
			this.waitForTargetFPS.TargetFPS = 20;
		}
		Log.Out("StartGame done");
		this.IsStartingGame = false;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetUnprotectedChunksOnLoad(EnumResetUnprotectedChunksGroupingMode _resetChunksMode)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (_resetChunksMode == EnumResetUnprotectedChunksGroupingMode.Disabled)
		{
			return;
		}
		World world = this.m_World;
		object obj;
		if (world == null)
		{
			obj = null;
		}
		else
		{
			ChunkCluster chunkCache = world.ChunkCache;
			obj = ((chunkCache != null) ? chunkCache.ChunkProvider : null);
		}
		ChunkProviderGenerateWorld chunkProviderGenerateWorld = obj as ChunkProviderGenerateWorld;
		if (chunkProviderGenerateWorld != null)
		{
			chunkProviderGenerateWorld.MainThreadCacheProtectedPositions();
			chunkProviderGenerateWorld.ResetAllChunks(ChunkProtectionLevel.All, _resetChunksMode);
			Log.Out("[GameManager] Unprotected chunks have been reset.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTimeOfDay()
	{
		if (!GameManager.IsDedicatedServer || this.m_World.Players.list.Count > 0)
		{
			int @int = GameStats.GetInt(EnumGameStats.TimeOfDayIncPerSec);
			if (@int == 0)
			{
				this.msPassedSinceLastUpdate += (int)(Time.deltaTime * 1000f);
				if (this.msPassedSinceLastUpdate >= 100)
				{
					this.m_World.SetTime(this.m_World.worldTime);
					this.msPassedSinceLastUpdate = 0;
				}
			}
			else
			{
				float num = 1000f / (float)@int;
				this.msPassedSinceLastUpdate += (int)(Time.deltaTime * 1000f);
				if ((float)this.msPassedSinceLastUpdate <= Utils.FastMax(num, 50f))
				{
					return;
				}
				int num2 = (int)((float)this.msPassedSinceLastUpdate / num);
				this.msPassedSinceLastUpdate -= (int)num * num2;
				ulong num3 = this.m_World.worldTime + (ulong)((long)num2);
				this.m_World.SetTime(num3);
			}
		}
		ILobbyHost lobbyHost = PlatformManager.NativePlatform.LobbyHost;
		if (lobbyHost != null)
		{
			lobbyHost.UpdateGameTimePlayers(this.m_World.worldTime, this.m_World.Players.list.Count);
		}
		GameSenseManager instance = GameSenseManager.Instance;
		if (instance != null)
		{
			instance.UpdateEventTime(this.m_World.worldTime);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.UpdateGameTimePlayers(this.m_World.worldTime, this.m_World.Players.list.Count);
			if (Time.time - this.lastTimeWorldTickTimeSentToClients > global::Constants.cSendWorldTickTimeToClients)
			{
				this.lastTimeWorldTickTimeSentToClients = Time.time;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWorldTime>().Setup(this.m_World.worldTime), true, -1, -1, -1, null, 192, false);
				if (WeatherManager.Instance != null)
				{
					WeatherManager.Instance.SendPackages();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSendClientPlayerPositionToServer()
	{
		EntityPlayerLocal primaryPlayer = this.m_World.GetPrimaryPlayer();
		EntityAlive entityAlive = primaryPlayer;
		if (entityAlive != null)
		{
			if (entityAlive.AttachedToEntity != null)
			{
				entityAlive = entityAlive.AttachedToEntity as EntityAlive;
				this.bLastWasAttached = true;
				if (entityAlive.isEntityRemote)
				{
					return;
				}
			}
			else
			{
				if (this.bLastWasAttached)
				{
					this.lastTimeAbsPosSentToServer = int.MaxValue;
				}
				this.bLastWasAttached = false;
			}
		}
		if (entityAlive == null)
		{
			return;
		}
		if (primaryPlayer.bPlayerStatsChanged)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(primaryPlayer), false);
			primaryPlayer.bPlayerStatsChanged = false;
		}
		if (primaryPlayer.bPlayerTwitchChanged)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerTwitchStats>().Setup(primaryPlayer), false);
			primaryPlayer.bPlayerTwitchChanged = false;
		}
		if (primaryPlayer.bEntityAliveFlagsChanged)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAliveFlags>().Setup(primaryPlayer), false);
			primaryPlayer.bEntityAliveFlagsChanged = false;
		}
		if (primaryPlayer.bPlayerEquipmentChanged)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerEquipment>().Setup(primaryPlayer), false);
			primaryPlayer.bPlayerEquipmentChanged = false;
		}
		Vector3i vector3i = NetEntityDistributionEntry.EncodePos(entityAlive.position);
		Vector3i vector3i2 = vector3i - entityAlive.serverPos;
		bool flag = Utils.FastAbs((float)vector3i2.x) >= 2f || Utils.FastAbs((float)vector3i2.y) >= 2f || Utils.FastAbs((float)vector3i2.z) >= 2f || entityAlive.emodel.IsRagdollActive;
		Vector3i vector3i3 = NetEntityDistributionEntry.EncodeRot(entityAlive.rotation);
		Vector3i vector3i4 = vector3i3 - entityAlive.serverRot;
		bool flag2 = Utils.FastAbs((float)vector3i4.x) >= 1f || Utils.FastAbs((float)vector3i4.y) >= 1f || Utils.FastAbs((float)vector3i4.z) >= 1f || entityAlive.emodel.IsRagdollActive;
		if (flag || flag2)
		{
			if (vector3i2.x < -256 || vector3i2.x >= 256 || vector3i2.y < -256 || vector3i2.y >= 256 || vector3i2.z < -256 || vector3i2.z >= 256)
			{
				this.lastTimeAbsPosSentToServer = 0;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityTeleport>().Setup(entityAlive), false);
			}
			else if (vector3i2.x < -128 || vector3i2.x >= 128 || vector3i2.y < -128 || vector3i2.y >= 128 || vector3i2.z < -128 || vector3i2.z >= 128 || this.lastTimeAbsPosSentToServer > 100)
			{
				this.lastTimeAbsPosSentToServer = 0;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityPosAndRot>().Setup(entityAlive), false);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityRelPosAndRot>().Setup(entityAlive.entityId, vector3i2, vector3i3, entityAlive.qrotation, entityAlive.onGround, entityAlive.IsQRotationUsed(), 3), false);
			}
			entityAlive.serverPos = vector3i;
			entityAlive.serverRot = vector3i3;
			this.lastTimeAbsPosSentToServer++;
		}
		if (entityAlive != primaryPlayer)
		{
			if (entityAlive.bPlayerStatsChanged)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(entityAlive), false);
				entityAlive.bPlayerStatsChanged = false;
			}
			if (entityAlive.bPlayerTwitchChanged)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerTwitchStats>().Setup(entityAlive), false);
				entityAlive.bPlayerTwitchChanged = false;
			}
			if (entityAlive.bEntityAliveFlagsChanged)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAliveFlags>().Setup(entityAlive), false);
				entityAlive.bEntityAliveFlagsChanged = false;
			}
			if (primaryPlayer.bPlayerEquipmentChanged)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerEquipment>().Setup(primaryPlayer), false);
				primaryPlayer.bPlayerEquipmentChanged = false;
			}
		}
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(primaryPlayer);
		if (this.countdownSendPlayerDataFileToServer.HasPassed() && uiforPlayer.xui != null && uiforPlayer.xui.IsReady)
		{
			this.countdownSendPlayerDataFileToServer.ResetAndRestart();
			this.doSendLocalPlayerData(primaryPlayer);
		}
		if (this.countdownSendPlayerInventoryToServer.HasPassed())
		{
			this.countdownSendPlayerInventoryToServer.Reset();
			this.doSendLocalInventory(primaryPlayer);
		}
		if (primaryPlayer.persistentPlayerData != null && primaryPlayer.persistentPlayerData.questPositionsChanged)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerQuestPositions>().Setup(primaryPlayer.entityId, primaryPlayer.persistentPlayerData), false);
			primaryPlayer.persistentPlayerData.questPositionsChanged = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		GameManager.fixedUpdateCount++;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		this.gmUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gmUpdate()
	{
		GameManager.frameCount = Time.frameCount;
		GameManager.frameTime = Time.time;
		GameManager.fixedUpdateCount = 0;
		this.updatePauseState();
		GameOptionsManager.CheckResolution();
		ModEvents.SUnityUpdateData sunityUpdateData;
		ModEvents.UnityUpdate.Invoke(ref sunityUpdateData);
		this.handleGlobalActions();
		if (!GameManager.ReportUnusedAssets(false))
		{
			return;
		}
		if ((double)Time.timeScale <= 0.001)
		{
			Physics.SyncTransforms();
		}
		LoadManager.Update();
		PlatformManager.Update();
		InviteManager.Instance.Update();
		LockManager.Instance.Update();
		this.swUpdateTime.ResetAndRestart();
		this.fps.Update();
		BlockLiquidv2.UpdateTime();
		if (QuestEventManager.Current != null)
		{
			QuestEventManager.Current.Update();
		}
		if (this.m_World != null)
		{
			this.m_World.triggerManager.Update();
		}
		if (TwitchVoteScheduler.Current != null)
		{
			TwitchVoteScheduler.Current.Update(Time.deltaTime);
		}
		if (TwitchManager.Current != null)
		{
			TwitchManager.Current.Update(Time.unscaledDeltaTime);
		}
		if (GameEventManager.Current != null)
		{
			GameEventManager.Current.Update(Time.deltaTime);
		}
		if (PowerManager.HasInstance)
		{
			PowerManager.Instance.Update();
		}
		if (PartyManager.HasInstance)
		{
			PartyManager.Current.Update();
		}
		if (VehicleManager.Instance != null)
		{
			VehicleManager.Instance.Update();
		}
		if (DroneManager.Instance != null)
		{
			DroneManager.Instance.Update();
		}
		if (DismembermentManager.Instance != null)
		{
			DismembermentManager.Instance.Update();
		}
		if (TurretTracker.Instance != null)
		{
			TurretTracker.Instance.Update();
		}
		if (RaycastPathManager.Instance)
		{
			RaycastPathManager.Instance.Update();
		}
		if (TokenManager.Instance != null)
		{
			TokenManager.Instance.Update();
		}
		TrajectorySimulation.UpdateSimulationQueue();
		if (FactionManager.Instance != null)
		{
			FactionManager.Instance.Update();
		}
		if (NavObjectManager.HasInstance)
		{
			NavObjectManager.Instance.Update();
		}
		if (BlockedPlayerList.Instance != null)
		{
			BlockedPlayerList.Instance.Update();
		}
		PrefabEditModeManager instance = PrefabEditModeManager.Instance;
		if (instance != null)
		{
			instance.Update();
		}
		TriggerEffectManager triggerEffectManager = this.triggerEffectManager;
		if (triggerEffectManager != null)
		{
			triggerEffectManager.Update();
		}
		SpeedTreeWindHistoryBufferManager.Instance.Update();
		ThreadManager.UpdateMainThreadTasks();
		if (!GameManager.IsDedicatedServer)
		{
			if (XUiC_MainMenu.openedOnce && !this.isQuitting)
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				bool? flag;
				string text;
				if (crossplatformPlatform == null)
				{
					flag = null;
				}
				else
				{
					IAntiCheatClient antiCheatClient = crossplatformPlatform.AntiCheatClient;
					flag = ((antiCheatClient != null) ? new bool?(antiCheatClient.GetUnhandledViolationMessage(out text)) : null);
				}
				bool? flag2 = flag;
				if (flag2.GetValueOrDefault())
				{
					GUIWindowManager guiwindowManager = LocalPlayerUI.primaryUI.windowManager;
					if (guiwindowManager != null)
					{
						string text2 = "EAC: " + Localization.Get("eacIntegrityViolation", false, null);
						if (!string.IsNullOrEmpty(text))
						{
							text += "\n";
						}
						else
						{
							text = "";
						}
						text += Localization.Get("eacUnableToPlayOnProtected", false, null);
						XUiC_MessageBoxWindowGroup.ShowOk(guiwindowManager.playerUI.xui, text2, text, null, !this.gameStateManager.IsGameStarted(), true, false);
					}
				}
			}
			if (!this.bCursorVisibleOverride && !this.isQuitting)
			{
				bool flag3 = this.isAnyCursorWindowOpen(null);
				if (this.GameIsFocused && this.bCursorVisible != flag3)
				{
					this.setCursorEnabled(flag3);
				}
				if (!flag3 && Cursor.visible && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
				{
					this.setCursorEnabled(false);
				}
			}
			this.UpdateFPSCap();
		}
		object syncRoot = ((ICollection)this.tileEntitiesMusicToRemove).SyncRoot;
		lock (syncRoot)
		{
			for (int i = 0; i < this.tileEntitiesMusicToRemove.Count; i++)
			{
				Object.Destroy(this.tileEntitiesMusicToRemove[i]);
			}
		}
		if (!this.gameStateManager.IsGameStarted())
		{
			GameTimer.Instance.Reset(GameTimer.Instance.ticks);
			return;
		}
		this.m_World.entityAsyncManager.Update();
		GameTimer.Instance.updateTimer(GameManager.IsDedicatedServer && this.m_World.Players.Count == 0);
		this.updateBlockParticles();
		this.updateTimeOfDay();
		Manager.FrameUpdate();
		WaterSimulationNative.Instance.Update();
		SignTextureManager.Instance.MainThreadUpdate();
		WaterEvaporationManager.UpdateEvaporation();
		if (GameTimer.Instance.elapsedTicks > 0 || this.m_World.m_ChunkManager.IsForceUpdate() || this.m_World.Players.list.Count == 0)
		{
			this.m_World.m_ChunkManager.DetermineChunksToLoad();
		}
		if (GameManager.IsDedicatedServer && this.m_World.Players.list.Count == 0 && this.lastPlayerCount > 0)
		{
			this.timeToClearAllPools = 8f;
		}
		this.lastPlayerCount = this.m_World.Players.list.Count;
		if (this.m_World.Players.list.Count == 0 && this.timeToClearAllPools > 0f && (this.timeToClearAllPools -= Time.deltaTime) <= 0f)
		{
			Log.Out("Clearing all pools");
			MemoryPools.Cleanup();
			this.m_World.ClearCaches();
		}
		if (!this.UpdateTick())
		{
			return;
		}
		this.m_World.m_ChunkManager.GroundAlignFrameUpdate();
		int num = (GameManager.IsDedicatedServer ? 25000 : 2500);
		this.swCopyChunks.ResetAndRestart();
		while (this.m_World.m_ChunkManager.CopyChunksToUnity() && this.swCopyChunks.ElapsedMicroseconds < (long)num)
		{
		}
		if (this.prefabLODManager != null)
		{
			this.prefabLODManager.FrameUpdate();
		}
		this.ExplodeGroupFrameUpdate();
		this.fpsCountdownTimer -= Time.deltaTime;
		if (this.fpsCountdownTimer <= 0f)
		{
			this.fpsCountdownTimer = 30f;
			GameManager.MaxMemoryConsumption = Math.Max(GC.GetTotalMemory(false), GameManager.MaxMemoryConsumption);
			if (!GameManager.IsDedicatedServer || SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0 || this.lastStatsPlayerCount > 0)
			{
				this.lastStatsPlayerCount = SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount();
				Log.Out(ConsoleCmdMem.GetStats(false, this));
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.m_World.ChunkCache.ChunkProvider.Update();
			this.wsCountdownTimer -= Time.deltaTime;
			if (this.wsCountdownTimer <= 0f)
			{
				this.wsCountdownTimer = 30f;
				if (!this.isEditMode)
				{
					this.m_World.SaveWorldState();
					if (Block.nameIdMapping != null)
					{
						Block.nameIdMapping.SaveIfDirty(true);
					}
					if (ItemClass.nameIdMapping != null)
					{
						ItemClass.nameIdMapping.SaveIfDirty(true);
					}
					EventPrefabs eventPrefabs = this.m_World.ChunkCache.ChunkProvider.GetEventPrefabs();
					if (eventPrefabs != null)
					{
						eventPrefabs.Save(false);
					}
				}
			}
			this.playerPositionsCountdownTimer -= Time.deltaTime;
			if (this.playerPositionsCountdownTimer <= 0f)
			{
				this.playerPositionsCountdownTimer = 6f;
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerPositions>().Setup(this.persistentPlayers), false, -1, -1, -1, null, 192, true);
				}
			}
		}
		if (GameManager.IsDedicatedServer)
		{
			this.gcCountdownTimer -= Time.deltaTime;
			if (this.gcCountdownTimer <= 0f)
			{
				this.gcCountdownTimer = 120f;
				GC.Collect();
			}
			if ((float)this.swUpdateTime.ElapsedMilliseconds > 50f)
			{
				this.waitForTargetFPS.SkipSleepThisFrame = true;
			}
		}
		else
		{
			GameSenseManager instance2 = GameSenseManager.Instance;
			if (instance2 != null)
			{
				instance2.Update();
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.countdownSaveLocalPlayerDataFile.HasPassed())
			{
				this.countdownSaveLocalPlayerDataFile.ResetAndRestart();
				this.SaveLocalPlayerData();
			}
			this.unloadAssetsDuration += Time.deltaTime;
			if (this.unloadAssetsDuration > 1200f)
			{
				bool flag5 = this.unloadAssetsDuration > 3600f;
				if (!this.isAnyModalWindowOpen())
				{
					this.isUnloadAssetsReady = true;
				}
				else if (this.isUnloadAssetsReady)
				{
					flag5 = true;
				}
				if (flag5)
				{
					this.stopwatchUnloadAssets.ResetAndRestart();
					Resources.UnloadUnusedAssets();
					this.stopwatchUnloadAssets.Stop();
					Log.Out("UnloadUnusedAssets after {0} m, took {1} ms", new object[]
					{
						this.unloadAssetsDuration / 60f,
						this.stopwatchUnloadAssets.ElapsedMilliseconds
					});
					this.unloadAssetsDuration = 0f;
					this.isUnloadAssetsReady = false;
				}
			}
		}
		if (this.stabilityViewer != null)
		{
			this.stabilityViewer.Update();
		}
		ModEvents.SGameUpdateData sgameUpdateData;
		ModEvents.GameUpdate.Invoke(ref sgameUpdateData);
		GameObjectPool.Instance.FrameUpdate();
	}

	public void LateUpdate()
	{
		ThreadManager.LateUpdate();
		PlatformManager.LateUpdate();
		if (this.m_World != null && this.m_World.aiDirector != null)
		{
			this.m_World.aiDirector.DebugFrameLateUpdate();
		}
		this.UpdateMultiplayerServices();
		MeshDataManager.Instance.LateUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool UpdateTick()
	{
		GameTimer instance = GameTimer.Instance;
		if (instance.elapsedTicks <= 0 && this.m_World.Players.list.Count != 0)
		{
			this.m_World.TickEntitiesSlice();
			return true;
		}
		this.m_World.TickEntitiesFlush();
		float num = (Time.time - this.lastTime) * 20f;
		this.lastTime = Time.time;
		this.m_World.OnUpdateTick(num, this.m_World.m_ChunkManager.GetActiveChunkSet());
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !this.gameStateManager.OnUpdateTick())
		{
			return false;
		}
		this.m_World.TickEntities(num);
		this.m_World.LetBlocksFall();
		if (!GameManager.IsDedicatedServer)
		{
			this.m_World.SetEntitiesVisibleNearToLocalPlayer();
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.m_World.entityDistributer.OnUpdateEntities();
			this.m_World.m_ChunkManager.SendChunksToClients();
			if (GameManager.bSavingActive)
			{
				ChunkCluster chunkCache = this.m_World.ChunkCache;
				ChunkProviderGenerateWorld chunkProviderGenerateWorld = ((chunkCache != null) ? chunkCache.ChunkProvider : null) as ChunkProviderGenerateWorld;
				if (chunkProviderGenerateWorld != null)
				{
					chunkProviderGenerateWorld.MainThreadCacheProtectedPositions();
				}
				if (instance.ticks % 40UL == 0UL)
				{
					if (chunkCache != null)
					{
						chunkCache.ChunkProvider.SaveRandomChunks(2, instance.ticks, this.m_World.m_ChunkManager.GetActiveChunkSet());
					}
				}
				else if (Time.time - this.lastTimeDecoSaved > 60f)
				{
					this.lastTimeDecoSaved = Time.time;
					this.m_World.SaveDecorations();
					if (chunkCache != null)
					{
						EventPrefabs eventPrefabs = chunkCache.ChunkProvider.GetEventPrefabs();
						if (eventPrefabs != null)
						{
							eventPrefabs.Save(false);
						}
					}
				}
			}
		}
		else
		{
			this.updateSendClientPlayerPositionToServer();
		}
		if (this.lastTime - this.activityCheck >= 1f)
		{
			PlatformManager.MultiPlatform.RichPresence.UpdateRichPresence(IRichPresence.PresenceStates.InGame);
			this.activityCheck = this.lastTime;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnNetworkStateChanged(bool connectionState)
	{
		if (!connectionState)
		{
			this.ShutdownMultiplayerServices(GameManager.EMultiShutReason.AppNoNetwork);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnApplicationResume()
	{
		PlatformApplicationManager.SetRestartRequired();
		if (!this.IsSafeToConnect())
		{
			this.ShutdownMultiplayerServices(GameManager.EMultiShutReason.AppSuspended);
			return;
		}
		ThreadManager.StartCoroutine(PlatformApplicationManager.CheckRestartCoroutine(false));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateMultiplayerServices()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (this.shuttingDownMultiplayerServices)
		{
			return;
		}
		if (this.IsSafeToConnect() || !this.IsSafeToDisconnect())
		{
			return;
		}
		ConnectionManager instance = SingletonMonoBehaviour<ConnectionManager>.Instance;
		if (!(instance == null))
		{
			ProtocolManager.NetworkType currentMode = instance.CurrentMode;
			if (currentMode != ProtocolManager.NetworkType.None && currentMode != ProtocolManager.NetworkType.OfflineServer)
			{
				EUserPerms permissions = PermissionsManager.GetPermissions(PermissionsManager.PermissionSources.All);
				if (!permissions.HasMultiplayer())
				{
					this.ShutdownMultiplayerServices(GameManager.EMultiShutReason.PermMissingMultiplayer);
					return;
				}
				if ((SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient ? SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo).AllowsCrossplay && !permissions.HasCrossplay())
				{
					this.ShutdownMultiplayerServices(GameManager.EMultiShutReason.PermMissingCrossplay);
					return;
				}
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetLocalizationKey(GameManager.EMultiShutReason _reason)
	{
		string text;
		switch (_reason)
		{
		case GameManager.EMultiShutReason.AppNoNetwork:
			text = "app_noNetwork";
			break;
		case GameManager.EMultiShutReason.AppSuspended:
			text = "app_suspended";
			break;
		case GameManager.EMultiShutReason.PermMissingMultiplayer:
			text = "permMissing_multiplayer";
			break;
		case GameManager.EMultiShutReason.PermMissingCrossplay:
			text = "permMissing_crossplay";
			break;
		default:
			throw new ArgumentOutOfRangeException("_reason", _reason, string.Format("Unknown Localization for {0}.{1}", "EMultiShutReason", _reason));
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShutdownMultiplayerServices(GameManager.EMultiShutReason _reason)
	{
		ThreadManager.StartCoroutine(this.ShutdownMultiplayerServicesCoroutine(_reason));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ShutdownMultiplayerServicesCoroutine(GameManager.EMultiShutReason _reason)
	{
		yield return null;
		if (GameManager.IsDedicatedServer)
		{
			yield break;
		}
		if (this.shuttingDownMultiplayerServices)
		{
			yield break;
		}
		this.shuttingDownMultiplayerServices = true;
		bool isClient = false;
		bool success = false;
		bool failReasonProvided = false;
		try
		{
			Log.Out(string.Format("Waiting to Shut Down Multiplayer Services ({0})...", _reason));
			while (this.shuttingDownMultiplayerServices && !this.IsSafeToConnect() && !this.IsSafeToDisconnect())
			{
				yield return null;
			}
			ConnectionManager connectionManager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			for (;;)
			{
				yield return null;
				if (!this.shuttingDownMultiplayerServices)
				{
					break;
				}
				if (this.IsSafeToConnect())
				{
					goto Block_9;
				}
				if (connectionManager == null)
				{
					goto IL_018D;
				}
				ProtocolManager.NetworkType currentMode = connectionManager.CurrentMode;
				if (currentMode == ProtocolManager.NetworkType.None || currentMode == ProtocolManager.NetworkType.OfflineServer)
				{
					goto IL_018D;
				}
				if (this.IsSafeToDisconnect())
				{
					goto Block_12;
				}
			}
			Log.Warning(string.Format("Cancelled Shutting Down Multiplayer Services ({0}) because already shutting down.", _reason));
			failReasonProvided = true;
			yield break;
			Block_9:
			Log.Warning(string.Format("Cancelled Shutting Down Multiplayer Services ({0}) because safe to connect.", _reason));
			failReasonProvided = true;
			yield break;
			IL_018D:
			Log.Warning(string.Format("Cancelled Shutting Down Multiplayer Services ({0}) because no online connection.", _reason));
			failReasonProvided = true;
			yield break;
			Block_12:
			Log.Out(string.Format("Shutting Down Multiplayer Services ({0})...", _reason));
			if (connectionManager.IsClient)
			{
				this.Disconnect();
				isClient = true;
				success = true;
				yield break;
			}
			ClientInfo[] clientInfos = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List.ToArray<ClientInfo>();
			if (clientInfos.Length != 0)
			{
				NetPackagePlayerDenied netPackagePlayerDenied = NetPackageManager.GetPackage<NetPackagePlayerDenied>().Setup(new GameUtils.KickPlayerData(GameUtils.EKickReason.SessionClosed, 0, default(DateTime), ""));
				ClientInfo[] array = clientInfos;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SendPackage(netPackagePlayerDenied);
				}
				yield return new WaitForSecondsRealtime(1f);
				foreach (ClientInfo clientInfo in clientInfos)
				{
					try
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.DisconnectClient(clientInfo, false, false);
					}
					catch (Exception ex)
					{
						Log.Warning(string.Format("Failed to disconnect client '{0}' : {1}", clientInfo.playerName, ex));
					}
				}
			}
			this.ShutdownMultiplayerServicesNow();
			connectionManager.MakeServerOffline();
			GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, 1);
			success = true;
			connectionManager = null;
			clientInfos = null;
		}
		finally
		{
			this.shuttingDownMultiplayerServices = false;
			if (success)
			{
				Log.Out(string.Format("Multiplayer Services ({0}) have been shut down.", _reason));
				string text = Localization.Get(isClient ? "multiShut_titleClient" : "multiShut_titleHost", false, null);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat(Localization.Get("auth_reason", false, null), Localization.Get(GameManager.GetLocalizationKey(_reason), false, null));
				if (!isClient)
				{
					stringBuilder.Append('\n');
					stringBuilder.Append(Localization.Get("multiShut_commonHost", false, null));
				}
				XUiC_MessageBoxWindowGroup.ShowOk(this.windowManager.playerUI.xui, text, stringBuilder.ToString(), new Action(this.OnShutdownMultiplayerServicesMessageBoxClosed), true, true, false);
			}
			else if (!failReasonProvided)
			{
				Log.Warning(string.Format("Failed Shutting Down Multiplayer Services ({0}).", _reason));
			}
		}
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnShutdownMultiplayerServicesMessageBoxClosed()
	{
		if (this.World != null)
		{
			this.Pause(false);
		}
	}

	public void CreateStabilityViewer()
	{
		if (this.stabilityViewer == null)
		{
			this.stabilityViewer = new StabilityViewer();
		}
	}

	public void ClearStabilityViewer()
	{
		if (this.stabilityViewer != null)
		{
			this.stabilityViewer.worldIsReady = false;
			this.stabilityViewer.Clear();
			this.stabilityViewer = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLocalPlayerEntity(EntityPlayerLocal _playerEntity)
	{
		_playerEntity.IsFlyMode.Value = this.IsEditMode();
		_playerEntity.SetEntityName(GamePrefs.GetString(EnumGamePrefs.PlayerName));
		this.myPlayerId = _playerEntity.entityId;
		this.myEntityPlayerLocal = _playerEntity;
		this.persistentLocalPlayer = this.getPersistentPlayerData(null);
		_playerEntity.persistentPlayerData = this.persistentLocalPlayer;
		_playerEntity.InventoryChangedEvent += this.LocalPlayerInventoryChanged;
		_playerEntity.inventory.OnToolbeltItemsChangedInternal += delegate
		{
			this.sendPlayerToolbelt = true;
		};
		_playerEntity.bag.OnBackpackItemsChangedInternal += delegate
		{
			this.sendPlayerBag = true;
		};
		_playerEntity.equipment.OnChanged += delegate
		{
			this.sendPlayerEquipment = true;
		};
		_playerEntity.DragAndDropItemChanged += delegate
		{
			this.sendDragAndDropItem = true;
		};
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.persistentPlayers != null)
		{
			if (this.persistentLocalPlayer == null)
			{
				this.persistentLocalPlayer = this.persistentPlayers.CreatePlayerData(this.getPersistentPlayerID(null), PlatformManager.NativePlatform.User.PlatformUserId, _playerEntity.EntityName, DeviceFlag.StandaloneWindows.ToPlayGroup());
				this.persistentLocalPlayer.EntityId = this.myPlayerId;
				this.persistentPlayers.MapPlayer(this.persistentLocalPlayer);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerState>().Setup(this.persistentLocalPlayer, EnumPersistentPlayerDataReason.New), true, -1, -1, -1, null, 192, false);
				this.persistentPlayers.SavePersistentPlayerData();
			}
			else
			{
				this.persistentLocalPlayer.Update(PlatformManager.NativePlatform.User.PlatformUserId, new AuthoredText(_playerEntity.EntityName, this.persistentLocalPlayer.PrimaryId), DeviceFlag.StandaloneWindows.ToPlayGroup());
				this.persistentLocalPlayer.EntityId = this.myPlayerId;
				this.persistentPlayers.MapPlayer(this.persistentLocalPlayer);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerState>().Setup(this.persistentLocalPlayer, EnumPersistentPlayerDataReason.Login), true, -1, -1, -1, null, 192, false);
			}
		}
		this.m_World.SetLocalPlayer(_playerEntity);
		LocalPlayerUI.DispatchNewPlayerForUI(_playerEntity);
		if (this.OnLocalPlayerChanged != null)
		{
			this.OnLocalPlayerChanged(_playerEntity);
		}
		GameSenseManager instance = GameSenseManager.Instance;
		if (instance == null)
		{
			return;
		}
		instance.SessionStarted(_playerEntity);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator StartAsServer(bool _offline)
	{
		while (XUiC_WorldGenerationWindow.WorldBuilderIsGenerating)
		{
			yield return null;
		}
		Log.Out("StartAsServer");
		if (!SandboxOptionManager.HasInstance)
		{
			Log.Warning("Sandbox Option Manager not initialized before starting server, this may cause issues with some settings such as crossplay");
		}
		SandboxOptionManager.Current.LoadOptionsFromCode(GamePrefs.GetString(EnumGamePrefs.SandboxCode));
		this.gameStateManager.InitGame(true);
		SandboxOptionManager.Current.UpdateInGameValuesWithSandboxOptions(true);
		GameServerInfo.PrepareLocalServerInfo();
		this.CalculatePersistentPlayerCount(GamePrefs.GetString(EnumGamePrefs.GameWorld), GamePrefs.GetString(EnumGamePrefs.GameName), (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType));
		PlatformManager.MultiPlatform.RichPresence.UpdateRichPresence(IRichPresence.PresenceStates.Loading);
		XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, Localization.Get("uiLoadLoadingXml", false, null), null, true, true, false);
		yield return null;
		WorldStaticData.Cleanup(null);
		Block.nameIdMapping = null;
		ItemClass.nameIdMapping = null;
		string @string = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		if (!@string.Equals("Empty") && !@string.Equals("Playtesting"))
		{
			string text = GameIO.GetSaveGameDir() + "/main.ttw";
			string text2 = GameIO.GetSaveGameDir() + "/" + global::Constants.cFileBlockMappings;
			string text3 = GameIO.GetSaveGameDir() + "/" + global::Constants.cFileItemMappings;
			if (!SdFile.Exists(text))
			{
				if (!SdDirectory.Exists(GameIO.GetSaveGameDir()))
				{
					SdDirectory.CreateDirectory(GameIO.GetSaveGameDir());
				}
				Block.nameIdMapping = new NameIdMapping(text2, Block.MAX_BLOCKS);
				Block.nameIdMapping.WriteToFile();
				ItemClass.nameIdMapping = new NameIdMapping(text3, ItemClass.MAX_ITEMS);
				ItemClass.nameIdMapping.WriteToFile();
			}
			else
			{
				Block.nameIdMapping = new NameIdMapping(text2, Block.MAX_BLOCKS);
				if (!Block.nameIdMapping.LoadFromFile())
				{
					Log.Warning("Could not load block-name-mappings file '" + text2 + "'!");
					Block.nameIdMapping = null;
				}
				ItemClass.nameIdMapping = new NameIdMapping(text3, ItemClass.MAX_ITEMS);
				if (!ItemClass.nameIdMapping.LoadFromFile())
				{
					Log.Warning("Could not load item-name-mappings file '" + text3 + "'!");
					ItemClass.nameIdMapping = null;
				}
			}
		}
		yield return WorldStaticData.LoadAllXmlsCo(false, null, null);
		yield return null;
		SingletonMonoBehaviour<ConnectionManager>.Instance.ServerReady();
		Manager.CreateServer();
		LightManager.CreateServer();
		yield return null;
		PowerManager.Instance.LoadPowerManager();
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadCreatingWorld", false, null), true);
		yield return null;
		if (this.isEditMode)
		{
			this.persistentPlayers = new PersistentPlayerList();
		}
		else
		{
			this.persistentPlayers = PersistentPlayerList.ReadXML(GameIO.GetSaveGameDir() + "/players.xml");
			if (this.persistentPlayers != null && this.persistentPlayers.CleanupPlayers())
			{
				this.persistentPlayers.SavePersistentPlayerData();
			}
		}
		string text4;
		PathAbstractions.AbstractedLocation abstractedLocation;
		PathAbstractions.Contextual.FindActiveWorld(out text4, out abstractedLocation);
		yield return this.createWorld(text4, abstractedLocation, GamePrefs.GetString(EnumGamePrefs.GameName), false);
		GameServerInfo.SetLocalServerWorldInfo();
		NetPackageWorldInfo.PrepareWorldHashes();
		yield return null;
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadCreatingPlayer", false, null), true);
		yield return null;
		if (!GameManager.IsDedicatedServer)
		{
			GameManager.<>c__DisplayClass167_0 CS$<>8__locals1 = new GameManager.<>c__DisplayClass167_0();
			CS$<>8__locals1.persistentPlayerId = this.getPersistentPlayerID(null).CombinedString;
			if (!GamePrefs.GetBool(EnumGamePrefs.SkipSpawnButton) && !this.IsEditMode())
			{
				this.canSpawnPlayer = false;
				bool flag = !PlayerDataFile.Exists(GameIO.GetPlayerDataDir(), CS$<>8__locals1.persistentPlayerId);
				XUiC_SpawnSelectionWindow.Open(LocalPlayerUI.primaryUI, false, true, flag);
				while (!this.canSpawnPlayer)
				{
					yield return null;
				}
				yield return new WaitForSeconds(0.1f);
			}
			PlayerDataFile playerDataFile = new PlayerDataFile();
			playerDataFile.Load(GameIO.GetPlayerDataDir(), CS$<>8__locals1.persistentPlayerId);
			EntityCreationData entityCreationData = new EntityCreationData();
			Vector3 vector;
			Vector3 vector2;
			int num2;
			if (playerDataFile.bLoaded)
			{
				vector = playerDataFile.ecd.pos;
				vector2..ctor(playerDataFile.ecd.rot.x, playerDataFile.ecd.rot.y, 0f);
				if (this.isEditMode)
				{
					playerDataFile.id = -1;
				}
				int num;
				if (playerDataFile.id == -1)
				{
					EntityFactory.nextEntityID = (num = EntityFactory.nextEntityID) + 1;
				}
				else
				{
					num = playerDataFile.id;
				}
				num2 = num;
				entityCreationData.entityData = playerDataFile.ecd.entityData;
				entityCreationData.readFileVersion = playerDataFile.ecd.readFileVersion;
			}
			else
			{
				SpawnPosition spawnPosition = this.GetSpawnPointList().GetRandomSpawnPosition(this.m_World, null, 0, 0);
				if (this.m_World.IsRandomWorld())
				{
					DynamicPrefabDecorator dynamicPrefabDecorator = this.GetDynamicPrefabDecorator();
					if (dynamicPrefabDecorator != null)
					{
						PrefabInstance closestPOIToWorldPos = dynamicPrefabDecorator.GetClosestPOIToWorldPos(QuestEventManager.traderTag, Vector3.zero, null, -1, false, BiomeFilterTypes.OnlyBiome, BiomeDefinition.BiomeNames[3], "traderquest");
						if (closestPOIToWorldPos != null)
						{
							spawnPosition = this.GetSpawnPointList().GetRandomSpawnPosition(this.m_World, new Vector3?(closestPOIToWorldPos.GetAABB().center), 250, 750);
							Vector3.Distance(spawnPosition.position, closestPOIToWorldPos.GetAABB().center);
						}
					}
				}
				vector = spawnPosition.position;
				vector2..ctor(0f, spawnPosition.heading, 0f);
				num2 = EntityFactory.nextEntityID++;
			}
			if (playerDataFile.bLoaded && playerDataFile.ecd.playerProfile != null && GamePrefs.GetBool(EnumGamePrefs.PersistentPlayerProfiles))
			{
				entityCreationData.entityClass = EntityClass.FromString(playerDataFile.ecd.playerProfile.EntityClassName);
				entityCreationData.playerProfile = playerDataFile.ecd.playerProfile;
			}
			else
			{
				entityCreationData.playerProfile = PlayerProfile.LoadLocalProfile();
				entityCreationData.entityClass = EntityClass.FromString(entityCreationData.playerProfile.EntityClassName);
			}
			entityCreationData.skinTexture = GamePrefs.GetString(EnumGamePrefs.OptionsPlayerModelTexture);
			entityCreationData.id = num2;
			entityCreationData.pos = vector;
			entityCreationData.rot = vector2;
			entityCreationData.belongsPlayerId = num2;
			EntityPlayerLocal entityPlayerLocal = (EntityPlayerLocal)EntityFactory.CreateEntity(entityCreationData);
			this.setLocalPlayerEntity(entityPlayerLocal);
			if (playerDataFile.bLoaded)
			{
				playerDataFile.ToPlayer(entityPlayerLocal);
				entityPlayerLocal.bPreferFirstPerson = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxDefaultFirstPersonCamera);
				entityPlayerLocal.SetFirstPersonView(true, false);
			}
			this.m_World.SpawnEntityInWorld(entityPlayerLocal);
			this.myEntityPlayerLocal.Respawn(playerDataFile.bLoaded ? RespawnType.LoadedGame : RespawnType.NewGame);
			this.myEntityPlayerLocal.ChunkObserver = this.m_World.m_ChunkManager.AddChunkObserver(this.myEntityPlayerLocal.GetPosition(), true, Utils.FastMin(12, GameUtils.GetViewDistance()), -1);
			IMapChunkDatabase.TryCreateOrLoad(this.myEntityPlayerLocal.entityId, out this.myEntityPlayerLocal.ChunkObserver.mapDatabase, () => new IMapChunkDatabase.DirectoryPlayerId(GameIO.GetPlayerDataDir(), CS$<>8__locals1.persistentPlayerId));
			LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
			uiforPlayer.xui.SetDataConnections();
			uiforPlayer.xui.SetCraftingData(playerDataFile.craftingData);
			CS$<>8__locals1 = null;
		}
		Log.Out("Loaded player");
		yield return null;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			VehicleManager.Init();
			DroneManager.Init();
			TurretTracker.Init();
			RaycastPathManager.Init();
			TokenManager.Init();
			BlockLimitTracker.Init();
			ChunkProviderGenerateWorld chunkProviderGenerateWorld = this.m_World.ChunkCache.ChunkProvider as ChunkProviderGenerateWorld;
			if (chunkProviderGenerateWorld != null)
			{
				chunkProviderGenerateWorld.CheckPersistentData();
			}
		}
		yield return null;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.m_World.ChunkCache.IsFixedSize && !this.IsEditMode() && this.m_World.m_WorldEnvironment != null)
		{
			this.m_World.m_WorldEnvironment.SetColliders((float)((this.m_World.ChunkCache.ChunkMinPos.x + 1) * 16), (float)((this.m_World.ChunkCache.ChunkMinPos.y + 1) * 16), (float)((this.m_World.ChunkCache.ChunkMaxPos.x - this.m_World.ChunkCache.ChunkMinPos.x - 1) * 16), (float)((this.m_World.ChunkCache.ChunkMaxPos.y - this.m_World.ChunkCache.ChunkMinPos.y - 1) * 16), global::Constants.cSizePlanesAround, 0f);
			this.m_World.m_WorldEnvironment.CreateLevelBorderBox(this.m_World);
		}
		if (this.isEditMode)
		{
			PrefabEditModeManager instance = PrefabEditModeManager.Instance;
			if (instance != null)
			{
				instance.Init();
			}
			yield return null;
		}
		yield return null;
		if (GameManager.IsDedicatedServer || !_offline)
		{
			ServerInformationTcpProvider.Instance.StartServer();
			IMasterServerAnnouncer serverListAnnouncer = PlatformManager.MultiPlatform.ServerListAnnouncer;
			if (serverListAnnouncer != null)
			{
				serverListAnnouncer.AdvertiseServer(delegate
				{
					ILobbyHost lobbyHost = PlatformManager.NativePlatform.LobbyHost;
					if (lobbyHost != null)
					{
						lobbyHost.UpdateLobby(SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo);
					}
					ModEvents.SServerRegisteredData sserverRegisteredData;
					ModEvents.ServerRegistered.Invoke(ref sserverRegisteredData);
				});
			}
			PlayerInteractions.Instance.JoinedMultiplayerServer(this.persistentPlayers);
			AuthorizationManager.Instance.ServerStart();
		}
		else
		{
			GamePrefs.Set(EnumGamePrefs.ServerMaxPlayerCount, 1);
		}
		yield return GCUtils.UnloadAndCollectCo();
		this.LogServerStartEventAnalytics(_offline);
		this.gameStateManager.StartGame();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogServerStartEventAnalytics(bool isOffline)
	{
		SandboxOptionManager sandboxOptionManager = SandboxOptionManager.Current;
		SandboxOptionPreset preset = sandboxOptionManager.GetPreset(GamePrefs.GetString(EnumGamePrefs.SandboxPreset));
		List<ValueTuple<string, string, bool>> list;
		sandboxOptionManager.GetChangedPresetOptions(preset, out list);
		Dictionary<string, object> dictionary = new Dictionary<string, object>
		{
			{
				"Group",
				(preset != null) ? preset.Group : null
			},
			{
				"Preset",
				(preset != null) ? preset.Name : null
			},
			{
				"Server_Enabled",
				GamePrefs.GetBool(EnumGamePrefs.ServerEnabled)
			},
			{
				"Region",
				GamePrefs.GetString(EnumGamePrefs.Region)
			},
			{
				"Server_Visibility",
				((EnumServerVisibility)GamePrefs.GetInt(EnumGamePrefs.ServerVisibility)).ToString()
			},
			{
				"EAC_Protected",
				GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer)
			},
			{
				"Crossplay_Enabled",
				GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay)
			},
			{
				"Max_Players",
				GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount)
			},
			{
				"Chunk_Reset_Timer",
				GamePrefs.GetInt(EnumGamePrefs.MaxChunkAge)
			},
			{
				"Reset_Unprotected_Chunks",
				((EnumResetUnprotectedChunksGroupingMode)GamePrefs.GetInt(EnumGamePrefs.ResetUnprotectedChunks)).ToString()
			},
			{
				"Creative_Mode",
				GamePrefs.GetBool(EnumGamePrefs.BuildCreate)
			},
			{
				"Persistent_Profiles",
				GamePrefs.GetBool(EnumGamePrefs.PersistentPlayerProfiles)
			},
			{
				"Restrict_Camera_Mode",
				((EnumCameraRestriction)GamePrefs.GetInt(EnumGamePrefs.CameraRestrictionMode)).ToString()
			},
			{
				"Player_Killing",
				((EnumPlayerKillingMode)GamePrefs.GetInt(EnumGamePrefs.PlayerKillingMode)).ToString()
			},
			{
				"Claim_Size",
				GamePrefs.GetInt(EnumGamePrefs.LandClaimSize)
			},
			{
				"Claim_Deadzone",
				GamePrefs.GetInt(EnumGamePrefs.LandClaimDeadZone)
			},
			{
				"Claim_Duration",
				GamePrefs.GetInt(EnumGamePrefs.LandClaimExpiryTime)
			},
			{
				"Claim_Decay_Mode",
				GamePrefs.GetInt(EnumGamePrefs.LandClaimDecayMode)
			},
			{
				"Claim_Health_Online",
				GamePrefs.GetInt(EnumGamePrefs.LandClaimOnlineDurabilityModifier)
			},
			{
				"Claim_Health_Offline",
				GamePrefs.GetInt(EnumGamePrefs.LandClaimOfflineDurabilityModifier)
			},
			{
				"Bedroll_Deadzone",
				GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize)
			},
			{
				"Bedroll_Duration",
				GamePrefs.GetInt(EnumGamePrefs.BedrollExpiryTime)
			},
			{
				"Party_Shared_Kill_Range",
				GamePrefs.GetInt(EnumGamePrefs.PartySharedKillRange)
			}
		};
		ServerStartEventData serverStartEventData = new ServerStartEventData();
		GameServerInfo currentGameServerInfoServerOrClient = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentGameServerInfoServerOrClient;
		serverStartEventData.ServerId = ((currentGameServerInfoServerOrClient != null) ? currentGameServerInfoServerOrClient.GetValue(GameInfoString.UniqueId) : null);
		serverStartEventData.SaveId = this.World.Guid;
		serverStartEventData.ServerStartTimestamp = DateTime.UtcNow.ToString("o");
		serverStartEventData.ServerType = (GameManager.IsDedicatedServer ? "Dedicated" : (isOffline ? "Offline" : "Listen"));
		serverStartEventData.SandboxSeed = GameStats.GetString(EnumGameStats.SandboxCode);
		serverStartEventData.SandboxSettingsDelta = list;
		serverStartEventData.WorldName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		serverStartEventData.GeneralSettings = dictionary;
		serverStartEventData.ServerMods = ModManager.GetLoadedMods();
		ServerStartEventData serverStartEventData2 = serverStartEventData;
		ServiceProvider.Instance.Get<IAnalyticsService>().LogEvent(serverStartEventData2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartAsClient()
	{
		Log.Out("StartAsClient");
		this.worldCreated = false;
		this.chunkClusterLoaded = false;
		this.worldInitInfoReceived = false;
		GamePrefs.Set(EnumGamePrefs.GameMode, string.Empty);
		GamePrefs.Set(EnumGamePrefs.GameWorld, string.Empty);
		WorldStaticData.WaitForConfigsFromServer();
		PlatformManager.MultiPlatform.RichPresence.UpdateRichPresence(IRichPresence.PresenceStates.Connecting);
		IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
		if (antiCheatClient == null || !antiCheatClient.ClientAntiCheatEnabled())
		{
			Log.Out("Sending RequestToEnterGame...");
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageRequestToEnterGame>(), false);
		}
		else
		{
			PlatformManager.MultiPlatform.AntiCheatClient.WaitForRemoteAuth(delegate
			{
				Log.Out("Sending RequestToEnterGame...");
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageRequestToEnterGame>(), false);
			});
		}
		BlockLimitTracker.Init();
	}

	public bool IsSafeToConnect()
	{
		return SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode == ProtocolManager.NetworkType.None;
	}

	public bool IsSafeToDisconnect()
	{
		return SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode == ProtocolManager.NetworkType.None || ((!PrefabEditModeManager.Instance.IsActive() || !PrefabEditModeManager.Instance.NeedsSaving) && (this.gameStateManager.IsGameStarted() && !this.IsStartingGame) && !this.isDisconnectingLater);
	}

	public void Disconnect()
	{
		Log.Out("Disconnect");
		if (!GameManager.IsDedicatedServer)
		{
			this.windowManager.CloseAllOpenModalWindows(null, false);
			if (this.m_World != null)
			{
				List<EntityPlayerLocal> localPlayers = this.m_World.GetLocalPlayers();
				for (int i = 0; i < localPlayers.Count; i++)
				{
					LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(localPlayers[i]);
					if (null != uiforPlayer && null != uiforPlayer.windowManager)
					{
						uiforPlayer.windowManager.CloseAllOpenModalWindows(null, false);
						uiforPlayer.xui.gameObject.SetActive(false);
					}
				}
			}
			LocalPlayerUI.primaryUI.windowManager.Close(XUiC_SubtitlesDisplay.ID);
			Manager.StopAllLocal();
		}
		this.Pause(false);
		if (!GameManager.IsDedicatedServer)
		{
			if (!this.isEditMode && this.myEntityPlayerLocal)
			{
				GameSenseManager instance = GameSenseManager.Instance;
				if (instance != null)
				{
					instance.SessionEnded();
				}
				if (this.myEntityPlayerLocal.AttachedToEntity)
				{
					this.myEntityPlayerLocal.Detach();
				}
				this.myEntityPlayerLocal.FireEvent(MinEventTypes.onSelfLeaveGame, true);
				this.myEntityPlayerLocal.dropItemOnQuit();
			}
			this.triggerEffectManager.StopGamepadVibration(true);
			PlatformManager.MultiPlatform.User.StopAdvertisePlaying();
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerDisconnect>().Setup(this.myEntityPlayerLocal), true);
			base.StartCoroutine(this.disconnectLater());
		}
		else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.StopServers();
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.DisconnectFromServer();
		}
		if (GameSparksManager.Instance() != null)
		{
			GameSparksManager.Instance().SessionEnded();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator disconnectLater()
	{
		this.isDisconnectingLater = true;
		yield return new WaitForSeconds(0.2f);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
		GamePrefs.Set(EnumGamePrefs.GameGuidClient, "");
		this.isDisconnectingLater = false;
		yield break;
	}

	public void SaveAndCleanupWorld()
	{
		Log.Out("SaveAndCleanupWorld");
		World world = this.m_World;
		if (world != null)
		{
			EntityAsyncManager entityAsyncManager = world.entityAsyncManager;
			if (entityAsyncManager != null)
			{
				entityAsyncManager.CompletePendingCreateTasks();
			}
		}
		ModEvents.SWorldShuttingDownData sworldShuttingDownData;
		ModEvents.WorldShuttingDown.Invoke(ref sworldShuttingDownData);
		this.shuttingDownMultiplayerServices = false;
		PathAbstractions.CacheEnabled = false;
		this.OnClientSpawned = null;
		PlayerInputRecordingSystem.Instance.AutoSave();
		this.gameStateManager.EndGame();
		PlatformManager.MultiPlatform.RichPresence.UpdateRichPresence(IRichPresence.PresenceStates.Menu);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && GameManager.bSavingActive && !this.IsEditMode())
		{
			if (VehicleManager.Instance != null)
			{
				VehicleManager.Instance.RemoveAllVehiclesFromMap();
			}
			if (DroneManager.Instance != null)
			{
				DroneManager.Instance.RemoveAllDronesFromMap();
			}
			if (QuestEventManager.HasInstance)
			{
				QuestEventManager.Current.HandleAllPlayersDisconnect();
			}
			this.SaveLocalPlayerData();
			this.SaveWorld();
			World world2 = this.m_World;
			EntityPlayerLocal entityPlayerLocal = ((world2 != null) ? world2.GetPrimaryPlayer() : null);
			if (this.persistentPlayers != null)
			{
				foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in this.persistentPlayers.Players)
				{
					if (keyValuePair.Value.EntityId != -1)
					{
						if (entityPlayerLocal && keyValuePair.Value.EntityId == entityPlayerLocal.entityId)
						{
							keyValuePair.Value.Position = new Vector3i(entityPlayerLocal.position);
						}
						keyValuePair.Value.LastLogin = DateTime.Now;
					}
				}
				this.persistentPlayers.SavePersistentPlayerData();
			}
		}
		if (Block.nameIdMapping != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Block.nameIdMapping.SaveIfDirty(true);
			}
			Block.nameIdMapping = null;
		}
		if (ItemClass.nameIdMapping != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ItemClass.nameIdMapping.SaveIfDirty(true);
			}
			ItemClass.nameIdMapping = null;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && GameManager.bSavingActive && !this.IsEditMode())
		{
			if (this.m_World != null && this.m_World.GetPrimaryPlayer() != null && this.m_World.GetPrimaryPlayer().ChunkObserver.mapDatabase != null)
			{
				ThreadManager.AddSingleTask(new ThreadManager.TaskFunctionDelegate(this.m_World.GetPrimaryPlayer().ChunkObserver.mapDatabase.SaveAsync), new IMapChunkDatabase.DirectoryPlayerId(GameIO.GetPlayerDataLocalDir(), this.persistentLocalPlayer.PrimaryId.CombinedString), null, true);
			}
			if (!GameManager.IsDedicatedServer && this.m_World != null)
			{
				foreach (EntityPlayerLocal entityPlayerLocal2 in this.m_World.GetLocalPlayers())
				{
					entityPlayerLocal2.EnableCamera(false);
					entityPlayerLocal2.SetControllable(false);
				}
			}
		}
		this.ShutdownMultiplayerServicesNow();
		PlayerInteractions.Instance.OnNewPlayerInteraction -= this.HandleFirstSpawnInteractions;
		PlayerInteractions.Instance.Shutdown();
		IGameplayNotifier gameplayNotifier = PlatformManager.NativePlatform.GameplayNotifier;
		if (gameplayNotifier != null)
		{
			gameplayNotifier.GameplayEnd();
		}
		if (!GameManager.IsDedicatedServer)
		{
			if (this.myEntityPlayerLocal != null)
			{
				this.myEntityPlayerLocal.EnableCamera(false);
				this.myEntityPlayerLocal.SetControllable(false);
				if (this.OnLocalPlayerChanged != null)
				{
					this.OnLocalPlayerChanged(null);
				}
				this.m_World.RemoveEntity(this.myPlayerId, EnumRemoveEntityReason.Unloaded);
				this.myPlayerId = -1;
				this.myEntityPlayerLocal = null;
			}
			foreach (LocalPlayerUI localPlayerUI in LocalPlayerUI.PlayerUIs)
			{
				if (!localPlayerUI.isPrimaryUI && !localPlayerUI.IsCleanCopy)
				{
					if (localPlayerUI.entityPlayer)
					{
						localPlayerUI.entityPlayer.EnableCamera(false);
						localPlayerUI.entityPlayer.SetControllable(false);
						World world3 = this.m_World;
						if (world3 != null)
						{
							world3.RemoveEntity(localPlayerUI.entityPlayer.entityId, EnumRemoveEntityReason.Unloaded);
						}
					}
					if (localPlayerUI.gameObject)
					{
						localPlayerUI.xui.Shutdown(false);
						localPlayerUI.windowManager.CloseAllOpenModalWindows(null, false);
						Object.Destroy(localPlayerUI.gameObject);
					}
				}
			}
		}
		ModManager.GameEnded();
		if (!GameManager.IsDedicatedServer)
		{
			if (!PlatformApplicationManager.IsRestartRequired)
			{
				GameManager.LoadRemoteResources(null);
			}
			GUIWindowConsole.Close();
			this.windowManager.Close(XUiC_LoadingScreen.ID);
			if (!GameManager.bHideMainMenuNextTime)
			{
				this.windowManager.Open(XUiC_MainMenu.ID, true);
			}
			GameManager.bHideMainMenuNextTime = false;
		}
		PrefabInstanceClientManager.Instance.Cleanup();
		TrajectorySimulation.Cleanup();
		TokenManager.Cleanup();
		AstarManager.Cleanup();
		DynamicMeshManager.OnWorldUnload();
		if (GameEventManager.HasInstance)
		{
			GameEventManager.Current.Cleanup();
		}
		if (this.m_World != null)
		{
			if (this.OnWorldChanged != null)
			{
				this.OnWorldChanged(null);
			}
			this.prefabLODManager.Cleanup();
			PrefabEditModeManager instance = PrefabEditModeManager.Instance;
			if (instance != null && instance.IsActive())
			{
				PrefabEditModeManager instance2 = PrefabEditModeManager.Instance;
				if (instance2 != null)
				{
					instance2.Cleanup();
				}
			}
			EnvironmentAudioManager.DestroyInstance();
			LightManager.Clear();
			SkyManager.Cleanup();
			WeatherManager.Cleanup();
			CharacterGazeController.Cleanup();
			WaterSplashCubes.Clear();
			WaterEvaporationManager.ClearAll();
			SleeperVolumeToolManager.CleanUp();
			POIMarkerToolManager.CleanUp();
			this.ClearStabilityViewer();
			if (this.m_World.GetPrimaryPlayer() && this.m_World.GetPrimaryPlayer().DynamicMusicManager != null)
			{
				this.m_World.GetPrimaryPlayer().DynamicMusicManager.CleanUpDynamicMembers();
			}
			this.m_World.UnloadWorld(true);
			this.m_World.Cleanup();
			this.m_World = null;
			this.GameHasStarted = false;
		}
		WaterSimulationNative.Instance.Cleanup();
		ProjectileManager.Cleanup();
		VehicleManager.Cleanup();
		DroneManager.Cleanup();
		DismembermentManager.Cleanup();
		TurretTracker.Cleanup();
		BlockLimitTracker.Cleanup();
		MapObjectManager.Reset();
		vp_TargetEventHandler.UnregisterAll();
		this.lootManager = null;
		this.traderManager = null;
		if (QuestEventManager.HasInstance)
		{
			QuestEventManager.Current.Cleanup();
		}
		if (TwitchVoteScheduler.HasInstance)
		{
			TwitchVoteScheduler.Current.Cleanup();
		}
		if (TwitchManager.HasInstance)
		{
			TwitchManager.Current.Cleanup();
		}
		if (PowerManager.HasInstance)
		{
			PowerManager.Instance.Cleanup();
		}
		if (WireManager.HasInstance)
		{
			WireManager.Instance.Cleanup();
		}
		if (PartyManager.HasInstance)
		{
			PartyManager.Current.Cleanup();
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && GameManager.bSavingActive)
		{
			this.IsEditMode();
		}
		if (UIDisplayInfoManager.HasInstance)
		{
			UIDisplayInfoManager.Current.Cleanup();
		}
		if (TextureLoadingManager.Instance != null)
		{
			TextureLoadingManager.Instance.Cleanup();
		}
		if (NavObjectManager.HasInstance)
		{
			NavObjectManager.Instance.Cleanup();
		}
		SignTextureExporter.Instance.Cleanup();
		SignTextureManager.Instance.Cleanup(false);
		if (SignDataManager.HasInstance)
		{
			SignDataManager.Instance.Cleanup();
		}
		SelectionBoxManager.Instance.Clear();
		Origin.Cleanup();
		GameObjectPool.Instance.Cleanup();
		MemoryPools.Cleanup();
		VoxelMeshLayer.StaticCleanup();
		GamePrefs.Instance.Save();
		GameManager.bRecordNextSession = false;
		GameManager.bPlayRecordedSession = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShutdownMultiplayerServicesNow()
	{
		if (!GameManager.IsDedicatedServer)
		{
			PlatformManager.MultiPlatform.User.StopAdvertisePlaying();
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			AuthorizationManager.Instance.ServerStop();
		}
		ILobbyHost lobbyHost = PlatformManager.NativePlatform.LobbyHost;
		if (lobbyHost != null)
		{
			lobbyHost.ExitLobby();
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PlatformManager.MultiPlatform.ServerListAnnouncer.StopServer();
			ServerInformationTcpProvider.Instance.StopServer();
		}
		IGameplayNotifier gameplayNotifier = PlatformManager.NativePlatform.GameplayNotifier;
		if (gameplayNotifier == null)
		{
			return;
		}
		gameplayNotifier.EndOnlineMultiplayer();
	}

	public void SaveWorld()
	{
		if (this.m_World != null)
		{
			this.m_World.Save();
		}
	}

	public void SaveLocalPlayerData()
	{
		if (this.m_World == null)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = this.m_World.GetPrimaryPlayer();
		if (primaryPlayer == null || !GameManager.bSavingActive)
		{
			return;
		}
		string combinedString = this.getPersistentPlayerID(null).CombinedString;
		PlayerDataFile playerDataFile = new PlayerDataFile();
		playerDataFile.FromPlayer(primaryPlayer);
		playerDataFile.Save(GameIO.GetPlayerDataDir(), combinedString);
		if (primaryPlayer.ChunkObserver.mapDatabase != null)
		{
			ThreadManager.AddSingleTask(new ThreadManager.TaskFunctionDelegate(primaryPlayer.ChunkObserver.mapDatabase.SaveAsync), new IMapChunkDatabase.DirectoryPlayerId(GameIO.GetPlayerDataDir(), combinedString), null, true);
		}
	}

	public void Cleanup()
	{
		Log.Out("Cleanup");
		WaterSimulationNative.Instance.Cleanup();
		ModEvents.SGameShutdownData sgameShutdownData;
		ModEvents.GameShutdown.Invoke(ref sgameShutdownData);
		AuthorizationManager.Instance.Cleanup();
		VehicleManager.Cleanup();
		Cursor.visible = true;
		Cursor.lockState = SoftCursor.DefaultCursorLockState;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Cleanup();
		WorldStaticData.Cleanup();
		this.adminTools = null;
		GUIWindowConsole.Shutdown();
		GameObjectPool.Instance.Cleanup();
		SaveDataUtils.SaveDataManager.Cleanup();
		LocalPlayerManager.Destroy();
		PlatformManager.Destroy();
		LoadManager.Destroy();
		TaskManager.Destroy();
		MemoryPools.Cleanup();
		GC.Collect();
	}

	public bool IsQuitting
	{
		get
		{
			return this.isQuitting;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool OnApplicationQuit()
	{
		AdminTools adminTools = this.adminTools;
		if (adminTools != null)
		{
			adminTools.DestroyFileWatcher();
		}
		if (!this.allowQuit)
		{
			if (!this.isQuitting)
			{
				this.isQuitting = true;
				base.StartCoroutine(this.ApplicationQuitCo(0.3f));
			}
			return false;
		}
		GameSenseManager instance = GameSenseManager.Instance;
		if (instance != null)
		{
			instance.Cleanup();
		}
		ThreadManager.Shutdown();
		WorldStaticData.QuitCleanup();
		if (SingletonMonoBehaviour<SdtdConsole>.Instance != null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Cleanup();
		}
		Log.Out("OnApplicationQuit");
		return true;
	}

	public void OnApplicationFocus(bool _focus)
	{
		if (!GameManager.IsDedicatedServer)
		{
			this.GameIsFocused = _focus;
			if (Application.isEditor)
			{
				return;
			}
			if (!_focus)
			{
				this.setCursorEnabled(true);
			}
			else if (this.bCursorVisibleOverride)
			{
				this.setCursorEnabled(this.bCursorVisibleOverrideState);
			}
			else if (!this.isAnyCursorWindowOpen(null))
			{
				this.setCursorEnabled(false);
			}
			if (ActionSetManager.DebugLevel != ActionSetManager.EDebugLevel.Off)
			{
				Log.Out("Focus: " + _focus.ToString());
				Log.Out("Input state:");
				foreach (PlayerActionsBase playerActionsBase in PlatformManager.NativePlatform.Input.ActionSets)
				{
					Log.Out(string.Format("   {0}: {1}", playerActionsBase.GetType().Name, playerActionsBase.Enabled));
				}
				Log.Out("Modal window open: " + LocalPlayerUI.PlayerUIs.Any((LocalPlayerUI ui) => ui.windowManager.IsModalWindowOpen()).ToString());
				Log.Out("Cursor window: " + this.isAnyCursorWindowOpen(null).ToString());
			}
			ModEvents.SGameFocusData sgameFocusData = new ModEvents.SGameFocusData(_focus);
			ModEvents.GameFocus.Invoke(ref sgameFocusData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isAnyModalWindowOpen()
	{
		IList<LocalPlayerUI> playerUIs = LocalPlayerUI.PlayerUIs;
		for (int i = playerUIs.Count - 1; i >= 0; i--)
		{
			if (playerUIs[i].windowManager.IsModalWindowOpen())
			{
				return true;
			}
		}
		return false;
	}

	public bool isAnyCursorWindowOpen(LocalPlayerUI _ui = null)
	{
		if (_ui == null)
		{
			IList<LocalPlayerUI> playerUIs = LocalPlayerUI.PlayerUIs;
			for (int i = 0; i < playerUIs.Count; i++)
			{
				if (playerUIs[i].windowManager.IsModalWindowOpen() || playerUIs[i].windowManager.IsCursorWindowOpen())
				{
					return true;
				}
			}
		}
		else if (_ui.windowManager.IsModalWindowOpen() || _ui.windowManager.IsCursorWindowOpen())
		{
			return true;
		}
		return false;
	}

	public void SetCursorEnabledOverride(bool _bOverrideOn, bool _bOverrideState)
	{
		if (this.bCursorVisibleOverride != _bOverrideOn)
		{
			this.bCursorVisibleOverride = _bOverrideOn;
			this.setCursorEnabled(_bOverrideState);
		}
	}

	public bool GetCursorEnabledOverride()
	{
		return this.bCursorVisibleOverride;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setCursorEnabled(bool _e)
	{
		if (this.IsQuitting)
		{
			return;
		}
		this.bCursorVisible = _e;
		if (ActionSetManager.DebugLevel == ActionSetManager.EDebugLevel.Verbose)
		{
			Log.Out("CursorEnabled: " + _e.ToString());
		}
		SoftCursor.SetCursorVisible(this.bCursorVisible);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ApplicationQuitCo(float _delay)
	{
		Log.Out("Preparing quit");
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode != ProtocolManager.NetworkType.None)
		{
			try
			{
				this.Disconnect();
			}
			catch (Exception ex)
			{
				Log.Error("Disconnecting failed:");
				Log.Exception(ex);
			}
			yield return new WaitForSeconds(_delay);
		}
		if (!GameManager.IsDedicatedServer)
		{
			this.windowManager.CloseAllOpenModalWindows(null, false);
			this.windowManager.playerUI.xui.StopAllVideo();
		}
		GamePrefs.Instance.Save();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.StopServers();
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
		}
		this.Cleanup();
		yield return new WaitForSeconds(0.05f);
		this.allowQuit = true;
		Application.Quit();
		yield break;
	}

	public void ShowMessagePlayerDenied(GameUtils.KickPlayerData _kickData)
	{
		Log.Out("[NET] Kicked from server: " + _kickData.ToString());
		XUiC_MessageBoxWindowGroup.ShowOk(this.windowManager.playerUI.xui, Localization.Get("auth_messageTitle", false, null), _kickData.LocalizedMessage(), null, true, true, false);
	}

	public void ShowMessageServerAuthFailed(string _message)
	{
		Log.Out("Client failed to authorize server: " + _message);
		XUiC_MessageBoxWindowGroup.ShowOk(this.windowManager.playerUI.xui, Localization.Get("auth_serverAuthFailedTitle", false, null), _message, null, true, true, false);
	}

	public void PlayerLoginRPC(ClientInfo _cInfo, string _playerName, [TupleElementNames(new string[] { "userId", "token" })] ValueTuple<PlatformUserIdentifierAbs, string> _platformUserAndToken, [TupleElementNames(new string[] { "userId", "token" })] ValueTuple<PlatformUserIdentifierAbs, string> _crossplatformUserAndToken, string _compatibilityVersion, ulong _discordUserId)
	{
		Log.Out("PlayerLogin: " + _playerName + "/" + _compatibilityVersion);
		Log.Out("Client IP: " + _cInfo.ip);
		AuthorizationManager.Instance.Authorize(_cInfo, _playerName, _platformUserAndToken, _crossplatformUserAndToken, _compatibilityVersion, _discordUserId);
	}

	public IEnumerator RequestToEnterGame(ClientInfo _cInfo)
	{
		ModEvents.SPlayerJoinedGameData splayerJoinedGameData = new ModEvents.SPlayerJoinedGameData(_cInfo);
		ModEvents.PlayerJoinedGame.Invoke(ref splayerJoinedGameData);
		string playerName = _cInfo.playerName;
		Log.Out("RequestToEnterGame: " + _cInfo.InternalId.CombinedString + "/" + playerName);
		IPlatformUserData userData = PlatformUserManager.GetOrCreate(_cInfo.CrossplatformId);
		if (userData != null)
		{
			userData.MarkBlockedStateChanged();
			yield return PlatformUserManager.ResolveUserBlockedCoroutine(userData);
			if (userData.Blocked[EBlockType.Play].IsBlocked())
			{
				Log.Out(string.Format("Player {0} is blocked", _cInfo.InternalId));
				_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerDenied>().Setup(new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, 0, default(DateTime), "")));
				yield break;
			}
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && !this.persistentPlayerIds.Contains(_cInfo.InternalId.ToString()))
		{
			if (this.persistentPlayerCount + 1 > 100)
			{
				Log.Out("Persistent player data entries limit reached, rejecting new player {0}", new object[] { _cInfo.InternalId.ToString() });
				_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerDenied>().Setup(new GameUtils.KickPlayerData(GameUtils.EKickReason.PersistentPlayerDataExceeded, 0, default(DateTime), "")));
				yield break;
			}
			this.persistentPlayerIds.Add(_cInfo.InternalId.ToString());
		}
		PersistentPlayerList ppList = ((this.persistentPlayers != null) ? this.persistentPlayers.NetworkCloneRelevantForPlayer() : null);
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageIdMapping>().Setup("blocks", Block.fullMappingDataForClients));
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageIdMapping>().Setup("items", ItemClass.fullMappingDataForClients));
		yield return NetPackageLocalization.StartSendingPacketsToClient(_cInfo);
		WorldStaticData.SendXmlsToClient(_cInfo);
		PlatformUserIdentifierAbs persistentPlayerID = this.getPersistentPlayerID(_cInfo);
		bool flag = !PlayerDataFile.Exists(GameIO.GetPlayerDataDir(), persistentPlayerID.CombinedString);
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageWorldInfo>().Setup(GamePrefs.GetString(EnumGamePrefs.GameMode), GamePrefs.GetString(EnumGamePrefs.GameWorld), GamePrefs.GetString(EnumGamePrefs.GameName), this.m_World.Guid, ppList, GameTimer.Instance.ticks, this.m_World.ChunkCache.IsFixedSize, flag));
		DecoManager.Instance.SendDecosToClient(_cInfo);
		ChunkCluster chunkCache = this.m_World.ChunkCache;
		if (chunkCache != null)
		{
			_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChunkClusterInfo>().Setup(chunkCache));
		}
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageWorldSpawnPoints>().Setup(this.GetSpawnPointList()));
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageWorldAreas>().Setup(this.m_World.TraderAreas));
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameStats>().Setup(GameStats.Instance));
		yield break;
	}

	public void WorldInfo(string _gameMode, string _levelName, string _gameName, string _guid, PersistentPlayerList _playerList, ulong _ticks, bool _fixedSizeCC, bool _firstTimeJoin, Dictionary<string, uint> _worldFileHashes, long _worldDataSize)
	{
		Log.Out("Received game GUID: " + _guid);
		GamePrefs.Set(EnumGamePrefs.GameMode, _gameMode);
		GameIO.SetSaveGameLocalGuid(_guid);
		GamePrefs.Set(EnumGamePrefs.GameWorld, _levelName);
		this.persistentPlayers = _playerList;
		base.StartCoroutine(this.worldInfoCo(_levelName, _gameName, _fixedSizeCC, _firstTimeJoin, _worldFileHashes, _worldDataSize));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator worldInfoCo(string _levelName, string _gameName, bool _fixedSizeCC, bool _firstTimeJoin, Dictionary<string, uint> _worldFileHashes, long _worldDataSize)
	{
		while (!WorldStaticData.AllConfigsReceivedAndLoaded())
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
			{
				yield break;
			}
			yield return null;
		}
		GeneratedTextManager.PrefilterText(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.ServerLoginConfirmationText, GeneratedTextManager.TextFilteringMode.Filter);
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadCreatingWorld", false, null), true);
		yield return null;
		string dataDir = GameIO.GetSaveGameLocalDir();
		string rwiFilename = Path.Combine(dataDir, "RemoteWorldInfo.xml");
		bool downloadWorld = false;
		PathAbstractions.AbstractedLocation worldLocation = PathAbstractions.AbstractedLocation.None;
		List<PathAbstractions.AbstractedLocation> availablePathsList = PathAbstractions.WorldsSearchPaths.GetAvailablePathsList(null, false, null);
		foreach (PathAbstractions.AbstractedLocation possibleWorld in availablePathsList)
		{
			if (possibleWorld.Type == PathAbstractions.EAbstractedLocationType.LocalSave && SdFile.Exists(possibleWorld.FullPath + "/completed"))
			{
				worldLocation = possibleWorld;
				break;
			}
			if (possibleWorld.Type != PathAbstractions.EAbstractedLocationType.None)
			{
				if (possibleWorld.Name.Equals(_levelName))
				{
					bool worldValid = true;
					yield return NetPackageWorldFolder.TestWorldValid(possibleWorld.FullPath, _worldFileHashes, delegate(bool _valid)
					{
						worldValid = _valid;
					});
					if (worldValid)
					{
						worldLocation = possibleWorld;
						break;
					}
				}
			}
			else
			{
				possibleWorld = default(PathAbstractions.AbstractedLocation);
			}
		}
		List<PathAbstractions.AbstractedLocation>.Enumerator enumerator = default(List<PathAbstractions.AbstractedLocation>.Enumerator);
		if (worldLocation.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			Log.Out("Matching world for " + _levelName + " not found. Will download from server");
			downloadWorld = true;
		}
		else
		{
			GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, (int)worldLocation.Type);
			GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)worldLocation.StorageType);
		}
		int value = SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.GetValue(GameInfoInt.WorldSize);
		long num = SaveDataLimitUtils.CalculatePlayerMapSize(new Vector2i(value, value));
		long requiredSpace = 2048L + num;
		if (downloadWorld || worldLocation.Type == PathAbstractions.EAbstractedLocationType.LocalSave)
		{
			requiredSpace += _worldDataSize;
		}
		UserDataStorageType @int = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
		if (SaveInfoProvider.DataLimitEnabled && @int.UsesDataLimit())
		{
			long num2 = 0L;
			string @string = GamePrefs.GetString(EnumGamePrefs.GameGuidClient);
			SaveInfoProvider.SaveEntryInfo saveEntryInfo;
			if (SaveInfoProvider.Instance.TryGetRemoteSaveEntry(@string, out saveEntryInfo))
			{
				num2 = saveEntryInfo.SizeInfo.ReportedSize;
			}
			if (num2 < requiredSpace)
			{
				long num3 = requiredSpace - num2;
				string text = ((saveEntryInfo != null) ? saveEntryInfo.SaveDir : null);
				XUiC_SaveSpaceNeeded confirmationWindow = XUiC_SaveSpaceNeeded.Open(num3, text, @int, false, true, false, "xuiDmRemoteSaveTitle", "xuiDmRemoteSaveBody", null, null, "xuiStart", null);
				if (confirmationWindow != null)
				{
					while (confirmationWindow.IsOpen || confirmationWindow.Result == XUiC_SaveSpaceNeeded.ConfirmationResult.Pending)
					{
						yield return null;
					}
					if (confirmationWindow.Result != XUiC_SaveSpaceNeeded.ConfirmationResult.Confirmed)
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.Disconnect();
					}
					if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
					{
						yield break;
					}
				}
				XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, null, null, true, true, false);
				confirmationWindow = null;
			}
			else if (num2 > requiredSpace)
			{
				requiredSpace = num2;
			}
			SaveInfoProvider.Instance.ClearResources();
		}
		try
		{
			if (!SdDirectory.Exists(dataDir))
			{
				SdDirectory.CreateDirectory(dataDir);
			}
			else
			{
				SdFile.Delete(Path.Combine(dataDir, "archived.flag"));
			}
		}
		catch (Exception ex)
		{
			Log.Error("Exception creating local save dir: " + dataDir + " - GUID len: " + GamePrefs.GetString(EnumGamePrefs.GameGuidClient).Length.ToString());
			Log.Exception(ex);
			throw;
		}
		string text2 = Path.Combine(dataDir, "hosts.txt");
		string text3 = SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.GetValue(GameInfoString.IP) + ":" + SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.GetValue(GameInfoInt.Port).ToString();
		List<string> list;
		if (SdFile.Exists(text2))
		{
			list = new List<string>(SdFile.ReadAllLines(text2));
		}
		else
		{
			list = new List<string>();
		}
		list.Remove(text3);
		list.Insert(0, text3);
		SdFile.WriteAllLines(text2, list.ToArray());
		VersionInformation versionInformation;
		if (VersionInformation.TryParseSerializedString(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.GetValue(GameInfoString.ServerVersion), out versionInformation))
		{
			RemoteWorldInfo remoteWorldInfo = new RemoteWorldInfo(_gameName, _levelName, versionInformation, requiredSpace);
			remoteWorldInfo.Write(rwiFilename);
		}
		else
		{
			Log.Error("Failed writing RemoteWorldInfo. Could not parse LastGameServerInfo information.");
		}
		if (downloadWorld)
		{
			XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, string.Format(Localization.Get("uiLoadDownloadingWorldWait", false, null), 0f, 0, 0), true);
			yield return NetPackageWorldFolder.RequestWorld();
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
			{
				yield break;
			}
			Log.Out("World received");
			GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType));
			GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, 1);
			worldLocation = PathAbstractions.Contextual.FindActiveWorldLocation();
		}
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadDownloadingSigns", false, null), true);
		yield return SignDataManager.Instance.RequestWorldSignDataFromServer();
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			yield break;
		}
		yield return this.createWorld(_levelName, worldLocation, _gameName, _fixedSizeCC);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWorldInitInfoRequest>().Setup(), false);
		while (!this.worldInitInfoReceived)
		{
			yield return null;
		}
		XUiC_ProgressWindow.SetText(LocalPlayerUI.primaryUI, Localization.Get("uiLoadCreatingPlayer", false, null), true);
		yield return null;
		this.worldCreated = true;
		this.firstTimeJoin = _firstTimeJoin;
		string confirmationText = GeneratedTextManager.GetDisplayTextImmediately(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.ServerLoginConfirmationText, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.Supported);
		if (string.IsNullOrEmpty(confirmationText))
		{
			confirmationText = SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.ServerLoginConfirmationText.Text;
		}
		if (!string.IsNullOrEmpty(confirmationText))
		{
			LocalPlayerUI playerUI = LocalPlayerUI.GetUIForPrimaryPlayer();
			while (!playerUI.xui.IsReady)
			{
				yield return null;
			}
			yield return null;
			if (!string.IsNullOrEmpty(XUiC_ServerJoinRulesDialog.ID) && playerUI.xui.FindWindowGroupByName(XUiC_ServerJoinRulesDialog.ID) != null)
			{
				XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
				this.windowManager.Close("crossplayWarning");
				XUiC_ServerJoinRulesDialog.Show(playerUI, confirmationText);
			}
			else
			{
				this.DoSpawn();
			}
			playerUI = null;
		}
		else
		{
			this.DoSpawn();
		}
		confirmationText = null;
		DynamicMeshManager.Init();
		yield break;
		yield break;
	}

	public void DoSpawn()
	{
		if (GamePrefs.GetBool(EnumGamePrefs.SkipSpawnButton))
		{
			this.RequestToSpawn(-1);
			return;
		}
		XUiC_SpawnSelectionWindow.Open(LocalPlayerUI.primaryUI, false, true, this.firstTimeJoin);
	}

	public void RequestToSpawn(int _nearEntityId = -1)
	{
		XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, null, null, true, true, false);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageRequestToSpawnPlayer>().Setup(Utils.FastMin(12, GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance)), PlayerProfile.LoadLocalProfile(), _nearEntityId), false);
	}

	public void ChunkClusterInfo(string _name, bool _bInifiniteTerrain, Vector2i _cMin, Vector2i _cMax, Vector3 _pos)
	{
		base.StartCoroutine(this.chunkClusterInfoCo(_name, _bInifiniteTerrain, _cMin, _cMax, _pos));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator chunkClusterInfoCo(string _name, bool _bInifiniteTerrain, Vector2i _cMin, Vector2i _cMax, Vector3 _pos)
	{
		while (!this.worldCreated && SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			yield return null;
		}
		if (!this.worldCreated)
		{
			yield break;
		}
		if (this.m_World == null)
		{
			yield break;
		}
		ChunkCluster chunkCache = this.m_World.ChunkCache;
		chunkCache.Position = _pos;
		chunkCache.ChunkMinPos = _cMin;
		chunkCache.ChunkMaxPos = _cMax;
		if (!_bInifiniteTerrain && this.m_World.m_WorldEnvironment != null)
		{
			this.m_World.m_WorldEnvironment.SetColliders((float)((_cMin.x + 1) * 16), (float)((_cMin.y + 1) * 16), (float)((_cMax.x - _cMin.x - 1) * 16), (float)((_cMax.y - _cMin.y - 1) * 16), global::Constants.cSizePlanesAround, 0f);
			this.m_World.m_WorldEnvironment.CreateLevelBorderBox(this.m_World);
			this.m_World.ChunkCache.IsFixedSize = true;
		}
		this.chunkClusterLoaded = true;
		yield break;
	}

	public void RequestToSpawnPlayer(ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile, int _nearEntityId)
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.ServerMaxAllowedViewDistance);
		if (num < 4)
		{
			num = 4;
		}
		else if (num > 12)
		{
			num = 12;
		}
		_chunkViewDim = Mathf.Clamp(_chunkViewDim, 4, num);
		PlatformUserIdentifierAbs persistentPlayerId = this.getPersistentPlayerID(_cInfo);
		PlayerDataFile playerDataFile = new PlayerDataFile();
		playerDataFile.Load(GameIO.GetPlayerDataDir(), persistentPlayerId.CombinedString);
		playerDataFile.lastSpawnPosition = SpawnPosition.Undef;
		int num2 = 0;
		int num3;
		if (!playerDataFile.bLoaded || playerDataFile.id == -1)
		{
			EntityFactory.nextEntityID = (num3 = EntityFactory.nextEntityID) + 1;
		}
		else
		{
			num3 = playerDataFile.id;
		}
		int num4 = num3;
		if (this.m_World.GetEntity(num4) != null)
		{
			num4 = EntityFactory.nextEntityID++;
			playerDataFile.id = num4;
		}
		Log.Out(string.Format("RequestToSpawnPlayer: {0}, {1}, {2}", num4, _cInfo.playerName, _chunkViewDim));
		if (GameStats.GetBool(EnumGameStats.IsSpawnNearOtherPlayer))
		{
			for (int i = 0; i < this.m_World.Players.list.Count; i++)
			{
				int num5;
				int num6;
				int num7;
				if (this.m_World.Players.list[i].TeamNumber == num2 && this.m_World.FindRandomSpawnPointNearPlayer(this.m_World.Players.list[i], 15, out num5, out num6, out num7, 15))
				{
					playerDataFile.lastSpawnPosition = new SpawnPosition(new Vector3i(num5, num6, num7), 0f);
					break;
				}
			}
		}
		if (_nearEntityId != -1)
		{
			AllowSpawnNearFriend spawnNearFriendMode = XUiC_SpawnNearFriendsList.SpawnNearFriendMode;
			Entity entity = this.m_World.GetEntity(_nearEntityId);
			if (entity && spawnNearFriendMode != AllowSpawnNearFriend.Disabled)
			{
				int num8 = 15;
				Vector3 vector;
				bool flag;
				do
				{
					num8--;
					flag = this.m_World.GetRandomSpawnPositionMinMaxToPosition(entity.position, 40, 150, 1, true, out vector, num4, true, 20, true, EnumLandClaimOwner.None, false);
					if (!flag)
					{
						break;
					}
					if (spawnNearFriendMode == AllowSpawnNearFriend.InForest)
					{
						BiomeDefinition biomeInWorld = this.m_World.GetBiomeInWorld((int)vector.x, (int)vector.z);
						BiomeDefinition.BiomeType? biomeType = ((biomeInWorld != null) ? new BiomeDefinition.BiomeType?(biomeInWorld.m_BiomeType) : null);
						if (biomeType == null)
						{
							goto IL_0234;
						}
						BiomeDefinition.BiomeType valueOrDefault = biomeType.GetValueOrDefault();
						if (valueOrDefault - BiomeDefinition.BiomeType.Forest > 1)
						{
							goto IL_0234;
						}
						bool flag2 = true;
						IL_0237:
						flag = flag2;
						goto IL_023B;
						IL_0234:
						flag2 = false;
						goto IL_0237;
					}
					IL_023B:;
				}
				while (num8 > 0 && !flag);
				if (flag)
				{
					playerDataFile.lastSpawnPosition = new SpawnPosition(vector, this.m_World.RandomRange(0f, 360f));
				}
				else
				{
					Log.Warning(string.Format("RequestToSpawnPlayer: Failed getting a valid spawn position near player with entity ID {0}", _nearEntityId));
				}
			}
		}
		if (playerDataFile.lastSpawnPosition.IsUndef())
		{
			playerDataFile.lastSpawnPosition = this.GetSpawnPointList().GetRandomSpawnPosition(this.m_World, null, 0, 0);
		}
		if (!playerDataFile.bLoaded)
		{
			playerDataFile.ecd.pos = playerDataFile.lastSpawnPosition.position;
		}
		EntityCreationData entityCreationData = new EntityCreationData();
		if (!playerDataFile.bLoaded || playerDataFile.ecd.playerProfile == null || !GamePrefs.GetBool(EnumGamePrefs.PersistentPlayerProfiles))
		{
			playerDataFile.ecd.playerProfile = _playerProfile;
		}
		if (playerDataFile.bLoaded)
		{
			entityCreationData.entityData = playerDataFile.ecd.entityData;
			entityCreationData.readFileVersion = playerDataFile.ecd.readFileVersion;
		}
		entityCreationData.entityClass = EntityClass.FromString(playerDataFile.ecd.playerProfile.EntityClassName);
		entityCreationData.playerProfile = playerDataFile.ecd.playerProfile;
		entityCreationData.id = num4;
		entityCreationData.teamNumber = num2;
		entityCreationData.pos = playerDataFile.ecd.pos;
		entityCreationData.rot = playerDataFile.ecd.rot;
		EntityPlayer entityPlayer = (EntityPlayer)EntityFactory.CreateEntity(entityCreationData);
		entityPlayer.isEntityRemote = true;
		entityPlayer.Respawn(playerDataFile.bLoaded ? RespawnType.JoinMultiplayer : RespawnType.EnterMultiplayer);
		playerDataFile.ToPlayer(entityPlayer);
		bool flag3 = false;
		PersistentPlayerList persistentPlayerList = this.persistentPlayers;
		PersistentPlayerData persistentPlayerData = ((persistentPlayerList != null) ? persistentPlayerList.GetPlayerData(persistentPlayerId) : null);
		if (persistentPlayerData == null)
		{
			PersistentPlayerList persistentPlayerList2 = this.persistentPlayers;
			persistentPlayerData = ((persistentPlayerList2 != null) ? persistentPlayerList2.CreatePlayerData(persistentPlayerId, _cInfo.PlatformId, _cInfo.playerName, _cInfo.device.ToPlayGroup()) : null);
		}
		else
		{
			persistentPlayerData.Update(_cInfo.PlatformId, new AuthoredText(_cInfo.playerName, persistentPlayerId), _cInfo.device.ToPlayGroup());
			flag3 = true;
		}
		persistentPlayerData.LastLogin = DateTime.Now;
		persistentPlayerData.EntityId = num4;
		if (this.persistentPlayers != null)
		{
			this.persistentPlayers.MapPlayer(persistentPlayerData);
		}
		PersistentPlayerList persistentPlayerList3 = this.persistentPlayers;
		if (persistentPlayerList3 != null)
		{
			persistentPlayerList3.SavePersistentPlayerData();
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SetClientEntityId(_cInfo, num4, playerDataFile);
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerId>().Setup(num4, num2, playerDataFile, _chunkViewDim));
		GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>().RefreshCrates(num4);
		this.m_World.SpawnEntityInWorld(entityPlayer);
		entityPlayer.ChunkObserver = this.m_World.m_ChunkManager.AddChunkObserver(entityPlayer.GetPosition(), false, _chunkViewDim, entityPlayer.entityId);
		IMapChunkDatabase.TryCreateOrLoad(entityPlayer.entityId, out entityPlayer.ChunkObserver.mapDatabase, () => new IMapChunkDatabase.DirectoryPlayerId(GameIO.GetPlayerDataDir(), persistentPlayerId.CombinedString));
		if (this.persistentPlayers != null)
		{
			this.persistentPlayers.DispatchPlayerEvent(persistentPlayerData, null, EnumPersistentPlayerDataReason.Login);
		}
		if (flag3)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerState>().Setup(persistentPlayerData, EnumPersistentPlayerDataReason.Login), false, -1, -1, -1, null, 192, false);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerState>().Setup(persistentPlayerData, EnumPersistentPlayerDataReason.New), false, -1, -1, -1, null, 192, false);
		}
		ModEvents.SPlayerSpawningData splayerSpawningData = new ModEvents.SPlayerSpawningData(_cInfo, _chunkViewDim, _playerProfile);
		ModEvents.PlayerSpawning.Invoke(ref splayerSpawningData);
	}

	public void PersistentPlayerLogin(PersistentPlayerData ppData)
	{
		if (this.persistentPlayers == null)
		{
			return;
		}
		this.persistentPlayers.SetPlayerData(ppData);
		if (this.myPlayerId != -1 && ppData.EntityId == this.myPlayerId)
		{
			this.persistentLocalPlayer = ppData;
			if (this.myEntityPlayerLocal != null)
			{
				this.myEntityPlayerLocal.persistentPlayerData = this.persistentLocalPlayer;
			}
		}
		this.persistentPlayers.DispatchPlayerEvent(ppData, null, EnumPersistentPlayerDataReason.Login);
	}

	public void HandlePersistentPlayerDisconnected(int _entityId)
	{
		PersistentPlayerData playerDataFromEntityID = this.persistentPlayers.GetPlayerDataFromEntityID(_entityId);
		if (playerDataFromEntityID != null)
		{
			this.persistentPlayers.DispatchPlayerEvent(playerDataFromEntityID, null, EnumPersistentPlayerDataReason.Disconnected);
			this.persistentPlayers.UnmapPlayer(playerDataFromEntityID.PrimaryId);
		}
	}

	public void PlayerId(int _playerId, int _teamNumber, PlayerDataFile _playerDataFile, int _chunkViewDim)
	{
		Log.Out(string.Format("PlayerId({0}, {1})", _playerId, _teamNumber));
		Log.Out("Allowed ChunkViewDistance: " + _chunkViewDim.ToString());
		GameStats.Set(EnumGameStats.AllowedViewDistance, _chunkViewDim);
		this.myPlayerId = _playerId;
		EntityCreationData entityCreationData = new EntityCreationData();
		entityCreationData.id = _playerId;
		entityCreationData.teamNumber = _teamNumber;
		if (_playerDataFile.bLoaded)
		{
			entityCreationData.entityClass = EntityClass.FromString(_playerDataFile.ecd.playerProfile.EntityClassName);
			entityCreationData.playerProfile = _playerDataFile.ecd.playerProfile;
		}
		else
		{
			entityCreationData.playerProfile = PlayerProfile.LoadLocalProfile();
			entityCreationData.entityClass = EntityClass.FromString(entityCreationData.playerProfile.EntityClassName);
		}
		entityCreationData.skinTexture = GamePrefs.GetString(EnumGamePrefs.OptionsPlayerModelTexture);
		entityCreationData.id = _playerId;
		entityCreationData.pos = _playerDataFile.ecd.pos;
		entityCreationData.rot = _playerDataFile.ecd.rot;
		entityCreationData.belongsPlayerId = _playerId;
		EntityPlayerLocal entityPlayerLocal = EntityFactory.CreateEntity(entityCreationData) as EntityPlayerLocal;
		this.setLocalPlayerEntity(entityPlayerLocal);
		Log.Out(string.Format("Found own player entity with id {0}", entityPlayerLocal.entityId));
		entityPlayerLocal.lastSpawnPosition = _playerDataFile.lastSpawnPosition;
		if (_playerDataFile.bLoaded)
		{
			_playerDataFile.ToPlayer(entityPlayerLocal);
			this.clientRespawnType = RespawnType.JoinMultiplayer;
		}
		else
		{
			this.clientRespawnType = RespawnType.EnterMultiplayer;
		}
		this.m_World.SpawnEntityInWorld(entityPlayerLocal);
		entityPlayerLocal.ChunkObserver = this.m_World.m_ChunkManager.AddChunkObserver(entityPlayerLocal.GetPosition(), true, GameUtils.GetViewDistance(), -1);
		IMapChunkDatabase.TryCreateOrLoad(entityPlayerLocal.entityId, out entityPlayerLocal.ChunkObserver.mapDatabase, delegate
		{
			string combinedString = this.getPersistentPlayerID(null).CombinedString;
			return new IMapChunkDatabase.DirectoryPlayerId(GameIO.GetPlayerDataLocalDir(), combinedString);
		});
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		uiforPlayer.xui.SetDataConnections();
		uiforPlayer.xui.SetCraftingData(_playerDataFile.craftingData);
		this.SetWorldTime(this.m_World.worldTime);
		PlayerInteractions.Instance.JoinedMultiplayerServer(this.persistentPlayers);
		entityPlayerLocal.Respawn(this.clientRespawnType);
		this.gameStateManager.InitGame(false);
		this.gameStateManager.StartGame();
	}

	public void PlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos, int _entityId)
	{
		if (_entityId == -1)
		{
			return;
		}
		Entity entity;
		if (!this.m_World.Entities.dict.TryGetValue(_entityId, out entity))
		{
			return;
		}
		EntityPlayer entityPlayer = entity as EntityPlayer;
		if (entityPlayer == null)
		{
			return;
		}
		if (_respawnReason == RespawnType.Died && entityPlayer.isEntityRemote)
		{
			entityPlayer.SetAlive();
		}
		if (_respawnReason == RespawnType.EnterMultiplayer || _respawnReason == RespawnType.JoinMultiplayer)
		{
			this.DisplayGameMessage(EnumGameMessages.JoinedGame, _entityId, -1, true);
		}
		PlayerInteractions.Instance.PlayerSpawnedInMultiplayerServer(this.persistentPlayers, _entityId, _respawnReason);
		bool flag = _respawnReason == RespawnType.NewGame || _respawnReason == RespawnType.EnterMultiplayer || _respawnReason == RespawnType.JoinMultiplayer || _respawnReason == RespawnType.LoadedGame;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && flag)
		{
			VehicleManager.Instance.UpdateVehicleWaypointsForPlayer(_entityId);
			DroneManager.Instance.UpdateWaypointsForPlayer(_entityId);
			DroneManager.Instance.SpawnFollowingDronesForPLayer(_entityId, this.World);
		}
		ModEvents.SPlayerSpawnedInWorldData splayerSpawnedInWorldData = new ModEvents.SPlayerSpawnedInWorldData(_cInfo, entityPlayer is EntityPlayerLocal, _entityId, _respawnReason, _pos);
		ModEvents.PlayerSpawnedInWorld.Invoke(ref splayerSpawnedInWorldData);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Action<ClientInfo> onClientSpawned = this.OnClientSpawned;
			if (onClientSpawned != null)
			{
				onClientSpawned(_cInfo);
			}
			Log.Out("PlayerSpawnedInWorld (reason: {0}, position: {2}): {1}", new object[]
			{
				_respawnReason.ToStringCached<RespawnType>(),
				(_cInfo != null) ? _cInfo.ToString() : "localplayer",
				_pos.ToString()
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleFirstSpawnInteractions(PlayerInteraction _interaction)
	{
		if (_interaction.Type != PlayerInteractionType.FirstSpawn)
		{
			return;
		}
		int num = this.persistentPlayers.PlayerToEntityMap[_interaction.PlayerData.PrimaryId];
		if (!(this.myEntityPlayerLocal == null))
		{
			int num2 = num;
			EntityPlayerLocal entityPlayerLocal = this.myEntityPlayerLocal;
			int? num3 = ((entityPlayerLocal != null) ? new int?(entityPlayerLocal.entityId) : null);
			if (!((num2 == num3.GetValueOrDefault()) & (num3 != null)))
			{
				IPlatformUserData orCreate = PlatformUserManager.GetOrCreate(_interaction.PlayerData.PrimaryId);
				if (num != -1 && orCreate != null && orCreate.Blocked[EBlockType.Play].IsBlocked())
				{
					this.DisplayGameMessage(EnumGameMessages.BlockedPlayerAlert, num, -1, true);
					return;
				}
				if (GamePrefs.GetBool(EnumGamePrefs.OptionsAutoPartyWithFriends))
				{
					PersistentPlayerData persistentPlayerData = this.myEntityPlayerLocal.persistentPlayerData;
					if (persistentPlayerData != null && persistentPlayerData.IsAlly(_interaction.PlayerData.PrimaryId))
					{
						if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SendInvite, this.myEntityPlayerLocal.entityId, num, null, null), false);
							return;
						}
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.SendInvite, this.myEntityPlayerLocal.entityId, num, null, null), false, -1, -1, -1, null, 192, false);
					}
				}
				return;
			}
		}
	}

	public void PlayerDisconnected(ClientInfo _cInfo)
	{
		if (_cInfo.entityId != -1)
		{
			EntityPlayer entityPlayer = (EntityPlayer)this.m_World.GetEntity(_cInfo.entityId);
			Log.Out("Player {0} disconnected after {1} minutes", new object[]
			{
				GameUtils.SafeStringFormat(entityPlayer.EntityName),
				((Time.timeSinceLevelLoad - entityPlayer.CreationTimeSinceLevelLoad) / 60f).ToCultureInvariantString("0.0")
			});
		}
		if (GameManager.IsDedicatedServer)
		{
			GC.Collect();
			MemoryPools.Cleanup();
		}
		PersistentPlayerData persistentPlayerData = this.getPersistentPlayerData(_cInfo);
		if (persistentPlayerData != null)
		{
			persistentPlayerData.LastLogin = DateTime.Now;
			persistentPlayerData.EntityId = -1;
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePersistentPlayerState>().Setup(persistentPlayerData, EnumPersistentPlayerDataReason.Disconnected), false, -1, -1, -1, null, 192, true);
		}
		PersistentPlayerList persistentPlayerList = this.persistentPlayers;
		if (persistentPlayerList != null)
		{
			persistentPlayerList.SavePersistentPlayerData();
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.DisconnectClient(_cInfo, false, true);
	}

	public void SavePlayerData(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
	{
		_cInfo.latestPlayerData = _playerDataFile;
		int entityId = _cInfo.entityId;
		if (entityId != -1)
		{
			EntityPlayer entityPlayer = (EntityPlayer)this.m_World.GetEntity(entityId);
			if (entityPlayer != null)
			{
				_playerDataFile.Save(GameIO.GetPlayerDataDir(), _cInfo.InternalId.CombinedString);
				if (entityPlayer.ChunkObserver.mapDatabase != null)
				{
					ThreadManager.AddSingleTask(new ThreadManager.TaskFunctionDelegate(entityPlayer.ChunkObserver.mapDatabase.SaveAsync), new IMapChunkDatabase.DirectoryPlayerId(GameIO.GetPlayerDataDir(), _cInfo.InternalId.CombinedString), null, true);
				}
				entityPlayer.QuestJournal = _playerDataFile.questJournal;
				if (this.persistentPlayers != null)
				{
					foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in this.persistentPlayers.Players)
					{
						if (keyValuePair.Value.EntityId == _playerDataFile.id)
						{
							keyValuePair.Value.Position = new Vector3i(_playerDataFile.ecd.pos);
							break;
						}
					}
				}
			}
		}
		ModEvents.SSavePlayerDataData ssavePlayerDataData = new ModEvents.SSavePlayerDataData(_cInfo, _playerDataFile);
		ModEvents.SavePlayerData.Invoke(ref ssavePlayerDataData);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs getPersistentPlayerID(ClientInfo _cInfo)
	{
		return ((_cInfo != null) ? _cInfo.InternalId : null) ?? PlatformManager.InternalLocalUserIdentifier;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerData getPersistentPlayerData(ClientInfo _cInfo)
	{
		PersistentPlayerList persistentPlayerList = this.persistentPlayers;
		if (persistentPlayerList == null)
		{
			return null;
		}
		return persistentPlayerList.GetPlayerData(this.getPersistentPlayerID(_cInfo));
	}

	public PersistentPlayerList GetPersistentPlayerList()
	{
		return this.persistentPlayers;
	}

	public PersistentPlayerData GetPersistentLocalPlayer()
	{
		return this.persistentLocalPlayer;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator createWorld(string _levelName, PathAbstractions.AbstractedLocation _worldLocation, string _sGameName, bool _fixedSizeCC = false)
	{
		Log.Out(string.Format("createWorld: {0}, {1}, {2}, {3}", new object[]
		{
			_levelName,
			_worldLocation,
			_sGameName,
			GamePrefs.GetString(EnumGamePrefs.GameMode)
		}));
		GamePrefs.Set(EnumGamePrefs.GameNameClient, _sGameName);
		bool flag = GameModeEditWorld.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode));
		PathAbstractions.CacheEnabled = !flag;
		if (flag)
		{
			global::Constants.cDigAndBuildDistance = 50f;
			global::Constants.cBuildIntervall = 0.2f;
			global::Constants.cCollectItemDistance = 50f;
		}
		else if (GameModeCreative.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
		{
			global::Constants.cDigAndBuildDistance = 25f;
			global::Constants.cBuildIntervall = 0.2f;
			global::Constants.cCollectItemDistance = 25f;
		}
		else
		{
			global::Constants.cDigAndBuildDistance = 5f;
			global::Constants.cBuildIntervall = 0.5f;
			global::Constants.cCollectItemDistance = 3.5f;
		}
		OcclusionManager.Instance.WorldChanging(flag);
		yield return null;
		this.m_World = new World();
		if (GameManager.IsDedicatedServer || this.IsEditMode())
		{
			this.GameHasStarted = true;
		}
		else
		{
			base.StartCoroutine(this.waitForGameStart());
		}
		this.m_World.Init(this, WorldBiomes.Instance);
		yield return null;
		if (this.biomeParticleManager == null)
		{
			this.biomeParticleManager = new BiomeParticleManager();
		}
		if (this.OnWorldChanged != null)
		{
			this.OnWorldChanged(this.m_World);
		}
		PlayerInteractions.Instance.OnNewPlayerInteraction += this.HandleFirstSpawnInteractions;
		yield return null;
		SignTextureManager.Instance.Initialize();
		yield return null;
		yield return this.m_World.LoadWorld(_levelName, _worldLocation, _fixedSizeCC);
		yield return null;
		AstarManager.Init(base.gameObject);
		yield return null;
		this.lootManager = new LootManager(this.m_World);
		yield return null;
		LockManager.Instance.Init();
		yield return null;
		this.traderManager = new TraderManager(this.m_World);
		yield return null;
		ResourceRequest weatherLoading = Resources.LoadAsync("Prefabs/WeatherManager");
		while (!weatherLoading.isDone)
		{
			yield return null;
		}
		GameObject gameObject = Object.Instantiate(weatherLoading.asset as GameObject) as GameObject;
		gameObject.transform.SetParent(base.transform, false);
		WeatherManager.Init(this.m_World, gameObject);
		yield return null;
		yield return EnvironmentAudioManager.CreateNewInstance();
		yield return null;
		new WaterSplashCubes();
		yield return null;
		WireManager.Instance.Init();
		yield return null;
		LoadManager.AssetRequestTask<GameObject> requestTask = LoadManager.LoadAsset<GameObject>("@:Prefabs/SkySystem/SkySystem.prefab", null, null, false, false, false);
		yield return new WaitUntil(() => requestTask.IsDone);
		SkyManager.Loaded(Object.Instantiate<GameObject>(requestTask.Asset));
		yield return null;
		if (WeatherManager.Instance)
		{
			WeatherManager.Instance.CloudsFrameUpdateNow();
			WeatherManager.Instance.InitParticles();
		}
		yield return null;
		if (this.IsEditMode())
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = this.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator != null && this.IsEditMode())
			{
				dynamicPrefabDecorator.CreateBoundingBoxes();
			}
			SpawnPointList spawnPointList = this.GetSpawnPointList();
			for (int i = 0; i < spawnPointList.Count; i++)
			{
				SpawnPoint spawnPoint = spawnPointList[i];
				SelectionBoxManager.Instance.CategoryStartPoint.AddBox(spawnPoint.spawnPosition.ToBlockPos().ToString(), spawnPoint.spawnPosition.ToBlockPos(), Vector3i.one, true, false).FacingDirection = spawnPoint.spawnPosition.heading;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				PrefabInstanceClientManager.Instance.StartAsServer();
			}
			else
			{
				PrefabInstanceClientManager.Instance.StartAsClient();
			}
		}
		ModEvents.SCreateWorldDoneData screateWorldDoneData = default(ModEvents.SCreateWorldDoneData);
		ModEvents.CreateWorldDone.Invoke(ref screateWorldDoneData);
		Log.Out("createWorld() done");
		yield break;
	}

	public World World
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.m_World;
		}
	}

	public SpawnPointList GetSpawnPointList()
	{
		return this.m_World.ChunkCache.ChunkProvider.GetSpawnPointList();
	}

	public ChunkManager.ChunkObserver AddChunkObserver(Vector3 _initialPosition, bool _bBuildVisualMeshAround, int _viewDim, int _entityIdToSendChunksTo)
	{
		return this.m_World.m_ChunkManager.AddChunkObserver(_initialPosition, _bBuildVisualMeshAround, _viewDim, _entityIdToSendChunksTo);
	}

	public void RemoveChunkObserver(ChunkManager.ChunkObserver _observer)
	{
		this.m_World.m_ChunkManager.RemoveChunkObserver(_observer);
	}

	public void ExplosionServer(Vector3 _worldPos, Vector3i _blockPos, Quaternion _rotation, ExplosionData _explosionData, int _entityId, float _delay, bool _bRemoveBlockAtExplPosition, ItemValue _itemValueExplosionSource = null)
	{
		if (_bRemoveBlockAtExplPosition)
		{
			this.m_World.SetBlockRPC(_blockPos, BlockValue.Air);
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageExplosionInitiate>().Setup(_worldPos, _blockPos, _rotation, _explosionData, _entityId, _delay, _bRemoveBlockAtExplPosition, _itemValueExplosionSource), false);
			return;
		}
		if (_delay <= 0f)
		{
			this.explode(_worldPos, _blockPos, _rotation, _explosionData, _entityId, _itemValueExplosionSource);
			return;
		}
		base.StartCoroutine(this.explodeLater(_worldPos, _blockPos, _rotation, _explosionData, _entityId, _itemValueExplosionSource, _delay));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator explodeLater(Vector3 _position, Vector3i _blockPos, Quaternion _rotation, ExplosionData _explosionData, int _entityId, ItemValue _itemValueExplosionSource, float _delayInSec)
	{
		yield return new WaitForSeconds(_delayInSec);
		this.explode(_position, _blockPos, _rotation, _explosionData, _entityId, _itemValueExplosionSource);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void explode(Vector3 _worldPos, Vector3i _blockPos, Quaternion _rotation, ExplosionData _explosionData, int _entityId, ItemValue _itemValueExplosionSource)
	{
		Explosion explosion = new Explosion(this.m_World, _worldPos, _blockPos, _explosionData, _entityId);
		explosion.AttackBlocks(_entityId, _itemValueExplosionSource);
		explosion.AttackEntites(_entityId, _itemValueExplosionSource, _explosionData.DamageType);
		this.tempExplPositions.Clear();
		explosion.ChangedBlockPositions.CopyValuesTo(this.tempExplPositions);
		GameManager.ExplodeGroup explodeGroup = new GameManager.ExplodeGroup();
		explodeGroup.pos = _worldPos;
		explodeGroup.radius = _explosionData.BlockRadius;
		explodeGroup.delay = 3;
		foreach (BlockChangeInfo blockChangeInfo in this.tempExplPositions)
		{
			if (blockChangeInfo.blockValue.isair)
			{
				BlockValue block = this.m_World.GetBlock(blockChangeInfo.blockValueRef);
				if (!block.isair && block.Block.IsExplosionAffected)
				{
					GameManager.ExplodeGroup.Falling falling;
					falling.pos = blockChangeInfo.blockValueRef;
					falling.bv = block;
					explodeGroup.fallings.Add(falling);
				}
			}
		}
		if (explodeGroup.fallings.Count > 0)
		{
			this.explodeFallingGroups.Add(explodeGroup);
		}
		GameObject gameObject = this.ExplosionClient(_worldPos, _rotation, _explosionData.ParticleIndex, _explosionData.BlastPower, (float)_explosionData.EntityRadius, _explosionData.BlockDamage, _entityId, this.tempExplPositions);
		if (gameObject != null)
		{
			if (_explosionData.Duration > 0f)
			{
				TemporaryObject component = gameObject.GetComponent<TemporaryObject>();
				if (component != null)
				{
					component.SetLife(_explosionData.Duration);
				}
			}
			ExplosionDamageArea explosionDamageArea;
			if (gameObject.TryGetComponent<ExplosionDamageArea>(ref explosionDamageArea))
			{
				explosionDamageArea.BuffActions = _explosionData.BuffActions;
				explosionDamageArea.InitiatorEntityId = _entityId;
			}
			if (this.m_World.aiDirector != null && !_explosionData.IgnoreHeatMap)
			{
				AudioPlayer component2 = gameObject.GetComponent<AudioPlayer>();
				if (component2)
				{
					this.m_World.aiDirector.OnSoundPlayedAtPosition(_entityId, _worldPos, component2.soundName, 1f);
				}
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageExplosionClient>().Setup(_worldPos, _rotation, _explosionData.ParticleIndex, _explosionData.BlastPower, _explosionData.BlockDamage, (float)_explosionData.EntityRadius, _entityId, this.tempExplPositions), true, -1, -1, -1, null, 192, false);
		}
		this.tempExplPositions.Clear();
	}

	public GameObject ExplosionClient(Vector3 _center, Quaternion _rotation, int _index, int _blastPower, float _blastRadius, float _blockDamage, int _entityId, List<BlockChangeInfo> _explosionChanges)
	{
		if (this.m_World == null)
		{
			return null;
		}
		GameObject gameObject = null;
		if (_index > 0 && _index < WorldStaticData.prefabExplosions.Length && WorldStaticData.prefabExplosions[_index] != null)
		{
			gameObject = Object.Instantiate<GameObject>(WorldStaticData.prefabExplosions[_index].gameObject, _center - Origin.position, _rotation);
			ApplyExplosionForce.Explode(_center, (float)_blastPower, _blastRadius);
		}
		if (_explosionChanges.Count > 0)
		{
			this.ChangeBlocks(null, _explosionChanges);
		}
		QuestEventManager.Current.DetectedExplosion(_center, _entityId, _blockDamage);
		return gameObject;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExplodeGroupFrameUpdate()
	{
		int num = EntityClass.FromString("fallingBlock");
		GameRandom gameRandom = this.m_World.GetGameRandom();
		for (int i = this.explodeFallingGroups.Count - 1; i >= 0; i--)
		{
			GameManager.ExplodeGroup explodeGroup = this.explodeFallingGroups[i];
			GameManager.ExplodeGroup explodeGroup2 = explodeGroup;
			int num2 = explodeGroup2.delay - 1;
			explodeGroup2.delay = num2;
			if (num2 <= 0)
			{
				float num3 = 20f + Mathf.Pow((float)explodeGroup.fallings.Count, 0.73f);
				float num4 = Utils.FastMax(1f, (float)explodeGroup.fallings.Count / num3);
				float num5 = 1f;
				for (int j = 0; j < explodeGroup.fallings.Count; j++)
				{
					if ((num5 -= 1f) <= 0f)
					{
						num5 += num4;
						GameManager.ExplodeGroup.Falling falling = explodeGroup.fallings[j];
						Vector3 vector = falling.pos.ToVector3Center();
						vector.y += 1.4f;
						if (Physics.Raycast(vector - Origin.position, Vector3.down, 3.4028235E+38f, 65536))
						{
							vector.y -= 1.4f;
							Block block = falling.bv.Block;
							block.DropItemsOnEvent(this.m_World, falling.bv, EnumDropEvent.Destroy, 0.5f, vector, Vector3.zero, global::Constants.cItemExplosionLifetime, -1, true);
							if (block.ShowModelOnFall())
							{
								EntityFallingBlock entityFallingBlock = (EntityFallingBlock)EntityFactory.CreateEntity(num, -1, new BlockValue[] { falling.bv }, new TextureFullArray[] { this.m_World.GetTextureFullArray(falling.pos.x, falling.pos.y, falling.pos.z) }, 1, vector, Vector3.zero, -1f, -1, -1, "");
								Vector3 vector2 = vector - explodeGroup.pos;
								float num6 = 1f - Mathf.Clamp01(vector2.magnitude / explodeGroup.radius) * 0.6f;
								float num7 = 18f * num6;
								vector2.y += -0.2f + gameRandom.RandomFloat * 6f;
								entityFallingBlock.SetStartVelocity(vector2.normalized * num7, (gameRandom.RandomFloat * 15f + 2f) * num6);
								this.m_World.SpawnEntityInWorld(entityFallingBlock);
							}
						}
					}
				}
				this.explodeFallingGroups.RemoveAt(i);
			}
		}
	}

	public void ChangeBlocks(PlatformUserIdentifierAbs persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
	{
		if (this.m_World == null)
		{
			return;
		}
		List<ChunkCluster> list = this.ccChanged;
		lock (list)
		{
			Entity entity = null;
			if (persistentPlayerId == null)
			{
				PersistentPlayerData playerData = this.persistentLocalPlayer;
				entity = this.myEntityPlayerLocal;
			}
			else if (this.persistentPlayers != null)
			{
				PersistentPlayerData playerData = this.persistentPlayers.GetPlayerData(persistentPlayerId);
				if (playerData != null && playerData.EntityId != -1)
				{
					entity = this.m_World.GetEntity(playerData.EntityId);
				}
			}
			bool flag2 = false;
			ChunkCluster chunkCluster = null;
			int num = 0;
			int i = 0;
			while (i < _blocksToChange.Count)
			{
				BlockChangeInfo blockChangeInfo = _blocksToChange[i];
				if (chunkCluster != null)
				{
					goto IL_00C0;
				}
				chunkCluster = this.m_World.ChunkCache;
				if (chunkCluster != null)
				{
					if (!this.ccChanged.Contains(chunkCluster))
					{
						this.ccChanged.Add(chunkCluster);
						num++;
						chunkCluster.ChunkPosNeedsRegeneration_DelayedStart();
						goto IL_00C0;
					}
					goto IL_00C0;
				}
				IL_0558:
				i++;
				continue;
				IL_00C0:
				bool flag3 = blockChangeInfo.bChangeDensity;
				bool bForceDensity = blockChangeInfo.bForceDensity;
				sbyte density = chunkCluster.GetDensity(blockChangeInfo.blockValueRef);
				sbyte b = blockChangeInfo.density;
				if (!flag3)
				{
					if (density < 0 && blockChangeInfo.blockValue.isair)
					{
						b = MarchingCubes.DensityAir;
						flag3 = true;
					}
					else if (density >= 0 && blockChangeInfo.blockValue.Block.shape.IsTerrain())
					{
						b = MarchingCubes.DensityTerrain;
						flag3 = true;
					}
				}
				if (density == b)
				{
					flag3 = false;
				}
				if (blockChangeInfo.bChangeDamage && chunkCluster.GetBlock(blockChangeInfo.blockValueRef).type != blockChangeInfo.blockValue.type)
				{
					goto IL_0558;
				}
				if (blockChangeInfo.blockValueRef.Type != BlockValueRefType.Block)
				{
					chunkCluster.SetBlockValue(blockChangeInfo.blockValueRef, blockChangeInfo.blockValue);
					goto IL_0558;
				}
				Chunk chunk = chunkCluster.GetChunkFromWorldPos(blockChangeInfo.blockValueRef) as Chunk;
				Vector3i vector3i;
				if (chunk != null && blockChangeInfo.blockValueRef.TryGetBlockPos(out vector3i))
				{
					int num2 = World.toBlockXZ(vector3i.x);
					int num3 = World.toBlockXZ(vector3i.z);
					if (vector3i.y >= (int)chunk.GetHeight(num2, num3) && blockChangeInfo.blockValue.Block.shape.IsTerrain())
					{
						chunk.SetTopSoilBroken(num2, num3);
						Chunk chunk2 = chunk;
						if (num3 == 15)
						{
							chunk2 = chunkCluster.GetChunkSync(chunk.X, chunk.Z + 1);
						}
						if (chunk2 != null)
						{
							chunk2.SetTopSoilBroken(num2, World.toBlockXZ(num3 + 1));
						}
						chunk2 = chunk;
						if (num2 == 15)
						{
							chunk2 = chunkCluster.GetChunkSync(chunk.X + 1, chunk.Z);
						}
						if (chunk2 != null)
						{
							chunk2.SetTopSoilBroken(World.toBlockXZ(num2 + 1), num3);
						}
						chunk2 = chunk;
						if (num3 == 0)
						{
							chunk2 = chunkCluster.GetChunkSync(chunk.X, chunk.Z - 1);
						}
						if (chunk2 != null)
						{
							chunk2.SetTopSoilBroken(num2, World.toBlockXZ(num3 - 1));
						}
						chunk2 = chunk;
						if (num2 == 0)
						{
							chunk2 = chunkCluster.GetChunkSync(chunk.X - 1, chunk.Z);
						}
						if (chunk2 != null)
						{
							chunk2.SetTopSoilBroken(World.toBlockXZ(num2 - 1), num3);
						}
					}
					this.m_World.UncullChunk(chunk);
				}
				TileEntity tileEntity = null;
				if (!blockChangeInfo.blockValue.ischild)
				{
					tileEntity = this.m_World.GetTileEntity(blockChangeInfo.blockValueRef);
				}
				BlockValue blockValue = chunkCluster.SetBlock(blockChangeInfo.blockValueRef, blockChangeInfo.bChangeBlockValue, blockChangeInfo.blockValue, flag3, b, true, blockChangeInfo.bUpdateLight, bForceDensity, false, blockChangeInfo.changedByEntityId);
				if (tileEntity != null)
				{
					TileEntity tileEntity2 = this.m_World.GetTileEntity(blockChangeInfo.blockValueRef);
					bool flag4 = false;
					if (tileEntity != tileEntity2 && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						flag4 = true;
						tileEntity.ReplacedBy(blockValue, blockChangeInfo.blockValue, tileEntity2);
					}
					if (blockChangeInfo.blockValue.isair)
					{
						flag4 = true;
						if (chunk != null)
						{
							chunk.RemoveTileEntityAt<TileEntity>(this.m_World, World.toBlock(blockChangeInfo.blockValueRef));
						}
					}
					else if (tileEntity != tileEntity2)
					{
						flag4 = true;
						if (tileEntity2 != null)
						{
							tileEntity2.UpgradeDowngradeFrom(tileEntity);
						}
					}
					if (flag4 && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && LockManager.Instance.IsLockedServer(tileEntity, 0))
					{
						LockManager.Instance.ForceUnlockLockTarget(tileEntity);
					}
				}
				if (chunk != null && blockChangeInfo.blockValue.isair)
				{
					chunk.RemoveBlockTrigger(World.toBlock(blockChangeInfo.blockValueRef));
				}
				if (blockValue.type != blockChangeInfo.blockValue.type)
				{
					Block block = blockChangeInfo.blockValue.Block;
					Block block2 = blockValue.Block;
					QuestEventManager.Current.BlockChanged(block2, block, blockChangeInfo.blockValueRef);
					if (block is BlockSleepingBag || block2 is BlockSleepingBag)
					{
						EntityAlive entityAlive = entity as EntityAlive;
						if (entityAlive)
						{
							if (block is BlockSleepingBag)
							{
								NavObjectManager.Instance.UnRegisterNavObjectByOwnerEntity(entityAlive, "sleeping_bag");
								entityAlive.SpawnPoints.Set(blockChangeInfo.blockValueRef);
							}
							else
							{
								this.persistentPlayers.SpawnPointRemoved(blockChangeInfo.blockValueRef);
							}
							flag2 = true;
						}
					}
				}
				if (blockChangeInfo.bChangeTexture)
				{
					chunkCluster.SetTextureFullArray(blockChangeInfo.blockValueRef, blockChangeInfo.textureFull);
					goto IL_0558;
				}
				if (blockValue.Block.CanBlocksReplace)
				{
					chunkCluster.SetTextureFullArray(blockChangeInfo.blockValueRef, new TextureFullArray(0L));
					goto IL_0558;
				}
				goto IL_0558;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && flag2)
			{
				PersistentPlayerList persistentPlayerList = this.persistentPlayers;
				if (persistentPlayerList != null)
				{
					persistentPlayerList.SavePersistentPlayerData();
				}
			}
			if (num > 0)
			{
				int num4 = this.ccChanged.Count;
				for (int j = 0; j < num; j++)
				{
					this.ccChanged[--num4].ChunkPosNeedsRegeneration_DelayedStop();
				}
				this.ccChanged.RemoveRange(num4, num);
			}
		}
	}

	public void ChangeProps(PlatformUserIdentifierAbs persistentPlayerId, List<PropChangeInfo> propsToChange)
	{
		if (this.m_World == null)
		{
			return;
		}
		List<ChunkCluster> list = this.ccChanged;
		lock (list)
		{
			Log.Warning(string.Format("ChangeProps player {0}, count {1}", (persistentPlayerId != null) ? persistentPlayerId.ToString() : "", propsToChange.Count));
			ChunkCluster chunkCache = this.m_World.ChunkCache;
			int num = 0;
			if (chunkCache != null)
			{
				if (!this.ccChanged.Contains(chunkCache))
				{
					this.ccChanged.Add(chunkCache);
					num++;
					chunkCache.ChunkPosNeedsRegeneration_DelayedStart();
				}
				foreach (PropChangeInfo propChangeInfo in propsToChange)
				{
					if (chunkCache.GetChunkSync(propChangeInfo.ChunkPos) == null)
					{
						return;
					}
					chunkCache.SetProp(propChangeInfo.ChunkPos, propChangeInfo.PropId, propChangeInfo.Position, propChangeInfo.Rotation, propChangeInfo.Scale, propChangeInfo.BlockValue);
				}
			}
			if (num > 0)
			{
				int num2 = this.ccChanged.Count;
				for (int i = 0; i < num; i++)
				{
					this.ccChanged[--num2].ChunkPosNeedsRegeneration_DelayedStop();
				}
				this.ccChanged.RemoveRange(num2, num);
			}
		}
	}

	public void SetBlocksRPC(List<BlockChangeInfo> _changes, PlatformUserIdentifierAbs _persistentPlayerId = null)
	{
		this.ChangeBlocks(_persistentPlayerId, _changes);
		NetPackageSetBlock netPackageSetBlock = NetPackageManager.GetPackage<NetPackageSetBlock>().Setup(this.persistentLocalPlayer, _changes, GameManager.IsDedicatedServer ? (-1) : this.myPlayerId);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.SetBlocksOnClients(-1, netPackageSetBlock);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(netPackageSetBlock, false);
	}

	public void SetPropsRPC(List<PropChangeInfo> _changes, PlatformUserIdentifierAbs _persistentPlayerId = null)
	{
		this.ChangeProps(_persistentPlayerId, _changes);
		NetPackageSetProp netPackageSetProp = NetPackageManager.GetPackage<NetPackageSetProp>().Setup(this.persistentLocalPlayer, _changes, GameManager.IsDedicatedServer ? (-1) : this.myPlayerId);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.SetPropsOnClients(-1, netPackageSetProp);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(netPackageSetProp, false);
	}

	public void SetBlocksOnClients(int _exceptThisEntityId, NetPackageSetBlock package)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, -1, _exceptThisEntityId, -1, null, 192, false);
	}

	public void SetPropsOnClients(int _exceptThisEntityId, NetPackageSetProp package)
	{
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, -1, _exceptThisEntityId, -1, null, 192, false);
	}

	public void SetWaterRPC(NetPackageWaterSet package)
	{
		if (this.m_World != null)
		{
			ChunkCluster chunkCache = this.m_World.ChunkCache;
			if (chunkCache != null)
			{
				package.ApplyChanges(chunkCache);
			}
		}
		package.SetSenderId(GameManager.IsDedicatedServer ? (-1) : this.myPlayerId);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, -1, -1, -1, null, 192, false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBlockParticles()
	{
		List<GameManager.BlockParticleCreationData> list = this.blockParticlesToSpawn;
		lock (list)
		{
			for (int i = 0; i < this.blockParticlesToSpawn.Count; i++)
			{
				if (this.m_BlockParticles.ContainsKey(this.blockParticlesToSpawn[i].blockPos))
				{
					this.RemoveBlockParticleEffect(this.blockParticlesToSpawn[i].blockPos);
				}
				Transform transform = GameManager.Instance.SpawnParticleEffectClientForceCreation(this.blockParticlesToSpawn[i].particleEffect, -1, true);
				this.m_BlockParticles[this.blockParticlesToSpawn[i].blockPos] = transform;
			}
			this.blockParticlesToSpawn.Clear();
		}
	}

	public void SpawnBlockParticleEffect(Vector3i _blockPos, ParticleEffect _pe)
	{
		List<GameManager.BlockParticleCreationData> list = this.blockParticlesToSpawn;
		lock (list)
		{
			this.blockParticlesToSpawn.Add(new GameManager.BlockParticleCreationData(_blockPos, _pe));
		}
	}

	public bool HasBlockParticleEffect(Vector3i _blockPos)
	{
		return this.m_BlockParticles.ContainsKey(_blockPos);
	}

	public Transform GetBlockParticleEffect(Vector3i _blockPos)
	{
		return this.m_BlockParticles[_blockPos];
	}

	public void RemoveBlockParticleEffect(Vector3i _blockPos)
	{
		List<GameManager.BlockParticleCreationData> list = this.blockParticlesToSpawn;
		lock (list)
		{
			if (this.m_BlockParticles.ContainsKey(_blockPos))
			{
				Transform transform = this.m_BlockParticles[_blockPos];
				this.m_BlockParticles.Remove(_blockPos);
				if (transform != null)
				{
					Object.Destroy(transform.gameObject);
				}
			}
			else
			{
				for (int i = this.blockParticlesToSpawn.Count - 1; i >= 0; i--)
				{
					if (this.blockParticlesToSpawn[i].blockPos == _blockPos)
					{
						this.blockParticlesToSpawn.RemoveAt(i);
					}
				}
			}
		}
	}

	public void SpawnParticleEffectServer(ParticleEffect _pe, int _entityId, bool _forceCreation = false, bool _worldSpawn = false)
	{
		if (this.m_World == null)
		{
			return;
		}
		ParticleEffect.SpawnParticleEffect(_pe, _entityId, _forceCreation, _worldSpawn);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, _forceCreation, _worldSpawn), false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, _forceCreation, _worldSpawn), false, -1, _entityId, -1, null, 192, false);
	}

	public Transform SpawnParticleEffectClientForceCreation(ParticleEffect _pe, int _entityThatCausedIt, bool _worldSpawn)
	{
		return ParticleEffect.SpawnParticleEffect(_pe, _entityThatCausedIt, true, _worldSpawn);
	}

	public void SpawnParticleEffectClient(ParticleEffect _pe, int _entityThatCausedIt, bool _forceCreation = false, bool _worldSpawn = false)
	{
		ParticleEffect.SpawnParticleEffect(_pe, _entityThatCausedIt, _forceCreation, _worldSpawn);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PhysicsInit()
	{
		Physics.ContactEvent += new Physics.ContactEventDelegate(this.PhysicsContactEvent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PhysicsContactEvent(PhysicsScene scene, NativeArray<ContactPairHeader>.ReadOnly pairHeaders)
	{
		int length = pairHeaders.Length;
		for (int i = 0; i < length; i++)
		{
			Rigidbody rigidbody = pairHeaders[i].Body as Rigidbody;
			if (rigidbody)
			{
				EntityFallingBlock entityFallingBlock;
				EntityFallingBlocks entityFallingBlocks;
				if (rigidbody.TryGetComponent<EntityFallingBlock>(ref entityFallingBlock))
				{
					entityFallingBlock.OnContactEvent();
				}
				else if (rigidbody.TryGetComponent<EntityFallingBlocks>(ref entityFallingBlocks))
				{
					entityFallingBlocks.OnContactEvent();
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsEditMode()
	{
		return this.isEditMode;
	}

	public void GameMessage(EnumGameMessages _type, EntityAlive _mainEntity, EntityAlive _otherEntity)
	{
		if (_mainEntity == null)
		{
			return;
		}
		int num = -1;
		if (_mainEntity is EntityPlayer)
		{
			int num2;
			switch (_type)
			{
			case EnumGameMessages.PlainTextLocal:
			case EnumGameMessages.ChangedTeam:
				return;
			case EnumGameMessages.EntityWasKilled:
				num2 = _mainEntity.entityId;
				if (_otherEntity is EntityPlayer)
				{
					num = _otherEntity.entityId;
				}
				break;
			case EnumGameMessages.JoinedGame:
			case EnumGameMessages.LeftGame:
			case EnumGameMessages.Chat:
				num2 = _mainEntity.entityId;
				break;
			default:
				return;
			}
			this.GameMessageServer(null, _type, num2, num);
			return;
		}
		if (_type == EnumGameMessages.EntityWasKilled || _type == EnumGameMessages.Chat)
		{
			int num2 = ((_mainEntity != null) ? _mainEntity.entityId : (-1));
			this.GameMessageServer(null, _type, num2, num);
			return;
		}
	}

	public void GameMessageServer(ClientInfo _cInfo, EnumGameMessages _type, int _mainEntityId, int _secondaryEntityId)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageGameMessage>().Setup(_type, _mainEntityId, _secondaryEntityId), false);
			return;
		}
		Entity entity = this.World.GetEntity(_mainEntityId);
		EntityPlayer entityPlayer = entity as EntityPlayer;
		string text;
		if (entityPlayer != null)
		{
			text = entityPlayer.PlayerDisplayName;
		}
		else
		{
			EntityAlive entityAlive = entity as EntityAlive;
			if (entityAlive != null)
			{
				text = Localization.Get(entityAlive.EntityName, false, null);
			}
			else
			{
				text = Localization.Get("xuiChatServer", false, null);
			}
		}
		this.FinishGameMessageServer(_cInfo, _type, _mainEntityId, _secondaryEntityId, text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FinishGameMessageServer(ClientInfo _cInfo, EnumGameMessages _type, int _mainEntityId, int _secondaryEntityId, string _mainName)
	{
		PersistentPlayerData playerDataFromEntityID = this.persistentPlayers.GetPlayerDataFromEntityID(_secondaryEntityId);
		string text = ((playerDataFromEntityID != null) ? playerDataFromEntityID.PlayerName.DisplayName : null);
		ModEvents.SGameMessageData sgameMessageData = new ModEvents.SGameMessageData(_cInfo, _type, _mainName, text);
		ValueTuple<ModEvents.EModEventResult, Mod> valueTuple = ModEvents.GameMessage.Invoke(ref sgameMessageData);
		ModEvents.EModEventResult item = valueTuple.Item1;
		Mod item2 = valueTuple.Item2;
		string text2 = this.DisplayGameMessage(_type, _mainEntityId, _secondaryEntityId, item2 == null);
		if (item != ModEvents.EModEventResult.StopHandlersAndVanilla)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameMessage>().Setup(_type, _mainEntityId, _secondaryEntityId), true, -1, -1, -1, null, 192, false);
			return;
		}
		Log.Out("GameMessage handled by mod '{0}': {1}", new object[] { item2.Name, text2 });
	}

	public string DisplayGameMessage(EnumGameMessages _type, int _mainEntity, int _secondaryEntity = -1, bool _log = true)
	{
		string text = null;
		PersistentPlayerData playerDataFromEntityID = this.persistentPlayers.GetPlayerDataFromEntityID(_mainEntity);
		string text2;
		if (playerDataFromEntityID == null)
		{
			text2 = null;
		}
		else
		{
			PersistentPlayerName playerName = playerDataFromEntityID.PlayerName;
			text2 = ((playerName != null) ? playerName.DisplayName : null);
		}
		string text3 = text2;
		string text4;
		if (_secondaryEntity != -1)
		{
			PersistentPlayerData playerDataFromEntityID2 = this.persistentPlayers.GetPlayerDataFromEntityID(_secondaryEntity);
			if (playerDataFromEntityID2 == null)
			{
				text4 = null;
			}
			else
			{
				PersistentPlayerName playerName2 = playerDataFromEntityID2.PlayerName;
				text4 = ((playerName2 != null) ? playerName2.DisplayName : null);
			}
		}
		else
		{
			text4 = null;
		}
		string text5 = text4;
		switch (_type)
		{
		case EnumGameMessages.EntityWasKilled:
		{
			string text6;
			if (!string.IsNullOrEmpty(text5))
			{
				text = string.Format("GMSG: Player '{0}' killed by '{1}'", text3, text5);
				text6 = string.Format(Localization.Get("killedGameMessage", false, null), text5, text3);
				goto IL_0138;
			}
			text = string.Format("GMSG: Player '{0}' died", text3);
			text6 = string.Format(Localization.Get("diedGameMessage", false, null), text3);
			goto IL_0138;
		}
		case EnumGameMessages.JoinedGame:
		{
			text = string.Format("GMSG: Player '{0}' joined the game", text3);
			string text6 = string.Format(Localization.Get("joinGameMessage", false, null), text3);
			goto IL_0138;
		}
		case EnumGameMessages.LeftGame:
		{
			text = string.Format("GMSG: Player '{0}' left the game", text3);
			string text6 = string.Format(Localization.Get("leaveGameMessage", false, null), text3);
			goto IL_0138;
		}
		case EnumGameMessages.BlockedPlayerAlert:
		{
			text = string.Format("GMSG: Blocked player '{0}' is present on this server!", text3);
			string text6 = string.Format("[FF0000A0]" + Localization.Get("blockedPlayerMessage", false, null), text3);
			goto IL_0138;
		}
		}
		return text;
		IL_0138:
		if (_log)
		{
			Log.Out(text);
		}
		if (!GameManager.IsDedicatedServer)
		{
			if (_type == EnumGameMessages.BlockedPlayerAlert)
			{
				string text6;
				XUiC_ChatOutput.AddMessage(this.myEntityPlayerLocal.PlayerUI.xui, _type, text6, EChatType.Global, EChatDirection.Inbound, -1, null, null, EMessageSender.None, GeneratedTextManager.TextFilteringMode.None, GeneratedTextManager.BbCodeSupportMode.Supported);
			}
			else
			{
				foreach (EntityPlayerLocal entityPlayerLocal in this.m_World.GetLocalPlayers())
				{
					string text6;
					XUiC_ChatOutput.AddMessage(LocalPlayerUI.GetUIForPlayer(entityPlayerLocal).xui, _type, text6, EChatType.Global, EChatDirection.Inbound, -1, null, null, EMessageSender.None, GeneratedTextManager.TextFilteringMode.None, GeneratedTextManager.BbCodeSupportMode.Supported);
				}
			}
		}
		return text;
	}

	public void ChatMessageServer(ClientInfo _cInfo, EChatType _chatType, int _senderEntityId, string _msg, List<int> _recipientEntityIds, EMessageSender _msgSender, GeneratedTextManager.BbCodeSupportMode _bbMode = GeneratedTextManager.BbCodeSupportMode.Supported)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			string text = null;
			if (_senderEntityId != -1)
			{
				PersistentPlayerData playerDataFromEntityID = this.persistentPlayers.GetPlayerDataFromEntityID(_senderEntityId);
				string text2;
				if (playerDataFromEntityID == null)
				{
					text2 = null;
				}
				else
				{
					PersistentPlayerName playerName = playerDataFromEntityID.PlayerName;
					if (playerName == null)
					{
						text2 = null;
					}
					else
					{
						AuthoredText authoredName = playerName.AuthoredName;
						text2 = ((authoredName != null) ? authoredName.Text : null);
					}
				}
				text = Utils.EscapeBbCodes(text2, false, false);
			}
			ModEvents.SChatMessageData schatMessageData = new ModEvents.SChatMessageData(_cInfo, _chatType, _senderEntityId, _msg, text, _recipientEntityIds);
			ValueTuple<ModEvents.EModEventResult, Mod> valueTuple = ModEvents.ChatMessage.Invoke(ref schatMessageData);
			ModEvents.EModEventResult item = valueTuple.Item1;
			Mod item2 = valueTuple.Item2;
			this.ChatMessageClient(_chatType, _senderEntityId, _msg, _recipientEntityIds, _msgSender, GeneratedTextManager.BbCodeSupportMode.Supported);
			string text3 = ((((_cInfo != null) ? _cInfo.PlatformId : null) != null) ? _cInfo.PlatformId.CombinedString : "-non-player-");
			string text4 = string.Format("Chat (from '{0}', entity id '{1}', to '{2}'): {3}{4}", new object[]
			{
				text3,
				_senderEntityId,
				_chatType.ToStringCached<EChatType>(),
				(text != null) ? ("'" + text + "': ") : "",
				_msg
			});
			if (item == ModEvents.EModEventResult.StopHandlersAndVanilla)
			{
				Log.Out("Chat handled by mod '{0}': {1}", new object[] { item2.Name, text4 });
			}
			else
			{
				Log.Out(text4);
			}
			if (item != ModEvents.EModEventResult.StopHandlersAndVanilla)
			{
				if (_recipientEntityIds != null)
				{
					using (List<int>.Enumerator enumerator = _recipientEntityIds.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							int num = enumerator.Current;
							ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(num);
							if (clientInfo != null)
							{
								clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(_chatType, _senderEntityId, _msg, null, _msgSender, _bbMode));
							}
						}
						return;
					}
				}
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(_chatType, _senderEntityId, _msg, null, _msgSender, _bbMode), true, -1, -1, -1, null, 192, false);
				return;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageChat>().Setup(_chatType, _senderEntityId, _msg, _recipientEntityIds, _msgSender, _bbMode), false);
		}
	}

	public void ChatMessageClient(EChatType _chatType, int _senderEntityId, string _msg, List<int> _recipientEntityIds, EMessageSender _msgSender, GeneratedTextManager.BbCodeSupportMode _bbMode)
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		foreach (EntityPlayerLocal entityPlayerLocal in this.m_World.GetLocalPlayers())
		{
			if (_recipientEntityIds == null || _recipientEntityIds.Contains(entityPlayerLocal.entityId))
			{
				XUiC_ChatOutput.AddMessage(LocalPlayerUI.GetUIForPlayer(entityPlayerLocal).xui, EnumGameMessages.Chat, _msg, _chatType, EChatDirection.Inbound, _senderEntityId, null, _senderEntityId.ToString(), _msgSender, GeneratedTextManager.TextFilteringMode.Filter, _bbMode);
			}
		}
	}

	public void RemoveChunk(long _chunkKey)
	{
		this.m_World.m_ChunkManager.RemoveChunk(_chunkKey);
	}

	public IBlockTool GetActiveBlockTool()
	{
		if (this.activeBlockTool == null)
		{
			return this.blockSelectionTool;
		}
		return this.activeBlockTool;
	}

	public void SetActiveBlockTool(IBlockTool _tool)
	{
		this.activeBlockTool = _tool;
	}

	public DynamicPrefabDecorator GetDynamicPrefabDecorator()
	{
		if (this.m_World == null)
		{
			return null;
		}
		ChunkCluster chunkCache = this.m_World.ChunkCache;
		if (chunkCache == null)
		{
			return null;
		}
		return chunkCache.ChunkProvider.GetDynamicPrefabDecorator();
	}

	public void SimpleRPC(int _entityId, SimpleRPCType _rpcType, bool _bExeLocal, bool _bOnlyLocal)
	{
		if (_bExeLocal)
		{
			EntityAlive entityAlive = (EntityAlive)this.m_World.GetEntity(_entityId);
			if (entityAlive != null)
			{
				if (_rpcType != SimpleRPCType.OnActivateItem)
				{
					if (_rpcType == SimpleRPCType.OnResetItem)
					{
						entityAlive.inventory.holdingItem.OnHoldingReset(entityAlive.inventory.holdingItemData);
					}
				}
				else
				{
					entityAlive.inventory.holdingItem.OnHoldingItemActivated(entityAlive.inventory.holdingItemData);
				}
			}
		}
		if (_bOnlyLocal)
		{
			return;
		}
		NetPackage netPackage = NetPackageManager.GetPackage<NetPackageSimpleRPC>().Setup(_entityId, _rpcType);
		if (this.m_World.IsRemote())
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(netPackage, false);
			return;
		}
		this.m_World.entityDistributer.SendPacketToTrackedPlayers(_entityId, _entityId, netPackage, false);
	}

	public void ItemDropServer(ItemStack _itemStack, Vector3 _dropPos, Vector3 _randomPosAdd, int _entityId = -1, float _lifetime = 60f, bool _bDropPosIsRelativeToHead = false)
	{
		this.ItemDropServer(_itemStack, _dropPos, _randomPosAdd, Vector3.zero, _entityId, _lifetime, _bDropPosIsRelativeToHead, 0);
	}

	public void ItemDropServer(ItemStack _itemStack, Vector3 _dropPos, Vector3 _randomPosAdd, Vector3 _initialMotion, int _entityId = -1, float _lifetime = 60f, bool _bDropPosIsRelativeToHead = false, int _clientEntityId = 0)
	{
		if (this.m_World == null)
		{
			return;
		}
		bool flag = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		Entity entity = this.m_World.GetEntity(_entityId);
		if (_clientEntityId != 0)
		{
			if (!entity)
			{
				return;
			}
			flag = !entity.isEntityRemote;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (_clientEntityId == -1)
			{
				World world = this.m_World;
				int num = world.clientLastEntityId - 1;
				world.clientLastEntityId = num;
				_clientEntityId = num;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageItemDrop>().Setup(_itemStack, _dropPos, _initialMotion, _randomPosAdd, _lifetime, _entityId, _bDropPosIsRelativeToHead, _clientEntityId), false);
			if (!flag)
			{
				return;
			}
		}
		if (_bDropPosIsRelativeToHead)
		{
			if (entity == null)
			{
				return;
			}
			_dropPos += entity.getHeadPosition();
		}
		if (!_randomPosAdd.Equals(Vector3.zero))
		{
			_dropPos += new Vector3(this.m_World.RandomRange(-_randomPosAdd.x, _randomPosAdd.x), this.m_World.RandomRange(-_randomPosAdd.y, _randomPosAdd.y), this.m_World.RandomRange(-_randomPosAdd.z, _randomPosAdd.z));
		}
		EntityCreationData entityCreationData = new EntityCreationData();
		entityCreationData.entityClass = EntityClass.FromString("item");
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && _clientEntityId < -1)
		{
			entityCreationData.id = _clientEntityId;
		}
		else
		{
			entityCreationData.id = EntityFactory.nextEntityID++;
		}
		entityCreationData.itemStack = _itemStack.Clone();
		entityCreationData.pos = _dropPos;
		entityCreationData.rot = new Vector3(20f, 0f, 20f);
		entityCreationData.lifetime = _lifetime;
		entityCreationData.belongsPlayerId = _entityId;
		if (_clientEntityId != -1)
		{
			entityCreationData.clientEntityId = _clientEntityId;
		}
		EntityItem entityItem = (EntityItem)EntityFactory.CreateEntity(entityCreationData);
		entityItem.isPhysicsMaster = flag;
		if (_initialMotion.sqrMagnitude > 0.01f)
		{
			entityItem.AddVelocity(_initialMotion);
		}
		this.m_World.SpawnEntityInWorld(entityItem);
		Chunk chunk = (Chunk)this.m_World.GetChunkSync(World.toChunkXZ((int)_dropPos.x), World.toChunkXZ((int)_dropPos.z));
		if (chunk != null)
		{
			List<EntityItem> list = new List<EntityItem>();
			for (int i = 0; i < chunk.entityLists.Length; i++)
			{
				if (chunk.entityLists[i] != null)
				{
					for (int j = 0; j < chunk.entityLists[i].Count; j++)
					{
						if (chunk.entityLists[i][j] is EntityItem)
						{
							list.Add(chunk.entityLists[i][j] as EntityItem);
						}
					}
				}
			}
			int num2 = list.Count - 50;
			if (num2 > 0)
			{
				list.Sort(new GameManager.EntityItemLifetimeComparer());
				int num3 = list.Count - 1;
				while (num3 >= 0 && num2 > 0)
				{
					list[num3].MarkToUnload();
					num2--;
					num3--;
				}
			}
		}
	}

	public void AddExpServer(int _entityId, string UNUSED_skill, int _experience)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAddExpServer>().Setup(_entityId, _experience), false);
		}
	}

	public void AddScoreServer(int _entityId, int _zombieKills, int _playerKills, int _otherTeamnumber, int _conditions)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAddScoreServer>().Setup(_entityId, _zombieKills, _playerKills, _otherTeamnumber, _conditions), false);
			return;
		}
		EntityAlive entityAlive = (EntityAlive)this.m_World.GetEntity(_entityId);
		if (entityAlive == null)
		{
			return;
		}
		if (entityAlive.isEntityRemote)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAddScoreClient>().Setup(_entityId, _zombieKills, _playerKills, _otherTeamnumber, _conditions), false, entityAlive.entityId, -1, -1, null, 192, false);
			return;
		}
		entityAlive.AddScore(0, _zombieKills, _playerKills, _otherTeamnumber, _conditions);
	}

	public void AwardKill(EntityAlive killer, EntityAlive killedEntity)
	{
		if (killer.isEntityRemote)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAwardKillServer>().Setup(killer.entityId, killedEntity.entityId), false, killer.entityId, -1, -1, null, 192, false);
			return;
		}
		QuestEventManager.Current.EntityKilled(killer, killedEntity);
	}

	public void ItemReloadServer(int _entityId)
	{
		if (this.m_World == null)
		{
			return;
		}
		this.ItemReloadClient(_entityId);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageItemReload>().Setup(_entityId), false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageItemReload>().Setup(_entityId), false, -1, _entityId, -1, null, 192, false);
	}

	public void ItemReloadClient(int _entityId)
	{
		if (this.m_World == null)
		{
			return;
		}
		EntityAlive entityAlive = (EntityAlive)this.m_World.GetEntity(_entityId);
		if (entityAlive != null && entityAlive.inventory.IsHoldingGun())
		{
			entityAlive.inventory.GetHoldingGun().ReloadGun(entityAlive.inventory.holdingItemData.actionData[0]);
		}
	}

	public void ItemActionEffectsServer(int _entityId, int _slotIdx, int _itemActionIdx, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		if (this.m_World == null)
		{
			return;
		}
		this.ItemActionEffectsClient(_entityId, _slotIdx, _itemActionIdx, _firingState, _startPos, _direction, _userData);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageItemActionEffects>().Setup(_entityId, _slotIdx, _itemActionIdx, (ItemActionFiringState)_firingState, _startPos, _direction, _userData), false);
			return;
		}
		int num = _entityId;
		Entity entity = this.m_World.GetEntity(_entityId);
		if (entity != null && entity.AttachedMainEntity != null)
		{
			num = entity.AttachedMainEntity.entityId;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageItemActionEffects>().Setup(_entityId, _slotIdx, _itemActionIdx, (ItemActionFiringState)_firingState, _startPos, _direction, _userData), false, -1, num, _entityId, null, 192, false);
	}

	public void ItemActionEffectsClient(int _entityId, int _slotIdx, int _itemActionIdx, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		if (this.m_World == null)
		{
			return;
		}
		EntityAlive entityAlive = (EntityAlive)this.m_World.GetEntity(_entityId);
		if (entityAlive == null)
		{
			return;
		}
		ItemAction itemActionInSlot = entityAlive.inventory.GetItemActionInSlot(_slotIdx, _itemActionIdx);
		if (itemActionInSlot == null)
		{
			return;
		}
		itemActionInSlot.ItemActionEffects(this, entityAlive.inventory.GetItemActionDataInSlot(_slotIdx, _itemActionIdx), _firingState, _startPos, _direction, _userData);
	}

	public void SetWorldTime(ulong _worldTime)
	{
		if (this.m_World == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			_worldTime = this.m_World.worldTime;
		}
		this.m_World.SetTime(_worldTime);
	}

	public void AddVelocityToEntityServer(int _entityId, Vector3 _velToAdd)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAddVelocity>().Setup(_entityId, _velToAdd), false);
			return;
		}
		Entity entity = this.m_World.GetEntity(_entityId);
		if (entity != null)
		{
			entity.AddVelocity(_velToAdd);
		}
	}

	public void PickupBlockServer(Vector3i _blockPos, BlockValue _blockValue, int _playerId, PlatformUserIdentifierAbs persistentPlayerId = null)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePickupBlock>().Setup(_blockPos, _blockValue, _playerId, this.persistentLocalPlayer), false);
			return;
		}
		if (this.m_World.GetBlock(_blockPos).type != _blockValue.type)
		{
			return;
		}
		if (this.m_World.IsLocalPlayer(_playerId))
		{
			this.PickupBlockClient(_blockPos, _blockValue, _playerId);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePickupBlock>().Setup(_blockPos, _blockValue, _playerId, null), false, _playerId, -1, -1, null, 192, false);
		}
		BlockValue blockValue = ((_blockValue.Block.PickupSource != null) ? Block.GetBlockValue(_blockValue.Block.PickupSource, false) : BlockValue.Air);
		this.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_blockPos, blockValue, true)
		}, persistentPlayerId);
	}

	public void PickupBlockClient(Vector3i _blockPos, BlockValue _blockValue, int _playerId)
	{
		if (this.m_World.GetBlock(_blockPos).type != _blockValue.type)
		{
			return;
		}
		ItemStack itemStack = _blockValue.Block.OnBlockPickedUp(this.m_World, _blockPos, _blockValue, _playerId);
		QuestEventManager.Current.BlockPickedUp(_blockValue.Block.GetBlockName(), _blockPos);
		QuestEventManager.Current.ItemAdded(itemStack);
		foreach (EntityPlayerLocal entityPlayerLocal in this.m_World.GetLocalPlayers())
		{
			if (entityPlayerLocal.entityId == _playerId && entityPlayerLocal.PlayerUI.xui.PlayerInventory.AddItem(itemStack, true))
			{
				return;
			}
		}
		this.ItemDropServer(itemStack, _blockPos.ToVector3() + Vector3.one * 0.5f, Vector3.zero, _playerId, 60f, false);
	}

	public void PlaySoundAtPositionServer(Vector3 _pos, string _audioClipName, AudioRolloffMode _mode, int _distance, float _volumeScale = 1f)
	{
		this.PlaySoundAtPositionServer(_pos, _audioClipName, _mode, _distance, this.m_World.GetPrimaryPlayerId(), _volumeScale);
	}

	public void PlaySoundAtPositionServer(Vector3 _pos, string _audioClipName, AudioRolloffMode _mode, int _distance, int _entityId, float _volumeScale = 1f)
	{
		if (this.m_World == null)
		{
			return;
		}
		if (!GameManager.IsDedicatedServer)
		{
			Manager.BroadcastPlay(_pos, _audioClipName, 0f);
			if (this.m_World.aiDirector != null)
			{
				this.m_World.aiDirector.NotifyNoise(this.m_World.GetEntity(_entityId), _pos, _audioClipName, _volumeScale);
			}
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(_pos, _audioClipName, _mode, _distance, _entityId, _volumeScale), false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSoundAtPosition>().Setup(_pos, _audioClipName, _mode, _distance, _entityId, _volumeScale), false, -1, _entityId, -1, null, 192, false);
	}

	public void PlaySoundAtPositionClient(Vector3 _pos, string _audioClipName, AudioRolloffMode _mode, int _distance, float _volumeScale)
	{
		if (this.m_World == null)
		{
			return;
		}
		Manager.Play(_pos, _audioClipName, -1, false, _volumeScale);
		if (this.m_World.aiDirector != null)
		{
			this.m_World.aiDirector.NotifyNoise(null, _pos, _audioClipName, _volumeScale);
		}
	}

	public void WaypointInviteServer(Waypoint _waypoint, EnumWaypointInviteMode _inviteMode, int _inviterEntityId)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWaypoint>().Setup(_waypoint, _inviteMode, _inviterEntityId), false);
			return;
		}
		_waypoint = _waypoint.Clone();
		_waypoint.bTracked = false;
		if (_inviteMode != EnumWaypointInviteMode.Friends)
		{
			for (int i = 0; i < this.m_World.Players.list.Count; i++)
			{
				EntityPlayer entityPlayer = this.m_World.Players.list[i];
				if (entityPlayer.entityId != _inviterEntityId)
				{
					if (this.m_World.IsLocalPlayer(entityPlayer.entityId))
					{
						this.WaypointInviteClient(_waypoint, _inviteMode, _inviterEntityId, null);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWaypoint>().Setup(_waypoint, _inviteMode, _inviterEntityId), false, entityPlayer.entityId, -1, -1, null, 192, false);
					}
				}
			}
			return;
		}
		if (this.m_World.GetEntity(_inviterEntityId) as EntityPlayer == null)
		{
			return;
		}
		PersistentPlayerData playerDataFromEntityID = this.persistentPlayers.GetPlayerDataFromEntityID(_inviterEntityId);
		if (playerDataFromEntityID == null)
		{
			return;
		}
		for (int j = 0; j < this.m_World.Players.list.Count; j++)
		{
			EntityPlayer entityPlayer2 = this.m_World.Players.list[j];
			if (entityPlayer2.entityId != _inviterEntityId)
			{
				PersistentPlayerData persistentPlayerData = ((this.persistentPlayers != null) ? this.persistentPlayers.GetPlayerDataFromEntityID(entityPlayer2.entityId) : null);
				if (playerDataFromEntityID.IsAlly(persistentPlayerData))
				{
					if (this.m_World.IsLocalPlayer(entityPlayer2.entityId))
					{
						this.WaypointInviteClient(_waypoint, _inviteMode, _inviterEntityId, null);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWaypoint>().Setup(_waypoint, _inviteMode, _inviterEntityId), false, entityPlayer2.entityId, -1, -1, null, 192, false);
					}
				}
			}
		}
	}

	public void RemovePartyInvitesFromAllPlayers(EntityPlayer _player)
	{
		for (int i = 0; i < this.m_World.Players.list.Count; i++)
		{
			EntityPlayer entityPlayer = this.m_World.Players.list[i];
			if (entityPlayer != _player)
			{
				entityPlayer.RemovePartyInvite(_player.entityId);
			}
		}
	}

	public void WaypointInviteClient(Waypoint _waypoint, EnumWaypointInviteMode _inviteMode, int _inviterEntityId, EntityPlayerLocal _player = null)
	{
		if (_player == null)
		{
			_player = this.myEntityPlayerLocal;
		}
		if (_player == null)
		{
			return;
		}
		PersistentPlayerData playerDataFromEntityID = this.persistentPlayers.GetPlayerDataFromEntityID(_inviterEntityId);
		if (playerDataFromEntityID != null && playerDataFromEntityID.PlatformData.Blocked[EBlockType.TextChat].IsBlocked())
		{
			return;
		}
		if (_player.Waypoints.ContainsWaypoint(_waypoint))
		{
			return;
		}
		for (int i = 0; i < _player.WaypointInvites.Count; i++)
		{
			if (_player.WaypointInvites[i].Equals(_waypoint))
			{
				return;
			}
		}
		_player.WaypointInvites.Insert(0, _waypoint);
		XUiV_Window window = LocalPlayerUI.GetUIForPlayer(_player).xui.GetWindow("mapInvites");
		if (window != null && window.IsVisible)
		{
			((XUiC_MapInvitesList)window.Controller.GetChildById("invitesList")).UpdateInvitesList();
		}
		string strPlayerName = "?";
		EntityPlayer entityPlayer = this.m_World.GetEntity(_inviterEntityId) as EntityPlayer;
		if (entityPlayer != null)
		{
			strPlayerName = entityPlayer.PlayerDisplayName;
		}
		GeneratedTextManager.GetDisplayText(_waypoint.name, delegate(string _filtered)
		{
			GameManager.ShowTooltip(_player, string.Format(Localization.Get("tooltipInviteMarker", false, null), strPlayerName, _waypoint.bUsingLocalizationId ? Localization.Get(_filtered, false, null) : _filtered), false, false, 0f);
		}, true, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
	}

	public void QuestShareServer(NetPackageSharedQuest.SharedQuestData sqd)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(sqd), false);
			return;
		}
		if (this.m_World.IsLocalPlayer(sqd.sharedWithEntityID))
		{
			this.QuestShareClient(sqd, null);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(sqd), false, sqd.sharedWithEntityID, -1, -1, null, 192, false);
	}

	public void QuestShareClient(NetPackageSharedQuest.SharedQuestData sqd, EntityPlayerLocal _player = null)
	{
		if (_player == null)
		{
			_player = this.myEntityPlayerLocal;
		}
		if (_player == null)
		{
			return;
		}
		if (_player.QuestJournal.HasActiveQuestByQuestCode(sqd.questCode))
		{
			if (PartyQuests.AutoAccept)
			{
				Log.Out(string.Format("Ignoring received quest, already have one active with the quest code {0}:", sqd.questCode));
				for (int i = 0; i < _player.QuestJournal.quests.Count; i++)
				{
					Quest quest = _player.QuestJournal.quests[i];
					Log.Out(string.Format("  {0}.: id={1}, code={2}, name={3}, POI={4}, state={5}, owner={6}", new object[]
					{
						i,
						quest.ID,
						quest.QuestCode,
						quest.QuestClass.Name,
						quest.GetParsedText("{poi.name}"),
						quest.CurrentState,
						quest.SharedOwnerID
					}));
				}
			}
			return;
		}
		_player.AddSharedQuestEntry(new NetPackageSharedQuest.SharedQuestData(sqd));
	}

	public void SharedKillServer(int _entityID, int _killerID, float _xpModifier = 1f)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSharedPartyKill>().Setup(_entityID, _killerID), false);
			return;
		}
		EntityPlayer entityPlayer = (EntityPlayer)this.m_World.GetEntity(_killerID);
		EntityAlive entityAlive = this.m_World.GetEntity(_entityID) as EntityAlive;
		if (entityPlayer == null || entityAlive == null)
		{
			return;
		}
		int num = EntityClass.list[entityAlive.entityClass].ExperienceValue;
		num = (int)EffectManager.GetValue(PassiveEffects.ExperienceGain, entityAlive.inventory.holdingItemItemValue, (float)num, entityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		if (_xpModifier != 1f)
		{
			num = (int)((float)num * _xpModifier + 0.5f);
		}
		if (entityPlayer.IsInParty())
		{
			int num2 = entityPlayer.Party.MemberCountInRange(entityPlayer);
			num = (int)((float)num * (1f - 0.1f * (float)num2));
		}
		if (entityPlayer.Party != null)
		{
			for (int i = 0; i < entityPlayer.Party.MemberList.Count; i++)
			{
				EntityPlayer entityPlayer2 = entityPlayer.Party.MemberList[i];
				if (!(entityPlayer2 == entityPlayer) && Vector3.Distance(entityPlayer.position, entityPlayer2.position) < (float)GameStats.GetInt(EnumGameStats.PartySharedKillRange))
				{
					if (this.m_World.IsLocalPlayer(entityPlayer2.entityId))
					{
						this.SharedKillClient(entityAlive.entityClass, num, null, entityAlive.entityId, -1);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedPartyKill>().Setup(entityAlive.entityClass, num, _killerID, entityAlive.entityId), false, entityPlayer2.entityId, -1, -1, null, 192, false);
					}
				}
			}
		}
	}

	public void SharedKillClient(int _entityTypeID, int _xp, EntityPlayerLocal _player = null, int _entityID = -1, int _killerID = -1)
	{
		if (_player == null)
		{
			_player = this.myEntityPlayerLocal;
		}
		if (_player == null)
		{
			return;
		}
		string entityClassName = EntityClass.list[_entityTypeID].entityClassName;
		_xp = _player.Progression.AddLevelExp(_xp, "_xpFromParty", Progression.XPTypes.Kill, true, true, _killerID, null);
		_player.bPlayerStatsChanged = true;
		if (_xp > 0 && (Progression.ShowXPType == Progression.ShowXPTypes.All || Progression.ShowXPType == Progression.ShowXPTypes.NotificationsOnly))
		{
			GameManager.ShowTooltip(_player, string.Format(Localization.Get("ttPartySharedXPReceived", false, null), _xp), false, false, 0f);
		}
		QuestEventManager.Current.EntityKilled(_player, (_entityID == -1) ? null : (this.m_World.GetEntity(_entityID) as EntityAlive));
	}

	public IEnumerator ShowExitingGameUICoroutine()
	{
		bool flag = this.windowManager.IsWindowOpen(XUiC_ExitingGame.ID);
		this.windowManager.Open(XUiC_ExitingGame.ID, false);
		if (flag)
		{
			yield break;
		}
		yield return null;
		yield return null;
		yield break;
	}

	public static void ShowTooltipMP(EntityPlayer _player, string _text, string _alertSound = "")
	{
		if (_player is EntityPlayerLocal)
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, _text, string.Empty, _alertSound, null, false, false, 0f);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>().Setup(_text, _alertSound), false, _player.entityId, -1, -1, null, 192, false);
	}

	public static void ShowTooltip(EntityPlayerLocal _player, string _text, bool _showImmediately = false, bool _pinTooltip = false, float _timeout = 0f)
	{
		GameManager.ShowTooltip(_player, _text, null, null, null, _showImmediately, _pinTooltip, _timeout);
	}

	public static void ShowTooltip(EntityPlayerLocal _player, string _text, string _arg, string _alertSound = null, ToolTipEvent _handler = null, bool _showImmediately = false, bool _pinTooltip = false, float _timeout = 0f)
	{
		GameManager.ShowTooltip(_player, _text, new string[] { _arg }, _alertSound, _handler, _showImmediately, false, _timeout);
	}

	public static void ShowTooltip(EntityPlayerLocal _player, string _text, string[] _args, string _alertSound = null, ToolTipEvent _handler = null, bool _showImmediately = false, bool _pinTooltip = false, float _timeout = 0f)
	{
		if (GameManager.IsDedicatedServer || _player == null)
		{
			return;
		}
		XUiC_PopupToolTip.QueueTooltip(LocalPlayerUI.GetUIForPlayer(_player).nguiWindowManager.WindowManager.playerUI.xui, _text, _args, _alertSound, _handler, _showImmediately, _pinTooltip, _timeout);
	}

	public static void RemovePinnedTooltip(EntityPlayerLocal _player, string _key)
	{
		if (GameManager.IsDedicatedServer || _player == null)
		{
			return;
		}
		XUiC_PopupToolTip.RemovePinnedTooltip(LocalPlayerUI.GetUIForPlayer(_player).nguiWindowManager.WindowManager.playerUI.xui, _key);
	}

	public void ClearTooltips(NGUIWindowManager _nguiWindowManager)
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		XUiC_PopupToolTip.ClearTooltips(_nguiWindowManager.WindowManager.playerUI.xui);
	}

	public void ClearCurrentTooltip(NGUIWindowManager _nguiWindowManager)
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		XUiC_PopupToolTip.ClearCurrentTooltip(_nguiWindowManager.WindowManager.playerUI.xui);
	}

	public void SetToolTipPause(NGUIWindowManager _nguiWindowManager, bool _isPaused)
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		XUiC_PopupToolTip.SetToolTipPause(_nguiWindowManager.WindowManager.playerUI.xui, _isPaused);
	}

	public static void ShowSubtitle(XUi _xui, string speaker, string content, float duration, bool centerAlign = false)
	{
		XUiC_SubtitlesDisplay.DisplaySubtitle(_xui.playerUI, speaker, content, duration, centerAlign);
	}

	public static void PlayVideo(string id, bool skippable, XUiC_VideoPlayer.DelegateOnVideoFinished callback = null)
	{
		XUiC_VideoPlayer.PlayVideo(LocalPlayerUI.primaryUI.xui, VideoManager.GetVideoData(id), skippable, callback);
	}

	public static bool IsVideoPlaying()
	{
		return XUiC_VideoPlayer.IsVideoPlaying;
	}

	public void CheckDestroyTileEntity(ITileEntity _te, Vector3i _blockPos)
	{
		ITileEntityLootable tileEntityLootable = _te as ITileEntityLootable;
		if (tileEntityLootable == null)
		{
			return;
		}
		if (!tileEntityLootable.ShouldDestroyOnClose())
		{
			return;
		}
		BlockValue block = this.m_World.GetBlock(_blockPos);
		this.DropContentOfLootContainerServer(block, _blockPos, null);
		block.Block.DamageBlock(this.m_World, _blockPos, block, block.Block.MaxDamage, -1, null, false, false);
	}

	public void DropContentOfLootContainerServer(BlockValue _bvOld, Vector3i _worldPos, ITileEntityLootable _teOld = null)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Log.Warning("DropContentOfLootContainerServer can not be called on clients! From:\n" + StackTraceUtility.ExtractStackTrace());
			return;
		}
		string text = "DroppedLootContainer";
		ITileEntityLootable tileEntityLootable = _teOld ?? this.m_World.GetTileEntity(_worldPos);
		ITileEntityLootable tileEntityLootable2 = ((tileEntityLootable != null) ? tileEntityLootable.GetSelfOrFeature<ITileEntityLootable>() : null);
		if (tileEntityLootable2 == null || LockManager.Instance.IsLockedServer(tileEntityLootable2, 0))
		{
			return;
		}
		ITileEntityLootable tileEntityLootable3 = tileEntityLootable2;
		Vector3 vector = tileEntityLootable3.ToWorldPos().ToVector3() + new Vector3(0.5f, 0.75f, 0.5f);
		if (_bvOld.Block.Properties.Values.ContainsKey("DroppedEntityClass"))
		{
			text = _bvOld.Block.Properties.Values["DroppedEntityClass"];
		}
		if (!tileEntityLootable3.bTouched)
		{
			this.lootManager.LootContainerOpened(tileEntityLootable3, -1, _bvOld.Block.Tags);
		}
		if (!tileEntityLootable3.IsEmpty())
		{
			EntityLootContainer entityLootContainer = EntityFactory.CreateEntity(text.GetHashCode(), vector, Vector3.zero) as EntityLootContainer;
			if (entityLootContainer != null)
			{
				entityLootContainer.SetContent(ItemStack.Clone(tileEntityLootable3.items));
			}
			this.m_World.SpawnEntityInWorld(entityLootContainer);
		}
		tileEntityLootable3.SetEmpty();
	}

	public void DropContentInLootContainerServer(int _droppedByID, string _containerEntity, Vector3 _pos, ItemStack[] _items, bool _skipIfEmpty = false, Vector3? increment = null)
	{
		List<EntityLootContainer> list = new List<EntityLootContainer>();
		if (_skipIfEmpty && ItemStack.IsEmpty(_items))
		{
			return;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageDropItemsContainer>().Setup(_droppedByID, _containerEntity, _pos, _items), false);
			return;
		}
		_pos.y += 0.25f;
		int hashCode = _containerEntity.GetHashCode();
		Vector2i size = LootContainer.GetLootContainer(EntityClass.list[hashCode].Properties.GetString(EntityClass.PropLootList), true).size;
		int num = size.x * size.y;
		int i = 0;
		int num2 = _items.Length;
		while (i < num2)
		{
			EntityLootContainer entityLootContainer = EntityFactory.CreateEntity(_containerEntity.GetHashCode(), _pos, Vector3.zero) as EntityLootContainer;
			if (increment != null)
			{
				_pos += increment.Value;
			}
			if (entityLootContainer)
			{
				int num3 = num2 - i;
				int num4 = Math.Min(num, num3);
				entityLootContainer.SetContent(ItemStack.Clone(_items, i, num4));
				entityLootContainer.spawnById = _droppedByID;
				this.m_World.SpawnEntityInWorld(entityLootContainer);
				i += num4;
				list.Add(entityLootContainer);
			}
		}
	}

	public GameStateManager GetGameStateManager()
	{
		return this.gameStateManager;
	}

	public void IdMappingReceived(string _name, byte[] _data)
	{
		Log.Out("Received mapping data for: " + _name);
		if (_name == "blocks")
		{
			Block.nameIdMapping = new NameIdMapping(null, Block.MAX_BLOCKS);
			Block.nameIdMapping.LoadFromArray(_data);
			return;
		}
		if (!(_name == "items"))
		{
			Log.Warning("Unknown mapping received for: " + _name);
			return;
		}
		ItemClass.nameIdMapping = new NameIdMapping(null, ItemClass.MAX_ITEMS);
		ItemClass.nameIdMapping.LoadFromArray(_data);
	}

	public void SetSpawnPointList(SpawnPointList _startPoints)
	{
		base.StartCoroutine(this.setSpawnPointListCo(_startPoints));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setSpawnPointListCo(SpawnPointList _startPoints)
	{
		while (!this.chunkClusterLoaded && SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			yield return null;
		}
		if (!this.chunkClusterLoaded)
		{
			yield break;
		}
		this.m_World.ChunkCache.ChunkProvider.SetSpawnPointList(_startPoints);
		yield break;
	}

	public void RequestToSpawnEntityServer(EntityCreationData _ecd)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageRequestToSpawnEntity>().Setup(_ecd), false);
			return;
		}
		if (_ecd.entityClass == "fallingTree".GetHashCode())
		{
			for (int i = 0; i < this.m_World.Entities.list.Count; i++)
			{
				if (this.m_World.Entities.list[i] is EntityFallingTree && ((EntityFallingTree)this.m_World.Entities.list[i]).GetBlockPos() == _ecd.blockPos)
				{
					return;
				}
			}
		}
		Entity entity = EntityFactory.CreateEntity(_ecd);
		EntityBackpack entityBackpack = entity as EntityBackpack;
		if (entityBackpack != null)
		{
			foreach (PersistentPlayerData persistentPlayerData in this.persistentPlayers.Players.Values)
			{
				if (persistentPlayerData.EntityId == entityBackpack.RefPlayerId)
				{
					uint num = GameUtils.WorldTimeToTotalMinutes(this.m_World.worldTime);
					persistentPlayerData.AddDroppedBackpack(entity.entityId, new Vector3i(_ecd.pos), num);
					break;
				}
			}
		}
		this.m_World.SpawnEntityInWorld(entity);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LocalPlayerInventoryChanged()
	{
		this.countdownSendPlayerInventoryToServer.ResetAndRestart();
	}

	public void TriggerSendOfLocalPlayerDataFile(float _sendItInSeconds)
	{
		this.countdownSendPlayerDataFileToServer.SetPassedIn(_sendItInSeconds);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doSendLocalInventory(EntityPlayerLocal _player)
	{
		if (_player == null)
		{
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerInventory>().Setup(_player, this.sendPlayerToolbelt, this.sendPlayerBag, this.sendPlayerEquipment, this.sendDragAndDropItem), false);
		this.sendPlayerToolbelt = false;
		this.sendPlayerBag = false;
		this.sendPlayerEquipment = false;
		this.sendDragAndDropItem = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doSendLocalPlayerData(EntityPlayerLocal _player)
	{
		if (_player == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.SaveLocalPlayerData();
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerData>().Setup(_player), false);
		this.sendPlayerToolbelt = false;
		this.sendPlayerBag = false;
		this.sendPlayerEquipment = false;
		this.sendDragAndDropItem = false;
	}

	public void SetPauseWindowEffects(bool _bOn)
	{
		if (_bOn && !GameModeSurvivalSP.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
		{
			foreach (EntityPlayerLocal entityPlayerLocal in this.m_World.GetLocalPlayers())
			{
				if (entityPlayerLocal.AimingGun)
				{
					entityPlayerLocal.AimingGun = false;
				}
			}
		}
	}

	public static bool ReportUnusedAssets(bool bStart = false)
	{
		if (bStart)
		{
			if (GameManager.materialsBefore == null)
			{
				GameManager.materialsBefore = new List<string>();
			}
			else
			{
				GameManager.materialsBefore.Clear();
			}
			Material[] array = Resources.FindObjectsOfTypeAll<Material>();
			for (int i = 0; i < array.Length; i++)
			{
				GameManager.materialsBefore.Add(array[i].name);
			}
			Resources.UnloadUnusedAssets();
			GC.Collect();
			GameManager.runningAssetsUnused = true;
			GameManager.unusedAssetsTimer = Time.realtimeSinceStartup;
			GameManager.Instance.Pause(true);
		}
		else
		{
			if (GameManager.materialsBefore == null)
			{
				return true;
			}
			if (!GameManager.runningAssetsUnused)
			{
				return true;
			}
			if (Time.realtimeSinceStartup < GameManager.unusedAssetsTimer + 5f)
			{
				return false;
			}
			Material[] array2 = Resources.FindObjectsOfTypeAll<Material>();
			if (GameManager.materialsBefore.Count == array2.Length)
			{
				Log.Out("No unused assets found. ( " + GameManager.materialsBefore.Count.ToString() + " materials found. )");
			}
			else
			{
				Log.Out("Material before: " + GameManager.materialsBefore.Count.ToString());
				Log.Out("Material after: " + array2.Length.ToString());
				string text = "Material Diff: ";
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				for (int j = 0; j < array2.Length; j++)
				{
					int num;
					if (dictionary.TryGetValue(array2[j].name, out num))
					{
						num++;
					}
					else
					{
						dictionary.Add(array2[j].name, 1);
					}
				}
				for (int k = 0; k < GameManager.materialsBefore.Count; k++)
				{
					if (!dictionary.ContainsKey(GameManager.materialsBefore[k]))
					{
						text = text + GameManager.materialsBefore[k] + ", ";
					}
				}
				Log.Out(text);
			}
			GameManager.Instance.Pause(false);
			GameManager.runningAssetsUnused = false;
		}
		return true;
	}

	public bool IsPaused()
	{
		return this.gamePaused;
	}

	public void Pause(bool _bOn)
	{
		this.requestedPauseState = new bool?(_bOn);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePauseState()
	{
		if (this.requestedPauseState == null)
		{
			return;
		}
		bool flag = this.requestedPauseState.Value;
		this.requestedPauseState = null;
		if (flag == this.gamePaused)
		{
			return;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsSinglePlayer || GameModeEditWorld.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)) || GameStats.GetInt(EnumGameStats.GameState) == 0)
		{
			flag = false;
		}
		this.SetPauseWindowEffects(flag);
		if (flag)
		{
			GameStats.Set(EnumGameStats.GameState, 2);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.SaveLocalPlayerData();
				this.SaveWorld();
			}
			Time.timeScale = 0f;
			if (this.World.GetPrimaryPlayer() != null)
			{
				this.triggerEffectManager.StopGamepadVibration(false);
			}
		}
		else
		{
			if (GameStats.GetInt(EnumGameStats.GameState) != 0)
			{
				GameStats.Set(EnumGameStats.GameState, 1);
			}
			Time.timeScale = 1f;
		}
		if (this.gamePaused != flag && this.World != null)
		{
			if (flag)
			{
				Manager.PauseGameplayAudio();
				EnvironmentAudioManager.Instance.Pause();
				this.m_World.dmsConductor.OnPauseGame();
			}
			else
			{
				Manager.UnPauseGameplayAudio();
				EnvironmentAudioManager.Instance.UnPause();
				this.m_World.dmsConductor.OnUnPauseGame();
			}
		}
		this.gamePaused = flag;
	}

	public void AddLMPPersistentPlayerData(EntityPlayerLocal _playerEntity)
	{
	}

	public void SetBlockTextureServer(BlockValueRef _bvRef, BlockFace _blockFace, int _idx, int _playerIdThatChanged, byte _channel = 255)
	{
		this.SetBlockTextureClient(_bvRef, _blockFace, _idx, _channel);
		NetPackageSetBlockTexture netPackageSetBlockTexture = NetPackageManager.GetPackage<NetPackageSetBlockTexture>().Setup(_bvRef, _blockFace, _idx, GameManager.IsDedicatedServer ? (-1) : this.myPlayerId, _channel);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackageSetBlockTexture, false, -1, -1, -1, null, 192, false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(netPackageSetBlockTexture, false);
	}

	public void SetBlockTextureClient(BlockValueRef _bvRef, BlockFace _blockFace, int _idx, byte _channel)
	{
		DynamicMeshManager.ChunkChanged(_bvRef, -1, 1);
		int num;
		int num2;
		if (_channel == 255)
		{
			num = 0;
			num2 = 0;
		}
		else
		{
			if (_channel >= 1)
			{
				Log.Error(string.Format("Specified texture channel \"{0}\" is out of range of the project channel count of \"{1}\".", _channel, 1));
				return;
			}
			num2 = (int)_channel;
			num = (int)_channel;
		}
		for (int i = num; i <= num2; i++)
		{
			if (_blockFace != BlockFace.None)
			{
				this.m_World.ChunkCache.SetBlockFaceTexture(_bvRef, _blockFace, _idx, i);
			}
			else
			{
				long num3 = (long)_idx;
				long num4 = num3 | (num3 << 8) | (num3 << 16) | (num3 << 24) | (num3 << 32) | (num3 << 40);
				this.m_World.ChunkCache.SetTextureFull(_bvRef, num4, i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleGlobalActions()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (PlayerActionsGlobal.Instance.Console.WasPressed)
		{
			GUIWindowConsole.Open();
		}
		if (PlayerActionsGlobal.Instance.Fullscreen.WasPressed)
		{
			Screen.fullScreen = !Screen.fullScreen;
		}
		if (PlayerActionsGlobal.Instance.Screenshot.WasPressed)
		{
			Manager.PlayButtonClick();
			GameUtils.TakeScreenShot(GameUtils.EScreenshotMode.Both, null, 0f, false, 0, 0, InputUtils.ControlKeyPressed);
		}
		if (PlayerActionsGlobal.Instance.DebugScreenshot.WasPressed)
		{
			Manager.PlayButtonClick();
			LocalPlayerUI.primaryUI.windowManager.Open(GUIWindowScreenshotText.ID, false);
		}
		if (LocalPlayerUI.primaryUI != null)
		{
			IPlatform nativePlatform = PlatformManager.NativePlatform;
			bool? flag;
			if (nativePlatform == null)
			{
				flag = null;
			}
			else
			{
				PlayerActionsLocal primaryPlayer = nativePlatform.Input.PrimaryPlayer;
				if (primaryPlayer == null)
				{
					flag = null;
				}
				else
				{
					PlayerActionsGUI guiactions = primaryPlayer.GUIActions;
					if (guiactions == null)
					{
						flag = null;
					}
					else
					{
						PlayerAction focusSearch = guiactions.FocusSearch;
						flag = ((focusSearch != null) ? new bool?(focusSearch.WasPressed) : null);
					}
				}
			}
			bool? flag2 = flag;
			if (flag2.GetValueOrDefault())
			{
				XUiC_TextInput.SelectCurrentSearchField(LocalPlayerUI.primaryUI);
			}
		}
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (uiforPrimaryPlayer != null)
		{
			PlayerActionsLocal playerInput = uiforPrimaryPlayer.playerInput;
			bool? flag3;
			if (playerInput == null)
			{
				flag3 = null;
			}
			else
			{
				PlayerActionsGUI guiactions2 = playerInput.GUIActions;
				if (guiactions2 == null)
				{
					flag3 = null;
				}
				else
				{
					PlayerAction focusSearch2 = guiactions2.FocusSearch;
					flag3 = ((focusSearch2 != null) ? new bool?(focusSearch2.WasPressed) : null);
				}
			}
			bool? flag2 = flag3;
			if (flag2.GetValueOrDefault())
			{
				XUiC_TextInput.SelectCurrentSearchField(uiforPrimaryPlayer);
			}
		}
	}

	public static bool IsSplatMapAvailable()
	{
		string @string = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		return !(@string == "Empty") && !(@string == "Playtesting");
	}

	public static bool UpdatingRemoteResources
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static bool RemoteResourcesLoaded
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static void LoadRemoteResources(GameManager.RemoteResourcesCompleteHandler _callback = null)
	{
		if (GameManager.UpdatingRemoteResources)
		{
			return;
		}
		NewsManager.Instance.UpdateNews(false);
		if (BlockedPlayerList.Instance != null)
		{
			GameManager.Instance.StartCoroutine(BlockedPlayerList.Instance.ReadStorageAndResolve());
		}
		DLCTitleStorageManager.Instance.FetchFromSource();
		if (PlatformManager.NativePlatform.User.UserStatus == EUserStatus.LoggedIn)
		{
			GameManager.Instance.StartCoroutine(GameManager.Instance.UpdateRemoteResourcesRoutine(_callback));
			return;
		}
		GameManager.UpdatingRemoteResources = false;
		GameManager.RemoteResourcesLoaded = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator UpdateRemoteResourcesRoutine(GameManager.RemoteResourcesCompleteHandler _callback)
	{
		IRemoteFileStorage storage = PlatformManager.MultiPlatform.RemoteFileStorage;
		if (storage == null)
		{
			GameManager.RemoteResourcesLoaded = true;
			yield break;
		}
		GameManager.UpdatingRemoteResources = true;
		float readyTime = Time.time;
		while (!storage.IsReady)
		{
			yield return null;
			if (Time.time - readyTime > 3f)
			{
				Log.Warning("Waiting for remote resources timed out");
				GameManager.UpdatingRemoteResources = false;
				GameManager.RemoteResourcesLoaded = true;
				yield break;
			}
		}
		this.retrievingEula = true;
		string text = string.Format("eula_{0}", Localization.ActiveLanguage.ToLower());
		storage.GetFile(text, new IRemoteFileStorage.FileDownloadCompleteCallback(this.EulaProviderCallback));
		while (this.retrievingEula)
		{
			yield return null;
		}
		if (BacktraceUtils.Initialized)
		{
			this.retrievingBacktraceConfig = true;
			storage.GetFile("backtraceconfig.xml", new IRemoteFileStorage.FileDownloadCompleteCallback(this.BacktraceConfigProviderCallback));
			while (this.retrievingBacktraceConfig)
			{
				yield return null;
			}
		}
		GameManager.UpdatingRemoteResources = false;
		GameManager.RemoteResourcesLoaded = true;
		if (_callback != null)
		{
			_callback();
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EulaProviderCallback(IRemoteFileStorage.EFileDownloadResult _result, string _errorDetails, byte[] _data)
	{
		this.retrievingEula = false;
		if (_result != IRemoteFileStorage.EFileDownloadResult.Ok)
		{
			Log.Warning(string.Concat(new string[]
			{
				"Retrieving EULA file failed: ",
				_result.ToStringCached<IRemoteFileStorage.EFileDownloadResult>(),
				" (",
				_errorDetails,
				")"
			}));
			return;
		}
		string text;
		if (this.LoadEulaXML(_data, out text))
		{
			XUiC_EulaWindow.RetrievedEula = text;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool LoadEulaXML(byte[] _data, out string contents)
	{
		contents = "";
		XmlFile xmlFile;
		try
		{
			xmlFile = new XmlFile(_data, true);
		}
		catch (Exception ex)
		{
			Log.Error("Failed loading EULA XML: {0}", new object[] { ex.Message });
			return false;
		}
		XElement root = xmlFile.XmlDoc.Root;
		if (root == null)
		{
			return false;
		}
		int num = int.Parse(root.GetAttribute("version").Trim());
		contents = root.Value;
		if (num > GamePrefs.GetInt(EnumGamePrefs.EulaLatestVersion))
		{
			GamePrefs.Set(EnumGamePrefs.EulaLatestVersion, num);
		}
		return true;
	}

	public static bool HasAcceptedLatestEula()
	{
		return GamePrefs.GetInt(EnumGamePrefs.EulaVersionAccepted) >= GamePrefs.GetInt(EnumGamePrefs.EulaLatestVersion);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BacktraceConfigProviderCallback(IRemoteFileStorage.EFileDownloadResult _result, string _errorDetails, byte[] _data)
	{
		this.retrievingBacktraceConfig = false;
		if (_result != IRemoteFileStorage.EFileDownloadResult.Ok)
		{
			Log.Warning(string.Concat(new string[]
			{
				"Retrieving Backtrace config file failed: ",
				_result.ToStringCached<IRemoteFileStorage.EFileDownloadResult>(),
				" (",
				_errorDetails,
				")"
			}));
			return;
		}
		try
		{
			BacktraceUtils.UpdateConfig(new XmlFile(_data, true));
		}
		catch (Exception ex)
		{
			Log.Error("Failed loading Backtrace config XML: {0}", new object[] { ex.Message });
		}
	}

	public bool IsGoreCensored()
	{
		return GameManager.DebugCensorship;
	}

	public int persistentPlayerCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.persistentPlayerIds.Count;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalculatePersistentPlayerCount(string worldName, string saveName, UserDataStorageType storage)
	{
		this.persistentPlayerIds = new List<string>();
		string text = GameIO.GetSaveGameDir(worldName, saveName, storage) + "/Player";
		if (!SdDirectory.Exists(text))
		{
			Log.Warning("save folder does not exist");
			return;
		}
		foreach (SdFileSystemInfo sdFileSystemInfo in new SdDirectoryInfo(text).GetFileSystemInfos())
		{
			int num;
			string text2;
			if ((num = sdFileSystemInfo.Name.IndexOf('.')) != -1)
			{
				text2 = sdFileSystemInfo.Name.Substring(0, num);
			}
			else
			{
				text2 = sdFileSystemInfo.Name;
			}
			if (!this.persistentPlayerIds.Contains(text2))
			{
				this.persistentPlayerIds.Add(text2);
			}
		}
	}

	public void OnResolutionChanged(int width, int height)
	{
		this.RefreshRefreshRate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshRefreshRate()
	{
		this.currentRefreshRate = (int)PlatformApplicationManager.Application.GetCurrentRefreshRate().value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateFPSCap()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		int num = (this.GameHasStarted ? (-1) : this.currentRefreshRate);
		if (Application.targetFrameRate != num)
		{
			Application.targetFrameRate = num;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cMinSpawnDistanceFromTrader = 250;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cMaxSpawnDistanceFromTrader = 750;

	public static int frameCount;

	public static float frameTime;

	public static int fixedUpdateCount;

	public AudioSource UIAudioSource;

	public AudioClip BackgroundMusicClip;

	public AudioClip CreditsSongClip;

	public bool DebugAILines;

	public bool DebugAIAim;

	public StabilityViewer stabilityViewer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BiomeParticleManager biomeParticleManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int cameraCullMask;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bShowBackground = true;

	public static bool enableNetworkdPrioritization = true;

	public static bool unreliableNetPackets = true;

	public static ServerDateTimeResult ServerClockSync;

	public NetPackageMetrics netpackageMetrics;

	public bool showOpenerMovieOnLoad;

	public bool GameHasStarted;

	public RuntimeAnimatorController FirstPersonWeaponAnimatorController;

	public RuntimeAnimatorController ThirdPersonWeaponAnimatorController;

	public Color backgroundColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int currentBackgroundColorChannel;

	public static bool bPhysicsActive;

	public static bool bTickingActive;

	public static bool bSavingActive = true;

	public static bool bShowDecorBlocks = true;

	public static bool bShowLootBlocks = true;

	public static bool bShowPaintables = true;

	public static bool bShowUnpaintables = true;

	public static bool bShowTerrain = true;

	public static bool bVolumeBlocksEditing;

	public static bool bHideMainMenuNextTime;

	public static bool bRecordNextSession;

	public static bool bPlayRecordedSession;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool isDedicatedChecked = false;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool isDedicated = false;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public World m_World;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool worldCreated;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool chunkClusterLoaded;

	public bool worldInitInfoReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int myPlayerId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityPlayerLocal myEntityPlayerLocal;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public IMapChunkDatabase fowDatabaseForLocalPlayer;

	public FPS fps = new FPS(5f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject m_SoundsGameObject;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastTimeWorldTickTimeSentToClients;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastTimeGameStateCheckedAndSynced;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastTimeDecoSaved;

	public AdminTools adminTools;

	public PersistentPlayerList persistentPlayers;

	public PersistentPlayerData persistentLocalPlayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GUIWindowManager windowManager;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public NGUIWindowManager nguiWindowManager;

	public LootManager lootManager;

	public TraderManager traderManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int lastDisplayedValueOfTeamTickets;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<Vector3i, GameObject> m_PositionSoundMap = new Dictionary<Vector3i, GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<GameObject> tileEntitiesMusicToRemove = new List<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int msPassedSinceLastUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public IBlockTool activeBlockTool;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public IBlockTool blockSelectionTool;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isEditMode;

	public bool bCursorVisible = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bCursorVisibleOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bCursorVisibleOverrideState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public DictionarySave<Vector3i, Transform> m_BlockParticles = new DictionarySave<Vector3i, Transform>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CountdownTimer countdownCheckBlockParticles = new CountdownTimer(1.1f, true);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CountdownTimer countdownSendPlayerDataFileToServer = new CountdownTimer(30f, true);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CountdownTimer countdownSaveLocalPlayerDataFile = new CountdownTimer(30f, true);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float unloadAssetsDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isUnloadAssetsReady;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly MicroStopwatch stopwatchUnloadAssets = new MicroStopwatch(false);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CountdownTimer countdownSendPlayerInventoryToServer = new CountdownTimer(0.1f, false);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool sendPlayerToolbelt;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool sendPlayerBag;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool sendPlayerEquipment;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool sendDragAndDropItem;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameRandomManager gameRandomManager;

	public GameStateManager gameStateManager;

	public PrefabLODManager prefabLODManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public RespawnType clientRespawnType;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fpsCountdownTimer = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float gcCountdownTimer = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float wsCountdownTimer = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float playerPositionsCountdownTimer = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MicroStopwatch swCopyChunks = new MicroStopwatch();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MicroStopwatch swUpdateTime = new MicroStopwatch();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int lastStatsPlayerCount;

	public static long MaxMemoryConsumption;

	public WaitForTargetFPS waitForTargetFPS;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<GameManager.BlockParticleCreationData> blockParticlesToSpawn = new List<GameManager.BlockParticleCreationData>();

	public static GameManager Instance;

	public TriggerEffectManager triggerEffectManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int lastPlayerCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int currentRefreshRate = 60;

	public bool bStaticDataLoadSync;

	public bool bStaticDataLoaded;

	public string CurrentLoadAction;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int lastTimeAbsPosSentToServer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bLastWasAttached;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeToClearAllPools = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float activityCheck;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool shuttingDownMultiplayerServices;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int testing;

	public bool canSpawnPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isDisconnectingLater;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool allowQuit;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isQuitting;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool firstTimeJoin;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<BlockChangeInfo> tempExplPositions = new List<BlockChangeInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<GameManager.ExplodeGroup> explodeFallingGroups = new List<GameManager.ExplodeGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<ChunkCluster> ccChanged = new List<ChunkCluster>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<string> materialsBefore = null;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float unusedAssetsTimer = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool runningAssetsUnused = false;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool? requestedPauseState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool gamePaused;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool retrievingEula;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool retrievingBacktraceConfig;

	public static bool DebugCensorship;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<string> persistentPlayerIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct BlockParticleCreationData
	{
		public BlockParticleCreationData(Vector3i _blockPos, ParticleEffect _particleEffect)
		{
			this.blockPos = _blockPos;
			this.particleEffect = _particleEffect;
		}

		public Vector3i blockPos;

		public ParticleEffect particleEffect;
	}

	public delegate void OnWorldChangedEvent(World _world);

	public delegate void OnLocalPlayerChangedEvent(EntityPlayerLocal _localPlayer);

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EMultiShutReason
	{
		AppNoNetwork,
		AppSuspended,
		PermMissingMultiplayer,
		PermMissingCrossplay
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class ExplodeGroup
	{
		public Vector3 pos;

		public float radius;

		public int delay;

		public List<GameManager.ExplodeGroup.Falling> fallings = new List<GameManager.ExplodeGroup.Falling>();

		public struct Falling
		{
			public Vector3i pos;

			public BlockValue bv;
		}
	}

	public class EntityItemLifetimeComparer : IComparer<EntityItem>
	{
		public int Compare(EntityItem _obj1, EntityItem _obj2)
		{
			return (int)(_obj2.lifetime - _obj1.lifetime);
		}
	}

	public delegate void RemoteResourcesCompleteHandler();
}
