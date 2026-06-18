using System;
using UnityEngine;

public class XUiV_TextList : XUiV_LabelBase
{
	public float ScrollValue
	{
		get
		{
			return this.textList.scrollValue;
		}
		set
		{
			this.textList.scrollValue = value;
		}
	}

	[XuiXmlAttribute("max_paragraphs", false)]
	public int ParagraphHistory
	{
		get
		{
			return this.paragraphHistory;
		}
		set
		{
			if (value == this.paragraphHistory)
			{
				return;
			}
			this.paragraphHistory = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("list_style", false)]
	public UITextList.Style ListStyle
	{
		get
		{
			return this.listStyle;
		}
		set
		{
			if (value == this.listStyle)
			{
				return;
			}
			this.listStyle = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("prefix_first_line", false)]
	public string FirstLinePrefix
	{
		get
		{
			return this.firstLinePrefix;
		}
		set
		{
			if (this.firstLinePrefix == value)
			{
				return;
			}
			this.firstLinePrefix = value;
			base.SetDirty();
		}
	}

	public XUiV_TextList(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		base.createComponents(_go);
		_go.AddComponent<UITextList>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		this.textList = this.uiTransform.GetComponent<UITextList>();
	}

	public override void InitView()
	{
		base.EventOnDrag = true;
		base.EventOnScroll = true;
		base.InitView();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		this.label.width = this.size.x;
		this.label.height = this.size.y;
		this.textList.paragraphHistory = this.paragraphHistory;
		this.textList.style = this.listStyle;
		base.updateData();
	}

	public override void Cleanup()
	{
		this.textList.Clear();
		base.Cleanup();
	}

	public void AddLine(string _line)
	{
		if (!string.IsNullOrEmpty(this.firstLinePrefix))
		{
			_line = this.firstLinePrefix + _line;
		}
		_line = this.getFormattedText(_line);
		this.textList.Add(_line);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public UITextList textList;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UITextList.Style listStyle;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string firstLinePrefix;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int paragraphHistory = 50;
}
