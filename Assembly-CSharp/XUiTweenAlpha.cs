using System;
using UnityEngine;

public class XUiTweenAlpha : XUiTweenAbs
{
	[XuiXmlAttribute("start", false)]
	public float Start
	{
		get
		{
			return this.start;
		}
		set
		{
			this.start = Mathf.Clamp01(value);
			if (this.tween != null)
			{
				this.tween.from = this.start;
			}
		}
	}

	[XuiXmlAttribute("end", false)]
	public float End
	{
		get
		{
			return this.end;
		}
		set
		{
			this.end = Mathf.Clamp01(value);
			if (this.tween != null)
			{
				this.tween.to = this.end;
			}
		}
	}

	[XuiXmlAttribute("play_to", false)]
	public void AttributePlayToEnd(float _value)
	{
		if (this.tween == null)
		{
			return;
		}
		if ((double)Mathf.Abs(this.tween.to - _value) < 0.0001)
		{
			return;
		}
		base.Enabled = true;
		this.tween.tweenFactor = 0f;
		this.tween.SetStartToCurrentValue();
		this.End = _value;
		this.tween.PlayForward();
	}

	public XUiTweenAlpha(XUiView _targetView)
		: base(_targetView)
	{
	}

	public override void CreateTween(GameObject _uiGameObject)
	{
		this.tween = NGUITools.AddMissingComponent<TweenAlpha>(_uiGameObject);
		this.tween.from = this.start;
		this.tween.to = this.end;
		base.setCommonTweenValues(this.tween);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TweenAlpha tween;

	[PublicizedFrom(EAccessModifier.Private)]
	public float start;

	[PublicizedFrom(EAccessModifier.Private)]
	public float end = 1f;
}
