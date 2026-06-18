using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ShapeStackGrid : XUiController
{
	public int Length
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			this.page = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_ShapeStack>(null);
		this.shapeControllers = childrenByType;
		this.Length = this.shapeControllers.Length;
		Log.Out("ShapeControllers: " + this.shapeControllers.Length.ToString());
		this.bAwakeCalled = true;
		this.IsDirty = false;
	}

	public void SetShapes(List<XUiC_ShapeStackGrid.ShapeData> shapeIndexList, int newSelectedBlock = -1)
	{
		if (GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled))
		{
			GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
		}
		XUiC_ShapeInfoWindow childByType = this.xui.GetChildByType<XUiC_ShapeInfoWindow>();
		XUiC_ShapeMaterialInfoWindow childByType2 = this.xui.GetChildByType<XUiC_ShapeMaterialInfoWindow>();
		int count = shapeIndexList.Count;
		this.currentList = shapeIndexList;
		for (int i = 0; i < this.Length; i++)
		{
			int num = i + this.Length * this.page;
			XUiC_ShapeStack xuiC_ShapeStack = (XUiC_ShapeStack)this.shapeControllers[i];
			xuiC_ShapeStack.Owner = this;
			xuiC_ShapeStack.InfoWindow = childByType;
			xuiC_ShapeStack.MaterialInfoWindow = childByType2;
			if (num < count)
			{
				xuiC_ShapeStack.BlockData = shapeIndexList[num].Block;
				xuiC_ShapeStack.ShapeIndex = shapeIndexList[num].Index;
				if (xuiC_ShapeStack.BlockData == this.selectedBlock)
				{
					xuiC_ShapeStack.IsSelected = true;
				}
				if (xuiC_ShapeStack.IsSelected && xuiC_ShapeStack.BlockData != this.selectedBlock)
				{
					xuiC_ShapeStack.IsSelected = false;
				}
			}
			else
			{
				xuiC_ShapeStack.BlockData = null;
				xuiC_ShapeStack.ShapeIndex = -1;
				if (xuiC_ShapeStack.IsSelected)
				{
					xuiC_ShapeStack.IsSelected = false;
				}
			}
		}
		if (this.selectedBlock == null && newSelectedBlock != -1)
		{
			for (int j = 0; j < this.shapeControllers.Length; j++)
			{
				XUiC_ShapeStack xuiC_ShapeStack2 = this.shapeControllers[j] as XUiC_ShapeStack;
				if (xuiC_ShapeStack2.BlockData != null && xuiC_ShapeStack2.ShapeIndex == newSelectedBlock)
				{
					xuiC_ShapeStack2.SetSelectedShapeForItem();
					xuiC_ShapeStack2.IsSelected = true;
					return;
				}
			}
		}
		this.IsDirty = false;
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.SetShapes(this.currentList, this.Owner.ItemValue.Meta);
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int curPageIdx;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int numPages;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController[] shapeControllers;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAwakeCalled;

	public XUiC_ShapesWindow Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public Block selectedBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_ShapeStackGrid.ShapeData> currentList;

	public class ShapeData
	{
		public Block Block;

		public int Index;
	}
}
