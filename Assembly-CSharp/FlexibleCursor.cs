using System;
using System.Collections;
using Platform;
using UnityEngine;

public class FlexibleCursor : CursorControllerAbs
{
	public bool SoftcursorAllowed
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.guiActions != null && this.guiActions.Enabled && LocalPlayerUI.AnyModalWindowOpen();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.AwakeBase();
		this.speedMultiplier = this.speed;
		this.cursor = base.GetComponentInChildren<UISprite>();
		if (FlexibleCursor.defaultMouseCursor == null)
		{
			FlexibleCursor.emptyCursor = new Texture2D(32, 32, 5, false);
			for (int i = 0; i < 32; i++)
			{
				for (int j = 0; j < 32; j++)
				{
					FlexibleCursor.emptyCursor.SetPixel(i, j, new Color(0f, 0f, 0f, 0.01f));
				}
			}
			FlexibleCursor.emptyCursor.Apply();
			FlexibleCursor.defaultMouseCursor = Resources.Load<Texture2D>(FlexibleCursor.defaultMouseCursorResource);
			FlexibleCursor.defaultControllerCursor = Resources.Load<Texture2D>(FlexibleCursor.defaultControllerCursorResource);
			FlexibleCursor.mapCursor = Resources.Load<Texture2D>(FlexibleCursor.mapCursorResource);
		}
		UISprite[] componentsInChildren = base.GetComponentsInChildren<UISprite>(true);
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			componentsInChildren[k].gameObject.SetActive(false);
		}
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.OnLastInputStyleChanged;
		FlexibleCursor.SetCursor(FlexibleCursor.currentCursorType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		FlexibleCursor.SetCursor(FlexibleCursor.currentCursorType);
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
		IPlatform nativePlatform = PlatformManager.NativePlatform;
		if (((nativePlatform != null) ? nativePlatform.Input : null) != null)
		{
			PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= this.OnLastInputStyleChanged;
		}
		base.DestroyBase();
	}

	public override void UpdateMoveSpeed()
	{
		CursorControllerAbs.regularSpeed = GamePrefs.GetFloat(EnumGamePrefs.OptionsInterfaceSensitivity);
		this.speed = 500f + 1000f * CursorControllerAbs.regularSpeed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Application.isPlaying)
		{
			IPlatform nativePlatform = PlatformManager.NativePlatform;
			if (((nativePlatform != null) ? nativePlatform.Input : null) != null && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && this.SoftcursorAllowed)
			{
				this.HandleControllerInput();
			}
			this.LastFrameTime = Time.realtimeSinceStartup;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleControllerInput()
	{
		if (this.guiActions == null)
		{
			return;
		}
		Vector2 vector;
		vector..ctor(this.guiActions.Right.RawValue - this.guiActions.Left.RawValue, this.guiActions.Up.RawValue - this.guiActions.Down.RawValue);
		float magnitude = vector.magnitude;
		Vector3 vector2 = this.GetScreenPosition();
		Vector3 vector3 = vector2;
		if (this.bHasHoverTarget && (this.HoverTarget == null || !this.HoverTarget.ColliderEnabled || !this.HoverTarget.IsActiveInHierarchy))
		{
			this.HoverTarget = null;
		}
		float num = (this.bHasHoverTarget ? CursorControllerAbs.hoverSpeed : 1f);
		this.currentAcceleration = Mathf.Clamp(this.currentAcceleration + magnitude * Time.unscaledDeltaTime, 0f, Mathf.Min(magnitude, num));
		this.speedMultiplier = Mathf.MoveTowards(this.speedMultiplier, this.bHasHoverTarget ? CursorControllerAbs.hoverSpeed : 1f, Time.unscaledDeltaTime * (this.bHasHoverTarget ? 10f : 1f));
		float num2 = Time.unscaledDeltaTime * this.speed * this.speedMultiplier * this.accelerationCurve.Evaluate(this.currentAcceleration);
		vector.x *= num2;
		vector.y *= num2;
		vector3.x += vector.x;
		vector3.y += vector.y;
		if (CursorControllerAbs.bSnapCursor)
		{
			if (vector2 == vector3)
			{
				if (!this.snapped)
				{
					vector3 = this.SnapOs(vector3);
					this.snapped = true;
				}
			}
			else
			{
				this.snapped = false;
			}
		}
		vector3 = this.ConstrainCursorOs(vector3);
		this.SetScreenPosition(vector3.x, vector3.y);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 SnapOs(Vector3 _newPos)
	{
		if (this.hoverTarget == null || !this.hoverTarget.IsActiveInHierarchy)
		{
			return _newPos;
		}
		if (this.cursorWorldBounds.extents.x > this.hoverTarget.ColliderBounds.extents.x - this.OffsetSnapBounds)
		{
			return this.uiCamera.cachedCamera.WorldToScreenPoint(this.hoverTarget.ColliderBounds.center);
		}
		Vector3 vector = this.hoverTarget.ColliderBounds.ClosestPoint(this.uiCamera.cachedCamera.ScreenToWorldPoint(_newPos));
		Vector3 vector2 = Vector3.right * this.cursorWorldBounds.extents.x;
		Vector3 vector3 = vector - vector2;
		Vector3 vector4 = vector + vector2;
		if (!this.hoverTarget.ColliderBounds.Contains(vector3))
		{
			vector = this.hoverTarget.ColliderBounds.ClosestPoint(vector3) + vector2;
		}
		else if (!this.hoverTarget.ColliderBounds.Contains(vector4))
		{
			vector = this.hoverTarget.ColliderBounds.ClosestPoint(vector4) - vector2;
		}
		vector.y = this.hoverTarget.ColliderBounds.center.y;
		return this.uiCamera.cachedCamera.WorldToScreenPoint(vector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ConstrainCursorOs(Vector3 _newPos)
	{
		Vector3 vector = this.ConstrainToBounds(_newPos);
		vector.x = Mathf.Clamp(vector.x, 5f, (float)(Screen.width - 5));
		vector.y = Mathf.Clamp(vector.y, 5f, (float)(Screen.height - 5));
		return vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ConstrainToBounds(Vector3 _newPosition)
	{
		Vector3 vector = _newPosition;
		vector.z = this.currentBounds.center.z;
		return this.currentBounds.ClosestPoint(vector);
	}

	public override Vector2 GetScreenPosition()
	{
		return MouseLib.GetLocalMousePosition();
	}

	public override Vector2 GetLocalScreenPosition()
	{
		return MouseLib.GetLocalMousePosition();
	}

	public override void SetScreenPosition(Vector2 _newPosition)
	{
		MouseLib.SetCursorPosition((int)(_newPosition.x + 0.5f), (int)(_newPosition.y + 0.5f));
	}

	public override void SetScreenPosition(float _x, float _y)
	{
		this.SetScreenPosition(new Vector2(_x, _y));
	}

	public override void ResetToCenter()
	{
		this.SetScreenPosition((float)(Screen.width / 2), (float)(Screen.height / 2));
	}

	public override void SetNavigationTarget(XUiView _view)
	{
		throw new NotImplementedException();
	}

	public override void SetNavigationTargetLater(XUiView _view)
	{
		throw new NotImplementedException();
	}

	public override void ResetNavigationTarget()
	{
		throw new NotImplementedException();
	}

	public override void SetNavigationLockView(XUiView _view, XUiView _viewToSelect = null)
	{
		throw new NotImplementedException();
	}

	public override bool IsWithinNavigationLockView(XUiView _view)
	{
		throw new NotImplementedException();
	}

	public override void RefreshSelection()
	{
		throw new NotImplementedException();
	}

	public override void SetCursorHidden(bool _hidden)
	{
		GameManager.Instance.SetCursorEnabledOverride(_hidden, false);
	}

	public override bool GetCursorHidden()
	{
		return GameManager.Instance.GetCursorEnabledOverride();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnVirtualCursorVisibleChanged()
	{
		throw new NotImplementedException();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator ApplyCursorChangeLater()
	{
		while (!Cursor.visible)
		{
			yield return null;
		}
		Cursor.SetCursor(FlexibleCursor.currentCursorTexture, FlexibleCursor.currentCursorHotspot, 0);
		FlexibleCursor.cursorUpdateCo = null;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetCursorTexture(Texture2D _tex, Vector2 _hotspot)
	{
		if (_tex != FlexibleCursor.currentCursorTexture)
		{
			FlexibleCursor.currentCursorTexture = _tex;
			FlexibleCursor.currentCursorHotspot = _hotspot;
			if (FlexibleCursor.cursorUpdateCo == null)
			{
				FlexibleCursor.cursorUpdateCo = ThreadManager.StartCoroutine(FlexibleCursor.ApplyCursorChangeLater());
			}
		}
	}

	public new static void SetCursor(CursorControllerAbs.ECursorType _cursorType)
	{
		FlexibleCursor.currentCursorType = _cursorType;
		switch (_cursorType)
		{
		case CursorControllerAbs.ECursorType.None:
			FlexibleCursor.SetCursorTexture(FlexibleCursor.emptyCursor, FlexibleCursor.mapCursorCenter);
			return;
		case CursorControllerAbs.ECursorType.Default:
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				FlexibleCursor.SetCursorTexture(FlexibleCursor.defaultMouseCursor, FlexibleCursor.defaultMouseCursorCenter);
				return;
			}
			FlexibleCursor.SetCursorTexture(FlexibleCursor.defaultControllerCursor, FlexibleCursor.defaultControllerCursorCenter);
			return;
		case CursorControllerAbs.ECursorType.Map:
			FlexibleCursor.SetCursorTexture(FlexibleCursor.mapCursor, FlexibleCursor.mapCursorCenter);
			return;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float BaseSpeed = 500f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float SpeedModRange = 1000f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string defaultMouseCursorResource = "@:Textures/UI/cursor01.tga";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly Vector2 defaultMouseCursorCenter = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string defaultControllerCursorResource = "@:Textures/UI/soft_cursor";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly Vector2 defaultControllerCursorCenter = new Vector2(16f, 16f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string mapCursorResource = "@:Textures/UI/map_cursor.tga";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly Vector2 mapCursorCenter = new Vector2(16f, 16f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture2D emptyCursor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture2D defaultControllerCursor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture2D defaultMouseCursor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture2D mapCursor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture2D currentCursorTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector2 currentCursorHotspot;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static CursorControllerAbs.ECursorType currentCursorType = CursorControllerAbs.ECursorType.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Coroutine cursorUpdateCo;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float speed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float speedMultiplier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float LastFrameTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool snapped;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currentAcceleration;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float OffsetSnapBounds = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public PlayerInputManager.InputStyle m_lastInputStyle = PlayerInputManager.InputStyle.Count;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int CONTROLLER_CURSOR_MOVEMENT_LIMIT = 5;
}
