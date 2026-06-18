using System;
using System.Collections.Generic;
using UnityEngine;

public class NGUIWindowManager : MonoBehaviour
{
	public GUIWindowManager WindowManager
	{
		get
		{
			if (!(this.playerUI != null))
			{
				return null;
			}
			return this.playerUI.windowManager;
		}
	}

	public NGuiWdwInGameHUD InGameHUD
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		this.playerUI = base.GetComponent<LocalPlayerUI>();
		this.bGlobalShowFlag = true;
		this.ParseWindows();
	}

	public void ParseWindows()
	{
		if (this.parsedWindows)
		{
			return;
		}
		this.parsedWindows = true;
		foreach (Transform transform in this.Windows)
		{
			if (!(transform == null))
			{
				EnumNGUIWindow enumNGUIWindow = EnumUtils.Parse<EnumNGUIWindow>(transform.name.Substring(3), false);
				this.windowMap[enumNGUIWindow] = transform;
			}
		}
		Transform window = this.GetWindow(EnumNGUIWindow.InGameHUD);
		if (window != null)
		{
			this.InGameHUD = window.GetComponent<NGuiWdwInGameHUD>();
			return;
		}
		Log.Error("Window wdwInGameHUD not found!");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform GetWindow(EnumNGUIWindow _wdw)
	{
		Transform transform;
		if (!this.windowMap.TryGetValue(_wdw, out transform))
		{
			Log.Error("NGUIWindowManager.GetWindow: Window " + _wdw.ToStringCached<EnumNGUIWindow>() + " not found!");
		}
		return transform;
	}

	public void ShowAll(bool _bShow)
	{
		this.bGlobalShowFlag = _bShow;
		if (!_bShow)
		{
			this.windowsVisibleBeforeHide.Clear();
			using (Dictionary<EnumNGUIWindow, Transform>.Enumerator enumerator = this.windowMap.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<EnumNGUIWindow, Transform> keyValuePair = enumerator.Current;
					EnumNGUIWindow enumNGUIWindow;
					Transform transform;
					keyValuePair.Deconstruct(out enumNGUIWindow, out transform);
					EnumNGUIWindow enumNGUIWindow2 = enumNGUIWindow;
					if (this.IsShowing(enumNGUIWindow2))
					{
						this.windowsVisibleBeforeHide.Add(enumNGUIWindow2);
						this.Show(enumNGUIWindow2, false);
					}
				}
				return;
			}
		}
		foreach (EnumNGUIWindow enumNGUIWindow3 in this.windowsVisibleBeforeHide)
		{
			this.Show(enumNGUIWindow3, true);
		}
	}

	public bool IsShowing(EnumNGUIWindow _eWindow)
	{
		Transform window = this.GetWindow(_eWindow);
		return window != null && window.gameObject.activeSelf;
	}

	public void Show(EnumNGUIWindow _eWindow, bool _bEnable)
	{
		Transform window = this.GetWindow(_eWindow);
		if (window == null)
		{
			return;
		}
		window.gameObject.SetActive(_bEnable && this.bGlobalShowFlag);
		if (_eWindow == EnumNGUIWindow.Loading && this.ellipsisAnimator != null)
		{
			this.ellipsisAnimator = null;
		}
	}

	public void SetLabelText(EnumNGUIWindow _eElement, string _text, bool _toUpper = true)
	{
		Transform window = this.GetWindow(_eElement);
		if (window == null)
		{
			return;
		}
		this.Show(_eElement, !string.IsNullOrEmpty(_text));
		if (string.IsNullOrEmpty(_text))
		{
			return;
		}
		UILabel component = window.GetComponent<UILabel>();
		if (!component)
		{
			return;
		}
		_text = ((!_toUpper) ? _text : _text.ToUpper());
		component.text = _text;
		if (_eElement == EnumNGUIWindow.Loading)
		{
			this.ellipsisAnimator = new TextEllipsisAnimator(_text, component);
		}
	}

	public void SetLabel(EnumNGUIWindow _eElement, string _text, Color? _color = null, bool _toUpper = true)
	{
		Transform window = this.GetWindow(_eElement);
		if (window == null)
		{
			return;
		}
		this.Show(_eElement, !string.IsNullOrEmpty(_text));
		UILabel component = window.GetComponent<UILabel>();
		if (!component)
		{
			return;
		}
		_text = ((!_toUpper) ? _text : ((_text != null) ? _text.ToUpper() : null));
		component.text = _text ?? "";
		if (_color != null)
		{
			component.color = _color.Value;
		}
		if (_eElement == EnumNGUIWindow.Loading)
		{
			this.ellipsisAnimator = new TextEllipsisAnimator(_text + "...", component);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.playerUI == null)
		{
			this.playerUI = base.GetComponent<LocalPlayerUI>();
		}
		if (!this.playerUI.isPrimaryUI)
		{
			return;
		}
		this.updateVersionLabelDisplayState();
		TextEllipsisAnimator textEllipsisAnimator = this.ellipsisAnimator;
		if (textEllipsisAnimator == null)
		{
			return;
		}
		textEllipsisAnimator.GetNextAnimatedString(Time.unscaledDeltaTime);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateVersionLabelDisplayState()
	{
		GUIWindowManager windowManager = this.WindowManager;
		bool flag = this.AlwaysShowVersionUi && LocalPlayerUI.GetUIForPrimaryPlayer() != null;
		if (!flag)
		{
			LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
			if (uiforPrimaryPlayer != null)
			{
				flag |= uiforPrimaryPlayer.windowManager.IsWindowOpen("ingameMenu");
			}
		}
		this.ShowVersionUi(flag);
	}

	public bool AlwaysShowVersionUi { get; set; } = true;

	public bool VersionUiVisible
	{
		get
		{
			return this.GetWindow(EnumNGUIWindow.Version).gameObject.activeSelf;
		}
	}

	public void ToggleVersionUi()
	{
		this.ShowVersionUi(!this.VersionUiVisible);
	}

	public void ShowVersionUi(bool _show)
	{
		this.GetWindow(EnumNGUIWindow.Version).gameObject.SetActive(_show);
	}

	public void SetBackgroundScale(float _uiScale)
	{
		this.GetWindow(EnumNGUIWindow.MainMenuBackground).localScale = new Vector3(_uiScale, _uiScale, _uiScale);
	}

	public Transform[] Windows;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly Dictionary<EnumNGUIWindow, Transform> windowMap = new EnumDictionary<EnumNGUIWindow, Transform>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly HashSet<EnumNGUIWindow> windowsVisibleBeforeHide = new HashSet<EnumNGUIWindow>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bGlobalShowFlag;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LocalPlayerUI playerUI;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool parsedWindows;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public TextEllipsisAnimator ellipsisAnimator;
}
