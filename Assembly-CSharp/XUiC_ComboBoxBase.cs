using System;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_ComboBoxBase : XUiController
{
	public event XUiC_ComboBoxBase.XUiEvent_GenericValueChanged OnValueChangedGeneric;

	public event XUiC_ComboBoxBase.XUiEvent_HoveredStateChanged OnHoveredStateChanged;

	[XuiXmlAttribute("combo_enabled", false)]
	[XuiXmlBinding("combo_enabled")]
	public bool Enabled
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
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("show_buttons")]
	public bool ShowButtons
	{
		get
		{
			return this.showButtons;
		}
		set
		{
			if (value == this.showButtons)
			{
				return;
			}
			this.showButtons = value;
			this.IsDirty = true;
		}
	}

	public Color TextColor
	{
		get
		{
			return this.ColorEnabled;
		}
		set
		{
			if (this.ColorEnabled == value)
			{
				return;
			}
			this.ColorEnabled = value;
			this.IsDirty = true;
		}
	}

	public abstract long ValueGeneric { get; set; }

	public abstract long ValueMinGeneric { get; set; }

	public abstract long ValueMaxGeneric { get; set; }

	[XuiXmlAttribute("scroll_by_increment", false)]
	public bool ScrollByIncrement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[XuiXmlAttribute("enabled_color", false)]
	public Color ColorEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.colorEnabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.colorEnabled == value)
			{
				return;
			}
			this.colorEnabled = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("disabled_color", false)]
	public Color ColorDisabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.colorDisabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.colorDisabled == value)
			{
				return;
			}
			this.colorDisabled = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("bg_color", false)]
	[XuiXmlBinding("bgcolor")]
	public Color ColorBg
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.colorBg;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.colorBg == value)
			{
				return;
			}
			this.colorBg = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("enabled_fill_color", false)]
	public Color ColorFillEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.colorFillEnabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.colorFillEnabled == value)
			{
				return;
			}
			this.colorFillEnabled = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("disabled_fill_color", false)]
	public Color ColorFillDisabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.colorFillDisabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.colorFillDisabled == value)
			{
				return;
			}
			this.colorFillDisabled = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("index_markers", false)]
	public bool IndexMarkers
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public abstract int IndexElementCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public abstract int IndexMarkerIndex
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	[XuiXmlAttribute("index_marker_spacing", false)]
	public int IndexMarkerSpacing
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
		[PublicizedFrom(EAccessModifier.Protected)]
		set;
	} = 5;

	[XuiXmlBinding("usesmarkers")]
	public virtual bool UsesIndexMarkers
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.IndexMarkers && this.sprFill != null && this.IndexElementCount > 0 && this.IndexElementCount < this.indexMarkerSprites.Length;
		}
	}

	[XuiXmlAttribute("gamepad_default_hover_control", false)]
	public bool GamepadDefaultHoverControl
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("gamepad_decrease", false)]
	public PlayerAction GamepadDecreaseShortcut
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("gamepad_increase", false)]
	public PlayerAction GamepadIncreaseShortcut
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("gamepad_shortcuts_hoveronly", false)]
	public bool GamepadShortcutsHoverOnly
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("valuetext")]
	public string ValueText
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.valueTextOverride ?? this.valueText;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (this.valueText == value)
			{
				return;
			}
			this.valueText = value;
			this.IsDirty = true;
		}
	}

	public string ValueTextOverride
	{
		get
		{
			return this.valueTextOverride;
		}
		set
		{
			this.valueTextOverride = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("segment_count", false)]
	[XuiXmlBinding("segmentcount")]
	public int SegmentedFillCount
	{
		get
		{
			return Mathf.Max(this.segmentedFillCount, 1);
		}
		set
		{
			if (value == this.segmentedFillCount)
			{
				return;
			}
			this.segmentedFillCount = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("cbx_is_hovered")]
	public bool IsOver
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.isOver;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.isOver == value)
			{
				return;
			}
			this.isOver = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("fillvalue")]
	public abstract double RelativeValue { get; set; }

	[XuiXmlBinding("valuecolor")]
	public Color ValueColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!this.Enabled)
			{
				return this.ColorDisabled;
			}
			return this.ColorEnabled;
		}
	}

	[XuiXmlBinding("isnumber")]
	public virtual bool IsNumber
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return false;
		}
	}

	[XuiXmlBinding("fillcolor")]
	public Color FillColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!this.Enabled)
			{
				return this.ColorFillDisabled;
			}
			return this.ColorFillEnabled;
		}
	}

	[XuiXmlBinding("hascontrollershortcuts")]
	public bool HasControllerShortcuts
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.GamepadIncreaseShortcut != null && this.GamepadDecreaseShortcut != null;
		}
	}

	[XuiXmlBinding("can_backward")]
	public bool CanBackward
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.Enabled && !this.isEmpty() && (this.Wrap || !this.isMin());
		}
	}

	[XuiXmlBinding("can_forward")]
	public bool CanForward
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.Enabled && !this.isEmpty() && (this.Wrap || !this.isMax());
		}
	}

	public override void Init()
	{
		base.Init();
		base.OnVisiblity += delegate(XUiController _, bool _, bool _visibleInScene)
		{
			if (!_visibleInScene)
			{
				XUiC_Paging.IsOverPagingOverrideElement = false;
			}
		};
		XUiController childById = base.GetChildById("indexMarkers");
		if (childById != null)
		{
			object obj;
			if (childById.CustomAttributes.TryGetValue("active_color", out obj))
			{
				this.colorIndexMarkerActive = StringParsers.ParseColor32(obj.ToString());
			}
			object obj2;
			if (childById.CustomAttributes.TryGetValue("inactive_color", out obj2))
			{
				this.colorIndexMarkerInactive = StringParsers.ParseColor32(obj2.ToString());
			}
			object obj3;
			if (childById.CustomAttributes.TryGetValue("disabled_active_color", out obj3))
			{
				this.colorIndexMarkerDisabledActive = StringParsers.ParseColor32(obj3.ToString());
			}
		}
		if (string.IsNullOrEmpty(this.viewComponent.ToolTip) && base.Parent != null)
		{
			foreach (XUiController xuiController in base.Parent.Children)
			{
				XUiV_Label xuiV_Label = xuiController.ViewComponent as XUiV_Label;
				if (xuiV_Label != null)
				{
					this.viewComponent.ToolTip = xuiV_Label.ToolTip;
					break;
				}
			}
		}
	}

	public override void OnOpen()
	{
		this.IsDirty = true;
		base.OnOpen();
		this.UpdateLabel();
		this.UpdateIndexMarkerPositions();
		this.UpdateIndexMarkerStates();
		base.RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.isOver)
		{
			XUiC_GamepadCalloutWindow calloutWindow = this.xui.CalloutWindow;
			if (calloutWindow == null)
			{
				return;
			}
			calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuComboBox);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.UsesIndexMarkers && this.indexMarkersSetupBefore != this.IndexElementCount)
		{
			this.UpdateIndexMarkerPositions();
		}
		if (this.Enabled)
		{
			bool flag = this.clickable != null && this.xui.playerUI.CursorController.CurrentTarget == this.clickable.ViewComponent && !this.xui.playerUI.CursorController.Locked && XUiUtils.HotkeysAllowedFor(this.viewComponent, false);
			if (flag && this.GamepadDefaultHoverControl)
			{
				XUi.HandlePaging(this.xui, new Func<bool>(this.PageUpAction), new Func<bool>(this.PageDownAction), false);
			}
			if (this.GamepadDecreaseShortcut != null && this.viewComponent.IsActiveInHierarchy && this.GamepadDecreaseShortcut.WasPressed && (!this.GamepadShortcutsHoverOnly || flag))
			{
				this.PageDownAction();
			}
			if (this.GamepadIncreaseShortcut != null && this.viewComponent.IsActiveInHierarchy && this.GamepadIncreaseShortcut.WasPressed && (!this.GamepadShortcutsHoverOnly || flag))
			{
				this.PageUpAction();
			}
		}
		if (base.handleDirtyUpdateDefault())
		{
			this.UpdateIndexMarkerStates();
		}
	}

	[XuiXmlAttribute("value_wrap", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueWrap(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			this.Wrap = StringParsers.ParseBool(_value, 0, -1, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void UpdateLabel();

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateIndexMarkerStates()
	{
		if (!this.UsesIndexMarkers)
		{
			return;
		}
		for (int i = 0; i < this.indexMarkerSprites.Length; i++)
		{
			this.indexMarkerSprites[i].Color = this.colorIndexMarkerInactive;
		}
		if (this.IndexMarkerIndex >= 0 && this.IndexMarkerIndex < this.indexMarkerSprites.Length)
		{
			this.indexMarkerSprites[this.IndexMarkerIndex].Color = (this.Enabled ? this.colorIndexMarkerActive : this.colorIndexMarkerDisabledActive);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateIndexMarkerPositions()
	{
		if (!this.UsesIndexMarkers)
		{
			return;
		}
		this.indexMarkersSetupBefore = this.IndexElementCount;
		float num = (float)(this.sprFill.Size.x + this.IndexMarkerSpacing) / (float)this.IndexElementCount;
		int num2 = Mathf.RoundToInt(num - (float)this.IndexMarkerSpacing);
		for (int i = 0; i < this.indexMarkerSprites.Length; i++)
		{
			if (i >= this.IndexElementCount)
			{
				this.indexMarkerSprites[i].IsVisible = false;
			}
			else
			{
				Vector2i position = this.indexMarkerSprites[i].Position;
				position.x = Mathf.RoundToInt((float)i * num);
				this.indexMarkerSprites[i].Position = position;
				Vector2i size = this.indexMarkerSprites[i].Size;
				size.x = num2;
				if (i == this.IndexElementCount - 1)
				{
					size.x = this.sprFill.Size.x - position.x;
				}
				this.indexMarkerSprites[i].Size = size;
				this.indexMarkerSprites[i].IsVisible = true;
			}
		}
	}

	[XuiBindEvent("OnHover", "forwardButton")]
	[XuiBindEvent("OnHover", "backButton")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Button_OnHover(XUiController _sender, bool _isOver)
	{
		this.isOverAnyPart = _isOver;
		XUiC_ComboBoxBase.XUiEvent_HoveredStateChanged onHoveredStateChanged = this.OnHoveredStateChanged;
		if (onHoveredStateChanged == null)
		{
			return;
		}
		onHoveredStateChanged(this, this.isOver, this.isOverAnyPart);
	}

	[XuiBindEvent("OnPress", "backButton")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BackButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.TryPageDown();
	}

	[XuiBindEvent("OnPress", "forwardButton")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ForwardButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.TryPageUp();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool PageUpAction()
	{
		bool flag = this.TryPageUp();
		if (this.xui.playerUI.CursorController.CursorModeActive)
		{
			base.SelectCursorElement(false, true);
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool PageDownAction()
	{
		bool flag = this.TryPageDown();
		if (this.xui.playerUI.CursorController.CursorModeActive)
		{
			base.SelectCursorElement(false, true);
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void BackPressed();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void ForwardPressed();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool isMax();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool isMin();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool isEmpty();

	[XuiBindEvent("OnPress", "clickable")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void pressEvent(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			return;
		}
		if (!this.Enabled)
		{
			return;
		}
		ref Vector2i mouseXUiPosition = this.xui.GetMouseXUiPosition();
		Rect xuiRect = this.clickable.ViewComponent.GetXUiRect();
		float num = ((float)mouseXUiPosition.x - xuiRect.min.x) / xuiRect.width;
		this.setRelativeValue(num);
	}

	[XuiBindEvent("OnScroll", "clickable")]
	public void ScrollEvent(XUiController _sender, float _delta)
	{
		if (!InputUtils.ShiftKeyPressed)
		{
			return;
		}
		if (this.Enabled && !this.isEmpty())
		{
			if (this.ScrollByIncrement)
			{
				if (_delta > 0f)
				{
					this.PageUpAction();
					return;
				}
				this.PageDownAction();
				return;
			}
			else
			{
				this.incrementalChangeValue((double)_delta);
			}
		}
	}

	[XuiBindEvent("OnHover", "clickable")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void hoverEvent(XUiController _sender, bool _isOver)
	{
		this.IsOver = _isOver;
		this.isOverAnyPart = _isOver;
		XUiC_ComboBoxBase.XUiEvent_HoveredStateChanged onHoveredStateChanged = this.OnHoveredStateChanged;
		if (onHoveredStateChanged != null)
		{
			onHoveredStateChanged(this, this.isOver, this.isOverAnyPart);
		}
		XUiC_Paging.IsOverPagingOverrideElement = this.isOver;
		if (this.isOver && this.GamepadDefaultHoverControl)
		{
			XUiC_GamepadCalloutWindow calloutWindow = this.xui.CalloutWindow;
			if (calloutWindow == null)
			{
				return;
			}
			calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuComboBox, 0f);
			return;
		}
		else
		{
			XUiC_GamepadCalloutWindow calloutWindow2 = this.xui.CalloutWindow;
			if (calloutWindow2 == null)
			{
				return;
			}
			calloutWindow2.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuComboBox);
			return;
		}
	}

	[XuiBindEvent("OnDrag", "clickable")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void dragEvent(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		if (!this.isOver)
		{
			return;
		}
		if (this.Enabled && !this.isEmpty())
		{
			this.pressEvent(_sender, -1);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void invokeValueChangedGeneric()
	{
		XUiC_ComboBoxBase.XUiEvent_GenericValueChanged onValueChangedGeneric = this.OnValueChangedGeneric;
		if (onValueChangedGeneric == null)
		{
			return;
		}
		onValueChangedGeneric(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void incrementalChangeValue(double _value);

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool TryPageUp();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool TryPageDown();

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void setRelativeValue(float _value);

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ComboBoxBase()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showButtons = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool Wrap = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorIndexMarkerActive = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorIndexMarkerInactive = Color.grey;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorIndexMarkerDisabledActive = Color.white;

	[XuiBindComponent("indexMarkers.", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite[] indexMarkerSprites;

	[PublicizedFrom(EAccessModifier.Private)]
	public int indexMarkersSetupBefore;

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueTextOverride;

	[XuiBindComponent("fill", true)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiV_Sprite sprFill;

	[PublicizedFrom(EAccessModifier.Private)]
	public int segmentedFillCount = 1;

	[XuiBindComponent("directvalue", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController clickable;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOverAnyPart;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragging;

	[XuiBindComponent("forward", false)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiController forwardButton;

	[XuiBindComponent("back", false)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiController backButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorDisabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorBg;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorFillEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color colorFillDisabled;

	public delegate void XUiEvent_GenericValueChanged(XUiController _sender);

	public delegate void XUiEvent_HoveredStateChanged(XUiController _sender, bool _isOverMainArea, bool _isOverAnyPart);
}
