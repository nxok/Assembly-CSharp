using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TextInput : XUiController
{
	public event XUiEvent_InputOnSubmitEventHandler OnSubmitHandler;

	public event XUiEvent_InputOnChangedEventHandler OnChangeHandler;

	public event XUiEvent_InputOnAbortedEventHandler OnInputAbortedHandler;

	public event XUiEvent_InputOnSelectedEventHandler OnInputSelectedHandler;

	public event XUiEvent_InputOnErrorEventHandler OnInputErrorHandler;

	public event UIInput.OnClipboard OnClipboardHandler;

	[XuiXmlAttribute("value", false)]
	public string InitialValue
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("caret_color", false)]
	public Color CaretColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("selection_color", false)]
	public Color SelectionColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("validation", false)]
	public UIInput.Validation Validation
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("hide_input", false)]
	public bool HideInput
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("on_return", false)]
	public UIInput.OnReturnKey OnReturnKey
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = 1;

	[XuiXmlAttribute("character_limit", false)]
	public int CharacterLimit
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("input_type", false)]
	public UIInput.KeyboardType InputType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("ignore_up_down_keys", false)]
	public bool IgnoreUpDownKeys
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("search_field", false)]
	public bool IsSearchField
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("clear_button", false)]
	[XuiXmlBinding("clearbutton")]
	public bool HasClearButton
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("password_field", false)]
	[XuiXmlBinding("passwordfield")]
	public bool IsPasswordField
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("focus_on_open", false)]
	public bool FocusOnOpen
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("open_vk_on_open", false)]
	public bool OpenVkOnOpen
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("clear_on_open", false)]
	public bool ClearOnOpen
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("vk_propagates_events", false)]
	public bool VkPropagatesEvents
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = true;

	[XuiXmlAttribute("close_group_on_tab", false)]
	public bool CloseGroupOnTab
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static void SelectCurrentSearchField(LocalPlayerUI _playerUi)
	{
		if (XUiC_TextInput.currentSearchField != null && !_playerUi.windowManager.IsInputActive() && XUiC_TextInput.currentSearchField.viewComponent.IsActiveInHierarchy)
		{
			XUiC_TextInput.currentSearchField.SetSelected(true, false);
		}
	}

	public UIInput UIInput
	{
		get
		{
			return this.uiInput;
		}
	}

	public XUiController UIInputController
	{
		get
		{
			return this.uiInputController;
		}
	}

	public string Text
	{
		get
		{
			UIInput uiinput = this.uiInput;
			return ((uiinput != null) ? uiinput.value : null) ?? "";
		}
		set
		{
			this.textChangeFromCode = true;
			this.uiInput.value = value;
			this.textChangeFromCode = false;
			this.uiInput.UpdateLabel();
		}
	}

	public XUiC_TextInput SelectOnTab
	{
		get
		{
			return this.selectOnTab;
		}
		set
		{
			if (this.selectOnTab == value)
			{
				return;
			}
			this.selectOnTab = value;
			if (value != null)
			{
				UIKeyNavigation uikeyNavigation = NGUITools.AddMissingComponent<UIKeyNavigation>(this.uiInputController.ViewComponent.UiTransform.gameObject);
				uikeyNavigation.constraint = 3;
				uikeyNavigation.onTab = value.uiInput.gameObject;
				return;
			}
			UIKeyNavigation component = this.uiInputController.ViewComponent.UiTransform.gameObject.GetComponent<UIKeyNavigation>();
			if (component != null)
			{
				Object.Destroy(component);
			}
		}
	}

	[XuiXmlAttribute("active_text_color", false)]
	public Color ActiveTextColor
	{
		get
		{
			return this.activeTextColor;
		}
		set
		{
			if (value == this.activeTextColor)
			{
				return;
			}
			this.activeTextColor = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlAttribute("input_enabled", false)]
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

	public bool SupportBbCode
	{
		get
		{
			return this.label.SupportBbCode;
		}
		set
		{
			this.label.SupportBbCode = value;
		}
	}

	public bool IsSelected
	{
		get
		{
			return this.uiInput.isSelected;
		}
	}

	[XuiXmlBinding("showpassword")]
	public bool ShowPassword
	{
		get
		{
			return this.displayType == 0;
		}
	}

	[XuiXmlAttribute("virtual_keyboard_prompt", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeVirtualKeyboardPrompt(string _value)
	{
		this.virtKeyboardPrompt = Localization.Get(_value, false, null);
	}

	[XuiXmlBinding("txt_empty")]
	public bool IsEmpty
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.Text.Length == 0;
		}
	}

	[XuiXmlAttribute("select_on_tab", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeSelectOnTab(string _value)
	{
		if (string.IsNullOrEmpty(_value))
		{
			return;
		}
		this.selectOnTabString = _value;
		this.selectOnTabSetFromXML = true;
		this.IsDirty = true;
	}

	public override void Init()
	{
		base.Init();
		this.uiInputController = this.label.Controller;
		this.uiInput = this.uiInputController.ViewComponent.UiTransform.gameObject.AddComponent<UIInput>();
		if (this.IgnoreUpDownKeys)
		{
			this.uiInput.onDownArrow = delegate
			{
			};
			this.uiInput.onUpArrow = delegate
			{
			};
		}
		XUiController childById = base.GetChildById("btnShowPassword");
		if (childById != null)
		{
			childById.OnPress += this.BtnShowPassword_OnPress;
		}
		XUiController childById2 = base.GetChildById("btnClearInput");
		if (childById2 != null)
		{
			childById2.OnPress += this.BtnClearInput_OnPress;
		}
		EventDelegate.Add(this.uiInput.onSubmit, new EventDelegate.Callback(this.OnSubmit));
		EventDelegate.Add(this.uiInput.onChange, new EventDelegate.Callback(this.OnChange));
		this.uiInput.onClipboard += new UIInput.OnClipboard(this.OnClipboard);
		this.label.BindToUiInput(this.uiInput);
		this.uiInput.value = this.InitialValue ?? "";
		this.uiInput.activeTextColor = this.activeTextColor;
		this.uiInput.caretColor = this.CaretColor;
		this.uiInput.selectionColor = this.SelectionColor;
		this.uiInput.inputType = this.displayType;
		this.uiInput.validation = this.Validation;
		this.uiInput.hideInput = this.HideInput;
		this.uiInput.onReturnKey = this.OnReturnKey;
		this.uiInput.characterLimit = this.CharacterLimit;
		this.uiInput.keyboardType = this.InputType;
		this.uiInputController.OnSelect += this.inputFieldSelected;
	}

	[XuiBindEvent("OnPress", "scrollArea")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ScrollArea_OnPress(XUiController _sender, int _mouseButton)
	{
		this.SelectOrVirtualKeyboard(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnShowPassword_OnPress(XUiController _sender, int _mouseButton)
	{
		this.displayType = ((this.displayType == 2) ? 0 : 2);
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClearInput_OnPress(XUiController _sender, int _mouseButton)
	{
		this.Text = "";
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showVirtualKeyboard()
	{
		if (!this.Enabled)
		{
			return;
		}
		IVirtualKeyboard virtualKeyboard = PlatformManager.NativePlatform.VirtualKeyboard;
		string text;
		if (virtualKeyboard == null)
		{
			text = Localization.Get("ttPlatformHasNoVirtualKeyboard", false, null);
		}
		else
		{
			uint num = (uint)((this.CharacterLimit <= 0) ? 200 : this.CharacterLimit);
			text = virtualKeyboard.Open(this.virtKeyboardPrompt, this.uiInput.value, new Action<bool, string>(this.OnTextReceived), this.displayType, this.OnReturnKey == 2, num);
		}
		if (text == null)
		{
			return;
		}
		GameManager instance = GameManager.Instance;
		EntityPlayerLocal entityPlayerLocal;
		if (instance == null)
		{
			entityPlayerLocal = null;
		}
		else
		{
			World world = instance.World;
			entityPlayerLocal = ((world != null) ? world.GetPrimaryPlayer() : null);
		}
		GameManager.ShowTooltip(entityPlayerLocal, "[BB0000]" + text, false, false, 0f);
		XUiEvent_InputOnErrorEventHandler onInputErrorHandler = this.OnInputErrorHandler;
		if (onInputErrorHandler == null)
		{
			return;
		}
		onInputErrorHandler(this, text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void inputFieldSelected(XUiController _sender, bool _selected)
	{
		if (_selected)
		{
			ThreadManager.StartCoroutine(this.removeSelection());
			return;
		}
		XUiEvent_InputOnSelectedEventHandler onInputSelectedHandler = this.OnInputSelectedHandler;
		if (onInputSelectedHandler == null)
		{
			return;
		}
		onInputSelectedHandler(this, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator removeSelection()
	{
		yield return null;
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			this.SetSelected(false, false);
			this.showVirtualKeyboard();
		}
		XUiEvent_InputOnSelectedEventHandler onInputSelectedHandler = this.OnInputSelectedHandler;
		if (onInputSelectedHandler != null)
		{
			onInputSelectedHandler(this, true);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTextReceived(bool _success, string _text)
	{
		if (!this.isOpen)
		{
			return;
		}
		this.Text = _text;
		if (this.VkPropagatesEvents)
		{
			if (_success)
			{
				if (this.OnSubmitHandler != null)
				{
					this.OnSubmitHandler(this, _text);
				}
				else if (this.OnChangeHandler != null)
				{
					this.OnChangeHandler(this, _text, false);
				}
			}
			else if (this.OnInputAbortedHandler != null)
			{
				this.OnInputAbortedHandler(this);
			}
		}
		this.uiInput.RemoveFocus();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSubmit()
	{
		string value = UIInput.current.value;
		this.uiInput.RemoveFocus();
		if (this.OnSubmitHandler != null)
		{
			ThreadManager.StartCoroutine(this.delaySubmitHandler(value));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator delaySubmitHandler(string _input)
	{
		yield return null;
		XUiEvent_InputOnSubmitEventHandler onSubmitHandler = this.OnSubmitHandler;
		if (onSubmitHandler != null)
		{
			onSubmitHandler(this, _input);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnChange()
	{
		if (this.windowGroup.isShowing)
		{
			this.updateScrollPositionToCaret();
		}
		this.IsDirty = true;
		XUiEvent_InputOnChangedEventHandler onChangeHandler = this.OnChangeHandler;
		if (onChangeHandler == null)
		{
			return;
		}
		onChangeHandler(this, UIInput.current.value, this.textChangeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateScrollPositionToCaret()
	{
		if (this.scrollView == null)
		{
			return;
		}
		UITexture caret = this.uiInput.caret;
		if (!(caret == null) && caret.enabled)
		{
			List<Vector3> verts = caret.geometry.verts;
			for (int i = 0; i < verts.Count; i++)
			{
				if (i >= this.caretWorldCorners.Count)
				{
					this.caretWorldCorners.Add(Vector3.zero);
				}
				this.caretWorldCorners[i] = caret.transform.TransformPoint(verts[i]);
			}
			this.scrollView.MakeVisible(this.caretWorldCorners);
			return;
		}
		XUiV_ScrollView xuiV_ScrollView = this.scrollView;
		if (xuiV_ScrollView == null)
		{
			return;
		}
		xuiV_ScrollView.UpdatePosition();
	}

	public void TriggerOnChangeHandler(bool _changeFromCode = false)
	{
		XUiEvent_InputOnChangedEventHandler onChangeHandler = this.OnChangeHandler;
		if (onChangeHandler == null)
		{
			return;
		}
		onChangeHandler(this, this.uiInput.value, _changeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClipboard(UIInput.ClipboardAction _actionType, string _oldText, int _selStart, int _selEnd, string _actionResultText)
	{
		UIInput.OnClipboard onClipboardHandler = this.OnClipboardHandler;
		if (onClipboardHandler == null)
		{
			return;
		}
		onClipboardHandler.Invoke(_actionType, _oldText, _selStart, _selEnd, _actionResultText);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.label.Text = this.uiInput.value;
		this.IsDirty = true;
		this.isOpen = true;
		this.openCompleted = false;
		this.removeFocusOnOpen = true;
		if (this.IsPasswordField)
		{
			this.displayType = 2;
			this.uiInput.UpdateLabel();
		}
		if (this.IsSearchField)
		{
			XUiC_TextInput.currentSearchField = this;
		}
		if (this.ClearOnOpen)
		{
			this.Text = "";
		}
		if (this.FocusOnOpen)
		{
			this.SetSelected(true, true);
		}
		if (this.OpenVkOnOpen)
		{
			this.SelectOrVirtualKeyboard(true);
		}
	}

	public override void OnClose()
	{
		this.SetSelected(false, false);
		base.OnClose();
		this.isOpen = false;
		if (XUiC_TextInput.currentSearchField == this)
		{
			XUiC_TextInput.currentSearchField = null;
		}
	}

	[XuiBindEvent("OnVisiblity", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void visibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (!_visibleInScene)
		{
			this.SetSelected(false, false);
			GameObject selectedObject = UICamera.selectedObject;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.xui.playerUI.CursorController.GetMouseButton(0) && this.uiInput.isSelected && !this.uiInputController.ViewComponent.UiTransformIsHovered)
		{
			this.uiInput.isSelected = false;
		}
		if (this.CloseGroupOnTab && this.uiInput.isSelected && this.selectOnTab == null)
		{
			PlayerAction inventory = this.xui.playerUI.playerInput.PermanentActions.Inventory;
			KeyBindingSource keyBindingSource = inventory.GetBindingOfType(false) as KeyBindingSource;
			if (inventory.WasPressed && keyBindingSource != null && keyBindingSource.Control == XUiC_TextInput.tabCombo)
			{
				ThreadManager.StartCoroutine(this.closeOnTabLater());
			}
		}
		if (this.IsDirty)
		{
			if (!this.Enabled)
			{
				this.uiInput.isSelected = false;
			}
			this.uiInput.enabled = this.Enabled;
			this.uiInput.inputType = this.displayType;
			this.uiInput.activeTextColor = this.activeTextColor;
			this.uiInput.UpdateLabel();
			if (!this.openCompleted && this.removeFocusOnOpen)
			{
				this.uiInput.RemoveFocus();
			}
			this.parseSelectOnTabName();
			this.IsDirty = false;
			base.RefreshBindings();
			if (!this.openCompleted)
			{
				this.openCompleted = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator closeOnTabLater()
	{
		PlayerAction inventoryAction = this.xui.playerUI.playerInput.PermanentActions.Inventory;
		KeyBindingSource keyBindingSource = inventoryAction.GetBindingOfType(false) as KeyBindingSource;
		if (keyBindingSource == null || keyBindingSource.Control != XUiC_TextInput.tabCombo)
		{
			yield break;
		}
		while (inventoryAction.IsPressed)
		{
			yield return null;
		}
		if (this.CloseGroupOnTab && this.uiInput.isSelected && this.selectOnTab == null)
		{
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		}
		yield break;
	}

	public void SetSelected(bool _selected = true, bool _delayed = false)
	{
		ThreadManager.StartCoroutine(this.setSelectedDelayed(_selected, _delayed));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setSelectedDelayed(bool _selected, bool _delayed)
	{
		if (_delayed)
		{
			yield return null;
		}
		if (!_selected || PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			this.uiInput.isSelected = _selected;
			if (!this.openCompleted)
			{
				this.removeFocusOnOpen = false;
			}
		}
		yield break;
	}

	public void SelectOrVirtualKeyboard(bool _delayed = false)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			this.SetSelected(true, _delayed);
			return;
		}
		this.showVirtualKeyboard();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void parseSelectOnTabName()
	{
		if (!this.selectOnTabSetFromXML)
		{
			return;
		}
		XUiC_TextInput xuiC_TextInput = this.selectOnTab;
		if (((xuiC_TextInput != null) ? xuiC_TextInput.ViewComponent.ID : null) == this.selectOnTabString)
		{
			return;
		}
		XUiView xuiView = XUiUtils.FindHierarchyClosestView(this, this.selectOnTabString);
		XUiC_TextInput xuiC_TextInput2 = ((xuiView != null) ? xuiView.Controller : null) as XUiC_TextInput;
		if (xuiC_TextInput2 != null)
		{
			this.SelectOnTab = xuiC_TextInput2;
		}
	}

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label label;

	[XuiBindComponent("scrollarea", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController scrollArea;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_ScrollView scrollView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController uiInputController;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIInput uiInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput selectOnTab;

	[PublicizedFrom(EAccessModifier.Private)]
	public string selectOnTabString;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selectOnTabSetFromXML;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color activeTextColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIInput.InputType displayType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string virtKeyboardPrompt = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openCompleted;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool removeFocusOnOpen = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly KeyCombo tabCombo = new KeyCombo(new Key[] { 66 });

	[PublicizedFrom(EAccessModifier.Private)]
	public bool textChangeFromCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_TextInput currentSearchField;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector3> caretWorldCorners = new List<Vector3>(4);
}
