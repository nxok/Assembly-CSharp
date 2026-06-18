using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public abstract class CursorControllerAbs : MonoBehaviour, IGamePrefsChangedListener
{
	public static bool PrefabReady
	{
		get
		{
			return CursorControllerAbs.softCursorPrefab != null;
		}
	}

	public virtual XUiView HoverTarget
	{
		get
		{
			return this.hoverTarget;
		}
		set
		{
			this.hoverTarget = value;
			this.bHasHoverTarget = value != null;
		}
	}

	public XUiView navigationTarget
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public XUiView navigationTargetLater
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public XUiView lockNavigationToView
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public bool Locked
	{
		get
		{
			return this._locked;
		}
		set
		{
			this._locked = value;
		}
	}

	public bool VirtualCursorHidden
	{
		get
		{
			return this._virtualCursorHidden;
		}
		set
		{
			if (value != this._virtualCursorHidden)
			{
				this._virtualCursorHidden = value;
				this.OnVirtualCursorVisibleChanged();
			}
		}
	}

	public XUiView CurrentTarget
	{
		get
		{
			if (this.CursorModeActive)
			{
				return this.hoverTarget;
			}
			return this.navigationTarget;
		}
	}

	public virtual bool CursorModeActive
	{
		get
		{
			return false;
		}
	}

	public abstract Vector2 GetScreenPosition();

	public abstract Vector2 GetLocalScreenPosition();

	public abstract void SetScreenPosition(Vector2 _newPosition);

	public abstract void SetScreenPosition(float _x, float _y);

	public abstract void SetNavigationTarget(XUiView _view);

	public abstract void SetNavigationTargetLater(XUiView _view);

	public abstract void ResetNavigationTarget();

	public abstract void ResetToCenter();

	public abstract void SetNavigationLockView(XUiView _view, XUiView _viewToSelect = null);

	public abstract bool IsWithinNavigationLockView(XUiView _view);

	public abstract void RefreshSelection();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void OnVirtualCursorVisibleChanged();

	public void SetGUIActions(PlayerActionsGUI _guiActions)
	{
		this.guiActions = _guiActions;
	}

	public void SetWindowManager(GUIWindowManager _windowManager)
	{
		this.windowManager = _windowManager;
	}

	public static void UpdateGamePrefs()
	{
		CursorControllerAbs.bSnapCursor = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerCursorSnap);
		CursorControllerAbs.regularSpeed = GamePrefs.GetFloat(EnumGamePrefs.OptionsInterfaceSensitivity);
		CursorControllerAbs.hoverSpeed = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerCursorHoverSensitivity);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void AwakeBase()
	{
		GamePrefs.AddChangeListener(this);
		GameOptionsManager.ResolutionChanged += this.OnResolutionChanged;
		CursorControllerAbs.UpdateGamePrefs();
		CursorControllerAbs.softCursors.Add(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DestroyBase()
	{
		GamePrefs.RemoveChangeListener(this);
		GameOptionsManager.ResolutionChanged -= this.OnResolutionChanged;
		CursorControllerAbs.softCursors.Remove(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void InitCursorBounds()
	{
		this.cursorWorldBounds = new Bounds(this.cursor.worldCenter, Vector3.zero);
		Vector3[] worldCorners = this.cursor.worldCorners;
		for (int i = 0; i < worldCorners.Length; i++)
		{
			this.cursorWorldBounds.Encapsulate(worldCorners[i]);
		}
		Bounds bounds;
		bounds..ctor(this.uiCamera.cachedCamera.WorldToScreenPoint(this.cursorWorldBounds.min), Vector3.zero);
		bounds.Encapsulate(this.uiCamera.cachedCamera.WorldToScreenPoint(this.cursorWorldBounds.max));
		this.cursorBuffer = bounds.extents;
	}

	public abstract void UpdateMoveSpeed();

	public void UpdateBounds(string _boundsName, Bounds _bounds)
	{
		_bounds.Expand(this.cursorBuffer);
		this.activeBounds[_boundsName] = _bounds;
		this.RefreshBounds();
	}

	public void RemoveBounds(string _boundsName)
	{
		this.activeBounds.Remove(_boundsName);
		this.RefreshBounds();
	}

	public void RefreshBounds()
	{
		this.currentBounds.size = Vector3.zero;
		if (this.activeBounds.Count > 0)
		{
			bool flag = true;
			using (Dictionary<string, Bounds>.Enumerator enumerator = this.activeBounds.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, Bounds> keyValuePair = enumerator.Current;
					if (flag)
					{
						this.currentBounds.center = keyValuePair.Value.center;
						flag = false;
					}
					this.currentBounds.Encapsulate(keyValuePair.Value);
				}
				return;
			}
		}
		this.currentBounds.center = new Vector3(0f, 0f);
		this.currentBounds.Encapsulate(new Vector3((float)this.uiCamera.cachedCamera.pixelWidth, (float)this.uiCamera.cachedCamera.pixelHeight));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnResolutionChanged(int _width, int _height)
	{
		base.StartCoroutine(this.RefreshBoundsNextFrame());
	}

	public void OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum == EnumGamePrefs.OptionsInterfaceSensitivity)
		{
			this.UpdateMoveSpeed();
			return;
		}
		if (_enum == EnumGamePrefs.OptionsControllerCursorSnap)
		{
			CursorControllerAbs.bSnapCursor = GamePrefs.GetBool(_enum);
			return;
		}
		if (_enum != EnumGamePrefs.OptionsControllerCursorHoverSensitivity)
		{
			return;
		}
		CursorControllerAbs.hoverSpeed = GamePrefs.GetFloat(_enum);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator RefreshBoundsNextFrame()
	{
		yield return null;
		this.RefreshBounds();
		yield break;
	}

	public abstract void SetCursorHidden(bool _hidden);

	public abstract bool GetCursorHidden();

	public bool GetMouseButtonDown(UICamera.MouseButton _mouseButton)
	{
		if (this.guiActions == null)
		{
			return false;
		}
		if (GUIWindowConsole.IsOpen())
		{
			return false;
		}
		switch (_mouseButton)
		{
		case 0:
			return this.guiActions.Submit.WasPressed || this.guiActions.LeftClick.WasPressed;
		case 1:
			return this.guiActions.Inspect.WasPressed || this.guiActions.RightClick.WasPressed;
		case 2:
			return this.guiActions.MiddleClick.WasPressed;
		default:
			return false;
		}
	}

	public bool GetMouseButton(UICamera.MouseButton _mouseButton)
	{
		if (this.guiActions == null)
		{
			return false;
		}
		if (GUIWindowConsole.IsOpen())
		{
			return false;
		}
		switch (_mouseButton)
		{
		case 0:
			return this.guiActions.Submit.IsPressed || this.guiActions.LeftClick.IsPressed;
		case 1:
			return this.guiActions.Inspect.IsPressed || this.guiActions.RightClick.IsPressed;
		case 2:
			return this.guiActions.MiddleClick.IsPressed;
		default:
			return false;
		}
	}

	public bool GetMouseButtonUp(UICamera.MouseButton _mouseButton)
	{
		if (this.guiActions == null)
		{
			return false;
		}
		switch (_mouseButton)
		{
		case 0:
			return this.guiActions.Submit.WasReleased || this.guiActions.LeftClick.WasReleased;
		case 1:
			return this.guiActions.Inspect.WasReleased || this.guiActions.RightClick.WasReleased;
		case 2:
			return this.guiActions.MiddleClick.WasReleased;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DebugDrawBound(Bounds _bound)
	{
		Vector3 vector = _bound.max;
		Vector3 vector2 = _bound.min;
		Vector3 vector3;
		vector3..ctor(vector2.x, vector.y, vector.z);
		Vector3 vector4;
		vector4..ctor(vector.x, vector2.y, vector2.z);
		vector = this.uiCamera.cachedCamera.ScreenToWorldPoint(vector);
		vector3 = this.uiCamera.cachedCamera.ScreenToWorldPoint(vector3);
		vector4 = this.uiCamera.cachedCamera.ScreenToWorldPoint(vector4);
		vector2 = this.uiCamera.cachedCamera.ScreenToWorldPoint(vector2);
		Debug.DrawLine(vector, vector3);
		Debug.DrawLine(vector4, vector2);
		Debug.DrawLine(vector, vector4);
		Debug.DrawLine(vector3, vector2);
	}

	public static void SetCursor(CursorControllerAbs.ECursorType _cursorType)
	{
		SoftCursor.SetCursor(_cursorType);
	}

	public static void LoadStaticData(LoadManager.LoadGroup _loadGroup)
	{
		LoadManager.LoadAssetFromResources<GameObject>(CursorControllerAbs.softCursorPrefabPath, delegate(GameObject _asset)
		{
			CursorControllerAbs.softCursorPrefab = _asset;
		}, null, false, true);
	}

	public static CursorControllerAbs AddSoftCursor(UICamera _camera, PlayerActionsGUI _guiActions, GUIWindowManager _windowManager)
	{
		GameObject gameObject = NGUITools.AddChild(_camera.gameObject, CursorControllerAbs.softCursorPrefab);
		SoftCursor component = gameObject.GetComponent<SoftCursor>();
		component.SetGUIActions(_guiActions);
		component.SetWindowManager(_windowManager);
		_camera.cancelKey0 = 0;
		_camera.submitKey1 = 0;
		_camera.cancelKey1 = 0;
		UICamera.GetMousePosition = new UICamera.GetMousePositionFunc(component.GetScreenPosition);
		UICamera.GetMouseButton = new UICamera.GetMouseButtonFunc(component.GetMouseButton);
		UICamera.GetMouseButtonDown = new UICamera.GetMouseButtonFunc(component.GetMouseButtonDown);
		UICamera.GetMouseButtonUp = new UICamera.GetMouseButtonFunc(component.GetMouseButtonUp);
		gameObject.SetActive(true);
		return component;
	}

	public void PlayPagingSound()
	{
		Manager.PlayXUiSound(this.pagingSound, 1f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public CursorControllerAbs()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string softCursorPrefabPath = "Prefabs/SoftCursor";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static GameObject softCursorPrefab;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static List<CursorControllerAbs> softCursors = new List<CursorControllerAbs>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public UICamera uiCamera;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public PlayerActionsGUI guiActions;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GUIWindowManager windowManager;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public UISprite cursor;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Bounds cursorWorldBounds;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 cursorBuffer;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Dictionary<string, Bounds> activeBounds = new Dictionary<string, Bounds>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Bounds currentBounds;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioClip cursorSelectSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioClip pagingSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public XUiView hoverTarget;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bHasHoverTarget;

	public static bool bSnapCursor;

	public static float regularSpeed = 1f;

	public static float hoverSpeed = 1f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimationCurve accelerationCurve;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool _locked;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool _virtualCursorHidden;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static bool FreeCursorEnabled = true;

	public enum InputType
	{
		Controller,
		Mouse,
		Both
	}

	public enum ECursorType
	{
		None,
		Default,
		Map,
		Count
	}
}
