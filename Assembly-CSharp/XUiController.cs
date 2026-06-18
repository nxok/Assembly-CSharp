using System;
using System.Collections.Generic;
using Platform;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiController : IXUiElement
{
	public XUiView ViewComponent
	{
		get
		{
			return this.viewComponent;
		}
		set
		{
			this.viewComponent = value;
		}
	}

	public XUiController Parent
	{
		get
		{
			return this.parent;
		}
		set
		{
			if (this.parent == value)
			{
				return;
			}
			XUiController xuiController = this.parent;
			if (xuiController != null)
			{
				xuiController.children.Remove(this);
			}
			this.parent = value;
			XUiController xuiController2 = this.parent;
			if (xuiController2 == null)
			{
				return;
			}
			xuiController2.children.Add(this);
		}
	}

	public List<XUiController> Children
	{
		get
		{
			return this.children;
		}
	}

	public XUiWindowGroup WindowGroup
	{
		get
		{
			return this.windowGroup;
		}
		set
		{
			this.windowGroup = value;
		}
	}

	public bool IsOpen
	{
		get
		{
			return this.windowGroup.isShowing;
		}
	}

	public void AutoBindComponents()
	{
		AutoBindCache.Instance.BindComponents(this);
	}

	public void AutoBindEvents()
	{
		AutoBindCache.Instance.BindEvents(this);
	}

	public virtual void Init()
	{
		if (this.viewComponent != null)
		{
			this.viewComponent.InitView();
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].AutoBindComponents();
			this.children[i].Init();
			this.children[i].AutoBindEvents();
		}
		this.curInputStyle = PlatformManager.NativePlatform.Input.CurrentInputStyle;
	}

	public virtual void Update(float _dt)
	{
		XUiView xuiView = this.viewComponent;
		bool isShowing = this.windowGroup.isShowing;
		if (this.curInputStyle != this.lastInputStyle)
		{
			PlayerInputManager.InputStyle inputStyle = this.lastInputStyle;
			this.lastInputStyle = this.curInputStyle;
			this.RefreshBindings();
			this.inputStyleChanged(inputStyle, this.lastInputStyle);
		}
		if (isShowing && xuiView != null && xuiView.IsVisible)
		{
			using (this.pmVcUpdate.Auto())
			{
				using (this.pmVcNamedUpdate.Auto())
				{
					xuiView.Update(_dt);
				}
			}
		}
		List<XUiController> list = this.children;
		for (int i = 0; i < list.Count; i++)
		{
			XUiController xuiController = list[i];
			if (isShowing || xuiController.AlwaysUpdate)
			{
				xuiController.Update(_dt);
			}
		}
	}

	public bool TryGetChildController(Type _controllerType, ReadOnlyMemory<char> _id, out XUiController _child)
	{
		if ((_id.IsEmpty || (this.viewComponent != null && _id.Span.Equals(this.viewComponent.ID, StringComparison.OrdinalIgnoreCase))) && _controllerType.IsInstanceOfType(this))
		{
			_child = this;
			return true;
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i].TryGetChildController(_controllerType, _id, out _child))
			{
				return true;
			}
		}
		_child = null;
		return false;
	}

	public bool TryGetChildController<T>(ReadOnlyMemory<char> _id, out T _child) where T : XUiController
	{
		if (_id.IsEmpty || (this.viewComponent != null && _id.Span.Equals(this.viewComponent.ID, StringComparison.OrdinalIgnoreCase)))
		{
			T t = this as T;
			if (t != null)
			{
				_child = t;
				return true;
			}
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i].TryGetChildController<T>(_id, out _child))
			{
				return true;
			}
		}
		_child = default(T);
		return false;
	}

	public bool TryGetChildController<T>(string _id, out T _child) where T : XUiController
	{
		return this.TryGetChildController<T>(_id.AsMemory(), out _child);
	}

	public XUiController[] GetChildControllers(Type _controllerType, ReadOnlyMemory<char> _id, List<XUiController> _list = null)
	{
		List<XUiController> list = _list ?? new List<XUiController>();
		if ((_id.IsEmpty || (this.viewComponent != null && _id.Span.Equals(this.viewComponent.ID, StringComparison.OrdinalIgnoreCase))) && _controllerType.IsInstanceOfType(this))
		{
			list.Add(this);
		}
		else
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				this.children[i].GetChildControllers(_controllerType, _id, list);
			}
		}
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T[] GetChildControllers<T>(ReadOnlyMemory<char> _id, List<T> _list = null) where T : XUiController
	{
		List<T> list = _list ?? new List<T>();
		if (_id.IsEmpty || (this.viewComponent != null && _id.Span.Equals(this.viewComponent.ID, StringComparison.OrdinalIgnoreCase)))
		{
			T t = this as T;
			if (t != null)
			{
				list.Add(t);
				goto IL_0082;
			}
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].GetChildControllers<T>(_id, list);
		}
		IL_0082:
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T[] GetChildControllers<T>(string _id, List<T> _list = null) where T : XUiController
	{
		return this.GetChildControllers<T>(_id.AsMemory(), _list);
	}

	public bool TryGetChildView(Type _viewType, ReadOnlyMemory<char> _id, out XUiView _child)
	{
		if (this.viewComponent != null && (_id.IsEmpty || _id.Span.Equals(this.viewComponent.ID, StringComparison.OrdinalIgnoreCase)) && _viewType.IsInstanceOfType(this.viewComponent))
		{
			_child = this.viewComponent;
			return true;
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i].TryGetChildView(_viewType, _id, out _child))
			{
				return true;
			}
		}
		_child = null;
		return false;
	}

	public bool TryGetChildView<T>(ReadOnlyMemory<char> _id, out T _child) where T : XUiView
	{
		T t = this.viewComponent as T;
		if (t != null && (_id.IsEmpty || _id.Span.Equals(t.ID, StringComparison.OrdinalIgnoreCase)))
		{
			_child = t;
			return true;
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i].TryGetChildView<T>(_id, out _child))
			{
				return true;
			}
		}
		_child = default(T);
		return false;
	}

	public bool TryGetChildView<T>(string _id, out T _child) where T : XUiView
	{
		return this.TryGetChildView<T>(_id.AsMemory(), out _child);
	}

	public XUiView[] GetChildViews(Type _viewType, ReadOnlyMemory<char> _id, List<XUiView> _list = null)
	{
		List<XUiView> list = _list ?? new List<XUiView>();
		if (this.viewComponent != null && (_id.IsEmpty || _id.Span.Equals(this.viewComponent.ID, StringComparison.OrdinalIgnoreCase)) && _viewType.IsInstanceOfType(this.viewComponent))
		{
			list.Add(this.viewComponent);
		}
		else
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				this.children[i].GetChildViews(_viewType, _id, list);
			}
		}
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T[] GetChildViews<T>(ReadOnlyMemory<char> _id, List<T> _list = null) where T : XUiView
	{
		List<T> list = _list ?? new List<T>();
		T t = this.viewComponent as T;
		if (t != null && (_id.IsEmpty || _id.Span.Equals(t.ID, StringComparison.OrdinalIgnoreCase)))
		{
			list.Add(t);
		}
		else
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				this.children[i].GetChildViews<T>(_id, list);
			}
		}
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T[] GetChildViews<T>(string _id, List<T> _list = null) where T : XUiView
	{
		return this.GetChildViews<T>(_id.AsMemory(), _list);
	}

	public bool TryGetChildByIdAndType<T>(string _id, out T _child) where T : XUiController
	{
		if (this.viewComponent != null && string.Equals(this.viewComponent.ID, _id, StringComparison.OrdinalIgnoreCase))
		{
			T t = this as T;
			if (t != null)
			{
				_child = t;
				return true;
			}
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i].TryGetChildByIdAndType<T>(_id, out _child))
			{
				return true;
			}
		}
		_child = default(T);
		return false;
	}

	public XUiController GetChildById(string _id)
	{
		if (this.viewComponent != null && string.Equals(this.viewComponent.ID, _id, StringComparison.OrdinalIgnoreCase))
		{
			return this;
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			XUiController childById = this.children[i].GetChildById(_id);
			if (childById != null)
			{
				return childById;
			}
		}
		return null;
	}

	public XUiController[] GetChildrenById(string _id, List<XUiController> _list = null)
	{
		List<XUiController> list = _list ?? new List<XUiController>();
		if (this.viewComponent != null && string.Equals(this.viewComponent.ID, _id, StringComparison.OrdinalIgnoreCase))
		{
			list.Add(this);
		}
		else
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				this.children[i].GetChildrenById(_id, list);
			}
		}
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T GetChildByType<T>() where T : XUiController
	{
		T t = this as T;
		if (t != null)
		{
			return t;
		}
		foreach (XUiController xuiController in this.children)
		{
			t = xuiController.GetChildByType<T>();
			if (t != null)
			{
				return t;
			}
		}
		return default(T);
	}

	public T[] GetChildrenByType<T>(List<T> _list = null) where T : XUiController
	{
		List<T> list = _list ?? new List<T>();
		T t = this as T;
		if (t != null)
		{
			list.Add(t);
		}
		else
		{
			foreach (XUiController xuiController in this.children)
			{
				xuiController.GetChildrenByType<T>(list);
			}
		}
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T[] GetChildrenByViewType<T>(List<T> _list = null) where T : XUiView
	{
		List<T> list = _list ?? new List<T>();
		T t = this.viewComponent as T;
		if (t != null)
		{
			list.Add(t);
		}
		else
		{
			foreach (XUiController xuiController in this.children)
			{
				xuiController.GetChildrenByViewType<T>(list);
			}
		}
		if (_list != null)
		{
			return null;
		}
		return list.ToArray();
	}

	public T GetParentByType<T>() where T : XUiController
	{
		T t = this as T;
		if (t != null)
		{
			return t;
		}
		XUiController xuiController = this.Parent;
		if (xuiController == null)
		{
			return default(T);
		}
		return xuiController.GetParentByType<T>();
	}

	public bool TryGetParentController(Type _controllerType, out XUiController _parent)
	{
		if (_controllerType.IsInstanceOfType(this))
		{
			_parent = this;
			return true;
		}
		if (this.Parent != null)
		{
			return this.Parent.TryGetParentController(_controllerType, out _parent);
		}
		_parent = null;
		return false;
	}

	public bool TryGetParentController<T>(out T _parent) where T : XUiController
	{
		T t = this as T;
		if (t != null)
		{
			_parent = t;
			return true;
		}
		if (this.Parent != null)
		{
			return this.Parent.TryGetParentController<T>(out _parent);
		}
		_parent = default(T);
		return false;
	}

	public bool TryGetParentView(Type _viewType, out XUiView _parent)
	{
		if (_viewType.IsInstanceOfType(this.viewComponent))
		{
			_parent = this.viewComponent;
			return true;
		}
		if (this.Parent != null)
		{
			return this.Parent.TryGetParentView(_viewType, out _parent);
		}
		_parent = null;
		return false;
	}

	public bool TryGetParentView<T>(out T _parent) where T : XUiView
	{
		T t = this.viewComponent as T;
		if (t != null)
		{
			_parent = t;
			return true;
		}
		if (this.Parent != null)
		{
			return this.Parent.TryGetParentView<T>(out _parent);
		}
		_parent = default(T);
		return false;
	}

	public bool IsChildOf(XUiController _controller)
	{
		return this.Parent != null && (this.Parent == _controller || this.Parent.IsChildOf(_controller));
	}

	public bool IsSelfOrChildOf(XUiController _controller)
	{
		return this == _controller || this.IsChildOf(_controller);
	}

	public XUiV_Window GetParentWindow()
	{
		XUiV_Window xuiV_Window = this.ViewComponent as XUiV_Window;
		if (xuiV_Window != null)
		{
			return xuiV_Window;
		}
		XUiController xuiController = this.Parent;
		if (xuiController == null)
		{
			return null;
		}
		return xuiController.GetParentWindow();
	}

	public event XUiEvent_OnMouseUpDownEventHandler OnMouseUpDown;

	public event XUiEvent_OnPressEventHandler OnPress;

	public event XUiEvent_OnPressEventHandler OnDoubleClick;

	public event XUiEvent_OnPressEventHandler OnRightPress;

	public event XUiEvent_OnHoverEventHandler OnHover;

	public event XUiEvent_OnDragEventHandler OnDrag;

	public event XUiEvent_OnHeldHandler OnHold;

	public event XUiEvent_OnScrollEventHandler OnScroll;

	public event XUiEvent_OnSelectEventHandler OnSelect;

	public event XUiEvent_OnVisibilityChanged OnVisiblity;

	public event XUiEvent_OnEnabledChanged OnEnabled;

	public event XUiEvent_OnInteraction OnInteraction;

	public void MouseUpDown(bool _pressed)
	{
		XUiEvent_OnMouseUpDownEventHandler onMouseUpDown = this.OnMouseUpDown;
		if (onMouseUpDown != null)
		{
			onMouseUpDown(this, _pressed);
		}
		XUiEvent_OnInteraction onInteraction = this.OnInteraction;
		if (onInteraction == null)
		{
			return;
		}
		onInteraction(this, EXUiControllerInteractionType.MouseUpDown);
	}

	public virtual void Pressed(int _mouseButton)
	{
		if (_mouseButton != -1)
		{
			if (_mouseButton == -2)
			{
				XUiEvent_OnPressEventHandler onRightPress = this.OnRightPress;
				if (onRightPress != null)
				{
					onRightPress(this, _mouseButton);
				}
				XUiEvent_OnInteraction onInteraction = this.OnInteraction;
				if (onInteraction == null)
				{
					return;
				}
				onInteraction(this, EXUiControllerInteractionType.RightClick);
			}
			return;
		}
		XUiEvent_OnPressEventHandler onPress = this.OnPress;
		if (onPress != null)
		{
			onPress(this, _mouseButton);
		}
		XUiEvent_OnInteraction onInteraction2 = this.OnInteraction;
		if (onInteraction2 == null)
		{
			return;
		}
		onInteraction2(this, EXUiControllerInteractionType.LeftClick);
	}

	public void DoubleClicked(int _mouseButton)
	{
		XUiEvent_OnPressEventHandler onDoubleClick = this.OnDoubleClick;
		if (onDoubleClick != null)
		{
			onDoubleClick(this, _mouseButton);
		}
		XUiEvent_OnInteraction onInteraction = this.OnInteraction;
		if (onInteraction == null)
		{
			return;
		}
		onInteraction(this, EXUiControllerInteractionType.DoubleClick);
	}

	public void Hovered(bool _isOver)
	{
		XUiEvent_OnHoverEventHandler onHover = this.OnHover;
		if (onHover != null)
		{
			onHover(this, _isOver);
		}
		XUiEvent_OnInteraction onInteraction = this.OnInteraction;
		if (onInteraction != null)
		{
			onInteraction(this, EXUiControllerInteractionType.Hover);
		}
		this.OnHovered(_isOver);
	}

	public void Scrolled(float _delta)
	{
		XUiEvent_OnScrollEventHandler onScroll = this.OnScroll;
		if (onScroll != null)
		{
			onScroll(this, _delta);
		}
		XUiEvent_OnInteraction onInteraction = this.OnInteraction;
		if (onInteraction == null)
		{
			return;
		}
		onInteraction(this, EXUiControllerInteractionType.Scroll);
	}

	public void Selected(bool _selected)
	{
		XUiEvent_OnSelectEventHandler onSelect = this.OnSelect;
		if (onSelect != null)
		{
			onSelect(this, _selected);
		}
		XUiEvent_OnInteraction onInteraction = this.OnInteraction;
		if (onInteraction == null)
		{
			return;
		}
		onInteraction(this, EXUiControllerInteractionType.Select);
	}

	public void Dragged(Vector2 _mouseDelta, EDragType _dragType)
	{
		if (this.viewComponent.ID == "btn2")
		{
			Log.Warning(string.Format("[XUi] F={0} ButtonC received Dragged={1} event", Time.frameCount, _dragType));
		}
		XUiEvent_OnDragEventHandler onDrag = this.OnDrag;
		if (onDrag != null)
		{
			onDrag(this, _dragType, _mouseDelta);
		}
		XUiEvent_OnInteraction onInteraction = this.OnInteraction;
		if (onInteraction == null)
		{
			return;
		}
		onInteraction(this, EXUiControllerInteractionType.Drag);
	}

	public void Held(EHoldType _event, float _holdDuration, float _deltaSinceLastTimedEvent = -1f)
	{
		XUiEvent_OnHeldHandler onHold = this.OnHold;
		if (onHold != null)
		{
			onHold(this, _event, _holdDuration, _deltaSinceLastTimedEvent);
		}
		XUiEvent_OnInteraction onInteraction = this.OnInteraction;
		if (onInteraction == null)
		{
			return;
		}
		onInteraction(this, EXUiControllerInteractionType.Held);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnHovered(bool _isOver)
	{
	}

	public virtual void OnOpen()
	{
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnOpen();
		}
		XUiView xuiView = this.ViewComponent;
		if (xuiView != null)
		{
			xuiView.OnOpen();
		}
		XUiEvent_OnInteraction onInteraction = this.OnInteraction;
		if (onInteraction != null)
		{
			onInteraction(this, EXUiControllerInteractionType.Open);
		}
		this.RefreshBindings();
	}

	public virtual void OnClose()
	{
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnClose();
		}
		XUiView xuiView = this.ViewComponent;
		if (xuiView != null)
		{
			xuiView.OnClose();
		}
		XUiEvent_OnInteraction onInteraction = this.OnInteraction;
		if (onInteraction == null)
		{
			return;
		}
		onInteraction(this, EXUiControllerInteractionType.Close);
	}

	public void OnVisibilityChanged(bool _parentVisibleInScene)
	{
		bool isVisible = this.viewComponent.IsVisible;
		bool flag = isVisible && _parentVisibleInScene;
		XUiEvent_OnVisibilityChanged onVisiblity = this.OnVisiblity;
		if (onVisiblity != null)
		{
			onVisiblity(this, isVisible, flag);
		}
		XUiEvent_OnInteraction onInteraction = this.OnInteraction;
		if (onInteraction != null)
		{
			onInteraction(this, EXUiControllerInteractionType.VisibilityChanged);
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnVisibilityChanged(flag);
			this.children[i].viewComponent.OnVisibilityChanged(flag);
		}
	}

	public void OnEnabledChanged(bool _enabled)
	{
		XUiEvent_OnEnabledChanged onEnabled = this.OnEnabled;
		if (onEnabled != null)
		{
			onEnabled(this, _enabled);
		}
		XUiEvent_OnInteraction onInteraction = this.OnInteraction;
		if (onInteraction == null)
		{
			return;
		}
		onInteraction(this, EXUiControllerInteractionType.EnabledChanged);
	}

	public virtual void OnCursorSelected(bool _isActualElement)
	{
		XUiV_ScrollView xuiV_ScrollView;
		if (_isActualElement && this.TryGetParentView<XUiV_ScrollView>(out xuiV_ScrollView))
		{
			xuiV_ScrollView.MakeVisible(this.ViewComponent);
		}
		XUiController xuiController = this.parent;
		if (xuiController == null)
		{
			return;
		}
		xuiController.OnCursorSelected(false);
	}

	public virtual void OnCursorUnSelected()
	{
		XUiController xuiController = this.parent;
		if (xuiController == null)
		{
			return;
		}
		xuiController.OnCursorUnSelected();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool handleDirtyUpdateDefault()
	{
		if (!this.IsDirty)
		{
			return false;
		}
		this.IsDirty = false;
		this.RefreshBindings();
		return true;
	}

	public virtual bool ParseAttribute(string _name, string _value)
	{
		if (_name == "always_update")
		{
			StringParsers.TryParseBool(_value, out this.AlwaysUpdate);
			return true;
		}
		return false;
	}

	public bool GetBindingValue(ref string _value, string _bindingName)
	{
		bool bindingValueInternal;
		try
		{
			bindingValueInternal = this.GetBindingValueInternal(ref _value, _bindingName);
		}
		catch
		{
			Log.Error("Unhandled exception in GetBindingValue. Binding name: " + _bindingName + ", hierarchy: " + this.GetXuiHierarchy());
			throw;
		}
		return bindingValueInternal;
	}

	[XuiXmlBinding("is_online")]
	public bool IsOnline
	{
		get
		{
			return PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.LoggedIn;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 2161620009U)
		{
			if (num <= 820250566U)
			{
				if (num <= 364418046U)
				{
					if (num != 23126121U)
					{
						if (num == 364418046U)
						{
							if (_bindingName == "is_map_enabled")
							{
								_value = World.MapEnabled.ToString();
								return true;
							}
						}
					}
					else if (_bindingName == "is_challenge_enabled")
					{
						_value = (ChallengeJournal.AllowChallenges || World.BiomeProgressionEnabled).ToString();
						return true;
					}
				}
				else if (num != 470361894U)
				{
					if (num == 820250566U)
					{
						if (_bindingName == "is_unityeditor")
						{
							_value = "false";
							return true;
						}
					}
				}
				else if (_bindingName == "version_long")
				{
					_value = Constants.cVersionInformation.LongString;
					return true;
				}
			}
			else if (num <= 1573359332U)
			{
				if (num != 1383400110U)
				{
					if (num == 1573359332U)
					{
						if (_bindingName == "is_playtesting")
						{
							_value = GameUtils.IsPlaytesting().ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "is_world_loaded")
				{
					_value = (GameManager.Instance.World != null).ToString();
					return true;
				}
			}
			else if (num != 1899437583U)
			{
				if (num == 2161620009U)
				{
					if (_bindingName == "gamelanguage")
					{
						_value = Localization.ActiveLanguage;
						return true;
					}
				}
			}
			else if (_bindingName == "is_modal")
			{
				_value = this.WindowGroup.isModal.ToString();
				return true;
			}
		}
		else if (num <= 2799769034U)
		{
			if (num <= 2745931225U)
			{
				if (num != 2577987401U)
				{
					if (num == 2745931225U)
					{
						if (_bindingName == "is_server")
						{
							_value = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer.ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "is_editmode")
				{
					_value = GameManager.Instance.IsEditMode().ToString();
					return true;
				}
			}
			else if (num != 2759425756U)
			{
				if (num == 2799769034U)
				{
					if (_bindingName == "is_ps5")
					{
						_value = DeviceFlag.PS5.IsCurrent().ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "is_prefab_editor")
			{
				_value = PrefabEditModeManager.Instance.IsActive().ToString();
				return true;
			}
		}
		else if (num <= 2963904327U)
		{
			if (num != 2934648263U)
			{
				if (num == 2963904327U)
				{
					if (_bindingName == "is_release")
					{
						_value = "true";
						return true;
					}
				}
			}
			else if (_bindingName == "is_console")
			{
				_value = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent().ToString();
				return true;
			}
		}
		else if (num != 3445481009U)
		{
			if (num != 3556752319U)
			{
				if (num == 4180283068U)
				{
					if (_bindingName == "inputstyle")
					{
						this.registerForInputStyleChanges();
						_value = this.lastInputStyle.ToStringCached<PlayerInputManager.InputStyle>();
						return true;
					}
				}
			}
			else if (_bindingName == "is_creative")
			{
				_value = (GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled)).ToString();
				return true;
			}
		}
		else if (_bindingName == "is_controller_input")
		{
			this.registerForInputStyleChanges();
			_value = (this.lastInputStyle != PlayerInputManager.InputStyle.Keyboard).ToString();
			return true;
		}
		return false;
	}

	public void RefreshBindingsSelfAndChildren()
	{
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].RefreshBindingsSelfAndChildren();
		}
		this.Bindings.RefreshBindings();
	}

	public void RefreshBindings()
	{
		this.Bindings.RefreshBindings();
	}

	public virtual void Cleanup()
	{
		foreach (XUiController xuiController in this.children)
		{
			xuiController.Cleanup();
		}
		if (this.registeredForInputStyleChanges)
		{
			IPlatform nativePlatform = PlatformManager.NativePlatform;
			if (((nativePlatform != null) ? nativePlatform.Input : null) != null)
			{
				PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= this.OnLastInputStyleChanged;
			}
		}
		XUiView xuiView = this.ViewComponent;
		if (xuiView == null)
		{
			return;
		}
		xuiView.Cleanup();
	}

	public void FindNavigatableChildren(List<XUiView> _views)
	{
		foreach (XUiController xuiController in this.children)
		{
			if (xuiController.viewComponent.IsNavigatable)
			{
				_views.Add(xuiController.viewComponent);
			}
			xuiController.FindNavigatableChildren(_views);
		}
	}

	public bool TryFindFirstNavigableView(out XUiView _foundView)
	{
		_foundView = null;
		bool isActiveInHierarchy = this.ViewComponent.IsActiveInHierarchy;
		if (this.ViewComponent.IsNavigatable && this.ViewComponent.IsVisible && isActiveInHierarchy)
		{
			_foundView = this.ViewComponent;
			return true;
		}
		if (!isActiveInHierarchy)
		{
			return false;
		}
		using (List<XUiController>.Enumerator enumerator = this.children.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.TryFindFirstNavigableView(out _foundView))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool SelectCursorElement(bool _withDelay = false, bool _overrideCursorMode = false)
	{
		if (this.ViewComponent == null)
		{
			return false;
		}
		CursorControllerAbs cursorController = this.xui.playerUI.CursorController;
		if (cursorController.CursorModeActive && !_overrideCursorMode)
		{
			return false;
		}
		XUiView xuiView;
		this.TryFindFirstNavigableView(out xuiView);
		if (xuiView == null)
		{
			return false;
		}
		if (_withDelay)
		{
			cursorController.SetNavigationTargetLater(xuiView);
		}
		else
		{
			cursorController.SetNavigationTarget(xuiView);
		}
		return true;
	}

	public void SetAllChildrenDirty()
	{
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].SetAllChildrenDirty();
		}
		XUiView xuiView = this.viewComponent;
		if (xuiView != null)
		{
			xuiView.SetDirty();
		}
		this.IsDirty = true;
	}

	public PlayerInputManager.InputStyle CurrentInputStyle
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.lastInputStyle;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void registerForInputStyleChanges()
	{
		if (this.registeredForInputStyleChanges)
		{
			return;
		}
		this.registeredForInputStyleChanges = true;
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.OnLastInputStyleChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		this.curInputStyle = _style;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void inputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
	}

	public void ForceInputStyleChange(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		if (this.registeredForInputStyleChanges)
		{
			this.inputStyleChanged(_oldStyle, _newStyle);
		}
		foreach (XUiController xuiController in this.children)
		{
			xuiController.ForceInputStyleChange(_oldStyle, _newStyle);
		}
	}

	public XUiController Controller
	{
		get
		{
			return this;
		}
	}

	public string GetXuiHierarchy()
	{
		return XUiUtils.GetXuiHierarchy(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiView viewComponent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController parent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<XUiController> children = new List<XUiController>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiWindowGroup windowGroup;

	public bool AlwaysUpdate;

	public bool IsDirty;

	public object CustomData;

	public readonly ObservableDictionary<string, object> CustomAttributes = new ObservableDictionary<string, object>();

	public XUi xui;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmControllerUpdate = new ProfilerMarker("XC.Update");

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmVcUpdate = new ProfilerMarker("VC");

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmVcNamedUpdate;

	public readonly BindingsManager Bindings = new BindingsManager();

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle lastInputStyle = PlayerInputManager.InputStyle.Count;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle curInputStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool registeredForInputStyleChanges;
}
