using System;
using System.Collections;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BhvrAnalyticsServices.Interfaces;
using Platform;
using Platform.EOS;
using Services;
using Services.Analytics.Events;
using UnityEngine;
using WorldGenerationEngineFinal;

public class MainMenuMono : MonoBehaviour
{
	public static bool IsQuickContinue
	{
		get
		{
			return GameUtils.GetLaunchArgument("quick-continue") != null || (ToggleCapsLock.GetScrollLock() && Application.isEditor);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		this.windowManager = base.GetComponent<GUIWindowManager>();
		this.nguiWindowManager = base.GetComponent<NGUIWindowManager>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (GameManager.IsDedicatedServer)
		{
			ThreadManager.StartCoroutine(this.startDedicatedServer());
			return;
		}
		this.nguiWindowManager.SetLabelText(EnumNGUIWindow.Version, Constants.cVersionInformation.LongString, false);
		Cursor.visible = true;
		Cursor.lockState = SoftCursor.DefaultCursorLockState;
		this.nguiWindowManager.Show(EnumNGUIWindow.Loading, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckLogin()
	{
		this.loginCheckDone = true;
		this.bOpenMainMenu = false;
		XUiC_LoginBase.Login(this.windowManager.playerUI.xui, new Action(this.OnLoginComplete));
		return this.bOpenMainMenu;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLoginComplete()
	{
		if (!GameManager.RemoteResourcesLoaded)
		{
			GameManager.LoadRemoteResources(null);
		}
		base.StartCoroutine(this.LoginEventAnalyticCoroutine());
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.OfflineMode)
		{
			this.bOpenMainMenu = true;
			return;
		}
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.LoggedIn)
		{
			EUserPerms euserPerms = EUserPerms.All;
			if (!GamePrefs.GetBool(EnumGamePrefs.ServerEnabled))
			{
				euserPerms &= ~EUserPerms.HostMultiplayer;
			}
			this.nguiWindowManager.SetLabelText(EnumNGUIWindow.Loading, Localization.Get("xuiSteamLoginProgressCheckAccount", false, null) + "...", false);
			base.StartCoroutine(this.<OnLoginComplete>g__ResolveInitialPermissions|9_0(euserPerms));
			return;
		}
		string text = "Login complete but user is not in valid state. Native platform user status: {0}, Crossplatform user status: {1}";
		object obj = PlatformManager.NativePlatform.User.UserStatus;
		IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
		object obj2;
		if (crossplatformPlatform == null)
		{
			obj2 = null;
		}
		else
		{
			IUserClient user = crossplatformPlatform.User;
			obj2 = ((user != null) ? user.UserStatus.ToString() : null);
		}
		Log.Error(string.Format(text, obj, obj2 ?? "N/A"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator LoginEventAnalyticCoroutine()
	{
		Task<string> ipAddressTask = new WebClient().DownloadStringTaskAsync(new Uri("https://api.ipify.org"));
		yield return new WaitUntil(() => ipAddressTask.IsCompleted);
		LoginEventData loginEventData = new LoginEventData();
		loginEventData.SessionStartTimeStamp = DateTime.UtcNow.ToString("O");
		loginEventData.Platform = Application.platform.ToString();
		loginEventData.Provider = PlatformManager.NativePlatform.PlatformIdentifier.ToString();
		loginEventData.IP = ipAddressTask.Result;
		loginEventData.CrossplayEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsCrossplay);
		LoginEventData loginEventData2 = loginEventData;
		EUserAccountState userAccountState = EosHelpers.UserAccountState;
		bool? flag;
		if (userAccountState != EUserAccountState.NewUser)
		{
			if (userAccountState == EUserAccountState.Unknown)
			{
				flag = null;
			}
			else
			{
				flag = new bool?(false);
			}
		}
		else
		{
			flag = new bool?(true);
		}
		loginEventData2.IsFirstLaunchEos = flag;
		LoginEventData loginEventData3 = loginEventData;
		ServiceProvider.Instance.Get<IAnalyticsService>().LogEvent(loginEventData3);
		ServiceProvider.Instance.Get<IAnalyticsService>().LogEvent(new HardwareInfoEventData());
		SingletonMonoBehaviour<ConnectionManager>.Instance.BeginHeartbeat();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (!GameStartupHelper.Instance.OpenMainMenuAfterAwake)
		{
			return;
		}
		if (!GameManager.Instance.bStaticDataLoaded)
		{
			if (this.loadingText == "")
			{
				this.loadingText = Localization.Get("loadActionLoading", false, null);
			}
			if (GameManager.Instance.CurrentLoadAction != this.displayedLoadAction)
			{
				this.displayedLoadAction = GameManager.Instance.CurrentLoadAction;
				this.nguiWindowManager.SetLabelText(EnumNGUIWindow.Loading, this.loadingText + " " + this.displayedLoadAction + "...", false);
			}
		}
		else
		{
			if (this.windowManager.playerUI == null || this.windowManager.playerUI.xui == null || !this.windowManager.playerUI.xui.IsReady)
			{
				return;
			}
			if (!this.loginCheckDone && !this.CheckLogin())
			{
				return;
			}
			if (!this.bOpenMainMenu)
			{
				return;
			}
			if (this.nguiWindowManager.IsShowing(EnumNGUIWindow.Loading))
			{
				this.nguiWindowManager.Show(EnumNGUIWindow.Loading, false);
			}
			if (ModManager.GetFailedMods(new Mod.EModLoadState?(Mod.EModLoadState.NotAntiCheatCompatible)).Count > 0)
			{
				IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
				if (antiCheatClient != null && antiCheatClient.ClientAntiCheatEnabled())
				{
					XUiC_MessageBoxWindowGroup.ShowCustom(this.windowManager.playerUI.xui, Localization.Get("xuiModsAntiCheatModWithCodeTitle", false, null), Localization.Get("xuiModsAntiCheatModWithCodeText", false, null), delegate(XUiC_MessageBoxWindowGroup _wdw)
					{
						_wdw.Buttons[0].DefaultConfirm("xuiRestart", delegate
						{
							Utils.RestartGame(Utils.ERestartAntiCheatMode.ForceOff);
						}, true, 0f, 0f, false);
						_wdw.Buttons[2].DefaultCancel("btnContinue", delegate
						{
							XUiC_EulaWindow.Open(this.windowManager.playerUI.xui, false);
						}, true, 0f, 0f, false);
					}, false, true, -1);
					goto IL_018B;
				}
			}
			XUiC_EulaWindow.Open(this.windowManager.playerUI.xui, false);
		}
		IL_018B:
		this.bOpenMainMenu = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startDedicatedServer()
	{
		bool abort = false;
		bool serverUserReady = false;
		IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
		IUserClient userClient = ((crossplatformPlatform != null) ? crossplatformPlatform.UserServer : null);
		if (userClient != null)
		{
			userClient.Login(delegate(IPlatform _, EApiStatusReason _reason, string _text)
			{
				if (_reason != EApiStatusReason.Ok)
				{
					abort = true;
					this.abortServerStartupWithError(new string[]
					{
						"Error starting dedicated server: Failed initializing cross platform user:",
						_reason.ToStringCached<EApiStatusReason>() + ": " + _text
					});
					return;
				}
				serverUserReady = true;
			});
		}
		else
		{
			serverUserReady = true;
		}
		while (!serverUserReady && !abort)
		{
			yield return null;
		}
		if (abort)
		{
			yield break;
		}
		UserDataStorageType userDataStorageType = UserDataStorageType.DeviceLocal;
		GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, (int)userDataStorageType);
		GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)userDataStorageType);
		GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, 5);
		if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "RWG")
		{
			yield return this.startGeneration(userDataStorageType, delegate(bool _success)
			{
				abort = !_success;
			});
		}
		if (abort)
		{
			yield break;
		}
		yield return new WaitForSeconds(2.5f);
		GamePrefs.Set(EnumGamePrefs.ServerEnabled, true);
		yield return PermissionsManager.ResolvePermissions(EUserPerms.All, false, null);
		string @string = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		if (PathAbstractions.Contextual.FindActiveWorldLocation().Type == PathAbstractions.EAbstractedLocationType.None)
		{
			this.abortServerStartupWithError(new string[] { "Error starting dedicated server: GameWorld \"" + @string + "\" not found!" });
			yield break;
		}
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), false);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			this.abortServerStartupWithError(new string[]
			{
				"Error starting dedicated server: " + networkConnectionError.ToStringCached<NetworkConnectionError>(),
				"Make sure all required ports are unused: " + SingletonMonoBehaviour<ConnectionManager>.Instance.GetRequiredPortsString()
			});
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startGeneration(UserDataStorageType _worldStorageType, Action<bool> _finishedCallback)
	{
		string @string = GamePrefs.GetString(EnumGamePrefs.WorldGenSeed);
		int @int = GamePrefs.GetInt(EnumGamePrefs.WorldGenSize);
		string worldName = WorldBuilder.GetGeneratedWorldName(@string, @int);
		PathAbstractions.SearchDefinition worldsSearchPaths = PathAbstractions.WorldsSearchPaths;
		string text = worldName;
		UserDataStorageType? userDataStorageType = new UserDataStorageType?(_worldStorageType);
		PathAbstractions.AbstractedLocation location = worldsSearchPaths.GetLocation(text, null, userDataStorageType);
		if (location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			WorldBuilder worldBuilder = new WorldBuilder(@string, @int, _worldStorageType);
			yield return worldBuilder.GenerateFromServer();
		}
		else if (GameUtils.WorldInfo.LoadWorldInfo(location) == null)
		{
			this.abortServerStartupWithError(new string[]
			{
				string.Concat(new string[] { "Error starting dedicated server: Folder for requested RWG world \"", worldName, "\" to be generated from seed \"", @string, "\" already exists." }),
				"It does not contain a map_info.xml, so the world likely was never successfully generated!",
				"Either delete the folder or change the WorldGenSeed and/or WorldGenSize settings!",
				"(Path of the world: " + location.FullPath + ")"
			});
			_finishedCallback(false);
			yield break;
		}
		GamePrefs.Set(EnumGamePrefs.GameWorld, worldName);
		yield return new WaitForSeconds(2f);
		_finishedCallback(true);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void abortServerStartupWithError(params string[] _errorMessage)
	{
		Log.Error("====================================================================================================");
		for (int i = 0; i < _errorMessage.Length; i++)
		{
			Log.Error(_errorMessage[i]);
		}
		Log.Error("====================================================================================================");
		Application.Quit();
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator <OnLoginComplete>g__ResolveInitialPermissions|9_0(EUserPerms _perms)
	{
		yield return PermissionsManager.ResolvePermissions(_perms, false, null);
		this.bOpenMainMenu = true;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindowManager windowManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NGUIWindowManager nguiWindowManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bOpenMainMenu;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool loginCheckDone;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string loadingText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string displayedLoadAction = "";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float ServerStartDelaySec = 2.5f;
}
