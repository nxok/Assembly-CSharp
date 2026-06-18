using System;
using UnityEngine;

public class XUiTweenWidth : XUiTweenAbs
{
	[XuiXmlAttribute("start", false)]
	public int Start
	{
		get
		{
			return this.start;
		}
		set
		{
			this.start = value;
			if (this.tween != null)
			{
				this.tween.from = this.start;
			}
		}
	}

	[XuiXmlAttribute("end", false)]
	public int End
	{
		get
		{
			return this.end;
		}
		set
		{
			this.end = value;
			if (this.tween != null)
			{
				this.tween.to = this.end;
			}
		}
	}

	[XuiXmlAttribute("play_to", false)]
	public void AttributePlayToEnd(int _value)
	{
		if (this.tween == null)
		{
			return;
		}
		if (Mathf.Abs(this.tween.to - _value) < 1)
		{
			return;
		}
		base.Enabled = true;
		this.tween.tweenFactor = 0f;
		this.tween.SetStartToCurrentValue();
		this.End = _value;
		this.tween.PlayForward();
	}

	public XUiTweenWidth(XUiView _targetView)
		: base(_targetView)
	{
	}

	public override void CreateTween(GameObject _uiGameObject)
	{
		this.tween = NGUITools.AddMissingComponent<TweenWidth>(_uiGameObject);
		this.tween.from = this.start;
		this.tween.to = this.end;
		base.setCommonTweenValues(this.tween);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TweenWidth tween;

	[PublicizedFrom(EAccessModifier.Private)]
	public int start = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public int end = 40;
}
