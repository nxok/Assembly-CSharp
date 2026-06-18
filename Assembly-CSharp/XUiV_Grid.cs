using System;
using System.Collections.Generic;
using UnityEngine;

public class XUiV_Grid : XUiView_WidgetBased
{
	[XuiXmlAttribute("arrangement", false)]
	public UIGrid.Arrangement Arrangement { get; set; }

	public event UIGrid.OnSizeChanged OnSizeChanged;

	[XuiXmlAttribute("cols", false)]
	public int Columns
	{
		get
		{
			return this.columns;
		}
		set
		{
			if (this.columns == value)
			{
				return;
			}
			this.columns = value;
			base.SetDirty();
		}
	}

	[XuiXmlAttribute("rows", false)]
	public int Rows
	{
		get
		{
			return this.rows;
		}
		set
		{
			if (this.rows == value)
			{
				return;
			}
			this.rows = value;
			base.SetDirty();
		}
	}

	public override int RepeatCount
	{
		get
		{
			return this.Columns * this.Rows;
		}
		set
		{
		}
	}

	[XuiXmlAttribute("cell_width", false)]
	public int CellWidth { get; set; }

	[XuiXmlAttribute("cell_height", false)]
	public int CellHeight { get; set; }

	[XuiXmlAttribute("hide_inactive", false)]
	public bool HideInactive { get; set; }

	public override Vector2i InnerSize
	{
		get
		{
			return new Vector2i(this.CellWidth, this.CellHeight);
		}
	}

	public XUiV_Grid(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		_go.AddComponent<UIWidget>();
		_go.AddComponent<UIGrid>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		this.widget = this.uiTransform.gameObject.GetComponent<UIWidget>();
		this.grid = this.uiTransform.gameObject.GetComponent<UIGrid>();
	}

	public override void InitView()
	{
		base.InitView();
		this.widget.autoResizeBoxCollider = true;
		this.grid.hideInactive = this.HideInactive;
		this.grid.arrangement = this.Arrangement;
		this.grid.pivot = this.pivot;
		this.grid.onSizeChanged = new UIGrid.OnSizeChanged(this.OnGridSizeChanged);
		this.grid.maxPerLine = ((this.Arrangement == null) ? this.Columns : this.Rows);
		this.grid.cellWidth = (float)this.CellWidth;
		this.grid.cellHeight = (float)this.CellHeight;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGridSizeChanged(Vector2Int _cells, Vector2 _size)
	{
		this.widget.width = Mathf.RoundToInt(_size.x);
		this.widget.height = Mathf.RoundToInt(_size.y);
		this.size = new Vector2i(this.widget.width, this.widget.height);
		UIGrid.OnSizeChanged onSizeChanged = this.OnSizeChanged;
		if (onSizeChanged == null)
		{
			return;
		}
		onSizeChanged.Invoke(_cells, _size);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void refreshBoxCollider()
	{
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.grid.repositionNow = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		this.grid.maxPerLine = ((this.Arrangement == null) ? this.Columns : this.Rows);
		this.grid.cellWidth = (float)this.CellWidth;
		this.grid.cellHeight = (float)this.CellHeight;
		base.updateData();
	}

	public override void SetDefaults(XUiController _parent)
	{
		base.SetDefaults(_parent);
		this.Columns = 0;
		this.Rows = 0;
		this.CellWidth = int.MinValue;
		this.CellHeight = int.MinValue;
		this.Arrangement = 0;
		this.HideInactive = true;
	}

	public override void SetPostParsingDefaults(XUiController _parent)
	{
		base.SetPostParsingDefaults(_parent);
		if (this.CellWidth == -2147483648)
		{
			this.CellWidth = base.Width;
		}
		if (this.CellHeight == -2147483648)
		{
			this.CellHeight = base.Height;
		}
	}

	public override void SetRepeatContentTemplateParams(Dictionary<string, object> _templateParams, int _curRepeatNum)
	{
		base.SetRepeatContentTemplateParams(_templateParams, _curRepeatNum);
		int num;
		int num2;
		if (this.Arrangement == null)
		{
			num = _curRepeatNum % this.Columns;
			num2 = _curRepeatNum / this.Columns;
		}
		else
		{
			num = _curRepeatNum / this.Rows;
			num2 = _curRepeatNum % this.Rows;
		}
		_templateParams["repeat_col"] = num;
		_templateParams["repeat_row"] = num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int columns;

	[PublicizedFrom(EAccessModifier.Private)]
	public int rows;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIGrid grid;
}
