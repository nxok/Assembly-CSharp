using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MaterialStackGrid : XUiController
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
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_MaterialStack>(null);
		this.materialControllers = childrenByType;
		this.Length = this.materialControllers.Length;
		this.bAwakeCalled = true;
	}

	public void SetMaterials(List<BlockTextureData> materialIndexList, int newSelectedMaterial = -1)
	{
		bool isCreative = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) && GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
		materialIndexList = (from m in materialIndexList
			orderby m.GetLocked(this.xui.playerUI.entityPlayer), (m.ID != 0) & isCreative, m.Group, m.SortIndex, m.LocalizedName
			select m).ToList<BlockTextureData>();
		XUiC_MaterialInfoWindow childByType = this.xui.GetChildByType<XUiC_MaterialInfoWindow>();
		int count = materialIndexList.Count;
		this.currentList = materialIndexList;
		if (newSelectedMaterial != -1)
		{
			this.selectedMaterial = BlockTextureData.list[newSelectedMaterial];
			if (this.selectedMaterial.Hidden && !isCreative)
			{
				this.selectedMaterial = null;
			}
		}
		for (int i = 0; i < this.Length; i++)
		{
			int num = i + this.Length * this.page;
			XUiC_MaterialStack xuiC_MaterialStack = (XUiC_MaterialStack)this.materialControllers[i];
			xuiC_MaterialStack.InfoWindow = childByType;
			if (num < count)
			{
				xuiC_MaterialStack.TextureData = materialIndexList[num];
				if (xuiC_MaterialStack.TextureData == this.selectedMaterial)
				{
					xuiC_MaterialStack.IsSelected = true;
				}
				if (xuiC_MaterialStack.IsSelected && xuiC_MaterialStack.TextureData != this.selectedMaterial)
				{
					xuiC_MaterialStack.IsSelected = false;
				}
			}
			else
			{
				xuiC_MaterialStack.TextureData = null;
				if (xuiC_MaterialStack.IsSelected)
				{
					xuiC_MaterialStack.IsSelected = false;
				}
			}
		}
		if (this.selectedMaterial == null && newSelectedMaterial != -1)
		{
			for (int j = 0; j < this.materialControllers.Length; j++)
			{
				XUiC_MaterialStack xuiC_MaterialStack2 = this.materialControllers[j] as XUiC_MaterialStack;
				if (xuiC_MaterialStack2.TextureData != null && !xuiC_MaterialStack2.IsLocked)
				{
					xuiC_MaterialStack2.SetSelectedTextureForItem();
					xuiC_MaterialStack2.IsSelected = true;
					return;
				}
			}
		}
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
			int num;
			if (this.xui.playerUI.entityPlayer.inventory.holdingItem is ItemClassBlock)
			{
				num = (int)(this.xui.playerUI.entityPlayer.inventory.holdingItemItemValue.TextureFullArray[0] & 255L);
			}
			else
			{
				num = ((ItemActionTextureBlock.ItemActionTextureBlockData)this.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1]).idx;
			}
			this.SetMaterials(this.currentList, num);
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
	public XUiController[] materialControllers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int[] materialIndices;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAwakeCalled;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockTextureData selectedMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockTextureData> currentList;
}
