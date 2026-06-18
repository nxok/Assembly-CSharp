using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Audio;
using Unity.Profiling;
using UnityEngine;

public abstract class XUiView : IXUiElement
{
	public XUiController Controller
	{
		get
		{
			return this.controller;
		}
		set
		{
			this.controller = value;
			if (this.controller.ViewComponent != this)
			{
				this.controller.ViewComponent = this;
			}
		}
	}

	public abstract UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public Transform UiTransform
	{
		get
		{
			return this.uiTransform;
		}
	}

	public bool ColliderEnabled
	{
		get
		{
			return !this.destroyed && this.collider.enabled;
		}
	}

	public Vector3 ColliderCenter
	{
		get
		{
			if (!this.destroyed)
			{
				return this.collider.bounds.center;
			}
			return default(Vector3);
		}
	}

	public float ColliderHeightExtent
	{
		get
		{
			if (!this.destroyed)
			{
				return this.collider.bounds.size.y / 2f;
			}
			return 0f;
		}
	}

	public float ColliderWidthExtent
	{
		get
		{
			if (!this.destroyed)
			{
				return this.collider.bounds.size.x / 2f;
			}
			return 0f;
		}
	}

	public Bounds ColliderBounds
	{
		get
		{
			if (!this.destroyed)
			{
				return this.collider.bounds;
			}
			return default(Bounds);
		}
	}

	public virtual Vector3 LocalCenter
	{
		get
		{
			return (this.position + this.size / 2).AsVector2();
		}
	}

	public abstract Vector3[] WorldCorners { get; }

	[XuiXmlAttribute("repeat_content", false)]
	public bool RepeatContent { get; set; }

	[XuiXmlAttribute("repeat_count", false)]
	public virtual int RepeatCount { get; set; }

	public bool UiTransformIsHovered
	{
		get
		{
			return UICamera.hoveredObject == this.uiTransform.gameObject;
		}
	}

	public bool IsActiveInHierarchy
	{
		get
		{
			if (!(this.uiTransform != null))
			{
				return this.isVisible;
			}
			return this.uiTransform.gameObject.activeInHierarchy;
		}
	}

	public string ID
	{
		get
		{
			return this.id;
		}
	}

