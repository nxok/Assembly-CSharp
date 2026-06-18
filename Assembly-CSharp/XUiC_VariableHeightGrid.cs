using System;
using System.Collections.Generic;
using System.Linq;

public class XUiC_VariableHeightGrid : XUiC_ItemStackGrid
{
	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	public void SetHeight(int height)
	{
		this.gridHeight = height;
		this.heightChanged = true;
	}

	public void InitVariableHeightGrids(XUiV_Grid[] _grids)
	{
		this.grids = _grids;
	}

	public override void OnOpen()
	{
		base.OnOpen();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.heightChanged)
		{
			this.heightChanged = false;
			foreach (XUiV_Grid xuiV_Grid in this.grids)
			{
				this.updateGridHeight(xuiV_Grid, this.gridHeight);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateGridHeight(XUiV_Grid grid, int height)
	{
		grid.Rows = height;
		List<XUiController> children = grid.Controller.Children;
		for (int i = 0; i < children.Count; i++)
		{
			int num = i / 3;
			children[i].ViewComponent.IsVisible = num < height;
		}
	}

	public override XUiC_ItemStack[] GetItemStackControllers()
	{
		int num = 3 * this.gridHeight;
		return this.itemControllers.Take(num).ToArray<XUiC_ItemStack>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCollector tileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRowWidth = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid[] grids;

	[PublicizedFrom(EAccessModifier.Private)]
	public int gridHeight = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool heightChanged = true;
}
