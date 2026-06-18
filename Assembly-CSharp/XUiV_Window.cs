using System;
using UnityEngine;

public class XUiV_Window : XUiView
{
	public override UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return null;
		}
	}

	[XuiXmlAttribute("anchor", false)]
	public string Anchor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.anchor;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.anchor = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("cursor_area", false)]
	public bool IsCursorArea
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("lock_navigation", false)]
	public bool LockNavigation
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsOpen
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsInStackPanel
	{
		get
		{
			return this.stackPanel != null;
		}
	}

	public override Vector3[] WorldCorners
	{
		get
		{
			return this.panel.worldCorners;
		}
	}

	public XUiV_Window(XUi _xui, string _id)
		: base(_xui, _id)
	{
		this.xui.AddWindow(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UIPanel>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		this.panel = this.uiTransform.gameObject.GetComponent<UIPanel>();
	}

	public override void InitView()
	{
		base.InitView();
		this.setRootNode();
		base.Controller.OnVisiblity += this.updateVisibility;
		this.panel.depth = base.Depth + 1;
		this.panel.alpha = 0f;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (this.uiTransform != null)
		{
			Object.DestroyImmediate(this.uiTransform.gameObject);
		}
		this.xui.RemoveWindow(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void setRootNode()
	{
		Transform transform;
		if (this.stackPanel != null)
		{
			transform = this.stackPanel.Transform;
		}
		else if (!string.IsNullOrEmpty(this.Anchor))
		{
			transform = this.xui.GetAnchor(this.Anchor);
			if (transform == null)
			{
				Log.Error(string.Concat(new string[] { "Specified window anchor \"", this.Anchor, "\" not found for window \"", base.ID, "\"" }));
				throw new Exception();
			}
		}
		else
		{
			transform = this.xui.transform;
		}
		this.uiTransform.parent = transform;
		this.uiTransform.gameObject.layer = 12;
		this.uiTransform.localScale = Vector3.one;
		this.uiTransform.localPosition = new Vector3((float)base.PaddedPosition.x, (float)base.PaddedPosition.y, 0f);
		this.uiTransform.localEulerAngles = new Vector3(0f, 0f, this.rotation);
	}

	public float PanelAlpha
	{
		get
		{
			return this.panel.alpha;
		}
	}

	[XuiXmlAttribute("fade_window", false)]
	public bool Fade
	{
		get
		{
			return this.fade;
		}
		set
		{
			this.fade = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("fade_time", false)]
	public float FadeInTime
	{
		get
		{
			return this.fadeInTime;
		}
		set
		{
			this.fadeInTime = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("fade_delay", false)]
	public float DelayToFadeTime
	{
		get
		{
			return this.delayToFadeTime;
		}
		set
		{
			this.delayToFadeTime = value;
			base.SetDirty();
		}
	}

	public override void Update(float _dt)
	{
		if ((double)Time.timeScale < 0.01 || !this.fade)
		{
			this.delayTimer = this.delayToFadeTime + 1f;
			this.fadeTimer = this.fadeInTime;
		}
		if (this.delayTimer < this.delayToFadeTime)
		{
			this.delayTimer += _dt;
		}
		if (this.delayTimer < this.delayToFadeTime)
		{
			base.Update(_dt);
			return;
		}
		this.panel.alpha = Mathf.Lerp(this.panel.alpha, this.targetAlpha, this.fadeTimer / this.fadeInTime);
		this.fadeTimer += _dt;
		if (this.IsCursorArea && this.oldTransformPosition != this.uiTransform.position && base.IsVisible)
		{
			this.xui.UpdateWindowSoftCursorBounds(this);
			this.oldTransformPosition = this.uiTransform.position;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		this.setRootNode();
		base.updateData();
		this.panel.SetDirty();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.IsVisible = true;
		if (this.frameClosed == Time.frameCount)
		{
			this.panel.alpha = 1f;
			this.targetAlpha = 1f;
			this.fadeTimer = this.fadeInTime + 1f;
			this.delayTimer = this.delayToFadeTime + 1f;
		}
		else
		{
			this.panel.alpha = 0f;
			this.targetAlpha = 1f;
			this.fadeTimer = 0f;
			this.delayTimer = 0f;
		}
		if (this.IsCursorArea)
		{
			this.oldTransformPosition = Vector3.zero;
		}
		this.applyNavigationChanges();
		this.IsOpen = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyNavigationChanges()
	{
		CursorControllerAbs cursorController = this.xui.playerUI.CursorController;
		this.previousNavigationTarget = cursorController.navigationTargetLater ?? cursorController.CurrentTarget;
		if (this.LockNavigation)
		{
			this.previousNavigationLockView = cursorController.lockNavigationToView;
			cursorController.SetNavigationLockView(this, null);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		base.IsVisible = false;
		this.panel.alpha = 0f;
		this.targetAlpha = 0f;
		this.fadeTimer = this.fadeInTime + 1f;
		this.delayTimer = this.delayToFadeTime + 1f;
		this.frameClosed = Time.frameCount;
		this.IsOpen = false;
		if (this.LockNavigation)
		{
			this.xui.playerUI.CursorController.SetNavigationLockView(this.previousNavigationLockView, this.previousNavigationTarget);
		}
		this.previousNavigationTarget = null;
		this.previousNavigationLockView = null;
	}

	public void ForceVisible(float _alpha = -1f)
	{
		this.delayTimer = this.delayToFadeTime + 1f;
		this.fadeTimer = this.fadeInTime + 1f;
		if (_alpha >= 0f)
		{
			this.targetAlpha = _alpha;
		}
		this.panel.alpha = this.targetAlpha;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateVisibility(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (!this.IsCursorArea)
		{
			return;
		}
		if (_visibleInScene)
		{
			this.xui.UpdateWindowSoftCursorBounds(this);
			return;
		}
		this.xui.RemoveWindowFromSoftCursorBounds(this);
	}

	[XuiXmlAttribute("panel", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributePanel(string _value)
	{
		if (string.IsNullOrEmpty(_value))
		{
			return;
		}
		if (!this.xui.StackPanels.dict.TryGetValue(_value, out this.stackPanel))
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Could not find StackPanel '",
				_value,
				"' for view '",
				this.id,
				"' in window group '",
				this.controller.WindowGroup.Id,
				"'"
			}));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchor;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIPanel panel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView previousNavigationTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView previousNavigationLockView;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 oldTransformPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUi.StackPanel stackPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public float targetAlpha;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fade = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int frameClosed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeInTime = 0.05f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadeTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayToFadeTime = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float delayTimer;
}
