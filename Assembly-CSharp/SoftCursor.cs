using System;
using System.Collections;
using Audio;
using InControl;
using Platform;
using UnityEngine;

public class SoftCursor : CursorControllerAbs
{
	public PlayerInputManager.InputStyle LastInputStyle
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_lastInputStyle;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (value != this.m_lastInputStyle)
			{
				this.m_lastInputStyle = value;
				this.SetVisible(this.SoftcursorAllowed);
			}
		}
	}

	public bool SoftcursorEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.m_softcursorEnabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != this.m_softcursorEnabled)
			{
				this.m_softcursorEnabled = value;
				this.SetVisible(this.SoftcursorAllowed);
			}
		}
	}

	public override XUiView HoverTarget
	{
		get
		{
			return base.HoverTarget;
		}
		set
		{
			base.HoverTarget = value;
			if (this.hoverTarget != null && !this.CursorModeActive && this.LastInputStyle != PlayerInputManager.InputStyle.Keyboard && !this.movingMouse && this.hoverTarget.IsNavigatable && !this.hoverTarget.HasHoverSound)
			{
				Manager.PlayXUiSound(this.cursorSelectSound, 0.75f);
			}
		}
	}

	public override bool CursorModeActive
	{
		get
		{
			return this.cursorModeActive;
		}
	}

	public bool SoftcursorAllowed
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.guiActions != null && this.guiActions.Enabled && LocalPlayerUI.AnyModalWindowOpen();
		}
	}

	public Vector3 Position
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.cursor.transform.position;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.cursor.transform.position = value;
			this.lastMousePosition = Input.mousePosition;
		}
	}

	public Vector3 LocalPosition
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.cursor.transform.localPosition;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.cursor.transform.localPosition = value;
			this.lastMousePosition = Input.mousePosition;
		}
	}

	public static CursorLockMode DefaultCursorLockState
	{
		get
		{
			return 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.AwakeBase();
		Cursor.lockState = SoftCursor.DefaultCursorLockState;
		foreach (UISprite uisprite in base.GetComponentsInChildren<UISprite>())
		{
			string name = uisprite.gameObject.name;
			if (name == "Cursor")
			{
				this.cursor = uisprite;
			}
			else if (name == "SelectionBox")
			{
				this.selectionBox = uisprite;
			}
		}
		this.cursorPanel = base.GetComponent<UIPanel>();
		GameObject gameObject = new GameObject("cursorMouse");
		gameObject.transform.parent = base.gameObject.transform;
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		if (SoftCursor.defaultMouseCursorAtlas == null)
		{
			foreach (UIAtlas uiatlas in Resources.FindObjectsOfTypeAll<UIAtlas>())
			{
				if (uiatlas.name.EqualsCaseInsensitive(SoftCursor.defaultMouseCursorAtlasName))
				{
					SoftCursor.defaultMouseCursorAtlas = uiatlas;
				}
				if (uiatlas.name.EqualsCaseInsensitive(SoftCursor.emptyCursorAtlasName))
				{
					SoftCursor.emptyCursorAtlas = uiatlas;
				}
				if (uiatlas.name.EqualsCaseInsensitive(SoftCursor.defaultControllerCursorAtlasName))
				{
					SoftCursor.defaultControllerCursorAtlas = uiatlas;
				}
				if (uiatlas.name.EqualsCaseInsensitive(SoftCursor.mapCursorAtlasName))
				{
					SoftCursor.mapCursorAtlas = uiatlas;
				}
			}
		}
		LocalPlayerUI.primaryUI.xui.LoadData<AudioClip>("@:Sounds/UI/ui_hover.wav", delegate(AudioClip _o)
		{
			this.cursorSelectSound = _o;
		});
		LocalPlayerUI.primaryUI.xui.LoadData<AudioClip>("@:Sounds/UI/ui_tab.wav", delegate(AudioClip _o)
		{
			this.pagingSound = _o;
		});
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.OnLastInputStyleChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		if (_style != PlayerInputManager.InputStyle.Keyboard)
		{
			if (this.LastInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				this.LocalPosition = this.cursorPanel.transform.worldToLocalMatrix.MultiplyPoint3x4(this.uiCamera.cachedCamera.ScreenToWorldPoint(Input.mousePosition));
			}
			this.RefreshSelection();
		}
		this.SetVisible(this.SoftcursorAllowed);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.uiCamera = base.GetComponentInParent<UICamera>();
		this.UpdateMoveSpeed();
		base.InitCursorBounds();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		base.DestroyBase();
	}

	public override void UpdateMoveSpeed()
	{
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsInterfaceSensitivity);
		this.speed = 500f + 2000f * @float;
		this.mouseSpeed = (500f + 2000f * @float) / 1000f * (1f / MouseBindingSource.ScaleX) * 4f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Application.isPlaying && !GameManager.Instance.IsQuitting && !GUIWindowConsole.IsOpen())
		{
			this.LastInputStyle = PlatformManager.NativePlatform.Input.CurrentInputStyle;
			if (this.guiActions != null && this.windowManager != null)
			{
				if (this.SoftcursorAllowed != this.SoftcursorEnabled)
				{
					this.SoftcursorEnabled = this.SoftcursorAllowed & !this.hidden;
				}
				if (this.SoftcursorEnabled)
				{
					this.HandleMovement();
				}
			}
			this.LastFrameTime = Time.realtimeSinceStartup;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (this.selectionBox.enabled)
		{
			this.RefreshSelection();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleMovement()
	{
		if (this.guiActions == null)
		{
			return;
		}
		if (base.Locked || this.hidden)
		{
			return;
		}
		this.movingMouse = false;
		Vector2 vector = Input.mousePosition;
		if (vector != this.lastMousePosition)
		{
			this.movingMouse = true;
		}
		this.lastMousePosition = vector;
		if (this.LastInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			if (this.movingMouse)
			{
				this.LocalPosition = this.cursorPanel.transform.worldToLocalMatrix.MultiplyPoint3x4(this.uiCamera.cachedCamera.ScreenToWorldPoint(Input.mousePosition));
				this.SetVisible(true);
			}
			else
			{
				if (!CursorControllerAbs.FreeCursorEnabled)
				{
					return;
				}
				Vector2 vector2;
				vector2..ctor(this.guiActions.Right.RawValue - this.guiActions.Left.RawValue, this.guiActions.Up.RawValue - this.guiActions.Down.RawValue);
				float magnitude = vector2.magnitude;
				if (magnitude > 0f)
				{
					this.cursorModeActive = true;
					this.SetVisible(true);
					this.SetNavigationTarget(null);
				}
				Vector3 localPosition = this.LocalPosition;
				Vector3 localPosition2 = this.LocalPosition;
				if (this.bHasHoverTarget && (this.HoverTarget == null || !this.HoverTarget.ColliderEnabled || !this.HoverTarget.IsActiveInHierarchy))
				{
					this.HoverTarget = null;
				}
				float num = (this.bHasHoverTarget ? CursorControllerAbs.hoverSpeed : 1f);
				this.currentAcceleration = Mathf.Clamp(this.currentAcceleration + magnitude * Time.unscaledDeltaTime, 0f, Mathf.Min(magnitude, num));
				this.speedMultiplier = Mathf.MoveTowards(this.speedMultiplier, (this.bHasHoverTarget && this.HoverTarget.IsSnappable) ? CursorControllerAbs.hoverSpeed : 1f, Time.unscaledDeltaTime * (this.bHasHoverTarget ? 10f : 1f));
				float num2 = Time.unscaledDeltaTime * this.speed * this.speedMultiplier * this.accelerationCurve.Evaluate(this.currentAcceleration);
				vector2.x *= num2;
				vector2.y *= num2;
				localPosition2.x += vector2.x;
				localPosition2.y += vector2.y;
				this.LocalPosition = localPosition2;
				if (CursorControllerAbs.bSnapCursor && localPosition == localPosition2)
				{
					if (!this.snapped)
					{
						this.Snap();
						this.snapped = true;
					}
				}
				else
				{
					this.snapped = false;
				}
			}
			this.ConstrainCursor();
			return;
		}
		this.LocalPosition = this.cursorPanel.transform.worldToLocalMatrix.MultiplyPoint3x4(this.uiCamera.cachedCamera.ScreenToWorldPoint(Input.mousePosition));
		this.ConstrainCursor();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ConstrainToBounds(Vector3 _newPosition)
	{
		Vector3 vector = _newPosition;
		if (this.LastInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			vector.z = this.currentBounds.center.z;
			vector = this.currentBounds.ClosestPoint(vector);
		}
		return vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConstrainCursor()
	{
		Vector3 vector = this.uiCamera.cachedCamera.WorldToScreenPoint(this.Position);
		Vector3 vector2 = this.ConstrainToBounds(vector);
		vector2 = this.uiCamera.cachedCamera.ScreenToViewportPoint(vector2);
		if (vector2.x < 0f)
		{
			vector2.x = 0f;
		}
		else if (vector2.x > 1f)
		{
			vector2.x = 1f;
		}
		if (vector2.y < 0f)
		{
			vector2.y = 0f;
		}
		else if (vector2.y > 1f)
		{
			vector2.y = 1f;
		}
		this.Position = this.uiCamera.cachedCamera.ViewportToWorldPoint(vector2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Snap()
	{
		if (GamePrefs.GetBool(EnumGamePrefs.OptionsControllerCursorSnap) && this.hoverTarget != null && this.hoverTarget.IsSnappable && this.hoverTarget.ColliderEnabled && this.hoverTarget.IsActiveInHierarchy)
		{
			Bounds colliderBounds = this.hoverTarget.ColliderBounds;
			if (this.cursorWorldBounds.extents.x > colliderBounds.extents.x - this.OffsetSnapBounds)
			{
				this.Position = colliderBounds.center;
				return;
			}
			Vector3 vector = colliderBounds.ClosestPoint(this.Position);
			Vector3 vector2 = Vector3.right * this.cursorWorldBounds.extents.x;
			Vector3 vector3 = vector - vector2;
			Vector3 vector4 = vector + vector2;
			if (!colliderBounds.Contains(vector3))
			{
				vector = colliderBounds.ClosestPoint(vector3) + vector2;
			}
			else if (!colliderBounds.Contains(vector4))
			{
				vector = colliderBounds.ClosestPoint(vector4) - vector2;
			}
			vector.y = colliderBounds.center.y;
			this.Position = vector;
		}
	}

	public override Vector2 GetScreenPosition()
	{
		Vector2 vector = default(Vector2);
		if (this.uiCamera != null)
		{
			vector = this.uiCamera.cachedCamera.WorldToScreenPoint(this.Position);
		}
		return vector;
	}

	public override Vector2 GetLocalScreenPosition()
	{
		Vector2 vector = default(Vector2);
		if (this.uiCamera != null)
		{
			vector = this.uiCamera.cachedCamera.WorldToViewportPoint(this.Position);
			vector.x *= (float)this.uiCamera.cachedCamera.pixelWidth;
			vector.y *= (float)this.uiCamera.cachedCamera.pixelHeight;
		}
		return vector;
	}

	public override void SetScreenPosition(Vector2 _newPosition)
	{
		this.Position = this.uiCamera.cachedCamera.ScreenToWorldPoint(_newPosition);
	}

	public override void SetScreenPosition(float _x, float _y)
	{
		this.SetScreenPosition(new Vector2(_x, _y));
	}

	public override void ResetToCenter()
	{
		this.LocalPosition = Vector3.zero;
	}

	public override void SetCursorHidden(bool _hidden)
	{
		this.hidden = _hidden;
		this.SoftcursorEnabled &= !this.hidden;
		SoftCursor.SetCursor(_hidden ? CursorControllerAbs.ECursorType.None : CursorControllerAbs.ECursorType.Default);
		Cursor.visible = PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && !this.hidden;
		if (this.LastInputStyle != PlayerInputManager.InputStyle.Keyboard && !this.hidden)
		{
			this.RefreshSelection();
		}
	}

	public override bool GetCursorHidden()
	{
		return this.hidden;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetCursorSprite(INGUIAtlas _atlasMouse, string _spriteMouse, UIWidget.Pivot _pivotMouse, INGUIAtlas _atlasController, string _spriteController, UIWidget.Pivot _pivotController)
	{
		this.cursor.atlas = _atlasController;
		this.cursor.spriteName = _spriteController;
		this.cursor.pivot = _pivotController;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetCursorSpriteNone()
	{
		this.cursor.atlas = null;
		this.cursor.spriteName = "";
		this.SetSelectionBoxEnabled(false);
	}

	public new static void SetCursor(CursorControllerAbs.ECursorType _cursorType)
	{
		switch (_cursorType)
		{
		case CursorControllerAbs.ECursorType.None:
		{
			for (int i = 0; i < CursorControllerAbs.softCursors.Count; i++)
			{
				(CursorControllerAbs.softCursors[i] as SoftCursor).SetCursorSpriteNone();
			}
			return;
		}
		case CursorControllerAbs.ECursorType.Default:
		{
			for (int j = 0; j < CursorControllerAbs.softCursors.Count; j++)
			{
				(CursorControllerAbs.softCursors[j] as SoftCursor).SetCursorSprite(SoftCursor.defaultMouseCursorAtlas, SoftCursor.defaultMouseCursorSprite, SoftCursor.defaultMouseCursorPivot, SoftCursor.defaultControllerCursorAtlas, SoftCursor.defaultControllerCursorSprite, SoftCursor.defaultControllerCursorPivot);
			}
			return;
		}
		case CursorControllerAbs.ECursorType.Map:
		{
			for (int k = 0; k < CursorControllerAbs.softCursors.Count; k++)
			{
				(CursorControllerAbs.softCursors[k] as SoftCursor).SetCursorSprite(SoftCursor.mapCursorAtlas, SoftCursor.mapCursorSprite, SoftCursor.mapCursorPivot, SoftCursor.mapCursorAtlas, SoftCursor.mapCursorSprite, SoftCursor.mapCursorPivot);
			}
			return;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetVisible(bool _visible)
	{
		Cursor.lockState = (_visible ? SoftCursor.DefaultCursorLockState : 1);
		if (this.LastInputStyle == PlayerInputManager.InputStyle.Keyboard || this.movingMouse)
		{
			Cursor.visible = _visible;
			this.cursor.enabled = false;
			this.SetSelectionBoxEnabled(false);
		}
		else
		{
			Cursor.visible = false;
			this.cursor.enabled = _visible && CursorControllerAbs.FreeCursorEnabled && !base.Locked && this.cursorModeActive && !base.VirtualCursorHidden;
			this.SetSelectionBoxEnabled(_visible && !this.cursorModeActive && !this.hidden && base.navigationTarget != null && base.navigationTarget.UseSelectionBox);
		}
		GameManager.Instance.bCursorVisible = _visible;
		this.lastMousePosition = Input.mousePosition;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnVirtualCursorVisibleChanged()
	{
		if (this.LastInputStyle == PlayerInputManager.InputStyle.Keyboard || this.movingMouse)
		{
			return;
		}
		this.cursor.enabled = !base.VirtualCursorHidden && !base.Locked && this.cursorModeActive;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectionBoxEnabled(bool _enabled)
	{
		if (this.selectionBox == null)
		{
			return;
		}
		this.selectionBox.enabled = _enabled;
		this.selectionBox.alpha = 0f;
		if (!_enabled)
		{
			this.selectionBox.transform.parent = null;
		}
	}

	public static void SetCursorVisible(bool _visible)
	{
		for (int i = 0; i < CursorControllerAbs.softCursors.Count; i++)
		{
			(CursorControllerAbs.softCursors[i] as SoftCursor).SetVisible(_visible);
		}
	}

	public Vector2 GetFlatPosition()
	{
		return new Vector2(this.Position.x, this.Position.y);
	}

	public override void SetNavigationTarget(XUiView _view)
	{
		if (_view != null && !_view.IsNavigatable)
		{
			this.SetNavigationTarget(null);
			return;
		}
		if (_view == base.navigationTarget)
		{
			return;
		}
		if (base.lockNavigationToView != null && (_view == null || !_view.Controller.IsChildOf(base.lockNavigationToView.Controller)))
		{
			return;
		}
		if (base.navigationTarget != null)
		{
			base.navigationTarget.Controller.OnCursorUnSelected();
		}
		if (_view != null && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			_view.Controller.OnCursorSelected(true);
			this.cursorModeActive = false;
			this.SetSelectionBoxEnabled(_view.UseSelectionBox);
			this.PositionSelectionBox(_view);
		}
		else
		{
			this.SetSelectionBoxEnabled(false);
		}
		base.navigationTarget = _view;
		IPlatform nativePlatform = PlatformManager.NativePlatform;
		if (nativePlatform == null || nativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			this.SetVisible(base.navigationTarget != null);
		}
	}

	public override void RefreshSelection()
	{
		if (base.navigationTarget == null)
		{
			this.selectionBox.alpha = 0f;
			return;
		}
		this.PositionSelectionBox(base.navigationTarget);
		this.selectionBox.alpha = (base.navigationTarget.UseSelectionBox ? base.navigationTarget.Controller.GetParentWindow().PanelAlpha : 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PositionSelectionBox(XUiView _view)
	{
		this.Position = _view.ColliderCenter;
		if (_view.UseSelectionBox)
		{
			this.selectionBox.width = _view.Size.x + SoftCursor.selectionBoxMargin;
			this.selectionBox.height = _view.Size.y + SoftCursor.selectionBoxMargin;
			this.selectionBox.depth = 1000000;
			this.selectionBox.transform.localScale = Vector3.one;
			this.selectionBox.transform.localPosition = _view.LocalCenter;
			this.selectionBox.transform.localRotation = Quaternion.identity;
			this.selectionBox.transform.parent = _view.UiTransform;
			this.selectionBox.RemoveFromPanel();
		}
	}

	public override void SetNavigationLockView(XUiView _view, XUiView _viewToSelect = null)
	{
		if (_view != null && (!_view.IsVisible || !_view.IsActiveInHierarchy))
		{
			this.SetNavigationLockView(null, null);
			return;
		}
		base.lockNavigationToView = _view;
		if (_viewToSelect != null)
		{
			_viewToSelect.Controller.SelectCursorElement(true, false);
			return;
		}
		if (_view != null)
		{
			_view.Controller.SelectCursorElement(true, false);
		}
	}

	public override bool IsWithinNavigationLockView(XUiView _view)
	{
		return base.lockNavigationToView == null || _view.Controller.IsSelfOrChildOf(base.lockNavigationToView.Controller);
	}

	public override void SetNavigationTargetLater(XUiView _view)
	{
		if (_view == null)
		{
			this.SetNavigationTarget(null);
			return;
		}
		if (base.isActiveAndEnabled)
		{
			base.StartCoroutine(this.SetNavigationTargetWithDelay(_view));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SetNavigationTargetWithDelay(XUiView _view)
	{
		base.navigationTargetLater = _view;
		base.Locked = true;
		int num;
		for (int i = 0; i < 3; i = num + 1)
		{
			yield return null;
			num = i;
		}
		base.Locked = false;
		if (_view != null)
		{
			this.SetNavigationTarget(_view);
		}
		base.navigationTargetLater = null;
		this.lastMousePosition = Input.mousePosition;
		yield break;
	}

	public override void ResetNavigationTarget()
	{
		if (base.navigationTarget != null)
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				this.Position = base.navigationTarget.ColliderCenter;
				return;
			}
			this.SetNavigationTarget(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float BaseSpeed = 500f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float SpeedModRange = 2000f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string emptyCursorAtlasName = "UIAtlas";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string emptyCursorSprite = "";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly UIWidget.Pivot emptyCursorPivot = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static INGUIAtlas emptyCursorAtlas;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string defaultControllerCursorAtlasName = "UIAtlas";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string defaultControllerCursorSprite = "soft_cursor";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly UIWidget.Pivot defaultControllerCursorPivot = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static INGUIAtlas defaultControllerCursorAtlas;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string defaultMouseCursorAtlasName = "UIAtlas";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string defaultMouseCursorSprite = "cursor01";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly UIWidget.Pivot defaultMouseCursorPivot = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static INGUIAtlas defaultMouseCursorAtlas;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string mapCursorAtlasName = "UIAtlas";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string mapCursorSprite = "map_cursor";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly UIWidget.Pivot mapCursorPivot = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static INGUIAtlas mapCursorAtlas;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float speed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float mouseSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float LastFrameTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hidden;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool snapped;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float OffsetSnapBounds = 0.1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public UIPanel cursorPanel;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector2 lastMousePosition = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float currentAcceleration;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float speedMultiplier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool movingMouse;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public UISprite selectionBox;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int selectionBoxMargin = 10;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public PlayerInputManager.InputStyle m_lastInputStyle = PlayerInputManager.InputStyle.Count;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_softcursorEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool cursorModeActive;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public CursorControllerAbs.ECursorType currentCursorType = CursorControllerAbs.ECursorType.Default;
}
