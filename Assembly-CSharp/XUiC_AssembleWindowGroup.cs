using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_AssembleWindowGroup : XUiController
{
	public ItemStack ItemStack
	{
		get
		{
			return this.itemStack;
		}
		set
		{
			this.itemStack = value;
			this.assembleWindow.ItemStack = value;
			this.grid.CurrentItem = value;
			this.grid.SetParts(this.itemStack.itemValue.Modifications);
			this.grid.AssembleWindow = this.assembleWindow;
			this.cosmeticGrid.CurrentItem = value;
			this.cosmeticGrid.SetParts(this.itemStack.itemValue.CosmeticMods);
			this.cosmeticGrid.AssembleWindow = this.assembleWindow;
		}
	}

	public override void Init()
	{
		base.Init();
		this.assembleWindow = base.GetChildByType<XUiC_AssembleWindow>();
		this.grid = base.GetChildByType<XUiC_ItemPartStackGrid>();
		this.cosmeticGrid = base.GetChildByType<XUiC_ItemCosmeticStackGrid>();
		this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
	}

	public override void OnOpen()
	{
		this.ItemStack = this.xui.AssembleItem.CurrentItem;
		base.OnOpen();
		GUIWindowManager windowManager = this.xui.playerUI.windowManager;
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader(Localization.Get("lblContextActionModify", false, null));
		}
		windowManager.Close("windowpaging");
		XUiC_ItemInfoWindow childByType = this.xui.GetChildByType<XUiC_ItemInfoWindow>();
		if (this.xui.AssembleItem.CurrentItemStackController != null)
		{
			this.xui.AssembleItem.CurrentItemStackController.IsSelected = true;
			childByType.SetItemStack(this.xui.AssembleItem.CurrentItemStackController, true);
			this.openEquipmentOnClose = false;
			return;
		}
		if (this.xui.AssembleItem.CurrentEquipmentStackController != null)
		{
			this.xui.AssembleItem.CurrentEquipmentStackController.IsSelected = false;
			childByType.SetItemStack(this.xui.AssembleItem.CurrentEquipmentStackController, true);
			this.openEquipmentOnClose = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_AssembleItem assembleItem = this.xui.AssembleItem;
		assembleItem.CurrentItem = null;
		assembleItem.CurrentItemStackController = null;
		assembleItem.CurrentEquipmentStackController = null;
		GameManager.Instance.StartCoroutine(this.showCraftingLater());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator showCraftingLater()
	{
		yield return new WaitForEndOfFrame();
		if (this.xui != null && this.xui.playerUI != null && this.xui.playerUI.entityPlayer != null)
		{
			XUiC_WindowSelector.OpenSelectorAndWindow(this.xui.playerUI.entityPlayer, this.openEquipmentOnClose ? "character" : "crafting");
		}
		yield break;
	}

	public static XUiC_AssembleWindowGroup GetWindowGroup(XUi _xuiInstance)
	{
		return _xuiInstance.FindWindowGroupByName(XUiC_AssembleWindowGroup.ID) as XUiC_AssembleWindowGroup;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_AssembleWindow assembleWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemPartStackGrid grid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemCosmeticStackGrid cosmeticGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	public static string ID = "assemble";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openEquipmentOnClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack;
}
