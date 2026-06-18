using System;
using UnityEngine;

public class XUiV_Table : XUiView
{
	public override UIRect UiRect
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return null;
		}
	}

	[XuiXmlAttribute("sorting", false)]
	public UITable.Sorting Sorting { get; set; }

	[XuiXmlAttribute("cols", false)]
	public int Columns { get; set; } = 1;

	[XuiXmlAttribute("padding", true)]
	public Vector2 Padding { get; set; }

	[XuiXmlAttribute("hide_inactive", false)]
	public bool HideInactive { get; set; } = true;

	[XuiXmlAttribute("always_reposition", false)]
	public bool AlwaysReposition { get; set; }

	[XuiXmlAttribute("reposition_twice", false)]
	public bool RepositionTwice { get; set; }

	[XuiXmlAttribute("pivot", false)]
	public UIWidget.Pivot Pivot
	{
		get
		{
			return this.pivot;
		}
		set
		{
			if (this.pivot == value)
			{
				return;
			}
			this.pivot = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("cell_alignment", false)]
	public UIWidget.Pivot CellAlignment
	{
		get
		{
			return this.cellAlignment;
		}
		set
		{
			if (this.cellAlignment == value)
			{
				return;
			}
			this.cellAlignment = value;
			base.SetDirty();
		}
	}

	public override Vector3[] WorldCorners
	{
		get
		{
			return XUiV_Empty.WorldCornersEmpty;
		}
	}

	public XUiV_Table(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UITable>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		this.table = this.uiTransform.gameObject.GetComponent<UITable>();
	}

	public override void InitView()
	{
		base.InitView();
		this.table.hideInactive = this.HideInactive;
		this.table.sorting = this.Sorting;
		this.table.direction = 0;
		this.table.columns = this.Columns;
		this.table.padding = this.Padding;
		this.table.pivot = this.pivot;
		this.table.cellAlignment = this.cellAlignment;
		this.table.Reposition();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.table.repositionNow = true;
		if (this.RepositionTwice)
		{
			this.repositionNextFrame = true;
		}
	}

	public override void Update(float _dt)
	{
		if (this.firstUpdate || this.AlwaysReposition)
		{
			this.firstUpdate = false;
			this.table.Reposition();
		}
		if (this.repositionNextFrame && !this.table.enabled)
		{
			this.table.repositionNow = true;
			this.repositionNextFrame = false;
		}
		base.Update(_dt);
	}

	public void Reposition()
	{
		this.repositionNextFrame = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public UITable table;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget.Pivot pivot;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget.Pivot cellAlignment;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstUpdate = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool repositionNextFrame;
}