	[XuiXmlAttribute("name", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeName(string _value)
	{
		this.id = _value;
	}

	[XuiXmlAttribute("visible", false)]
	public bool IsVisible
	{
		get
		{
			return this.isVisible;
		}
		set
		{
			if (this.isVisible == value)
			{
				return;
			}
			this.isVisible = value;
			if (this.uiTransform != null && this.isVisible != this.uiTransform.gameObject.activeSelf)
			{
				if (this.ID == "newContent")
				{
					Log.Warning(string.Format("XUiView.IsVisible - SetActive({0}), frame={1}", this.isVisible, Time.frameCount));
				}
				this.uiTransform.gameObject.SetActive(this.isVisible);
			}
			this.Controller.OnVisibilityChanged(this.IsActiveInHierarchy);
			if (this.xui.playerUI.CursorController.navigationTarget == this)
			{
				this.xui.playerUI.RefreshNavigationTarget();
			}
			this.SetDirty();
		}
	}

	[XuiXmlAttribute("enabled", false)]
	public virtual bool Enabled
	{
		get
		{
			return this.enabled;
		}
		set
		{
			if (value == this.enabled)
			{
				return;
			}
			this.enabled = value;
			if (!this.enabled && this.xui.playerUI.CursorController.navigationTarget == this)
			{
				this.xui.playerUI.RefreshNavigationTarget();
			}
			if (!value && this.isOver)
			{
				this.OnHover(true);
			}
			this.Controller.OnEnabledChanged(this.enabled);
			this.SetDirty();
		}
	}

	[XuiXmlAttribute("collider_scale", false)]
	public float ColliderScale
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Protected)]
		set;
	} = 1f;

	[XuiXmlAttribute("collider_padding", false)]
	public Vector2i ColliderPadding
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Protected)]
		set;
	} = Vector2i.zero;

	[XuiXmlAttribute("size", false)]
	public Vector2i Size
	{
		get
		{
			return this.size;
		}
		set
		{
			if (this.size == value)
			{
				return;
			}
			this.size = value;
			this.SetDirty();
		}
	}

	[XuiXmlAttribute("width", false)]
	public int Width
	{
		get
		{
			return this.Size.x;
		}
		set
		{
			if (this.Width == value)
			{
				return;
			}
			Vector2i vector2i = this.Size;
			vector2i.x = value;
			this.Size = vector2i;
		}
	}

	[XuiXmlAttribute("height", false)]
	public int Height
	{
		get
		{
			return this.Size.y;
		}
		set
		{
			if (this.Height == value)
			{
				return;
			}
			Vector2i vector2i = this.Size;
			vector2i.y = value;
			this.Size = vector2i;
		}
	}

	[XuiXmlAttribute("pos", false)]
	public Vector2i Position
	{
		get
		{
			return this.position;
		}
		set
		{
			if (this.position == value)
			{
				return;
			}
			this.position = value;
			this.SetDirty();
			this.positionDirty = true;
		}
	}

	[XuiXmlAttribute("ignoreparentpadding", false)]
	public bool IgnoreParentPadding
	{
		get
		{
			return this.ignoreParentPadding;
		}
		set
		{
			if (this.ignoreParentPadding == value)
			{
				return;
			}
			this.ignoreParentPadding = value;
			this.SetDirty();
		}
	}

	public Vector2i PaddedPosition
	{
		get
		{
			Vector2i vector2i = this.position;
			Vector2i vector2i3;
			if (!this.ignoreParentPadding)
			{
				XUiController parent = this.Controller.Parent;
				Vector2i? vector2i2;
				if (parent == null)
				{
					vector2i2 = null;
				}
				else
				{
					XUiView viewComponent = parent.ViewComponent;
					vector2i2 = ((viewComponent != null) ? new Vector2i?(viewComponent.InnerPosition) : null);
				}
				vector2i3 = vector2i2 ?? Vector2i.zero;
			}
			else
			{
				vector2i3 = Vector2i.zero;
			}
			return vector2i + vector2i3;
		}
	}

	public virtual Vector2i InnerSize
	{
		get
		{
			return new Vector2i(this.size.x - this.padding.SumLeftRight, this.size.y - this.padding.SumTopBottom);
		}
	}

	public Vector2i InnerPosition
	{
		get
		{
			return new Vector2i(this.padding.Left, -this.padding.Top);
		}
	}

	[XuiXmlAttribute("rotation", false)]
	public float Rotation
	{
		get
		{
			return this.rotation;
		}
		set
		{
			if (Mathf.Approximately(this.rotation, value))
			{
				return;
			}
			this.rotation = value;
			this.SetDirty();
			this.rotationDirty = true;
		}
	}

	public int Depth
	{
		get
		{
			return this.depth;
		}
		set
		{
			if (this.depth == value)
			{
				return;
			}
			this.depth = value;
			this.SetDirty();
		}
	}

	[XuiXmlAttribute("depth", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeDepth(int _value)
	{
		XUiView viewComponent = this.controller.Parent.ViewComponent;
		this.Depth = _value + ((viewComponent != null) ? viewComponent.Depth : 0);
	}

	[XuiXmlAttribute("padding_left", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributePaddingLeft(int _value)
	{
		this.padding = this.padding.SetLeft(_value);
	}

	[XuiXmlAttribute("padding_right", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributePaddingRight(int _value)
	{
		this.padding = this.padding.SetRight(_value);
	}

	[XuiXmlAttribute("padding_top", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributePaddingTop(int _value)
	{
		this.padding = this.padding.SetTop(_value);
	}

	[XuiXmlAttribute("padding_bottom", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributePaddingBottom(int _value)
	{
		this.padding = this.padding.SetBottom(_value);
	}

	[XuiXmlAttribute("padding", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributePadding(string _value)
	{
		XUiSideSizes.TryParse(_value, out this.padding, "padding");
	}

	[XuiXmlAttribute("sound", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeSound(string _value)
	{
		this.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
		{
			this.xuiSound = _o;
		});
	}

	[XuiXmlAttribute("sound_play_on_hover", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeSoundOnHover(string _value)
	{
		this.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
		{
			this.xuiHoverSound = _o;
		});
	}

	[XuiXmlAttribute("sound_play_on_press", false)]
	public bool SoundPlayOnClick { get; set; }

	public bool SoundPlayOnHover { get; set; }

	[XuiXmlAttribute("sound_play_on_open", false)]
	public bool SoundPlayOnOpen { get; set; }

	[XuiXmlAttribute("sound_volume", false)]
	public float SoundVolume { get; set; }

	public bool HasHoverSound
	{
		get
		{
			return this.xuiHoverSound != null;
		}
	}

	[XuiXmlAttribute("hold_delay", false)]
	public float HoldDelay
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 0.5f;

	[XuiXmlAttribute("hold_timed_initial_interval", false)]
	public float HoldEventIntervalInitial
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 0.5f;

	[XuiXmlAttribute("hold_timed_final_interval", false)]
	public float HoldEventIntervalFinal
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 0.06f;

	[XuiXmlAttribute("hold_timed_step_acceleration", false)]
	public float HoldEventIntervalAcceleration
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 0.015f;

	[XuiXmlAttribute("on_hover", false)]
	public bool EventOnHover
	{
		get
		{
			return this.eventOnHover;
		}
		set
		{
			if (this.eventOnHover == value)
			{
				return;
			}
			this.eventOnHover = value;
			this.SetDirty();
		}
	}

	[XuiXmlAttribute("on_press", false)]
	public bool EventOnPress
	{
		get
		{
			return this.eventOnPress;
		}
		set
		{
			if (this.eventOnPress == value)
			{
				return;
			}
			this.eventOnPress = value;
			this.SetDirty();
		}
	}

	[XuiXmlAttribute("on_doubleclick", false)]
	public bool EventOnDoubleClick
	{
		get
		{
			return this.eventOnDoubleClick;
		}
		set
		{
			if (this.eventOnDoubleClick == value)
			{
				return;
			}
			this.eventOnDoubleClick = value;
			this.SetDirty();
		}
	}

	[XuiXmlAttribute("on_held", false)]
	public bool EventOnHeld
	{
		get
		{
			return this.eventOnHeld;
		}
		set
		{
			if (this.eventOnHeld == value)
			{
				return;
			}
			this.eventOnHeld = value;
			this.SetDirty();
		}
	}

	[XuiXmlAttribute("on_scroll", false)]
	public bool EventOnScroll
	{
		get
		{
			return this.eventOnScroll;
		}
		set
		{
			if (this.eventOnScroll == value)
			{
				return;
			}
			this.eventOnScroll = value;
			this.SetDirty();
		}
	}

	[XuiXmlAttribute("on_drag", false)]
	public bool EventOnDrag
	{
		get
		{
			return this.eventOnDrag;
		}
		set
		{
			if (this.eventOnDrag == value)
			{
				return;
			}
			this.eventOnDrag = value;
			this.SetDirty();
		}
	}

	[XuiXmlAttribute("on_select", false)]
	public bool EventOnSelect
	{
		get
		{
			return this.eventOnSelect;
		}
		set
		{
			if (this.eventOnSelect == value)
			{
				return;
			}
			this.eventOnSelect = value;
			this.SetDirty();
		}
	}

	public virtual bool HasAnyEvent
	{
		get
		{
			return this.EventOnPress || this.EventOnDoubleClick || this.EventOnHover || this.EventOnHeld || this.EventOnDrag || this.EventOnScroll || this.EventOnSelect || !string.IsNullOrEmpty(this.ToolTip);
		}
	}

	public bool IsHovered
	{
		get
		{
			return this.isOver;
		}
	}

	public bool IsNavigatable
	{
		get
		{
			return this.isNavigatable && this.Enabled && this.IsVisible && this.IsActiveInHierarchy;
		}
		set
		{
			this.isNavigatable = value;
		}
	}

	[XuiXmlAttribute("gamepad_selectable", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeGamepadSelectable(bool _value)
	{
		this.IsNavigatable = _value;
		this.gamepadSelectableSetFromAttributes = true;
	}

	public bool IsSnappable { get; set; } = true;

	[XuiXmlAttribute("snap", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeGamepadSnappable(bool _value)
	{
		this.IsSnappable = _value;
		this.gamepadSnappableSetFromAttributes = true;
	}

	[XuiXmlAttribute("use_selection_box", false)]
	public bool UseSelectionBox { get; set; } = true;

	[XuiXmlAttribute("nav_up", false)]
	public string NavUpTargetString
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.navUpTargetString;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.navUpTargetString == value)
			{
				return;
			}
			this.navUpTargetString = value;
			this.SetDirty();
		}
	}

	public XUiView NavUpTarget
	{
		get
		{
			return this.navUpTarget;
		}
		set
		{
			this.navUpTarget = value;
			foreach (XUiController xuiController in this.Controller.Children)
			{
				xuiController.ViewComponent.NavUpTarget = value;
			}
		}
	}

	[XuiXmlAttribute("nav_down", false)]
	public string NavDownTargetString
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.navDownTargetString;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.navDownTargetString == value)
			{
				return;
			}
			this.navDownTargetString = value;
			this.SetDirty();
		}
	}

	public XUiView NavDownTarget
	{
		get
		{
			return this.navDownTarget;
		}
		set
		{
			this.navDownTarget = value;
			foreach (XUiController xuiController in this.Controller.Children)
			{
				xuiController.ViewComponent.navDownTarget = value;
			}
		}
	}

	[XuiXmlAttribute("nav_left", false)]
	public string NavLeftTargetString
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.navLeftTargetString;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.navLeftTargetString == value)
			{
				return;
			}
			this.navLeftTargetString = value;
			this.SetDirty();
		}
	}

	public XUiView NavLeftTarget
	{
		get
		{
			return this.navLeftTarget;
		}
		set
		{
			this.navLeftTarget = value;
			foreach (XUiController xuiController in this.Controller.Children)
			{
				xuiController.ViewComponent.navLeftTarget = value;
			}
		}
	}

	[XuiXmlAttribute("nav_right", false)]
	public string NavRightTargetString
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.navRightTargetString;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.navRightTargetString == value)
			{
				return;
			}
			this.navRightTargetString = value;
			this.SetDirty();
		}
	}

	public XUiView NavRightTarget
	{
		get
		{
			return this.navRightTarget;
		}
		set
		{
			this.navRightTarget = value;
			foreach (XUiController xuiController in this.Controller.Children)
			{
				xuiController.ViewComponent.navRightTarget = value;
			}
		}
	}

	[XuiXmlAttribute("tooltip", false)]
	public string ToolTip
	{
		get
		{
			return this.toolTip;
		}
		set
		{
			if (this.toolTip == value)
			{
				return;
			}
			if (GameManager.Instance.GameIsFocused && this.enabled && this.isOver && this.xui.ToolTipWindow != null && this.xui.ToolTipWindow.ToolTip == this.toolTip)
			{
				this.xui.ToolTipWindow.ToolTip = value;
			}
			this.toolTip = value;
		}
	}

	[XuiXmlAttribute("disabled_tooltip", false)]
	public string DisabledToolTip
	{
		get
		{
			return this.disabledToolTip;
		}
		set
		{
			if (this.disabledToolTip == value)
			{
				return;
			}
			if (GameManager.Instance.GameIsFocused && this.enabled && this.isOver && this.xui.ToolTipWindow != null && this.xui.ToolTipWindow.ToolTip == this.disabledToolTip)
			{
				this.xui.ToolTipWindow.ToolTip = value;
			}
			this.disabledToolTip = value;
		}
	}

	[XuiXmlAttribute("tooltip_key", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeTooltipKey(string _value)
	{
		this.ToolTip = Localization.Get(_value, false, null);
	}

	[XuiXmlAttribute("disabled_tooltip_key", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeDisabledTooltipKey(string _value)
	{
		this.DisabledToolTip = Localization.Get(_value, false, null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiView(XUi _xui, string _id)
	{
		this.xui = _xui;
		this.id = _id;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void createComponents(GameObject _go)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void buildView()
	{
		Type type = base.GetType();
		GameObject gameObject;
		if (XUiView.componentTemplates.TryGetValue(type, out gameObject))
		{
			this.uiTransform = Object.Instantiate<GameObject>(gameObject).transform;
			return;
		}
		if (XUiView.templatesParent == null)
		{
			Transform transform = this.xui.playerUI.uiCamera.transform;
			GameObject gameObject2 = new GameObject("_ViewTemplates");
			gameObject2.SetActive(false);
			XUiView.templatesParent = gameObject2.transform;
			XUiView.templatesParent.parent = transform;
		}
		GameObject gameObject3 = new GameObject(type.Name)
		{
			layer = 12,
			transform = 
			{
				parent = XUiView.templatesParent
			}
		};
		gameObject3.AddComponent<BoxCollider>().enabled = false;
		UIEventListener.Get(gameObject3);
		this.createComponents(gameObject3);
		XUiView.componentTemplates[type] = gameObject3;
		this.uiTransform = Object.Instantiate<GameObject>(gameObject3).transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void captureComponents()
	{
		this.collider = this.uiTransform.gameObject.GetComponent<BoxCollider>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void addTweeners()
	{
		for (int i = 0; i < this.Tweeners.Count; i++)
		{
			this.Tweeners[i].CreateTween(this.uiTransform.gameObject);
		}
	}

	public virtual void InitView()
	{
		this.buildView();
		this.uiTransform.name = this.id;
		this.captureComponents();
		this.addTweeners();
		XUiController parent = this.controller.Parent;
		if (((parent != null) ? parent.ViewComponent : null) != null)
		{
			XUiView viewComponent = this.controller.Parent.ViewComponent;
			this.uiTransform.parent = viewComponent.uiTransform;
			this.uiTransform.localScale = Vector3.one;
			this.uiTransform.localPosition = new Vector3((float)this.PaddedPosition.x, (float)this.PaddedPosition.y, 0f);
			this.uiTransform.localEulerAngles = new Vector3(0f, 0f, this.rotation);
		}
		if (this.HasAnyEvent)
		{
			this.collider.enabled = true;
			this.refreshBoxCollider();
			UIEventListener uieventListener = UIEventListener.Get(this.uiTransform.gameObject);
			uieventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uieventListener.onHover, new UIEventListener.BoolDelegate(delegate(GameObject _, bool _state)
			{
				this.OnHover(_state);
			}));
			uieventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener.onClick, new UIEventListener.VoidDelegate(this.OnClick));
			uieventListener.onDoubleClick = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener.onDoubleClick, new UIEventListener.VoidDelegate(this.OnDoubleClick));
			uieventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uieventListener.onDrag, new UIEventListener.VectorDelegate(this.OnDrag));
			uieventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uieventListener.onPress, new UIEventListener.BoolDelegate(this.OnPress));
			uieventListener.onScroll = (UIEventListener.FloatDelegate)Delegate.Combine(uieventListener.onScroll, new UIEventListener.FloatDelegate(this.OnScroll));
			uieventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uieventListener.onSelect, new UIEventListener.BoolDelegate(this.OnSelect));
			uieventListener.onDragOut = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener.onDragOut, new UIEventListener.VoidDelegate(this.OnHeldDragOut));
		}
		if (this.uiTransform.gameObject.activeSelf != this.isVisible)
		{
			this.uiTransform.gameObject.SetActive(this.isVisible);
		}
		if (!this.gamepadSelectableSetFromAttributes)
		{
			this.IsNavigatable = this.EventOnPress;
		}
		if (!this.gamepadSnappableSetFromAttributes)
		{
			this.IsSnappable = this.EventOnPress;
		}
	}

	public virtual void SetRepeatContentTemplateParams(Dictionary<string, object> _templateParams, int _curRepeatNum)
	{
	}

	public virtual void Cleanup()
	{
		this.destroyed = true;
	}

	public virtual void Update(float _dt)
	{
		if (this.isOver && !this.UiTransformIsHovered)
		{
			using (this.pmUpdateHover.Auto())
			{
				this.OnHover(false);
			}
		}
		if (this.isPressed && this.enabled)
		{
			using (this.pmUpdatePress.Auto())
			{
				float unscaledTime = Time.unscaledTime;
				if (!this.isHold)
				{
					this.isHold = unscaledTime - this.pressStartTime >= this.HoldDelay;
					if (this.isHold)
					{
						this.holdStartTime = unscaledTime;
						this.controller.Held(EHoldType.HoldStart, 0f, -1f);
						this.holdEventNextTime = 0f;
						this.holdEventLastTime = unscaledTime;
						this.holdEventIntervalChangeSpeed = 0f;
						this.holdEventIntervalCurrent = this.HoldEventIntervalInitial;
					}
				}
				else
				{
					this.controller.Held(EHoldType.Hold, unscaledTime - this.holdStartTime, -1f);
				}
				if (this.isHold && unscaledTime >= this.holdEventNextTime)
				{
					this.holdEventIntervalCurrent = Mathf.SmoothDamp(this.holdEventIntervalCurrent, this.HoldEventIntervalFinal, ref this.holdEventIntervalChangeSpeed, this.HoldEventIntervalAcceleration, float.PositiveInfinity, _dt);
					this.holdEventNextTime = unscaledTime + this.holdEventIntervalCurrent;
					this.controller.Held(EHoldType.HoldTimed, unscaledTime - this.holdStartTime, unscaledTime - this.holdEventLastTime);
					this.holdEventLastTime = unscaledTime;
				}
			}
		}
		if (this.isDirty)
		{
			using (this.pmUpdateData.Auto())
			{
				this.updateData();
				this.isDirty = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateData()
	{
		if (this.positionDirty)
		{
			this.uiTransform.localPosition = new Vector3((float)this.PaddedPosition.x, (float)this.PaddedPosition.y, this.uiTransform.localPosition.z);
			this.positionDirty = false;
		}
		if (this.rotationDirty)
		{
			this.uiTransform.localEulerAngles = new Vector3(0f, 0f, this.rotation);
			this.rotationDirty = false;
		}
		this.parseAnchors();
		this.parseNavigationTargets();
	}

	public void TryUpdatePosition()
	{
		if (this.positionDirty)
		{
			this.uiTransform.localPosition = new Vector3((float)this.PaddedPosition.x, (float)this.PaddedPosition.y, this.uiTransform.localPosition.z);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void refreshBoxCollider()
	{
		this.collider.center = new Vector3((float)this.size.x * 0.5f, 0f - (float)this.size.y * 0.5f, 0f);
		this.collider.size = new Vector3((float)this.size.x * this.ColliderScale + (float)(2 * this.ColliderPadding.x), (float)this.size.y * this.ColliderScale + (float)(2 * this.ColliderPadding.y), 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetDirty()
	{
		this.isDirty = true;
	}

	[XuiXmlAttribute("anchor_left", false)]
	public string AnchorLeft
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.anchorLeft;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.anchorLeft == value)
			{
				return;
			}
			this.anchorLeft = value;
			this.SetDirty();
		}
	}

	[XuiXmlAttribute("anchor_right", false)]
	public string AnchorRight
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.anchorRight;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.anchorRight == value)
			{
				return;
			}
			this.anchorRight = value;
			this.SetDirty();
		}
	}

	[XuiXmlAttribute("anchor_bottom", false)]
	public string AnchorBottom
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.anchorBottom;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.anchorBottom == value)
			{
				return;
			}
			this.anchorBottom = value;
			this.SetDirty();
		}
	}

	[XuiXmlAttribute("anchor_top", false)]
	public string AnchorTop
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.anchorTop;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.anchorTop == value)
			{
				return;
			}
			this.anchorTop = value;
			this.SetDirty();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void parseAnchors()
	{
		UIRect uiRect = this.UiRect;
		if (uiRect == null)
		{
			return;
		}
		if (this.<parseAnchors>g__ParseAnchorString|269_0(this.anchorLeft, ref this.anchorLeftParsed, uiRect, uiRect.leftAnchor) | this.<parseAnchors>g__ParseAnchorString|269_0(this.anchorRight, ref this.anchorRightParsed, uiRect, uiRect.rightAnchor) | this.<parseAnchors>g__ParseAnchorString|269_0(this.anchorBottom, ref this.anchorBottomParsed, uiRect, uiRect.bottomAnchor) | this.<parseAnchors>g__ParseAnchorString|269_0(this.anchorTop, ref this.anchorTopParsed, uiRect, uiRect.topAnchor))
		{
			this.anchoredLeftAndRight = uiRect.leftAnchor.target && uiRect.rightAnchor.target;
			this.anchoredTopAndBottom = uiRect.bottomAnchor.target && uiRect.topAnchor.target;
			this.SetDirty();
			uiRect.ResetAnchors();
		}
		this.anchorsParsed();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void anchorsParsed()
	{
	}

	public virtual void OnOpen()
	{
		if (this.xuiSound != null && this.SoundPlayOnOpen)
		{
			Manager.PlayXUiSound(this.xuiSound, this.SoundVolume);
		}
		this.isPressed = false;
		this.isHold = false;
	}

	public virtual void OnClose()
	{
		this.isPressed = false;
		this.isHold = false;
		if (!GameManager.Instance.IsQuitting)
		{
			CursorControllerAbs cursorController = this.xui.playerUI.CursorController;
			if (cursorController.HoverTarget == this)
			{
				cursorController.HoverTarget = null;
			}
			if (cursorController.navigationTarget == this)
			{
				this.controller.Hovered(false);
				cursorController.SetNavigationTarget(null);
			}
			if (cursorController.lockNavigationToView == this)
			{
				cursorController.SetNavigationLockView(null, null);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScroll(GameObject _go, float _delta)
	{
		if (this.EventOnScroll)
		{
			this.controller.Scrolled(_delta);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSelect(GameObject _go, bool _selected)
	{
		if (this.EventOnSelect && this.enabled)
		{
			this.controller.Selected(_selected);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDrag(GameObject _go, Vector2 _delta)
	{
		if (this.EventOnDrag && this.enabled)
		{
			EDragType edragType = (this.wasDragging ? EDragType.Dragging : EDragType.DragStart);
			this.wasDragging = true;
			this.controller.Dragged(_delta, edragType);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPress(GameObject _go, bool _pressed)
	{
		if (this.EventOnPress && this.enabled)
		{
			this.controller.MouseUpDown(_pressed);
		}
		if (this.EventOnDrag && this.enabled && !_pressed && this.wasDragging)
		{
			this.wasDragging = false;
			this.controller.Dragged(default(Vector2), EDragType.DragEnd);
		}
		if (this.EventOnHeld)
		{
			if (_pressed && !this.isPressed && this.enabled)
			{
				this.isPressed = true;
				this.isHold = false;
				this.pressStartTime = Time.unscaledTime;
				this.holdStartTime = -1f;
				return;
			}
			if (!_pressed)
			{
				this.isPressed = false;
				bool flag = this.isHold;
				this.isHold = false;
				if (flag)
				{
					this.controller.Held(EHoldType.HoldEnd, Time.unscaledTime - this.holdStartTime, -1f);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHeldDragOut(GameObject _go)
	{
		if (this.EventOnHeld && this.isPressed)
		{
			this.isPressed = false;
			bool flag = this.isHold;
			this.isHold = false;
			if (flag)
			{
				this.controller.Held(EHoldType.HoldEnd, Time.unscaledTime - this.holdStartTime, -1f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClick(GameObject _go)
	{
		if (this.EventOnPress && this.enabled)
		{
			if (this.xuiSound != null && this.SoundPlayOnClick && UICamera.currentTouchID == -1)
			{
				Manager.PlayXUiSound(this.xuiSound, this.SoundVolume);
			}
			this.controller.Pressed(UICamera.currentTouchID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDoubleClick(GameObject _go)
	{
		if (this.EventOnDoubleClick && this.enabled)
		{
			if (this.xuiSound != null && this.SoundPlayOnClick && UICamera.currentTouchID == -1)
			{
				Manager.PlayXUiSound(this.xuiSound, this.SoundVolume);
			}
			this.controller.DoubleClicked(UICamera.currentTouchID);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnHover(bool _isOver)
	{
		PlayerActionsLocal playerInput = this.xui.playerUI.playerInput;
		if (playerInput != null && playerInput.LastDeviceClass == 1 && !Cursor.visible)
		{
			_isOver = false;
		}
		bool flag = _isOver && !this.enabled && !string.IsNullOrEmpty(this.DisabledToolTip);
		_isOver &= this.enabled;
		bool flag2 = _isOver && !string.IsNullOrEmpty(this.ToolTip);
		if (_isOver != this.isOver && _isOver)
		{
			this.PlayHoverSound();
		}
		this.isOver = _isOver;
		if (this.EventOnHover)
		{
			this.controller.Hovered(_isOver);
		}
		if (this.xui.ToolTipWindow != null)
		{
			if (flag2)
			{
				this.xui.ToolTipWindow.ToolTip = this.ToolTip;
			}
			else if (flag)
			{
				this.xui.ToolTipWindow.ToolTip = this.DisabledToolTip;
			}
			else
			{
				this.xui.ToolTipWindow.ToolTip = "";
			}
		}
		this.xui.playerUI.CursorController.HoverTarget = (_isOver ? this : null);
	}

	public void PlayHoverSound()
	{
		if (this.xuiHoverSound != null && this.SoundPlayOnHover && this.enabled && GameManager.Instance.GameIsFocused)
		{
			Manager.PlayXUiSound(this.xuiHoverSound, this.SoundVolume);
		}
	}

	public void PlayClickSound()
	{
		if (this.xuiSound != null)
		{
			Manager.PlayXUiSound(this.xuiSound, this.SoundVolume);
		}
	}

	public Rect GetXUiRect()
	{
		return this.GetXUiRect(Vector2.zero);
	}

	public Rect GetXUiRect(Vector2 _padding)
	{
		XUiController parent = this.controller;
		Vector3 vector = this.uiTransform.localPosition;
		vector.x -= _padding.x;
		vector.y += _padding.y;
		while (parent.Parent != null && parent.Parent.ViewComponent != null)
		{
			parent = parent.Parent;
			vector += parent.ViewComponent.uiTransform.localPosition;
		}
		vector += parent.ViewComponent.uiTransform.parent.localPosition;
		Vector2 vector2 = this.Size.AsVector2() + 2f * _padding;
		XUiV_Window xuiV_Window = parent.ViewComponent as XUiV_Window;
		if (xuiV_Window != null && xuiV_Window.IsInStackPanel)
		{
			Transform parent2 = xuiV_Window.uiTransform.parent.parent;
			vector *= parent2.localScale.x;
			vector2 *= parent2.localScale.x;
			vector += parent2.localPosition;
		}
		return new Rect(new Vector2(vector.x, vector.y - vector2.y), vector2);
	}

	public string GetXuiHierarchy()
	{
		return XUiUtils.GetXuiHierarchy(this.Controller);
	}

	public Vector2 GetClosestPoint(Vector3 _point)
	{
		return this.collider.ClosestPointOnBounds(_point);
	}

	public void ClearNavigationTargets()
	{
		this.NavUpTarget = (this.NavDownTarget = (this.NavLeftTarget = (this.NavRightTarget = null)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void parseNavigationTargets()
	{
		if (!string.IsNullOrEmpty(this.navUpTargetString))
		{
			this.NavUpTarget = this.<parseNavigationTargets>g__FindView|298_0(this.navUpTargetString);
		}
		if (!string.IsNullOrEmpty(this.navDownTargetString))
		{
			this.NavDownTarget = this.<parseNavigationTargets>g__FindView|298_0(this.navDownTargetString);
		}
		if (!string.IsNullOrEmpty(this.navLeftTargetString))
		{
			this.NavLeftTarget = this.<parseNavigationTargets>g__FindView|298_0(this.navLeftTargetString);
		}
		if (!string.IsNullOrEmpty(this.navRightTargetString))
		{
			this.NavRightTarget = this.<parseNavigationTargets>g__FindView|298_0(this.navRightTargetString);
		}
	}

	public virtual void SetDefaults(XUiController _parent)
	{
		this.RepeatContent = false;
		this.RepeatCount = 1;
		this.Size = Vector2i.min;
		int? num;
		if (_parent == null)
		{
			num = null;
		}
		else
		{
			XUiView viewComponent = _parent.ViewComponent;
			num = ((viewComponent != null) ? new int?(viewComponent.Depth) : null);
		}
		int? num2 = num;
		this.Depth = num2.GetValueOrDefault();
		this.ToolTip = "";
		this.SoundPlayOnClick = true;
		this.SoundPlayOnOpen = false;
		this.SoundPlayOnHover = true;
		this.SoundVolume = 1f;
	}

	public virtual void SetPostParsingDefaults(XUiController _parent)
	{
		XUiView xuiView = ((_parent != null) ? _parent.ViewComponent : null);
		Vector2i vector2i = this.Size;
		if (vector2i.x == -2147483648)
		{
			vector2i.x = (this.ignoreParentPadding ? ((xuiView != null) ? xuiView.Size.x : 0) : ((xuiView != null) ? xuiView.InnerSize.x : 0));
		}
		if (vector2i.y == -2147483648)
		{
			vector2i.y = (this.ignoreParentPadding ? ((xuiView != null) ? xuiView.Size.y : 0) : ((xuiView != null) ? xuiView.InnerSize.y : 0));
		}
		this.Size = vector2i;
	}

	public void ParseInitialAttributeValue(string _attribute, string _value)
	{
		if (_value.Contains("{"))
		{
			BindingsManager.CreateBinding(this, _attribute, _value);
			return;
		}
		if (ParsingMethodCache.Instance.TryParseDirect(this, _attribute, _value))
		{
			return;
		}
		if (this.Controller != null && ParsingMethodCache.Instance.TryParseDirect(this.Controller, _attribute, _value))
		{
			return;
		}
		this.ParseAttributeViewAndController(_attribute, _value);
	}

	public bool ParseAttributeViewAndController(string _attribute, string _value)
	{
		if (_value.Contains("{") && XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
		{
			Log.Warning(string.Concat(new string[] { "[XUi] Refreshed binding contained '{': ", _attribute, "='", _value, "' on ", this.id }));
		}
		if (this.Controller != null)
		{
			if (!this.Controller.ParseAttribute(_attribute, _value))
			{
				this.Controller.CustomAttributes[_attribute] = _value;
			}
			return true;
		}
		return false;
	}

	public virtual void OnVisibilityChanged(bool _visibleInScene)
	{
		if (this.isOver && !_visibleInScene)
		{
			this.OnHover(false);
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <parseAnchors>g__ParseAnchorString|269_0(string _anchorString, ref string _parsedString, UIRect _uiRect, UIRect.AnchorPoint _anchor)
	{
		if (string.IsNullOrEmpty(_anchorString))
		{
			return false;
		}
		if (_anchorString == _parsedString && _anchor.target != null)
		{
			return false;
		}
		_parsedString = _anchorString;
		int num = _anchorString.IndexOf(',');
		if (num < 0)
		{
			throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
		}
		string text = _anchorString.Substring(0, num);
		int num2 = _anchorString.IndexOf(',', num + 1);
		if (num2 < 0)
		{
			throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
		}
		float num3 = StringParsers.ParseFloat(_anchorString, num + 1, num2 - 1, NumberStyles.Any);
		int num4 = StringParsers.ParseSInt32(_anchorString, num2 + 1, -1, NumberStyles.Integer);
		if (text.Length == 0)
		{
			throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
		}
		if (text.EqualsCaseInsensitive("#parent"))
		{
			_anchor.target = this.uiTransform.parent;
		}
		else if (text.EqualsCaseInsensitive("#cam"))
		{
			UICamera componentInParent = this.uiTransform.gameObject.GetComponentInParent<UICamera>();
			if (componentInParent == null)
			{
				throw new Exception("UICamera not found");
			}
			_anchor.target = componentInParent.transform;
		}
		else if (text[0] == '#')
		{
			string text2 = text.Substring(1);
			UIAnchor.Side side;
			if (!EnumUtils.TryParse<UIAnchor.Side>(text2, out side, true))
			{
				throw new ArgumentException("Invalid anchor side name '" + text2 + "', expected any of '\tBottomLeft,Left,TopLeft,Top,TopRight,Right,BottomRight,Bottom,Center'");
			}
			_anchor.target = this.xui.GetAnchor(side).transform;
		}
		else
		{
			XUiView xuiView = XUiUtils.FindHierarchyClosestView(this.Controller, text);
			if (xuiView == null)
			{
				throw new ArgumentException(string.Concat(new string[]
				{
					"Invalid anchor string '",
					_anchorString,
					"', view component with name '",
					text,
					"' not found.\nOn: ",
					this.GetXuiHierarchy()
				}));
			}
			_anchor.target = xuiView.UiTransform;
			XUiV_Grid xuiV_Grid = xuiView as XUiV_Grid;
			if (xuiV_Grid != null)
			{
				xuiV_Grid.OnSizeChanged += delegate(Vector2Int _, Vector2 _)
				{
					_uiRect.UpdateAnchors();
				};
			}
		}
		_anchor.relative = num3;
		_anchor.absolute = num4;
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView <parseNavigationTargets>g__FindView|298_0(string _name)
	{
		if (string.IsNullOrEmpty(_name))
		{
			return null;
		}
		XUiView xuiView = XUiUtils.FindHierarchyClosestView(this.Controller, _name);
		if (xuiView != null)
		{
			return xuiView;
		}
		XUiController childById = this.controller.WindowGroup.Controller.GetChildById(_name);
		xuiView = ((childById != null) ? childById.ViewComponent : null);
		if (xuiView != null)
		{
			return xuiView;
		}
		Log.Error("Invalid navigation target, view component with name '" + _name + "' not found.\nOn: " + this.GetXuiHierarchy());
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<Type, GameObject> componentTemplates = new Dictionary<Type, GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform templatesParent;

	public readonly XUi xui;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool destroyed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController controller;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform uiTransform;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BoxCollider collider;

	public readonly List<XUiTweenAbs> Tweeners = new List<XUiTweenAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string id;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isVisible = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool enabled = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2i size;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2i position;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool positionDirty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ignoreParentPadding;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiSideSizes padding;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float rotation;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool rotationDirty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int depth;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip xuiSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip xuiHoverSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnHover;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnPress;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnDoubleClick;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnHeld;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnScroll;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnDrag;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool eventOnSelect;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isNavigatable = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool gamepadSelectableSetFromAttributes;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool gamepadSnappableSetFromAttributes;

	[PublicizedFrom(EAccessModifier.Private)]
	public string navUpTargetString;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView navUpTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public string navDownTargetString;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView navDownTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public string navLeftTargetString;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView navLeftTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public string navRightTargetString;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView navRightTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public string toolTip;

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledToolTip;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmUpdateHover = new ProfilerMarker("XV.U-Hover");

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmUpdatePress = new ProfilerMarker("XV.U-Press");

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmUpdateData = new ProfilerMarker("XV.U-UpdateData");

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorBottom;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorTop;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorLeftParsed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorRightParsed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorBottomParsed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string anchorTopParsed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool anchoredLeftAndRight;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool anchoredTopAndBottom;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasDragging;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHold;

	[PublicizedFrom(EAccessModifier.Private)]
	public float pressStartTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdStartTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventIntervalCurrent;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventIntervalChangeSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventNextTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventLastTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isOver;
}
