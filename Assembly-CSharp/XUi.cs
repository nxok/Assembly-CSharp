using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio;
using Platform;
using UnityEngine;

public class XUi : MonoBehaviour
{
	public static bool InGameMenuOpen
	{
		get
		{
			return XUi.inGameMenuOpen;
		}
		set
		{
			XUi.inGameMenuOpen = value;
			World world = GameManager.Instance.World;
			if (world == null)
			{
				return;
			}
			EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
			if (primaryPlayer == null)
			{
				return;
			}
			LocalPlayerUI playerUI = primaryPlayer.PlayerUI;
			if (playerUI == null)
			{
				return;
			}
			NGUIWindowManager nguiWindowManager = playerUI.nguiWindowManager;
			if (nguiWindowManager == null)
			{
				return;
			}
			GameManager.Instance.SetToolTipPause(nguiWindowManager, value);
		}
	}

	public static bool IsGameRunning()
	{
		return GameManager.Instance != null && GameManager.Instance.World != null;
	}

	public float BackgroundGlobalOpacity
	{
		get
		{
			return this.backgroundGlobalOpacity;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.backgroundGlobalOpacity = value;
		}
	}

	public float ForegroundGlobalOpacity
	{
		get
		{
			return this.foregroundGlobalOpacity;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.foregroundGlobalOpacity = value;
		}
	}

	public XUiC_DragAndDropWindow DragAndDropWindow { get; set; }

	public XUiC_OnScreenIcons OnScreenIconsWindow { get; set; }

	public XUiC_ToolTip ToolTipWindow { get; set; }

	public XUiC_PopupMenu PopupMenuWindow { get; set; }

	public XUiC_SaveIndicator SaveIndicator { get; set; }

	public XUiC_WorkstationToolGrid CurrentWorkstationToolGrid { get; set; }

	public XUiC_WorkstationFuelGrid CurrentWorkstationFuelGrid { get; set; }

	public XUiC_WorkstationInputGrid CurrentWorkstationInputGrid { get; set; }

	public XUiC_WorkstationOutputGrid CurrentWorkstationOutputGrid { get; set; }

	public XUiC_DewCollectorModGrid CurrentCollectorModGrid { get; set; }

	public XUiC_CollectorFuelGrid CurrentCollectorFuelGrid { get; set; }

	public XUiC_CollectorFuelGrid currentCollectorCatalystGrid { get; set; }

	public XUiC_CombineGrid CurrentCombineGrid { get; set; }

	public XUiC_PowerSourceSlots CurrentPowerSourceSlots { get; set; }

	public XUiC_PowerRangedAmmoSlots CurrentPowerAmmoSlots { get; set; }

	public XUiC_SelectableEntry CurrentSelectedEntry { get; set; }

	public ProgressionValue SelectedSkill { get; set; }

	public ITileEntityLootable LootContainer { get; set; }

	public bool IsUsingItemActionEntryUse { get; set; }

	public bool IsUsingItemActionEntryPromptComplete { get; set; }

	public bool IsReady
	{
		get
		{
			return this.mIsReady;
		}
		set
		{
			this.mIsReady = value;
			if (this.mIsReady)
			{
				Action onBuilt = this.OnBuilt;
				if (onBuilt == null)
				{
					return;
				}
				onBuilt();
			}
		}
	}

	public event Action OnShutdown;

	public event Action OnBuilt;

	public LocalPlayerUI playerUI
	{
		get
		{
			if (this.mPlayerUI == null)
			{
				this.mPlayerUI = base.GetComponentInParent<LocalPlayerUI>();
			}
			return this.mPlayerUI;
		}
	}

	public static XUi Instantiate(LocalPlayerUI _playerUI)
	{
		if (GameManager.IsDedicatedServer)
		{
			return null;
		}
		Log.Out("[XUi] Instantiating XUi from default prefab.");
		Transform transform = Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/XUi")).transform;
		transform.name = "XUi";
		transform.parent = _playerUI.transform;
		XUi.uiRoot = Object.FindAnyObjectByType<UIRoot>();
		XUi component = transform.GetComponent<XUi>();
		component.SetScale(-1f);
		component.gameObject.SetActive(true);
		component.Init(_playerUI.isPrimaryUI);
		return component;
	}

	public static void Reload(LocalPlayerUI _playerUI)
	{
		if (_playerUI.xui != null)
		{
			_playerUI.xui.Shutdown(true);
		}
		XUi.SetXmlsForUi(_playerUI.isPrimaryUI);
		XUi xui = XUi.Instantiate(_playerUI);
		xui.Load(null, false);
		xui.SetDataConnections();
	}

	public static bool ReloadWindow(LocalPlayerUI _playerUI, string _windowGroupName)
	{
		if (_playerUI.xui == null)
		{
			Log.Error("Can not reload single window, XUi not instantiated");
			return false;
		}
		for (int i = 0; i < _playerUI.xui.WindowGroups.Count; i++)
		{
			XUiWindowGroup xuiWindowGroup = _playerUI.xui.WindowGroups[i];
			if (xuiWindowGroup.Id.EqualsCaseInsensitive(_windowGroupName))
			{
				_playerUI.windowManager.Remove(xuiWindowGroup);
				_playerUI.xui.WindowGroups.RemoveAt(i);
				xuiWindowGroup.Cleanup();
				break;
			}
		}
		XUi.SetXmlsForUi(_playerUI.isPrimaryUI);
		_playerUI.xui.Load(new List<string> { _windowGroupName }, false);
		_playerUI.xui.IsReady = true;
		return _playerUI.xui.WindowGroups.Find((XUiWindowGroup _group) => _group.Id.EqualsCaseInsensitive(_windowGroupName)) != null;
	}

