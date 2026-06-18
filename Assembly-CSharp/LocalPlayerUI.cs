using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Platform;
using UnityEngine;

public class LocalPlayerUI : MonoBehaviour
{
	public event Action<EntityPlayerLocal> OnEntityPlayerLocalAssigned;

	public event Action OnUIShutdown;

	public static void QueueUIForNewPlayerEntity(LocalPlayerUI _playerUI)
	{
		if (!LocalPlayerUI.playerUIQueueForPendingEntity.Contains(_playerUI))
		{
			LocalPlayerUI.playerUIQueueForPendingEntity.Enqueue(_playerUI);
		}
	}

	public static LocalPlayerUI DispatchNewPlayerForUI(EntityPlayerLocal _entityPlayer)
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_entityPlayer);
		uiforPlayer.entityPlayer = _entityPlayer;
		return uiforPlayer;
	}

	public static LocalPlayerUI GetUIForPrimaryPlayer()
	{
		for (int i = 0; i < LocalPlayerUI.playerUIs.Count; i++)
		{
			LocalPlayerUI localPlayerUI = LocalPlayerUI.playerUIs[i];
			if (!localPlayerUI.isPrimaryUI)
			{
				return localPlayerUI;
			}
		}
		return null;
	}

	public static LocalPlayerUI GetUIForPlayer(EntityPlayerLocal _entityPlayer)
	{
		if (_entityPlayer == null)
		{
			return null;
		}
		LocalPlayerUI localPlayerUI = null;
		for (int i = 0; i < LocalPlayerUI.playerUIs.Count; i++)
		{
			LocalPlayerUI localPlayerUI2 = LocalPlayerUI.playerUIs[i];
			if (localPlayerUI2.entityPlayer == _entityPlayer)
			{
				return localPlayerUI2;
			}
			if (!localPlayerUI2.isPrimaryUI && localPlayerUI2.entityPlayer == null)
			{
				localPlayerUI = localPlayerUI2;
			}
		}
		while (LocalPlayerUI.playerUIQueueForPendingEntity.Count > 0)
		{
			LocalPlayerUI localPlayerUI3 = LocalPlayerUI.playerUIQueueForPendingEntity.Dequeue();
			if (!(localPlayerUI3 == null))
			{
				localPlayerUI3.mEntityPlayer = _entityPlayer;
				return localPlayerUI3;
			}
		}
		return localPlayerUI;
	}

	public static LocalPlayerUI CreateUIForNewLocalPlayer()
	{
		if (!LocalPlayerUI.primaryUI)
		{
			throw new Exception("Can't create UI for new local player, primary UI not set.");
		}
		GameObject gameObject = LocalPlayerUI.mCleanCopy.gameObject;
		Transform transform = LocalPlayerUI.primaryUI.transform;
		gameObject.SetActive(false);
		GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, LocalPlayerUI.primaryUI.transform.parent, true);
		gameObject2.transform.position = Vector3.back * (float)LocalPlayerUI.localPlayerCount * 4f;
		gameObject2.transform.localRotation = transform.localRotation;
		gameObject2.transform.localScale = transform.localScale;
		gameObject2.gameObject.name = "GUI(Player" + LocalPlayerUI.playerUIs.Count.ToString() + ")";
		MainMenuMono component = gameObject2.GetComponent<MainMenuMono>();
		if (component != null)
		{
			Object.Destroy(component);
		}
		gameObject2.SetActive(true);
		LocalPlayerUI component2 = gameObject2.GetComponent<LocalPlayerUI>();
		component2.nguiWindowManager.Show(EnumNGUIWindow.Version, false);
		GameManager.Instance.AddWindows(component2.windowManager);
		List<string> list = null;
		bool flag = true;
		if (LocalPlayerUI.localPlayerCount > 1)
		{
			list = new List<string>(new string[] { "secondaryPlayerJoin", "CalloutGroup", "popupGroup" });
			flag = false;
		}
		XUi.Instantiate(component2).Load(list, flag);
		return component2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void PrepareUIForSplitScreen(Transform _uiTransform)
	{
		UIRect.AnchorPoint[] array = new UIRect.AnchorPoint[4];
		UICamera componentInParent = _uiTransform.GetComponentInParent<UICamera>();
		foreach (UIRect uirect in _uiTransform.GetComponentsInChildren<UIRect>(true))
		{
			if (uirect.isAnchored)
			{
				array[0] = uirect.topAnchor;
				array[1] = uirect.leftAnchor;
				array[2] = uirect.rightAnchor;
				array[3] = uirect.bottomAnchor;
				foreach (UIRect.AnchorPoint anchorPoint in array)
				{
					if (anchorPoint != null && anchorPoint.target != null)
					{
						UIPanel component = anchorPoint.target.gameObject.GetComponent<UIPanel>();
						if (component != null && component.clipping == null)
						{
							anchorPoint.target = componentInParent.transform;
						}
					}
				}
				uirect.UpdateAnchors();
			}
		}
	}

	public static bool AnyModalWindowOpen()
	{
		for (int i = 0; i < LocalPlayerUI.playerUIs.Count; i++)
		{
			if (LocalPlayerUI.playerUIs[i].windowManager.IsModalWindowOpen())
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateChildCameraIndices()
	{
		int num = 0;
		foreach (LocalPlayerCamera localPlayerCamera in base.GetComponentsInChildren<LocalPlayerCamera>())
		{
			localPlayerCamera.uiChildIndex = num++;
			localPlayerCamera.SetCameraDepth();
		}
	}

	public static LocalPlayerUI primaryUI
	{
		get
		{
			return LocalPlayerUI.mPrimaryUI;
		}
	}

	public int playerIndex
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static int localPlayerCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return LocalPlayerUI.playerUIs.Count - 1;
		}
	}

	public PlayerActionsLocal playerInput
	{
		get
		{
			IPlatform nativePlatform = PlatformManager.NativePlatform;
			if (nativePlatform == null)
			{
				return null;
			}
			PlayerInputManager input = nativePlatform.Input;
			if (input == null)
			{
				return null;
			}
			return input.PrimaryPlayer;
		}
	}

	public ActionSetManager ActionSetManager
	{
		get
		{
			IPlatform nativePlatform = PlatformManager.NativePlatform;
			if (nativePlatform == null)
			{
				return null;
			}
			PlayerInputManager input = nativePlatform.Input;
			if (input == null)
			{
				return null;
			}
			return input.ActionSetManager;
		}
	}

	public EntityPlayerLocal entityPlayer
	{
		get
		{
			return this.mEntityPlayer;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.mEntityPlayer = value;
			Action<EntityPlayerLocal> onEntityPlayerLocalAssigned = this.OnEntityPlayerLocalAssigned;
			if (onEntityPlayerLocalAssigned == null)
			{
				return;
			}
			onEntityPlayerLocalAssigned(this.mEntityPlayer);
		}
	}

	public LocalPlayer localPlayer
	{
		get
		{
			if (this.entityPlayer != null)
			{
				return this.entityPlayer.GetComponent<LocalPlayer>();
			}
			return null;
		}
	}

	public bool isPrimaryUI
	{
		get
		{
			return this.mIsPrimaryUI;
		}
	}

	public bool IsCleanCopy
	{
		get
		{
			return this.mIsCleanCopy;
		}
		set
		{
			this.mIsCleanCopy = value;
		}
	}

	public GUIWindowManager windowManager
	{
		get
		{
			if (!(this.mWindowManager != null))
			{
				return this.mWindowManager = base.GetComponent<GUIWindowManager>();
			}
			return this.mWindowManager;
		}
	}

	public NGUIWindowManager nguiWindowManager
	{
		get
		{
			if (!(this.mNGUIWindowManager != null))
			{
				return this.mNGUIWindowManager = base.GetComponent<NGUIWindowManager>();
			}
			return this.mNGUIWindowManager;
		}
	}

	public UICamera uiCamera
	{
		get
		{
			if (!(this.mUICamera != null))
			{
				return this.mUICamera = base.GetComponentInParent<UICamera>();
			}
			return this.mUICamera;
		}
	}

	public Camera camera
	{
		get
		{
			if (!(this.mCamera != null))
			{
				return this.mCamera = base.GetComponentInParent<Camera>();
			}
			return this.mCamera;
		}
	}

	public CursorControllerAbs CursorController
	{
		get
		{
			if (this.mCursorController != null)
			{
				return this.mCursorController;
			}
			UICamera uiCamera = this.uiCamera;
			if (uiCamera == null)
			{
				return null;
			}
			return this.mCursorController = uiCamera.GetComponentInChildren<CursorControllerAbs>();
		}
	}

	public XUi xui
	{
		get
		{
			if (this.mXUi == null)
			{
				this.mXUi = base.GetComponentInChildren<XUi>();
			}
			return this.mXUi;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		LocalPlayerUI.playerUIs.Add(this);
		this.mIsPrimaryUI = LocalPlayerUI.playerUIs.Count == 1;
		if (this.mIsPrimaryUI)
		{
			LocalPlayerUI.mPrimaryUI = this;
			base.gameObject.name = "GUI(Menu)";
			LocalPlayerUI.PrepareUIForSplitScreen(this.uiCamera.transform);
			LocalPlayerUI.CreatingCleanCopy = true;
			LocalPlayerUI.mCleanCopy = Object.Instantiate<LocalPlayerUI>(this, LocalPlayerUI.primaryUI.transform.parent, true);
			LocalPlayerUI.mCleanCopy.name = "GUI(CleanCopy)";
			LocalPlayerUI.mCleanCopy.gameObject.SetActive(false);
			LocalPlayerUI.mCleanCopy.IsCleanCopy = true;
			GameManager.Instance.AddWindows(LocalPlayerUI.mCleanCopy.windowManager);
			LocalPlayerUI.mCleanCopy.nguiWindowManager.ShowAll(false);
			MainMenuMono component = LocalPlayerUI.mCleanCopy.GetComponent<MainMenuMono>();
			if (component != null)
			{
				Object.Destroy(component);
			}
			LocalPlayerUI.playerUIs.Remove(LocalPlayerUI.mCleanCopy);
			LocalPlayerUI.CreatingCleanCopy = false;
			return;
		}
		Camera[] componentsInParent = base.GetComponentsInParent<Camera>();
		for (int i = 0; i < componentsInParent.Length; i++)
		{
			LocalPlayerCamera.AddToCamera(componentsInParent[i], LocalPlayerCamera.CameraType.UI);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.DispatchLocalPlayersChanged();
		if (!this.mIsPrimaryUI || GameManager.IsDedicatedServer)
		{
			return;
		}
		XUi.SetXmlsForUi(true);
		XUi.Instantiate(this).Load(null, true);
		this.SetupMenuSoftCursor();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupMenuSoftCursor()
	{
		CursorControllerAbs.AddSoftCursor(this.uiCamera, PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions, this.windowManager);
		this.CursorController.RefreshBounds();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		LocalPlayerManager.OnLocalPlayersChanged += this.HandleLocalPlayersChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		LocalPlayerManager.OnLocalPlayersChanged -= this.HandleLocalPlayersChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DispatchLocalPlayersChanged()
	{
		int num = 0;
		for (int i = 0; i < LocalPlayerUI.playerUIs.Count; i++)
		{
			LocalPlayerUI localPlayerUI = LocalPlayerUI.playerUIs[i];
			if (!localPlayerUI.isPrimaryUI)
			{
				localPlayerUI.playerIndex = num++;
			}
		}
		LocalPlayerManager.LocalPlayersChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLocalPlayersChanged()
	{
		if (this.isPrimaryUI)
		{
			AudioListener componentInParent = base.GetComponentInParent<AudioListener>();
			if (componentInParent != null)
			{
				componentInParent.enabled = !(GameManager.Instance != null) || GameManager.Instance.World == null || !(GameManager.Instance.World.GetPrimaryPlayer() != null);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (this.isPrimaryUI && GameManager.Instance.triggerEffectManager != null)
		{
			bool flag = this.windowManager.IsModalWindowOpen();
			if (!flag)
			{
				using (IEnumerator<LocalPlayerUI> enumerator = LocalPlayerUI.PlayerUIs.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.windowManager.IsModalWindowOpen())
						{
							flag = true;
							break;
						}
					}
				}
			}
			GameManager.Instance.triggerEffectManager.inUI = flag;
		}
		if (this.isPrimaryUI && !this.windowManager.IsModalWindowOpen() && this.xui.CalloutWindow != null)
		{
			this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		}
		if (GUIWindowConsole.IsOpen())
		{
			return;
		}
		if (!this.isPrimaryUI && LocalPlayerUI.primaryUI.windowManager.IsModalWindowOpen())
		{
			return;
		}
		PlayerActionsLocal playerInput = this.playerInput;
		if (playerInput == null)
		{
			return;
		}
		if (playerInput.GUIActions.Inspect.IsPressed)
		{
			return;
		}
		if (this.xui.playerUI.CursorController.Locked)
		{
			return;
		}
		if (this.xui.playerUI.CursorController.lockNavigationToView != null && this.xui.playerUI.CursorController.lockNavigationToView.xui.playerUI != this)
		{
			return;
		}
		if (this.playerInput.GUIActions.BackButton.WasPressed && this.TryItemStackGridNavigation())
		{
			return;
		}
		Vector2 vector = playerInput.GUIActions.Nav.Vector;
		if (vector == Vector2.zero)
		{
			this.inputRepeatTimer = 0f;
			this.previousInputVector = Vector2.zero;
			return;
		}
		if (this.previousInputVector != Vector2.zero)
		{
			this.inputRepeatTimer -= Time.unscaledDeltaTime;
			if (this.inputRepeatTimer > 0f)
			{
				return;
			}
		}
		else
		{
			this.initialRepeat = true;
		}
		XUiView navigationTarget = this.CursorController.navigationTarget;
		this.previousInputVector = vector;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		if (Mathf.Abs(vector.y) > Mathf.Abs(vector.x))
		{
			flag2 = vector.y > 0f;
			flag3 = vector.y < 0f;
		}
		else
		{
			flag5 = vector.x > 0f;
			flag4 = vector.x < 0f;
		}
		LocalPlayerUI.openWindows.Clear();
		LocalPlayerUI.navViews.Clear();
		if (this.CursorController.lockNavigationToView != null)
		{
			this.CursorController.lockNavigationToView.Controller.FindNavigatableChildren(LocalPlayerUI.navViews);
		}
		else
		{
			this.xui.GetOpenWindows(LocalPlayerUI.openWindows);
			foreach (XUiV_Window xuiV_Window in LocalPlayerUI.openWindows)
			{
				xuiV_Window.Controller.FindNavigatableChildren(LocalPlayerUI.navViews);
			}
		}
		if (LocalPlayerUI.navViews.Count == 0)
		{
			return;
		}
		float num = float.MaxValue;
		XUiView xuiView = null;
		Vector2 flatPosition = ((SoftCursor)this.CursorController).GetFlatPosition();
		if (navigationTarget != null && !navigationTarget.IsNavigatable)
		{
			this.CursorController.SetNavigationTarget(null);
		}
		bool flag6 = false;
		if (navigationTarget == null)
		{
			foreach (XUiView xuiView2 in LocalPlayerUI.navViews)
			{
				float sqrMagnitude = (flatPosition - xuiView2.GetClosestPoint(flatPosition)).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					xuiView = xuiView2;
				}
			}
			if (num < 3.4028235E+38f)
			{
				this.CursorController.SetNavigationTarget(xuiView);
				flag6 = true;
			}
		}
		if (!flag6)
		{
			if (flag2 && navigationTarget.NavUpTarget != null)
			{
				navigationTarget.NavUpTarget.Controller.SelectCursorElement(false, false);
				flag6 = true;
			}
			else if (flag3 && navigationTarget.NavDownTarget != null)
			{
				navigationTarget.NavDownTarget.Controller.SelectCursorElement(false, false);
				flag6 = true;
			}
			else if (flag4 && navigationTarget.NavLeftTarget != null)
			{
				navigationTarget.NavLeftTarget.Controller.SelectCursorElement(false, false);
				flag6 = true;
			}
			else if (flag5 && navigationTarget.NavRightTarget != null)
			{
				navigationTarget.NavRightTarget.Controller.SelectCursorElement(false, false);
				flag6 = true;
			}
		}
		LocalPlayerUI.navigationCandidates.Clear();
		LocalPlayerUI.navigationPrimeCandidates.Clear();
		LocalPlayerUI.navigationWrapAroundCandidates.Clear();
		LocalPlayerUI.navigationWrapAroundPrimeCandidates.Clear();
		if (!flag6)
		{
			Vector2 vector2;
			if (flag2)
			{
				vector2..ctor(navigationTarget.ColliderCenter.x, (float)(-(float)Screen.height));
			}
			else if (flag3)
			{
				vector2..ctor(navigationTarget.ColliderCenter.x, (float)Screen.height);
			}
			else if (flag4)
			{
				vector2..ctor((float)Screen.width, navigationTarget.ColliderCenter.y);
			}
			else
			{
				vector2..ctor((float)(-(float)Screen.width), navigationTarget.ColliderCenter.y);
			}
			foreach (XUiView xuiView3 in LocalPlayerUI.navViews)
			{
				if (flag2)
				{
					if (xuiView3.ColliderCenter.y > navigationTarget.ColliderCenter.y)
					{
						if (Vector3.Angle(navigationTarget.UiTransform.up, (xuiView3.ColliderCenter - navigationTarget.ColliderCenter).normalized) <= 78f)
						{
							if (Mathf.Abs(navigationTarget.ColliderCenter.x - xuiView3.ColliderCenter.x) <= Mathf.Max(navigationTarget.ColliderWidthExtent, xuiView3.ColliderWidthExtent))
							{
								LocalPlayerUI.navigationPrimeCandidates.Add(xuiView3);
							}
							else
							{
								LocalPlayerUI.navigationCandidates.Add(xuiView3);
							}
						}
					}
					else if (Mathf.Abs(navigationTarget.ColliderCenter.x - xuiView3.ColliderCenter.x) <= Mathf.Max(navigationTarget.ColliderWidthExtent, xuiView3.ColliderWidthExtent))
					{
						LocalPlayerUI.navigationWrapAroundPrimeCandidates.Add(xuiView3);
					}
					else
					{
						LocalPlayerUI.navigationWrapAroundCandidates.Add(xuiView3);
					}
				}
				else if (flag3)
				{
					if (xuiView3.ColliderCenter.y < navigationTarget.ColliderCenter.y)
					{
						if (Vector3.Angle(-navigationTarget.UiTransform.up, (xuiView3.ColliderCenter - navigationTarget.ColliderCenter).normalized) <= 78f)
						{
							if (Mathf.Abs(navigationTarget.ColliderCenter.x - xuiView3.ColliderCenter.x) <= Mathf.Max(navigationTarget.ColliderWidthExtent, xuiView3.ColliderWidthExtent))
							{
								LocalPlayerUI.navigationPrimeCandidates.Add(xuiView3);
							}
							else
							{
								LocalPlayerUI.navigationCandidates.Add(xuiView3);
							}
						}
					}
					else if (Mathf.Abs(navigationTarget.ColliderCenter.x - xuiView3.ColliderCenter.x) <= Mathf.Max(navigationTarget.ColliderWidthExtent, xuiView3.ColliderWidthExtent))
					{
						LocalPlayerUI.navigationWrapAroundPrimeCandidates.Add(xuiView3);
					}
					else
					{
						LocalPlayerUI.navigationWrapAroundCandidates.Add(xuiView3);
					}
				}
				else if (flag4)
				{
					if (xuiView3.ColliderCenter.x < navigationTarget.ColliderCenter.x)
					{
						if (Vector3.Angle(-navigationTarget.UiTransform.right, (xuiView3.ColliderCenter - navigationTarget.ColliderCenter).normalized) <= 78f)
						{
							if (Mathf.Abs(navigationTarget.ColliderCenter.y - xuiView3.ColliderCenter.y) <= Mathf.Max(navigationTarget.ColliderHeightExtent, xuiView3.ColliderHeightExtent))
							{
								LocalPlayerUI.navigationPrimeCandidates.Add(xuiView3);
							}
							else
							{
								LocalPlayerUI.navigationCandidates.Add(xuiView3);
							}
						}
					}
					else if (Mathf.Abs(navigationTarget.ColliderCenter.y - xuiView3.ColliderCenter.y) <= Mathf.Max(navigationTarget.ColliderHeightExtent, xuiView3.ColliderHeightExtent))
					{
						LocalPlayerUI.navigationWrapAroundPrimeCandidates.Add(xuiView3);
					}
					else
					{
						LocalPlayerUI.navigationWrapAroundCandidates.Add(xuiView3);
					}
				}
				else if (flag5)
				{
					if (xuiView3.ColliderCenter.x > navigationTarget.ColliderCenter.x)
					{
						if (Vector3.Angle(navigationTarget.UiTransform.right, (xuiView3.ColliderCenter - navigationTarget.ColliderCenter).normalized) <= 78f)
						{
							if (Mathf.Abs(navigationTarget.ColliderCenter.y - xuiView3.ColliderCenter.y) <= Mathf.Max(navigationTarget.ColliderHeightExtent, xuiView3.ColliderHeightExtent))
							{
								LocalPlayerUI.navigationPrimeCandidates.Add(xuiView3);
							}
							else
							{
								LocalPlayerUI.navigationCandidates.Add(xuiView3);
							}
						}
					}
					else if (Mathf.Abs(navigationTarget.ColliderCenter.y - xuiView3.ColliderCenter.y) <= Mathf.Max(navigationTarget.ColliderHeightExtent, xuiView3.ColliderHeightExtent))
					{
						LocalPlayerUI.navigationWrapAroundPrimeCandidates.Add(xuiView3);
					}
					else
					{
						LocalPlayerUI.navigationWrapAroundCandidates.Add(xuiView3);
					}
				}
			}
			if (!flag6 && LocalPlayerUI.navigationPrimeCandidates.Count > 0)
			{
				using (List<XUiView>.Enumerator enumerator3 = LocalPlayerUI.navigationPrimeCandidates.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						XUiView xuiView4 = enumerator3.Current;
						float num2;
						if (flag4 || flag5)
						{
							num2 = Mathf.Abs(navigationTarget.ColliderCenter.x - xuiView4.ColliderCenter.x);
						}
						else
						{
							num2 = Mathf.Abs(navigationTarget.ColliderCenter.y - xuiView4.ColliderCenter.y);
						}
						if (num2 < num)
						{
							num = num2;
							xuiView = xuiView4;
						}
					}
					goto IL_0B1B;
				}
			}
			if (!flag6 && LocalPlayerUI.navigationCandidates.Count > 0)
			{
				using (List<XUiView>.Enumerator enumerator3 = LocalPlayerUI.navigationCandidates.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						XUiView xuiView5 = enumerator3.Current;
						float sqrMagnitude2 = (navigationTarget.GetClosestPoint(xuiView5.ColliderCenter) - xuiView5.GetClosestPoint(navigationTarget.ColliderCenter)).sqrMagnitude;
						if (sqrMagnitude2 < num)
						{
							num = sqrMagnitude2;
							xuiView = xuiView5;
						}
					}
					goto IL_0B1B;
				}
			}
			if (!flag6 && LocalPlayerUI.navigationWrapAroundPrimeCandidates.Count > 0)
			{
				num = float.MaxValue;
				using (List<XUiView>.Enumerator enumerator3 = LocalPlayerUI.navigationWrapAroundPrimeCandidates.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						XUiView xuiView6 = enumerator3.Current;
						float num3;
						if (flag4 || flag5)
						{
							num3 = Mathf.Abs(vector2.x - xuiView6.ColliderCenter.x);
						}
						else
						{
							num3 = Mathf.Abs(vector2.y - xuiView6.ColliderCenter.y);
						}
						if (num3 < num)
						{
							num = num3;
							xuiView = xuiView6;
						}
					}
					goto IL_0B1B;
				}
			}
			if (!flag6 && LocalPlayerUI.navigationWrapAroundCandidates.Count > 0)
			{
				num = float.MaxValue;
				foreach (XUiView xuiView7 in LocalPlayerUI.navigationWrapAroundCandidates)
				{
					float sqrMagnitude3 = (vector2 - xuiView7.GetClosestPoint(vector2)).sqrMagnitude;
					if (sqrMagnitude3 < num)
					{
						num = sqrMagnitude3;
						xuiView = xuiView7;
					}
				}
			}
		}
		IL_0B1B:
		if (!flag6 && num < 3.4028235E+38f)
		{
			this.CursorController.SetNavigationTarget(xuiView);
		}
		this.inputRepeatTimer = (this.initialRepeat ? 0.35f : 0.1f);
		this.initialRepeat = false;
	}

	public void RefreshNavigationTarget()
	{
		LocalPlayerUI.openWindows.Clear();
		LocalPlayerUI.navViews.Clear();
		if (this.CursorController.lockNavigationToView != null)
		{
			this.CursorController.lockNavigationToView.Controller.FindNavigatableChildren(LocalPlayerUI.navViews);
		}
		else
		{
			this.xui.GetOpenWindows(LocalPlayerUI.openWindows);
			foreach (XUiV_Window xuiV_Window in LocalPlayerUI.openWindows)
			{
				xuiV_Window.Controller.FindNavigatableChildren(LocalPlayerUI.navViews);
			}
		}
		if (LocalPlayerUI.navViews.Count == 0)
		{
			return;
		}
		XUiView navigationTarget = this.CursorController.navigationTarget;
		float num = float.MaxValue;
		XUiView xuiView = null;
		Vector2 flatPosition = ((SoftCursor)this.CursorController).GetFlatPosition();
		foreach (XUiView xuiView2 in LocalPlayerUI.navViews)
		{
			float sqrMagnitude = (flatPosition - xuiView2.GetClosestPoint(flatPosition)).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				xuiView = xuiView2;
			}
		}
		if (num < 3.4028235E+38f)
		{
			this.CursorController.SetNavigationTarget(xuiView);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		Action onUIShutdown = this.OnUIShutdown;
		if (onUIShutdown != null)
		{
			onUIShutdown();
		}
		LocalPlayerUI.playerUIs.Remove(this);
		LocalPlayerUI.navViews.Clear();
		LocalPlayerUI.openWindows.Clear();
		this.DispatchLocalPlayersChanged();
	}

	public void RegisterItemStackGrid(XUiC_ItemStackGrid _grid)
	{
		if (this.activeItemStackGrids.Contains(_grid))
		{
			return;
		}
		this.activeItemStackGrids.Add(_grid);
		this.SortItemStackGrids();
	}

	public void UnregisterItemStackGrid(XUiC_ItemStackGrid _grid)
	{
		this.activeItemStackGrids.Remove(_grid);
		this.SortItemStackGrids();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortItemStackGrids()
	{
		this.activeItemStackGrids.Sort(delegate(XUiC_ItemStackGrid x, XUiC_ItemStackGrid y)
		{
			if (x.ViewComponent.ColliderCenter.y + x.ViewComponent.ColliderHeightExtent <= y.ViewComponent.ColliderCenter.y + y.ViewComponent.ColliderHeightExtent)
			{
				return -1;
			}
			return 1;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryItemStackGridNavigation()
	{
		if (this.activeItemStackGrids.Count == 0)
		{
			return false;
		}
		XUiC_ItemStackGrid xuiC_ItemStackGrid = null;
		if (this.CursorController.navigationTarget != null)
		{
			foreach (XUiC_ItemStackGrid xuiC_ItemStackGrid2 in this.activeItemStackGrids)
			{
				if (this.CursorController.navigationTarget.Controller.IsChildOf(xuiC_ItemStackGrid2))
				{
					xuiC_ItemStackGrid = xuiC_ItemStackGrid2;
					break;
				}
			}
		}
		int num = 0;
		if (xuiC_ItemStackGrid != null)
		{
			num = this.activeItemStackGrids.IndexOf(xuiC_ItemStackGrid);
		}
		int num2 = num;
		if (this.activeItemStackGrids.Count == 1)
		{
			this.activeItemStackGrids[num].SelectCursorElement(true, false);
			return true;
		}
		num++;
		if (num >= this.activeItemStackGrids.Count)
		{
			num = 0;
		}
		while (num != num2)
		{
			XUiView xuiView;
			if (this.activeItemStackGrids[num].TryFindFirstNavigableView(out xuiView))
			{
				if (!this.xui.DragAndDropWindow.CurrentStack.Equals(ItemStack.Empty))
				{
					int num3 = this.activeItemStackGrids[num].FindFirstEmptySlot();
					if (num3 >= 0)
					{
						this.activeItemStackGrids[num].GetItemStackControllers()[num3].SelectCursorElement(true, true);
						return true;
					}
				}
				this.activeItemStackGrids[num].SelectCursorElement(true, true);
				return true;
			}
			num++;
			if (num >= this.activeItemStackGrids.Count)
			{
				num = 0;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static LocalPlayerUI mPrimaryUI;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static LocalPlayerUI mCleanCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityPlayerLocal mEntityPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool mIsPrimaryUI;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool mIsCleanCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindowManager mWindowManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NGUIWindowManager mNGUIWindowManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UICamera mUICamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera mCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CursorControllerAbs mCursorController;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public XUi mXUi;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly List<LocalPlayerUI> playerUIs = new List<LocalPlayerUI>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly Queue<LocalPlayerUI> playerUIQueueForPendingEntity = new Queue<LocalPlayerUI>();

	public static readonly ReadOnlyCollection<LocalPlayerUI> PlayerUIs = new ReadOnlyCollection<LocalPlayerUI>(LocalPlayerUI.playerUIs);

	public static bool CreatingCleanCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<XUiView> navigationCandidates = new List<XUiView>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<XUiView> navigationPrimeCandidates = new List<XUiView>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<XUiView> navigationWrapAroundCandidates = new List<XUiView>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<XUiView> navigationWrapAroundPrimeCandidates = new List<XUiView>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<XUiV_Window> openWindows = new List<XUiV_Window>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<XUiView> navViews = new List<XUiView>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float navigationAngleLimit = 78f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 previousInputVector = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float inputRepeatTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool initialRepeat;

	public List<XUiC_ItemStackGrid> activeItemStackGrids = new List<XUiC_ItemStackGrid>();
}
