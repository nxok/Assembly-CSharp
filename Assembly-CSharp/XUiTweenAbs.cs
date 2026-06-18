using System;
using UnityEngine;

public abstract class XUiTweenAbs : IXUiElement
{
	[XuiXmlAttribute("repeat", false)]
	public UITweener.Style RepeatStyle
	{
		get
		{
			return this.repeatStyle;
		}
		set
		{
			if (this.repeatStyle == value)
			{
				return;
			}
			this.repeatStyle = value;
			if (this.tweenGeneric != null)
			{
				this.tweenGeneric.style = value;
			}
		}
	}

	[XuiXmlAttribute("duration", false)]
	public float Duration
	{
		get
		{
			return this.duration;
		}
		set
		{
			if (Mathf.Approximately(this.duration, value))
			{
				return;
			}
			this.duration = value;
			if (this.tweenGeneric != null)
			{
				this.tweenGeneric.duration = value;
			}
		}
	}

	[XuiXmlAttribute("method", false)]
	public UITweener.Method Method
	{
		get
		{
			return this.method;
		}
		set
		{
			if (this.method == value)
			{
				return;
			}
			this.method = value;
			if (this.tweenGeneric != null)
			{
				this.tweenGeneric.method = value;
			}
		}
	}

	[XuiXmlAttribute("enabled", false)]
	public bool Enabled
	{
		get
		{
			return this.enabled;
		}
		set
		{
			if (this.enabled == value)
			{
				return;
			}
			this.enabled = value;
			if (this.tweenGeneric != null)
			{
				this.tweenGeneric.enabled = value;
			}
		}
	}

	[XuiXmlAttribute("curve", false)]
	public void AttributeCurve(string _curveString)
	{
		this.curve = XUiTweenAbs.parseCurve(_curveString);
		if (this.tweenGeneric != null)
		{
			this.tweenGeneric.animationCurve = this.curve;
		}
	}

	[XuiXmlAttribute("play_forward", false)]
	public void AttributePlayForward(bool _play)
	{
		if (!_play)
		{
			return;
		}
		this.Enabled = true;
		if (this.tweenGeneric != null)
		{
			this.tweenGeneric.PlayForward();
		}
	}

	[XuiXmlAttribute("play_reverse", false)]
	public void AttributePlayReverse(bool _play)
	{
		if (!_play)
		{
			return;
		}
		this.Enabled = true;
		if (this.tweenGeneric != null)
		{
			this.tweenGeneric.PlayReverse();
		}
	}

	[XuiXmlAttribute("set_to_start", false)]
	public void AttributeSetToStart(bool _set)
	{
		if (!_set)
		{
			return;
		}
		this.Enabled = false;
		if (this.tweenGeneric != null)
		{
			this.tweenGeneric.tweenFactor = 0f;
			this.tweenGeneric.Sample(0f, false);
		}
	}

	[XuiXmlAttribute("set_to_end", false)]
	public void AttributeSetToEnd(bool _set)
	{
		if (!_set)
		{
			return;
		}
		this.Enabled = false;
		if (this.tweenGeneric != null)
		{
			this.tweenGeneric.tweenFactor = 1f;
			this.tweenGeneric.Sample(1f, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiTweenAbs(XUiView _targetView)
	{
		this.targetView = _targetView;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void setCommonTweenValues(UITweener _tween)
	{
		this.tweenGeneric = _tween;
		_tween.style = this.repeatStyle;
		_tween.duration = this.duration;
		_tween.method = this.method;
		_tween.animationCurve = this.curve;
		_tween.enabled = this.enabled;
	}

	public abstract void CreateTween(GameObject _uiGameObject);

	public XUiController Controller
	{
		get
		{
			return this.targetView.Controller;
		}
	}

	public string GetXuiHierarchy()
	{
		return XUiUtils.GetXuiHierarchy(this.targetView.Controller);
	}

	public void ParseInitialAttributeValue(string _attribute, string _value)
	{
		if (_value.Contains("{"))
		{
			new BindingInfoNcalc(this, _attribute, _value);
			return;
		}
		if (ParsingMethodCache.Instance.TryParseDirect(this, _attribute, _value))
		{
			return;
		}
		Log.Error("[XUi] Unknown Tween attribute '" + _attribute + "', hierarchy: " + this.GetXuiHierarchy());
	}

	public static AnimationCurve SecondHalfLinear
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0f, 0f, 0f, 0f),
				new Keyframe(0.5f, 0f, 0f, 2f),
				new Keyframe(1f, 1f, 2f, 2f)
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static AnimationCurve parseCurve(string _curveString)
	{
		AnimationCurve animationCurve;
		if (!(_curveString == "EaseInOut"))
		{
			if (!(_curveString == "Linear"))
			{
				if (!(_curveString == "SecondHalfLinear"))
				{
					throw new ArgumentOutOfRangeException();
				}
				animationCurve = XUiTweenAbs.SecondHalfLinear;
			}
			else
			{
				animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
			}
		}
		else
		{
			animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		}
		return animationCurve;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView targetView;

	[PublicizedFrom(EAccessModifier.Private)]
	public UITweener tweenGeneric;

	[PublicizedFrom(EAccessModifier.Private)]
	public UITweener.Style repeatStyle;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float duration = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UITweener.Method method;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool enabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AnimationCurve curve = XUiTweenAbs.parseCurve("Linear");

	public enum ETweenType
	{
		Alpha,
		Color,
		Fill,
		Height,
		Position,
		Rotation,
		Scale,
		Width
	}
}
