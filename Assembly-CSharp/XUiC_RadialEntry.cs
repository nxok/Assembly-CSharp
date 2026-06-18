using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RadialEntry : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("icon");
		XUiController childById2 = base.GetChildById("background");
		XUiController childById3 = base.GetChildById("text");
		this.icon = childById.ViewComponent as XUiV_Sprite;
		this.background = childById2.ViewComponent as XUiV_Sprite;
		if (childById3 != null)
		{
			this.text = childById3.ViewComponent as XUiV_Label;
		}
		if (this.background != null)
		{
			this.backgroundColor = this.background.Color;
		}
	}

	public void SetHighlighted(bool _highlighted)
	{
		this.background.Color = (_highlighted ? this.highlightColor : this.backgroundColor);
	}

	public void SetSprite(string _sprite, Color _color)
	{
		this.icon.SpriteName = _sprite;
		this.icon.Color = _color;
	}

	public void SetText(string _text)
	{
		if (this.text != null)
		{
			this.text.Text = _text;
			this.text.IsVisible = _text != "";
		}
	}

	public void SetAtlas(string _atlas)
	{
		this.icon.UIAtlas = _atlas;
	}

	public void ResetScale()
	{
		this.SetScale(1f, true);
	}

	public void SetScale(float _scale, bool _instant = false)
	{
		float num = (_instant ? 0f : 0.15f);
		TweenScale.Begin(this.viewComponent.UiTransform.gameObject, num, Vector3.one * _scale);
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		bool flag = base.ParseAttribute(_name, _value);
		if (!flag && _name.EqualsCaseInsensitive("highlight_color"))
		{
			this.highlightColor = StringParsers.ParseColor32(_value);
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite icon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label text;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color backgroundColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color highlightColor;

	public string SelectionText;

	public int MenuItemIndex;

	public int CommandIndex;
}
