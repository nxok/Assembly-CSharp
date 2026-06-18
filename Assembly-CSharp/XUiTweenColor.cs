using System;
using UnityEngine;

public class XUiTweenColor : XUiTweenAbs
{
	[XuiXmlAttribute("start", false)]
	public Color Start
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
	public Color End
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
	public void AttributePlayToEnd(Color _value)
	{
		if (this.tween == null)
		{
			return;
		}
		Color to = this.tween.to;
		if ((double)(Mathf.Abs(to.r - _value.r) + Mathf.Abs(to.g - _value.g) + Mathf.Abs(to.b - _value.b) + Mathf.Abs(to.a - _value.a)) < 0.002)
		{
			return;
		}
		base.Enabled = true;
		this.tween.tweenFactor = 0f;
		this.tween.SetStartToCurrentValue();
		this.End = _value;
		this.tween.PlayForward();
	}

	public XUiTweenColor(XUiView _targetView)
		: base(_targetView)
	{
	}

	public override void CreateTween(GameObject _uiGameObject)
	{
		this.tween = NGUITools.AddMissingComponent<TweenColor>(_uiGameObject);
		this.tween.from = this.start;
		this.tween.to = this.end;
		base.setCommonTweenValues(this.tween);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TweenColor tween;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color start = Color.black;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color end = Color.white;
}