	public static void SetXmlsForUi(bool _menuUi)
	{
		XUiFromXml.ClearData();
		ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Common/styles"));
		ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Common/templates"));
		if (!_menuUi)
		{
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_InGame/styles"));
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_InGame/templates"));
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_InGame/windows"));
			ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_InGame/xui"));
			return;
		}
		ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Menu/styles"));
		ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Menu/templates"));
		ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Menu/windows"));
		ThreadManager.RunCoroutineSync(XUi.PatchAndLoadXuiXml("XUi_Menu/xui"));
	}

	public static IEnumerator PatchAndLoadXuiXml(string _relPathXuiFile)
	{
		MicroStopwatch timer = null;
		bool coroutineHadException = false;
		XmlFile xmlFile = null;
		yield return XmlPatcher.LoadAndPatchConfig(_relPathXuiFile, delegate(XmlFile _file)
		{
			xmlFile = _file;
		});
		yield return XmlPatcher.ApplyConditionalXmlBlocks(_relPathXuiFile, xmlFile, timer, XmlPatcher.EEvaluator.Host, delegate
		{
			coroutineHadException = true;
		});
		if (coroutineHadException)
		{
			yield break;
		}
		yield return XmlPatcher.ApplyConditionalXmlBlocks(_relPathXuiFile, xmlFile, timer, XmlPatcher.EEvaluator.Client, delegate
		{
			coroutineHadException = true;
		});
		if (coroutineHadException)
		{
			yield break;
		}
		yield return XUiFromXml.Load(xmlFile);
		yield break;
	}

	public void Awake()
	{
		this.WindowGroups = new List<XUiWindowGroup>();
		this.windowsAndGroupsByControllerType = new Dictionary<Type, List<XUiController>>();
		this.initFonts();
	}

	public void Init(bool _menuUi)
	{
		GamePrefs.OnGamePrefChanged += this.OnGamePrefChanged;
		GameOptionsManager.ResolutionChanged += this.OnResolutionChanged;
		this.loadGroup = LoadManager.CreateGroup();
		this.windows = new List<XUiV_Window>();
		this.lastScreenSize = new Vector2i(this.playerUI.camera.pixelWidth, this.playerUI.camera.pixelHeight);
		if (!_menuUi)
		{
			base.gameObject.GetOrAddComponent<XUi_FallThrough>().SetXUi(this);
		}
		this.initStackPanels();
		this.initAtlases();
		if (Application.isPlaying)
		{
			this.initSounds();
		}
		this.initAnchors();
		this.updateAnchors();
		this.BackgroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
		this.ForegroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
		LocalPlayerManager.OnLocalPlayersChanged += this.handleLocalPlayersChanged;
		this.Vehicle = new XUiM_Vehicle();
		this.AssembleItem = new XUiM_AssembleItem();
		this.QuestTracker = new XUiM_Quest();
		this.Recipes = new XUiM_Recipes();
		this.Trader = new XUiM_Trader();
		this.Dialog = new XUiM_Dialog();
	}

