using System;
using UnityEngine;

public class XUiTweenPosition : XUiTweenAbs
{
	[XuiXmlAttribute("start", false)]
	public Vector3 Start
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
	public Vector3 End
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
	public void AttributePlayToEnd(Vector3 _value)
	{
		if (this.tween == null)
		{
			return;
		}
		if ((double)(this.tween.to - _value).sqrMagnitude < 0.1)
		{
			return;
		}
		base.Enabled = true;
		this.tween.tweenFactor = 0f;
		this.tween.SetStartToCurrentValue();
		this.End = _value;
		this.tween.PlayForward();
	}

	public XUiTweenPosition(XUiView _targetView)
		: base(_targetView)
	{
	}

	public override void CreateTween(GameObject _uiGameObject)
	{
		this.tween = NGUITools.AddMissingComponent<TweenPosition>(_uiGameObject);
		this.tween.from = this.start;
		this.tween.to = this.end;
		base.setCommonTweenValues(this.tween);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TweenPosition tween;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 start = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 end = new Vector3(0f, 8f, 0f);
}