	public void Load(List<string> _windowGroupSubset = null, bool _async = false)
	{
		if (_async)
		{
			this.asyncLoad = true;
			this.loadAsyncCoroutine = ThreadManager.StartCoroutine(this.loadAsync(_windowGroupSubset));
			return;
		}
		this.asyncLoad = false;
		if (!XUiFromXml.HasData())
		{
			Log.Error("Loading XUi synchronously failed: XMLs not set.");
			return;
		}
		ThreadManager.RunCoroutineSync(this.loadAsync(_windowGroupSubset));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator loadAsync(List<string> _windowGroupSubset = null)
	{
		yield return null;
		while (!XUiFromXml.HasData())
		{
			yield return null;
		}
		MicroStopwatch msw = new MicroStopwatch();
		Log.Out("[XUi] Loading XUi " + (this.asyncLoad ? "asynchronously" : "synchronously"));
		List<string> asyncWindowGroupList = ((_windowGroupSubset != null) ? new List<string>(_windowGroupSubset) : new List<string>());
		if (_windowGroupSubset == null)
		{
			XUiFromXml.GetWindowGroupNames(out asyncWindowGroupList);
		}
		if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
		{
			yield return null;
			msw.ResetAndRestart();
		}
		this.accumElapsedMilliseconds = 0L;
		MicroStopwatch ms = new MicroStopwatch(true);
		foreach (string text in asyncWindowGroupList)
		{
			ms.Reset();
			ms.Start();
			XUiFromXml.LoadXui(this, text);
			this.accumElapsedMilliseconds += ms.ElapsedMilliseconds;
			if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
			{
				Log.Out("[XUi] Parsing window group, {0}, completed in {1} ms.", new object[] { text, ms.ElapsedMilliseconds });
			}
			if (msw.ElapsedMilliseconds > 20L)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		List<string>.Enumerator enumerator = default(List<string>.Enumerator);
		XUiFromXml.LoadDone(_windowGroupSubset == null);
		Log.Out("[XUi] Parsing all window groups completed in {0} ms total.", new object[] { this.accumElapsedMilliseconds });
		if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
		{
			yield return null;
			msw.ResetAndRestart();
		}
		this.accumElapsedMilliseconds = 0L;
		int num;
		for (int i = 0; i < this.WindowGroups.Count; i = num + 1)
		{
			XUiWindowGroup xuiWindowGroup = this.WindowGroups[i];
			ms.Reset();
			ms.Start();
			try
			{
				xuiWindowGroup.Init();
			}
			catch (Exception ex)
			{
				Log.Error("[XUi] Failed initializing window group " + xuiWindowGroup.Id);
				Log.Exception(ex);
			}
			this.accumElapsedMilliseconds += ms.ElapsedMilliseconds;
			if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
			{
				Log.Out("[XUi] Initialize window group, {0}, completed in {1} ms.", new object[] { xuiWindowGroup.Id, ms.ElapsedMilliseconds });
			}
			if (msw.ElapsedMilliseconds > 20L)
			{
				yield return null;
				msw.ResetAndRestart();
			}
			num = i;
		}
		Log.Out("[XUi] Initialized all window groups completed in {0} ms total.", new object[] { this.accumElapsedMilliseconds });
		while (this.loadGroup.Pending)
		{
			yield return null;
		}
		this.postLoadInit();
		this.IsReady = _windowGroupSubset == null;
		this.loadAsyncCoroutine = null;
		XUiUpdater.Add(this);
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void postLoadInit()
	{
		this.windowsAndGroupsByControllerType.Clear();
		foreach (XUiWindowGroup xuiWindowGroup in this.WindowGroups)
		{
			if (xuiWindowGroup.Initialized)
			{
				this.addWindowOrGroupControllerTypeEntry(xuiWindowGroup.Controller);
				foreach (XUiController xuiController in xuiWindowGroup.Controller.Children)
				{
					this.addWindowOrGroupControllerTypeEntry(xuiController);
				}
			}
		}
		if (WorldStaticData.LoadAllXmlsCoComplete)
		{
			XUiFromXml.ClearLoadingData();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		GameOptionsManager.ResolutionChanged -= this.OnResolutionChanged;
		GamePrefs.OnGamePrefChanged -= this.OnGamePrefChanged;
		LocalPlayerManager.OnLocalPlayersChanged -= this.handleLocalPlayersChanged;
		this.Shutdown(false);
	}

	public void Shutdown(bool _destroyImmediate = false)
	{
		Action onShutdown = this.OnShutdown;
		if (onShutdown != null)
		{
			onShutdown();
		}
		this.Cleanup(_destroyImmediate);
	}

	public void Cleanup(bool _destroyImmediate = false)
	{
		this.CancelLoading();
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			XUiWindowGroup xuiWindowGroup = this.WindowGroups[i];
			this.playerUI.windowManager.Remove(xuiWindowGroup);
			xuiWindowGroup.Cleanup();
		}
		this.WindowGroups.Clear();
		XUiUpdater.Remove(this);
		if (_destroyImmediate)
		{
			Object.DestroyImmediate(base.gameObject);
		}
	}

	public void CancelLoading()
	{
		if (this.loadAsyncCoroutine != null)
		{
			ThreadManager.StopCoroutine(this.loadAsyncCoroutine);
			this.loadAsyncCoroutine = null;
		}
	}

	public void StopAllVideo()
	{
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			XUi.<StopAllVideo>g__StopVideo|145_0(this.WindowGroups[i].Controller);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addWindowOrGroupControllerTypeEntry(XUiController _controller)
	{
		Type type = _controller.GetType();
		List<XUiController> list;
		if (!this.windowsAndGroupsByControllerType.TryGetValue(type, out list))
		{
			list = new List<XUiController>();
			this.windowsAndGroupsByControllerType.Add(type, list);
		}
		list.Add(_controller);
	}

	public void AddWindow(XUiV_Window _window)
	{
		this.windows.Add(_window);
	}

	public void RemoveWindow(XUiV_Window _window)
	{
		this.windows.Remove(_window);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetDataConnections()
	{
		if (this.playerUI.entityPlayer == null)
		{
			return;
		}
		if (this.PlayerInventory == null)
		{
			this.PlayerInventory = new XUiM_PlayerInventory(this, this.playerUI.entityPlayer);
		}
		if (this.PlayerEquipment == null)
		{
			this.PlayerEquipment = new XUiM_PlayerEquipment(this, this.playerUI.entityPlayer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs _obj)
	{
		if (_obj <= EnumGamePrefs.OptionsHudSize)
		{
			if (_obj == EnumGamePrefs.OptionsScreenBoundsValue)
			{
				this.SetScale(-1f);
				this.updateAnchors();
				this.RecenterWindowGroup(null, true);
				return;
			}
			if (_obj != EnumGamePrefs.OptionsHudSize)
			{
				return;
			}
			this.SetScale(-1f);
			return;
		}
		else
		{
			if (_obj == EnumGamePrefs.OptionsBackgroundGlobalOpacity)
			{
				this.BackgroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsBackgroundGlobalOpacity);
				return;
			}
			if (_obj != EnumGamePrefs.OptionsForegroundGlobalOpacity)
			{
				return;
			}
			this.ForegroundGlobalOpacity = GamePrefs.GetFloat(EnumGamePrefs.OptionsForegroundGlobalOpacity);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnResolutionChanged(int _arg1, int _arg2)
	{
		ThreadManager.StartCoroutine(this.delayedScaleUpdate());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator delayedScaleUpdate()
	{
		yield return null;
		this.SetScale(-1f);
		yield break;
	}

	public void SetScale(float _scale = -1f)
	{
		if (_scale > 0f)
		{
			this.xuiGlobalScaling = _scale;
		}
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
		float activeUiScale = GameOptionsManager.GetActiveUiScale();
		base.transform.localScale = this.xuiGlobalScaling * @float * activeUiScale * Vector3.one;
		base.transform.localPosition = Vector3.zero;
		XUi.pixelRatioFactor = 0f;
	}

	public float GetScale()
	{
		return this.xuiGlobalScaling;
	}

	public void LoadData<T>(string _path, Action<T> _callback) where T : Object
	{
		LoadManager.LoadAsset<T>(_path, _callback, this.loadGroup, false, !this.asyncLoad, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleLocalPlayersChanged()
	{
		this.lastScreenSize = new Vector2i(this.playerUI.camera.pixelWidth, this.playerUI.camera.pixelHeight);
		this.updateAnchors();
		this.RecenterWindowGroup(null, true);
		for (int i = 0; i < this.windows.Count; i++)
		{
			XUiV_Window xuiV_Window = this.windows[i];
			if (xuiV_Window.IsCursorArea && xuiV_Window.IsOpen)
			{
				this.UpdateWindowSoftCursorBounds(xuiV_Window);
			}
		}
	}

	public void OnUpdateDeltaTime(float _updateDeltaTime)
	{
		if (this.playerUI.entityPlayer != null)
		{
			this.SetDataConnections();
		}
		if (this.WindowGroups == null)
		{
			return;
		}
		if (this.ToolTipWindow != null)
		{
			this.playerUI.windowManager.Open(this.ToolTipWindow.WindowGroup, false);
		}
		if (this.SaveIndicator != null)
		{
			this.playerUI.windowManager.Open(this.SaveIndicator.WindowGroup, false);
		}
		if (this.lastScreenSize.x != this.playerUI.camera.pixelWidth || this.lastScreenSize.y != this.playerUI.camera.pixelHeight)
		{
			this.lastScreenSize = new Vector2i(this.playerUI.camera.pixelWidth, this.playerUI.camera.pixelHeight);
			this.updateAnchors();
			this.RecenterWindowGroup(null, true);
		}
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			XUiWindowGroup xuiWindowGroup = this.WindowGroups[i];
			if (xuiWindowGroup.Initialized && (xuiWindowGroup.isShowing || xuiWindowGroup.Controller.AlwaysUpdate))
			{
				try
				{
					xuiWindowGroup.Controller.Update(_updateDeltaTime);
				}
				catch (Exception ex)
				{
					Log.Error("[XUi] Error while updating window group '" + xuiWindowGroup.Id + "':");
					Log.Exception(ex);
				}
			}
		}
	}

	public float GetPixelRatioFactor()
	{
		if (XUi.pixelRatioFactor == 0f || Screen.height != XUi.lastScreenHeight)
		{
			float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
			float activeUiScale = GameOptionsManager.GetActiveUiScale();
			XUi.pixelRatioFactor = XUi.uiRoot.pixelSizeAdjustment / this.xuiGlobalScaling / @float / activeUiScale;
			XUi.lastScreenHeight = Screen.height;
		}
		return XUi.pixelRatioFactor;
	}

	public Vector2i GetXUiScreenSize()
	{
		return new Vector2i(new Vector2((float)this.playerUI.camera.pixelWidth, (float)this.playerUI.camera.pixelHeight) * this.GetPixelRatioFactor());
	}

	public Vector2i GetMouseXUiPosition()
	{
		Vector3 vector = this.playerUI.CursorController.GetLocalScreenPosition();
		return this.TranslateScreenVectorToXuiVector(vector);
	}

	public Vector2i TranslateScreenVectorToXuiVector(Vector2 _screenSpaceVector)
	{
		_screenSpaceVector.x -= (float)this.playerUI.camera.pixelWidth / 2f;
		_screenSpaceVector.y -= (float)this.playerUI.camera.pixelHeight / 2f;
		return new Vector2i(_screenSpaceVector * this.GetPixelRatioFactor());
	}

	public Vector3 TranslateScreenVectorToXuiVector(Vector3 _screenSpaceVector)
	{
		_screenSpaceVector.x -= (float)this.playerUI.camera.pixelWidth / 2f;
		_screenSpaceVector.y -= (float)this.playerUI.camera.pixelHeight / 2f;
		return _screenSpaceVector * this.GetPixelRatioFactor();
	}

	public Bounds GetXUiWindowScreenBounds(Transform _xuiElement, bool _includeInactive = false)
	{
		Bounds xuiWindowWorldBounds = XUi.getXUiWindowWorldBounds(_xuiElement, _includeInactive);
		Bounds bounds;
		bounds..ctor(this.playerUI.camera.WorldToScreenPoint(xuiWindowWorldBounds.min), Vector3.zero);
		bounds.Encapsulate(this.playerUI.camera.WorldToScreenPoint(xuiWindowWorldBounds.max));
		return bounds;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Bounds getXUiWindowWorldBounds(Transform _xuiElement, bool _includeInactive = false)
	{
		Bounds bounds;
		bounds..ctor(Vector3.zero, Vector3.zero);
		bool flag = false;
		_xuiElement.GetComponentsInChildren<UIBasicSprite>(_includeInactive, XUi.getXUiWindowWorldBoundsList);
		List<UIBasicSprite> list = XUi.getXUiWindowWorldBoundsList;
		for (int i = 0; i < list.Count; i++)
		{
			UIBasicSprite uibasicSprite = list[i];
			Transform parent = uibasicSprite.transform.parent;
			if (!(parent != null) || !parent.name.Equals("MapSpriteEntity(Clone)"))
			{
				Vector3[] worldCorners = uibasicSprite.worldCorners;
				for (int j = 0; j < worldCorners.Length; j++)
				{
					if (!flag)
					{
						bounds..ctor(worldCorners[j], Vector3.zero);
						flag = true;
					}
					else
					{
						bounds.Encapsulate(worldCorners[j]);
					}
				}
			}
		}
		XUi.getXUiWindowWorldBoundsList.Clear();
		return bounds;
	}

	public Transform StackPanelTransform
	{
		get
		{
			return this.stackPanelRoot;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initStackPanels()
	{
		this.stackPanelRoot = base.transform.Find("StackPanels").transform;
		this.StackPanels.Add("Left", new XUi.StackPanel("Left", XUi.EStackPanelPos.Left, base.transform.Find("StackPanels/Left").transform));
		this.StackPanels.Add("Center", new XUi.StackPanel("Center", XUi.EStackPanelPos.Center, base.transform.Find("StackPanels/Center").transform));
		this.StackPanels.Add("Right", new XUi.StackPanel("Right", XUi.EStackPanelPos.Right, base.transform.Find("StackPanels/Right").transform));
	}

	public void SetStackPanelScale(float _scale)
	{
		this.defaultStackPanelScale = _scale;
		this.stackPanelRoot.localScale = Vector3.one * _scale;
		this.stackPanelRoot.localPosition = Vector3.zero;
	}

	public void RecenterWindowGroup(XUiWindowGroup _wg, bool _forceImmediate = false)
	{
		if (!_forceImmediate && GameStats.GetInt(EnumGameStats.GameState) != 2)
		{
			if (base.gameObject.activeInHierarchy)
			{
				base.StartCoroutine(this.recenterLater(_wg));
			}
			return;
		}
		if (_wg == null)
		{
			this.calculateWindowGroupLayouts();
			return;
		}
		this.calculateWindowGroupLayout(_wg);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator recenterLater(XUiWindowGroup _wg)
	{
		yield return null;
		if (_wg != null)
		{
			this.calculateWindowGroupLayout(_wg);
		}
		else
		{
			this.calculateWindowGroupLayouts();
		}
		yield return null;
		this.playerUI.CursorController.ResetNavigationTarget();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void calculateWindowGroupLayouts()
	{
		foreach (XUiWindowGroup xuiWindowGroup in this.WindowGroups)
		{
			if (xuiWindowGroup.isShowing)
			{
				this.calculateWindowGroupLayout(xuiWindowGroup);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void calculateWindowGroupLayout(XUiWindowGroup _wg)
	{
		if (_wg == null || _wg.Id == "backpack")
		{
			return;
		}
		XUiController controller = _wg.Controller;
		if (((controller != null) ? controller.Children : null) == null)
		{
			return;
		}
		if (!_wg.HasStackPanelWindows())
		{
			return;
		}
		bool flag = false;
		int num = 0;
		int num2 = 0;
		foreach (XUi.StackPanel stackPanel in this.StackPanels.list)
		{
			bool flag2 = this.layoutWindowsInPanel(_wg, stackPanel);
			if (flag2 && num > 0)
			{
				num += _wg.StackPanelPadding;
			}
			stackPanel.Transform.localPosition = new Vector3((float)num, 0f, 0f);
			num += stackPanel.Size.x;
			num2 = Math.Max(num2, stackPanel.Size.y);
			flag = flag || flag2;
		}
		if (flag)
		{
			this.stackPanelRoot.localPosition = new Vector3(-(this.defaultStackPanelScale * (float)num / 2f), (float)_wg.StackPanelYOffset, 0f);
			this.stackPanelRoot.localScale = Vector3.one * this.defaultStackPanelScale;
		}
		if (flag && (!_wg.LeftPanelVAlignTop || !_wg.RightPanelVAlignTop))
		{
			if (!_wg.LeftPanelVAlignTop)
			{
				this.StackPanels.list[0].Transform.localPosition = new Vector3((float)((int)this.StackPanels.list[0].Transform.position.x), (float)(-(float)(num2 - this.StackPanels.list[0].Size.y)), 0f);
			}
			if (!_wg.RightPanelVAlignTop)
			{
				this.StackPanels.list[2].Transform.localPosition = new Vector3((float)((int)this.StackPanels.list[2].Transform.position.x), (float)(-(float)(num2 - this.StackPanels.list[2].Size.y)), 0f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool layoutWindowsInPanel(XUiWindowGroup _windowGroup, XUi.StackPanel _panel)
	{
		XUi.<>c__DisplayClass179_0 CS$<>8__locals1;
		CS$<>8__locals1._panel = _panel;
		CS$<>8__locals1.windowCount = 0;
		CS$<>8__locals1.yPos = 0;
		CS$<>8__locals1.maxWidth = 0;
		XUi.<layoutWindowsInPanel>g__LayoutWindowGroupWindows|179_0(_windowGroup, ref CS$<>8__locals1);
		if (this.playerUI.windowManager.IsWindowOpen("backpack"))
		{
			XUi.<layoutWindowsInPanel>g__LayoutWindowGroupWindows|179_0(this.GetWindowGroupById("backpack"), ref CS$<>8__locals1);
		}
		CS$<>8__locals1._panel.Size.x = CS$<>8__locals1.maxWidth;
		CS$<>8__locals1._panel.Size.y = -CS$<>8__locals1.yPos;
		CS$<>8__locals1._panel.WindowCount = CS$<>8__locals1.windowCount;
		return CS$<>8__locals1.windowCount != 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initSounds()
	{
		this.LoadData<AudioClip>("@:Sounds/UI/ui_menu_cycle.wav", delegate(AudioClip _o)
		{
			this.uiScrollSound = _o;
		});
		this.LoadData<AudioClip>("@:Sounds/UI/ui_menu_click.wav", delegate(AudioClip _o)
		{
			this.uiClickSound = _o;
		});
		this.LoadData<AudioClip>("@:Sounds/UI/ui_menu_start.wav", delegate(AudioClip _o)
		{
			this.uiConfirmSound = _o;
		});
		this.LoadData<AudioClip>("@:Sounds/UI/ui_menu_back.wav", delegate(AudioClip _o)
		{
			this.uiBackSound = _o;
		});
		this.LoadData<AudioClip>("@:Sounds/UI/ui_hover.wav", delegate(AudioClip _o)
		{
			this.uiSliderSound = _o;
		});
	}

	public void PlayMenuSound(XUi.UISoundType _soundType)
	{
		switch (_soundType)
		{
		case XUi.UISoundType.ClickSound:
			this.PlayMenuClickSound();
			return;
		case XUi.UISoundType.ScrollSound:
			this.PlayMenuScrollSound();
			return;
		case XUi.UISoundType.ConfirmSound:
			this.PlayMenuConfirmSound();
			return;
		case XUi.UISoundType.BackSound:
			this.PlayMenuBackSound();
			return;
		case XUi.UISoundType.SliderSound:
			this.PlayMenuSliderSound();
			return;
		case XUi.UISoundType.None:
			return;
		default:
			return;
		}
	}

	public void PlayMenuScrollSound()
	{
		Manager.PlayXUiSound(this.uiScrollSound, this.uiScrollVolume);
	}

	public void PlayMenuClickSound()
	{
		Manager.PlayXUiSound(this.uiClickSound, this.uiClickVolume);
	}

	public void PlayMenuConfirmSound()
	{
		Manager.PlayXUiSound(this.uiConfirmSound, this.uiConfirmVolume);
	}

	public void PlayMenuBackSound()
	{
		Manager.PlayXUiSound(this.uiBackSound, this.uiBackVolume);
	}

	public void PlayMenuSliderSound()
	{
		Manager.PlayXUiSound(this.uiSliderSound, this.uiSliderVolume);
	}

	public XUiV_Window GetWindow(string _name)
	{
		for (int i = 0; i < this.windows.Count; i++)
		{
			if (this.windows[i].ID == _name)
			{
				return this.windows[i];
			}
		}
		return null;
	}

	public XUiController FindWindowGroupByName(string _name)
	{
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			if (this.WindowGroups[i].Id.EqualsCaseInsensitive(_name))
			{
				return this.WindowGroups[i].Controller;
			}
		}
		return null;
	}

	public XUiController GetChildById(string _id)
	{
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			XUiController childById = this.WindowGroups[i].Controller.GetChildById(_id);
			if (childById != null)
			{
				return childById;
			}
		}
		return null;
	}

	public List<XUiController> GetChildrenById(string _id)
	{
		List<XUiController> list = new List<XUiController>();
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			this.WindowGroups[i].Controller.GetChildrenById(_id, list);
		}
		return list;
	}

	public T GetChildByType<T>() where T : XUiController
	{
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			T childByType = this.WindowGroups[i].Controller.GetChildByType<T>();
			if (childByType != null)
			{
				return childByType;
			}
		}
		return default(T);
	}

	public List<T> GetChildrenByType<T>() where T : XUiController
	{
		List<T> list = new List<T>();
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			this.WindowGroups[i].Controller.GetChildrenByType<T>(list);
		}
		return list;
	}

	public T GetWindowByType<T>() where T : XUiController
	{
		Type typeFromHandle = typeof(T);
		List<XUiController> list;
		this.windowsAndGroupsByControllerType.TryGetValue(typeFromHandle, out list);
		if (list == null || list.Count == 0)
		{
			return default(T);
		}
		if (list.Count > 1)
		{
			Log.Warning("Multiple controllers of type " + typeof(T).FullName);
		}
		return (T)((object)list[0]);
	}

	public List<T> GetWindowsByType<T>() where T : XUiController
	{
		Type typeFromHandle = typeof(T);
		List<XUiController> list;
		this.windowsAndGroupsByControllerType.TryGetValue(typeFromHandle, out list);
		List<T> list2 = new List<T>();
		if (list == null)
		{
			return list2;
		}
		foreach (XUiController xuiController in list)
		{
			list2.Add((T)((object)xuiController));
		}
		return list2;
	}

	public XUiWindowGroup GetWindowGroupById(string _id)
	{
		foreach (XUiWindowGroup xuiWindowGroup in this.WindowGroups)
		{
			if (xuiWindowGroup.Id == _id)
			{
				return xuiWindowGroup;
			}
		}
		return null;
	}

	public void GetOpenWindows(List<XUiV_Window> _list)
	{
		_list.Clear();
		foreach (XUiV_Window xuiV_Window in this.windows)
		{
			if (xuiV_Window.IsOpen && xuiV_Window.IsVisible)
			{
				_list.Add(xuiV_Window);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initAtlases()
	{
		MultiSourceAtlasManager[] array = Resources.FindObjectsOfTypeAll<MultiSourceAtlasManager>();
		for (int i = 0; i < array.Length; i++)
		{
			this.allMultiSourceAtlases.Add(array[i].name, array[i]);
		}
	}

	public INGUIAtlas GetAtlasByName(string _atlasName, string _spriteName)
	{
		if (string.IsNullOrEmpty(_atlasName))
		{
			return null;
		}
		MultiSourceAtlasManager multiSourceAtlasManager;
		if (!string.IsNullOrEmpty(_spriteName) && this.allMultiSourceAtlases.TryGetValue(_atlasName, out multiSourceAtlasManager))
		{
			return multiSourceAtlasManager.GetAtlasForSprite(_spriteName);
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initFonts()
	{
		this.fontsByName = new CaseInsensitiveStringDictionary<NGUIFont>();
		foreach (NGUIFont nguifont in this.NGUIFonts)
		{
			this.fontsByName.Add(nguifont.name, nguifont);
		}
	}

	public NGUIFont GetUIFontByName(string _name, bool _showWarning = true)
	{
		NGUIFont nguifont;
		if (this.fontsByName.TryGetValue(_name, out nguifont))
		{
			return nguifont;
		}
		if (_showWarning)
		{
			Log.Warning("XUi font not found: " + _name + ", from: " + StackTraceUtility.ExtractStackTrace());
		}
		return null;
	}

	public Transform GetAnchor(string _anchorName)
	{
		if (string.IsNullOrEmpty(_anchorName))
		{
			return null;
		}
		UIAnchor.Side side;
		if (EnumUtils.TryParse<UIAnchor.Side>(_anchorName, out side, true))
		{
			return this.xuiAnchors[side].transform;
		}
		return this.anchorRoot.Find(_anchorName);
	}

	public UIAnchor GetAnchor(UIAnchor.Side _anchorSide)
	{
		return this.xuiAnchors[_anchorSide];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initAnchors()
	{
		this.anchorRoot = base.transform.Find("Anchors").transform;
		UIAnchor[] componentsInChildren = this.anchorRoot.parent.GetComponentsInChildren<UIAnchor>();
		this.xuiAnchors = new UIAnchor[9];
		foreach (UIAnchor uianchor in componentsInChildren)
		{
			uianchor.runOnlyOnce = false;
			uianchor.uiCamera = this.playerUI.camera;
			this.xuiAnchors[uianchor.side] = uianchor;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAnchors()
	{
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsScreenBoundsValue);
		float num = (1f - @float) / 2f;
		foreach (UIAnchor uianchor in this.xuiAnchors)
		{
			if (!(uianchor == null))
			{
				UIAnchor.Side side = uianchor.side;
				if (side == 5 || side == 4 || side == 6)
				{
					uianchor.relativeOffset.x = -num;
				}
				side = uianchor.side;
				if (side == 1 || side == 2 || side == null)
				{
					uianchor.relativeOffset.x = num;
				}
				side = uianchor.side;
				if (side == 3 || side == 4 || side == 2)
				{
					uianchor.relativeOffset.y = -num;
				}
				side = uianchor.side;
				if (side == 7 || side == 6 || side == null)
				{
					uianchor.relativeOffset.y = num;
				}
				side = uianchor.side;
				if (side == 7 || side == 3 || side == 8)
				{
					uianchor.relativeOffset.x = 0f;
				}
			}
		}
	}

	public void RefreshAllWindows()
	{
		if (this.WindowGroups == null)
		{
			return;
		}
		for (int i = 0; i < this.WindowGroups.Count; i++)
		{
			this.WindowGroups[i].Controller.SetAllChildrenDirty();
		}
	}

	public void CancelAllCrafting()
	{
		XUiC_RecipeStack[] recipesToCraft = this.FindWindowGroupByName("crafting").GetChildByType<XUiC_CraftingQueue>().GetRecipesToCraft();
		for (int i = 0; i < recipesToCraft.Length; i++)
		{
			recipesToCraft[i].ForceCancel();
		}
	}

	public CraftingData GetCraftingData()
	{
		CraftingData craftingData = new CraftingData();
		XUiController xuiController = this.FindWindowGroupByName("crafting");
		if (xuiController == null)
		{
			return craftingData;
		}
		XUiC_CraftingQueue childByType = xuiController.GetChildByType<XUiC_CraftingQueue>();
		if (childByType == null)
		{
			return craftingData;
		}
		XUiC_RecipeStack[] recipesToCraft = childByType.GetRecipesToCraft();
		if (recipesToCraft == null)
		{
			return craftingData;
		}
		craftingData.RecipeQueueItems = new RecipeQueueItem[recipesToCraft.Length];
		for (int i = 0; i < recipesToCraft.Length; i++)
		{
			RecipeQueueItem recipeQueueItem = new RecipeQueueItem();
			recipeQueueItem.Recipe = recipesToCraft[i].GetRecipe();
			recipeQueueItem.Multiplier = (short)recipesToCraft[i].GetRecipeCount();
			recipeQueueItem.CraftingTimeLeft = recipesToCraft[i].GetRecipeCraftingTimeLeft();
			recipeQueueItem.IsCrafting = recipesToCraft[i].IsCrafting;
			recipeQueueItem.Quality = (byte)recipesToCraft[i].OutputQuality;
			recipeQueueItem.StartingEntityId = recipesToCraft[i].StartingEntityId;
			recipeQueueItem.RepairItem = recipesToCraft[i].OriginalItem;
			recipeQueueItem.AmountToRepair = (ushort)recipesToCraft[i].AmountToRepair;
			recipeQueueItem.OneItemCraftTime = recipesToCraft[i].GetOneItemCraftTime();
			craftingData.RecipeQueueItems[i] = recipeQueueItem;
		}
		return craftingData;
	}

	public void SetCraftingData(CraftingData _cd)
	{
		base.StartCoroutine(this.setCraftingDataAsync(_cd));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setCraftingDataAsync(CraftingData _cd)
	{
		while (!this.IsReady)
		{
			yield return null;
		}
		XUiC_CraftingQueue childByType = this.GetChildByType<XUiC_CraftingQueue>();
		if (childByType == null)
		{
			yield break;
		}
		childByType.ClearQueue();
		for (int i = 0; i < _cd.RecipeQueueItems.Length; i++)
		{
			RecipeQueueItem recipeQueueItem = _cd.RecipeQueueItems[i];
			if (recipeQueueItem != null)
			{
				if (recipeQueueItem.RepairItem != null && recipeQueueItem.RepairItem.type != 0)
				{
					childByType.AddItemToRepairAtIndex(i, recipeQueueItem.CraftingTimeLeft, recipeQueueItem.RepairItem, (int)recipeQueueItem.AmountToRepair, recipeQueueItem.IsCrafting, recipeQueueItem.StartingEntityId);
				}
				else
				{
					childByType.AddRecipeToCraftAtIndex(i, recipeQueueItem.Recipe, (int)recipeQueueItem.Multiplier, recipeQueueItem.CraftingTimeLeft, recipeQueueItem.IsCrafting, false, (int)recipeQueueItem.Quality, recipeQueueItem.StartingEntityId, recipeQueueItem.OneItemCraftTime);
				}
			}
			else
			{
				childByType.AddRecipeToCraftAtIndex(i, null, 0, -1f, false, false, -1, -1, -1f);
			}
		}
		childByType.IsDirty = true;
		yield break;
	}

	public static void HandlePaging(XUi _xui, Func<bool> _onPageUp, Func<bool> _onPageDown, bool _useVerticalAxis = false)
	{
		if (null != _xui.playerUI)
		{
			PlayerActionsLocal playerInput = _xui.playerUI.playerInput;
			if (((playerInput != null) ? playerInput.GUIActions : null) != null && _xui.playerUI.windowManager.IsKeyShortcutsAllowed())
			{
				Vector2 vector = _xui.playerUI.playerInput.GUIActions.Camera.Vector;
				if (vector == Vector2.zero)
				{
					XUi.pagingRepeatTimer = 0f;
					XUi.previousPagingVector = Vector2.zero;
					return;
				}
				if (XUi.previousPagingVector != Vector2.zero)
				{
					XUi.pagingRepeatTimer -= Time.unscaledDeltaTime;
					if (XUi.pagingRepeatTimer > 0f)
					{
						return;
					}
				}
				else
				{
					XUi.initialPagingInput = true;
				}
				XUi.previousPagingVector = vector;
				XUi.pagingRepeatTimer = (XUi.initialPagingInput ? 0.35f : 0.1f);
				XUi.initialPagingInput = false;
				bool flag = false;
				float num = Mathf.Abs(vector.x);
				float num2 = Mathf.Abs(vector.y);
				if (_useVerticalAxis)
				{
					if ((double)num2 < (double)num * 1.8)
					{
						return;
					}
					if ((vector.y > 0f && _onPageUp()) || (vector.y < 0f && _onPageDown()))
					{
						flag = true;
					}
				}
				else
				{
					if ((double)num < (double)num2 * 1.8)
					{
						return;
					}
					if ((vector.x > 0f && _onPageUp()) || (vector.x < 0f && _onPageDown()))
					{
						flag = true;
					}
				}
				if (flag)
				{
					_xui.playerUI.CursorController.PlayPagingSound();
					return;
				}
				return;
			}
		}
		XUi.previousPagingVector = Vector2.zero;
	}

	public void UpdateWindowSoftCursorBounds(XUiV_Window _window)
	{
		this.playerUI.CursorController != null;
	}

	public void RemoveWindowFromSoftCursorBounds(XUiV_Window _window)
	{
		CursorControllerAbs cursorController = this.playerUI.CursorController;
		if (cursorController != null)
		{
			cursorController.RemoveBounds(_window.ID);
		}
	}

	public void ForceInputStyleChange()
	{
		this.GetOpenWindows(XUi.forceInputStyleChangedWindowsList);
		foreach (XUiV_Window xuiV_Window in XUi.forceInputStyleChangedWindowsList)
		{
			xuiV_Window.Controller.ForceInputStyleChange(PlatformManager.NativePlatform.Input.CurrentInputStyle, PlatformManager.NativePlatform.Input.CurrentInputStyle);
		}
		XUi.forceInputStyleChangedWindowsList.Clear();
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <StopAllVideo>g__StopVideo|145_0(XUiController controller)
	{
		XUiV_Video xuiV_Video = controller.ViewComponent as XUiV_Video;
		if (xuiV_Video != null)
		{
			xuiV_Video.Playing = false;
		}
		foreach (XUiController xuiController in controller.Children)
		{
			XUi.<StopAllVideo>g__StopVideo|145_0(xuiController);
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <layoutWindowsInPanel>g__LayoutWindowGroupWindows|179_0(XUiWindowGroup _wg, ref XUi.<>c__DisplayClass179_0 A_1)
	{
		foreach (XUiController xuiController in _wg.Controller.Children)
		{
			XUiV_Window xuiV_Window = (XUiV_Window)xuiController.ViewComponent;
			if (xuiV_Window != null && xuiV_Window.IsActiveInHierarchy && !(xuiController.ViewComponent.UiTransform.parent.name != A_1._panel.Name) && xuiV_Window.Size.y > 0)
			{
				if (A_1.yPos < 0)
				{
					A_1.yPos -= _wg.StackPanelPadding;
				}
				xuiV_Window.Position = new Vector2i(0, A_1.yPos);
				xuiV_Window.UiTransform.localPosition = new Vector3(0f, (float)A_1.yPos);
				A_1.yPos -= xuiV_Window.Size.y;
				A_1.maxWidth = Math.Max(A_1.maxWidth, xuiV_Window.Size.x);
				int windowCount = A_1.windowCount;
				A_1.windowCount = windowCount + 1;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int lastScreenHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float pixelRatioFactor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static UIRoot uiRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool inGameMenuOpen;

	public NGUIFont[] NGUIFonts;

	public List<XUiWindowGroup> WindowGroups;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<Type, List<XUiController>> windowsAndGroupsByControllerType;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<XUiV_Window> windows;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float backgroundGlobalOpacity = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float foregroundGlobalOpacity = 1f;

	public XUiM_PlayerInventory PlayerInventory;

	public XUiM_PlayerEquipment PlayerEquipment;

	public XUiM_Vehicle Vehicle;

	public XUiM_AssembleItem AssembleItem;

	public XUiM_Quest QuestTracker;

	public XUiM_Recipes Recipes;

	public XUiM_Trader Trader;

	public XUiM_Dialog Dialog;

	public XUiC_BuffPopoutList BuffPopoutList;

	public XUiC_CollectedItemList CollectedItemList;

	public XUiC_Radial RadialWindow;

	public XUiC_GamepadCalloutWindow CalloutWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float xuiGlobalScaling = 1.255f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2i lastScreenSize = Vector2i.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool mIsReady;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool asyncLoad;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LoadManager.LoadGroup loadGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LocalPlayerUI mPlayerUI;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public long accumElapsedMilliseconds;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Coroutine loadAsyncCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly List<UIBasicSprite> getXUiWindowWorldBoundsList = new List<UIBasicSprite>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float defaultStackPanelScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform stackPanelRoot;

	public readonly DictionaryList<string, XUi.StackPanel> StackPanels = new DictionaryList<string, XUi.StackPanel>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioClip uiScrollSound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float uiScrollVolume = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioClip uiClickSound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float uiClickVolume = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioClip uiConfirmSound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float uiConfirmVolume = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioClip uiBackSound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float uiBackVolume = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioClip uiSliderSound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float uiSliderVolume = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly Dictionary<string, MultiSourceAtlasManager> allMultiSourceAtlases = new CaseInsensitiveStringDictionary<MultiSourceAtlasManager>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CaseInsensitiveStringDictionary<NGUIFont> fontsByName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform anchorRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UIAnchor[] xuiAnchors;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 previousPagingVector = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float pagingRepeatTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool initialPagingInput;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly List<XUiV_Window> forceInputStyleChangedWindowsList = new List<XUiV_Window>();

	public enum EStackPanelPos
	{
		Left,
		Center,
		Right
	}

	public class StackPanel
	{
		public StackPanel(string _name, XUi.EStackPanelPos _position, Transform _transform)
		{
			this.Name = _name;
			this.Position = _position;
			this.Transform = _transform;
		}

		public readonly string Name;

		public readonly XUi.EStackPanelPos Position;

		public readonly Transform Transform;

		public int WindowCount;

		public Vector2Int Size;
	}

	public enum UISoundType
	{
		ClickSound,
		ScrollSound,
		ConfirmSound,
		BackSound,
		SliderSound,
		None
	}
}
